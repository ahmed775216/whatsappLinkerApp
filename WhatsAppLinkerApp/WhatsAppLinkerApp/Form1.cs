using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.VisualBasic;
using WhatsAppLinkerApp.Models;
using WhatsAppLinkerApp.Database;

namespace WhatsAppLinkerApp
{
    public class AppSettings
    {
        public string? ApiUsername { get; set; }
        public string? EncryptedApiPassword { get; set; }
        public string? OwnerCountryCodeSelectedItem { get; set; }
        public string? OwnerPhoneNumber { get; set; }
        public string? LastManagedClientId { get; set; }
    }

    public partial class Form1 : Form
    {
        private readonly DatabaseConnection _db = new DatabaseConnection();
        private ClientWebSocket? _managerControlWs;
        private CancellationTokenSource? _controlWsCts;
        private Task? _readMessageTask;
        private CancellationTokenSource? _reconnectCts; // For controlling the reconnection loop
        private Task? _reconnectTask; // To hold the reconnection loop task
        private QrDisplayForm? _currentQrDisplayForm;
        private GroupsDisplayForm? _activeGroupsForm;

        private InstanceStatusInfo? _managedInstance;
        private string? _managedInstanceClientId;

        public Form1()
        {
            InitializeComponent();
            ApplyFormStyles();
            InitializeCountryCodeComboBox();
            AttachEventHandlers(); // This will now correctly attach to the original method names
            SetUIConnectedState(false);
        }

        private void AttachEventHandlers()
        {
            // IMPORTANT: Unsubscribe first to prevent duplicate subscriptions
            linkWhatsappButton.Click -= linkWhatsappButton_Click;
            btnManageGroups.Click -= btnManageGroups_Click;
            btnClearCacheAndLogs.Click -= btnClearCacheAndLogs_Click;
            instanceListView.SelectedIndexChanged -= instanceListView_SelectedIndexChanged;
            instanceContextMenuStrip.Opening -= instanceContextMenuStrip_Opening;
            setAliasToolStripMenuItem.Click -= setAliasToolStripMenuItem_Click;
            btnRefreshInstances.Click -= btnRefreshInstances_Click;
            btnStartInstance.Click -= btnStartInstance_Click;
            btnStopInstance.Click -= btnStopInstance_Click;
            btnRestartInstance.Click -= btnRestartInstance_Click;
            btnStopAndDeleteInstance.Click -= btnStopAndDeleteInstance_Click;
            btnGetLogs.Click -= btnGetLogs_Click;
            textBoxApiUsername.TextChanged -= (s, e) => UpdateLinkAndActionButtonsState();
            textBoxApiPassword.TextChanged -= (s, e) => UpdateLinkAndActionButtonsState();
            comboBoxCountryCode.SelectedIndexChanged -= (s, e) => UpdateLinkAndActionButtonsState();
            textBoxPhoneNumber.TextChanged -= (s, e) => UpdateLinkAndActionButtonsState();

            // Then subscribe
            linkWhatsappButton.Click += linkWhatsappButton_Click;
            btnManageGroups.Click += btnManageGroups_Click;
            btnClearCacheAndLogs.Click += btnClearCacheAndLogs_Click;
            instanceListView.SelectedIndexChanged += instanceListView_SelectedIndexChanged;
            instanceContextMenuStrip.Opening += instanceContextMenuStrip_Opening;
            setAliasToolStripMenuItem.Click += setAliasToolStripMenuItem_Click;
            btnRefreshInstances.Click += btnRefreshInstances_Click;
            btnStartInstance.Click += btnStartInstance_Click;
            btnStopInstance.Click += btnStopInstance_Click;
            btnRestartInstance.Click += btnRestartInstance_Click;
            btnStopAndDeleteInstance.Click += btnStopAndDeleteInstance_Click;
            btnGetLogs.Click += btnGetLogs_Click;
            textBoxApiUsername.TextChanged += (s, e) => UpdateLinkAndActionButtonsState();
            textBoxApiPassword.TextChanged += (s, e) => UpdateLinkAndActionButtonsState();
            comboBoxCountryCode.SelectedIndexChanged += (s, e) => UpdateLinkAndActionButtonsState();
            textBoxPhoneNumber.TextChanged += (s, e) => UpdateLinkAndActionButtonsState();
        }

