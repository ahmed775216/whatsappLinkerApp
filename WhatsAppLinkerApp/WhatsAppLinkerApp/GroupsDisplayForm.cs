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
using System.IO;
using WhatsAppLinkerApp.Database;

namespace WhatsAppLinkerApp
{
    public class ParticipantInfo
    {
        public string Jid { get; set; }
        public string? ResolvedPhoneJid { get; set; }
        public string DisplayName { get; set; }
        public string PhoneNumber { get; set; } // Cleaned phone number for display/logic
        public bool IsAdmin { get; set; }
        public bool IsWhitelisted { get; set; }

        public ParticipantInfo()
        {
            Jid = string.Empty;
            DisplayName = string.Empty;
            PhoneNumber = string.Empty;
        }

        public override string ToString()
        {
            var adminText = IsAdmin ? "Admin" : "Member";
            var whitelistText = IsWhitelisted ? "✓ Whitelisted" : "✗ Not Whitelisted";
            string nameToUse = DisplayName;
            string phoneToUse = PhoneNumber;

            if (string.IsNullOrEmpty(nameToUse) || nameToUse == phoneToUse || nameToUse.Contains("@"))
            {
                nameToUse = phoneToUse;
            }
            return $"{nameToUse} ({phoneToUse}) - {adminText}, {whitelistText}";
        }
    }

    public partial class GroupsDisplayForm : Form
    {
        private readonly ClientWebSocket _sharedWebSocketClient;
        private readonly CancellationTokenSource _sharedCancellationTokenSource;
        private readonly string _clientId;
        private readonly string _clientPhoneNumber; // The phone number of the bot instance
        private bool _isProcessingCommand = false;

        private List<JObject> _allFetchedGroups = new List<JObject>();
        private List<ParticipantInfo> _allParticipants = new List<ParticipantInfo>(); // Store all participants for filtering

        // Add at the top of the class
        // private readonly DatabaseConnection? _db;
        private int? _cachedBotInstanceId = null;

        // Update constructor
        public GroupsDisplayForm(ClientWebSocket sharedWsClient, CancellationTokenSource sharedCts, string clientId, string clientPhoneNumber)
        {
            InitializeComponent();

            _sharedWebSocketClient = sharedWsClient ?? throw new ArgumentNullException(nameof(sharedWsClient));
            _sharedCancellationTokenSource = sharedCts ?? throw new ArgumentNullException(nameof(sharedCts));
            _clientId = clientId;
            _clientPhoneNumber = clientPhoneNumber;

            // Initialize database connection with error handling
            try
            {
                _db = new DatabaseConnection();
                Console.WriteLine("[GROUPS_FORM] Database connection initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GROUPS_FORM_ERROR] Failed to initialize database: {ex.Message}");
                _db = null;
            }

            SetupFormInitialState();
            ApplyGroupFormStyles();
            CheckWebSocketConnection();
        }

