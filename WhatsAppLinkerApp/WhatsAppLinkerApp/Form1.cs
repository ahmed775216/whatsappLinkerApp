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
        // private const string NodeJsWebSocketUrl = "ws://134.119.194.180:8088";
        private const string NodeJsWebSocketUrl = "ws://localhost:8088";

        private QrDisplayForm _currentQrDisplayForm;

        public Form1()
        {
            InitializeComponent();
            instanceListView.Columns[0].Width = 150;
            instanceListView.Columns[1].Width = 120;
            instanceListView.Columns[2].Width = 120;
            instanceListView.Columns[3].Width = 150;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            _controlWsCts = new CancellationTokenSource();
            _managerControlWs = new ClientWebSocket();
            await ConnectToManagerControlWs();
            await RequestInstanceList();
        }

        private async Task ConnectToManagerControlWs()
        {
            if (_managerControlWs.State == WebSocketState.Open) return;

            try
            {
                Console.WriteLine("Connecting to manager control WS...");
                await _managerControlWs.ConnectAsync(new Uri(NodeJsWebSocketUrl), _controlWsCts.Token);
                Console.WriteLine("Connected to manager control WS.");
                _ = ReceiveManagerControlMessagesAsync(_managerControlWs, _controlWsCts.Token);
            }
            catch (WebSocketException ex)
            {
                MessageBox.Show($"Error connecting to manager control: {ex.Message}. Ensure Node.js manager is running.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Manager Control WS Connection Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during manager control connection: {ex.Message}");
            }
        }

