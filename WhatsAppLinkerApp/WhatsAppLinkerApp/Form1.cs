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
        private ClientWebSocket _managerControlWs;
        private CancellationTokenSource _controlWsCts;
        private const string NodeJsWebSocketUrl = "ws://localhost:8088"; // Use for local testing
        // private const string NodeJsWebSocketUrl = "ws://134.119.194.180:8088"; // Use for remote server

        private QrDisplayForm _currentQrDisplayForm;
        private GroupsDisplayForm _activeGroupsForm; // New field for groups management form
        private bool _isConnecting = false; // Flag to prevent multiple concurrent connection attempts

        public Form1()
        {
            InitializeComponent();

            // Apply Custom UI Styling
            ApplyUIStyles();

            // Initialize components that depend on UI elements / Attach event handlers
            InitializeCountryCodeComboBox(); // This method also attaches validation event handlers
            EnableLinkButtonBasedOnInput(); // Initial validation check for the Link button

            // Explicitly attach all button click handlers (if not already done by designer)
            // This ensures they are wired up even if designer generated code is lost or modified
            linkWhatsappButton.Click += linkWhatsappButton_Click;
            btnRefreshInstances.Click += btnRefreshInstances_Click;
            btnStartInstance.Click += btnStartInstance_Click;
            btnStopInstance.Click += btnStopInstance_Click;
            btnStopAndDeleteInstance.Click += btnStopAndDeleteInstance_Click;
            btnRestartInstance.Click += btnRestartInstance_Click;
            btnGetLogs.Click += btnGetLogs_Click;
            btnManageGroups.Click += btnManageGroups_Click;

            // Attach selection change handler for ListView
            instanceListView.SelectedIndexChanged += InstanceListView_SelectedIndexChanged;
        }

        private void ApplyUIStyles()
        {
            // Set up common button styling
            Action<Button, Color, Color> setButtonStyle = (btn, backColor, foreColor) =>
            {
                btn.BackColor = backColor;
                btn.ForeColor = foreColor;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
            };

            // Apply styles based on color palette
            setButtonStyle(linkWhatsappButton, Color.FromArgb(37, 211, 102), Color.White); // Bright Green
            setButtonStyle(btnRefreshInstances, Color.FromArgb(18, 140, 126), Color.White); // Dark Green for Refresh
            setButtonStyle(btnStartInstance, Color.FromArgb(37, 211, 102), Color.White);    // Bright Green for Start
            setButtonStyle(btnStopInstance, Color.IndianRed, Color.White);                  // Red for Stop
            setButtonStyle(btnStopAndDeleteInstance, Color.Firebrick, Color.White);          // Strong Red for Stop & Delete
            setButtonStyle(btnRestartInstance, Color.Goldenrod, Color.White);                // Amber for Restart
            setButtonStyle(btnGetLogs, Color.SteelBlue, Color.White);                      // Blue for Logs
            setButtonStyle(btnManageGroups, Color.FromArgb(18, 140, 126), Color.White);     // Dark Green for Manage Groups

            // Log TextBox styling
            logTextBox.BackColor = Color.FromArgb(32, 44, 51); // Dark background
            logTextBox.ForeColor = Color.WhiteSmoke;          // Light text
            logTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

            // ListView Custom Header Drawing for WhatsApp-like aesthetic
            instanceListView.OwnerDraw = true;
            instanceListView.DrawColumnHeader += (s, e) =>
            {
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) // Using StringFormat for better centering
                using (var brush = new SolidBrush(Color.FromArgb(18, 140, 126))) // Dark Green
                using (var pen = new Pen(Color.FromArgb(12, 100, 90), 1)) // Darker green border
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                    e.Graphics.DrawRectangle(pen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1); // Fix border overdraw
                    TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
            };
            instanceListView.DrawSubItem += (s, e) => e.DrawDefault = true; // Use default for subitems
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
            if (defaultYemen != null)
            {
                comboBoxCountryCode.SelectedItem = defaultYemen;
            }
            else
            {
                comboBoxCountryCode.SelectedIndex = 0; // Fallback to first
            }

            // Attach event handlers for validation
            comboBoxCountryCode.SelectedIndexChanged += (s, e) => EnableLinkButtonBasedOnInput();
            textBoxPhoneNumber.TextChanged += (s, e) => EnableLinkButtonBasedOnInput();
            textBoxApiUsername.TextChanged += (s, e) => EnableLinkButtonBasedOnInput();
            textBoxApiPassword.TextChanged += (s, e) => EnableLinkButtonBasedOnInput();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            _controlWsCts = new CancellationTokenSource();
            _managerControlWs = new ClientWebSocket();

            // Initial connection attempt on load
            await ConnectToManagerControlWs();

            // Only request instance list if connection is open
            if (_managerControlWs.State == WebSocketState.Open)
            {
                await RequestInstanceList();
            }
        }

        private async Task ConnectToManagerControlWs()
        {
            if (_isConnecting) return; // Prevent multiple concurrent connection attempts

            // If already open, or opening, just update UI state and return
            if (_managerControlWs != null && (_managerControlWs.State == WebSocketState.Open || _managerControlWs.State == WebSocketState.Connecting))
            {
                SetUIConnectedState(true); // Assuming it's already connected or will connect
                return;
            }

            _isConnecting = true; // Set flag

            // Dispose previous CTS and socket if they exist from a failed/closed connection
            _controlWsCts?.Cancel();
            _controlWsCts?.Dispose();
            _controlWsCts = new CancellationTokenSource();

            _managerControlWs?.Dispose();
            _managerControlWs = new ClientWebSocket();

            UpdateConnectionStatus("Connecting...", Color.Goldenrod);
            SetUIConnectedState(false); // Disable buttons while connecting

            try
            {
                Console.WriteLine("Connecting to manager control WS...");
                // Add a timeout for the connection attempt
                var connectTask = _managerControlWs.ConnectAsync(new Uri(NodeJsWebSocketUrl), _controlWsCts.Token);

                // Wait for connection or timeout (5 seconds)
                var completedTask = await Task.WhenAny(connectTask, Task.Delay(5000, _controlWsCts.Token));

                if (completedTask == connectTask && connectTask.Status == TaskStatus.RanToCompletion)
                {
                    Console.WriteLine("Connected to manager control WS.");
                    SetUIConnectedState(true); // Update UI for connected state
                    // Start receiving messages (fire and forget)
                    _ = ReceiveManagerControlMessagesAsync(_managerControlWs, _controlWsCts.Token);
                }
                else
                {
                    // Connection failed or timed out
                    _controlWsCts.Cancel(); // Cancel the token to stop connectTask if still running
                    _managerControlWs.Abort(); // Force close the socket
                    throw new WebSocketException("Connection attempt timed out or failed.");
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"Manager Control WS Connection Error: {ex.Message}");
                SetUIConnectedState(false);
                MessageBox.Show($"Error connecting to manager: {ex.Message}. Ensure Node.js manager is running and accessible at {NodeJsWebSocketUrl}.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Manager WS connection attempt was cancelled.");
                SetUIConnectedState(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during manager control connection: {ex.Message}");
                SetUIConnectedState(false);
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isConnecting = false; // Reset flag
            }
        }

        private async Task ReceiveManagerControlMessagesAsync(ClientWebSocket wsClient, CancellationToken token)
        {
            var buffer = new byte[32768];

            try
            {
                while (wsClient.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("Manager Control WS disconnected by server (graceful close).");
                        break; // Exit loop, will fall to finally/catch
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        // Console.WriteLine("Received from Manager Control WS: " + messageJson); // Too noisy
                        ProcessManagerControlMessage(messageJson);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("ReceiveManagerControlMessagesAsync: Task was cancelled.");
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"Manager Control WS error/disconnect: {ex.Message}. Connection lost.");
                this.Invoke((MethodInvoker)delegate
                {
                    SetUIConnectedState(false);
                    if (!token.IsCancellationRequested) // Only show message if not closing app gracefully
                    {
                        MessageBox.Show(
                            $"Connection to the Bot Manager was lost: {ex.Message}\n\nPlease ensure the Node.js Manager application is running and accessible, then restart this UI application if necessary.",
                            "Connection Lost",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in ReceiveManagerControlMessagesAsync: {ex.Message}");
                this.Invoke((MethodInvoker)delegate
                {
                    SetUIConnectedState(false);
                    if (!token.IsCancellationRequested)
                    {
                        MessageBox.Show(
                            $"An unexpected error occurred with the connection: {ex.Message}",
                            "Communication Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                });
            }
            finally
            {
                Console.WriteLine("ReceiveManagerControlMessagesAsync: Exiting receive loop.");
                this.Invoke((MethodInvoker)delegate
                {
                    SetUIConnectedState(false); // Ensure UI reflects disconnected state
                    // If not explicitly cancelled by form closing, trigger a reconnection attempt
                    if (!token.IsCancellationRequested && !_isConnecting) // Avoid multiple reconnect loops
                    {
                        Console.WriteLine("Attempting to reconnect to Manager WS in 3 seconds...");
                        Task.Delay(3000).ContinueWith(t => ConnectToManagerControlWs()); // Fire and forget reconnection
                    }
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
                    string clientId = message["clientId"]?.ToString();

                    // 1. Route to QR display form if active
                    if (_currentQrDisplayForm != null && !_currentQrDisplayForm.IsDisposed)
                    {
                        if (type == "qr" || type == "status")
                        {
                            _currentQrDisplayForm.ProcessManagerMessageForQRDisplay(messageJson);
                            if (_currentQrDisplayForm != null && _currentQrDisplayForm.DialogResult == DialogResult.OK)
                            {
                                _currentQrDisplayForm = null;
                            }
                        }
                    }

                    // 2. Route to Groups display form if active and message is for its client
                    // Check if _activeGroupsForm is not null, not disposed, and its Tag (clientId) matches the message's clientId
                    if (_activeGroupsForm != null && !_activeGroupsForm.IsDisposed && _activeGroupsForm.Tag?.ToString() == clientId)
                    {
                        _activeGroupsForm.ProcessGroupsDisplayMessage(messageJson);
                    }

                    // 3. Process main Form1 specific messages
                    switch (type)
                    {
                        case "instanceList":
                            UpdateInstanceListView(message["instances"] as JArray);
                            break;
                        case "instanceStatusUpdate":
                            UpdateSingleInstanceStatus(message["clientId"]?.ToString(), message["status"]?.ToString(), message["phoneNumber"]?.ToString(), message["name"]?.ToString());
                            break;
                        case "instanceLogs":
                            DisplayInstanceLogs(message["clientId"]?.ToString(), message["logs"] as JArray);
                            break;
                        case "status":
                            // This status refers to the manager's overall state or a specific client's state from manager's perspective
                            // It's mostly handled by QrDisplayForm if active for linking process
                            // For general app status, lblConnectionStatus is updated by SetUIConnectedState
                            Console.WriteLine($"Main Form (Manager Overall Status): Type: {type}, Status: {message["status"]}, Message: {message["message"]}, QR: {message["qr"]}");
                            break;
                        default:
                            // We need to differentiate between internalReply and other unhandled types
                            // internalReply from bot instances will be routed to specific forms (_activeGroupsForm)
                            // If it's not internalReply and not handled, log it.
                            if (type != "internalReply")
                            { // Avoid logging internalReply if not routed to activeForms
                                Console.WriteLine($"Unhandled manager control message type on Main Form: {type}. Full message: {messageJson}");
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing manager control WS message on UI thread: " + ex.Message);
                }
            });
        }

        private void UpdateInstanceListView(JArray instances)
        {
            instanceListView.Items.Clear();
            if (instances == null) return;

            foreach (JObject instance in instances)
            {
                string clientId = instance["clientId"]?.ToString();
                string phoneNumber = instance["phoneNumber"]?.ToString();
                string name = instance["name"]?.ToString();
                string status = instance["status"]?.ToString();

                ListViewItem item = new ListViewItem(clientId);
                item.SubItems.Add(phoneNumber);
                item.SubItems.Add(name);
                item.SubItems.Add(status);
                item.Tag = clientId; // Store ClientID in Tag for easy retrieval
                instanceListView.Items.Add(item);
            }
            instanceListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            instanceListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            // Auto-select the first item if available
            if (instanceListView.Items.Count > 0)
            {
                instanceListView.Items[0].Selected = true;
                instanceListView.Items[0].Focused = true;
            }
        }

        private void UpdateSingleInstanceStatus(string clientId, string status, string phoneNumber = null, string name = null)
        {
            foreach (ListViewItem item in instanceListView.Items)
            {
                if (item.Tag?.ToString() == clientId)
                {
                    item.SubItems[3].Text = status;
                    if (phoneNumber != null) item.SubItems[1].Text = phoneNumber;
                    if (name != null) item.SubItems[2].Text = name;
                    return;
                }
            }
            // If status for an unknown/newly connected instance, refresh list
            Console.WriteLine($"Received status update for instance {clientId}, which was not found in list. Requesting full list refresh.");
            Task.Run(async () => await RequestInstanceList()); // Fire and forget this async call
        }

        private void DisplayInstanceLogs(string clientId, JArray logs)
        {
            logTextBox.Clear();
            if (logs == null || logs.Count == 0)
            {
                logTextBox.AppendText($"--- No logs available for Client: {clientId} ---\r\n");
                return;
            }

            logTextBox.AppendText($"--- Logs for Client: {clientId} ---\r\n");
            foreach (string line in logs)
            {
                logTextBox.AppendText(line + "\r\n");
            }
            logTextBox.AppendText($"--- End of Logs for Client: {clientId} ---\r\n");
            logTextBox.ScrollToCaret();
        }

        private async Task SendManagerCommand(string type, string clientId = null, string apiUsername = null, string apiPassword = null, string ownerNumber = null, string groupId = null)
        {
            if (_managerControlWs == null || _managerControlWs.State != WebSocketState.Open)
            {
                MessageBox.Show("Not connected to manager. Please ensure Node.js manager is running and restart the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var request = new JObject();
            request["type"] = type;
            if (clientId != null) request["clientId"] = clientId;
            if (apiUsername != null) request["apiUsername"] = apiUsername;
            if (apiPassword != null) request["apiPassword"] = apiPassword;
            if (ownerNumber != null) request["ownerNumber"] = ownerNumber;
            if (groupId != null) request["groupId"] = groupId; // Added for group operations

            var buffer = Encoding.UTF8.GetBytes(request.ToString());
            try
            {
                await _managerControlWs.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _controlWsCts.Token);
                Console.WriteLine($"Sent command: {type} for Client: {clientId ?? "N/A"}, Group: {groupId ?? "N/A"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending command {type}: {ex.Message}");
                MessageBox.Show($"Failed to send command: {ex.Message}", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RequestInstanceList()
        {
            await SendManagerCommand("listInstances");
        }

        private void EnableLinkButtonBasedOnInput()
        {
            bool isApiUsernameFilled = !string.IsNullOrWhiteSpace(textBoxApiUsername.Text);
            bool isApiPasswordFilled = !string.IsNullOrWhiteSpace(textBoxApiPassword.Text);
            bool isOwnerNumberFilled = !string.IsNullOrWhiteSpace(textBoxPhoneNumber.Text);
            bool isCountryCodeSelected = (comboBoxCountryCode.SelectedItem != null);

            linkWhatsappButton.Enabled = isApiUsernameFilled && isApiPasswordFilled && isOwnerNumberFilled && isCountryCodeSelected;
        }

        private void InstanceListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable/disable the "Manage Groups" button based on selection
            btnManageGroups.Enabled = instanceListView.SelectedItems.Count > 0;

            // Clear logs when selection changes, and potentially fetch new logs
            if (instanceListView.SelectedItems.Count > 0)
            {
                logTextBox.Clear(); // Clear current logs
                // Auto-fetch logs for newly selected instance (optional, but convenient)
                // string clientId = instanceListView.SelectedItems[0].Tag.ToString();
                // Task.Run(async () => await SendManagerCommand("getLogs", clientId));
            }
            SetUIConnectedState(_managerControlWs?.State == WebSocketState.Open); // Re-evaluate button states on selection change
        }


        // --- Event Handlers for UI Buttons ---
        private async void linkWhatsappButton_Click(object sender, EventArgs e)
        {
            // Double check validation before proceeding
            if (!linkWhatsappButton.Enabled) return;

            string apiUsername = textBoxApiUsername.Text.Trim();
            string apiPassword = textBoxApiPassword.Text.Trim();
            string selectedCountryCode = comboBoxCountryCode.SelectedItem?.ToString().Split(' ')[0]?.Replace("+", "").Trim();
            string ownerPhoneNumberPart = textBoxPhoneNumber.Text.Trim();
            string ownerNumber = $"{selectedCountryCode}{ownerPhoneNumberPart}";

            _currentQrDisplayForm = new QrDisplayForm(_managerControlWs, _controlWsCts, apiUsername, apiPassword, ownerNumber);
            _currentQrDisplayForm.ClientLinked += QrForm_ClientLinked; // Subscribe to success event

            _currentQrDisplayForm.ShowDialog(this); // Show modal dialog

            // _currentQrDisplayForm will be nulled by ProcessManagerControlMessage if successful
            // Or here if user closes it manually / or failure in QrDisplayForm itself
            _currentQrDisplayForm = null;

            await RequestInstanceList(); // Refresh list regardless of QR process outcome
        }

        private void QrForm_ClientLinked(string clientId, string phoneNumber, string clientName)
        {
            // This event handler runs when QrDisplayForm successfully links.
            // The QrDisplayForm itself closes, so no need to manage _currentQrDisplayForm here.
            MessageBox.Show($"Client successfully linked!\nID: {clientId}\nPhone: {phoneNumber}\nName: {clientName}", "WhatsApp Linked", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Request list will happen after dialog is closed in linkWhatsappButton_Click
        }

        private async void btnRefreshInstances_Click(object sender, EventArgs e)
        {
            await RequestInstanceList();
        }

        private async void btnStartInstance_Click(object sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0)
            {
                string clientId = instanceListView.SelectedItems[0].Tag.ToString();
                await SendManagerCommand("startInstance", clientId);
            }
            else { MessageBox.Show("Please select an instance from the list.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        private async void btnStopInstance_Click(object sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0)
            {
                string clientId = instanceListView.SelectedItems[0].Tag.ToString();
                await SendManagerCommand("stopInstance", clientId);
            }
            else { MessageBox.Show("Please select an instance from the list.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        private async void btnStopAndDeleteInstance_Click(object sender, EventArgs e) // NEW
        {
            if (instanceListView.SelectedItems.Count > 0)
            {
                string clientId = instanceListView.SelectedItems[0].Tag.ToString();
                string clientDisplayName = instanceListView.SelectedItems[0].SubItems[2].Text; // Get client name
                string clientPhoneNumber = instanceListView.SelectedItems[0].SubItems[1].Text; // Get client phone

                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to STOP and DELETE instance {clientDisplayName} ({clientPhoneNumber})?\nThis action will permanently remove all associated data and sessions.\nThis action cannot be undone.",
                    "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirmResult == DialogResult.Yes)
                {
                    await SendManagerCommand("deleteInstance", clientId);
                    // UI will update when "deleted" status comes back, or after a list refresh.
                    // Optimistically remove from UI, though server might re-add if not fully deleted.
                    instanceListView.SelectedItems[0].Remove();
                    logTextBox.AppendText($"Requested deletion for client {clientId}. Data cleanup will occur on server.\r\n");
                    // Ensure auto-selection happens after removal, if possible
                    if (instanceListView.Items.Count > 0) instanceListView.Items[0].Selected = true;
                }
            }
            else { MessageBox.Show("Please select an instance from the list.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }


        private async void btnRestartInstance_Click(object sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0)
            {
                string clientId = instanceListView.SelectedItems[0].Tag.ToString();
                await SendManagerCommand("restartInstance", clientId);
            }
            else { MessageBox.Show("Please select an instance from the list.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        private async void btnGetLogs_Click(object sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0)
            {
                string clientId = instanceListView.SelectedItems[0].Tag.ToString();
                logTextBox.Clear();
                logTextBox.AppendText($"Requesting logs for {clientId}...\r\n");
                await SendManagerCommand("getLogs", clientId);
            }
            else { MessageBox.Show("Please select an instance from the list to get logs.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        private async void btnManageGroups_Click(object sender, EventArgs e) // NEW
        {
            if (instanceListView.SelectedItems.Count > 0)
            {
                string clientId = instanceListView.SelectedItems[0].Tag.ToString();
                string clientPhoneNumber = instanceListView.SelectedItems[0].SubItems[1].Text; // Use the Phone number shown in UI

                // Pass the current WebSocket client, CancellationTokenSource, and clientId to the new form
                _activeGroupsForm = new GroupsDisplayForm(_managerControlWs, _controlWsCts, clientId, clientPhoneNumber);
                // The GroupsDisplayForm itself will filter messages by client ID and fetch groups.
                // It also uses the client ID in its Tag property for routing messages from manager.
                _activeGroupsForm.Tag = clientId; // Important for message routing in ProcessManagerControlMessage

                _activeGroupsForm.ShowDialog(this); // Show modal dialog

                _activeGroupsForm = null; // Clear reference when dialog closes

                await RequestInstanceList(); // Refresh main list when groups form closes
            }
            else
            {
                MessageBox.Show("Please select an active instance to manage its groups.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_controlWsCts != null)
            {
                _controlWsCts.Cancel(); // Signal cancellation to async loops
                _controlWsCts.Dispose();
            }

            // Gracefully close WebSocket
            // Use a separate task and await it with a timeout, without blocking UI thread too long
            Task.Run(async () =>
            {
                if (_managerControlWs != null && _managerControlWs.State == WebSocketState.Open)
                {
                    try
                    {
                        var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                        await _managerControlWs.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing application", closeCts.Token);
                    }
                    catch (OperationCanceledException) { Console.WriteLine("WebSocket close timed out during form closing."); }
                    catch (Exception ex) { Console.WriteLine($"Error during WebSocket close on form closing: {ex.Message}"); }
                }
            }).Wait(2500); // Wait a bit for the async close to attempt completion

            _managerControlWs?.Dispose(); // Ensure disposal happens
        }
          private void UpdateConnectionStatus(string message, Color color)
        {
            // This check ensures we're on the UI thread before updating UI elements.
            // InvokeRequired is true if the current thread is not the one that created the control.
            if (lblConnectionStatus.InvokeRequired)
            {
                lblConnectionStatus.Invoke((MethodInvoker)delegate {
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
            // Enable/disable buttons based on connection state
            // linkWhatsappButton's enabled state also depends on IsCredentialFieldsValid()
            linkWhatsappButton.Enabled = connected && IsCredentialFieldsValid(); 
            btnRefreshInstances.Enabled = connected;
            
            // These buttons also depend on list selection, so combine conditions
            bool hasSelection = instanceListView.SelectedItems.Count > 0;

            btnStartInstance.Enabled = connected && hasSelection;
            btnStopInstance.Enabled = connected && hasSelection;
            btnStopAndDeleteInstance.Enabled = connected && hasSelection;
            btnRestartInstance.Enabled = connected && hasSelection;
            btnGetLogs.Enabled = connected && hasSelection;
            btnManageGroups.Enabled = connected && hasSelection;

            if (connected)
            {
                UpdateConnectionStatus("Connected to Manager", Color.Green);
            }
            else
            {
                UpdateConnectionStatus("Disconnected from Manager", Color.Red);
            }
        }

        private bool IsCredentialFieldsValid()
        {
            bool isApiUsernameFilled = !string.IsNullOrWhiteSpace(textBoxApiUsername.Text);
            bool isApiPasswordFilled = !string.IsNullOrWhiteSpace(textBoxApiPassword.Text);
            bool isOwnerNumberFilled = !string.IsNullOrWhiteSpace(textBoxPhoneNumber.Text);
            bool isCountryCodeSelected = (comboBoxCountryCode.SelectedItem != null);
            
            return isApiUsernameFilled && isApiPasswordFilled && isOwnerNumberFilled && isCountryCodeSelected;
        }
    }
    
}