        // Update GetBotInstanceId with better error handling
        private async Task<int?> GetBotInstanceId()
        {
            Console.WriteLine($"[GROUPS_DB] GetBotInstanceId called for client: {_clientId}");

            // Return cached value if available
            if (_cachedBotInstanceId.HasValue)
            {
                Console.WriteLine($"[GROUPS_DB] Returning cached bot instance ID: {_cachedBotInstanceId}");
                return _cachedBotInstanceId;
            }

            if (_db == null)
            {
                Console.WriteLine("[GROUPS_DB] Database connection is null");
                return null;
            }

            try
            {
                Console.WriteLine($"[GROUPS_DB] Querying database for client_id: {_clientId}");

                var query = "SELECT id FROM bot_instances WHERE client_id = @clientId LIMIT 1";
                var result = await _db.QuerySingleAsync<int?>(query, new { clientId = _clientId });

                if (result.HasValue)
                {
                    Console.WriteLine($"[GROUPS_DB] Found bot instance ID: {result}");
                    _cachedBotInstanceId = result;
                }
                else
                {
                    Console.WriteLine($"[GROUPS_DB] No bot instance found for client_id: {_clientId}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GROUPS_DB_ERROR] Failed to get bot instance ID: {ex.Message}");
                Console.WriteLine($"[GROUPS_DB_ERROR] Stack trace: {ex.StackTrace}");

                // Don't throw, just return null
                return null;
            }
        }

        // Update ProcessParticipantsList to handle database errors gracefully
        private async void ProcessParticipantsList(JObject message)
        {
            Console.WriteLine("[GROUPS] ProcessParticipantsList called");

            participantsListView.Items.Clear();
            _allParticipants.Clear();

            JArray? participants = message["participants"] as JArray;
            string? forGroupId = message["groupId"]?.ToString();

            Console.WriteLine($"[GROUPS] Processing participants for group: {forGroupId}");
            Console.WriteLine($"[GROUPS] Participant count: {participants?.Count ?? 0}");

            string groupName = groupsListView.Items.Cast<ListViewItem>()
                               .FirstOrDefault(it => it.Tag?.ToString() == forGroupId)?.SubItems[1].Text ?? forGroupId?.Split('@')[0] ?? "Group";

            // Try to save to database but don't fail if it doesn't work
            if (_db != null)
            {
                try
                {
                    var botInstanceId = await GetBotInstanceId();
                    if (botInstanceId.HasValue && !string.IsNullOrEmpty(forGroupId))
                    {
                        Console.WriteLine($"[GROUPS_DB] Saving participants to database for bot instance: {botInstanceId}");

                        // Clear existing participants for this group
                        await _db.ExecuteAsync(
                            "DELETE FROM group_participants WHERE bot_instance_id = @botId AND group_jid = @groupJid",
                            new { botId = botInstanceId.Value, groupJid = forGroupId }
                        );

                        // Save new participants (but continue even if this fails)
                        // ... database save code ...


                    }
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"[GROUPS_DB_ERROR] Failed to save participants to database: {dbEx.Message}");
                    // Continue processing - database is optional
                }
            }

            if (participants != null && participants.Count > 0)
            {
                // In GroupsDisplayForm.cs, inside ProcessParticipantsList method

                foreach (JObject p in participants)
                {
                    try
                    {
                        string? jid = p["jid"]?.ToString();
                        if (string.IsNullOrEmpty(jid)) continue;

                        string? resolvedJidPayload = p["resolvedJid"]?.ToString();
                        string? namePayload = p["displayName"]?.ToString();

                        // *** NEW UI LOGIC START ***
                        string nameToDisplay;
                        string phoneToDisplay;

                        if (!string.IsNullOrEmpty(resolvedJidPayload))
                        {
                            // ---- AFTER RESOLUTION ----
                            phoneToDisplay = ExtractPhoneNumber(resolvedJidPayload);
                            // Use the name from the payload. If it's empty or just a number, use the phone number as the name.
                            nameToDisplay = (!string.IsNullOrEmpty(namePayload) && !namePayload.Contains("@")) ? namePayload : phoneToDisplay;
                        }
                        else
                        {
                            // ---- BEFORE RESOLUTION (could be LID or regular number) ----
                            phoneToDisplay = ExtractPhoneNumber(jid); // This will be "Unknown (LID)" for LIDs.
                                                                      // The name is the best name we have (likely the pushName).
                            nameToDisplay = namePayload ?? phoneToDisplay;
                        }
                        // *** NEW UI LOGIC END ***

                        ParticipantInfo newPInfo = new ParticipantInfo
                        {
                            Jid = jid,
                            ResolvedPhoneJid = resolvedJidPayload,
                            PhoneNumber = phoneToDisplay, // The clean phone number or "Unknown (LID)"
                            DisplayName = nameToDisplay,  // The best available name
                            IsAdmin = p["isAdmin"]?.ToObject<bool>() ?? false,
                            IsWhitelisted = p["isWhitelisted"]?.ToObject<bool>() ?? false
                        };

                        _allParticipants.Add(newPInfo);
                    }
                    catch (Exception pEx)
                    {
                        Console.WriteLine($"[GROUPS_ERROR] Failed to process participant: {pEx.Message}");
                    }
                }
                _allParticipants = _allParticipants
                    .OrderByDescending(p => p.IsAdmin)
                    .ThenByDescending(p => p.IsWhitelisted)
                    .ThenBy(p => p.DisplayName)
                    .ToList();

                UpdateStatus($"Loaded {_allParticipants.Count} participants for {groupName}.", SystemColors.ControlText);
            }
            else
            {
                UpdateStatus($"No participants found for {groupName}.", Color.Orange);
                lblParticipantsTitle.Text = $"No participants for \"{groupName}\"";
            }

            lblParticipantsTitle.Text = $"Participants for \"{groupName}\"";
            lblParticipantsCount.Text = $"Total: {_allParticipants.Count}";
            txtSearchParticipant.Clear();
            FilterParticipantsListView();
        }
        private void SetupFormInitialState()
        {
            labelSelectedClient.Text = $"Groups for Client: {(_clientPhoneNumber.Length > 5 ? _clientPhoneNumber.Substring(0, 5) + "..." : _clientPhoneNumber)} ({_clientId})";

            // Group ListView columns are already defined in designer, just set widths
            groupsListView.Columns[0].Width = 120; // Group ID
            groupsListView.Columns[1].Width = 150; // Group Name
            groupsListView.Columns[2].Width = 60;  // Members
            groupsListView.Columns[3].Width = 70;  // Whitelisted

            // Participants ListView columns are defined in designer, just set widths
            participantsListView.View = View.Details;
            participantsListView.FullRowSelect = true;
            participantsListView.GridLines = true;
            participantsListView.Columns[0].Width = 180; // Name
            participantsListView.Columns[1].Width = 130; // Phone
            participantsListView.Columns[2].Width = 80;  // Role
            participantsListView.Columns[3].Width = 80;  // Status

            // Initial state for LID entry controls
            txtManualLidPhoneNumber.Clear();
            btnUpdateLidCache.Enabled = false;
        }

        private void CheckWebSocketConnection()
        {
            if (_sharedWebSocketClient.State != WebSocketState.Open)
            {
                UpdateStatus("WebSocket connection lost. Please reconnect.", Color.Red);
                DisableAllButtons();
            }
        }

        private void DisableAllButtons() => SetButtonsProcessingState(true, true);
        private void EnableAllButtons() => SetButtonsProcessingState(false);

