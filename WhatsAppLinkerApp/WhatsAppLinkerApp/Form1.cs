// Form1.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography; // For ProtectedData
using System.IO; // For File operations
using Newtonsoft.Json; // For JsonConvert

namespace WhatsAppLinkerApp
{
    // New class for application settings persistence
    public class AppSettings
    {
        public string? ApiUsername { get; set; }
        public string? EncryptedApiPassword { get; set; } // Store encrypted password
        public string? OwnerCountryCodeSelectedItem { get; set; } // Store the selected item string
        public string? OwnerPhoneNumber { get; set; }
    }

    public partial class Form1 : Form
    {
        private ClientWebSocket? _managerControlWs;
        private CancellationTokenSource? _controlWsCts;
        private const string NodeJsWebSocketUrl = "ws://localhost:8088"; // Use for local testing
        // private const string NodeJsWebSocketUrl = "ws://134.119.194.180:8088"; // Use for remote server

        private QrDisplayForm? _currentQrDisplayForm; // Make nullable
        private GroupsDisplayForm? _activeGroupsForm; // Make nullable
        private bool _isConnecting = false; // Flag to prevent multiple concurrent connection attempts

        public Form1()
        {
            InitializeComponent();
            ApplyUIStyles();
            InitializeCountryCodeComboBox();
            EnableLinkButtonBasedOnInput();

            linkWhatsappButton.Click += linkWhatsappButton_Click;
            btnRefreshInstances.Click += btnRefreshInstances_Click;
            btnStartInstance.Click += btnStartInstance_Click;
            btnStopInstance.Click += btnStopInstance_Click;
            btnStopAndDeleteInstance.Click += btnStopAndDeleteInstance_Click;
            btnRestartInstance.Click += btnRestartInstance_Click;
            btnGetLogs.Click += btnGetLogs_Click;
            btnManageGroups.Click += btnManageGroups_Click;
            instanceListView.SelectedIndexChanged += InstanceListView_SelectedIndexChanged;
        }

        private void ApplyUIStyles()
        {
            Action<Button, Color, Color> setButtonStyle = (btn, backColor, foreColor) =>
            {
                btn.BackColor = backColor;
                btn.ForeColor = foreColor;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
            };

            setButtonStyle(linkWhatsappButton, Color.FromArgb(37, 211, 102), Color.White);
            setButtonStyle(btnRefreshInstances, Color.FromArgb(18, 140, 126), Color.White);
            setButtonStyle(btnStartInstance, Color.FromArgb(37, 211, 102), Color.White);
            setButtonStyle(btnStopInstance, Color.IndianRed, Color.White);
            setButtonStyle(btnStopAndDeleteInstance, Color.Firebrick, Color.White);
            setButtonStyle(btnRestartInstance, Color.Goldenrod, Color.White);
            setButtonStyle(btnGetLogs, Color.SteelBlue, Color.White);
            setButtonStyle(btnManageGroups, Color.FromArgb(18, 140, 126), Color.White);

            logTextBox.BackColor = Color.FromArgb(32, 44, 51);
            logTextBox.ForeColor = Color.WhiteSmoke;
            logTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

            instanceListView.OwnerDraw = true;
            instanceListView.DrawColumnHeader += (s, e) =>
            {
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var brush = new SolidBrush(Color.FromArgb(18, 140, 126)))
                using (var pen = new Pen(Color.FromArgb(12, 100, 90), 1))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                    e.Graphics.DrawRectangle(pen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                    TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
            };
            instanceListView.DrawSubItem += (s, e) => e.DrawDefault = true;
        }

