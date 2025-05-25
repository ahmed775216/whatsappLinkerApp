// GroupsDisplayForm.cs - Enhanced Version
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
    // Helper class to store participant data
    public class ParticipantInfo
    {
        public string Jid { get; set; }
        public string DisplayName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsWhitelisted { get; set; }

        public override string ToString()
        {
            var adminText = IsAdmin ? "Admin" : "Member";
            var whitelistText = IsWhitelisted ? "✓ Whitelisted" : "✗ Not Whitelisted";
            var displayText = !string.IsNullOrEmpty(DisplayName) && DisplayName != PhoneNumber 
                ? $"{DisplayName} ({PhoneNumber})" 
                : PhoneNumber;
            
            return $"{displayText} - {adminText}, {whitelistText}";
        }
    }

    public partial class GroupsDisplayForm : Form
    {
        private ClientWebSocket _sharedWebSocketClient;
        private CancellationTokenSource _sharedCancellationTokenSource;
        private string _clientId;
        private string _clientPhoneNumber;
        private Dictionary<string, string> _groupParticipantCounts = new Dictionary<string, string>();
        private bool _isProcessingCommand = false;

        public GroupsDisplayForm(ClientWebSocket sharedWsClient, CancellationTokenSource sharedCts, string clientId, string clientPhoneNumber)
        {
            InitializeComponent();
            _sharedWebSocketClient = sharedWsClient;
            _sharedCancellationTokenSource = sharedCts;
            _clientId = clientId;
            _clientPhoneNumber = clientPhoneNumber;
            labelSelectedClient.Text = $"Groups for Client: {_clientPhoneNumber} ({_clientId})";

            // Set column widths explicitly
            groupsListView.Columns[0].Width = 140; // Group ID
            groupsListView.Columns[1].Width = 150; // Group Name (increased)
            groupsListView.Columns[2].Width = 70;  // Members
            groupsListView.Columns[3].Width = 90;  // Whitelisted (increased)

            ApplyGroupFormStyles();
            
            // Check WebSocket connection status
            CheckWebSocketConnection();
        }

        private void CheckWebSocketConnection()
        {
            if (_sharedWebSocketClient?.State != WebSocketState.Open)
            {
                UpdateStatus("WebSocket connection lost. Please reconnect.", Color.Red);
                DisableAllButtons();
            }
        }

        private void DisableAllButtons()
        {
            btnFetchGroups.Enabled = false;
            btnWhitelistGroup.Enabled = false;
            btnFetchParticipants.Enabled = false;
            btnWhitelistParticipant.Enabled = false;
        }

        private void EnableFetchButton()
        {
            btnFetchGroups.Enabled = true;
        }

        private void ApplyGroupFormStyles()
        {
            groupsListView.OwnerDraw = true;
            groupsListView.DrawColumnHeader += (s, e) =>
            {
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var headerBrush = new SolidBrush(Color.FromArgb(18, 140, 126))) // Dark Green
                using (var borderPen = new Pen(Color.FromArgb(12, 100, 90), 1))
                {
                    e.Graphics.FillRectangle(headerBrush, e.Bounds);
                    e.Graphics.DrawRectangle(borderPen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                    TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, Color.White, 
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
            };
            groupsListView.DrawSubItem += (s, e) => e.DrawDefault = true;

            // Custom drawing for ListBox to show whitelisted status with color
            participantsListBox.DrawMode = DrawMode.OwnerDrawFixed;
            participantsListBox.DrawItem += participantsListBox_DrawItem;
        }

        private void GroupsDisplayForm_Load(object sender, EventArgs e)
        {
            if (_sharedWebSocketClient?.State == WebSocketState.Open)
            {
                EnableFetchButton();
                btnFetchGroups_Click(null, null);
            }
            else
            {
                UpdateStatus("WebSocket not connected. Cannot fetch groups.", Color.Red);
            }
        }

        private async Task<bool> SendManagerCommand(string type, string groupId = null, string participantJid = null)
        {
            if (_isProcessingCommand)
            {
                UpdateStatus("Please wait, processing previous command...", Color.Orange);
                return false;
            }

            if (_sharedWebSocketClient == null || _sharedWebSocketClient.State != WebSocketState.Open)
            {
                MessageBox.Show("Connection to manager lost. Please restart the application.", "Connection Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Connection lost", Color.Red);
                return false;
            }

            _isProcessingCommand = true;
            
            try
            {
                var request = new JObject
                {
                    ["type"] = type,
                    ["clientId"] = _clientId
                };
                
                if (groupId != null) request["groupId"] = groupId;
                if (participantJid != null) request["participantJid"] = participantJid;

                var buffer = Encoding.UTF8.GetBytes(request.ToString());
                await _sharedWebSocketClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _sharedCancellationTokenSource.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Operation was cancelled", Color.Orange);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send command: {ex.Message}", "Communication Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Command failed: {ex.Message}", Color.Red);
                return false;
            }
            finally
            {
                _isProcessingCommand = false;
            }
        }

        private async void btnFetchGroups_Click(object sender, EventArgs e)
        {
            groupsListView.Items.Clear();
            participantsListBox.Items.Clear();
            btnWhitelistGroup.Enabled = false;
            btnFetchParticipants.Enabled = false;
            btnWhitelistParticipant.Enabled = false;
            
            UpdateStatus("Fetching groups...", SystemColors.ControlText);
            
            if (await SendManagerCommand("fetchGroups"))
            {
                // Command sent successfully, wait for response
            }
            else
            {
                UpdateStatus("Failed to send fetch groups command", Color.Red);
            }
        }

        private async void btnWhitelistGroup_Click(object sender, EventArgs e)
        {
            if (groupsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a group.", "No Group Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ListViewItem selectedGroupItem = groupsListView.SelectedItems[0];
            string groupId = selectedGroupItem.Tag?.ToString();
            string groupName = selectedGroupItem.SubItems[1].Text;
            bool currentWhitelistStatus = selectedGroupItem.SubItems[3].Text.Equals("Yes", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(groupId))
            {
                MessageBox.Show("Invalid group selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Confirm action with user
            string action = currentWhitelistStatus ? "remove from whitelist" : "add to whitelist";
            DialogResult result = MessageBox.Show(
                $"Are you sure you want to {action} the group \"{groupName}\"?",
                "Confirm Action",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            string command = currentWhitelistStatus ? "removeFromChatWhitelist" : "addChatToWhitelist";
            UpdateStatus($"Processing {action} for group \"{groupName}\"...", SystemColors.ControlText);
            
            if (await SendManagerCommand(command, groupId))
            {
                // Command sent, wait for response
            }
        }

        private async void btnFetchParticipants_Click(object sender, EventArgs e)
        {
            if (groupsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a group.", "No Group Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string groupId = groupsListView.SelectedItems[0].Tag?.ToString();
            string groupName = groupsListView.SelectedItems[0].SubItems[1].Text;
            
            if (string.IsNullOrEmpty(groupId))
            {
                MessageBox.Show("Invalid group selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            participantsListBox.Items.Clear();
            participantsListBox.Items.Add($"Fetching participants for \"{groupName}\"...");
            btnWhitelistParticipant.Enabled = false;
            UpdateStatus("Fetching participants...", SystemColors.ControlText);
            
            if (await SendManagerCommand("fetchParticipants", groupId))
            {
                // Command sent, wait for response
            }
        }

        private async void btnWhitelistParticipant_Click(object sender, EventArgs e)
        {
            if (!(participantsListBox.SelectedItem is ParticipantInfo selectedParticipant))
            {
                MessageBox.Show("Please select a participant.", "No Participant Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string action = selectedParticipant.IsWhitelisted ? "remove from whitelist" : "add to whitelist";
            DialogResult result = MessageBox.Show(
                $"Are you sure you want to {action} participant \"{selectedParticipant.DisplayName}\"?",
                "Confirm Action",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            string command = selectedParticipant.IsWhitelisted ? "removeFromChatWhitelist" : "addChatToWhitelist";
            UpdateStatus($"Processing {action} for participant {selectedParticipant.DisplayName}...", SystemColors.ControlText);
            
            if (await SendManagerCommand(command, null, selectedParticipant.Jid))
            {
                // Command sent, wait for response
            }
        }

        private void groupsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isGroupSelected = groupsListView.SelectedItems.Count > 0;
            btnWhitelistGroup.Enabled = isGroupSelected && !_isProcessingCommand;
            btnFetchParticipants.Enabled = isGroupSelected && !_isProcessingCommand;
            participantsListBox.Items.Clear();
            btnWhitelistParticipant.Enabled = false;

            if (isGroupSelected)
            {
                bool isWhitelisted = groupsListView.SelectedItems[0].SubItems[3].Text.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                btnWhitelistGroup.Text = isWhitelisted ? "Remove from Whitelist" : "Add to Whitelist";
            }
        }

        private void participantsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isParticipantSelected = participantsListBox.SelectedItem is ParticipantInfo;
            btnWhitelistParticipant.Enabled = isParticipantSelected && !_isProcessingCommand;
            
            if (participantsListBox.SelectedItem is ParticipantInfo pInfo)
            {
                btnWhitelistParticipant.Text = pInfo.IsWhitelisted ? "Remove from Whitelist" : "Add to Whitelist";
            }
        }

        private void participantsListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            var item = participantsListBox.Items[e.Index];
            if (!(item is ParticipantInfo participant))
            {
                // Draw non-participant items (like "Fetching..." or headers) in gray
                e.Graphics.DrawString(item.ToString(), e.Font, Brushes.DarkGray, e.Bounds, StringFormat.GenericDefault);
                e.DrawFocusRectangle();
                return;
            }

            // Determine colors based on status
            Brush textBrush;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                textBrush = SystemBrushes.HighlightText;
            }
            else if (participant.IsWhitelisted)
            {
                textBrush = Brushes.DarkGreen;
            }
            else if (participant.IsAdmin)
            {
                textBrush = Brushes.DarkBlue;
            }
            else
            {
                textBrush = Brushes.Black;
            }

            // Draw the participant text
            e.Graphics.DrawString(participant.ToString(), e.Font, textBrush, e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }

        private string ExtractPhoneNumber(string jid)
        {
            if (string.IsNullOrEmpty(jid)) return "Unknown";
            
            // Extract phone number from JID (format: phone@domain)
            var parts = jid.Split('@');
            return parts.Length > 0 ? parts[0] : jid;
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 10)
                return phoneNumber;

            // Basic phone number formatting (you can enhance this based on your needs)
            if (phoneNumber.Length == 12 && phoneNumber.StartsWith("1"))
            {
                // US number format: +1 (XXX) XXX-XXXX
                return $"+1 ({phoneNumber.Substring(1, 3)}) {phoneNumber.Substring(4, 3)}-{phoneNumber.Substring(7)}";
            }
            else if (phoneNumber.Length >= 10)
            {
                // International format: +XXX XXX XXX XXXX
                return $"+{phoneNumber}";
            }
            
            return phoneNumber;
        }

        public void ProcessGroupsDisplayMessage(string messageJson)
        {
            this.Invoke((MethodInvoker)async delegate
            {
                try
                {
                    JObject message = JObject.Parse(messageJson);
                    string type = message["type"]?.ToString();
                    string msgClientId = message["clientId"]?.ToString();

                    if (msgClientId != _clientId) return;

                    switch (type)
                    {
                        case "groupsList":
                            ProcessGroupsList(message);
                            break;

                        case "participantsList":
                            ProcessParticipantsList(message);
                            break;

                        case "addChatToWhitelistResponse":
                        case "removeFromChatWhitelistResponse":
                            await ProcessWhitelistResponse(message, type);
                            break;

                        case "error":
                            ProcessError(message);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Error processing message: {ex.Message}", Color.Red);
                }
            });
        }

        private void ProcessGroupsList(JObject message)
        {
            groupsListView.Items.Clear();
            JArray groups = message["groups"] as JArray;
            
            if (groups != null && groups.Any())
            {
                foreach (JObject group in groups)
                {
                    ListViewItem item = new ListViewItem(group["id"]?.ToString() ?? "Unknown");
                    item.SubItems.Add(group["subject"]?.ToString() ?? "Unnamed Group");
                    
                    string participantsCount = group["participantsCount"]?.ToString() ?? "N/A";
                    item.SubItems.Add(participantsCount);
                    
                    bool isWhitelisted = group["isWhitelisted"]?.ToObject<bool>() == true;
                    item.SubItems.Add(isWhitelisted ? "Yes" : "No");
                    
                    item.Tag = group["id"]?.ToString();
                    
                    // Color code the row based on whitelist status
                    if (isWhitelisted)
                    {
                        item.BackColor = Color.LightGreen;
                    }
                    
                    groupsListView.Items.Add(item);
                }
                
                UpdateStatus($"Found {groups.Count} groups.", Color.Green);
                
                if (groupsListView.Items.Count > 0)
                {
                    groupsListView.Items[0].Selected = true;
                    groupsListView.Focus();
                }
            }
            else
            {
                UpdateStatus("No groups found.", Color.Orange);
            }
        }

        private void ProcessParticipantsList(JObject message)
        {
            participantsListBox.Items.Clear();
            JArray participants = message["participants"] as JArray;
            string forGroupId = message["groupId"]?.ToString();
            string groupName = groupsListView.Items.Cast<ListViewItem>()
                               .FirstOrDefault(it => it.Tag?.ToString() == forGroupId)?.SubItems[1].Text ?? forGroupId;

            if (participants != null && participants.Any())
            {
                participantsListBox.Items.Add($"=== Participants for \"{groupName}\" ({participants.Count} members) ===");
                
                var participantList = new List<ParticipantInfo>();
                
                foreach (JObject p in participants)
                {
                    string jid = p["jid"]?.ToString();
                    string phoneNumber = ExtractPhoneNumber(jid);
                    string formattedPhone = FormatPhoneNumber(phoneNumber);
                    
                    var participant = new ParticipantInfo
                    {
                        Jid = jid,
                        PhoneNumber = phoneNumber,
                        DisplayName = p["displayName"]?.ToString() ?? formattedPhone,
                        IsAdmin = p["isAdmin"]?.ToObject<bool>() == true,
                        IsWhitelisted = p["isWhitelisted"]?.ToObject<bool>() == true
                    };
                    
                    participantList.Add(participant);
                }
                
                // Sort participants: Admins first, then by whitelist status, then by name
                var sortedParticipants = participantList
                    .OrderByDescending(p => p.IsAdmin)
                    .ThenByDescending(p => p.IsWhitelisted)
                    .ThenBy(p => p.DisplayName);
                
                foreach (var participant in sortedParticipants)
                {
                    participantsListBox.Items.Add(participant);
                }
                
                UpdateStatus($"Loaded {participants.Count} participants for {groupName}.", Color.Green);
                
                if (participantsListBox.Items.Count > 1)
                {
                    participantsListBox.SelectedIndex = 1; // Select first actual participant
                }
            }
            else
            {
                participantsListBox.Items.Add($"No participants found for \"{groupName}\".");
                UpdateStatus($"No participants found for {groupName}.", Color.Orange);
            }
        }

        private async Task ProcessWhitelistResponse(JObject message, string responseType)
        {
            bool success = message["success"]?.ToObject<bool>() == true;
            string reason = message["reason"]?.ToString() ?? "Unknown error";
            string jid = message["jid"]?.ToString() ?? "Unknown";
            string itemType = message["typeOfItem"]?.ToString() ?? "item";

            string action = responseType == "addChatToWhitelistResponse" ? "whitelisting" : "un-whitelisting";
            string pastAction = responseType == "addChatToWhitelistResponse" ? "whitelisted" : "un-whitelisted";

            if (success)
            {
                string displayName = jid.Split('@')[0];
                UpdateStatus($"Successfully {pastAction} {itemType}: {displayName}", Color.Green);
                
                // Refresh the appropriate list
                if (itemType == "group")
                {
                    await SendManagerCommand("fetchGroups");
                }
                else if (itemType == "user" && groupsListView.SelectedItems.Count > 0)
                {
                    // Refresh participants for the current group
                    string currentGroupId = groupsListView.SelectedItems[0].Tag?.ToString();
                    if (!string.IsNullOrEmpty(currentGroupId))
                    {
                        await SendManagerCommand("fetchParticipants", currentGroupId);
                    }
                }
            }
            else
            {
                string displayName = jid.Split('@')[0];
                string errorMessage = $"Failed {action} {itemType} {displayName}: {reason}";
                MessageBox.Show(errorMessage, "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpdateStatus(errorMessage, Color.Red);
            }
        }

        private void ProcessError(JObject message)
        {
            string errorMsg = message["message"]?.ToString() ?? "Unknown error occurred";
            MessageBox.Show($"Server error: {errorMsg}", "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus($"Server error: {errorMsg}", Color.Red);
        }

        private void UpdateStatus(string message, Color color)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { UpdateStatus(message, color); });
                return;
            }

            labelSelectedClient.Text = $"Client: {_clientPhoneNumber} ({_clientId}) - {message}";
            labelSelectedClient.ForeColor = color;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isProcessingCommand = false;
            base.OnFormClosing(e);
        }
    }
}