        private void ApplyGroupFormStyles()
        {
            // Groups ListView Styling
            groupsListView.OwnerDraw = true;
            groupsListView.DrawColumnHeader += (s, e) =>
            {
                using var headerBrush = new SolidBrush(Color.FromArgb(18, 140, 126));
                using var borderPen = new Pen(Color.FromArgb(12, 100, 90), 1);
                e.Graphics.FillRectangle(headerBrush, e.Bounds);
                e.Graphics.DrawRectangle(borderPen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                TextRenderer.DrawText(e.Graphics, e.Header?.Text ?? "", e.Font, e.Bounds, Color.White,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            };
            groupsListView.DrawSubItem += (s, e) => e.DrawDefault = true;

            // Participants ListView Styling
            participantsListView.OwnerDraw = true;
            participantsListView.DrawColumnHeader += (s, e) =>
            {
                using var headerBrush = new SolidBrush(Color.FromArgb(70, 130, 180));
                e.Graphics.FillRectangle(headerBrush, e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Header?.Text ?? "", e.Font, e.Bounds, Color.White,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            };
            participantsListView.DrawSubItem += (s, e) =>
            {
                if (e.Item?.Tag is ParticipantInfo participant)
                {
                    Color backColor = e.Item.Selected ? SystemColors.Highlight :
                                     participant.IsWhitelisted ? Color.FromArgb(240, 255, 240) : // Light green for whitelisted
                                     Color.White; // Default
                    Color textColor = e.Item.Selected ? SystemColors.HighlightText :
                                     participant.IsWhitelisted && participant.IsAdmin ? Color.DarkMagenta : // Whitelisted Admin
                                     participant.IsWhitelisted ? Color.DarkGreen : // Just Whitelisted
                                     participant.IsAdmin ? Color.DarkBlue : // Just Admin
                                     Color.Black; // Default

                    e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);
                    TextRenderer.DrawText(e.Graphics, e.SubItem?.Text ?? "", e.Item.Font, e.Bounds, textColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
                else
                {
                    e.DrawDefault = true; // Use default drawing for non-ParticipantInfo items (like header/loading text)
                }
            };
        }

        private void GroupsDisplayForm_Load(object? sender, EventArgs e)
        {
            if (_sharedWebSocketClient.State == WebSocketState.Open)
            {
                EnableAllButtons();
                btnFetchGroups_Click(null, null); // Initial fetch
            }
            else
            {
                UpdateStatus("WebSocket not connected. Cannot fetch groups.", Color.Red);
                DisableAllButtons();
            }
        }

        private async Task<bool> SendManagerCommand(string type, string? groupId = null, string? participantJid = null, JObject? additionalData = null)
        {
            if (_isProcessingCommand)
            {
                UpdateStatus("Please wait, processing previous command...", Color.Orange);
                return false;
            }
            if (_sharedWebSocketClient.State != WebSocketState.Open)
            {
                MessageBox.Show("Connection to manager lost.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Connection lost", Color.Red);
                DisableAllButtons();
                return false;
            }

            SetButtonsProcessingState(true); // Disable buttons while sending

            try
            {
                var request = additionalData ?? new JObject();
                request["type"] = type;
                request["clientId"] = _clientId;
                if (groupId != null) request["groupId"] = groupId;
                if (participantJid != null) request["participantJid"] = participantJid;

                var buffer = Encoding.UTF8.GetBytes(request.ToString());
                if (_sharedCancellationTokenSource.IsCancellationRequested) return false;
                await _sharedWebSocketClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _sharedCancellationTokenSource.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Operation cancelled.", Color.Orange);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send command: {ex.Message}", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Command failed: {ex.Message}", Color.Red);
                return false;
            }
            finally
            {
                SetButtonsProcessingState(false); // Re-enable buttons after send attempt
            }
        }
// Add this method anywhere inside the GroupsDisplayForm class

            public void TriggerParticipantsRefresh()
            {
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate { TriggerParticipantsRefresh(); });
                    return;
                }

                // Check if a group is actually selected before trying to refresh
                if (groupsListView.SelectedItems.Count > 0)
                {
                    Console.WriteLine("[GROUPS_FORM] Participant refresh triggered externally (e.g., after LID update).");
                    btnFetchParticipants_Click(null, EventArgs.Empty);
                }
            }
        private void SetButtonsProcessingState(bool processing, bool forceDisable = false)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { SetButtonsProcessingState(processing, forceDisable); });
                return;
            }
            _isProcessingCommand = processing;
            bool canEnable = !processing && (_sharedWebSocketClient.State == WebSocketState.Open) && !forceDisable;

            btnFetchGroups.Enabled = canEnable;
            btnWhitelistGroup.Enabled = canEnable && groupsListView.SelectedItems.Count > 0;
            btnFetchParticipants.Enabled = canEnable && groupsListView.SelectedItems.Count > 0;
            toolStripButtonWhitelist.Enabled = canEnable && participantsListView.SelectedItems.Count > 0;
            btnUpdateLidCache.Enabled = canEnable && participantsListView.SelectedItems.Count > 0 &&
                                       (participantsListView.SelectedItems[0].Tag is ParticipantInfo pInfo && pInfo.Jid.EndsWith("@lid"));
        }

        private async void btnFetchGroups_Click(object? sender, EventArgs? e)
        {
            if (txtSearchGroup?.Text.Length > 0)
            {
                txtSearchGroup.Clear(); // Clearing text will trigger FilterGroupsListView
            }
            else
            {
                groupsListView.Items.Clear();
            }
            participantsListView.Items.Clear(); // Clear participants on new group fetch
            lblParticipantsTitle.Text = "Participants"; // Reset participant title
            lblParticipantsCount.Text = "Total: 0"; // Reset participant count

            UpdateStatus("Fetching groups...", SystemColors.ControlText);
            if (!await SendManagerCommand("fetchGroups"))
                UpdateStatus("Failed to send fetch groups command.", Color.Red);
        }

        private void txtSearchGroup_TextChanged(object? sender, EventArgs e)
        {
            FilterGroupsListView();
        }