        private void InitializeCountryCodeComboBox()
        {
            var countryCodes = new List<string>
            {
                "+967 (Yemen)", "+966 (Saudi Arabia)", "+971 (UAE)",
                "+20 (Egypt)", "+973 (Bahrain)", "+974 (Qatar)",
                "+965 (Kuwait)", "+968 (Oman)", "+1 (USA/Canada)",
                "+44 (UK)", "+49 (Germany)", "+33 (France)"
            };
            comboBoxCountryCode.Items.AddRange(countryCodes.ToArray());
            var defaultYemen = countryCodes.FirstOrDefault(c => c.StartsWith("+967"));
            if (defaultYemen != null) comboBoxCountryCode.SelectedItem = defaultYemen;
            else comboBoxCountryCode.SelectedIndex = 0;

            comboBoxCountryCode.SelectedIndexChanged += (s, e) => EnableLinkButtonBasedOnInput();
            textBoxPhoneNumber.TextChanged += (s, e) => EnableLinkButtonBasedOnInput();
            textBoxApiUsername.TextChanged += (s, e) => EnableLinkButtonBasedOnInput();
            textBoxApiPassword.TextChanged += (s, e) => EnableLinkButtonBasedOnInput();
        }

        private async void Form1_Load(object? sender, EventArgs e) // sender can be null
        {
            LoadAppSettings(); // Load settings first

            _controlWsCts = new CancellationTokenSource();
            _managerControlWs = new ClientWebSocket();
            await ConnectToManagerControlWs();
            if (_managerControlWs != null && _managerControlWs.State == WebSocketState.Open) // Check _managerControlWs for null
            {
                await RequestInstanceList();
            }
        }