        private async void linkWhatsappButton_Click(object? sender, EventArgs e)
        {
            // Immediately disable the button to prevent double-clicks
            linkWhatsappButton.Enabled = false;

            try
            {
                // Check if QR form is already active
                if (_currentQrDisplayForm != null && !_currentQrDisplayForm.IsDisposed)
                {
                    _currentQrDisplayForm.Activate();
                    return;
                }

                bool isInstanceCurrentlyManagedAndValid = _managedInstance != null &&
                                                       !string.IsNullOrEmpty(_managedInstanceClientId) &&
                                                       _managedInstance.Status != "deleted" &&
                                                       _managedInstance.Status != "logged_out" &&
                                                       _managedInstance.Status != "error_whatsapp_permanent" &&
                                                       _managedInstance.Status != "not_found_in_list" &&
                                                       !(_managedInstance.Status?.StartsWith("exited") ?? false) &&
                                                       _managedInstance.Status != "error_spawning";

                if (isInstanceCurrentlyManagedAndValid)
                {
                    if (MessageBox.Show($"Are you sure you want to unlink and delete the current account ({FormatDisplayInfo(_managedInstance, true)})? This will stop and delete its data from the manager.", "Confirm Unlink", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        AppendLogMessage($"User initiated unlink/delete for instance: {_managedInstanceClientId}", "info");
                        await SendManagerCommand("deleteInstance", _managedInstanceClientId);
                    }
                }
                else
                {
                    string apiUsername = textBoxApiUsername.Text.Trim();
                    string apiPassword = textBoxApiPassword.Text.Trim();
                    string ownerNumber = GetFullOwnerNumber();

                    if (string.IsNullOrWhiteSpace(apiUsername) || string.IsNullOrWhiteSpace(apiPassword) || string.IsNullOrWhiteSpace(ownerNumber))
                    {
                        MessageBox.Show("Please fill all API and Owner Number fields to link an account.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (_managerControlWs == null || _managerControlWs.State != WebSocketState.Open || _controlWsCts == null)
                    {
                        MessageBox.Show("Not connected to manager.", "Connection Error");
                        SetUIConnectedState(false);
                        return;
                    }

                    AppendLogMessage("Initiating new WhatsApp link process...", "info");
                    try
                    {
                        _currentQrDisplayForm = new QrDisplayForm(_managerControlWs, _controlWsCts, apiUsername, apiPassword, ownerNumber);
                        _currentQrDisplayForm.ClientLinked += OnNewClientSuccessfullyLinked;
                        _currentQrDisplayForm.FormClosed += QrForm_FormClosed;
                        _currentQrDisplayForm.Show(this);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Linking Error: {ex.Message}");
                        Console.WriteLine($"[LINKING_ERROR] {ex}");
                        AppendLogMessage($"Error initiating QR display: {ex.Message}", "error");
                        _currentQrDisplayForm = null;
                    }
                }
            }
            finally
            {
                // Re-enable button state will be handled by UpdateLinkAndActionButtonsState
                UpdateLinkAndActionButtonsState();
            }
        }
        private void ApplyFormStyles()
        {
            foreach (var button in new Button[] { linkWhatsappButton, btnManageGroups, btnClearCacheAndLogs,
                                                   btnRefreshInstances, btnStartInstance, btnStopInstance,
                                                   btnRestartInstance, btnStopAndDeleteInstance, btnGetLogs })
            {
                button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                button.ForeColor = Color.White;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
            }
            linkWhatsappButton.BackColor = Color.FromArgb(37, 211, 102);
            btnManageGroups.BackColor = Color.FromArgb(18, 140, 126);
            btnClearCacheAndLogs.BackColor = Color.FromArgb(47, 79, 79);
            btnRefreshInstances.BackColor = Color.FromArgb(18, 140, 126);
            btnStartInstance.BackColor = Color.FromArgb(37, 211, 102);
            btnStopInstance.BackColor = Color.FromArgb(205, 92, 92);
            btnRestartInstance.BackColor = Color.FromArgb(218, 165, 32);
            btnStopAndDeleteInstance.BackColor = Color.FromArgb(178, 34, 34);
            btnGetLogs.BackColor = Color.FromArgb(70, 130, 180);

            instanceListView.OwnerDraw = true;
            instanceListView.FullRowSelect = true;
            instanceListView.GridLines = true;
            instanceListView.HideSelection = false;
        }

        private void instanceListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var headerBrush = new SolidBrush(Color.FromArgb(7, 94, 84)))
            using (var borderPen = new Pen(Color.FromArgb(5, 70, 60), 1))
            {
                e.Graphics.FillRectangle(headerBrush, e.Bounds);
                e.Graphics.DrawRectangle(borderPen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                TextRenderer.DrawText(e.Graphics, e.Header?.Text ?? "", e.Font, e.Bounds, Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            }
        }
        private void instanceListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            if (e.Item?.Tag is string itemClientId && itemClientId == _managedInstanceClientId && _managedInstance != null)
            {
                if (e.ColumnIndex == colStatus.Index)
                {
                    e.DrawDefault = false;
                    Color statusColor = GetColorForStatus(_managedInstance.Status);
                    Brush textBrush;
                    Brush backgroundBrush;

                    if (e.Item.Selected)
                    {
                        backgroundBrush = SystemBrushes.Highlight;
                        textBrush = SystemBrushes.HighlightText;
                    }
                    else
                    {
                        backgroundBrush = new SolidBrush(e.Item.BackColor);
                        textBrush = new SolidBrush(statusColor);
                    }

                    e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
                    TextRenderer.DrawText(e.Graphics, e.SubItem?.Text ?? "", e.SubItem.Font, e.Bounds, (textBrush as SolidBrush)?.Color ?? statusColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

                    if (backgroundBrush != SystemBrushes.Highlight && backgroundBrush != null) backgroundBrush.Dispose();
                    if (textBrush != SystemBrushes.HighlightText && textBrush is IDisposable d) d.Dispose();
                    return;
                }
            }
            e.DrawDefault = true;
        }


        private Color GetColorForStatus(string? status)
        {
            switch (status?.ToLower())
            {
                case "connected": return Color.DarkGreen;
                case "disconnected_logout":
                case "stopped":
                case "error":
                case "deleted_by_manager":
                case "not_found_in_list":
                case "error_spawning":
                case "error_whatsapp_permanent":
                    return Color.Red;
                case var s when s != null && s.StartsWith("exited"):
                    return Color.Red;
                case "qr_received":
                case "linking_qr":
                case "connecting_whatsapp":
                case "restarting_stopping":
                case "restarting":
                case "pending_api_processing":
                    return Color.Orange;
                default: return SystemColors.ControlText;
            }
        }

        private void InitializeCountryCodeComboBox()
        {
            var countryCodesDisplay = new List<string>
            {
                "🇾🇪 +967", "🇸🇦 +966", "🇦🇪 +971", "🇪🇬 +20", "🇧🇭 +973", "🇶🇦 +974",
                "🇰🇼 +965", "🇴🇲 +968", "🇺🇸 +1", "🇬🇧 +44", "🇩🇪 +49", "🇫🇷 +33"
            };
            comboBoxCountryCode.Items.Clear();
            comboBoxCountryCode.Items.AddRange(countryCodesDisplay.ToArray());
            var defaultYemenDisplay = countryCodesDisplay.FirstOrDefault(c => c.Contains("+967"));
            if (defaultYemenDisplay != null) comboBoxCountryCode.SelectedItem = defaultYemenDisplay;
            else if (comboBoxCountryCode.Items.Count > 0) comboBoxCountryCode.SelectedIndex = 0;
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            AppendLogMessage("=== FORM LOAD STARTED ===", "debug");

            try
            {
                AppendLogMessage("Loading app settings from JSON...", "debug");
                LoadAppSettings();
                AppendLogMessage($"Loaded settings - ManagedInstanceClientId: {_managedInstanceClientId ?? "NULL"}", "debug");

                // Add delay to ensure form is fully loaded
                await Task.Delay(500);

                AppendLogMessage("Attempting to load instance from database...", "debug");
                await LoadManagedInstanceFromDatabase();

                AppendLogMessage("Initializing WebSocket connection...", "debug");
                _controlWsCts = new CancellationTokenSource();
                await ConnectToManagerControlWs();

                AppendLogMessage("=== FORM LOAD COMPLETED ===", "debug");
            }
            catch (Exception ex)
            {
                AppendLogMessage($"CRITICAL ERROR in Form_Load: {ex.Message}\nStack: {ex.StackTrace}", "error");
            }
        }

        private async Task LoadManagedInstanceFromDatabase()
        {
            AppendLogMessage($"[DB_LOAD] Starting LoadManagedInstanceFromDatabase...", "debug");

            try
            {
                if (string.IsNullOrEmpty(_managedInstanceClientId))
                {
                    AppendLogMessage("[DB_LOAD] No previous managed instance ID found in settings", "info");
                    return;
                }

                AppendLogMessage($"[DB_LOAD] Attempting to load instance: {_managedInstanceClientId}", "debug");

                // Test database connection first
                try
                {
                    AppendLogMessage("[DB_LOAD] Testing database connection...", "debug");
                    var testQuery = "SELECT 1";
                    var testResult = await _db.QuerySingleAsync<int>(testQuery);
                    AppendLogMessage($"[DB_LOAD] Database connection test successful: {testResult}", "debug");
                }
                catch (Exception dbTestEx)
                {
                    AppendLogMessage($"[DB_LOAD] Database connection test FAILED: {dbTestEx.Message}", "error");
                    // Keep the instance ID for later retry
                    return;
                }

                // Now try to load the instance
                var query = @"
            SELECT client_id, phone_number, display_name, status 
            FROM bot_instances 
            WHERE client_id = @clientId";

                AppendLogMessage($"[DB_LOAD] Executing query for client_id: {_managedInstanceClientId}", "debug");

                var instance = await _db.QuerySingleAsync<dynamic>(query, new { clientId = _managedInstanceClientId });

                if (instance != null)
                {
                    AppendLogMessage($"[DB_LOAD] Instance found in database: {instance.client_id}", "debug");

                    _managedInstance = new InstanceStatusInfo
                    {
                        ClientId = instance.client_id,
                        PhoneNumber = instance.phone_number,
                        Alias = instance.display_name ?? instance.phone_number,
                        Status = instance.status
                    };

                    AppendLogMessage($"[DB_LOAD] Created InstanceStatusInfo - Status: {_managedInstance.Status}, Phone: {_managedInstance.PhoneNumber}", "debug");

                    UpdateListViewForSingleInstance();
                    AppendLogMessage($"[DB_LOAD] Successfully loaded managed instance: {FormatDisplayInfo(_managedInstance)}", "info");

                    // Don't save settings here as it might cause recursion
                }
                else
                {
                    AppendLogMessage($"[DB_LOAD] Instance {_managedInstanceClientId} NOT found in database", "warn");
                    // Don't clear the ID - it might be valid but not in DB yet
                }
            }
            catch (Exception ex)
            {
                AppendLogMessage($"[DB_LOAD] ERROR: {ex.GetType().Name}: {ex.Message}", "error");
                AppendLogMessage($"[DB_LOAD] Stack trace: {ex.StackTrace}", "debug");

                // Don't clear the instance ID on error
                AppendLogMessage($"[DB_LOAD] Keeping instance ID {_managedInstanceClientId} for retry", "warn");
            }
        }

        // Add this method to Form1.cs:
        private async Task ConnectToManagerControlWs()
        {
            if (_managerControlWs != null && (_managerControlWs.State != WebSocketState.Closed && _managerControlWs.State != WebSocketState.None))
            {
                try
                {
                    if (_controlWsCts != null && !_controlWsCts.IsCancellationRequested) _controlWsCts.Cancel();
                    _managerControlWs.Abort();
                }
                catch { /* ignore */ }
            }
            _managerControlWs?.Dispose();
            _managerControlWs = new ClientWebSocket();

            _controlWsCts?.Dispose();
            _controlWsCts = new CancellationTokenSource();

            SetUIConnectedState(false, "Connecting to manager...");
            try
            {
                await _managerControlWs.ConnectAsync(new Uri("ws://localhost:8088"), _controlWsCts.Token);
                SetUIConnectedState(true, "Connected. Waiting for initial state...");

                // Request instance list immediately after connection
                await SendManagerCommand("listInstances");

                // Retry loading from database after connection
                if (_managedInstance == null && !string.IsNullOrEmpty(_managedInstanceClientId))
                {
                    AppendLogMessage("[WS] Retrying instance load after WebSocket connection...", "debug");
                    await LoadManagedInstanceFromDatabase();
                }

                if (_readMessageTask != null && !_readMessageTask.IsCompleted)
                {
                    try { await _readMessageTask; } catch (OperationCanceledException) { } catch { /* ignore other */ }
                }
                _readMessageTask = Task.Run(() => ReadMessagesFromManager(), _controlWsCts.Token);
            }
            catch (OperationCanceledException)
            {
                SetUIConnectedState(false, "Connection attempt cancelled.");
                Console.WriteLine("[WS_INFO] Connection attempt cancelled.");
            }
            catch (Exception ex)
            {
                SetUIConnectedState(false, $"An unexpected error occurred: {ex.Message}");
                Console.WriteLine($"[ERROR] Unexpected error connecting: {ex.Message}");
            }
            finally
            {
                UpdateLinkAndActionButtonsState();
            }
        }
      

        private async Task ReadMessagesFromManager()
        {
            if (_managerControlWs == null || _controlWsCts == null) return;
            var buffer = new byte[8192 * 2];
            List<byte> messageBytes = new List<byte>();
            try
            {
                while (_managerControlWs.State == WebSocketState.Open && !_controlWsCts.IsCancellationRequested)
                {
                    WebSocketReceiveResult result;
                    do // Loop to receive all fragments of a single message
                    {
                        result = await _managerControlWs.ReceiveAsync(new ArraySegment<byte>(buffer), _controlWsCts.Token);

                        messageBytes.AddRange(buffer.Take(result.Count)); // Add received bytes to our reassembly list

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Console.WriteLine("[WS_INFO] Manager requested WebSocket close during message read.");
                            return; // Exit the loop and task if manager closes connection
                        }

                    } while (!result.EndOfMessage && !_controlWsCts.IsCancellationRequested); // Continue until EndOfMessage is true or cancellation

                    // Only process if it's a text message and we have accumulated data
                    if (result.MessageType == WebSocketMessageType.Text && messageBytes.Count > 0)
                    {
                        try
                        {
                            var message = Encoding.UTF8.GetString(messageBytes.ToArray());
                            ProcessManagerControlMessage(message);
                        }
                        catch (Exception ex)
                        {
                            // Catch errors during message processing/JSON parsing here
                            Console.Error.WriteLine($"[WS_ERROR] Error processing received WebSocket message: {ex.Message}. Raw: {Encoding.UTF8.GetString(messageBytes.ToArray()).Substring(0, Math.Min(500, messageBytes.Count))}");
                            // Continue to next message, but log the error
                        }
                    }
                    // Clear for next message
                    messageBytes.Clear();

                }
            }
            catch (OperationCanceledException) { Console.WriteLine("[WS_INFO] Reading messages cancelled."); }
            catch (WebSocketException wse)
            {
                // Check for specific close codes (e.g., 1000 Normal Closure, 1001 Going Away)
                // Only log critical errors if not a normal closure/going away
                if (wse.WebSocketErrorCode != WebSocketError.ConnectionClosedPrematurely &&
                    wse.Message.Contains("aborted") == false && wse.Message.Contains("closed") == false &&
                    _managerControlWs?.State != WebSocketState.Closed && _managerControlWs?.State != WebSocketState.Aborted)
                {
                    Console.Error.WriteLine($"[WS_ERROR] WebSocket connection error during read: {wse.Message}");
                }
                else
                {
                    Console.WriteLine($"[WS_INFO] WebSocket connection gracefully closed during read or cancelled: {wse.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] Unexpected error in ReadMessagesFromManager: {ex.Message}", ex); // Include exception for full stack trace
            }
            finally
            {
                // Important: Ensure connection is handled gracefully here.
                // If the while loop exits because connection is no longer open, trigger disconnected state.
                if (_managerControlWs?.State == WebSocketState.Open ||
                    _managerControlWs?.State == WebSocketState.CloseReceived ||
                    _managerControlWs?.State == WebSocketState.CloseSent)
                {
                    try { await _managerControlWs.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing read loop", new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token); }
                    catch (Exception closeEx) { Console.Error.WriteLine($"[WS_ERROR] Exception during WebSocket close in ReadMessages: {closeEx.Message}"); }
                }
                SetUIConnectedState(false, "Disconnected from manager.");
            }
        }


        private async Task SendManagerCommand(string type, string? clientId = null, string? apiUsername = null, string? apiPassword = null, string? ownerNumber = null, JObject? additionalData = null)
        {
            if (_managerControlWs == null || _managerControlWs.State != WebSocketState.Open || _controlWsCts == null || _controlWsCts.IsCancellationRequested)
            {
                return;
            }
            var request = additionalData ?? new JObject();
            request["type"] = type;

            if (clientId == null && !string.IsNullOrEmpty(_managedInstanceClientId) && type != "requestQr" && type != "manualRelink" && type != "listInstances")
            {
                request["clientId"] = _managedInstanceClientId;
            }
            else if (clientId != null)
            {
                request["clientId"] = clientId;
            }

            if (apiUsername != null) request["apiUsername"] = apiUsername;
            if (apiPassword != null) request["apiPassword"] = apiPassword;
            if (ownerNumber != null) request["ownerNumber"] = ownerNumber;
            try
            {
                var buffer = Encoding.UTF8.GetBytes(request.ToString());
                if (!_controlWsCts.IsCancellationRequested)
                {
                    await _managerControlWs.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _controlWsCts.Token);
                    Console.WriteLine($"[UI] Sent command: {type} for {request["clientId"]?.ToString() ?? "Manager/Global"}");
                }
            }
            catch (OperationCanceledException) { Console.WriteLine("[UI_WARN] Command send cancelled."); }
            catch (WebSocketException wse)
            {
                Console.WriteLine($"[UI_ERROR] WebSocket error sending command: {wse.Message}");
                SetUIConnectedState(false, $"Connection lost: {wse.Message}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UI_ERROR] Error sending command: {ex.Message}");
            }
        }
        // Add this method to Form1.cs
        private void ApplyInstanceStatusColor(ListViewItem item, string? status) // Made status nullable
        {
            if (item == null) return; // Guard clause

            item.ForeColor = GetColorForStatus(status); // Use the existing GetColorForStatus
        }
        private void ProcessManagerControlMessage(string messageJson)
        {
            if (this.InvokeRequired) { this.Invoke((MethodInvoker)delegate { ProcessManagerControlMessage(messageJson); }); return; }
            try
            {
                JObject message = JObject.Parse(messageJson);
                string? type = message["type"]?.ToString();
                string? msgClientId = message["clientId"]?.ToString();

                // Log all incoming messages for debugging
                Console.WriteLine($"[FORM1] Received message - Type: {type}, ClientId: {msgClientId}");

                // Route messages to QR form if it's active
                if (_currentQrDisplayForm != null && !_currentQrDisplayForm.IsDisposed)
                {
                    // Route QR messages and relevant status messages
                    if (type == "qr")
                    {
                        // Always route QR messages to the active QR form
                        _currentQrDisplayForm.ProcessRelayedManagerMessage(messageJson);
                        return; // Don't process further
                    }
                    else if (type == "status" || type == "error")
                    {
                        // Check if this is a linking-related status
                        string? status = message["status"]?.ToString();
                        bool isLinkingRelated = status == "linking_in_progress" ||
                                               status == "linking_qr" ||
                                               status == "qr_received" ||
                                               status == "qr" || // Sometimes QR comes in status
                                               status == "connecting_whatsapp" ||
                                               status == "connected" ||
                                               status == "error" ||
                                               status == "linking_failed";

                        // Also check if the message contains a QR code
                        bool hasQr = message["qr"] != null && !string.IsNullOrEmpty(message["qr"]?.ToString());

                        if ((type == "status" && (isLinkingRelated || hasQr)) || type == "error")
                        {
                            _currentQrDisplayForm.ProcessRelayedManagerMessage(messageJson);

                            // If it's not a terminal status, continue processing
                            if (status != "connected" && status != "error" && status != "linking_failed")
                            {
                                return;
                            }
                        }
                    }
                }

                // Process the message in Form1 as well
                switch (type)
                {
                    case "status":
                        string? managerStatus = message["status"]?.ToString();
                        string? managerMessage = message["message"]?.ToString();
                        string? linkingClientIdFromStatus = message["clientId"]?.ToString();

                        lblConnectionStatus.Text = $"Manager: {managerMessage ?? managerStatus ?? "Unknown"}";
                        lblConnectionStatus.ForeColor = _managerControlWs?.State == WebSocketState.Open ? Color.DarkGreen : Color.Red;

                        if (managerStatus == "connected" && !string.IsNullOrEmpty(linkingClientIdFromStatus))
                        {
                            if (string.IsNullOrEmpty(_managedInstanceClientId) || _managedInstanceClientId == linkingClientIdFromStatus)
                            {
                                _managedInstanceClientId = linkingClientIdFromStatus;
                                _managedInstance = new InstanceStatusInfo
                                {
                                    ClientId = linkingClientIdFromStatus,
                                    Status = "connected",
                                    PhoneNumber = message["phoneNumber"]?.ToString(),
                                    Alias = message["name"]?.ToString() ?? message["phoneNumber"]?.ToString()?.Split('@')[0]
                                };
                                AppendLogMessage($"Successfully linked and now managing: {FormatDisplayInfo(_managedInstance)}", "info");
                                UpdateListViewForSingleInstance();
                                SaveAppSettings();
                            }
                        }
                        break;

                    case "instanceStatusUpdate":
                        string? instanceStatus = message["status"]?.ToString();
                        if (!string.IsNullOrEmpty(msgClientId) && msgClientId == _managedInstanceClientId)
                        {
                            _managedInstance ??= new InstanceStatusInfo { ClientId = msgClientId };
                            _managedInstance.Status = instanceStatus ?? _managedInstance.Status;
                            _managedInstance.PhoneNumber = message["phoneNumber"]?.ToString() ?? _managedInstance.PhoneNumber;
                            _managedInstance.Alias = message["name"]?.ToString() ?? _managedInstance.Alias;
                            UpdateListViewForSingleInstance();

                            if (instanceStatus == "disconnected_logout" || instanceStatus == "deleted" || instanceStatus == "error_whatsapp_permanent" || instanceStatus?.StartsWith("exited") == true || instanceStatus == "error_spawning")
                            {
                                HandleManagedInstanceBecameUnusable(msgClientId, instanceStatus);
                            }
                        }
                        break;

                    // Update ProcessManagerControlMessage to log instance updates
                    case "instancesList":
                        AppendLogMessage($"[MSG] Received instancesList", "debug");
                        JArray? instancesArray = message["instances"] as JArray;
                        if (instancesArray != null)
                        {
                            AppendLogMessage($"[MSG] Instance list contains {instancesArray.Count} instances", "debug");

                            // Log all instances for debugging
                            foreach (var inst in instancesArray)
                            {
                                AppendLogMessage($"[MSG] Instance in list: {inst["clientId"]} - Status: {inst["status"]}", "debug");
                            }

                            if (!string.IsNullOrEmpty(_managedInstanceClientId))
                            {
                                AppendLogMessage($"[MSG] Looking for our managed instance: {_managedInstanceClientId}", "debug");

                                var currentManagedInstanceData = instancesArray.OfType<JObject>()
                                    .FirstOrDefault(inst => inst["clientId"]?.ToString() == _managedInstanceClientId);

                                if (currentManagedInstanceData != null)
                                {
                                    AppendLogMessage($"[MSG] Found our instance in the list!", "debug");

                                    _managedInstance ??= new InstanceStatusInfo { ClientId = _managedInstanceClientId };
                                    _managedInstance.Status = currentManagedInstanceData["status"]?.ToString() ?? _managedInstance.Status;
                                    _managedInstance.PhoneNumber = currentManagedInstanceData["phoneNumber"]?.ToString() ?? _managedInstance.PhoneNumber;
                                    string? serverName = currentManagedInstanceData["name"]?.ToString();
                                    _managedInstance.Alias = serverName ?? _managedInstance.Alias ?? _managedInstance.PhoneNumber ?? _managedInstance.ClientId;

                                    UpdateListViewForSingleInstance();
                                    AppendLogMessage($"[MSG] Updated managed instance from list: {FormatDisplayInfo(_managedInstance)} - Status: {_managedInstance.Status}", "info");
                                }
                                else
                                {
                                    AppendLogMessage($"[MSG] Our instance {_managedInstanceClientId} NOT found in instance list", "warn");
                                    HandleManagedInstanceBecameUnusable(_managedInstanceClientId, "not_found_in_list");
                                }
                            }
                            else
                            {
                                AppendLogMessage("[MSG] No managed instance ID set", "debug");
                                UpdateListViewForSingleInstance();
                            }
                        }
                        else
                        {
                            AppendLogMessage("[MSG] Instance list is null or empty", "warn");
                        }
                        break;

                    case "instanceDeleted":
                        if (msgClientId == _managedInstanceClientId)
                        {
                            HandleManagedInstanceBecameUnusable(msgClientId, "deleted_by_manager");
                        }
                        break;
                    case "instanceLogs":
                        if (!string.IsNullOrEmpty(msgClientId) && msgClientId == _managedInstanceClientId)
                        {
                            JArray? logs = message["logs"] as JArray;
                            if (logs != null)
                            {
                                foreach (var log in logs)
                                {
                                    AppendLogMessage(log.ToString(), "info");
                                }
                            }
                        }
                        break;
                    case "logMessage":
                        if (!string.IsNullOrEmpty(msgClientId) && msgClientId == _managedInstanceClientId)
                        {
                            AppendLogMessage(message["log"]?.ToString() ?? "N/A", message["level"]?.ToString() ?? "info");
                        }
                        break;

                    case "groupsList":
                    case "participantsList":
                    case "addChatToWhitelistResponse":
                    case "removeFromChatWhitelistResponse":
                    case "participantDetailsUpdate":
                        if (_activeGroupsForm != null && !_activeGroupsForm.IsDisposed &&
                           !string.IsNullOrEmpty(msgClientId) && msgClientId == _managedInstanceClientId &&
                           _activeGroupsForm.IsForClient(msgClientId))
                        {
                            if (type == "participantDetailsUpdate") _activeGroupsForm.ProcessParticipantDetailsUpdate(messageJson);
                            else _activeGroupsForm.ProcessGroupsDisplayMessage(messageJson);
                        }
                        break;

                    // BEGIN EDIT: Update error handling in ProcessManagerControlMessage method
                    case "error":
                        string? errorSourceClientId = message["clientId"]?.ToString();
                        string? errorMessage = message["message"]?.ToString();
                        string effectiveClientIdForError = _currentQrDisplayForm?.CurrentClientId ?? _managedInstanceClientId ?? "Global";

                        if (!string.IsNullOrEmpty(errorSourceClientId) && errorSourceClientId == effectiveClientIdForError)
                        {
                            // Check if it's a non-critical error
                            bool isCriticalError = errorMessage?.Contains("fetchParticipants") == false &&
                                                 errorMessage?.Contains("fetchGroups") == false &&
                                                 errorMessage?.Contains("LID and Phone JID are required") == false && // Added this condition
                                                 errorMessage?.Contains("manual LID entry") == false; // Added for manual LID errors

                            MessageBox.Show($"Manager Error for {errorSourceClientId}: {errorMessage}", "Manager Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            AppendLogMessage($"Manager Error for {errorSourceClientId}: {errorMessage}", "error");

                            if (_currentQrDisplayForm != null && !_currentQrDisplayForm.IsDisposed)
                            {
                                _currentQrDisplayForm.Close();
                            }
                            else if (errorSourceClientId == _managedInstanceClientId && isCriticalError)
                            {
                                // Only mark as unusable for critical errors
                                HandleManagedInstanceBecameUnusable(errorSourceClientId, $"manager_error: {errorMessage}");
                            }
                        }
                        break;
                    // END EDIT

                    case "ack":
                        AppendLogMessage($"Manager ACK for '{message["originalType"] ?? ""}': {message["message"]}", (message["success"]?.ToObject<bool>() ?? false) ? "info" : "warn");
                        break;

                    case "manualLidEntryResponse":
                        string clientId = message["ClientId"]?.ToString() ?? "Unknown";
                        AppendLogMessage($"Received manualLidEntryResponse for ClientId: {clientId}", "info");
                        // You might want to add more specific handling here based on the response content
                        break;

                    default:
                        Console.WriteLine($"[UI] Unhandled message type from manager: {type}");
                        AppendLogMessage($"Received unhandled type: {type}", "debug");
                        break;
                }
            }
            catch (JsonException jex)
            {
                Console.WriteLine($"[JSON_ERROR] {jex.Message} - Msg: {messageJson}");
                AppendLogMessage($"JSON Error: {jex.Message}", "error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Processing manager message: {ex.Message} - Msg: {messageJson}");
                AppendLogMessage($"Processing Error: {ex.Message}", "error");
            }
            UpdateLinkAndActionButtonsState();
        }
        private void SetUIConnectedState(bool isConnected, string statusMessage = "")
        {
            if (InvokeRequired) { Invoke((MethodInvoker)delegate { SetUIConnectedState(isConnected, statusMessage); }); return; }
            lblConnectionStatus.Text = $"Manager: {statusMessage}";
            lblConnectionStatus.ForeColor = isConnected ? Color.DarkGreen : Color.Red;
            UpdateLinkAndActionButtonsState();
        }

        private void UpdateLinkAndActionButtonsState()
        {
            if (InvokeRequired) { Invoke((MethodInvoker)delegate { UpdateLinkAndActionButtonsState(); }); return; }

            bool isManagerConnected = _managerControlWs?.State == WebSocketState.Open;
            bool areCredentialsEntered = !string.IsNullOrWhiteSpace(textBoxApiUsername.Text) &&
                                         !string.IsNullOrWhiteSpace(textBoxApiPassword.Text) &&
                                         comboBoxCountryCode.SelectedItem != null &&
                                         !string.IsNullOrWhiteSpace(textBoxPhoneNumber.Text);
            bool isQrFormActive = _currentQrDisplayForm != null && !_currentQrDisplayForm.IsDisposed;

            bool isInstanceEffectivelyManaged = _managedInstance != null &&
                                             !string.IsNullOrEmpty(_managedInstanceClientId) &&
                                             _managedInstance.Status != "deleted" &&
                                             _managedInstance.Status != "not_found_in_list" &&
                                             !(_managedInstance.Status?.StartsWith("exited") ?? false) &&
                                             _managedInstance.Status != "error_spawning";

            if (isInstanceEffectivelyManaged && _managedInstance?.Status != "logged_out" && _managedInstance?.Status != "error_whatsapp_permanent")
            {
                linkWhatsappButton.Text = $"Unlink {FormatDisplayInfo(_managedInstance, true)}";
                linkWhatsappButton.BackColor = Color.IndianRed;
                linkWhatsappButton.Enabled = isManagerConnected && !isQrFormActive;
                textBoxApiUsername.Enabled = false;
                textBoxApiPassword.Enabled = false;
                comboBoxCountryCode.Enabled = false;
                textBoxPhoneNumber.Enabled = false;
            }
            else
            {
                linkWhatsappButton.Text = "Link New WhatsApp Account";
                linkWhatsappButton.BackColor = Color.FromArgb(37, 211, 102);
                linkWhatsappButton.Enabled = isManagerConnected && areCredentialsEntered && !isQrFormActive;
                textBoxApiUsername.Enabled = true;
                textBoxApiPassword.Enabled = true;
                comboBoxCountryCode.Enabled = true;
                textBoxPhoneNumber.Enabled = true;
            }

            string? currentStatus = _managedInstance?.Status?.ToLower();

            // Check if WhatsApp is connected (either "connected" or "connected_whatsapp")
            bool isWhatsAppConnected = currentStatus == "connected" || currentStatus == "connected_whatsapp";

            btnStartInstance.Enabled = isManagerConnected && isInstanceEffectivelyManaged && (currentStatus == "stopped" || currentStatus == "disconnected" || currentStatus == "error" || currentStatus == "logged_out" || currentStatus == "error_whatsapp_permanent") && !isQrFormActive;
            btnStopInstance.Enabled = isManagerConnected && isInstanceEffectivelyManaged && (isWhatsAppConnected || currentStatus == "qr_received" || currentStatus == "connecting_whatsapp" || currentStatus == "restarting_stopping" || currentStatus == "restarting") && !isQrFormActive;
            btnRestartInstance.Enabled = isManagerConnected && isInstanceEffectivelyManaged && (isWhatsAppConnected || currentStatus == "disconnected" || currentStatus == "stopped" || currentStatus == "error" || currentStatus == "logged_out") && !isQrFormActive;
            btnStopAndDeleteInstance.Enabled = isManagerConnected && isInstanceEffectivelyManaged && !isQrFormActive;
            btnGetLogs.Enabled = isManagerConnected && isInstanceEffectivelyManaged && !isQrFormActive;

            // Update this line to use isWhatsAppConnected
            btnManageGroups.Enabled = isManagerConnected && isInstanceEffectivelyManaged && isWhatsAppConnected && !isQrFormActive;

            btnClearCacheAndLogs.Enabled = isManagerConnected && isInstanceEffectivelyManaged && !isQrFormActive;
            btnRefreshInstances.Enabled = isManagerConnected;
        }
        private string GetAppSettingsFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolderPath = Path.Combine(appDataPath, "WhatsAppLinkerApp");
            Directory.CreateDirectory(appFolderPath);
            return Path.Combine(appFolderPath, "app_settings.json");
        }

        private void LoadAppSettings()
        {
            string settingsFile = GetAppSettingsFilePath();
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
                                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(encryptedData, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                                textBoxApiPassword.Text = Encoding.UTF8.GetString(decryptedData);
                            }
                            catch (Exception ex) { Console.WriteLine($"[SETTINGS] Decrypt error: {ex.Message}"); textBoxApiPassword.Text = ""; AppendLogMessage($"Error decrypting saved password: {ex.Message}", "error"); }
                        }
                        textBoxPhoneNumber.Text = settings.OwnerPhoneNumber;
                        if (!string.IsNullOrEmpty(settings.OwnerCountryCodeSelectedItem))
                        {
                            int index = comboBoxCountryCode.FindStringExact(settings.OwnerCountryCodeSelectedItem);
                            if (index != -1) comboBoxCountryCode.SelectedIndex = index;
                            else if (comboBoxCountryCode.Items.Count > 0) comboBoxCountryCode.SelectedIndex = 0;
                        }
                        _managedInstanceClientId = settings.LastManagedClientId;
                        if (!string.IsNullOrEmpty(_managedInstanceClientId))
                        {
                            AppendLogMessage($"Loaded last managed Client ID: {_managedInstanceClientId}. Will attempt to fetch its status.", "info");
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine($"[SETTINGS_ERROR] Load: {ex.Message}"); AppendLogMessage($"Error loading settings: {ex.Message}", "error"); }
            }
            UpdateLinkAndActionButtonsState();
            UpdateListViewForSingleInstance();
        }

        private async void SaveAppSettings()
        {
            AppSettings settings = new AppSettings
            {
                ApiUsername = textBoxApiUsername.Text,
                OwnerCountryCodeSelectedItem = comboBoxCountryCode.SelectedItem?.ToString(),
                OwnerPhoneNumber = textBoxPhoneNumber.Text,
                LastManagedClientId = _managedInstanceClientId
            };
            if (!string.IsNullOrEmpty(textBoxApiPassword.Text))
            {
                try
                {
                    byte[] dataToEncrypt = Encoding.UTF8.GetBytes(textBoxApiPassword.Text);
                    byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(dataToEncrypt, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                    settings.EncryptedApiPassword = Convert.ToBase64String(encryptedData);
                }
                catch (Exception ex) { Console.WriteLine($"[SETTINGS_ERROR] Encrypt: {ex.Message}"); settings.EncryptedApiPassword = null; AppendLogMessage($"Error encrypting password for saving: {ex.Message}", "error"); }
            }
            else { settings.EncryptedApiPassword = null; }
            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(GetAppSettingsFilePath(), json);
                AppendLogMessage("App settings saved.", "debug");
            }
            catch (Exception ex) { Console.WriteLine($"[SETTINGS_ERROR] Save: {ex.Message}"); AppendLogMessage($"Error saving settings: {ex.Message}", "error"); }
            // Also update database if we have a managed instance
            if (!string.IsNullOrEmpty(_managedInstanceClientId) && _managedInstance != null)
            {
                try
                {
                    await _db.ExecuteAsync(@"
                UPDATE bot_instances 
                SET last_seen = CURRENT_TIMESTAMP,
                    status = @status,
                    updated_at = CURRENT_TIMESTAMP
                WHERE client_id = @clientId",
                        new
                        {
                            clientId = _managedInstanceClientId,
                            status = _managedInstance.Status ?? "unknown"
                        });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DB_ERROR] Failed to update instance in database: {ex.Message}");
                }
            }
        }

