
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
            this.mainTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.topPanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblConnectionStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.groupBoxCredentials = new System.Windows.Forms.GroupBox();
            this.credentialsTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.labelApiUsername = new System.Windows.Forms.Label();
            this.textBoxApiUsername = new System.Windows.Forms.TextBox();
            this.labelApiPassword = new System.Windows.Forms.Label();
            this.textBoxApiPassword = new System.Windows.Forms.TextBox();
            this.labelOwnerNumber = new System.Windows.Forms.Label();
            this.phonePanel = new System.Windows.Forms.Panel();
            this.comboBoxCountryCode = new System.Windows.Forms.ComboBox();
            this.textBoxPhoneNumber = new System.Windows.Forms.TextBox();
            this.buttonsFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.linkWhatsappButton = new System.Windows.Forms.Button();
            this.btnManageGroups = new System.Windows.Forms.Button();
            this.btnClearCacheAndLogs = new System.Windows.Forms.Button();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.rightSplitContainer = new System.Windows.Forms.SplitContainer();
            this.groupBoxInstances = new System.Windows.Forms.GroupBox();
            this.instancesTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.instanceListView = new System.Windows.Forms.ListView();
            this.colClientId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPhoneNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.instanceButtonsFlow = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRefreshInstances = new System.Windows.Forms.Button();
            this.btnStartInstance = new System.Windows.Forms.Button();
            this.btnStopInstance = new System.Windows.Forms.Button();
            this.btnRestartInstance = new System.Windows.Forms.Button();
            this.btnStopAndDeleteInstance = new System.Windows.Forms.Button();
            this.btnGetLogs = new System.Windows.Forms.Button();
            this.groupBoxLogs = new System.Windows.Forms.GroupBox();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.mainTableLayout.SuspendLayout();
            this.topPanel.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.groupBoxCredentials.SuspendLayout();
            this.credentialsTableLayout.SuspendLayout();
            this.phonePanel.SuspendLayout();
            this.buttonsFlowLayout.SuspendLayout();
            this.rightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rightSplitContainer)).BeginInit();
            this.rightSplitContainer.Panel1.SuspendLayout();
            this.rightSplitContainer.Panel2.SuspendLayout();
            this.rightSplitContainer.SuspendLayout();
            this.groupBoxInstances.SuspendLayout();
            this.instancesTableLayout.SuspendLayout();
            this.instanceButtonsFlow.SuspendLayout();
            this.groupBoxLogs.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTableLayout
            // 
            this.mainTableLayout.ColumnCount = 1;
            this.mainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayout.Controls.Add(this.topPanel, 0, 0);
            this.mainTableLayout.Controls.Add(this.statusStrip, 0, 2);
            this.mainTableLayout.Controls.Add(this.splitContainer, 0, 1);
            this.mainTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayout.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayout.Name = "mainTableLayout";
            this.mainTableLayout.RowCount = 3;
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.mainTableLayout.Size = new System.Drawing.Size(1024, 768);
            this.mainTableLayout.TabIndex = 0;
            // 
            // topPanel
            // 
            this.topPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(7)))), ((int)(((byte)(94)))), ((int)(((byte)(84)))));
            this.topPanel.Controls.Add(this.titleLabel);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Margin = new System.Windows.Forms.Padding(0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(1024, 50);
            this.topPanel.TabIndex = 0;
            // 
            // titleLabel
            // 
            this.titleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.White;
            this.titleLabel.Location = new System.Drawing.Point(0, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(1024, 50);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "WhatsApp Bot Manager";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblConnectionStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 743);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1024, 25);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(115, 20);
            this.lblConnectionStatus.Text = "Status: Connecting...";
            this.lblConnectionStatus.ForeColor = System.Drawing.Color.Goldenrod;
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(3, 53);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.leftPanel);
            this.splitContainer.Panel1MinSize = 300;
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.rightPanel);
            this.splitContainer.Panel2MinSize = 400;
            this.splitContainer.Size = new System.Drawing.Size(1018, 687);
            this.splitContainer.SplitterDistance = 320;
            this.splitContainer.TabIndex = 2;
            // 
            // leftPanel
            // 
            this.leftPanel.Controls.Add(this.groupBoxCredentials);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftPanel.Location = new System.Drawing.Point(0, 0);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Padding = new System.Windows.Forms.Padding(10);
            this.leftPanel.Size = new System.Drawing.Size(320, 687);
            this.leftPanel.TabIndex = 0;
            // 
            // groupBoxCredentials
            // 
            this.groupBoxCredentials.BackColor = System.Drawing.Color.White;
            this.groupBoxCredentials.Controls.Add(this.credentialsTableLayout);
            this.groupBoxCredentials.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxCredentials.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBoxCredentials.Location = new System.Drawing.Point(10, 10);
            this.groupBoxCredentials.Name = "groupBoxCredentials";
            this.groupBoxCredentials.Padding = new System.Windows.Forms.Padding(15);
            this.groupBoxCredentials.Size = new System.Drawing.Size(300, 350);
            this.groupBoxCredentials.TabIndex = 0;
            this.groupBoxCredentials.TabStop = false;
            this.groupBoxCredentials.Text = "New WhatsApp Connection";
            // 
            // credentialsTableLayout
            // 
            this.credentialsTableLayout.ColumnCount = 2;
            this.credentialsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.credentialsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.credentialsTableLayout.Controls.Add(this.labelApiUsername, 0, 0);
            this.credentialsTableLayout.Controls.Add(this.textBoxApiUsername, 1, 0);
            this.credentialsTableLayout.Controls.Add(this.labelApiPassword, 0, 1);
            this.credentialsTableLayout.Controls.Add(this.textBoxApiPassword, 1, 1);
            this.credentialsTableLayout.Controls.Add(this.labelOwnerNumber, 0, 2);
            this.credentialsTableLayout.Controls.Add(this.phonePanel, 1, 2);
            this.credentialsTableLayout.Controls.Add(this.buttonsFlowLayout, 0, 3);
            this.credentialsTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.credentialsTableLayout.Location = new System.Drawing.Point(15, 33);
            this.credentialsTableLayout.Name = "credentialsTableLayout";
            this.credentialsTableLayout.RowCount = 4;
            this.credentialsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.credentialsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.credentialsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.credentialsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.credentialsTableLayout.Size = new System.Drawing.Size(270, 302);
            this.credentialsTableLayout.TabIndex = 0;
            // 
            // labelApiUsername
            // 
            this.labelApiUsername.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelApiUsername.AutoSize = true;
            this.labelApiUsername.Location = new System.Drawing.Point(3, 10);
            this.labelApiUsername.Name = "labelApiUsername";
            this.labelApiUsername.Size = new System.Drawing.Size(104, 19);
            this.labelApiUsername.TabIndex = 0;
            this.labelApiUsername.Text = "API Username:";
            // 
            // textBoxApiUsername
            // 
            this.textBoxApiUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxApiUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxApiUsername.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.textBoxApiUsername.Location = new System.Drawing.Point(113, 8);
            this.textBoxApiUsername.Name = "textBoxApiUsername";
            this.textBoxApiUsername.Size = new System.Drawing.Size(154, 23);
            this.textBoxApiUsername.TabIndex = 1;
            this.textBoxApiUsername.Text = "781028068";
            // 
            // labelApiPassword
            // 
            this.labelApiPassword.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelApiPassword.AutoSize = true;
            this.labelApiPassword.Location = new System.Drawing.Point(3, 50);
            this.labelApiPassword.Name = "labelApiPassword";
            this.labelApiPassword.Size = new System.Drawing.Size(98, 19);
            this.labelApiPassword.TabIndex = 2;
            this.labelApiPassword.Text = "API Password:";
            // 
            // textBoxApiPassword
            // 
                      // textBoxApiPassword
            // 
            this.textBoxApiPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxApiPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxApiPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.textBoxApiPassword.Location = new System.Drawing.Point(113, 48);
            this.textBoxApiPassword.Name = "textBoxApiPassword";
            this.textBoxApiPassword.Size = new System.Drawing.Size(154, 23);
            this.textBoxApiPassword.TabIndex = 3;
            this.textBoxApiPassword.Text = "781028068";
            this.textBoxApiPassword.UseSystemPasswordChar = true;
            // 
            // labelOwnerNumber
            // 
            this.labelOwnerNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelOwnerNumber.AutoSize = true;
            this.labelOwnerNumber.Location = new System.Drawing.Point(3, 90);
            this.labelOwnerNumber.Name = "labelOwnerNumber";
            this.labelOwnerNumber.Size = new System.Drawing.Size(104, 19);
            this.labelOwnerNumber.TabIndex = 4;
            this.labelOwnerNumber.Text = "Owner Number:";
            // 
            // phonePanel
            // 
            this.phonePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.phonePanel.Controls.Add(this.comboBoxCountryCode);
            this.phonePanel.Controls.Add(this.textBoxPhoneNumber);
            this.phonePanel.Location = new System.Drawing.Point(113, 83);
            this.phonePanel.Name = "phonePanel";
            this.phonePanel.Size = new System.Drawing.Size(154, 34);
            this.phonePanel.TabIndex = 5;
            // 
            // comboBoxCountryCode
            // 
            this.comboBoxCountryCode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCountryCode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.comboBoxCountryCode.FormattingEnabled = true;
            this.comboBoxCountryCode.Location = new System.Drawing.Point(0, 5);
            this.comboBoxCountryCode.Name = "comboBoxCountryCode";
            this.comboBoxCountryCode.Size = new System.Drawing.Size(60, 23);
            this.comboBoxCountryCode.TabIndex = 0;
            // 
            // textBoxPhoneNumber
            // 
            this.textBoxPhoneNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPhoneNumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxPhoneNumber.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.textBoxPhoneNumber.Location = new System.Drawing.Point(66, 5);
            this.textBoxPhoneNumber.Name = "textBoxPhoneNumber";
            this.textBoxPhoneNumber.Size = new System.Drawing.Size(88, 23);
            this.textBoxPhoneNumber.TabIndex = 1;
            this.textBoxPhoneNumber.Text = "733300785";
            // 
            // buttonsFlowLayout
            // 
            this.credentialsTableLayout.SetColumnSpan(this.buttonsFlowLayout, 2);
            this.buttonsFlowLayout.Controls.Add(this.linkWhatsappButton);
            this.buttonsFlowLayout.Controls.Add(this.btnManageGroups);
            this.buttonsFlowLayout.Controls.Add(this.btnClearCacheAndLogs);
            this.buttonsFlowLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonsFlowLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.buttonsFlowLayout.Location = new System.Drawing.Point(3, 123);
            this.buttonsFlowLayout.Name = "buttonsFlowLayout";
            this.buttonsFlowLayout.Size = new System.Drawing.Size(264, 176);
            this.buttonsFlowLayout.TabIndex = 6;
            // 
            // linkWhatsappButton
            // 
            this.linkWhatsappButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(211)))), ((int)(((byte)(102)))));
            this.linkWhatsappButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkWhatsappButton.Enabled = false;
            this.linkWhatsappButton.FlatAppearance.BorderSize = 0;
            this.linkWhatsappButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(190)))), ((int)(((byte)(90)))));
            this.linkWhatsappButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(230)))), ((int)(((byte)(112)))));
            this.linkWhatsappButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.linkWhatsappButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.linkWhatsappButton.ForeColor = System.Drawing.Color.White;
            this.linkWhatsappButton.Location = new System.Drawing.Point(3, 3);
            this.linkWhatsappButton.Name = "linkWhatsappButton";
            this.linkWhatsappButton.Size = new System.Drawing.Size(258, 40);
            this.linkWhatsappButton.TabIndex = 0;
            this.linkWhatsappButton.Text = "Link New WhatsApp Account";
            this.linkWhatsappButton.UseVisualStyleBackColor = false;
            // 
            // btnManageGroups
            // 
            this.btnManageGroups.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(140)))), ((int)(((byte)(126)))));
            this.btnManageGroups.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManageGroups.Enabled = false;
            this.btnManageGroups.FlatAppearance.BorderSize = 0;
            this.btnManageGroups.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(120)))), ((int)(((byte)(108)))));
            this.btnManageGroups.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(160)))), ((int)(((byte)(144)))));
            this.btnManageGroups.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageGroups.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnManageGroups.ForeColor = System.Drawing.Color.White;
            this.btnManageGroups.Location = new System.Drawing.Point(3, 49);
            this.btnManageGroups.Name = "btnManageGroups";
            this.btnManageGroups.Size = new System.Drawing.Size(258, 35);
            this.btnManageGroups.TabIndex = 1;
            this.btnManageGroups.Text = "Manage Groups for Selected Client";
            this.btnManageGroups.UseVisualStyleBackColor = false;
            // 
            // btnClearCacheAndLogs
            // 
            this.btnClearCacheAndLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(79)))), ((int)(((byte)(79)))));
            this.btnClearCacheAndLogs.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearCacheAndLogs.Enabled = false;
            this.btnClearCacheAndLogs.FlatAppearance.BorderSize = 0;
            this.btnClearCacheAndLogs.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.btnClearCacheAndLogs.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.btnClearCacheAndLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearCacheAndLogs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnClearCacheAndLogs.ForeColor = System.Drawing.Color.White;
            this.btnClearCacheAndLogs.Location = new System.Drawing.Point(3, 90);
            this.btnClearCacheAndLogs.Name = "btnClearCacheAndLogs";
            this.btnClearCacheAndLogs.Size = new System.Drawing.Size(258, 35);
            this.btnClearCacheAndLogs.TabIndex = 2;
            this.btnClearCacheAndLogs.Text = "Clear Client Cache && Logs";
            this.btnClearCacheAndLogs.UseVisualStyleBackColor = false;
            // 
            // rightPanel
            // 
            this.rightPanel.Controls.Add(this.rightSplitContainer);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Location = new System.Drawing.Point(0, 0);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Padding = new System.Windows.Forms.Padding(0, 10, 10, 10);
            this.rightPanel.Size = new System.Drawing.Size(694, 687);
            this.rightPanel.TabIndex = 0;
            // 
            // rightSplitContainer
            // 
            this.rightSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightSplitContainer.Location = new System.Drawing.Point(0, 10);
            this.rightSplitContainer.Name = "rightSplitContainer";
            this.rightSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // rightSplitContainer.Panel1
            // 
            this.rightSplitContainer.Panel1.Controls.Add(this.groupBoxInstances);
            this.rightSplitContainer.Panel1MinSize = 200;
            // 
            // rightSplitContainer.Panel2
            // 
            this.rightSplitContainer.Panel2.Controls.Add(this.groupBoxLogs);
            this.rightSplitContainer.Panel2MinSize = 150;
            this.rightSplitContainer.Size = new System.Drawing.Size(684, 667);
            this.rightSplitContainer.SplitterDistance = 350;
            this.rightSplitContainer.TabIndex = 0;
            // 
            // groupBoxInstances
            // 
            this.groupBoxInstances.BackColor = System.Drawing.Color.White;
            this.groupBoxInstances.Controls.Add(this.instancesTableLayout);
            this.groupBoxInstances.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxInstances.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBoxInstances.Location = new System.Drawing.Point(0, 0);
            this.groupBoxInstances.Name = "groupBoxInstances";
            this.groupBoxInstances.Padding = new System.Windows.Forms.Padding(15);
            this.groupBoxInstances.Size = new System.Drawing.Size(684, 350);
            this.groupBoxInstances.TabIndex = 0;
            this.groupBoxInstances.TabStop = false;
            this.groupBoxInstances.Text = "Bot Instances";
            // 
            // instancesTableLayout
            // 
            this.instancesTableLayout.ColumnCount = 1;
            this.instancesTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.instancesTableLayout.Controls.Add(this.instanceListView, 0, 0);
            this.instancesTableLayout.Controls.Add(this.instanceButtonsFlow, 0, 1);
            this.instancesTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.instancesTableLayout.Location = new System.Drawing.Point(15, 33);
            this.instancesTableLayout.Name = "instancesTableLayout";
            this.instancesTableLayout.RowCount = 2;
            this.instancesTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.instancesTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.instancesTableLayout.Size = new System.Drawing.Size(654, 302);
            this.instancesTableLayout.TabIndex = 0;
            // 
            // instanceListView
            // 
            this.instanceListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.instanceListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colClientId,
            this.colPhoneNumber,
            this.colName,
            this.colStatus});
            this.instanceListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.instanceListView.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.instanceListView.FullRowSelect = true;
            this.instanceListView.GridLines = true;
            this.instanceListView.HideSelection = false;
            this.instanceListView.Location = new System.Drawing.Point(3, 3);
            this.instanceListView.MultiSelect = false;
            this.instanceListView.Name = "instanceListView";
            this.instanceListView.OwnerDraw = true;
            this.instanceListView.Size = new System.Drawing.Size(648, 251);
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
            this.colName.Width = 150;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 120;
            // 
            // instanceButtonsFlow
            // 
            this.instanceButtonsFlow.Controls.Add(this.btnRefreshInstances);
            this.instanceButtonsFlow.Controls.Add(this.btnStartInstance);
            this.instanceButtonsFlow.Controls.Add(this.btnStopInstance);
            this.instanceButtonsFlow.Controls.Add(this.btnRestartInstance);
            this.instanceButtonsFlow.Controls.Add(this.btnStopAndDeleteInstance);
            this.instanceButtonsFlow.Controls.Add(this.btnGetLogs);
            this.instanceButtonsFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.instanceButtonsFlow.Location = new System.Drawing.Point(3, 260);
            this.instanceButtonsFlow.Name = "instanceButtonsFlow";
            this.instanceButtonsFlow.Size = new System.Drawing.Size(648, 39);
            this.instanceButtonsFlow.TabIndex = 1;
            // 
            // btnRefreshInstances
            // 
            this.btnRefreshInstances.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(140)))), ((int)(((byte)(126)))));
            this.btnRefreshInstances.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefreshInstances.FlatAppearance.BorderSize = 0;
            this.btnRefreshInstances.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(120)))), ((int)(((byte)(108)))));
            this.btnRefreshInstances.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(160)))), ((int)(((byte)(144)))));
            this.btnRefreshInstances.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefreshInstances.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRefreshInstances.ForeColor = System.Drawing.Color.White;
            this.btnRefreshInstances.Location = new System.Drawing.Point(3, 3);
            this.btnRefreshInstances.Name = "btnRefreshInstances";
            this.btnRefreshInstances.Size = new System.Drawing.Size(100, 32);
            this.btnRefreshInstances.TabIndex = 0;
            this.btnRefreshInstances.Text = "Refresh List";
            this.btnRefreshInstances.UseVisualStyleBackColor = false;
            // 
            // btnStartInstance
            // 
            this.btnStartInstance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(211)))), ((int)(((byte)(102)))));
            this.btnStartInstance.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStartInstance.FlatAppearance.BorderSize = 0;
            this.btnStartInstance.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(190)))), ((int)(((byte)(90)))));
            this.btnStartInstance.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(230)))), ((int)(((byte)(112)))));
            this.btnStartInstance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartInstance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnStartInstance.ForeColor = System.Drawing.Color.White;
            this.btnStartInstance.Location = new System.Drawing.Point(109, 3);
            this.btnStartInstance.Name = "btnStartInstance";
            this.btnStartInstance.Size = new System.Drawing.Size(75, 32);
            this.btnStartInstance.TabIndex = 1;
            this.btnStartInstance.Text = "Start";
            this.btnStartInstance.UseVisualStyleBackColor = false;
            // 
            // btnStopInstance
            // 
            this.btnStopInstance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
            this.btnStopInstance.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStopInstance.FlatAppearance.BorderSize = 0;
            this.btnStopInstance.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(82)))), ((int)(((byte)(82)))));
            this.btnStopInstance.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.btnStopInstance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopInstance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnStopInstance.ForeColor = System.Drawing.Color.White;
            this.btnStopInstance.Location = new System.Drawing.Point(190, 3);
            this.btnStopInstance.Name = "btnStopInstance";
            this.btnStopInstance.Size = new System.Drawing.Size(75, 32);
            this.btnStopInstance.TabIndex = 2;
            this.btnStopInstance.Text = "Stop";
            this.btnStopInstance.UseVisualStyleBackColor = false;
            // 
            // btnRestartInstance
            // 
            this.btnRestartInstance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(165)))), ((int)(((byte)(32)))));
            this.btnRestartInstance.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRestartInstance.FlatAppearance.BorderSize = 0;
            this.btnRestartInstance.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(145)))), ((int)(((byte)(28)))));
            this.btnRestartInstance.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(185)))), ((int)(((byte)(36)))));
            this.btnRestartInstance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestartInstance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRestartInstance.ForeColor = System.Drawing.Color.White;
            this.btnRestartInstance.Location = new System.Drawing.Point(271, 3);
            this.btnRestartInstance.Name = "btnRestartInstance";
            this.btnRestartInstance.Size = new System.Drawing.Size(85, 32);
            this.btnRestartInstance.TabIndex = 3;
            this.btnRestartInstance.Text = "Restart";
            this.btnRestartInstance.UseVisualStyleBackColor = false;
            // 
            // btnStopAndDeleteInstance
            // 
            this.btnStopAndDeleteInstance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(34)))), ((int)(((byte)(34)))));
            this.btnStopAndDeleteInstance.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStopAndDeleteInstance.FlatAppearance.BorderSize = 0;
            this.btnStopAndDeleteInstance.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.btnStopAndDeleteInstance.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.btnStopAndDeleteInstance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopAndDeleteInstance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnStopAndDeleteInstance.ForeColor = System.Drawing.Color.White;
            this.btnStopAndDeleteInstance.Location = new System.Drawing.Point(362, 3);
            this.btnStopAndDeleteInstance.Name = "btnStopAndDeleteInstance";
            this.btnStopAndDeleteInstance.Size = new System.Drawing.Size(120, 32);
            this.btnStopAndDeleteInstance.TabIndex = 4;
            this.btnStopAndDeleteInstance.Text = "Stop && Delete";
            this.btnStopAndDeleteInstance.UseVisualStyleBackColor = false;
            // 
            // btnGetLogs
            // 
            this.btnGetLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.btnGetLogs.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGetLogs.FlatAppearance.BorderSize = 0;
            this.btnGetLogs.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(120)))), ((int)(((byte)(170)))));
            this.btnGetLogs.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(140)))), ((int)(((byte)(190)))));
            this.btnGetLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGetLogs.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnGetLogs.ForeColor = System.Drawing.Color.White;
            this.btnGetLogs.Location = new System.Drawing.Point(488, 3);
            this.btnGetLogs.Name = "btnGetLogs";
            this.btnGetLogs.Size = new System.Drawing.Size(85, 32);
            this.btnGetLogs.TabIndex = 5;
            this.btnGetLogs.Text = "Get Logs";
            this.btnGetLogs.UseVisualStyleBackColor = false;
            // 
            // groupBoxLogs
            // 
            this.groupBoxLogs.BackColor = System.Drawing.Color.White;
            this.groupBoxLogs.Controls.Add(this.logTextBox);
            this.groupBoxLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxLogs.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBoxLogs.Location = new System.Drawing.Point(0, 0);
            this.groupBoxLogs.Name = "groupBoxLogs";
            this.groupBoxLogs.Padding = new System.Windows.Forms.Padding(15);
            this.groupBoxLogs.Size = new System.Drawing.Size(684, 313);
            this.groupBoxLogs.TabIndex = 0;
            this.groupBoxLogs.TabStop = false;
            this.groupBoxLogs.Text = "Instance Logs";
            // 
            // logTextBox
            // 
            this.logTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(44)))), ((int)(((byte)(51)))));
            this.logTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logTextBox.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.logTextBox.Location = new System.Drawing.Point(15, 33);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(654, 265);
            this.logTextBox.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.mainTableLayout);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WhatsApp Bot Manager";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.mainTableLayout.ResumeLayout(false);
            this.mainTableLayout.PerformLayout();
            this.topPanel.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.leftPanel.ResumeLayout(false);
            this.groupBoxCredentials.ResumeLayout(false);
            this.credentialsTableLayout.ResumeLayout(false);
            this.credentialsTableLayout.PerformLayout();
            this.phonePanel.ResumeLayout(false);
            this.phonePanel.PerformLayout();
            this.buttonsFlowLayout.ResumeLayout(false);
            this.rightPanel.ResumeLayout(false);
            this.rightSplitContainer.Panel1.ResumeLayout(false);
            this.rightSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.rightSplitContainer)).EndInit();
            this.rightSplitContainer.ResumeLayout(false);
            this.groupBoxInstances.ResumeLayout(false);
            this.instancesTableLayout.ResumeLayout(false);
            this.instanceButtonsFlow.ResumeLayout(false);
            this.groupBoxLogs.ResumeLayout(false);
            this.groupBoxLogs.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayout;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblConnectionStatus;
                private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.GroupBox groupBoxCredentials;
        private System.Windows.Forms.TableLayoutPanel credentialsTableLayout;
        private System.Windows.Forms.Label labelApiUsername;
        private System.Windows.Forms.TextBox textBoxApiUsername;
        private System.Windows.Forms.Label labelApiPassword;
        private System.Windows.Forms.TextBox textBoxApiPassword;
        private System.Windows.Forms.Label labelOwnerNumber;
        private System.Windows.Forms.Panel phonePanel;
        private System.Windows.Forms.ComboBox comboBoxCountryCode;
        private System.Windows.Forms.TextBox textBoxPhoneNumber;
        private System.Windows.Forms.FlowLayoutPanel buttonsFlowLayout;
        private System.Windows.Forms.Button linkWhatsappButton;
        private System.Windows.Forms.Button btnManageGroups;
        private System.Windows.Forms.Button btnClearCacheAndLogs;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.SplitContainer rightSplitContainer;
        private System.Windows.Forms.GroupBox groupBoxInstances;
        private System.Windows.Forms.TableLayoutPanel instancesTableLayout;
        private System.Windows.Forms.ListView instanceListView;
        private System.Windows.Forms.ColumnHeader colClientId;
        private System.Windows.Forms.ColumnHeader colPhoneNumber;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.FlowLayoutPanel instanceButtonsFlow;
        private System.Windows.Forms.Button btnRefreshInstances;
        private System.Windows.Forms.Button btnStartInstance;
        private System.Windows.Forms.Button btnStopInstance;
        private System.Windows.Forms.Button btnRestartInstance;
        private System.Windows.Forms.Button btnStopAndDeleteInstance;
        private System.Windows.Forms.Button btnGetLogs;
        private System.Windows.Forms.GroupBox groupBoxLogs;
        private System.Windows.Forms.TextBox logTextBox;
    }
}