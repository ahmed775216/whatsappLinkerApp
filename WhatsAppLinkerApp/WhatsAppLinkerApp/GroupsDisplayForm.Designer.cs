// GroupsDisplayForm.Designer.cs
namespace WhatsAppLinkerApp
{
    partial class GroupsDisplayForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            // System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GroupsDisplayForm)); // ADDED for toolstrip images
            this.labelSelectedClient = new System.Windows.Forms.Label();
            this.groupBoxGroups = new System.Windows.Forms.GroupBox();
            this.txtSearchGroup = new System.Windows.Forms.TextBox(); // ADDED here
            this.groupsListView = new System.Windows.Forms.ListView();
            this.columnHeaderGroupId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderGroupName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderParticipants = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderWhitelisted = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnFetchGroups = new System.Windows.Forms.Button();
            this.btnWhitelistGroup = new System.Windows.Forms.Button();
            this.btnFetchParticipants = new System.Windows.Forms.Button();
            
            // Enhanced Participants Area
            this.panelParticipants = new System.Windows.Forms.Panel();
            this.participantsHeaderPanel = new System.Windows.Forms.Panel();
            this.lblParticipantsTitle = new System.Windows.Forms.Label();
            this.lblParticipantsCount = new System.Windows.Forms.Label();
            this.txtSearchParticipant = new System.Windows.Forms.TextBox();
            this.participantsListView = new System.Windows.Forms.ListView(); // Changed to ListView
            this.colParticipantName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colParticipantPhone = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colParticipantRole = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colParticipantStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.participantsToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonWhitelist = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonExport = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRefresh = new System.Windows.Forms.ToolStripButton();
            this.participantsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyPhoneNumberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.whitelistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            
            this.groupBoxGroups.SuspendLayout();
            this.panelParticipants.SuspendLayout();
            this.participantsHeaderPanel.SuspendLayout();
            this.participantsToolStrip.SuspendLayout();
            this.participantsContextMenu.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // labelSelectedClient
            // 
            this.labelSelectedClient.AutoSize = true;
            this.labelSelectedClient.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.labelSelectedClient.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(140)))), ((int)(((byte)(126)))));
            this.labelSelectedClient.Location = new System.Drawing.Point(12, 9);
            this.labelSelectedClient.Name = "labelSelectedClient";
            this.labelSelectedClient.Size = new System.Drawing.Size(130, 19);
            this.labelSelectedClient.TabIndex = 0;
            this.labelSelectedClient.Text = "Groups for Client:";
            
            // 
            // groupBoxGroups
            // 
            this.groupBoxGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxGroups.Controls.Add(this.txtSearchGroup); // ADDED control
            this.groupBoxGroups.Controls.Add(this.groupsListView);
            this.groupBoxGroups.Controls.Add(this.btnFetchGroups);
            this.groupBoxGroups.Controls.Add(this.btnWhitelistGroup);
            this.groupBoxGroups.Controls.Add(this.btnFetchParticipants);
            this.groupBoxGroups.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.groupBoxGroups.Location = new System.Drawing.Point(12, 40);
            this.groupBoxGroups.Name = "groupBoxGroups";
            this.groupBoxGroups.Size = new System.Drawing.Size(450, 500);
            this.groupBoxGroups.TabIndex = 1;
            this.groupBoxGroups.TabStop = false;
            this.groupBoxGroups.Text = "Groups";
            
            // txtSearchGroup
            //
            this.txtSearchGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearchGroup.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearchGroup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearchGroup.Location = new System.Drawing.Point(15, 25); // Position relative to GroupBox
            this.txtSearchGroup.Name = "txtSearchGroup";
            this.txtSearchGroup.Size = new System.Drawing.Size(420, 23); // Adjust size as per GroupBox width
            this.txtSearchGroup.TabIndex = 4; // Or appropriate index
            this.txtSearchGroup.PlaceholderText = "Search by group name..."; // Placeholder
            this.txtSearchGroup.TextChanged += new System.EventHandler(this.txtSearchGroup_TextChanged); // Event hook
            
            // 
            // groupsListView
            // 
            this.groupsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderGroupId,
            this.columnHeaderGroupName,
            this.columnHeaderParticipants,
            this.columnHeaderWhitelisted});
            this.groupsListView.FullRowSelect = true;
            this.groupsListView.GridLines = true;
            this.groupsListView.HideSelection = false;
            this.groupsListView.Location = new System.Drawing.Point(15, 55); // Adjusted location to be below search box
            this.groupsListView.MultiSelect = false;
            this.groupsListView.Name = "groupsListView";
            this.groupsListView.Size = new System.Drawing.Size(420, 390); // Adjusted height
            this.groupsListView.TabIndex = 0;
            this.groupsListView.UseCompatibleStateImageBehavior = false;
            this.groupsListView.View = System.Windows.Forms.View.Details;
            this.groupsListView.SelectedIndexChanged += new System.EventHandler(this.groupsListView_SelectedIndexChanged);
            
            // 
            // columnHeaderGroupId
            // 
            this.columnHeaderGroupId.Text = "Group ID";
            this.columnHeaderGroupId.Width = 120;
            
            // 
            // columnHeaderGroupName
            // 
            this.columnHeaderGroupName.Text = "Group Name";
            this.columnHeaderGroupName.Width = 150;
            
            // 
            // columnHeaderParticipants
            // 
            this.columnHeaderParticipants.Text = "Members";
            this.columnHeaderParticipants.Width = 60;
            
            // 
            // columnHeaderWhitelisted
            // 
            this.columnHeaderWhitelisted.Text = "Whitelisted";
            this.columnHeaderWhitelisted.Width = 70;
            
            // 
            // btnFetchGroups
            // 
            this.btnFetchGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnFetchGroups.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(140)))), ((int)(((byte)(126)))));
            this.btnFetchGroups.FlatAppearance.BorderSize = 0;
            this.btnFetchGroups.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFetchGroups.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnFetchGroups.ForeColor = System.Drawing.Color.White;
            this.btnFetchGroups.Location = new System.Drawing.Point(15, 455);
            this.btnFetchGroups.Name = "btnFetchGroups";
            this.btnFetchGroups.Size = new System.Drawing.Size(120, 30);
            this.btnFetchGroups.TabIndex = 1;
            this.btnFetchGroups.Text = "Fetch Groups";
            this.btnFetchGroups.UseVisualStyleBackColor = false;
            this.btnFetchGroups.Click += new System.EventHandler(this.btnFetchGroups_Click);
            
            // 
            // btnWhitelistGroup
            // 
            this.btnWhitelistGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnWhitelistGroup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(211)))), ((int)(((byte)(102)))));
            this.btnWhitelistGroup.Enabled = false;
            this.btnWhitelistGroup.FlatAppearance.BorderSize = 0;
            this.btnWhitelistGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWhitelistGroup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnWhitelistGroup.ForeColor = System.Drawing.Color.White;
            this.btnWhitelistGroup.Location = new System.Drawing.Point(141, 455);
            this.btnWhitelistGroup.Name = "btnWhitelistGroup";
            this.btnWhitelistGroup.Size = new System.Drawing.Size(130, 30);
            this.btnWhitelistGroup.TabIndex = 2;
            this.btnWhitelistGroup.Text = "Add Whitelist";
            this.btnWhitelistGroup.UseVisualStyleBackColor = false;
            this.btnWhitelistGroup.Click += new System.EventHandler(this.btnWhitelistGroup_Click);
            
            // 
            // btnFetchParticipants
            // 
            this.btnFetchParticipants.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnFetchParticipants.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.btnFetchParticipants.Enabled = false;
            this.btnFetchParticipants.FlatAppearance.BorderSize = 0;
            this.btnFetchParticipants.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFetchParticipants.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnFetchParticipants.ForeColor = System.Drawing.Color.White;
            this.btnFetchParticipants.Location = new System.Drawing.Point(277, 455);
            this.btnFetchParticipants.Name = "btnFetchParticipants";
            this.btnFetchParticipants.Size = new System.Drawing.Size(140, 30);
            this.btnFetchParticipants.TabIndex = 3;
            this.btnFetchParticipants.Text = "Fetch Participants";
            this.btnFetchParticipants.UseVisualStyleBackColor = false;
            this.btnFetchParticipants.Click += new System.EventHandler(this.btnFetchParticipants_Click);
            
            // 
            // panelParticipants
            // 
            this.panelParticipants.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelParticipants.BackColor = System.Drawing.Color.White;
            this.panelParticipants.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelParticipants.Controls.Add(this.participantsHeaderPanel);
            this.panelParticipants.Controls.Add(this.txtSearchParticipant);
            this.panelParticipants.Controls.Add(this.participantsListView);
            this.panelParticipants.Controls.Add(this.participantsToolStrip);
            this.panelParticipants.Location = new System.Drawing.Point(468, 40);
            this.panelParticipants.Name = "panelParticipants";
            this.panelParticipants.Size = new System.Drawing.Size(500, 500);
            this.panelParticipants.TabIndex = 2;
            
            // 
            // participantsHeaderPanel
            // 
            this.participantsHeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.participantsHeaderPanel.Controls.Add(this.lblParticipantsTitle);
            this.participantsHeaderPanel.Controls.Add(this.lblParticipantsCount);
            this.participantsHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.participantsHeaderPanel.Location = new System.Drawing.Point(0, 0);
            this.participantsHeaderPanel.Name = "participantsHeaderPanel";
            this.participantsHeaderPanel.Size = new System.Drawing.Size(498, 40);
            this.participantsHeaderPanel.TabIndex = 0;
            
            // 
            // lblParticipantsTitle
            // 
            this.lblParticipantsTitle.AutoSize = true;
            this.lblParticipantsTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblParticipantsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblParticipantsTitle.Location = new System.Drawing.Point(10, 10);
            
            this.lblParticipantsTitle.Name = "lblParticipantsTitle";
            this.lblParticipantsTitle.Size = new System.Drawing.Size(89, 20);
            this.lblParticipantsTitle.TabIndex = 0;
            this.lblParticipantsTitle.Text = "Participants";
            
            // 
            // lblParticipantsCount
            // 
            this.lblParticipantsCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblParticipantsCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblParticipantsCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.lblParticipantsCount.Location = new System.Drawing.Point(350, 12);
            this.lblParticipantsCount.Name = "lblParticipantsCount";
            this.lblParticipantsCount.Size = new System.Drawing.Size(140, 15);
            this.lblParticipantsCount.TabIndex = 1;
            this.lblParticipantsCount.Text = "Total: 0";
            this.lblParticipantsCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            
            // 
            // txtSearchParticipant
            // 
            this.txtSearchParticipant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearchParticipant.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearchParticipant.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearchParticipant.Location = new System.Drawing.Point(10, 46);
            this.txtSearchParticipant.Name = "txtSearchParticipant";
            this.txtSearchParticipant.Size = new System.Drawing.Size(478, 23);
            this.txtSearchParticipant.TabIndex = 1;
            this.txtSearchParticipant.TextChanged += new System.EventHandler(this.txtSearchParticipant_TextChanged); // Event hook
            
            // 
            // participantsListView
            // 
            this.participantsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.participantsListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.participantsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colParticipantName,
            this.colParticipantPhone,
            this.colParticipantRole,
            this.colParticipantStatus});
            this.participantsListView.ContextMenuStrip = this.participantsContextMenu;
            this.participantsListView.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.participantsListView.FullRowSelect = true;
            this.participantsListView.GridLines = true;
            this.participantsListView.HideSelection = false;
            this.participantsListView.Location = new System.Drawing.Point(10, 75);
            this.participantsListView.Name = "participantsListView";
            this.participantsListView.Size = new System.Drawing.Size(478, 387);
            this.participantsListView.TabIndex = 2;
            this.participantsListView.UseCompatibleStateImageBehavior = false;
            this.participantsListView.View = System.Windows.Forms.View.Details;
            this.participantsListView.SelectedIndexChanged += new System.EventHandler(this.participantsListView_SelectedIndexChanged);
            this.participantsListView.DoubleClick += new System.EventHandler(this.participantsListView_DoubleClick);
            
            // 
            // colParticipantName
            // 
            this.colParticipantName.Text = "Name";
            this.colParticipantName.Width = 180;
            
            // 
            // colParticipantPhone
            // 
            this.colParticipantPhone.Text = "Phone Number";
            this.colParticipantPhone.Width = 130;
            
            // 
            // colParticipantRole
            // 
            this.colParticipantRole.Text = "Role";
            this.colParticipantRole.Width = 80;
            
            // 
            // colParticipantStatus
            // 
            this.colParticipantStatus.Text = "Status";
            this.colParticipantStatus.Width = 80;
            
            // 
            // participantsToolStrip
            // 
            this.participantsToolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.participantsToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.participantsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonWhitelist,
            this.toolStripSeparator1,
            this.toolStripButtonExport,
            this.toolStripButtonRefresh});
            this.participantsToolStrip.Location = new System.Drawing.Point(0, 468);
            this.participantsToolStrip.Name = "participantsToolStrip";
            this.participantsToolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.participantsToolStrip.Size = new System.Drawing.Size(498, 30);
            this.participantsToolStrip.TabIndex = 3;
            this.participantsToolStrip.Text = "toolStrip1";
            
            // 
            // toolStripButtonWhitelist
            // 
            this.toolStripButtonWhitelist.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(211)))), ((int)(((byte)(102)))));
            this.toolStripButtonWhitelist.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonWhitelist.Enabled = false;
            this.toolStripButtonWhitelist.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripButtonWhitelist.ForeColor = System.Drawing.Color.White;
            // this.toolStripButtonWhitelist.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonWhitelist.Image"))); // ADDED
            this.toolStripButtonWhitelist.ImageTransparentColor = System.Drawing.Color.Magenta; // ADDED
            this.toolStripButtonWhitelist.Margin = new System.Windows.Forms.Padding(0, 2, 2, 2);
            this.toolStripButtonWhitelist.Name = "toolStripButtonWhitelist";
            this.toolStripButtonWhitelist.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.toolStripButtonWhitelist.Size = new System.Drawing.Size(104, 26);
            this.toolStripButtonWhitelist.Text = "Add Whitelist";
            this.toolStripButtonWhitelist.Click += new System.EventHandler(this.toolStripButtonWhitelist_Click);
            
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 30);
            
            // 
            // toolStripButtonExport
            // 
            this.toolStripButtonExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonExport.Font = new System.Drawing.Font("Segoe UI", 9F);
            // this.toolStripButtonExport.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonExport.Image"))); // ADDED
            this.toolStripButtonExport.ImageTransparentColor = System.Drawing.Color.Magenta; // ADDED
            this.toolStripButtonExport.Name = "toolStripButtonExport";
            this.toolStripButtonExport.Size = new System.Drawing.Size(45, 27);
            this.toolStripButtonExport.Text = "Export";
            this.toolStripButtonExport.ToolTipText = "Export participants list";
            this.toolStripButtonExport.Click += new System.EventHandler(this.toolStripButtonExport_Click);
            
            // 
            // toolStripButtonRefresh
            // 
            this.toolStripButtonRefresh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
            // this.toolStripButtonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRefresh.Image"))); // ADDED
            this.toolStripButtonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta; // ADDED
            this.toolStripButtonRefresh.Name = "toolStripButtonRefresh";
            this.toolStripButtonRefresh.Size = new System.Drawing.Size(50, 27);
            this.toolStripButtonRefresh.Text = "Refresh";
            this.toolStripButtonRefresh.Click += new System.EventHandler(this.toolStripButtonRefresh_Click);
            
            // 
            // participantsContextMenu
            // 
            this.participantsContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyPhoneNumberToolStripMenuItem,
            this.copyNameToolStripMenuItem,
            this.toolStripSeparator2,
            this.whitelistToolStripMenuItem});
            this.participantsContextMenu.Name = "participantsContextMenu";
            this.participantsContextMenu.Size = new System.Drawing.Size(185, 76);
            this.participantsContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.participantsContextMenu_Opening); // ADDED
            
            // 
            // copyPhoneNumberToolStripMenuItem
            // 
            this.copyPhoneNumberToolStripMenuItem.Name = "copyPhoneNumberToolStripMenuItem";
            this.copyPhoneNumberToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.copyPhoneNumberToolStripMenuItem.Text = "Copy Phone Number";
            this.copyPhoneNumberToolStripMenuItem.Click += new System.EventHandler(this.copyPhoneNumberToolStripMenuItem_Click);
            
            // 
            // copyNameToolStripMenuItem
            // 
            this.copyNameToolStripMenuItem.Name = "copyNameToolStripMenuItem";
            this.copyNameToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.copyNameToolStripMenuItem.Text = "Copy Name";
            this.copyNameToolStripMenuItem.Click += new System.EventHandler(this.copyNameToolStripMenuItem_Click);
            
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(181, 6);
            
            // 
            // whitelistToolStripMenuItem
            // 
            this.whitelistToolStripMenuItem.Name = "whitelistToolStripMenuItem";
            this.whitelistToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.whitelistToolStripMenuItem.Text = "Toggle Whitelist";
            this.whitelistToolStripMenuItem.Click += new System.EventHandler(this.whitelistToolStripMenuItem_Click);
            
            // 
            // GroupsDisplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(980, 552);
            this.Controls.Add(this.panelParticipants);
            this.Controls.Add(this.groupBoxGroups);
            this.Controls.Add(this.labelSelectedClient);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(996, 591);
            this.Name = "GroupsDisplayForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WhatsApp Groups Manager";
            this.Load += new System.EventHandler(this.GroupsDisplayForm_Load);
            this.groupBoxGroups.ResumeLayout(false);
            this.groupBoxGroups.PerformLayout(); // ADDED here for txtSearchGroup
            this.panelParticipants.ResumeLayout(false);
            this.panelParticipants.PerformLayout();
            this.participantsHeaderPanel.ResumeLayout(false);
            this.participantsHeaderPanel.PerformLayout();
            this.participantsToolStrip.ResumeLayout(false);
            this.participantsToolStrip.PerformLayout();
            this.participantsContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelSelectedClient;
        private System.Windows.Forms.GroupBox groupBoxGroups;
        private System.Windows.Forms.TextBox txtSearchGroup; // DECLARE HERE
        private System.Windows.Forms.ListView groupsListView;
        private System.Windows.Forms.ColumnHeader columnHeaderGroupId;
        private System.Windows.Forms.ColumnHeader columnHeaderGroupName;
        private System.Windows.Forms.ColumnHeader columnHeaderParticipants;
        private System.Windows.Forms.ColumnHeader columnHeaderWhitelisted;
        private System.Windows.Forms.Button btnFetchGroups;
        private System.Windows.Forms.Button btnWhitelistGroup;
        private System.Windows.Forms.Button btnFetchParticipants;
        
        // Enhanced Participants Area
        private System.Windows.Forms.Panel panelParticipants;
        private System.Windows.Forms.Panel participantsHeaderPanel;
        private System.Windows.Forms.Label lblParticipantsTitle;
        private System.Windows.Forms.Label lblParticipantsCount;
        private System.Windows.Forms.TextBox txtSearchParticipant;
        private System.Windows.Forms.ListView participantsListView;
        private System.Windows.Forms.ColumnHeader colParticipantName;
        private System.Windows.Forms.ColumnHeader colParticipantPhone;
        private System.Windows.Forms.ColumnHeader colParticipantRole;
        private System.Windows.Forms.ColumnHeader colParticipantStatus;
        private System.Windows.Forms.ToolStrip participantsToolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButtonWhitelist;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonExport;
        private System.Windows.Forms.ToolStripButton toolStripButtonRefresh;
        private System.Windows.Forms.ContextMenuStrip participantsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyPhoneNumberToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem whitelistToolStripMenuItem;
    }
}