        private void UpdateListViewForSingleInstance()
        {
            if (InvokeRequired) { Invoke((MethodInvoker)delegate { UpdateListViewForSingleInstance(); }); return; }

            instanceListView.BeginUpdate();
            instanceListView.Items.Clear();

            if (_managedInstance != null && !string.IsNullOrEmpty(_managedInstanceClientId))
            {
                var item = new ListViewItem(_managedInstance.ClientId);
                item.SubItems.Add(_managedInstance.PhoneNumber ?? "N/A");
                item.SubItems.Add(FormatDisplayInfo(_managedInstance, true));
                item.SubItems.Add(_managedInstance.Status ?? "unknown");
                item.SubItems.Add(DateTime.Now.ToLocalTime().ToString("g"));
                item.Tag = _managedInstance.ClientId;
                ApplyInstanceStatusColor(item, _managedInstance.Status ?? "unknown");
                instanceListView.Items.Add(item);
                if (instanceListView.Items.Count > 0) instanceListView.Items[0].Selected = true;
            }
            instanceListView.EndUpdate();
            lblInstanceCount.Text = _managedInstance != null ? "Managed Instance: 1" : "Managed Instance: 0";
            UpdateLinkAndActionButtonsState();
        }

        private string FormatDisplayInfo(InstanceStatusInfo? instance, bool preferAlias = false)
        {
            if (instance == null) return "N/A";
            if (preferAlias && !string.IsNullOrWhiteSpace(instance.Alias)) return instance.Alias;
            if (!string.IsNullOrWhiteSpace(instance.PhoneNumber)) return instance.PhoneNumber;
            return instance.ClientId;
        }

