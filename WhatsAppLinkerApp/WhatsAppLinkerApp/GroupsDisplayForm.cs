// GroupsDisplayForm.cs - Modified to work with ListView for participants
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
using System.IO; // Used for export functionality
using System.Security.Cryptography; // For DPAPI in Form1.cs (ensure it's in the project if needed)

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

        // Keep ToString() for debugging or if ListBox is used elsewhere,
        // but ListView items are populated via SubItems directly.
        public override string ToString()
        {
            // This is primarily for debugging or if ListBox is ever re-introduced.
            // ListView uses SubItems directly.
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
        private readonly string _clientPhoneNumber;
        private bool _isProcessingCommand = false;

        private List<JObject> _allFetchedGroups = new List<JObject>();
        private List<ParticipantInfo> _allParticipants = new List<ParticipantInfo>(); // Store all participants for filtering
        // No longer need _filteredParticipants as a field, filter directly into ListView

        public GroupsDisplayForm(ClientWebSocket sharedWsClient, CancellationTokenSource sharedCts, string clientId, string clientPhoneNumber)
        {
            InitializeComponent(); // This MUST be the first call in the constructor.

            _sharedWebSocketClient = sharedWsClient ?? throw new ArgumentNullException(nameof(sharedWsClient));
            _sharedCancellationTokenSource = sharedCts ?? throw new ArgumentNullException(nameof(sharedCts));
            _clientId = clientId;
            _clientPhoneNumber = clientPhoneNumber;

            SetupFormInitialState(); // Rename from SetupForm for clarity
            ApplyGroupFormStyles();
            CheckWebSocketConnection();
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

        private async Task<bool> SendManagerCommand(string type, string? groupId = null, string? participantJid = null)
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
                var request = new JObject { ["type"] = type, ["clientId"] = _clientId };
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
            toolStripButtonWhitelist.Enabled = canEnable && participantsListView.SelectedItems.Count > 0; // Corrected
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
                bool isWhitelisted = group["isWhitelisted"]?.ToObject<bool>() ?? false;
                item.SubItems.Add(isWhitelisted ? "Yes" : "No");
                if (isWhitelisted) item.BackColor = Color.LightGreen;
                groupsListView.Items.Add(item);
            }
            groupsListView.EndUpdate();

            if (groupsListView.Items.Count > 0)
            {
                groupsListView.Items[0].Selected = true; // Select the first item after filtering
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
            // Manually trigger SelectedIndexChanged to update participant list if a group is selected
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
            participantsListView.Items.Clear(); // This line was missing EndUpdate()

            if (_allParticipants == null || _allParticipants.Count == 0)
            {
                participantsListView.EndUpdate();
                lblParticipantsCount.Text = "Total: 0";
                return;
            }

            // Filter participants based on search text
            // _filteredParticipants is no longer a field, it's a local variable for filtering
            var currentFilteredList = string.IsNullOrWhiteSpace(searchText)
                ? _allParticipants.ToList() // If search is empty, show all
                : _allParticipants.Where(p =>
                    p.DisplayName.ToLowerInvariant().Contains(searchText) ||
                    p.PhoneNumber.ToLowerInvariant().Contains(searchText) ||
                    (p.ResolvedPhoneJid?.ToLowerInvariant().Contains(searchText) ?? false) ||
                    p.Jid.ToLowerInvariant().Contains(searchText) // Also search in original JID
                  ).ToList();

            // Populate participantsListView from currentFilteredList
            foreach (ParticipantInfo pInfo in currentFilteredList)
            {
                ListViewItem item = new ListViewItem(pInfo.DisplayName);
                item.SubItems.Add(pInfo.PhoneNumber);
                item.SubItems.Add(pInfo.IsAdmin ? "Admin" : "Member");
                item.SubItems.Add(pInfo.IsWhitelisted ? "Yes" : "No");
                item.Tag = pInfo; // Store the ParticipantInfo object in Tag
                participantsListView.Items.Add(item);
            }
            participantsListView.EndUpdate(); // Corrected: Added this line

            lblParticipantsCount.Text = $"Total: {participantsListView.Items.Count}";

            if (participantsListView.Items.Count > 0)
            {
                participantsListView.Items[0].Selected = true;
                participantsListView.Items[0].Focused = true;
            }
            else
            {
                // Disable participant-specific buttons if no participants are visible
                toolStripButtonWhitelist.Enabled = false;
            }
            // Trigger SelectedIndexChanged to update toolStripButtonWhitelist text
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
            }
            else
            {
                toolStripButtonWhitelist.Text = "Add Whitelist"; // Default when no selection
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

                            // Use _allParticipants, then filter in memory for export or use current displayed list
                            // Given the current implementation, _filteredParticipants is not a field.
                            // So, we need to apply the filter logic again for export based on the current search text
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
            // Optional: Double-click to toggle whitelist or copy info
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

        // Context menu opening event to enable/disable items based on selection
        private void participantsContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            bool hasSelection = participantsListView.SelectedItems.Count > 0;
            copyPhoneNumberToolStripMenuItem.Enabled = hasSelection;
            copyNameToolStripMenuItem.Enabled = hasSelection;
            whitelistToolStripMenuItem.Enabled = hasSelection;

            if (hasSelection && participantsListView.SelectedItems[0].Tag is ParticipantInfo pInfo)
            {
                whitelistToolStripMenuItem.Text = pInfo.IsWhitelisted ? "Remove Whitelist" : "Add Whitelist";
            } else {
                whitelistToolStripMenuItem.Text = "Toggle Whitelist"; // Default text
            }
        }


        // Helper method to extract clean phone number from JID
        private string ExtractPhoneNumber(string? jid)
        {
            if (string.IsNullOrEmpty(jid)) return "Unknown";
            // A JID can be '1234567890@s.whatsapp.net' or '12345@lid'
            var parts = jid.Split('@');
            string numberPart = parts.Length > 0 ? parts[0] : jid;
            // Optionally, remove country code if it's the default one to make it cleaner for display
            // For example, if DEFAULT_PHONE_COUNTRY_CODE is '967'
            // if (numberPart.StartsWith("967") && numberPart.Length > 9) return numberPart.Substring(3);
            return numberPart; // Return as is for now
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
                // We're not directly receiving newWhitelistStatus from Node.js in this specific message type,
                // but if the API sync happens, the next fetchParticipants would update it.
                // For a more immediate update, the Node.js side would need to send isWhitelisted: bool in this message.

                if (string.IsNullOrEmpty(originalLid) || string.IsNullOrEmpty(resolvedPhoneJid))
                {
                    Console.WriteLine("[GroupsDisplayForm] Received incomplete participantDetailsUpdate (missing originalLid or resolvedPhoneJid).");
                    return;
                }

                bool participantUpdated = false;
                // Update _allParticipants list
                foreach (var pInfo in _allParticipants)
                {
                    if (pInfo.Jid == originalLid)
                    {
                        Console.WriteLine($"[GroupsDisplayForm] Updating ALL_PARTICIPANTS: Original LID={originalLid}, New ResolvedJID={resolvedPhoneJid}, New Name={displayName}");
                        pInfo.ResolvedPhoneJid = resolvedPhoneJid;
                        pInfo.PhoneNumber = ExtractPhoneNumber(resolvedPhoneJid);
                        pInfo.DisplayName = displayName ?? pInfo.DisplayName;
                        // To reflect whitelist status change from LID resolution, you'd need the node.js side to include it
                        // or re-fetch participants list entirely after a short delay to allow API sync to complete on Node.js side.
                        // For now, we only update display info.
                        participantUpdated = true;
                        break;
                    }
                }

                if (participantUpdated)
                {
                    // Re-apply the current search filter to update the ListView with new data
                    FilterParticipantsListView();
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

        private void ProcessParticipantsList(JObject message)
        {
            participantsListView.Items.Clear(); // Clear current display
            _allParticipants.Clear(); // Clear the full list

            JArray? participants = message["participants"] as JArray;
            string? forGroupId = message["groupId"]?.ToString();
            string groupName = groupsListView.Items.Cast<ListViewItem>()
                               .FirstOrDefault(it => it.Tag?.ToString() == forGroupId)?.SubItems[1].Text ?? forGroupId?.Split('@')[0] ?? "Group";

            if (participants != null && participants.Count > 0)
            {
                foreach (JObject p in participants)
                {
                    string? jid = p["jid"]?.ToString();
                    if (string.IsNullOrEmpty(jid)) continue;

                    string? resolvedJidPayload = p["resolvedJid"]?.ToString();
                    string phoneToUse = ExtractPhoneNumber(resolvedJidPayload ?? jid);

                    ParticipantInfo newPInfo = new ParticipantInfo
                    {
                        Jid = jid,
                        ResolvedPhoneJid = resolvedJidPayload,
                        PhoneNumber = phoneToUse,
                        DisplayName = p["displayName"]?.ToString() ?? phoneToUse,
                        IsAdmin = p["isAdmin"]?.ToObject<bool>() ?? false,
                        IsWhitelisted = p["isWhitelisted"]?.ToObject<bool>() ?? false
                    };
                    _allParticipants.Add(newPInfo);
                }

                // Sort all participants before filtering
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

            lblParticipantsTitle.Text = $"Participants for \"{groupName}\""; // Update title with group name
            lblParticipantsCount.Text = $"Total: {_allParticipants.Count}"; // Show total count
            txtSearchParticipant.Clear(); // Clear search box after new data fetch to show all
            FilterParticipantsListView(); // Populate ListView with filtered (or all) participants
        }

        private void ProcessWhitelistResponse(JObject message, string responseType)
        {
            bool success = message["success"]?.ToObject<bool>() ?? false;
            string reason = message["reason"]?.ToString() ?? "Error";
            string? jid = message["jid"]?.ToString();
            string itemType = message["typeOfItem"]?.ToString() ?? "item";
            string action = responseType.Contains("add") ? "whitelisted" : "removed from whitelist";

            if (success)
            {
                UpdateStatus($"Successfully {action} {itemType} '{jid?.Split('@')[0]}'.", Color.DarkGreen);
                if (itemType == "group")
                {
                    // Refresh groups list
                    btnFetchGroups_Click(null, null);
                }
                else if (itemType == "user")
                {
                    // Update the whitelist status in _allParticipants and then re-filter
                    var updatedParticipant = _allParticipants.FirstOrDefault(p => p.Jid == jid || p.ResolvedPhoneJid == jid);
                    if (updatedParticipant != null)
                    {
                        updatedParticipant.IsWhitelisted = responseType.Contains("add");
                        FilterParticipantsListView(); // Re-filter to show updated status
                        UpdateStatus($"Participant {updatedParticipant.DisplayName} whitelist status updated.", Color.Blue);
                    }
                    else
                    {
                        // If participant not found in current list, maybe re-fetch or just update status label
                        // For robustness, re-fetching participants for the current group
                        if (groupsListView.SelectedItems.Count > 0 && groupsListView.SelectedItems[0].Tag?.ToString() != null)
                        {
                            string currentGroupId = groupsListView.SelectedItems[0].Tag.ToString()!;
                            Task.Run(async () =>
                            {
                                await Task.Delay(100); // Small delay to allow manager to process
                                await SendManagerCommand("fetchParticipants", currentGroupId);
                            });
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show($"Failed to {action} {itemType} '{jid?.Split('@')[0]}': {reason}", "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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