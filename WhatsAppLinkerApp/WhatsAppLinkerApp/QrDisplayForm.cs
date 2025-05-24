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
        private ClientWebSocket _sharedWebSocketClient; 
        private CancellationTokenSource _sharedCancellationTokenSource; 

        private string _apiUsername;
        private string _apiPassword;
        private string _ownerNumber; // NEW: Store owner number

        public event Action<string, string, string> ClientLinked;

        // Modified constructor to accept ownerNumber
        public QrDisplayForm(ClientWebSocket sharedWsClient, CancellationTokenSource sharedCts, string apiUsername, string apiPassword, string ownerNumber)
        {
            InitializeComponent();
            _sharedWebSocketClient = sharedWsClient;
            _sharedCancellationTokenSource = sharedCts;
            _apiUsername = apiUsername;
            _apiPassword = apiPassword;
            _ownerNumber = ownerNumber; // NEW: Assign owner number
            this.Load += QrDisplayForm_Load;
            this.FormClosing += QrDisplayForm_FormClosing;
        }

        private async void QrDisplayForm_Load(object sender, EventArgs e)
        {
            if (_sharedWebSocketClient != null && _sharedWebSocketClient.State == WebSocketState.Open)
            {
                UpdateStatus("Connected to manager. Requesting QR code...");
                ClearQrDisplay();
                await RequestQrFromManager(_apiUsername, _apiPassword, _ownerNumber); // NEW: Pass ownerNumber
            }
            else
            {
                UpdateStatus("Not connected to manager. Please ensure the main application is connected.");
                Console.WriteLine("QrDisplayForm_Load: Shared WebSocket client is not open.");
            }
        }

        private void QrDisplayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // No need to close/dispose the shared WebSocket here; Form1 owns it.
        }

        // Modified to accept ownerNumber
        private async Task RequestQrFromManager(string apiUsername, string apiPassword, string ownerNumber) // NEW: ownerNumber param
        {
            if (_sharedWebSocketClient != null && _sharedWebSocketClient.State == WebSocketState.Open)
            {
                var request = new { 
                    type = "requestQr",
                    apiUsername = apiUsername, 
                    apiPassword = apiPassword,
                    ownerNumber = ownerNumber // NEW: Include ownerNumber in the request
                };
                var buffer = Encoding.UTF8.GetBytes(JObject.FromObject(request).ToString());
                if (_sharedCancellationTokenSource != null && !_sharedCancellationTokenSource.IsCancellationRequested)
                {
                    await _sharedWebSocketClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _sharedCancellationTokenSource.Token);
                    UpdateStatus("Requested new QR code from bot manager...");
                }
                else
                {
                    Console.WriteLine("Shared CancellationTokenSource is cancelled or null, cannot send requestQr.");
                }
            }
            else
            {
                UpdateStatus("Shared WebSocket not connected, cannot request QR.");
                Console.WriteLine("RequestQrFromManager: Shared WebSocket not open or null.");
            }
        }

        // Modified ManualRelinkFromManager to include ownerNumber
        private async Task ManualRelinkFromManager()
        {
            if (_sharedWebSocketClient != null && _sharedWebSocketClient.State == WebSocketState.Open)
            {
                var request = new { 
                    type = "manualRelink",
                    apiUsername = _apiUsername,
                    apiPassword = _apiPassword,
                    ownerNumber = _ownerNumber // NEW: Include ownerNumber
                };
                var buffer = Encoding.UTF8.GetBytes(JObject.FromObject(request).ToString());
                if (_sharedCancellationTokenSource != null && !_sharedCancellationTokenSource.IsCancellationRequested)
                    await _sharedWebSocketClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _sharedCancellationTokenSource.Token);
                else
                    Console.WriteLine("Shared CancellationTokenSource is cancelled or null, cannot send manual re-link request.");
            }
            else
            {
                UpdateStatus("Shared WebSocket not connected, cannot request re-link.");
                Console.WriteLine("ManualRelinkFromManager: Shared WebSocket not open or null.");
            }
        }

        public void ProcessManagerMessageForQRDisplay(string messageJson)
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
                    Console.WriteLine("Error parsing WebSocket message on UI thread (QrDisplayForm): " + ex.Message);
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