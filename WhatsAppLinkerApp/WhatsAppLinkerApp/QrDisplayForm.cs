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
using QRCoder; // Ensure this NuGet package is installed

namespace WhatsAppLinkerApp
{
    public partial class QrDisplayForm : Form
    {
        private ClientWebSocket _sharedWebSocketClient;
        private CancellationTokenSource _sharedCancellationTokenSource;

        private string _apiUsername;
        private string _apiPassword;
        private string _ownerNumber;

        public event Action<string, string, string> ClientLinked;

        // Modified constructor to accept ownerNumber
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

        private async void QrDisplayForm_Load(object sender, EventArgs e)
        {
            if (_sharedWebSocketClient != null && _sharedWebSocketClient.State == WebSocketState.Open)
            {
                UpdateStatus("Connecting to manager. Requesting QR code...", Color.Goldenrod); // Amber for connecting
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

        private void QrDisplayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If the form is closed not via DialogResult.OK (meaning linking failed or user canceled)
            if (this.DialogResult != DialogResult.OK)
            {
                // If it wasn't a successful link and the form closes, assume cancellation or error,
                // no explicit manualRelink needed here from QrDisplayForm as Form1's flow for failed linking handles this.
                // Just ensuring cleanup of QR if any is shown
                ClearQrDisplay();
            }
            // No need to close/dispose the shared WebSocket here; Form1 owns it.
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
                if (_sharedCancellationTokenSource != null && !_sharedCancellationTokenSource.IsCancellationRequested)
                {
                    await _sharedWebSocketClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _sharedCancellationTokenSource.Token);
                    UpdateStatus("Requested new QR code from bot manager...", Color.Goldenrod);
                }
                else
                {
                    Console.WriteLine("Shared CancellationTokenSource is cancelled or null, cannot send requestQr.");
                    UpdateStatus("Request aborted.", Color.Red);
                }
            }
            else
            {
                UpdateStatus("Shared WebSocket not connected, cannot request QR.", Color.Red);
                Console.WriteLine("RequestQrFromManager: Shared WebSocket not open or null.");
            }
        }

        // Modified ManualRelinkFromManager - this was for button clicks previously, not strictly needed for the initial auto-link
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
                if (_sharedCancellationTokenSource != null && !_sharedCancellationTokenSource.IsCancellationRequested)
                    await _sharedWebSocketClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _sharedCancellationTokenSource.Token);
                else
                    Console.WriteLine("Shared CancellationTokenSource is cancelled or null, cannot send manual re-link request.");
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
                    string type = message["type"]?.ToString();
                    string clientId = message["clientId"]?.ToString();
                    string phoneNumber = message["phoneNumber"]?.ToString();
                    string clientName = message["name"]?.ToString();

                    if (type == "qr")
                    {
                        string qrData = message["qr"]?.ToString();
                        if (!string.IsNullOrEmpty(qrData))
                        {
                            GenerateAndDisplayQr(qrData);
                            UpdateStatus("Scan the QR code with WhatsApp on your phone. " + (clientId != null ? $" (Client: {clientId})" : ""), SystemColors.ControlText); // Default color for scan
                        }
                        else
                        {
                            UpdateStatus("Waiting for QR or connection...", Color.Goldenrod);
                            ClearQrDisplay();
                        }
                    }
                    else if (type == "status")
                    {
                        string status = message["status"]?.ToString();
                        string qrFromStatus = message["qr"]?.ToString(); // Manager might send QR even in status update
                        string errorMsg = message["message"]?.ToString();

                        switch (status)
                        {
                            case "connected":
                                UpdateStatus($"WhatsApp Linked: {clientName ?? phoneNumber}!", Color.FromArgb(37, 211, 102)); // Bright Green
                                ClearQrDisplay();
                                if (ClientLinked != null && clientId != null && phoneNumber != null)
                                {
                                    ClientLinked.Invoke(clientId, phoneNumber, clientName);
                                }
                                this.DialogResult = DialogResult.OK; // Signal successful completion
                                this.Close(); // Close the form
                                break;
                            case "qr": // Should already be handled by specific 'qr' type message, but as fallback.
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
                            case "linking_failed": // These indicate failure in linking or existing connection issues
                                UpdateStatus($"Linking Error: {errorMsg ?? "An unexpected error occurred."} Please try again.", Color.Red);
                                ClearQrDisplay(); // Clear QR on error
                                this.DialogResult = DialogResult.Abort; // Signal abortion/failure to parent
                                // DO NOT CLOSE YET. Let user see the message. User will close manually.
                                break;
                            case "connecting":
                            case "reconnecting":
                            case "linking_in_progress":
                                UpdateStatus(errorMsg ?? "Connecting...", Color.Goldenrod); // Amber for connecting
                                if (!string.IsNullOrEmpty(qrFromStatus))
                                {
                                    GenerateAndDisplayQr(qrFromStatus);
                                }
                                else { ClearQrDisplay(); } // Clear QR if not present in connecting status
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
                // 20 is pixels per module; adjust for resolution needed. 10 is usually fine for screen
                // Use the overload that takes hex color strings if your QRCoder version requires it:
                byte[] qrCodeAsPngBytes = qrCode.GetGraphic(10, new byte[] { 0, 0, 0 }, new byte[] { 255, 255, 255 });

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