        private string GetFullOwnerNumber() // Helper method
        {
            string? selectedCountryCodeItem = comboBoxCountryCode.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(textBoxPhoneNumber.Text) || selectedCountryCodeItem == null)
            {
                return string.Empty;
            }
            string selectedCountryCode = "";
            string[] parts = selectedCountryCodeItem.Split(' ');
            if (parts.Length > 1)
            {
                selectedCountryCode = parts[1].Replace("+", "").Trim();
            }
            if (string.IsNullOrEmpty(selectedCountryCode))
            {
                return string.Empty;
            }
            return $"{selectedCountryCode}{textBoxPhoneNumber.Text.Trim()}";
        }

        private void OnNewClientSuccessfullyLinked(string clientId, string phoneNumber, string clientName, string apiUsernameUsed, string apiPasswordUsed, string ownerNumberUsed)
        {
            _managedInstanceClientId = clientId;
            _managedInstance = new InstanceStatusInfo
            {
                ClientId = clientId,
                PhoneNumber = phoneNumber,
                Alias = clientName,
                Status = "connected"
            };

            textBoxApiUsername.Text = apiUsernameUsed;
            textBoxApiPassword.Text = apiPasswordUsed;

            SaveAppSettings();
            UpdateListViewForSingleInstance();
            AppendLogMessage($"Successfully linked and now managing: {FormatDisplayInfo(_managedInstance)}", "info");
            MessageBox.Show($"Client {phoneNumber} ({clientName}) linked successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateLinkAndActionButtonsState();
        }

        private async void QrForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            bool success = (_currentQrDisplayForm?.DialogResult == DialogResult.OK);

            if (_currentQrDisplayForm != null)
            {
                _currentQrDisplayForm.ClientLinked -= OnNewClientSuccessfullyLinked;
                _currentQrDisplayForm.FormClosed -= QrForm_FormClosed;
                _currentQrDisplayForm = null;
            }

            if (!success && string.IsNullOrEmpty(_managedInstanceClientId))
            {
                AppendLogMessage("QR linking process was cancelled or failed before completion.", "warn");
            }
            if (_managerControlWs?.State == WebSocketState.Open)
            {
                await SendManagerCommand("listInstances");
            }
            UpdateLinkAndActionButtonsState();
        }

