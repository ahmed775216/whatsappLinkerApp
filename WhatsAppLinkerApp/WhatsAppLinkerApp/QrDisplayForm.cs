// QrDisplayForm.cs
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
        private const string NodeJsWebSocketUrl = "ws://localhost:8088";
        private bool _isConnecting = false;

        private string _apiUsername; // Store API username
        private string _apiPassword; // Store API password

        public event Action<string, string, string> ClientLinked;

        // Modified constructor to accept API credentials
        public QrDisplayForm(string apiUsername, string apiPassword)
        {
            InitializeComponent();
            _apiUsername = apiUsername;
            _apiPassword = apiPassword;
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
            _cancellationTokenSource?.Cancel();
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
                return;
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
                
                // Send API credentials along with the QR request
                await RequestQrFromManager(_apiUsername, _apiPassword);

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
                _isConnecting = false;
            }
        }

        // Modified to accept API credentials
        private async Task RequestQrFromManager(string apiUsername, string apiPassword)
        {
            if (_webSocketClient != null && _webSocketClient.State == WebSocketState.Open)
            {
                var request = new { 
                    type = "requestQr",
                    apiUsername = apiUsername, // Send API username
                    apiPassword = apiPassword  // Send API password
                };
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

        private async Task ManualRelinkFromManager()
        {
            if (_webSocketClient != null && _webSocketClient.State == WebSocketState.Open)
            {
                // For manual relink, also send current API credentials
                var request = new { 
                    type = "manualRelink",
                    apiUsername = _apiUsername,
                    apiPassword = _apiPassword
                };
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
            catch (OperationCanceledException) { }
            catch (WebSocketException ex) {
                UpdateStatus($"WebSocket error: {ex.Message}. Connection lost.");
                Console.WriteLine($"WebSocket Receive Error: {ex.Message}");
            }
            catch (Exception ex) {
                UpdateStatus($"An unexpected error occurred during connection: {ex.Message}");
                Console.WriteLine($"Message Processing Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("ReceiveMessagesAsync loop ended. WebSocket state: " + wsClient.State);
                if (wsClient.State != WebSocketState.Closed && wsClient.State != WebSocketState.Aborted)
                {
                   try { await wsClient.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client receiver loop ended", CancellationToken.None); } catch {}
                }
                if (!_cancellationTokenSource.IsCancellationRequested && (wsClient.State == WebSocketState.Closed || wsClient.State == WebSocketState.Aborted))
                {
                    Console.WriteLine("Attempting to reconnect WebSocket...");
                    _webSocketClient = new ClientWebSocket();
                    await ConnectToWebSocketAsync();
                } else if (_cancellationTokenSource.IsCancellationRequested) {
                    UpdateStatus("Window closing, not reconnecting.");
                } else {
                    UpdateStatus("Disconnected. Please click 'Link WhatsApp' to retry.");
                }
            }
        }

        private void ProcessWebSocketMessage(string messageJson)
        {
           this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    JObject message = JObject.Parse(messageJson);
                    string type = message["type"]?.ToString();
                    string clientId = message["clientId"]?.ToString();
                    string phoneNumber = message["phoneNumber"]?.ToString();
                    string clientName = message["name"]?.ToString();

                    if (type == "qr") {
                        string qrData = message["qr"]?.ToString();
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
                    else if (type == "status")
                    {
                        string status = message["status"]?.ToString();
                        string qrFromStatus = message["qr"]?.ToString();
                        string errorMsg = message["message"]?.ToString();

                        switch (status)
                        {
                            case "connected":
                                UpdateStatus($"WhatsApp Linked Successfully for {clientName ?? phoneNumber}!");
                                ClearQrDisplay();
                                if (ClientLinked != null && clientId != null && phoneNumber != null) {
                                    ClientLinked.Invoke(clientId, phoneNumber, clientName);
                                    // NO LONGER SEND API CREDENTIALS HERE, THEY ARE SENT UPFRONT
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
    
        private void GenerateAndDisplayQr(string qrText)
        {
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);

                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeAsPngBytes = qrCode.GetGraphic(20);

                using (var ms = new MemoryStream(qrCodeAsPngBytes))
                {
                    qrPictureBox.Image = new Bitmap(ms);
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
                qrPictureBox.Image.Dispose();
                qrPictureBox.Image = null;
            }
        }

        private void UpdateStatus(string message)
        {
            statusLabel.Text = message;
        }
    }
}