        private async Task ConnectToManagerControlWs()
        {
            if (_isConnecting) return;
            if (_managerControlWs != null && (_managerControlWs.State == WebSocketState.Open || _managerControlWs.State == WebSocketState.Connecting))
            {
                SetUIConnectedState(true);
                return;
            }
            _isConnecting = true;
            _controlWsCts?.Cancel();
            _controlWsCts?.Dispose();
            _controlWsCts = new CancellationTokenSource();
            _managerControlWs?.Dispose();
            _managerControlWs = new ClientWebSocket();
            UpdateConnectionStatus("Connecting...", Color.Goldenrod);
            SetUIConnectedState(false);
            try
            {
                var connectTask = _managerControlWs.ConnectAsync(new Uri(NodeJsWebSocketUrl), _controlWsCts.Token);
                var completedTask = await Task.WhenAny(connectTask, Task.Delay(5000, _controlWsCts.Token));
                if (completedTask == connectTask && connectTask.Status == TaskStatus.RanToCompletion)
                {
                    SetUIConnectedState(true);
                    _ = ReceiveManagerControlMessagesAsync(_managerControlWs, _controlWsCts.Token);
                }
                else
                {
                    _controlWsCts.Cancel();
                    _managerControlWs.Abort();
                    throw new WebSocketException("Connection attempt timed out or failed.");
                }
            }
            catch (WebSocketException ex)
            {
                SetUIConnectedState(false);
                MessageBox.Show($"Error connecting to manager: {ex.Message}. Ensure Node.js manager is running and accessible at {NodeJsWebSocketUrl}.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OperationCanceledException) { SetUIConnectedState(false); }
            catch (Exception ex)
            {
                SetUIConnectedState(false);
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { _isConnecting = false; }
        }

        private async Task ReceiveManagerControlMessagesAsync(ClientWebSocket wsClient, CancellationToken token)
        {
            var buffer = new ArraySegment<byte>(new byte[32768]); // استخدام ArraySegment مباشرة
            var messageBuilder = new StringBuilder(); // لتجميع الرسائل المجزأة

            try
            {
                while (wsClient.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    WebSocketReceiveResult result;
                    var currentSegment = new List<byte>(); // لتخزين بايتات الرسالة الحالية

                    do // حلقة لتجميع كافة أجزاء الرسالة
                    {
                        result = await wsClient.ReceiveAsync(buffer, token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Console.WriteLine("Manager Control WS disconnected by server (graceful close).");
                            // معالجة الإغلاق هنا بشكل مباشر أو الخروج من الحلقة الرئيسية
                            if (wsClient.State == WebSocketState.Open && !token.IsCancellationRequested) // التأكد قبل محاولة الإغلاق
                            {
                                try
                                {
                                    await wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client acknowledging close", token);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Exception during client initiated close: {ex.Message}");
                                }
                            }
                            goto EndReceiveLoop; // الانتقال إلى نهاية الحلقة الخارجية
                        }

                        // إضافة البايتات المستلمة إلى القائمة المؤقتة
                        if (buffer.Array != null)
                        {
                            for (int i = 0; i < result.Count; i++)
                            {
                                currentSegment.Add(buffer.Array[buffer.Offset + i]);
                            }
                        }

                    } while (!result.EndOfMessage); // استمر في التجميع حتى يتم استلام نهاية الرسالة

                    // الآن لدينا الرسالة الكاملة في currentSegment
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageJson = Encoding.UTF8.GetString(currentSegment.ToArray(), 0, currentSegment.Count);

                        // >>>>>>>>>>>> نقطة تسجيل مهمة <<<<<<<<<<<<<<<
                        Console.WriteLine($"[RAW_WS_MESSAGE_RECEIVED] Length: {messageJson.Length}, Content (first 500 chars): {messageJson.Substring(0, Math.Min(messageJson.Length, 500))}");
                        if (string.IsNullOrWhiteSpace(messageJson))
                        {
                            Console.WriteLine("[RAW_WS_MESSAGE_RECEIVED] Received empty or whitespace message. Skipping processing.");
                            continue; // تجاهل الرسائل الفارغة
                        }
                        // >>>>>>>>>>>> نهاية نقطة التسجيل <<<<<<<<<<<<<<<

                        ProcessManagerControlMessage(messageJson);
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        // إذا كنت تتوقع رسائل ثنائية، عالجها هنا
                        Console.WriteLine("Received binary message from manager, which is not expected for JSON communication.");
                    }
                    messageBuilder.Clear(); // امسح الـ builder للرسالة التالية (إذا كنت ستستخدمه)
                                            // في هذا السيناريو، currentSegment يتم إنشاؤه لكل رسالة
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("ReceiveManagerControlMessagesAsync: Task was cancelled.");
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"Manager Control WS error/disconnect: {ex.Message}. Connection lost.");
                if (!this.IsDisposed && this.IsHandleCreated) // تأكد أن الفورم لا يزال صالحًا
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        SetUIConnectedState(false);
                        if (!token.IsCancellationRequested)
                        {
                            MessageBox.Show(
                                this, // حدد المالك
                                $"Connection to the Bot Manager was lost: {ex.Message}\n\nPlease ensure the Node.js Manager application is running and accessible, then restart this UI application if necessary.",
                                "Connection Lost",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in ReceiveManagerControlMessagesAsync: {ex.Message} \nStack: {ex.StackTrace}");
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        SetUIConnectedState(false);
                        if (!token.IsCancellationRequested)
                        {
                            MessageBox.Show(
                                this, // حدد المالك
                                $"An unexpected error occurred with the connection: {ex.Message}",
                                "Communication Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                        }
                    });
                }
            }
            finally
            {
                Console.WriteLine("ReceiveManagerControlMessagesAsync: Exiting receive loop.");
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        SetUIConnectedState(false);
                        if (!token.IsCancellationRequested && _managerControlWs != null && _managerControlWs.State != WebSocketState.Open && !_isConnecting)
                        {
                            Console.WriteLine("Attempting to reconnect to Manager WS in 3 seconds...");
                            Task.Delay(3000, token).ContinueWith(t =>
                            { // مرر التوكن للـ Delay أيضًا
                                if (!t.IsCanceled) ConnectToManagerControlWs();
                            }, TaskScheduler.FromCurrentSynchronizationContext()); // لتشغيل Connect على UI thread إذا لزم الأمر
                        }
                    });
                }
            }

