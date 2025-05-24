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



        /// <summary>
        ///  Required method for Support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.linkWhatsappButton = new System.Windows.Forms.Button();
            this.labelApiUsername = new System.Windows.Forms.Label();
            this.textBoxApiUsername = new System.Windows.Forms.TextBox();
            this.labelApiPassword = new System.Windows.Forms.Label();
            this.textBoxApiPassword = new System.Windows.Forms.TextBox();
            this.labelOwnerNumber = new System.Windows.Forms.Label(); // NEW
            this.textBoxOwnerNumber = new System.Windows.Forms.TextBox();     // NEW
            this.instanceListView = new System.Windows.Forms.ListView();
            this.colClientId = new System.Windows.Forms.ColumnHeader();
            this.colPhoneNumber = new System.Windows.Forms.ColumnHeader();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.btnRefreshInstances = new System.Windows.Forms.Button();
            this.btnStartInstance = new System.Windows.Forms.Button();
            this.btnStopInstance = new System.Windows.Forms.Button();
            this.btnRestartInstance = new System.Windows.Forms.Button();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.btnGetLogs = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // linkWhatsappButton
            //
            this.linkWhatsappButton.Location = new System.Drawing.Point(20, 200); // Adjusted position
            this.linkWhatsappButton.Name = "linkWhatsappButton";
            this.linkWhatsappButton.Size = new System.Drawing.Size(150, 40);
            this.linkWhatsappButton.TabIndex = 0;
            this.linkWhatsappButton.Text = "Link New WhatsApp";
            this.linkWhatsappButton.UseVisualStyleBackColor = true;
            this.linkWhatsappButton.Click += new System.EventHandler(this.linkWhatsappButton_Click);
            //
            // labelApiUsername
            //
            this.labelApiUsername.AutoSize = true;
            this.labelApiUsername.Location = new System.Drawing.Point(20, 30);
            this.labelApiUsername.Name = "labelApiUsername";
            this.labelApiUsername.Size = new System.Drawing.Size(89, 15);
            this.labelApiUsername.TabIndex = 1;
            this.labelApiUsername.Text = "API Username:";
            //
            // textBoxApiUsername
            //
            this.textBoxApiUsername.Location = new System.Drawing.Point(120, 27);
            this.textBoxApiUsername.Name = "textBoxApiUsername";
            this.textBoxApiUsername.Size = new System.Drawing.Size(150, 23);
            this.textBoxApiUsername.TabIndex = 2;
            this.textBoxApiUsername.Text = ""; // Default value for testing
            //
            // labelApiPassword
            //
            this.labelApiPassword.AutoSize = true;
            this.labelApiPassword.Location = new System.Drawing.Point(20, 70);
            this.labelApiPassword.Name = "labelApiPassword";
            this.labelApiPassword.Size = new System.Drawing.Size(87, 15);
            this.labelApiPassword.TabIndex = 3;
            this.labelApiPassword.Text = "API Password:";
            //
            // textBoxApiPassword
            //
            this.textBoxApiPassword.Location = new System.Drawing.Point(120, 67);
            this.textBoxApiPassword.Name = "textBoxApiPassword";
            // this.textBoxApiPassword.PasswordChar = '*';
            this.textBoxApiPassword.Size = new System.Drawing.Size(150, 23);
            this.textBoxApiPassword.TabIndex = 4;
            this.textBoxApiPassword.Text = ""; // Default value for testing
            //
            // labelOwnerNumber
            //
            this.labelOwnerNumber.AutoSize = true;
            this.labelOwnerNumber.Location = new System.Drawing.Point(20, 110);
            this.labelOwnerNumber.Name = "labelOwnerNumber";
            this.labelOwnerNumber.Size = new System.Drawing.Size(91, 15);
            this.labelOwnerNumber.TabIndex = 5;
            this.labelOwnerNumber.Text = "Owner Number:";
            //
            // textBoxOwnerNumber
            //
            this.textBoxOwnerNumber.Location = new System.Drawing.Point(120, 107);
            this.textBoxOwnerNumber.Name = "textBoxOwnerNumber";
            this.textBoxOwnerNumber.Size = new System.Drawing.Size(150, 23);
            this.textBoxOwnerNumber.TabIndex = 6;
            this.textBoxOwnerNumber.Text = ""; // Default value for testing
            //
            // instanceListView
            //
            this.instanceListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colClientId,
            this.colPhoneNumber,
            this.colName,
            this.colStatus});
            this.instanceListView.FullRowSelect = true;
            this.instanceListView.Location = new System.Drawing.Point(300, 27);
            this.instanceListView.MultiSelect = false;
            this.instanceListView.Name = "instanceListView";
            this.instanceListView.Size = new System.Drawing.Size(600, 200);
            this.instanceListView.TabIndex = 7;
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
            this.btnRefreshInstances.Location = new System.Drawing.Point(300, 240);
            this.btnRefreshInstances.Name = "btnRefreshInstances";
            this.btnRefreshInstances.Size = new System.Drawing.Size(120, 30);
            this.btnRefreshInstances.TabIndex = 8;
            this.btnRefreshInstances.Text = "Refresh List";
            this.btnRefreshInstances.UseVisualStyleBackColor = true;
            this.btnRefreshInstances.Click += new System.EventHandler(this.btnRefreshInstances_Click);
            //
            // btnStartInstance
            //
            this.btnStartInstance.Location = new System.Drawing.Point(430, 240);
            this.btnStartInstance.Name = "btnStartInstance";
            this.btnStartInstance.Size = new System.Drawing.Size(80, 30);
            this.btnStartInstance.TabIndex = 9;
            this.btnStartInstance.Text = "Start";
            this.btnStartInstance.UseVisualStyleBackColor = true;
            this.btnStartInstance.Click += new System.EventHandler(this.btnStartInstance_Click);
            //
            // btnStopInstance
            //
            this.btnStopInstance.Location = new System.Drawing.Point(520, 240);
            this.btnStopInstance.Name = "btnStopInstance";
            this.btnStopInstance.Size = new System.Drawing.Size(80, 30);
            this.btnStopInstance.TabIndex = 10;
            this.btnStopInstance.Text = "Stop";
            this.btnStopInstance.UseVisualStyleBackColor = true;
            this.btnStopInstance.Click += new System.EventHandler(this.btnStopInstance_Click);
            //
            // btnRestartInstance
            //
            this.btnRestartInstance.Location = new System.Drawing.Point(610, 240);
            this.btnRestartInstance.Name = "btnRestartInstance";
            this.btnRestartInstance.Size = new System.Drawing.Size(80, 30);
            this.btnRestartInstance.TabIndex = 11;
            this.btnRestartInstance.Text = "Restart";
            this.btnRestartInstance.UseVisualStyleBackColor = true;
            this.btnRestartInstance.Click += new System.EventHandler(this.btnRestartInstance_Click);
            //
            // logTextBox
            //
            this.logTextBox.Location = new System.Drawing.Point(20, 300);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(880, 250);
            this.logTextBox.TabIndex = 12;
            //
            // btnGetLogs
            //
            this.btnGetLogs.Location = new System.Drawing.Point(700, 240);
            this.btnGetLogs.Name = "btnGetLogs";
            this.btnGetLogs.Size = new System.Drawing.Size(80, 30);
            this.btnGetLogs.TabIndex = 13;
            this.btnGetLogs.Text = "Get Logs";
            this.btnGetLogs.UseVisualStyleBackColor = true;
            this.btnGetLogs.Click += new System.EventHandler(this.btnGetLogs_Click);
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(920, 580);
            this.Controls.Add(this.btnGetLogs);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.btnRestartInstance);
            this.Controls.Add(this.btnStopInstance);
            this.Controls.Add(this.btnStartInstance);
            this.Controls.Add(this.btnRefreshInstances);
            this.Controls.Add(this.instanceListView);
            this.Controls.Add(this.textBoxOwnerNumber); // NEW
            this.Controls.Add(this.labelOwnerNumber);     // NEW
            this.Controls.Add(this.textBoxApiPassword);
            this.Controls.Add(this.labelApiPassword);
            this.Controls.Add(this.textBoxApiUsername);
            this.Controls.Add(this.labelApiUsername);
            this.Controls.Add(this.linkWhatsappButton);
            this.Name = "Form1";
            this.Text = "WhatsApp Bot Manager UI";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button linkWhatsappButton;
        private System.Windows.Forms.Label labelApiUsername;
        private System.Windows.Forms.TextBox textBoxApiUsername;
        private System.Windows.Forms.Label labelApiPassword;
        private System.Windows.Forms.TextBox textBoxApiPassword;
        private System.Windows.Forms.Label labelOwnerNumber; // NEW
        private System.Windows.Forms.TextBox textBoxOwnerNumber;     // NEW
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
    }
}