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

namespace WhatsAppLinkerApp
{
    public partial class Form1 : Form
    {
        // --- MODIFIED: Mark as nullable if they can be null initially ---
        private ClientWebSocket? _managerControlWs;
        private CancellationTokenSource? _controlWsCts;
        private const string NodeJsWebSocketUrl = "ws://localhost:8088"; // Use for local testing
        // private const string NodeJsWebSocketUrl = "ws://134.119.194.180:8088"; // Use for remote server

        private QrDisplayForm? _currentQrDisplayForm; // Make nullable
        private GroupsDisplayForm? _activeGroupsForm; // Make nullable
        // --- END MODIFIED ---
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
            _controlWsCts = new CancellationTokenSource();
            _managerControlWs = new ClientWebSocket();
            await ConnectToManagerControlWs();
            if (_managerControlWs.State == WebSocketState.Open)
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
            var buffer = new byte[32768];
            try
            {
                while (wsClient.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    if (result.MessageType == WebSocketMessageType.Close) break;
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessManagerControlMessage(messageJson);
                    }
                }
            }
            catch (OperationCanceledException) { Console.WriteLine("ReceiveManagerControlMessagesAsync: Task was cancelled."); }
            catch (WebSocketException ex)
            {
                this.Invoke((MethodInvoker)delegate {
                    SetUIConnectedState(false);
                    if (!token.IsCancellationRequested) MessageBox.Show($"Connection to the Bot Manager was lost: {ex.Message}\n\nPlease ensure the Node.js Manager application is running...", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate {
                    SetUIConnectedState(false);
                    if (!token.IsCancellationRequested) MessageBox.Show($"An unexpected error occurred with the connection: {ex.Message}", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
            finally
            {
                this.Invoke((MethodInvoker)delegate {
                    SetUIConnectedState(false);
                    if (!token.IsCancellationRequested && !_isConnecting) Task.Delay(3000).ContinueWith(t => ConnectToManagerControlWs());
                });
            }
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
                            type == "error" || type == "participantDetailsUpdate") // <<<<<<<<<< ADDED participantDetailsUpdate
                        {
                            if(type == "participantDetailsUpdate") // <<<<<<<<<<<<<<<<<<<<<<<< NEW BLOCK
                            {
                                _activeGroupsForm.ProcessParticipantDetailsUpdate(messageJson);
                            }
                            else // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< END NEW BLOCK
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
                            if (_currentQrDisplayForm == null || _currentQrDisplayForm.IsDisposed) {
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
                                if(targetClientId != null) logTextBox.AppendText($"Cache & logs for client {targetClientId} cleared: {clearMessage}\r\n");
                                Task.Run(async () => await RequestInstanceList());
                            }
                            else
                            {
                                MessageBox.Show($"Failed to clear cache and logs for client {targetClientId ?? "Unknown"}: {clearMessage}", "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                if(targetClientId != null) logTextBox.AppendText($"Failed to clear cache & logs for client {targetClientId}: {clearMessage}\r\n");
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
                    Console.WriteLine("Error parsing manager control WS message on UI thread: " + ex.Message + "\nJSON: " + messageJson.Substring(0, Math.Min(messageJson.Length,500)));
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
                    if(status != null) item.SubItems[3].Text = status;
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
                (comboBoxCountryCode.SelectedItem != null);
        }

        private void InstanceListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool hasSelection = instanceListView.SelectedItems.Count > 0;
            btnManageGroups.Enabled = hasSelection;
            if (hasSelection) logTextBox.Clear();
            if (_managerControlWs != null) SetUIConnectedState(_managerControlWs.State == WebSocketState.Open);
        }

        private async void linkWhatsappButton_Click(object? sender, EventArgs e)
        {
            if (!linkWhatsappButton.Enabled) return;
            string apiUsername = textBoxApiUsername.Text.Trim();
            string apiPassword = textBoxApiPassword.Text.Trim();
            string? selectedCountryCodeItem = comboBoxCountryCode.SelectedItem?.ToString();
            if (selectedCountryCodeItem == null) { MessageBox.Show("Please select a country code."); return; }
            string selectedCountryCode = selectedCountryCodeItem.Split(' ')[0]?.Replace("+", "").Trim() ?? "";
            string ownerPhoneNumberPart = textBoxPhoneNumber.Text.Trim();
            string ownerNumber = $"{selectedCountryCode}{ownerPhoneNumberPart}";

            if (_managerControlWs == null || _controlWsCts == null) { MessageBox.Show("Connection not initialized."); return; }
            _currentQrDisplayForm = new QrDisplayForm(_managerControlWs, _controlWsCts, apiUsername, apiPassword, ownerNumber);
            _currentQrDisplayForm.ClientLinked += QrForm_ClientLinked;
            _currentQrDisplayForm.ShowDialog(this);
            _currentQrDisplayForm = null;
            await RequestInstanceList();
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
                    instanceListView.SelectedItems[0].Remove();
                    logTextBox.AppendText($"Requested deletion for client {clientId}.\r\n");
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
            base.OnFormClosing(e);
            _controlWsCts?.Cancel();
            _controlWsCts?.Dispose();
            Task.Run(async () =>
            {
                if (_managerControlWs != null && _managerControlWs.State == WebSocketState.Open)
                {
                    try { await _managerControlWs.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None); } catch { /* ignore */ }
                }
            }).Wait(1500); // خفض مهلة الانتظار قليلاً
            _managerControlWs?.Dispose();
        }

        private void UpdateConnectionStatus(string message, Color color)
        {
            if (lblConnectionStatus.InvokeRequired) lblConnectionStatus.Invoke((MethodInvoker)delegate { lblConnectionStatus.Text = $"Status: {message}"; lblConnectionStatus.ForeColor = color; });
            else { lblConnectionStatus.Text = $"Status: {message}"; lblConnectionStatus.ForeColor = color; }
        }

        private void SetUIConnectedState(bool connected)
        {
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
    }
}