        private void FilterGroupsListView()
        {
            string searchText = txtSearchGroup?.Text.ToLowerInvariant().Trim() ?? "";
            groupsListView.BeginUpdate();
            groupsListView.Items.Clear();

            if (_allFetchedGroups == null)
            {
                groupsListView.EndUpdate();
                return;
            }

            var filteredGroups = string.IsNullOrWhiteSpace(searchText)
                ? _allFetchedGroups
                : _allFetchedGroups.Where(group =>
                    (group["subject"]?.ToString() ?? "").ToLowerInvariant().Contains(searchText) ||
                    (group["id"]?.ToString() ?? "").ToLowerInvariant().Contains(searchText)
                  ).ToList();

            foreach (JObject group in filteredGroups)
            {
                var item = new ListViewItem(group["id"]?.ToString() ?? "N/A")
                {
                    Tag = group["id"]?.ToString() // Store Group ID in Tag
                };
                item.SubItems.Add(group["subject"]?.ToString() ?? "Unnamed");
                item.SubItems.Add(group["participantsCount"]?.ToString() ?? "0");

                // Fix: Handle isWhitelisted as various types
                bool isWhitelisted = false;
                var whitelistedToken = group["isWhitelisted"];
                if (whitelistedToken != null)
                {
                    if (whitelistedToken.Type == JTokenType.Boolean)
                    {
                        isWhitelisted = whitelistedToken.ToObject<bool>();
                    }
                    else if (whitelistedToken.Type == JTokenType.String)
                    {
                        isWhitelisted = whitelistedToken.ToString().ToLower() == "true";
                    }
                    else if (whitelistedToken.Type == JTokenType.Integer)
                    {
                        isWhitelisted = whitelistedToken.ToObject<int>() == 1;
                    }
                }

                item.SubItems.Add(isWhitelisted ? "Yes" : "No");
                if (isWhitelisted) item.BackColor = Color.LightGreen;
                groupsListView.Items.Add(item);
            }
            groupsListView.EndUpdate();

            if (groupsListView.Items.Count > 0)
            {
                groupsListView.Items[0].Selected = true;
                groupsListView.Items[0].Focused = true;
            }
            else
            {
                participantsListView.Items.Clear();
                lblParticipantsTitle.Text = "Participants";
                lblParticipantsCount.Text = "Total: 0";
                btnWhitelistGroup.Enabled = false;
                btnFetchParticipants.Enabled = false;
            }
            groupsListView_SelectedIndexChanged(null, EventArgs.Empty);
        }
        private async void btnWhitelistGroup_Click(object? sender, EventArgs e)
        {
            if (groupsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a group.", "No Group Selected");
                return;
            }

            ListViewItem selectedGroupItem = groupsListView.SelectedItems[0];
            string? groupId = selectedGroupItem.Tag?.ToString();
            string groupName = selectedGroupItem.SubItems[1].Text;
            bool currentWhitelistStatus = selectedGroupItem.SubItems[3].Text.Equals("Yes", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(groupId))
            {
                MessageBox.Show("Invalid group selection (no ID).", "Error");
                return;
            }

            string action = currentWhitelistStatus ? "remove from whitelist" : "add to whitelist";
            if (MessageBox.Show($"Are you sure you want to {action} group \"{groupName}\"?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            string command = currentWhitelistStatus ? "removeFromChatWhitelist" : "addChatToWhitelist";
            UpdateStatus($"Processing {action} for group \"{groupName}\"...", SystemColors.ControlText);
            await SendManagerCommand(command, groupId);
        }

        private async void btnFetchParticipants_Click(object? sender, EventArgs e)
        {
            if (groupsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a group.", "No Group Selected");
                return;
            }

            string? groupId = groupsListView.SelectedItems[0].Tag?.ToString();
            string groupName = groupsListView.SelectedItems[0].SubItems[1].Text;

            if (string.IsNullOrEmpty(groupId))
            {
                MessageBox.Show("Invalid group selection (no ID).", "Error");
                return;
            }

            participantsListView.Items.Clear();
            lblParticipantsTitle.Text = $"Fetching participants for \"{groupName}\"...";
            lblParticipantsCount.Text = "Total: 0";
            txtSearchParticipant.Clear(); // Clear participant search on new group selection
            txtManualLidPhoneNumber.Clear(); // Clear manual LID entry on new group selection

            UpdateStatus($"Fetching participants for \"{groupName}\"...", SystemColors.ControlText);
            await SendManagerCommand("fetchParticipants", groupId);
        }

        // Event handlers for the enhanced participants area

        private void txtSearchParticipant_TextChanged(object? sender, EventArgs e)
        {
            FilterParticipantsListView();
        }

        private void FilterParticipantsListView()
        {
            string searchText = txtSearchParticipant?.Text.ToLowerInvariant().Trim() ?? "";
            participantsListView.BeginUpdate();
            participantsListView.Items.Clear();

            if (_allParticipants == null || _allParticipants.Count == 0)
            {
                participantsListView.EndUpdate();
                lblParticipantsCount.Text = "Total: 0";
                return;
            }

            var currentFilteredList = string.IsNullOrWhiteSpace(searchText)
                ? _allParticipants.ToList() // If search is empty, show all
                : _allParticipants.Where(p =>
                    p.DisplayName.ToLowerInvariant().Contains(searchText) ||
                    p.PhoneNumber.ToLowerInvariant().Contains(searchText) ||
                    (p.ResolvedPhoneJid?.ToLowerInvariant().Contains(searchText) ?? false) ||
                    p.Jid.ToLowerInvariant().Contains(searchText) // Also search in original JID
                  ).ToList();

            // In GroupsDisplayForm.cs, inside FilterParticipantsListView()

            foreach (ParticipantInfo pInfo in currentFilteredList)
            {
                // --- Determine what to display in each column ---

                // NAME column logic:
                string nameForDisplay = pInfo.DisplayName;
                if (string.IsNullOrWhiteSpace(nameForDisplay) || nameForDisplay == pInfo.Jid.Split('@')[0])
                {
                    // If name is missing or is just the JID number, use our user-friendly fallback.
                    nameForDisplay = $"User ({pInfo.Jid.Split('@')[0]})";
                }

                // PHONE column logic:
                string phoneForDisplay;
                if (!string.IsNullOrEmpty(pInfo.ResolvedPhoneJid))
                {
                    // If resolved, always show the phone number.
                    phoneForDisplay = pInfo.PhoneNumber;
                }
                else if (pInfo.Jid.EndsWith("@lid"))
                {
                    // If it's an unresolved LID, show the LID.
                    phoneForDisplay = pInfo.Jid;
                }
                else
                {
                    // Otherwise, show the phone number from the JID.
                    phoneForDisplay = pInfo.PhoneNumber;
                }

                // Create the ListViewItem
                ListViewItem item = new ListViewItem(nameForDisplay); // Column 1: Name
                item.SubItems.Add(phoneForDisplay);                  // Column 2: Phone/LID
                item.SubItems.Add(pInfo.IsAdmin ? "Admin" : "Member");
                item.SubItems.Add(pInfo.IsWhitelisted ? "Yes" : "No");
                item.Tag = pInfo; // Store the full ParticipantInfo object in Tag

                participantsListView.Items.Add(item);
            }
            participantsListView.EndUpdate();

            lblParticipantsCount.Text = $"Total: {participantsListView.Items.Count}";

            if (participantsListView.Items.Count > 0)
            {
                participantsListView.Items[0].Selected = true;
                participantsListView.Items[0].Focused = true;
            }
            else
            {
                toolStripButtonWhitelist.Enabled = false;
                btnUpdateLidCache.Enabled = false; // Disable if no participants
            }
            participantsListView_SelectedIndexChanged(null, EventArgs.Empty);
        }

        private void groupsListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool isGroupSelected = groupsListView.SelectedItems.Count > 0;
            SetButtonsProcessingState(_isProcessingCommand); // Re-evaluate based on selection and processing state

            if (isGroupSelected)
            {
                bool isWhitelisted = groupsListView.SelectedItems[0].SubItems[3].Text.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                btnWhitelistGroup.Text = isWhitelisted ? "Remove Whitelist" : "Add Whitelist";
                // Automatically fetch participants for the selected group
                btnFetchParticipants_Click(null, EventArgs.Empty);
            }
            else
            {
                participantsListView.Items.Clear();
                lblParticipantsTitle.Text = "Participants";
                lblParticipantsCount.Text = "Total: 0";
            }
        }

        private void participantsListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SetButtonsProcessingState(_isProcessingCommand); // Re-evaluate
            if (participantsListView.SelectedItems.Count > 0 && participantsListView.SelectedItems[0].Tag is ParticipantInfo pInfo)
            {
                toolStripButtonWhitelist.Text = pInfo.IsWhitelisted ? "Remove Whitelist" : "Add Whitelist";
                // Populate manual LID entry box if an LID is selected
                if (pInfo.Jid.EndsWith("@lid"))
                {
                    txtManualLidPhoneNumber.Text = pInfo.PhoneNumber == "Unknown" ? "" : pInfo.PhoneNumber;
                }
                else
                {
                    txtManualLidPhoneNumber.Clear();
                }
            }
            else
            {
                toolStripButtonWhitelist.Text = "Add Whitelist"; // Default when no selection
                txtManualLidPhoneNumber.Clear();
            }
        }

