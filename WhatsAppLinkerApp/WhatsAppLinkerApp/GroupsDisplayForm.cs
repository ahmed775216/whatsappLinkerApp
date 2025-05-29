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
    public class ParticipantInfo
    {
        public string Jid { get; set; } // المعرف الأصلي (قد يكون @lid أو رقم هاتف JID)
        public string? ResolvedPhoneJid { get; set; } // رقم الهاتف JID المحلول (إذا كان Jid هو @lid وتم حله)
        public string DisplayName { get; set; } // اسم العرض (يجب أن يكون pushName من Node.js)
        public string PhoneNumber { get; set; } // الرقم المستخلص للعرض (من ResolvedPhoneJid أو Jid)
        public bool IsAdmin { get; set; }
        public bool IsWhitelisted { get; set; } // يجب أن تعكس الحالة بناءً على ResolvedPhoneJid

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
            string phoneToUse = PhoneNumber; // هذا يجب أن يكون الرقم النظيف

            // إذا كان اسم العرض فارغًا، أو يبدو كـ JID، أو مطابق لرقم الهاتف، استخدم رقم الهاتف فقط كاسم رئيسي.
            if (string.IsNullOrEmpty(nameToUse) || 
                nameToUse == phoneToUse || 
                nameToUse.Contains("@") || // heuristic for JID-like names
                (ResolvedPhoneJid != null && nameToUse == ResolvedPhoneJid.Split('@')[0])) // if name is just the number part of resolved JID
            {
                nameToUse = phoneToUse;
                 return $"{nameToUse} - {adminText}, {whitelistText}";
            }
            // إذا كان هناك اسم عرض صالح ورقم هاتف، اعرضهما معًا.
            else if (!string.IsNullOrEmpty(phoneToUse))
            {
                 return $"{nameToUse} ({phoneToUse}) - {adminText}, {whitelistText}";
            }
            // إذا كان Jid الأصلي هو @lid ولم يتم حله بعد (لا يوجد رقم هاتف)
            else if (Jid.EndsWith("@lid") && string.IsNullOrEmpty(phoneToUse))
            {
                 return $"{Jid} - {adminText}, {whitelistText}"; // اعرض الـ @LID
            }
            // حالة افتراضية (نادراً ما تصل هنا إذا كان المنطق أعلاه صحيحًا)
            return $"{nameToUse} - {adminText}, {whitelistText}";
        }
    }

    public partial class GroupsDisplayForm : Form
    {
        private readonly ClientWebSocket _sharedWebSocketClient;
        private readonly CancellationTokenSource _sharedCancellationTokenSource;
        private readonly string _clientId;
        private readonly string _clientPhoneNumber; // رقم هاتف حساب البوت الرئيسي
        private bool _isProcessingCommand = false;

        public GroupsDisplayForm(ClientWebSocket sharedWsClient, CancellationTokenSource sharedCts, string clientId, string clientPhoneNumber)
        {
            InitializeComponent();
            _sharedWebSocketClient = sharedWsClient ?? throw new ArgumentNullException(nameof(sharedWsClient));
            _sharedCancellationTokenSource = sharedCts ?? throw new ArgumentNullException(nameof(sharedCts));
            _clientId = clientId;
            _clientPhoneNumber = clientPhoneNumber; 
            labelSelectedClient.Text = $"Groups for Client: {(_clientPhoneNumber.Length > 5 ? _clientPhoneNumber.Substring(0,5) + "..." : _clientPhoneNumber)} ({_clientId})";

            groupsListView.Columns[0].Width = 130; 
            groupsListView.Columns[1].Width = 160; 
            groupsListView.Columns[2].Width = 60;  
            groupsListView.Columns[3].Width = 80;  

            ApplyGroupFormStyles();
            CheckWebSocketConnection();
        }

        private void CheckWebSocketConnection()
        {
            if (_sharedWebSocketClient.State != WebSocketState.Open)
            {
                UpdateStatus("WebSocket connection lost. Please reconnect.", Color.Red);
                DisableAllButtons();
            }
        }

        private void DisableAllButtons() => SetButtonsProcessingState(true, true); // true to force disable
        private void EnableAllButtons() => SetButtonsProcessingState(false);


        private void ApplyGroupFormStyles()
        {
            groupsListView.OwnerDraw = true;
            groupsListView.DrawColumnHeader += (s, e) =>
            {
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var headerBrush = new SolidBrush(Color.FromArgb(18, 140, 126));
                using var borderPen = new Pen(Color.FromArgb(12, 100, 90), 1);
                e.Graphics.FillRectangle(headerBrush, e.Bounds);
                e.Graphics.DrawRectangle(borderPen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            };
            groupsListView.DrawSubItem += (s, e) => e.DrawDefault = true;
            participantsListBox.DrawMode = DrawMode.OwnerDrawFixed;
            participantsListBox.DrawItem += participantsListBox_DrawItem;
        }

        private void GroupsDisplayForm_Load(object? sender, EventArgs e) // sender can be null
        {
            if (_sharedWebSocketClient.State == WebSocketState.Open)
            {
                EnableAllButtons(); // Enable buttons initially
                btnFetchGroups_Click(null, null); 
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

            SetButtonsProcessingState(true);
            
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
            catch (OperationCanceledException) { UpdateStatus("Operation cancelled.", Color.Orange); return false; }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send command: {ex.Message}", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Command failed: {ex.Message}", Color.Red);
                return false;
            }
            finally
            {
                SetButtonsProcessingState(false);
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
            btnWhitelistParticipant.Enabled = canEnable && (participantsListBox.SelectedItem is ParticipantInfo);
        }

        private async void btnFetchGroups_Click(object? sender, EventArgs? e)
        {
            groupsListView.Items.Clear();
            participantsListBox.Items.Clear();
            UpdateStatus("Fetching groups...", SystemColors.ControlText);
            if (!await SendManagerCommand("fetchGroups")) UpdateStatus("Failed to send fetch groups command.", Color.Red);
        }

        private async void btnWhitelistGroup_Click(object? sender, EventArgs e)
        {
            if (groupsListView.SelectedItems.Count == 0) { MessageBox.Show("Please select a group.", "No Group Selected"); return; }
            ListViewItem selectedGroupItem = groupsListView.SelectedItems[0];
            string? groupId = selectedGroupItem.Tag?.ToString();
            string groupName = selectedGroupItem.SubItems[1].Text;
            bool currentWhitelistStatus = selectedGroupItem.SubItems[3].Text.Equals("Yes", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(groupId)) { MessageBox.Show("Invalid group selection (no ID).", "Error"); return; }
            string action = currentWhitelistStatus ? "remove from whitelist" : "add to whitelist";
            if (MessageBox.Show($"Are you sure you want to {action} group \"{groupName}\"?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            string command = currentWhitelistStatus ? "removeFromChatWhitelist" : "addChatToWhitelist";
            UpdateStatus($"Processing {action} for group \"{groupName}\"...", SystemColors.ControlText);
            await SendManagerCommand(command, groupId);
        }

        private async void btnFetchParticipants_Click(object? sender, EventArgs e)
        {
            if (groupsListView.SelectedItems.Count == 0) { MessageBox.Show("Please select a group.", "No Group Selected"); return; }
            string? groupId = groupsListView.SelectedItems[0].Tag?.ToString();
            string groupName = groupsListView.SelectedItems[0].SubItems[1].Text;
            if (string.IsNullOrEmpty(groupId)) { MessageBox.Show("Invalid group selection (no ID).", "Error"); return; }
            participantsListBox.Items.Clear();
            participantsListBox.Items.Add($"Fetching for \"{groupName}\"...");
            UpdateStatus($"Fetching participants for \"{groupName}\"...", SystemColors.ControlText);
            await SendManagerCommand("fetchParticipants", groupId);
        }

        private async void btnWhitelistParticipant_Click(object? sender, EventArgs e)
        {
            if (!(participantsListBox.SelectedItem is ParticipantInfo selectedParticipant)) { MessageBox.Show("Please select a participant.", "No Participant"); return; }
            string action = selectedParticipant.IsWhitelisted ? "remove from whitelist" : "add to whitelist";
            if (MessageBox.Show($"Confirm {action} for \"{selectedParticipant.DisplayName} ({selectedParticipant.PhoneNumber})\"?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            string command = selectedParticipant.IsWhitelisted ? "removeFromChatWhitelist" : "addChatToWhitelist";
            UpdateStatus($"Processing {action} for {selectedParticipant.DisplayName}...", SystemColors.ControlText);
            await SendManagerCommand(command, null, selectedParticipant.Jid);
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
            } else {
                 participantsListBox.Items.Clear();
            }
        }

        private void participantsListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SetButtonsProcessingState(_isProcessingCommand); // Re-evaluate
            if (participantsListBox.SelectedItem is ParticipantInfo pInfo)
            {
                btnWhitelistParticipant.Text = pInfo.IsWhitelisted ? "Remove Whitelist" : "Add Whitelist";
            }
        }

        private void participantsListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();
            var item = participantsListBox.Items[e.Index];
            if (!(item is ParticipantInfo participant))
            {
                e.Graphics.DrawString(item.ToString(), e.Font, Brushes.DimGray, e.Bounds, StringFormat.GenericDefault);
                e.DrawFocusRectangle(); return;
            }
            Brush textBrush = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? SystemBrushes.HighlightText :
                              participant.IsWhitelisted && participant.IsAdmin ? Brushes.DarkMagenta :
                              participant.IsWhitelisted ? Brushes.DarkGreen :
                              participant.IsAdmin ? Brushes.DarkBlue : Brushes.Black;
            e.Graphics.DrawString(participant.ToString(), e.Font, textBrush, e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }

        private string ExtractPhoneNumber(string? jid)
        {
            if (string.IsNullOrEmpty(jid)) return "Unknown";
            var parts = jid.Split('@');
            return parts.Length > 0 ? parts[0] : jid;
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
                    case "addChatToWhitelistResponse": case "removeFromChatWhitelistResponse": ProcessWhitelistResponse(message, type); break;
                    case "error": ProcessError(message); break;
                    default: Console.WriteLine($"GroupsDisplayForm: Unhandled type {type}"); break;
                }
            }
            catch (Exception ex) { UpdateStatus($"Error parsing msg: {ex.Message}", Color.Red); Console.WriteLine($"GroupsDisplayForm Parse Error: {ex}"); }
        }
        
        // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< NEW FUNCTION
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
                // افترض أن الخادم يرسل أيضًا حالة القائمة البيضاء المحدثة
                bool? newWhitelistStatus = message["isWhitelisted"]?.ToObject<bool?>();


                if (string.IsNullOrEmpty(originalLid) || string.IsNullOrEmpty(resolvedPhoneJid))
                {
                    Console.WriteLine("[GroupsDisplayForm] Received incomplete participantDetailsUpdate (missing originalLid or resolvedPhoneJid).");
                    return;
                }

                bool participantUpdated = false;
                int originalSelectedIndex = participantsListBox.SelectedIndex; // حفظ التحديد الحالي

                // استخدام نسخة من العناصر للمعالجة لتجنب مشاكل التعديل أثناء التكرار
                var currentItems = new List<object>();
                foreach (var item in participantsListBox.Items)
                {
                    currentItems.Add(item);
                }

                for (int i = 0; i < currentItems.Count; i++)
                {
                    if (currentItems[i] is ParticipantInfo pInfo && pInfo.Jid == originalLid)
                    {
                        Console.WriteLine($"[GroupsDisplayForm] Updating participant: Original LID={originalLid}, New ResolvedJID={resolvedPhoneJid}, New Name={displayName}, New Whitelist={newWhitelistStatus}");
                        
                        pInfo.ResolvedPhoneJid = resolvedPhoneJid;
                        pInfo.PhoneNumber = ExtractPhoneNumber(resolvedPhoneJid); // تحديث الرقم المعروض
                        pInfo.DisplayName = displayName ?? pInfo.DisplayName; // تحديث اسم العرض
                        
                        if (newWhitelistStatus.HasValue)
                        {
                            pInfo.IsWhitelisted = newWhitelistStatus.Value;
                        }
                        // إذا لم يتم إرسال newWhitelistStatus، يمكنك إعادة طلب قائمة المشاركين لتحديثها بالكامل
                        // أو الحفاظ على الحالة القديمة مؤقتًا. حاليًا، لن نغيرها إذا لم تُرسل.

                        participantUpdated = true;
                        // لا حاجة لتعديل participantsListBox.Items[i] هنا مباشرة لأننا سنعيد بناء القائمة
                        break; 
                    }
                }

                if (participantUpdated)
                {
                    participantsListBox.BeginUpdate();
                    participantsListBox.Items.Clear();
                    foreach (var item in currentItems) // أضف العناصر المحدثة
                    {
                        participantsListBox.Items.Add(item);
                    }
                    // حاول إعادة تحديد العنصر إذا كان لا يزال صالحًا
                    if (originalSelectedIndex >= 0 && originalSelectedIndex < participantsListBox.Items.Count)
                    {
                        participantsListBox.SelectedIndex = originalSelectedIndex;
                    }
                    else if (participantsListBox.Items.Count > 0)
                    {
                        // إذا كان التحديد السابق غير صالح، حدد العنصر الأول إذا وجدت قائمة
                        // ابحث عن العنصر المحدث مرة أخرى إذا كان الفهرس قد تغير
                        bool foundUpdated = false;
                        for(int i=0; i < participantsListBox.Items.Count; ++i) {
                            if (participantsListBox.Items[i] is ParticipantInfo updatedPInfo && updatedPInfo.ResolvedPhoneJid == resolvedPhoneJid) {
                                participantsListBox.SelectedIndex = i;
                                foundUpdated = true;
                                break;
                            }
                        }
                        if (!foundUpdated && participantsListBox.Items.Count > 0) participantsListBox.SelectedIndex = 0; // كحل أخير
                    }
                    participantsListBox.EndUpdate();
                    
                    string phoneNumForStatus = ExtractPhoneNumber(resolvedPhoneJid);
                    UpdateStatus($"Participant {originalLid.Split('@')[0]} identified as {displayName ?? phoneNumForStatus} ({phoneNumForStatus}). Refreshing list if needed.", Color.Blue);
                }
                else
                {
                     Console.WriteLine($"[GroupsDisplayForm] Participant with originalLid {originalLid} not found in ListBox for update.");
                     // يمكنك إضافة منطق هنا لإعادة طلب قائمة المشاركين إذا لم يتم العثور على العنصر
                     // هذا قد يعني أن القائمة لم تكن تحتوي على هذا ה-LID عند إرسال التحديث
                     // if (groupsListView.SelectedItems.Count > 0) btnFetchParticipants_Click(null, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error updating participant: {ex.Message}", Color.Red);
                Console.WriteLine($"[GroupsDisplayForm_ERROR] Processing participantDetailsUpdate: {ex.ToString()} \nJSON: {messageJson.Substring(0, Math.Min(500, messageJson.Length))}");
            }
        }
        // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< END MODIFIED FUNCTION

        private void ProcessGroupsList(JObject message)
        {
            groupsListView.Items.Clear();
            JArray? groups = message["groups"] as JArray;
            if (groups != null)
            {
                foreach (JObject group in groups.OfType<JObject>())
                {
                    var item = new ListViewItem(group["id"]?.ToString() ?? "N/A")
                    {
                        Tag = group["id"]?.ToString()
                    };
                    item.SubItems.Add(group["subject"]?.ToString() ?? "Unnamed");
                    item.SubItems.Add(group["participantsCount"]?.ToString() ?? "0");
                    bool isWhitelisted = group["isWhitelisted"]?.ToObject<bool>() ?? false;
                    item.SubItems.Add(isWhitelisted ? "Yes" : "No");
                    if (isWhitelisted) item.BackColor = Color.LightGreen;
                    groupsListView.Items.Add(item);
                }
                UpdateStatus($"Found {groupsListView.Items.Count} groups.", SystemColors.ControlText);
                if (groupsListView.Items.Count > 0) { groupsListView.Items[0].Selected = true; groupsListView.Focus(); }
            } else UpdateStatus("No groups found.", Color.Orange);
        }

        private void ProcessParticipantsList(JObject message)
        {
            participantsListBox.Items.Clear();
            JArray? participants = message["participants"] as JArray;
            string? forGroupId = message["groupId"]?.ToString();
            string groupName = groupsListView.Items.Cast<ListViewItem>()
                               .FirstOrDefault(it => it.Tag?.ToString() == forGroupId)?.SubItems[1].Text ?? forGroupId?.Split('@')[0] ?? "Group";

            if (participants != null)
            {
                participantsListBox.Items.Add($"=== Participants for \"{groupName}\" ({participants.Count}) ===");
                var participantList = new List<ParticipantInfo>();
                foreach (JObject p in participants.OfType<JObject>())
                {
                    string? jid = p["jid"]?.ToString();
                    if (string.IsNullOrEmpty(jid)) continue;

                    string? resolvedJidPayload = p["resolvedJid"]?.ToString(); // قد يكون null
                    string phoneToUse = ExtractPhoneNumber(resolvedJidPayload ?? jid); // استخدم الرقم المحلول إن وجد
                    
                    participantList.Add(new ParticipantInfo
                    {
                        Jid = jid,
                        ResolvedPhoneJid = resolvedJidPayload,
                        PhoneNumber = phoneToUse,
                        DisplayName = p["displayName"]?.ToString() ?? phoneToUse, // الاسم من Node.js
                        IsAdmin = p["isAdmin"]?.ToObject<bool>() ?? false,
                        IsWhitelisted = p["isWhitelisted"]?.ToObject<bool>() ?? false
                    });
                }
                var sortedParticipants = participantList.OrderByDescending(p => p.IsAdmin).ThenByDescending(p => p.IsWhitelisted).ThenBy(p => p.DisplayName);
                foreach (var pInfo in sortedParticipants) participantsListBox.Items.Add(pInfo);
                UpdateStatus($"Loaded {participantList.Count} participants for {groupName}.", SystemColors.ControlText);
                if (participantsListBox.Items.Count > 1) participantsListBox.SelectedIndex = 1;
            } else { UpdateStatus($"No participants found for {groupName}.", Color.Orange); participantsListBox.Items.Add("No participants found."); }
        }

        private void ProcessWhitelistResponse(JObject message, string responseType)
        {
            bool success = message["success"]?.ToObject<bool>() ?? false;
            string reason = message["reason"]?.ToString() ?? "Error";
            string? jid = message["jid"]?.ToString();
            string itemType = message["typeOfItem"]?.ToString() ?? "item";
            string action = responseType.Contains("add") ? "whitelisted" : "removed from whitelist";
            
            if (success) {
                UpdateStatus($"Successfully {action} {itemType} '{jid?.Split('@')[0]}'.", Color.DarkGreen);
                if (itemType == "group") btnFetchGroups_Click(null, null);
                else if (itemType == "user" && groupsListView.SelectedItems.Count > 0 && groupsListView.SelectedItems[0].Tag?.ToString() != null)
                {
                    // لتجنب استدعاء متزامن أثناء معالجة أمر آخر
                    string currentGroupId = groupsListView.SelectedItems[0].Tag.ToString()!;
                     Task.Run(async () => {
                        await Task.Delay(100); // تأخير بسيط إذا لزم الأمر
                        await SendManagerCommand("fetchParticipants", currentGroupId);
                    });
                }
            }
            else {
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
            labelSelectedClient.Text = $"Client: {(_clientPhoneNumber.Length > 5 ? _clientPhoneNumber.Substring(0,5) + "..." : _clientPhoneNumber)} - {message}";
            labelSelectedClient.ForeColor = color;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
    }
}