private async Task ReceiveManagerControlMessagesAsync(ClientWebSocket wsClient, CancellationToken token)
{
    var buffer = new byte[32768]; // Use a smaller buffer size for each chunk if preferred, but 32KB is fine.
    List<byte> messageBytes = new List<byte>();

    try
    {
        while (wsClient.State == WebSocketState.Open && !token.IsCancellationRequested)
        {
            WebSocketReceiveResult result;
            do
            {
                // Reset result to null for each new chunk
                result = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("Manager Control WS disconnected by server.");
                    // Process any remaining messageBytes if needed before breaking
                    if (messageBytes.Count > 0)
                    {
                        var finalPartialMessage = Encoding.UTF8.GetString(messageBytes.ToArray());
                        Console.WriteLine("Received partial message before close: " + finalPartialMessage);
                        messageBytes.Clear(); // Clear it, it's not a complete message for processing
                    }
                    break;
                }

                messageBytes.AddRange(buffer.Take(result.Count)); // Add the received chunk

            } while (!result.EndOfMessage && wsClient.State == WebSocketState.Open && !token.IsCancellationRequested);

            // Only process if the full message was received
            if (result.EndOfMessage && messageBytes.Count > 0)
            {
                var messageJson = Encoding.UTF8.GetString(messageBytes.ToArray());
                Console.WriteLine("Received from Manager Control WS: " + messageJson);
                ProcessManagerControlMessage(messageJson);
            }
            else if (messageBytes.Count > 0 && wsClient.State == WebSocketState.Open)
            {
                // This means loop exited without EndOfMessage. Possibly connection issue or unexpected scenario.
                Console.WriteLine("Warning: Receive loop exited without EndOfMessage. Message might be incomplete.");
                var partialMessageDebug = Encoding.UTF8.GetString(messageBytes.ToArray());
                Console.WriteLine("Partial message received (might be incomplete): " + partialMessageDebug);
            }

            messageBytes.Clear(); // Prepare for next message
        }
    }
    catch (OperationCanceledException) { /* CTS cancelled */ }
    catch (WebSocketException ex) {
        Console.WriteLine($"Manager Control WS error: {ex.Message}. Connection lost.");
        MessageBox.Show($"Manager control connection lost: {ex.Message}. Please restart the application if issues persist.", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    catch (Exception ex) {
        Console.WriteLine($"Error processing manager control message: {ex.Message}");
    }
}        private void ProcessManagerControlMessage(string messageJson)
        {
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    JObject message = JObject.Parse(messageJson);
                    string type = message["type"]?.ToString() ?? string.Empty;

                    if (_currentQrDisplayForm != null && !_currentQrDisplayForm.IsDisposed)
                    {
                        if (type == "qr" || type == "status")
                        {
                            _currentQrDisplayForm.ProcessManagerMessageForQRDisplay(messageJson);
                        }
                    }

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
                            Console.WriteLine($"Manager overall status: {message["status"]}, Message: {message["message"]}");
                            break;
                        case "qr":
                            Console.WriteLine($"Manager QR broadcast: {message["qr"]}");
                            break;
                        default:
                            Console.WriteLine($"Unhandled manager control message type: {type}");
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
                item.Tag = clientId;
                instanceListView.Items.Add(item);
            }
            instanceListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            instanceListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
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
            Console.WriteLine($"Received status update for unknown instance {clientId}, requesting full list refresh.");
            RequestInstanceList();
        }

        private void DisplayInstanceLogs(string clientId, JArray logs)
        {
            logTextBox.Clear();
            if (logs == null || logs.Count == 0)
            {
                logTextBox.AppendText($"No logs available for {clientId} or logs are empty.\r\n");
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

        private async Task SendManagerCommand(string type, string clientId = null, string apiUsername = null, string apiPassword = null, string ownerNumber = null) // NEW: ownerNumber param
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
            if (ownerNumber != null) request["ownerNumber"] = ownerNumber; // NEW: Add ownerNumber to request

            var buffer = Encoding.UTF8.GetBytes(request.ToString());
            try
            {
                await _managerControlWs.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _controlWsCts.Token);
                Console.WriteLine($"Sent command: {type} for Client: {clientId ?? "N/A"}");
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

        private async void linkWhatsappButton_Click(object sender, EventArgs e)
        {
            string apiUsername = textBoxApiUsername.Text;
            string apiPassword = textBoxApiPassword.Text;
            string ownerNumber = textBoxOwnerNumber.Text; // NEW: Get owner number from UI

            _currentQrDisplayForm = new QrDisplayForm(_managerControlWs, _controlWsCts, apiUsername, apiPassword, ownerNumber); // NEW: Pass ownerNumber
            
            _currentQrDisplayForm.ClientLinked += QrForm_ClientLinked; 

            _currentQrDisplayForm.ShowDialog();

            _currentQrDisplayForm = null; 

            await RequestInstanceList();
        }

        private async void QrForm_ClientLinked(string clientId, string phoneNumber, string clientName)
        {
            MessageBox.Show($"Client successfully linked!\nID: {clientId}\nPhone: {phoneNumber}\nName: {clientName}", "WhatsApp Linked");
            await RequestInstanceList();
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
                // When starting an existing instance, we'd ideally get its stored API/Owner info.
                // For now, it will launch with stored (or null) API/Owner from instanceManager's tracking.
                await SendManagerCommand("startInstance", clientId);
            } else {
                MessageBox.Show("Please select an instance from the list.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btnStopInstance_Click(object sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0)
            {
                string clientId = instanceListView.SelectedItems[0].Tag.ToString();
                await SendManagerCommand("stopInstance", clientId);
            } else {
                MessageBox.Show("Please select an instance from the list.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btnRestartInstance_Click(object sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0)
            {
                string clientId = instanceListView.SelectedItems[0].Tag.ToString();
                // When restarting, we need to pass the currently known API/Owner credentials if they are to persist.
                // This would require storing them in Form1 after successful linking.
                // For now, it will restart with credentials it was launched with or null.
                await SendManagerCommand("restartInstance", clientId);
            } else {
                MessageBox.Show("Please select an instance from the list.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btnGetLogs_Click(object sender, EventArgs e)
        {
            if (instanceListView.SelectedItems.Count > 0)
            {
                string clientId = instanceListView.SelectedItems[0].Tag.ToString();
                logTextBox.Clear();
                logTextBox.AppendText($"Requesting logs for {clientId}...\r\n");
                await SendManagerCommand("getLogs", clientId);
            } else {
                MessageBox.Show("Please select an instance from the list to get logs.", "No Instance Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_controlWsCts != null)
            {
                _controlWsCts.Cancel();
                _controlWsCts.Dispose();
            }
            if (_managerControlWs != null && _managerControlWs.State == WebSocketState.Open)
            {
                _managerControlWs.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None).Wait();
                _managerControlWs.Dispose();
            }
        }
    }
}