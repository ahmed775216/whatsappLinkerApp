using System;
using System.Drawing;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using QRCoder;
namespace WhatsAppLinkerApp
{
    public partial class QrDisplayForm : Form
    {
        private ClientWebSocket _webSocketClient;
        private CancellationTokenSource _cancellationTokenSource;
        private const string? NodeJsWebSocketUrl = "ws://localhost:8088";
        private bool _isConnecting = false; // Flag to prevent multiple connection attempts

        public event Action<string, string, string> ClientLinked;

        public QrDisplayForm()
        {
            InitializeComponent();
            this.Load += QrDisplayForm_Load;
            this.FormClosing += QrDisplayForm_FormClosing;
        }

        private async void QrDisplayForm_Load(object sender, EventArgs e)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _webSocketClient = new ClientWebSocket();
            await ConnectToWebSocketAsync();
        }

        private async void QrDisplayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancellationTokenSource?.Cancel(); // Signal cancellation
            if (_webSocketClient != null && _webSocketClient.State == WebSocketState.Open)
            {
                try { await _webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Form closed", CancellationToken.None); }
                catch (Exception ex) { Console.WriteLine($"Error closing WS: {ex.Message}"); }
            }
            _webSocketClient?.Dispose();
        }

        private async Task ConnectToWebSocketAsync()
        {
            if (_isConnecting || (_webSocketClient != null && _webSocketClient.State == WebSocketState.Open)) {
                Console.WriteLine("ConnectToWebSocketAsync: Already connecting or connected. Aborting.");
                return; // Prevent multiple connections
            }

            _isConnecting = true;
            try
            {
                UpdateStatus("Connecting to bot manager for QR code...");
                ClearQrDisplay();

                Console.WriteLine($"Attempting WS connection to {NodeJsWebSocketUrl}");
                await _webSocketClient.ConnectAsync(new Uri(NodeJsWebSocketUrl), _cancellationTokenSource.Token);
                
                UpdateStatus("Connected. Requesting QR code...");
                Console.WriteLine("WebSocket Connected! Sending requestQr.");
                
                // --- CRITICAL: Request QR from Manager ---
                await RequestQrFromManager();

                // Start receiving messages in a separate task
                _ = ReceiveMessagesAsync(_webSocketClient, _cancellationTokenSource.Token);
            }
            catch (WebSocketException ex)
            {
                UpdateStatus($"Error connecting: {ex.Message}. Ensure Node.js bot manager is running on {NodeJsWebSocketUrl}.");
                Console.WriteLine($"WebSocket Connection Error: {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Connection attempt cancelled.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"An unexpected error occurred during connection: {ex.Message}");
                Console.WriteLine($"General Connection Error: {ex.Message}");
            }
            finally
            {
                _isConnecting = false; // Reset flag
            }
        }

        private async Task RequestQrFromManager()
        {
            if (_webSocketClient != null && _webSocketClient.State == WebSocketState.Open)
            {
                var request = new { type = "requestQr" };
                var buffer = Encoding.UTF8.GetBytes(JObject.FromObject(request).ToString());
                if (_cancellationTokenSource != null)
                {
                    await _webSocketClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
                    UpdateStatus("Requested new QR code from bot manager...");
                }
                else
                {
                    UpdateStatus("Cancellation token is not initialized, cannot send request.");
                }
            }
            else
            {
                UpdateStatus("Not connected to bot manager, cannot request QR.");
            }
        }

        // This function will be added to the button event if you put a button in QrDisplayForm
        private async Task ManualRelinkFromManager()
        {
            if (_webSocketClient != null && _webSocketClient.State == WebSocketState.Open)
            {
                var request = new { type = "manualRelink" };
                var buffer = Encoding.UTF8.GetBytes(JObject.FromObject(request).ToString());
                if (_cancellationTokenSource != null)
                {
                    await _webSocketClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
                    UpdateStatus("Requested manual re-link from bot manager...");
                }
                else
                {
                    UpdateStatus("Cancellation token is not initialized, cannot send manual re-link request.");
                }
            }
            else
            {
                UpdateStatus("Not connected to bot manager, cannot request re-link.");
            }
        }



        private async Task ReceiveMessagesAsync(ClientWebSocket wsClient, CancellationToken token)
        {
            var buffer = new byte[4096 * 2];
            try
            {
                while (wsClient.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", token);
                        UpdateStatus("Disconnected by server.");
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine("Received from Node: " + messageJson);
                        ProcessWebSocketMessage(messageJson);
                    }
                }
            }
            catch (OperationCanceledException) { /* Task cancelled, graceful exit */ }
            catch (WebSocketException ex) {
                // This indicates connection was lost, update status
                UpdateStatus($"WebSocket error: {ex.Message}. Connection lost.");
                Console.WriteLine($"WebSocket Receive Error: {ex.Message}");
            }
            catch (Exception ex) {
                UpdateStatus($"Error processing message: {ex.Message}");
                Console.WriteLine($"Message Processing Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("ReceiveMessagesAsync loop ended. WebSocket state: " + wsClient.State);
                if (wsClient.State != WebSocketState.Closed && wsClient.State != WebSocketState.Aborted)
                {
                   try { await wsClient.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client receiver loop ended", CancellationToken.None); } catch {}
                }
                // Only try to reconnect if not explicitly closing the form
                if (!_cancellationTokenSource.IsCancellationRequested && (wsClient.State == WebSocketState.Closed || wsClient.State == WebSocketState.Aborted))
                {
                    Console.WriteLine("Attempting to reconnect WebSocket...");
                    _webSocketClient = new ClientWebSocket(); // Create new client instance
                    await ConnectToWebSocketAsync(); // Try to reconnect
                } else if (_cancellationTokenSource.IsCancellationRequested) {
                    UpdateStatus("Window closing, not reconnecting.");
                } else {
                    UpdateStatus("Disconnected. Please click 'Link WhatsApp' to retry.");
                }
            }
        }

        private void ProcessWebSocketMessage(string? messageJson)
        {
           this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    JObject message = JObject.Parse(messageJson);
                    string? type = message["type"]?.ToString();
                    string? clientId = message["clientId"]?.ToString();
                    string? phoneNumber = message["phoneNumber"]?.ToString();
                    string? clientName = message["name"]?.ToString();

                    if (type == "qr") {   if (type == "qr")
                    {
                        string? qrData = message["qr"]?.ToString();
                        if (!string.IsNullOrEmpty(qrData))
                        {
                            GenerateAndDisplayQr(qrData);
                            UpdateStatus("Scan the QR code with WhatsApp. " + (clientId != null ? $" (Client: {clientId})" : ""));
                        }
                        else
                        {
                            UpdateStatus("Waiting for QR or connection...");
                            ClearQrDisplay();
                        }
                    }
  }
                    else if (type == "status")
                    {
                        string? status = message["status"]?.ToString();
                        string? qrFromStatus = message["qr"]?.ToString();
                        string? errorMsg = message["message"]?.ToString(); // Node.js sends 'message' key

                        switch (status)
                        {
                            case "connected":
                                UpdateStatus($"WhatsApp Linked Successfully for {clientName ?? phoneNumber}!");
                                ClearQrDisplay();
                                if (ClientLinked != null && clientId != null && phoneNumber != null) {
                                    ClientLinked.Invoke(clientId, phoneNumber, clientName);
                                }
                                break;
                            case "qr":
                                if (!string.IsNullOrEmpty(qrFromStatus)) {
                                    GenerateAndDisplayQr(qrFromStatus);
                                    UpdateStatus("Scan the QR code with WhatsApp.");
                                } else { UpdateStatus("Waiting for QR code..."); ClearQrDisplay(); }
                                break;
                            case "disconnected": UpdateStatus("Disconnected. Ready to link."); ClearQrDisplay(); break;
                            case "disconnected_logout": UpdateStatus($"Logged out. {errorMsg ?? "Please click 'Link WhatsApp' again."}"); ClearQrDisplay(); break;
                            case "connecting": UpdateStatus("Connecting to WhatsApp..."); ClearQrDisplay(); break;
                            case "reconnecting": UpdateStatus("Reconnecting to WhatsApp..."); break;
                            case "error": UpdateStatus($"Error: {errorMsg ?? "An unknown error occurred."}"); ClearQrDisplay(); break;
                            case "linking_in_progress": UpdateStatus(errorMsg); if (!string.IsNullOrEmpty(qrFromStatus)) { GenerateAndDisplayQr(qrFromStatus); } else { ClearQrDisplay(); } break;
                            case "linking_failed": UpdateStatus($"Linking failed: {errorMsg}"); ClearQrDisplay(); break;
                            default: UpdateStatus($"Status: {status}"); break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing WebSocket message on UI thread: " + ex.Message);
                    UpdateStatus("Error processing update from bot.");
                }
            });
        }
    

        private void GenerateAndDisplayQr(string? qrText)
        {
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);

                // Use PngByteQRCode for direct byte array output, then convert to Bitmap
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeAsPngBytes = qrCode.GetGraphic(20); // 20 pixels per module (adjust size as needed)

                using (var ms = new MemoryStream(qrCodeAsPngBytes))
                {
                    qrPictureBox.Image = new Bitmap(ms); // Assign to PictureBox
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating/displaying QR: {ex.Message}");
                UpdateStatus("Error displaying QR code.");
                qrPictureBox.Image = null;
            }
        }

        private void ClearQrDisplay()
        {
            if (qrPictureBox.Image != null)
            {
                qrPictureBox.Image.Dispose(); // Release resources
                qrPictureBox.Image = null;
            }
        }

        private void UpdateStatus(string? message)
        {
            statusLabel.Text = message;
        }


    }
}