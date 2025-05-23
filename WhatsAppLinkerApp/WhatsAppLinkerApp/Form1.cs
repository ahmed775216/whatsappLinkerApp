// Form1.cs
using System;
using System.Windows.Forms;

namespace WhatsAppLinkerApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void linkWhatsappButton_Click(object sender, EventArgs e)
        {
            // Create an instance of the QR display form
            QrDisplayForm qrForm = new QrDisplayForm();
            
            // Subscribe to the ClientLinked event to get data back
            qrForm.ClientLinked += QrForm_ClientLinked; 

            // Show the QR form modally
            qrForm.ShowDialog();

            // After ShowDialog returns (meaning QR form is closed)
            // The QrForm_ClientLinked event handler already has the linked client info.
            // You can add more logic here, like refreshing a list of linked clients, etc.
        }

        // Event handler for when a client successfully links via the QrDisplayForm
        private void QrForm_ClientLinked(string clientId, string phoneNumber, string clientName)
        {
            MessageBox.Show($"Client successfully linked!\nID: {clientId}\nPhone: {phoneNumber}\nName: {clientName}", "WhatsApp Linked");
            // You can update UI elements on Form1 here.
            // Example: If you had a ListBox called `linkedClientsListBox`
            // linkedClientsListBox.Items.Add($"Client: {clientName} ({phoneNumber}) [ID: {clientId}]");

            // At this point, you have the `clientId` which corresponds to the folder name in `client_data`
            // and the `phoneNumber`. This is critical for Stage 4 where you will provide API credentials
            // and activate this specific client bot.
        }
    }
}