        // Context Menu and ToolStrip Button handlers
        private async void toolStripButtonWhitelist_Click(object? sender, EventArgs e)
        {
            if (participantsListView.SelectedItems.Count == 0) { MessageBox.Show("Please select a participant.", "No Participant Selected"); return; }
            if (!(participantsListView.SelectedItems[0].Tag is ParticipantInfo selectedParticipant)) return;

            string action = selectedParticipant.IsWhitelisted ? "remove from whitelist" : "add to whitelist";
            if (MessageBox.Show($"Confirm {action} for \"{selectedParticipant.DisplayName} ({selectedParticipant.PhoneNumber})\"?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            string command = selectedParticipant.IsWhitelisted ? "removeFromChatWhitelist" : "addChatToWhitelist";
            UpdateStatus($"Processing {action} for {selectedParticipant.DisplayName}...", SystemColors.ControlText);
            await SendManagerCommand(command, null, selectedParticipant.Jid);
        }

        private void toolStripButtonExport_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.Title = "Export Participants to CSV";
                saveFileDialog.FileName = $"Participants_{_clientId}_{DateTime.Now:yyyyMMdd_HHmm}.csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                        {
                            // Write CSV header
                            sw.WriteLine("DisplayName,PhoneNumber,IsAdmin,IsWhitelisted,Jid,ResolvedPhoneJid");

                            string currentSearchText = txtSearchParticipant?.Text.ToLowerInvariant().Trim() ?? "";
                            var participantsToExport = string.IsNullOrWhiteSpace(currentSearchText)
                                ? _allParticipants
                                : _allParticipants.Where(p =>
                                    p.DisplayName.ToLowerInvariant().Contains(currentSearchText) ||
                                    p.PhoneNumber.ToLowerInvariant().Contains(currentSearchText) ||
                                    (p.ResolvedPhoneJid?.ToLowerInvariant().Contains(currentSearchText) ?? false) ||
                                    p.Jid.ToLowerInvariant().Contains(currentSearchText)
                                ).ToList();

                            // Write participant data
                            foreach (ParticipantInfo pInfo in participantsToExport)
                            {
                                sw.WriteLine($"\"{pInfo.DisplayName.Replace("\"", "\"\"")}\"," +
                                             $"\"{pInfo.PhoneNumber.Replace("\"", "\"\"")}\"," +
                                             $"{pInfo.IsAdmin}," +
                                             $"{pInfo.IsWhitelisted}," +
                                             $"\"{pInfo.Jid.Replace("\"", "\"\"")}\"," +
                                             $"\"{pInfo.ResolvedPhoneJid?.Replace("\"", "\"\"") ?? ""}\"");
                            }
                        }
                        MessageBox.Show("Participants exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting participants: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void toolStripButtonRefresh_Click(object? sender, EventArgs e)
        {
            btnFetchParticipants_Click(sender, e); // Re-fetch participants for current group
        }

        private void participantsListView_DoubleClick(object? sender, EventArgs e)
        {
            if (participantsListView.SelectedItems.Count > 0)
            {
                toolStripButtonWhitelist_Click(sender, e); // Example: Toggle whitelist on double click
            }
        }

        private void copyPhoneNumberToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (participantsListView.SelectedItems.Count > 0 && participantsListView.SelectedItems[0].Tag is ParticipantInfo pInfo)
            {
                Clipboard.SetText(pInfo.PhoneNumber);
                UpdateStatus($"Copied phone number: {pInfo.PhoneNumber}", Color.Blue);
            }
        }

        private void copyNameToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (participantsListView.SelectedItems.Count > 0 && participantsListView.SelectedItems[0].Tag is ParticipantInfo pInfo)
            {
                Clipboard.SetText(pInfo.DisplayName);
                UpdateStatus($"Copied display name: {pInfo.DisplayName}", Color.Blue);
            }
        }

        private void whitelistToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            toolStripButtonWhitelist_Click(sender, e); // Re-use toolstrip button logic
        }
        public bool IsForClient(string clientId)
        {
            return _clientId == clientId; // _clientId is a field in GroupsDisplayForm holding its associated client ID
        }
        private void participantsContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            bool hasSelection = participantsListView.SelectedItems.Count > 0;
            copyPhoneNumberToolStripMenuItem.Enabled = hasSelection;
            copyNameToolStripMenuItem.Enabled = hasSelection;
            whitelistToolStripMenuItem.Enabled = hasSelection;

            if (hasSelection && participantsListView.SelectedItems[0].Tag is ParticipantInfo pInfo)
            {
                whitelistToolStripMenuItem.Text = pInfo.IsWhitelisted ? "Remove Whitelist" : "Add Whitelist";
            }
            else
            {
                whitelistToolStripMenuItem.Text = "Toggle Whitelist"; // Default text
            }
        }

        private async void btnUpdateLidCache_Click(object? sender, EventArgs e)
        {
            if (participantsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a participant (LID) from the list first.", "No Participant Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!(participantsListView.SelectedItems[0].Tag is ParticipantInfo selectedParticipant)) return;

            if (!selectedParticipant.Jid.EndsWith("@lid"))
            {
                MessageBox.Show("The selected participant does not appear to be an unresolved LID.", "Not a LID", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string lidToUpdate = selectedParticipant.Jid;
            string phoneNumber = txtManualLidPhoneNumber.Text.Trim();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                MessageBox.Show("Please enter a phone number to associate with the LID.", "Phone Number Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtManualLidPhoneNumber.Focus();
                return;
            }

            if (!phoneNumber.All(char.IsDigit) || phoneNumber.Length < 7) // Minimum reasonable phone length
            {
                MessageBox.Show("Please enter a valid phone number (digits only, minimum 7 digits).", "Invalid Phone Number", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtManualLidPhoneNumber.Focus();
                return;
            }

            string phoneJid = phoneNumber.Contains("@") ? phoneNumber : $"{phoneNumber}@s.whatsapp.net";

            UpdateStatus($"Requesting to set LID {lidToUpdate} to {phoneJid}...", SystemColors.ControlText);

            var commandData = new JObject
            {
                ["type"] = "manualLidEntry",
                ["lid"] = lidToUpdate,
                ["phoneJid"] = phoneJid
            };

            // Use the SendManagerCommand, passing the additionalData parameter
            bool sent = await SendManagerCommand("manualLidEntry", null, null, commandData);

            if (sent)
            {
                txtManualLidPhoneNumber.Clear();
                // Optionally re-fetch participant list or update UI to reflect potential change
                // A participantDetailsUpdate message from Node.js would handle this if it sends one.
                // For now, relying on Node.js to update the LID cache and then UI will refresh on next fetch.
                UpdateStatus($"LID update request sent for {lidToUpdate}.", Color.Blue);
            }
            else
            {
                UpdateStatus($"Failed to send LID update request for {lidToUpdate}.", Color.Red);
            }
        }

        // Helper method to extract clean phone number from JID
        private string ExtractPhoneNumber(string? jid)
        {
            if (string.IsNullOrEmpty(jid)) return "Unknown";
            var parts = jid.Split('@');
            string numberPart = parts.Length > 0 ? parts[0] : jid;
            return numberPart;
        }

        public void ProcessGroupsDisplayMessage(string messageJson)
        {
            if (this.InvokeRequired) { this.Invoke((MethodInvoker)delegate { ProcessGroupsDisplayMessage(messageJson); }); return; }
            SetButtonsProcessingState(false);
            try
            {
                JObject message = JObject.Parse(messageJson);
                string? type = message["type"]?.ToString();
                if (message["clientId"]?.ToString() != _clientId) return;
                switch (type)
                {
                    case "groupsList": ProcessGroupsList(message); break;
                    case "participantsList": ProcessParticipantsList(message); break;
                    case "addChatToWhitelistResponse":
                    case "removeFromChatWhitelistResponse": ProcessWhitelistResponse(message, type); break;
                    case "error": ProcessError(message); break;
                    default: Console.WriteLine($"GroupsDisplayForm: Unhandled type {type}"); break;
                }
            }
            catch (Exception ex) { UpdateStatus($"Error parsing msg: {ex.Message}", Color.Red); Console.WriteLine($"GroupsDisplayForm Parse Error: {ex}"); }
        }

        public void ProcessParticipantDetailsUpdate(string messageJson)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { ProcessParticipantDetailsUpdate(messageJson); });
                return;
            }
            try
            {
                JObject message = JObject.Parse(messageJson);
                string? originalLid = message["originalLid"]?.ToString();
                string? resolvedPhoneJid = message["resolvedPhoneJid"]?.ToString();
                string? displayName = message["displayName"]?.ToString();

                if (string.IsNullOrEmpty(originalLid) || string.IsNullOrEmpty(resolvedPhoneJid))
                {
                    Console.WriteLine("[GroupsDisplayForm] Received incomplete participantDetailsUpdate (missing originalLid or resolvedPhoneJid).");
                    return;
                }

                bool participantUpdated = false;
                foreach (var pInfo in _allParticipants)
                {
                    if (pInfo.Jid == originalLid)
                    {
                        Console.WriteLine($"[GroupsDisplayForm] Updating ALL_PARTICIPANTS: Original LID={originalLid}, New ResolvedJID={resolvedPhoneJid}, New Name={displayName}");
                        pInfo.ResolvedPhoneJid = resolvedPhoneJid;
                        pInfo.PhoneNumber = ExtractPhoneNumber(resolvedPhoneJid);
                        pInfo.DisplayName = displayName ?? pInfo.DisplayName;
                        participantUpdated = true;
                        break;
                    }
                }

                if (participantUpdated)
                {
                    FilterParticipantsListView(); // Re-apply the current search filter to update the ListView with new data
                    UpdateStatus($"Participant {originalLid.Split('@')[0]} identified as {displayName ?? ExtractPhoneNumber(resolvedPhoneJid)} ({ExtractPhoneNumber(resolvedPhoneJid)}).", Color.Blue);
                }
                else
                {
                    Console.WriteLine($"[GroupsDisplayForm] Participant with originalLid {originalLid} not found in _allParticipants list for update.");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error updating participant: {ex.Message}", Color.Red);
                Console.WriteLine($"[GroupsDisplayForm_ERROR] Processing participantDetailsUpdate: {ex}");
            }
        }

        private void ProcessGroupsList(JObject message)
        {
            JArray? groups = message["groups"] as JArray;
            _allFetchedGroups.Clear();

            if (groups != null)
            {
                foreach (JObject group in groups)
                {
                    _allFetchedGroups.Add(group);
                }
                UpdateStatus($"Received {_allFetchedGroups.Count} groups. Displaying filtered results.", SystemColors.ControlText);
            }
            else
            {
                UpdateStatus("No groups found from manager.", Color.Orange);
            }
            FilterGroupsListView(); // Apply current filter
        }


        private readonly DatabaseConnection _db = new DatabaseConnection();

        // private async Task<int?> GetBotInstanceId()
        // {
        //     var result = await _db.QuerySingleAsync<dynamic>(
        //         "SELECT id FROM bot_instances WHERE client_id = @clientId",
        //         new { clientId = _clientId }
        //     );
        //     return result?.id;
        // }

        // Update ProcessParticipantsList to check database for whitelist status
        // private async void ProcessParticipantsList(JObject message)
        // {
        //     participantsListView.Items.Clear();
        //     _allParticipants.Clear();

        //     JArray? participants = message["participants"] as JArray;
        //     string? forGroupId = message["groupId"]?.ToString();
        //     string groupName = groupsListView.Items.Cast<ListViewItem>()
        //                        .FirstOrDefault(it => it.Tag?.ToString() == forGroupId)?.SubItems[1].Text ?? forGroupId?.Split('@')[0] ?? "Group";

        //     var botInstanceId = await GetBotInstanceId();
        //     if (!botInstanceId.HasValue) return;

        //     // Get all whitelisted users for this bot instance
        //     var whitelistedUsers = await _db.QueryAsync<string>(
        //         "SELECT user_jid FROM whitelisted_users WHERE bot_instance_id = @botId AND api_active = true",
        //         new { botId = botInstanceId.Value }
        //     );
        //     var whitelistedSet = new HashSet<string>(whitelistedUsers);

        //     if (participants != null && participants.Count > 0)
        //     {
        //         foreach (JObject p in participants)
        //         {
        //             string? jid = p["jid"]?.ToString();
        //             if (string.IsNullOrEmpty(jid)) continue;

        //             string? resolvedJidPayload = p["resolvedJid"]?.ToString();
        //             string phoneToUse = ExtractPhoneNumber(resolvedJidPayload ?? jid);

        //             // Check whitelist status from database
        //             bool isWhitelisted = whitelistedSet.Contains(jid);

        //             // If it's a LID and we have a resolved JID, check that too
        //             if (!isWhitelisted && !string.IsNullOrEmpty(resolvedJidPayload))
        //             {
        //                 isWhitelisted = whitelistedSet.Contains(resolvedJidPayload);
        //             }

        //             ParticipantInfo newPInfo = new ParticipantInfo
        //             {
        //                 Jid = jid,
        //                 ResolvedPhoneJid = resolvedJidPayload,
        //                 PhoneNumber = phoneToUse,
        //                 DisplayName = p["displayName"]?.ToString() ?? phoneToUse,
        //                 IsAdmin = p["isAdmin"]?.ToObject<bool>() ?? false,
        //                 IsWhitelisted = isWhitelisted
        //             };
        //             _allParticipants.Add(newPInfo);
        //         }

        //         _allParticipants = _allParticipants
        //             .OrderByDescending(p => p.IsAdmin)
        //             .ThenByDescending(p => p.IsWhitelisted)
        //             .ThenBy(p => p.DisplayName)
        //             .ToList();

        //         UpdateStatus($"Loaded {_allParticipants.Count} participants for {groupName}.", SystemColors.ControlText);
        //     }

        //     else
        //     {
        //         UpdateStatus($"No participants found for {groupName}.", Color.Orange);
        //         lblParticipantsTitle.Text = $"No participants for \"{groupName}\"";
        //     }

        //     lblParticipantsTitle.Text = $"Participants for \"{groupName}\""; // Update title with group name
        //     lblParticipantsCount.Text = $"Total: {_allParticipants.Count}"; // Show total count
        //     txtSearchParticipant.Clear(); // Clear search box after new data fetch to show all
        //     FilterParticipantsListView(); // Populate ListView with filtered (or all) participants

        // }

        // Add method to update whitelist status in database
        private async Task UpdateWhitelistInDatabase(string jid, bool isWhitelisted)
        {
            var botInstanceId = await GetBotInstanceId();
            if (!botInstanceId.HasValue) return;

            if (jid.EndsWith("@g.us"))
            {
                if (isWhitelisted)
                {
                    await _db.ExecuteAsync(@"
                INSERT INTO whitelisted_groups (bot_instance_id, group_jid)
                VALUES (@botId, @jid)
                ON CONFLICT (bot_instance_id, group_jid) 
                DO UPDATE SET is_active = true, updated_at = CURRENT_TIMESTAMP",
                        new { botId = botInstanceId.Value, jid });
                }
                else
                {
                    await _db.ExecuteAsync(@"
                UPDATE whitelisted_groups 
                SET is_active = false, updated_at = CURRENT_TIMESTAMP
                WHERE bot_instance_id = @botId AND group_jid = @jid",
                        new { botId = botInstanceId.Value, jid });
                }
            }
            else
            {
                if (isWhitelisted)
                {
                    await _db.ExecuteAsync(@"
                INSERT INTO whitelisted_users (bot_instance_id, user_jid, phone_number)
                VALUES (@botId, @jid, @phone)
                ON CONFLICT (bot_instance_id, user_jid) 
                DO UPDATE SET api_active = true, updated_at = CURRENT_TIMESTAMP",
                        new { botId = botInstanceId.Value, jid, phone = jid.Split('@')[0] });
                }
                else
                {
                    await _db.ExecuteAsync(@"
                UPDATE whitelisted_users 
                SET api_active = false, updated_at = CURRENT_TIMESTAMP
                WHERE bot_instance_id = @botId AND user_jid = @jid",
                        new { botId = botInstanceId.Value, jid });
                }
            }
        }
        // In GroupsDisplayForm.cs

        private void ProcessWhitelistResponse(JObject message, string responseType)
        {
            bool success = message["success"]?.ToObject<bool>() ?? false;
            string? jid = message["jid"]?.ToString();
            string? itemType = message["typeOfItem"]?.ToString();

            if (string.IsNullOrEmpty(jid)) return; // Can't do anything without the JID

            if (success)
            {
                bool isAdding = responseType.Contains("add");
                string actionText = isAdding ? "whitelisted" : "removed from whitelist";

                if (itemType == "group")
                {
                    // Find the group in the ListView
                    ListViewItem? groupItem = groupsListView.Items.Cast<ListViewItem>()
                                               .FirstOrDefault(it => it.Tag?.ToString() == jid);

                    if (groupItem != null)
                    {
                        // **Directly update the UI without a full refresh**
                        groupItem.SubItems[3].Text = isAdding ? "Yes" : "No";
                        groupItem.BackColor = isAdding ? Color.LightGreen : Color.White;

                        // Update the button text if this group is still selected
                        if (groupItem.Selected)
                        {
                            btnWhitelistGroup.Text = isAdding ? "Remove Whitelist" : "Add Whitelist";
                        }
                    }
                    else
                    {
                        // If not found (e.g., due to filtering), we can still trigger a less disruptive fetch
                        btnFetchGroups_Click(null, null);
                    }

                    UpdateStatus($"Successfully {actionText} group '{groupItem?.SubItems[1].Text ?? jid.Split('@')[0]}'.", Color.DarkGreen);
                }
                else if (itemType == "user")
                {
                    // Find the participant in the main data list (_allParticipants)
                    var participant = _allParticipants.FirstOrDefault(p => p.Jid == jid);
                    if (participant != null)
                    {
                        // Update the data source
                        participant.IsWhitelisted = isAdding;

                        // Find the participant in the visible ListView
                        ListViewItem? participantItem = participantsListView.Items.Cast<ListViewItem>()
                                                        .FirstOrDefault(it => (it.Tag as ParticipantInfo)?.Jid == jid);
                        if (participantItem != null)
                        {
                            // Update the visible item directly
                            participantItem.SubItems[3].Text = isAdding ? "Yes" : "No";

                            // Force a redraw to apply the new color defined in DrawSubItem
                            participantsListView.Invalidate(participantItem.Bounds);
                        }
                        UpdateStatus($"Successfully {actionText} participant '{participant.DisplayName}'.", Color.DarkGreen);
                    }
                }
            }
            else // Handle failure
            {
                string reason = message["reason"]?.ToString() ?? "Unknown error";
                string actionFailed = responseType.Contains("add") ? "whitelist" : "remove from whitelist";
                MessageBox.Show($"Failed to {actionFailed} {itemType} '{jid.Split('@')[0]}': {reason}", "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpdateStatus($"Failed: {reason}", Color.Red);
            }
        }

        private void ProcessError(JObject message)
        {
            string errorMsg = message["message"]?.ToString() ?? "Unknown server error.";
            MessageBox.Show($"Server error: {errorMsg}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus($"Error: {errorMsg}", Color.Red);
        }

        private void UpdateStatus(string message, Color color)
        {
            if (this.InvokeRequired) { this.Invoke((MethodInvoker)delegate { UpdateStatus(message, color); }); return; }
            labelSelectedClient.Text = $"Client: {(_clientPhoneNumber.Length > 5 ? _clientPhoneNumber.Substring(0, 5) + "..." : _clientPhoneNumber)} - {message}";
            labelSelectedClient.ForeColor = color;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
    }
}
