using System;
using System.Drawing;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCoder;

namespace WhatsAppLinkerApp
{

    public partial class QrDisplayForm : Form
    {
        private readonly ClientWebSocket _sharedWebSocketClient;
        private readonly CancellationTokenSource _sharedCancellationTokenSource;

        private readonly string _apiUsername;
        private readonly string _apiPassword;
        private readonly string _ownerNumber;

        public string CurrentClientId { get; private set; } = string.Empty;
        private bool _isClosing = false;
        private bool _qrRequested = false; // Add flag to track if QR was already requested

        public event Action<string, string, string, string, string, string>? ClientLinked;

        public QrDisplayForm(ClientWebSocket sharedWsClient, CancellationTokenSource sharedCts, string apiUsername, string apiPassword, string ownerNumber)
        {
            InitializeComponent();
            _sharedWebSocketClient = sharedWsClient ?? throw new ArgumentNullException(nameof(sharedWsClient));
            _sharedCancellationTokenSource = sharedCts ?? throw new ArgumentNullException(nameof(sharedCts));
            _apiUsername = apiUsername;
            _apiPassword = apiPassword;
            _ownerNumber = ownerNumber;
            this.Load += QrDisplayForm_Load;
            this.FormClosing += QrDisplayForm_FormClosing;
        }

        private async void QrDisplayForm_Load(object? sender, EventArgs e)
        {
            if (_qrRequested) return; // Prevent duplicate requests

            if (_sharedWebSocketClient.State == WebSocketState.Open)
            {
                UpdateStatus("Requesting QR code from manager...", Color.Goldenrod);
                ClearQrDisplay();
                _qrRequested = true; // Set flag before requesting
                await RequestQrFromManager(_apiUsername, _apiPassword, _ownerNumber);
            }
            else
            {
                UpdateStatus("Not connected to manager. Please ensure the main application is connected.", Color.Red);
                MessageBox.Show("Failed to connect to manager for QR. Ensure main application is connected.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isClosing = true;
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }

        private void QrDisplayForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _isClosing = true;
            if (this.DialogResult != DialogResult.OK)
            {
                ClearQrDisplay();
            }
        }

        public void UpdateQrCode(string qrText, string clientId, string phoneNumberForContext)
        {
            if (InvokeRequired) { Invoke((MethodInvoker)delegate { UpdateQrCode(qrText, clientId, phoneNumberForContext); }); return; }
            this.CurrentClientId = clientId;
            if (!string.IsNullOrEmpty(qrText))
            {
                GenerateAndDisplayQr(qrText);
                UpdateStatus($"Scan QR with WhatsApp. (Client: {clientId})", SystemColors.ControlText);
            }
            else
            {
                UpdateStatus("Waiting for QR or connection update...", Color.Goldenrod);
                ClearQrDisplay();
            }
        }

        private async Task RequestQrFromManager(string apiUsername, string apiPassword, string ownerNumber)
        {
            if (_sharedWebSocketClient.State == WebSocketState.Open && !_sharedCancellationTokenSource.IsCancellationRequested)
            {
                var request = new JObject
                {
                    ["type"] = "requestQr",
                    ["apiUsername"] = apiUsername,
                    ["apiPassword"] = apiPassword,
                    ["ownerNumber"] = ownerNumber
                };
                var buffer = Encoding.UTF8.GetBytes(request.ToString());
                try
                {
                    await _sharedWebSocketClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _sharedCancellationTokenSource.Token);
                    UpdateStatus("QR code request sent...", Color.Goldenrod);
                }
                catch (OperationCanceledException) { UpdateStatus("Request cancelled.", Color.Orange); }
                catch (Exception ex) { UpdateStatus($"Error sending QR request: {ex.Message}", Color.Red); }
            }
            else { UpdateStatus("Cannot request QR: Not connected or task cancelled.", Color.Red); }
        }