        EndReceiveLoop: // ليبل للانتقال إليه عند إغلاق الاتصال
            Console.WriteLine("ReceiveManagerControlMessagesAsync: Reached end of receive loop or connection closed.");
        }
        private void ProcessManagerControlMessage(string messageJson)
        {
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    JObject message = JObject.Parse(messageJson);
                    string type = message["type"]?.ToString() ?? string.Empty;
                    string? clientId = message["clientId"]?.ToString(); // Nullable

                    // 1. Route to QR display form if active
                    if (_currentQrDisplayForm != null && !_currentQrDisplayForm.IsDisposed)
                    {
                        if (type == "qr" || type == "status") // "status" is often related to linking process
                        {
                            _currentQrDisplayForm.ProcessManagerMessageForQRDisplay(messageJson);
                            if (_currentQrDisplayForm.DialogResult == DialogResult.OK) // Check after processing
                            {
                                _currentQrDisplayForm = null; // Clear if linking was successful
                            }
                        }
                    }

                    // 2. Route to Groups display form if active
                    // Ensure to check clientId as well for targeted messages
                    if (_activeGroupsForm != null && !_activeGroupsForm.IsDisposed && _activeGroupsForm.Tag?.ToString() == clientId)
                    {
                        // Check specific types for GroupsDisplayForm or let it decide
                        if (type == "groupsList" || type == "participantsList" ||
                            type == "addChatToWhitelistResponse" || type == "removeFromChatWhitelistResponse" ||
                            type == "error" || type == "participantDetailsUpdate")
                        {
                            if (type == "participantDetailsUpdate")
                            {
                                _activeGroupsForm.ProcessParticipantDetailsUpdate(messageJson);
                            }
                            else
                            {
                                _activeGroupsForm.ProcessGroupsDisplayMessage(messageJson);
                            }
                        }
                    }

                    // 3. Process main Form1 specific messages
                    switch (type)
                    {
                        case "instanceList":
                            UpdateInstanceListView(message["instances"] as JArray);
                            break;
                        case "instanceStatusUpdate":
                            UpdateSingleInstanceStatus(clientId, message["status"]?.ToString(), message["phoneNumber"]?.ToString(), message["name"]?.ToString());
                            break;
                        case "instanceLogs":
                            DisplayInstanceLogs(clientId, message["logs"] as JArray);
                            break;
                        case "status": // General status messages not for QR form
                            if (_currentQrDisplayForm == null || _currentQrDisplayForm.IsDisposed)
                            {
                                Console.WriteLine($"Main Form (Manager Overall Status): Type: {type}, Status: {message["status"]}, Message: {message["message"]}");
                            }
                            break;
                        case "clearCacheResponse":
                            bool success = message["success"]?.ToObject<bool>() == true;
                            string clearMessage = message["message"]?.ToString() ?? "Operation completed.";
                            string? targetClientId = message["clientId"]?.ToString(); // Nullable
                            if (success)
                            {
                                MessageBox.Show($"Cache and logs for client {targetClientId ?? "Unknown"} cleared successfully: {clearMessage}", "Operation Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                if (targetClientId != null) logTextBox.AppendText($"Cache & logs for client {targetClientId} cleared: {clearMessage}\r\n");
                                Task.Run(async () => await RequestInstanceList());
                            }
                            else
                            {
                                MessageBox.Show($"Failed to clear cache and logs for client {targetClientId ?? "Unknown"}: {clearMessage}", "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                if (targetClientId != null) logTextBox.AppendText($"Failed to clear cache & logs for client {targetClientId}: {clearMessage}\r\n");
                            }
                            break;
                        default:
                            if (type != "internalReply" && type != "participantDetailsUpdate") // Avoid logging if already handled by GroupsForm
                            {
                                Console.WriteLine($"Unhandled manager control message type on Main Form: {type}. Full message: {messageJson.Substring(0, Math.Min(messageJson.Length, 200))}");
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing manager control WS message on UI thread: " + ex.Message + "\nJSON: " + messageJson.Substring(0, Math.Min(messageJson.Length, 500)));
                }
            });
        }

        private void UpdateInstanceListView(JArray? instances) // Nullable
        {
            instanceListView.Items.Clear();
            if (instances == null) return;
            foreach (JObject instance in instances.OfType<JObject>())
            {
                string? clientId = instance["clientId"]?.ToString();
                string? phoneNumber = instance["phoneNumber"]?.ToString();
                string? name = instance["name"]?.ToString();
                string? status = instance["status"]?.ToString();
                ListViewItem item = new ListViewItem(clientId ?? "N/A");
                item.SubItems.Add(phoneNumber ?? "N/A");
                item.SubItems.Add(name ?? "N/A");
                item.SubItems.Add(status ?? "N/A");
                item.Tag = clientId;
                instanceListView.Items.Add(item);
            }
            instanceListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            instanceListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            if (instanceListView.Items.Count > 0) { instanceListView.Items[0].Selected = true; instanceListView.Items[0].Focused = true; }
        }

        private void UpdateSingleInstanceStatus(string? clientId, string? status, string? phoneNumber = null, string? name = null) // Nullable
        {
            if (string.IsNullOrEmpty(clientId)) return;
            foreach (ListViewItem item in instanceListView.Items)
            {
                if (item.Tag?.ToString() == clientId)
                {
                    if (status != null) item.SubItems[3].Text = status;
                    if (phoneNumber != null) item.SubItems[1].Text = phoneNumber;
                    if (name != null) item.SubItems[2].Text = name;
                    return;
                }
            }
            Task.Run(async () => await RequestInstanceList());
        }

        private void DisplayInstanceLogs(string? clientId, JArray? logs) // Nullable
        {
            logTextBox.Clear();
            if (string.IsNullOrEmpty(clientId)) { logTextBox.AppendText("Client ID missing for logs.\r\n"); return; }
            if (logs == null || logs.Count == 0) { logTextBox.AppendText($"--- No logs available for Client: {clientId} ---\r\n"); return; }
            logTextBox.AppendText($"--- Logs for Client: {clientId} ---\r\n");
            foreach (var lineToken in logs) { logTextBox.AppendText(lineToken?.ToString() + "\r\n"); }
            logTextBox.AppendText($"--- End of Logs for Client: {clientId} ---\r\n");
            logTextBox.ScrollToCaret();
        }

        private async Task SendManagerCommand(string type, string? clientId = null, string? apiUsername = null, string? apiPassword = null, string? ownerNumber = null, string? groupId = null, string? participantJid = null) // Nullable
        {
            if (_managerControlWs == null || _managerControlWs.State != WebSocketState.Open || _controlWsCts == null)
            {
                MessageBox.Show("Not connected to manager.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }
            var request = new JObject { ["type"] = type };
            if (clientId != null) request["clientId"] = clientId;
            if (apiUsername != null) request["apiUsername"] = apiUsername;
            if (apiPassword != null) request["apiPassword"] = apiPassword;
            if (ownerNumber != null) request["ownerNumber"] = ownerNumber;
            if (groupId != null) request["groupId"] = groupId;
            if (participantJid != null) request["participantJid"] = participantJid;
            var buffer = Encoding.UTF8.GetBytes(request.ToString());
            try
            {
                await _managerControlWs.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _controlWsCts.Token);
            }
            catch (Exception ex) { MessageBox.Show($"Failed to send command: {ex.Message}", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async Task RequestInstanceList() { await SendManagerCommand("listInstances"); }

        private void EnableLinkButtonBasedOnInput()
        {
            linkWhatsappButton.Enabled =
                !string.IsNullOrWhiteSpace(textBoxApiUsername.Text) &&
                !string.IsNullOrWhiteSpace(textBoxApiPassword.Text) &&
                !string.IsNullOrWhiteSpace(textBoxPhoneNumber.Text) &&
                (comboBoxCountryCode.SelectedItem != null) &&
                (_managerControlWs != null && _managerControlWs.State == WebSocketState.Open); // Only enable if connected
        }

        private void InstanceListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool hasSelection = instanceListView.SelectedItems.Count > 0;
            // Only enable manage groups if there's a selection AND we are connected
            btnManageGroups.Enabled = hasSelection && (_managerControlWs != null && _managerControlWs.State == WebSocketState.Open);
            if (hasSelection) logTextBox.Clear();
            if (_managerControlWs != null) SetUIConnectedState(_managerControlWs.State == WebSocketState.Open);
        }

        private async void linkWhatsappButton_Click(object? sender, EventArgs e)
        {
            // Important: Disable the button immediately upon click.
            // This prevents multiple rapid clicks from queuing up.
            linkWhatsappButton.Enabled = false;

            // Validate inputs here to ensure we don't proceed with invalid data
            string apiUsername = textBoxApiUsername.Text.Trim();
            string apiPassword = textBoxApiPassword.Text.Trim();
            string? selectedCountryCodeItem = comboBoxCountryCode.SelectedItem?.ToString();
            if (selectedCountryCodeItem == null) { MessageBox.Show("Please select a country code."); EnableLinkButtonBasedOnInput(); return; } // Re-enable by re-evaluating
            string selectedCountryCode = selectedCountryCodeItem.Split(' ')[0]?.Replace("+", "").Trim() ?? "";
            string ownerPhoneNumberPart = textBoxPhoneNumber.Text.Trim();
            string ownerNumber = $"{selectedCountryCode}{ownerPhoneNumberPart}";

            if (_managerControlWs == null || _controlWsCts == null || _managerControlWs.State != WebSocketState.Open)
            {
                MessageBox.Show("Not connected to manager. Please wait for connection or restart the app.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetUIConnectedState(false); // Re-evaluate UI state (will re-enable if connected, otherwise keep disabled)
                return;
            }

            try
            {
                _currentQrDisplayForm = new QrDisplayForm(_managerControlWs, _controlWsCts, apiUsername, apiPassword, ownerNumber);
                _currentQrDisplayForm.ClientLinked += QrForm_ClientLinked;

                _currentQrDisplayForm.ShowDialog(this); // Blocking call
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during WhatsApp linking process: {ex.Message}", "Linking Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _currentQrDisplayForm = null; // Clear the reference
                await RequestInstanceList(); // After QR form closes, refresh the instance list
                SetUIConnectedState(_managerControlWs.State == WebSocketState.Open); // Re-evaluate button state based on connection
            }
        }

        private void QrForm_ClientLinked(string clientId, string phoneNumber, string clientName)
        {
            MessageBox.Show($"Client successfully linked!\nID: {clientId}\nPhone: {phoneNumber}\nName: {clientName}", "WhatsApp Linked", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnRefreshInstances_Click(object? sender, EventArgs e) { await RequestInstanceList(); }

        private async void btnStartInstance_Click(object? sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0 && instanceListView.SelectedItems[0].Tag is string clientId)
                await SendManagerCommand("startInstance", clientId);
            else MessageBox.Show("Please select an instance.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnStopInstance_Click(object? sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0 && instanceListView.SelectedItems[0].Tag is string clientId)
                await SendManagerCommand("stopInstance", clientId);
            else MessageBox.Show("Please select an instance.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnStopAndDeleteInstance_Click(object? sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0 && instanceListView.SelectedItems[0].Tag is string clientId)
            {
                string clientDisplayName = instanceListView.SelectedItems[0].SubItems[2].Text;
                string clientPhoneNumber = instanceListView.SelectedItems[0].SubItems[1].Text;
                var confirmResult = MessageBox.Show($"Are you sure you want to STOP and DELETE instance {clientDisplayName} ({clientPhoneNumber})?\nThis action will permanently remove all associated data and sessions.\nThis action cannot be undone.", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirmResult == DialogResult.Yes)
                {
                    await SendManagerCommand("deleteInstance", clientId);
                    // No need to remove directly, refresh list will handle it
                    // instanceListView.SelectedItems[0].Remove();
                    logTextBox.AppendText($"Requested deletion for client {clientId}.\r\n");
                    // Re-request instance list to reflect deletion
                    await RequestInstanceList();
                    if (instanceListView.Items.Count > 0) instanceListView.Items[0].Selected = true;
                }
            }
            else MessageBox.Show("Please select an instance.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnRestartInstance_Click(object? sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0 && instanceListView.SelectedItems[0].Tag is string clientId)
                await SendManagerCommand("restartInstance", clientId);
            else MessageBox.Show("Please select an instance.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnGetLogs_Click(object? sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0 && instanceListView.SelectedItems[0].Tag is string clientId)
            {
                logTextBox.Clear();
                logTextBox.AppendText($"Requesting logs for {clientId}...\r\n");
                await SendManagerCommand("getLogs", clientId);
            }
            else MessageBox.Show("Please select an instance.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnManageGroups_Click(object? sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0 && instanceListView.SelectedItems[0].Tag is string clientId)
            {
                string clientPhoneNumber = instanceListView.SelectedItems[0].SubItems[1].Text;
                if (_managerControlWs == null || _controlWsCts == null) { MessageBox.Show("Connection not initialized."); return; }
                _activeGroupsForm = new GroupsDisplayForm(_managerControlWs, _controlWsCts, clientId, clientPhoneNumber);
                _activeGroupsForm.Tag = clientId;
                _activeGroupsForm.ShowDialog(this);
                _activeGroupsForm = null;
                await RequestInstanceList();
            }
            else MessageBox.Show("Please select an active instance.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveAppSettings(); // Save settings before closing procedures

            base.OnFormClosing(e);
            _controlWsCts?.Cancel();
            _controlWsCts?.Dispose(); // Dispose of CancellationTokenSource
            
            // Properly close and dispose WebSocket
            ClientWebSocket? wsToClose = _managerControlWs; // Local variable for thread safety
            _managerControlWs = null; // Prevent further use

            if (wsToClose != null)
            {
                Task.Run(async () =>
                {
                    if (wsToClose.State == WebSocketState.Open)
                    {
                        try 
                        { 
                            // Use a shorter timeout for closing to avoid hanging the UI
                            var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(2)); // 2-second timeout
                            await wsToClose.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "UI Closing", closeCts.Token); 
                        } 
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine("[FORM_CLOSING] WebSocket CloseOutputAsync timed out.");
                        }
                        catch (Exception ex) 
                        { 
                            Console.WriteLine($"[FORM_CLOSING_ERROR] Exception during WebSocket CloseOutputAsync: {ex.Message}");
                        }
                    }
                }).ContinueWith(t => {
                    wsToClose.Dispose(); // Dispose after attempting to close
                    Console.WriteLine("[FORM_CLOSING] WebSocket disposed.");
                });
            }
        }

private void UpdateConnectionStatus(string message, Color color)
{
    if (statusStrip.InvokeRequired)
    {
        statusStrip.Invoke((MethodInvoker)delegate 
        { 
            lblConnectionStatus.Text = $"Status: {message}"; 
            lblConnectionStatus.ForeColor = color; 
        });
    }
    else 
    { 
        lblConnectionStatus.Text = $"Status: {message}"; 
        lblConnectionStatus.ForeColor = color; 
    }
}
        private void SetUIConnectedState(bool connected)
        {
            // Update button states based on connection status and selection
            linkWhatsappButton.Enabled = connected && IsCredentialFieldsValid();
            btnRefreshInstances.Enabled = connected;
            bool hasSelection = instanceListView.SelectedItems.Count > 0;
            btnStartInstance.Enabled = connected && hasSelection;
            btnStopInstance.Enabled = connected && hasSelection;
            btnStopAndDeleteInstance.Enabled = connected && hasSelection;
            btnRestartInstance.Enabled = connected && hasSelection;
            btnGetLogs.Enabled = connected && hasSelection;
            btnManageGroups.Enabled = connected && hasSelection;
            UpdateConnectionStatus(connected ? "Connected to Manager" : "Disconnected from Manager", connected ? Color.Green : Color.Red);
        }

        private bool IsCredentialFieldsValid()
        {
            return !string.IsNullOrWhiteSpace(textBoxApiUsername.Text) &&
                   !string.IsNullOrWhiteSpace(textBoxApiPassword.Text) &&
                   !string.IsNullOrWhiteSpace(textBoxPhoneNumber.Text) &&
                   (comboBoxCountryCode.SelectedItem != null);
        }

        private string GetSettingsFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolderPath = Path.Combine(appDataPath, "WhatsAppLinkerApp"); // Your application's folder
            Directory.CreateDirectory(appFolderPath); // Ensure the directory exists
            return Path.Combine(appFolderPath, "app_settings.json");
        }

        private void LoadAppSettings()
        {
            string settingsFile = GetSettingsFilePath();
            if (File.Exists(settingsFile))
            {
                try
                {
                    string json = File.ReadAllText(settingsFile);
                    AppSettings? settings = JsonConvert.DeserializeObject<AppSettings>(json);

                    if (settings != null)
                    {
                        textBoxApiUsername.Text = settings.ApiUsername;
                        if (!string.IsNullOrEmpty(settings.EncryptedApiPassword))
                        {
                            try
                            {
                                byte[] encryptedData = Convert.FromBase64String(settings.EncryptedApiPassword);
                                byte[] decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
                                textBoxApiPassword.Text = Encoding.UTF8.GetString(decryptedData);
                            }
                            catch (CryptographicException ex)
                            {
                                Console.WriteLine($"[SETTINGS] Failed to decrypt password: {ex.Message}. Password field will be empty. This might happen if settings are moved to a different user/PC.");
                                textBoxApiPassword.Text = string.Empty;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[SETTINGS] Generic error decrypting password: {ex.Message}. Password field will be empty.");
                                textBoxApiPassword.Text = string.Empty;
                            }
                        }
                        textBoxPhoneNumber.Text = settings.OwnerPhoneNumber;

                        if (!string.IsNullOrEmpty(settings.OwnerCountryCodeSelectedItem))
                        {
                            int index = comboBoxCountryCode.FindStringExact(settings.OwnerCountryCodeSelectedItem);
                            if (index != -1)
                            {
                                comboBoxCountryCode.SelectedIndex = index;
                            }
                            else if (comboBoxCountryCode.Items.Count > 0)
                            {
                                comboBoxCountryCode.SelectedIndex = 0;
                            }
                        }
                        Console.WriteLine("[SETTINGS] App settings loaded.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SETTINGS_ERROR] Error loading app settings: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("[SETTINGS] No settings file found. Using default values.");
            }
            EnableLinkButtonBasedOnInput();
        }

        private void SaveAppSettings()
        {
            AppSettings settings = new AppSettings
            {
                ApiUsername = textBoxApiUsername.Text,
                OwnerCountryCodeSelectedItem = comboBoxCountryCode.SelectedItem?.ToString(),
                OwnerPhoneNumber = textBoxPhoneNumber.Text
            };

            if (!string.IsNullOrEmpty(textBoxApiPassword.Text))
            {
                try
                {
                    byte[] dataToEncrypt = Encoding.UTF8.GetBytes(textBoxApiPassword.Text);
                    byte[] encryptedData = ProtectedData.Protect(dataToEncrypt, null, DataProtectionScope.CurrentUser);
                    settings.EncryptedApiPassword = Convert.ToBase64String(encryptedData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SETTINGS_ERROR] Failed to encrypt password: {ex.Message}. Password will not be saved.");
                    settings.EncryptedApiPassword = null;
                }
            }
            else
            {
                settings.EncryptedApiPassword = null;
            }

            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(GetSettingsFilePath(), json);
                Console.WriteLine("[SETTINGS] App settings saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SETTINGS_ERROR] Error saving app settings: {ex.Message}");
            }
        }
    }
}