        // Original method names restored below with updated logic

        private async void btnRefreshInstances_Click(object? sender, EventArgs e)
        {
            if (_managerControlWs?.State == WebSocketState.Open)
            {
                AppendLogMessage("Requesting instance list from manager...", "debug");
                await SendManagerCommand("listInstances");
            }
            else
            {
                AppendLogMessage("Cannot refresh, not connected to manager.", "warn");
            }
        }

        private async void btnStartInstance_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_managedInstanceClientId))
            {
                AppendLogMessage($"Attempting to start instance: {_managedInstanceClientId}", "info");
                await SendManagerCommand("startInstance", _managedInstanceClientId);
            }
            else MessageBox.Show("No instance is currently managed to start.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnStopInstance_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_managedInstanceClientId))
            {
                AppendLogMessage($"Attempting to stop instance: {_managedInstanceClientId}", "info");
                await SendManagerCommand("stopInstance", _managedInstanceClientId);
            }
            else MessageBox.Show("No instance is currently managed to stop.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnRestartInstance_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_managedInstanceClientId))
            {
                AppendLogMessage($"Attempting to restart instance: {_managedInstanceClientId}", "info");
                await SendManagerCommand("restartInstance", _managedInstanceClientId);
            }
            else MessageBox.Show("No instance is currently managed to restart.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnStopAndDeleteInstance_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_managedInstanceClientId) && _managedInstance != null)
            {
                if (MessageBox.Show($"Confirm DELETE instance {FormatDisplayInfo(_managedInstance, true)} and all its data? This is permanent.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    AppendLogMessage($"User initiated delete for instance: {_managedInstanceClientId}", "info");
                    await SendManagerCommand("deleteInstance", _managedInstanceClientId);
                }
            }
            else MessageBox.Show("No instance is currently managed to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnGetLogs_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_managedInstanceClientId))
            {
                logTextBox.Clear();
                logTextBox.Tag = _managedInstanceClientId;
                AppendLogMessage($"--- Logs for Client: {FormatDisplayInfo(_managedInstance)} ---", "info");
                AppendLogMessage("Fetching logs...", "info");
                await SendManagerCommand("getLogs", _managedInstanceClientId);
            }
            else MessageBox.Show("No instance is currently managed to get logs for.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnManageGroups_Click(object? sender, EventArgs e)
        {
            if (_managedInstance != null && !string.IsNullOrEmpty(_managedInstanceClientId) && !string.IsNullOrEmpty(_managedInstance.PhoneNumber))
            {
                if (_managerControlWs == null || _managerControlWs.State != WebSocketState.Open || _controlWsCts == null)
                { MessageBox.Show("Not connected to manager.", "Error"); return; }

                if (_activeGroupsForm != null && !_activeGroupsForm.IsDisposed && _activeGroupsForm.IsForClient(_managedInstanceClientId))
                { _activeGroupsForm.Activate(); return; }

                if (_activeGroupsForm != null && !_activeGroupsForm.IsDisposed)
                { _activeGroupsForm.Close(); _activeGroupsForm = null; }

                _activeGroupsForm = new GroupsDisplayForm(_managerControlWs, _controlWsCts, _managedInstanceClientId, _managedInstance.PhoneNumber);
                _activeGroupsForm.FormClosed += (s, args) => { _activeGroupsForm = null; UpdateLinkAndActionButtonsState(); };
                _activeGroupsForm.Show(this);
                AppendLogMessage($"Opening group management for {FormatDisplayInfo(_managedInstance)}.", "info");
            }
            else MessageBox.Show("No connected instance selected or phone number missing for group management.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnClearCacheAndLogs_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_managedInstanceClientId) && _managedInstance != null)
            {
                if (MessageBox.Show($"Confirm clear cache & logs for instance {FormatDisplayInfo(_managedInstance, true)}? This stops the instance and requires re-linking.", "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    AppendLogMessage($"Requesting cache/log clear for {_managedInstanceClientId}", "info");
                    await SendManagerCommand("deleteInstance", _managedInstanceClientId);
                    MessageBox.Show($"Data clear (delete) request sent for {FormatDisplayInfo(_managedInstance, true)}. You will need to link the account again.", "Info");
                }
            }
            else
            {
                MessageBox.Show("No instance is currently managed.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AppendLogMessage(string message, string level)
        {
            if (InvokeRequired) { Invoke((MethodInvoker)delegate { AppendLogMessage(message, level); }); return; }
            Color textColor = Color.WhiteSmoke;
            if (level.ToLower() == "error") textColor = Color.Red;
            else if (level.ToLower() == "warn") textColor = Color.Orange;
            else if (level.ToLower() == "debug") textColor = Color.LightGray;
            else if (level.ToLower() == "info") textColor = Color.LightBlue;

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string formattedMessage = $"{timestamp} [{level.ToUpper()}] {message}{Environment.NewLine}";

            const int maxLogLength = 30000;
            if (logTextBox.TextLength > maxLogLength)
            {
                logTextBox.Text = logTextBox.Text.Substring(logTextBox.TextLength - (maxLogLength / 2));
            }

            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.SelectionLength = 0;
            logTextBox.SelectionColor = textColor;
            logTextBox.AppendText(formattedMessage);
            logTextBox.SelectionColor = logTextBox.ForeColor;
            logTextBox.ScrollToCaret();
        }

        private void instanceListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0 && instanceListView.SelectedItems[0].Tag?.ToString() == _managedInstanceClientId)
            {
                // Already showing logs for this if it's the managed one.
            }
            else
            {
                if (instanceListView.SelectedItems.Count == 0 && logTextBox.Tag != null && _managedInstance == null)
                {
                    logTextBox.Clear();
                    logTextBox.Tag = null;
                }
            }
            UpdateLinkAndActionButtonsState();
        }

        private void instanceContextMenuStrip_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            setAliasToolStripMenuItem.Enabled = instanceListView.SelectedItems.Count > 0 && _managedInstance != null;
        }

        private void setAliasToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (_managedInstance == null || string.IsNullOrEmpty(_managedInstanceClientId)) return;

            string currentAlias = _managedInstance.Alias ?? "";
            string newAlias = Interaction.InputBox($"Enter new alias for instance {_managedInstanceClientId} (current: {FormatDisplayInfo(_managedInstance, true)}):", "Set Instance Alias", currentAlias);

            if (newAlias != currentAlias)
            {
                _managedInstance.Alias = string.IsNullOrWhiteSpace(newAlias) ? null : newAlias.Trim();
                UpdateListViewForSingleInstance();
                SaveAppSettings();
                MessageBox.Show(string.IsNullOrWhiteSpace(_managedInstance.Alias) ?
                                $"Alias for {_managedInstanceClientId} cleared." :
                                $"Alias for {_managedInstanceClientId} set to '{_managedInstance.Alias}'. (Note: This alias is local to this UI)",
                                "Alias Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppendLogMessage($"Local alias for {_managedInstanceClientId} updated to: '{_managedInstance.Alias ?? "(cleared)"}'", "info");
            }
        }

        private void HandleManagedInstanceBecameUnusable(string? clientId, string reason = "unknown")
        {
            if (this.InvokeRequired) { this.Invoke((MethodInvoker)delegate { HandleManagedInstanceBecameUnusable(clientId, reason); }); return; }

            if (clientId == _managedInstanceClientId)
            {
                string message = $"Managed instance '{FormatDisplayInfo(_managedInstance, true)}' (ID: {clientId}) is no longer usable (Reason: {reason}). Please link a new account.";
                AppendLogMessage(message, "warn");
                MessageBox.Show(message, "Instance Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                _managedInstance = null;
                _managedInstanceClientId = null;

                UpdateListViewForSingleInstance();
                SaveAppSettings();
                UpdateLinkAndActionButtonsState();
            }
        }
        private async Task<ApiSyncLog> GetLastApiSync()
        {
            if (_managedInstance == null) return null;

            var botInstance = await _db.QuerySingleAsync<dynamic>(
                "SELECT id FROM bot_instances WHERE client_id = @clientId",
                new { clientId = _managedInstanceClientId }
            );

            if (botInstance == null) return null;

            return await _db.QuerySingleAsync<ApiSyncLog>(
                @"SELECT * FROM api_sync_log 
          WHERE bot_instance_id = @botId 
          ORDER BY started_at DESC 
          LIMIT 1",
                new { botId = botInstance.id }
            );
        }

        // Add UI element to show sync status
        private async void ShowApiSyncStatus()
        {
            var lastSync = await GetLastApiSync();
            if (lastSync != null)
            {
                string status = lastSync.SyncStatus == "completed"
                    ? $"Last sync: {lastSync.CompletedAt:g} - Added: {lastSync.ContactsAdded}, Updated: {lastSync.ContactsUpdated}, Removed: {lastSync.ContactsRemoved}"
                    : lastSync.SyncStatus == "failed"
                    ? $"Last sync failed: {lastSync.ErrorMessage}"
                    : "Sync in progress...";

                // lblApiSyncStatus.Text = status;
                // lblApiSyncStatus.ForeColor = lastSync.SyncStatus == "completed" ? Color.Green : Color.Red;
            }
        }
        // lblApiSyncStatus is a Label control on the form to display sync status

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            SaveAppSettings();
            base.OnFormClosing(e);

            if (_controlWsCts != null)
            {
                _controlWsCts.Cancel();
            }

            if (_readMessageTask != null && !_readMessageTask.IsCompleted)
            {
                try { await Task.WhenAny(_readMessageTask, Task.Delay(TimeSpan.FromSeconds(1))); }
                catch (OperationCanceledException) { /* Expected */ }
                catch (Exception ex) { Console.WriteLine($"[FORM_CLOSING_ERROR] Awaiting readMessageTask: {ex.Message}"); }
            }
            _controlWsCts?.Dispose();
            _controlWsCts = null;

            ClientWebSocket? wsToClose = _managerControlWs;
            _managerControlWs = null;

            if (wsToClose != null)
            {
                if (wsToClose.State == WebSocketState.Open || wsToClose.State == WebSocketState.CloseReceived)
                {
                    try
                    {
                        using var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                        await wsToClose.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "UI Closing", closeCts.Token);
                    }
                    catch (OperationCanceledException) { Console.WriteLine("[FORM_CLOSING_INFO] WebSocket CloseOutputAsync timed out or was cancelled."); }
                    catch (Exception ex) { Console.WriteLine($"[FORM_CLOSING_ERROR] WebSocket CloseOutputAsync: {ex.Message}"); }
                }
                wsToClose.Dispose();
            }

            _activeGroupsForm?.Close(); _activeGroupsForm = null;
            _currentQrDisplayForm?.Close(); _currentQrDisplayForm = null;
        }
    }

    public class InstanceStatusInfo
    {
        public string ClientId { get; set; } = string.Empty;
        public string? Status { get; set; } = "unknown";
        public string? StatusMessage { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Alias { get; set; }
    }
}