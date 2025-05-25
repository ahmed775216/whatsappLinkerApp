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
            this.groupBoxGroups = new System.Windows.Forms.GroupBox();
            this.groupsListView = new System.Windows.Forms.ListView();
            this.colGroupId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colGroupName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colParticipantsCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colGroupWhitelisted = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader())); // NEW COLUMN
            this.btnFetchGroups = new System.Windows.Forms.Button();
            this.btnWhitelistGroup = new System.Windows.Forms.Button();
            this.btnFetchParticipants = new System.Windows.Forms.Button();
            this.labelSelectedClient = new System.Windows.Forms.Label();
            this.groupBoxParticipants = new System.Windows.Forms.GroupBox();
            this.btnWhitelistParticipant = new System.Windows.Forms.Button(); // NEW BUTTON
            this.participantsListBox = new System.Windows.Forms.ListBox();
            this.groupBoxGroups.SuspendLayout();
            this.groupBoxParticipants.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxGroups
            // 
            this.groupBoxGroups.BackColor = System.Drawing.Color.White;
            this.groupBoxGroups.Controls.Add(this.groupsListView);
            this.groupBoxGroups.Controls.Add(this.btnFetchGroups);
            this.groupBoxGroups.Controls.Add(this.btnWhitelistGroup);
            this.groupBoxGroups.Controls.Add(this.btnFetchParticipants);
            this.groupBoxGroups.Location = new System.Drawing.Point(10, 40);
            this.groupBoxGroups.Name = "groupBoxGroups";
            this.groupBoxGroups.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxGroups.Size = new System.Drawing.Size(500, 350); // Increased width for new column
            this.groupBoxGroups.TabIndex = 0;
            this.groupBoxGroups.TabStop = false;
            this.groupBoxGroups.Text = "Groups List";
            // 
            // groupsListView
            // 
            this.groupsListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.groupsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colGroupId,
            this.colGroupName,
            this.colParticipantsCount,
            this.colGroupWhitelisted});
            this.groupsListView.FullRowSelect = true;
            this.groupsListView.GridLines = true;
            this.groupsListView.HideSelection = false;
            this.groupsListView.Location = new System.Drawing.Point(15, 25);
            this.groupsListView.MultiSelect = false;
            this.groupsListView.Name = "groupsListView";
            this.groupsListView.OwnerDraw = true;
            this.groupsListView.Size = new System.Drawing.Size(470, 270); // Adjusted width
            this.groupsListView.TabIndex = 0;
            this.groupsListView.UseCompatibleStateImageBehavior = false;
            this.groupsListView.View = System.Windows.Forms.View.Details;
            this.groupsListView.SelectedIndexChanged += new System.EventHandler(this.groupsListView_SelectedIndexChanged);
            // 
            // colGroupId
            // 
            this.colGroupId.Text = "Group ID";
            this.colGroupId.Width = 140;
            // 
            // colGroupName
            // 
            this.colGroupName.Text = "Group Name";
            this.colGroupName.Width = 140;
            // 
            // colParticipantsCount
            // 
            this.colParticipantsCount.Text = "Members";
            this.colParticipantsCount.Width = 70;
            // 
            // colGroupWhitelisted
            // 
            this.colGroupWhitelisted.Text = "Whitelisted";
            this.colGroupWhitelisted.Width = 80;  // NEW
            // 
            // btnFetchGroups
            // 
            this.btnFetchGroups.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(140)))), ((int)(((byte)(126)))));
            this.btnFetchGroups.FlatAppearance.BorderSize = 0;
            this.btnFetchGroups.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFetchGroups.ForeColor = System.Drawing.Color.White;
            this.btnFetchGroups.Location = new System.Drawing.Point(15, 305);
            this.btnFetchGroups.Name = "btnFetchGroups";
            this.btnFetchGroups.Size = new System.Drawing.Size(120, 30);
            this.btnFetchGroups.TabIndex = 1;
            this.btnFetchGroups.Text = "Fetch Groups";
            this.btnFetchGroups.UseVisualStyleBackColor = false;
            this.btnFetchGroups.Click += new System.EventHandler(this.btnFetchGroups_Click);
            // 
            // btnWhitelistGroup
            // 
            this.btnWhitelistGroup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(211)))), ((int)(((byte)(102)))));
            this.btnWhitelistGroup.Enabled = false;
            this.btnWhitelistGroup.FlatAppearance.BorderSize = 0;
            this.btnWhitelistGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWhitelistGroup.ForeColor = System.Drawing.Color.White;
            this.btnWhitelistGroup.Location = new System.Drawing.Point(145, 305);
            this.btnWhitelistGroup.Name = "btnWhitelistGroup";
            this.btnWhitelistGroup.Size = new System.Drawing.Size(130, 30); // Adjusted size
            this.btnWhitelistGroup.TabIndex = 2;
            this.btnWhitelistGroup.Text = "Toggle Whitelist"; // Changed text
            this.btnWhitelistGroup.UseVisualStyleBackColor = false;
            this.btnWhitelistGroup.Click += new System.EventHandler(this.btnWhitelistGroup_Click);
            // 
            // btnFetchParticipants
            // 
            this.btnFetchParticipants.BackColor = System.Drawing.Color.SteelBlue;
            this.btnFetchParticipants.Enabled = false;
            this.btnFetchParticipants.FlatAppearance.BorderSize = 0;
            this.btnFetchParticipants.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFetchParticipants.ForeColor = System.Drawing.Color.White;
            this.btnFetchParticipants.Location = new System.Drawing.Point(285, 305); // Adjusted position
            this.btnFetchParticipants.Name = "btnFetchParticipants";
            this.btnFetchParticipants.Size = new System.Drawing.Size(140, 30);
            this.btnFetchParticipants.TabIndex = 3;
            this.btnFetchParticipants.Text = "Fetch Participants";
            this.btnFetchParticipants.UseVisualStyleBackColor = false;
            this.btnFetchParticipants.Click += new System.EventHandler(this.btnFetchParticipants_Click);
            // 
            // labelSelectedClient
            // 
            this.labelSelectedClient.AutoSize = true;
            this.labelSelectedClient.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSelectedClient.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(44)))), ((int)(((byte)(51)))));
            this.labelSelectedClient.Location = new System.Drawing.Point(10, 15);
            this.labelSelectedClient.Name = "labelSelectedClient";
            this.labelSelectedClient.Size = new System.Drawing.Size(117, 17);
            this.labelSelectedClient.TabIndex = 4;
            this.labelSelectedClient.Text = "Groups for Client:";
            // 
            // groupBoxParticipants
            // 
            this.groupBoxParticipants.BackColor = System.Drawing.Color.White;
            this.groupBoxParticipants.Controls.Add(this.btnWhitelistParticipant); // NEW
            this.groupBoxParticipants.Controls.Add(this.participantsListBox);
            this.groupBoxParticipants.Location = new System.Drawing.Point(520, 40); // Adjusted position
            this.groupBoxParticipants.Name = "groupBoxParticipants";
            this.groupBoxParticipants.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxParticipants.Size = new System.Drawing.Size(310, 350); // Adjusted width
            this.groupBoxParticipants.TabIndex = 5;
            this.groupBoxParticipants.TabStop = false;
            this.groupBoxParticipants.Text = "Participants List";
            // 
            // btnWhitelistParticipant
            // 
            this.btnWhitelistParticipant.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(211)))), ((int)(((byte)(102)))));
            this.btnWhitelistParticipant.Enabled = false;
            this.btnWhitelistParticipant.FlatAppearance.BorderSize = 0;
            this.btnWhitelistParticipant.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWhitelistParticipant.ForeColor = System.Drawing.Color.White;
            this.btnWhitelistParticipant.Location = new System.Drawing.Point(15, 305);
            this.btnWhitelistParticipant.Name = "btnWhitelistParticipant";
            this.btnWhitelistParticipant.Size = new System.Drawing.Size(280, 30);
            this.btnWhitelistParticipant.TabIndex = 1;
            this.btnWhitelistParticipant.Text = "Toggle Participant Whitelist";
            this.btnWhitelistParticipant.UseVisualStyleBackColor = false;
            this.btnWhitelistParticipant.Click += new System.EventHandler(this.btnWhitelistParticipant_Click); // NEW
            // 
            // participantsListBox
            // 
            this.participantsListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.participantsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed; // For custom drawing
            this.participantsListBox.FormattingEnabled = true;
            this.participantsListBox.IntegralHeight = false;
            this.participantsListBox.ItemHeight = 15;
            this.participantsListBox.Location = new System.Drawing.Point(15, 25);
            this.participantsListBox.Name = "participantsListBox";
            this.participantsListBox.Size = new System.Drawing.Size(280, 270); // Adjusted width
            this.participantsListBox.TabIndex = 0;
            this.participantsListBox.SelectedIndexChanged += new System.EventHandler(this.participantsListBox_SelectedIndexChanged); // NEW
            this.participantsListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.participantsListBox_DrawItem); // NEW for custom drawing
            // 
            // GroupsDisplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(237)))));
            this.ClientSize = new System.Drawing.Size(840, 400); // Adjusted total size
            this.Controls.Add(this.groupBoxParticipants);
            this.Controls.Add(this.labelSelectedClient);
            this.Controls.Add(this.groupBoxGroups);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GroupsDisplayForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manage WhatsApp Groups";
            this.Load += new System.EventHandler(this.GroupsDisplayForm_Load);
            this.groupBoxGroups.ResumeLayout(false);
            this.groupBoxParticipants.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxGroups;
        private System.Windows.Forms.ListView groupsListView;
        private System.Windows.Forms.ColumnHeader colGroupId;
        private System.Windows.Forms.ColumnHeader colGroupName;
        private System.Windows.Forms.ColumnHeader colParticipantsCount;
        private System.Windows.Forms.ColumnHeader colGroupWhitelisted; // NEW
        private System.Windows.Forms.Button btnFetchGroups;
        private System.Windows.Forms.Button btnWhitelistGroup;
        private System.Windows.Forms.Button btnFetchParticipants;
        private System.Windows.Forms.Label labelSelectedClient;
        private System.Windows.Forms.GroupBox groupBoxParticipants;
        private System.Windows.Forms.ListBox participantsListBox;
        private System.Windows.Forms.Button btnWhitelistParticipant; // NEW
    }
}