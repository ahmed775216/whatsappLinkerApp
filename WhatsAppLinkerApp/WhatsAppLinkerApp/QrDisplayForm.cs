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
        // --- MODIFIED: Made nullable ---
        private ClientWebSocket? _sharedWebSocketClient;
        private CancellationTokenSource? _sharedCancellationTokenSource;
        // --- END MODIFIED ---

        private string _apiUsername; // Will be initialized in constructor
        private string _apiPassword; // Will be initialized in constructor
        private string _ownerNumber; // Will be initialized in constructor

        // --- MODIFIED: Made nullable ---
        public event Action<string, string, string>? ClientLinked;
        // --- END MODIFIED ---

        public QrDisplayForm(ClientWebSocket sharedWsClient, CancellationTokenSource sharedCts, string apiUsername, string apiPassword, string ownerNumber)
        {
            InitializeComponent();
            _sharedWebSocketClient = sharedWsClient;
            _sharedCancellationTokenSource = sharedCts;
            _apiUsername = apiUsername;
            _apiPassword = apiPassword;
            _ownerNumber = ownerNumber;
            this.Load += QrDisplayForm_Load;
            this.FormClosing += QrDisplayForm_FormClosing;
        }

        private async void QrDisplayForm_Load(object? sender, EventArgs e) // Made nullable
        {
            if (_sharedWebSocketClient != null && _sharedWebSocketClient.State == WebSocketState.Open)
            {
                UpdateStatus("Connecting to manager. Requesting QR code...", Color.Goldenrod);
                ClearQrDisplay();
                await RequestQrFromManager(_apiUsername, _apiPassword, _ownerNumber);
            }
            else
            {
                UpdateStatus("Not connected to manager. Please ensure the main application is connected.", Color.Red);
                Console.WriteLine("QrDisplayForm_Load: Shared WebSocket client is not open.");
                MessageBox.Show("Failed to connect to manager for QR. Ensure Node.js manager is running.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }

        private void QrDisplayForm_FormClosing(object? sender, FormClosingEventArgs e) // Made nullable
        {
            if (this.DialogResult != DialogResult.OK)
            {
                ClearQrDisplay();
            }
        }

        private async Task RequestQrFromManager(string apiUsername, string apiPassword, string ownerNumber)
        {
            if (_sharedWebSocketClient != null && _sharedWebSocketClient.State == WebSocketState.Open)
            {
                var request = new
                {
                    type = "requestQr",
                    apiUsername = apiUsername,
                    apiPassword = apiPassword,
                    ownerNumber = ownerNumber
                };
                var buffer = Encoding.UTF8.GetBytes(JObject.FromObject(request).ToString());
                // --- MODIFIED: Use null-forgiving operator `!` ---
                if (_sharedCancellationTokenSource != null && !_sharedCancellationTokenSource.IsCancellationRequested)
                {
                    await _sharedWebSocketClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _sharedCancellationTokenSource!.Token);
                    UpdateStatus("Requested new QR code from bot manager...", Color.Goldenrod);
                }
                else
                {
                    Console.WriteLine("Shared CancellationTokenSource is cancelled or null, cannot send requestQr.");
                    UpdateStatus("Request aborted.", Color.Red);
                }
                // --- END MODIFIED ---
            }
            else
            {
                UpdateStatus("Shared WebSocket not connected, cannot request QR.", Color.Red);
                Console.WriteLine("RequestQrFromManager: Shared WebSocket not open or null.");
            }
        }

        private async Task ManualRelinkFromManager()
        {
            if (_sharedWebSocketClient != null && _sharedWebSocketClient.State == WebSocketState.Open)
            {
                var request = new
                {
                    type = "manualRelink",
                    apiUsername = _apiUsername,
                    apiPassword = _apiPassword,
                    ownerNumber = _ownerNumber
                };
                var buffer = Encoding.UTF8.GetBytes(JObject.FromObject(request).ToString());
                // --- MODIFIED: Use null-forgiving operator `!` ---
                if (_sharedCancellationTokenSource != null && !_sharedCancellationTokenSource.IsCancellationRequested)
                    await _sharedWebSocketClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _sharedCancellationTokenSource!.Token);
                else
                    Console.WriteLine("Shared CancellationTokenSource is cancelled or null, cannot send manual re-link request.");
                // --- END MODIFIED ---
            }
            else
            {
                UpdateStatus("Shared WebSocket not connected, cannot request re-link.", Color.Red);
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
                    string? type = message["type"]?.ToString(); // Made nullable
                    string? clientId = message["clientId"]?.ToString(); // Made nullable
                    string? phoneNumber = message["phoneNumber"]?.ToString(); // Made nullable
                    string? clientName = message["name"]?.ToString(); // Made nullable

                    if (type == "qr")
                    {
                        string? qrData = message["qr"]?.ToString(); // Made nullable
                        if (!string.IsNullOrEmpty(qrData))
                        {
                            GenerateAndDisplayQr(qrData);
                            UpdateStatus("Scan the QR code with WhatsApp on your phone. " + (clientId != null ? $" (Client: {clientId})" : ""), SystemColors.ControlText);
                        }
                        else
                        {
                            UpdateStatus("Waiting for QR or connection...", Color.Goldenrod);
                            ClearQrDisplay();
                        }
                    }
                    else if (type == "status")
                    {
                        string? status = message["status"]?.ToString(); // Made nullable
                        string? qrFromStatus = message["qr"]?.ToString(); // Made nullable
                        string? errorMsg = message["message"]?.ToString(); // Made nullable

                        switch (status)
                        {
                            case "connected":
                                UpdateStatus($"WhatsApp Linked: {clientName ?? phoneNumber}!", Color.FromArgb(37, 211, 102));
                                ClearQrDisplay();
                                // --- MODIFIED: Check if ClientLinked is null before invoking ---
                                if (ClientLinked != null && clientId != null && phoneNumber != null)
                                {
                                    ClientLinked.Invoke(clientId, phoneNumber, clientName ?? ""); // Pass empty string if clientName is null
                                }
                                // --- END MODIFIED ---
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                                break;
                            case "qr":
                                if (!string.IsNullOrEmpty(qrFromStatus))
                                {
                                    GenerateAndDisplayQr(qrFromStatus);
                                    UpdateStatus("Scan the QR code with WhatsApp.", SystemColors.ControlText);
                                }
                                else { UpdateStatus("Waiting for QR code...", Color.Goldenrod); ClearQrDisplay(); }
                                break;
                            case "disconnected":
                                UpdateStatus("Disconnected. Ready to link.", Color.Gray);
                                ClearQrDisplay();
                                break;
                            case "disconnected_logout":
                            case "error":
                            case "linking_failed":
                                UpdateStatus($"Linking Error: {errorMsg ?? "An unexpected error occurred."} Please try again.", Color.Red);
                                ClearQrDisplay();
                                this.DialogResult = DialogResult.Abort;
                                break;
                            case "connecting":
                            case "reconnecting":
                            case "linking_in_progress":
                                UpdateStatus(errorMsg ?? "Connecting...", Color.Goldenrod);
                                if (!string.IsNullOrEmpty(qrFromStatus))
                                {
                                    GenerateAndDisplayQr(qrFromStatus);
                                }
                                else { ClearQrDisplay(); }
                                break;
                            default:
                                UpdateStatus($"Status: {status}", Color.Gray);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing WebSocket message on UI thread (QrDisplayForm): " + ex.Message);
                    UpdateStatus("Error processing update from bot.", Color.Red);
                }
            });
        }

  private void GenerateAndDisplayQr(string qrText)
        {
            try
            {
                if (string.IsNullOrEmpty(qrText))
                {
                    ClearQrDisplay();
                    return;
                }
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);

                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

                byte[] blackBytes = new byte[] { 0, 0, 0, 255 };
                byte[] whiteBytes = new byte[] { 255, 255, 255, 255 };

                byte[] qrCodeAsPngBytes = qrCode.GetGraphic(10, blackBytes, whiteBytes);

                using (var ms = new MemoryStream(qrCodeAsPngBytes))
                {
                    qrPictureBox.Image = new Bitmap(ms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating/displaying QR: {ex.Message}");
                UpdateStatus("Error displaying QR code.", Color.Red);
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

        private void UpdateStatus(string message, Color color)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = color;
        }
    }
}