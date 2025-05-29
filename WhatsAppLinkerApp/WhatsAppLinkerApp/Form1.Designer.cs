// Form1.Designer.cs
namespace WhatsAppLinkerApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.linkWhatsappButton = new System.Windows.Forms.Button();
            this.labelApiUsername = new System.Windows.Forms.Label();
            this.textBoxApiUsername = new System.Windows.Forms.TextBox();
            this.labelApiPassword = new System.Windows.Forms.Label();
            this.textBoxApiPassword = new System.Windows.Forms.TextBox();
            this.labelOwnerNumber = new System.Windows.Forms.Label();
            this.comboBoxCountryCode = new System.Windows.Forms.ComboBox();
            this.textBoxPhoneNumber = new System.Windows.Forms.TextBox();
            this.instanceListView = new System.Windows.Forms.ListView();
            this.colClientId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPhoneNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnRefreshInstances = new System.Windows.Forms.Button();
            this.btnStartInstance = new System.Windows.Forms.Button();
            this.btnStopInstance = new System.Windows.Forms.Button();
            this.btnStopAndDeleteInstance = new System.Windows.Forms.Button();
            this.btnRestartInstance = new System.Windows.Forms.Button();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.btnGetLogs = new System.Windows.Forms.Button();
            this.btnManageGroups = new System.Windows.Forms.Button();
            // --- NEW: Declare btnClearCacheAndLogs ---
            // this.btnClearCacheAndLogs = new System.Windows.Forms.Button();
            // --- END NEW ---
            this.groupBoxCredentials = new System.Windows.Forms.GroupBox();
            this.groupBoxInstances = new System.Windows.Forms.GroupBox();
            this.groupBoxLogs = new System.Windows.Forms.GroupBox();
            this.lblConnectionStatus = new System.Windows.Forms.Label(); // Added for connection status
            this.groupBoxCredentials.SuspendLayout();
            this.groupBoxInstances.SuspendLayout();
            this.groupBoxLogs.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBoxCredentials
            //
            this.groupBoxCredentials.BackColor = System.Drawing.Color.White;
            this.groupBoxCredentials.Controls.Add(this.labelApiUsername);
            this.groupBoxCredentials.Controls.Add(this.textBoxApiUsername);
            this.groupBoxCredentials.Controls.Add(this.labelApiPassword);
            this.groupBoxCredentials.Controls.Add(this.textBoxApiPassword);
            this.groupBoxCredentials.Controls.Add(this.labelOwnerNumber);
            this.groupBoxCredentials.Controls.Add(this.comboBoxCountryCode);
            this.groupBoxCredentials.Controls.Add(this.textBoxPhoneNumber);
            this.groupBoxCredentials.Controls.Add(this.linkWhatsappButton);
            this.groupBoxCredentials.Controls.Add(this.btnManageGroups);
            this.groupBoxCredentials.Location = new System.Drawing.Point(10, 10);
            this.groupBoxCredentials.Name = "groupBoxCredentials";
            this.groupBoxCredentials.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxCredentials.Size = new System.Drawing.Size(280, 290);
            this.groupBoxCredentials.TabIndex = 0;
            this.groupBoxCredentials.TabStop = false;
            this.groupBoxCredentials.Text = "New WhatsApp Connection";
            //
            // labelApiUsername
            //
            this.labelApiUsername.AutoSize = true;
            this.labelApiUsername.Location = new System.Drawing.Point(15, 35);
            this.labelApiUsername.Name = "labelApiUsername";
            this.labelApiUsername.Size = new System.Drawing.Size(89, 15);
            this.labelApiUsername.TabIndex = 1;
            this.labelApiUsername.Text = "API Username:";
            //
            // textBoxApiUsername
            //
            this.textBoxApiUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxApiUsername.Location = new System.Drawing.Point(105, 32);
            this.textBoxApiUsername.Name = "textBoxApiUsername";
            this.textBoxApiUsername.Size = new System.Drawing.Size(160, 23);
            this.textBoxApiUsername.TabIndex = 2;
            this.textBoxApiUsername.Text = "781028068"; // Default value for API username
            //
            // labelApiPassword
            //
            this.labelApiPassword.AutoSize = true;
            this.labelApiPassword.Location = new System.Drawing.Point(15, 75);
            this.labelApiPassword.Name = "labelApiPassword";
            this.labelApiPassword.Size = new System.Drawing.Size(87, 15);
            this.labelApiPassword.TabIndex = 3;
            this.labelApiPassword.Text = "API Password:";
            //
            // textBoxApiPassword
            //
            this.textBoxApiPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxApiPassword.Location = new System.Drawing.Point(105, 72);
            this.textBoxApiPassword.Name = "textBoxApiPassword";
            this.textBoxApiPassword.Size = new System.Drawing.Size(160, 23);
            this.textBoxApiPassword.TabIndex = 4;
            this.textBoxApiPassword.UseSystemPasswordChar = true;
            this.textBoxApiPassword.Text = "781028068"; // Default value for API password
            //
            // labelOwnerNumber
            //
            this.labelOwnerNumber.AutoSize = true;
            this.labelOwnerNumber.Location = new System.Drawing.Point(15, 115);
            this.labelOwnerNumber.Name = "labelOwnerNumber";
            this.labelOwnerNumber.Size = new System.Drawing.Size(91, 15);
            this.labelOwnerNumber.TabIndex = 7;
            this.labelOwnerNumber.Text = "Owner Number:";
            //
            // comboBoxCountryCode
            //
            this.comboBoxCountryCode.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboBoxCountryCode.FormattingEnabled = true;
            this.comboBoxCountryCode.Location = new System.Drawing.Point(105, 112);
            this.comboBoxCountryCode.Name = "comboBoxCountryCode";
            this.comboBoxCountryCode.Size = new System.Drawing.Size(70, 23);
            this.comboBoxCountryCode.TabIndex = 6;
            //
            // textBoxPhoneNumber
            //
            this.textBoxPhoneNumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxPhoneNumber.Location = new System.Drawing.Point(180, 112);
            this.textBoxPhoneNumber.Name = "textBoxPhoneNumber";
            this.textBoxPhoneNumber.Size = new System.Drawing.Size(85, 23);
            this.textBoxPhoneNumber.TabIndex = 5;
            this.textBoxPhoneNumber.Text = "733300785"; // Default value for phone number
            //
            // linkWhatsappButton
            //
            this.linkWhatsappButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(211)))), ((int)(((byte)(102)))));
            this.linkWhatsappButton.Enabled = false;
            this.linkWhatsappButton.FlatAppearance.BorderSize = 0;
            this.linkWhatsappButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.linkWhatsappButton.ForeColor = System.Drawing.Color.White;
            this.linkWhatsappButton.Location = new System.Drawing.Point(15, 180);
            this.linkWhatsappButton.Name = "linkWhatsappButton";
            this.linkWhatsappButton.Size = new System.Drawing.Size(250, 35);
            this.linkWhatsappButton.TabIndex = 0;
            this.linkWhatsappButton.Text = "Link New WhatsApp Account";
            this.linkWhatsappButton.UseVisualStyleBackColor = false;
            //
            // btnManageGroups
            //
            this.btnManageGroups.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(140)))), ((int)(((byte)(126)))));
            this.btnManageGroups.Enabled = false;
            this.btnManageGroups.FlatAppearance.BorderSize = 0;
            this.btnManageGroups.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageGroups.ForeColor = System.Drawing.Color.White;
            this.btnManageGroups.Location = new System.Drawing.Point(15, 225);
            this.btnManageGroups.Name = "btnManageGroups";
            this.btnManageGroups.Size = new System.Drawing.Size(250, 30);
            this.btnManageGroups.TabIndex = 1;
            this.btnManageGroups.Text = "Manage Groups for Selected Client";
            this.btnManageGroups.UseVisualStyleBackColor = false;
            //
            // groupBoxInstances
            //
            this.groupBoxInstances.BackColor = System.Drawing.Color.White;
            this.groupBoxInstances.Controls.Add(this.instanceListView);
            this.groupBoxInstances.Controls.Add(this.btnRefreshInstances);
            this.groupBoxInstances.Controls.Add(this.btnStartInstance);
            this.groupBoxInstances.Controls.Add(this.btnStopInstance);
            this.groupBoxInstances.Controls.Add(this.btnStopAndDeleteInstance);
            this.groupBoxInstances.Controls.Add(this.btnRestartInstance);
            this.groupBoxInstances.Controls.Add(this.btnGetLogs);
            // --- NEW: Add btnClearCacheAndLogs to groupBoxInstances ---
            // this.groupBoxInstances.Controls.Add(this.btnClearCacheAndLogs);
            // --- END NEW ---
            this.groupBoxInstances.Location = new System.Drawing.Point(300, 10);
            this.groupBoxInstances.Name = "groupBoxInstances";
            this.groupBoxInstances.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxInstances.Size = new System.Drawing.Size(650, 290);
            this.groupBoxInstances.TabIndex = 1;
            this.groupBoxInstances.TabStop = false;
            this.groupBoxInstances.Text = "Bot Instances";
            //
            // instanceListView
            //
            this.instanceListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.instanceListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colClientId,
            this.colPhoneNumber,
            this.colName,
            this.colStatus});
            this.instanceListView.FullRowSelect = true;
            this.instanceListView.GridLines = true;
            this.instanceListView.HideSelection = false;
            this.instanceListView.Location = new System.Drawing.Point(15, 25);
            this.instanceListView.MultiSelect = false;
            this.instanceListView.Name = "instanceListView";
            this.instanceListView.OwnerDraw = true;
            this.instanceListView.Size = new System.Drawing.Size(620, 210);
            this.instanceListView.TabIndex = 0;
            this.instanceListView.UseCompatibleStateImageBehavior = false;
            this.instanceListView.View = System.Windows.Forms.View.Details;
            //
            // colClientId
            //
            this.colClientId.Text = "Client ID";
            this.colClientId.Width = 150;
            //
            // colPhoneNumber
            //
            this.colPhoneNumber.Text = "Phone Number";
            this.colPhoneNumber.Width = 120;
            //
            // colName
            //
            this.colName.Text = "Name";
            this.colName.Width = 120;
            //
            // colStatus
            //
            this.colStatus.Text = "Status";
            this.colStatus.Width = 150;
            //
            // btnRefreshInstances
            //
            this.btnRefreshInstances.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(140)))), ((int)(((byte)(126)))));
            this.btnRefreshInstances.FlatAppearance.BorderSize = 0;
            this.btnRefreshInstances.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefreshInstances.ForeColor = System.Drawing.Color.White;
            this.btnRefreshInstances.Location = new System.Drawing.Point(15, 245);
            this.btnRefreshInstances.Name = "btnRefreshInstances";
            this.btnRefreshInstances.Size = new System.Drawing.Size(110, 30);
            this.btnRefreshInstances.TabIndex = 1;
            this.btnRefreshInstances.Text = "Refresh List";
            this.btnRefreshInstances.UseVisualStyleBackColor = false;
            //
            // btnStartInstance
            //
            this.btnStartInstance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(211)))), ((int)(((byte)(102)))));
            this.btnStartInstance.FlatAppearance.BorderSize = 0;
            this.btnStartInstance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartInstance.ForeColor = System.Drawing.Color.White;
            this.btnStartInstance.Location = new System.Drawing.Point(135, 245);
            this.btnStartInstance.Name = "btnStartInstance";
            this.btnStartInstance.Size = new System.Drawing.Size(75, 30);
            this.btnStartInstance.TabIndex = 2;
            this.btnStartInstance.Text = "Start";
            this.btnStartInstance.UseVisualStyleBackColor = false;
            //
            // btnStopInstance
            //
            this.btnStopInstance.BackColor = System.Drawing.Color.IndianRed;
            this.btnStopInstance.FlatAppearance.BorderSize = 0;
            this.btnStopInstance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopInstance.ForeColor = System.Drawing.Color.White;
            this.btnStopInstance.Location = new System.Drawing.Point(220, 245);
            this.btnStopInstance.Name = "btnStopInstance";
            this.btnStopInstance.Size = new System.Drawing.Size(75, 30);
            this.btnStopInstance.TabIndex = 3;
            this.btnStopInstance.Text = "Stop";
            this.btnStopInstance.UseVisualStyleBackColor = false;
            //
            // btnStopAndDeleteInstance
            //
            this.btnStopAndDeleteInstance.BackColor = System.Drawing.Color.Firebrick;
            this.btnStopAndDeleteInstance.FlatAppearance.BorderSize = 0;
            this.btnStopAndDeleteInstance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopAndDeleteInstance.ForeColor = System.Drawing.Color.White;
            this.btnStopAndDeleteInstance.Location = new System.Drawing.Point(305, 245);
            this.btnStopAndDeleteInstance.Name = "btnStopAndDeleteInstance";
            this.btnStopAndDeleteInstance.Size = new System.Drawing.Size(120, 30);
            this.btnStopAndDeleteInstance.TabIndex = 4;
            this.btnStopAndDeleteInstance.Text = "Stop and Delete";
            this.btnStopAndDeleteInstance.UseVisualStyleBackColor = false;
            //
            // btnRestartInstance
            //
            this.btnRestartInstance.BackColor = System.Drawing.Color.Goldenrod;
            this.btnRestartInstance.FlatAppearance.BorderSize = 0;
            this.btnRestartInstance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestartInstance.ForeColor = System.Drawing.Color.White;
            this.btnRestartInstance.Location = new System.Drawing.Point(435, 245);
            this.btnRestartInstance.Name = "btnRestartInstance";
            this.btnRestartInstance.Size = new System.Drawing.Size(85, 30);
            this.btnRestartInstance.TabIndex = 5;
            this.btnRestartInstance.Text = "Restart";
            this.btnRestartInstance.UseVisualStyleBackColor = false;
            //
            // logTextBox
            //
            this.logTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(44)))), ((int)(((byte)(51)))));
            this.logTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logTextBox.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.logTextBox.Location = new System.Drawing.Point(10, 25);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(920, 215);
            this.logTextBox.TabIndex = 0;
            //
            // btnGetLogs
            //
            this.btnGetLogs.BackColor = System.Drawing.Color.SteelBlue;
            this.btnGetLogs.FlatAppearance.BorderSize = 0;
            this.btnGetLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGetLogs.ForeColor = System.Drawing.Color.White;
            this.btnGetLogs.Location = new System.Drawing.Point(530, 245);
            this.btnGetLogs.Name = "btnGetLogs";
            this.btnGetLogs.Size = new System.Drawing.Size(85, 30);
            this.btnGetLogs.TabIndex = 6;
            this.btnGetLogs.Text = "Get Logs";
            this.btnGetLogs.UseVisualStyleBackColor = false;
            //
            // btnManageGroups
            // (existing, no change in designer)
            //
            // --- NEW: btnClearCacheAndLogs properties ---
            // this.btnClearCacheAndLogs.BackColor = System.Drawing.Color.DarkSlateGray;
            // this.btnClearCacheAndLogs.Enabled = false;
            // this.btnClearCacheAndLogs.FlatAppearance.BorderSize = 0;
            // this.btnClearCacheAndLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            // this.btnClearCacheAndLogs.ForeColor = System.Drawing.Color.White;
            // this.btnClearCacheAndLogs.Location = new System.Drawing.Point(17, 270); // Adjusted position
            // this.btnClearCacheAndLogs.Name = "btnClearCacheAndLogs";
            // this.btnClearCacheAndLogs.Size = new System.Drawing.Size(250, 30);
            // this.btnClearCacheAndLogs.TabIndex = 7; // New TabIndex
            // this.btnClearCacheAndLogs.Text = "Clear Client Cache & Logs";
            // this.btnClearCacheAndLogs.UseVisualStyleBackColor = false;
            // this.btnClearCacheAndLogs = new System.Windows.Forms.Button();

            // --- END NEW ---
            //
            // groupBoxLogs
            //
            this.groupBoxLogs.BackColor = System.Drawing.Color.White;
            this.groupBoxLogs.Controls.Add(this.logTextBox);
            this.groupBoxLogs.Location = new System.Drawing.Point(10, 315);
            this.groupBoxLogs.Name = "groupBoxLogs";
            this.groupBoxLogs.Padding = new System.Windows.Forms.Padding(10, 12, 10, 10);
            this.groupBoxLogs.Size = new System.Drawing.Size(940, 250);
            this.groupBoxLogs.TabIndex = 2;
            this.groupBoxLogs.TabStop = false;
            this.groupBoxLogs.Text = "Instance Logs";
            //
            // lblConnectionStatus
            //
            this.lblConnectionStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblConnectionStatus.AutoSize = true;
            this.lblConnectionStatus.Location = new System.Drawing.Point(10, 560);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(115, 15);
            this.lblConnectionStatus.TabIndex = 3;
            this.lblConnectionStatus.Text = "Status: Connecting...";
            this.lblConnectionStatus.ForeColor = System.Drawing.Color.Goldenrod;
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(237)))));
            this.ClientSize = new System.Drawing.Size(960, 580);
            this.Controls.Add(this.lblConnectionStatus);
            this.Controls.Add(this.groupBoxLogs);
            this.Controls.Add(this.groupBoxInstances);
            this.Controls.Add(this.groupBoxCredentials);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(976, 619);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WhatsApp Bot Manager UI";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBoxCredentials.ResumeLayout(false);
            this.groupBoxCredentials.PerformLayout();
            this.groupBoxInstances.ResumeLayout(false);
            this.groupBoxLogs.ResumeLayout(false);
            this.groupBoxLogs.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button linkWhatsappButton;
        private System.Windows.Forms.Label labelApiUsername;
        private System.Windows.Forms.TextBox textBoxApiUsername;
        private System.Windows.Forms.Label labelApiPassword;
        private System.Windows.Forms.TextBox textBoxApiPassword;
        private System.Windows.Forms.Label labelOwnerNumber;
        private System.Windows.Forms.ComboBox comboBoxCountryCode;
        private System.Windows.Forms.TextBox textBoxPhoneNumber;
        private System.Windows.Forms.ListView instanceListView;
        private System.Windows.Forms.ColumnHeader colClientId;
        private System.Windows.Forms.ColumnHeader colPhoneNumber;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.Button btnRefreshInstances;
        private System.Windows.Forms.Button btnStartInstance;
        private System.Windows.Forms.Button btnStopInstance;
        private System.Windows.Forms.Button btnRestartInstance;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.Button btnGetLogs;
        private System.Windows.Forms.Button btnStopAndDeleteInstance;
        private System.Windows.Forms.Button btnManageGroups;
        // --- NEW: Declare btnClearCacheAndLogs in Designer ---
        // private System.Windows.Forms.Button btnClearCacheAndLogs;
        // --- END NEW ---
        private System.Windows.Forms.GroupBox groupBoxCredentials;
        private System.Windows.Forms.GroupBox groupBoxInstances;
        private System.Windows.Forms.GroupBox groupBoxLogs;
        private System.Windows.Forms.Label lblConnectionStatus;
    }
}