        public void ProcessRelayedManagerMessage(string messageJson)
        {
            if (_isClosing) return;

            if (InvokeRequired) { Invoke((MethodInvoker)delegate { ProcessRelayedManagerMessage(messageJson); }); return; }

            try
            {
                JObject message = JObject.Parse(messageJson);
                string? type = message["type"]?.ToString();
                string? msgClientId = message["clientId"]?.ToString();

                Console.WriteLine($"[QR_FORM] Processing message - Type: {type}, ClientId: {msgClientId}, CurrentClientId: {CurrentClientId}");

                // Ignore messages without a client ID when we already have one
                if (!string.IsNullOrEmpty(this.CurrentClientId) && string.IsNullOrEmpty(msgClientId))
                {
                    Console.WriteLine($"[QR_FORM] Ignoring message without clientId when we already have {this.CurrentClientId}");
                    return;
                }

                // Handle initial assignment of CurrentClientId
                if (string.IsNullOrEmpty(this.CurrentClientId) && !string.IsNullOrEmpty(msgClientId))
                {
                    if (type == "qr" || (type == "status" && message["status"]?.ToString() == "linking_in_progress"))
                    {
                        this.CurrentClientId = msgClientId;
                        Console.WriteLine($"[QR_FORM] Assigned CurrentClientId: {this.CurrentClientId}");
                    }
                }

                // Skip messages not for this client
                if (!string.IsNullOrEmpty(this.CurrentClientId) && this.CurrentClientId != msgClientId && !string.IsNullOrEmpty(msgClientId))
                {
                    Console.WriteLine($"[QR_FORM] Ignoring message for client {msgClientId}, form is for {this.CurrentClientId}");
                    return;
                }

                string? phoneNumber = message["phoneNumber"]?.ToString();
                string? clientName = message["name"]?.ToString() ?? phoneNumber?.Split('@')[0] ?? "Unknown";

                switch (type)
                {
                    case "qr":
                        string? qrData = message["qr"]?.ToString();
                        UpdateQrCode(qrData ?? "", msgClientId ?? "", phoneNumber ?? "");
                        UpdateStatus($"Scan QR with WhatsApp. (Client: {msgClientId ?? "N/A"})", SystemColors.ControlText);
                        break;

                    case "status":
                        string? status = message["status"]?.ToString();
                        string? statusMsg = message["message"]?.ToString();
                        string? qrFromStatus = message["qr"]?.ToString();

                        Console.WriteLine($"[QR_FORM] Status update - Status: {status}, Message: {statusMsg}");

                        if (!string.IsNullOrEmpty(qrFromStatus))
                        {
                            UpdateQrCode(qrFromStatus, msgClientId ?? "", phoneNumber ?? "");
                        }

                        switch (status)
                        {
                            case "connected":
                                UpdateStatus($"WhatsApp Linked: {clientName} ({phoneNumber})!", Color.FromArgb(37, 211, 102));
                                ClientLinked?.Invoke(msgClientId ?? "", phoneNumber ?? "", clientName, _apiUsername, _apiPassword, _ownerNumber);
                                _isClosing = true;
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                                break;

                            case "linking_in_progress":
                            case "linking_qr":
                                UpdateStatus(statusMsg ?? "Waiting for QR...", Color.Goldenrod);
                                break;

                            case "disconnected":
                            case "logged_out":
                                UpdateStatus(statusMsg ?? "Disconnected. Ready to link again.", Color.Gray);
                                ClearQrDisplay();
                                break;

                            case "error":
                                // Don't close on generic errors unless it's specifically about our linking
                                if (statusMsg?.Contains("linking process is already in progress") == true)
                                {
                                    // This is likely a duplicate request, ignore it
                                    Console.WriteLine("[QR_FORM] Ignoring duplicate linking error");
                                    return;
                                }
                                UpdateStatus($"Linking Error: {statusMsg ?? "Unknown error"}", Color.Red);
                                _isClosing = true;
                                this.DialogResult = DialogResult.Abort;
                                this.Close();
                                break;

                            case "linking_failed":
                            case "error_whatsapp_permanent":
                            case "error_startup":
                                UpdateStatus($"Linking Error: {statusMsg ?? "Unknown error"}. Please try again.", Color.Red);
                                _isClosing = true;
                                this.DialogResult = DialogResult.Abort;
                                this.Close();
                                break;

                            default:
                                UpdateStatus(statusMsg ?? $"Status: {status}", Color.Goldenrod);
                                break;
                        }
                        break;

                    case "error":
                        if (msgClientId == this.CurrentClientId || string.IsNullOrEmpty(this.CurrentClientId))
                        {
                            string? errorMessage = message["message"]?.ToString();
                            if (errorMessage?.Contains("linking process is already in progress") == true)
                            {
                                Console.WriteLine("[QR_FORM] Ignoring duplicate linking error message");
                                return;
                            }
                            UpdateStatus($"Error from manager: {errorMessage}", Color.Red);
                            _isClosing = true;
                            this.DialogResult = DialogResult.Abort;
                            this.Close();
                        }
                        break;
                }
            }
            catch (JsonException jex)
            {
                Console.WriteLine($"[QR_FORM_ERROR] JSON parse error: {jex.Message}. Msg: {messageJson}");
                UpdateStatus("Error processing update (JSON issue).", Color.Red);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QR_FORM_ERROR] General error processing update: {ex.Message}. Msg: {messageJson}");
                UpdateStatus("Error processing update.", Color.Red);
            }
        }

        private void GenerateAndDisplayQr(string qrText)
        {
            try
            {
                if (string.IsNullOrEmpty(qrText)) { ClearQrDisplay(); return; }
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeAsPngBytes = qrCode.GetGraphic(10, new byte[] { 0, 0, 0, 255 }, new byte[] { 255, 255, 255, 255 });
                using (var ms = new MemoryStream(qrCodeAsPngBytes)) { qrPictureBox.Image = new Bitmap(ms); }
            }
            catch (Exception ex) { UpdateStatus("Error displaying QR code.", Color.Red); qrPictureBox.Image = null; Console.WriteLine($"QR Gen Error: {ex}"); }
        }

        private void ClearQrDisplay()
        {
            if (qrPictureBox.Image != null) { qrPictureBox.Image.Dispose(); qrPictureBox.Image = null; }
        }

        private void UpdateStatus(string message, Color color)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = color;
        }
    }
}
