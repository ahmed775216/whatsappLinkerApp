// QrDisplayForm.Designer.cs
namespace WhatsAppLinkerApp
{
    partial class QrDisplayForm
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

        #region WinForm Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QrDisplayForm));
            this.qrPictureBox = new System.Windows.Forms.PictureBox();
            this.statusLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.qrPictureBox)).BeginInit();
            this.SuspendLayout();
            //
            // qrPictureBox
            //
            this.qrPictureBox.BackColor = System.Drawing.Color.White; // Set background to white for QR contrast
            this.qrPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.qrPictureBox.Location = new System.Drawing.Point(42, 60); // Centered a bit
            this.qrPictureBox.Name = "qrPictureBox";
            this.qrPictureBox.Size = new System.Drawing.Size(300, 300);
            this.qrPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.qrPictureBox.TabIndex = 0;
            this.qrPictureBox.TabStop = false;
            //
            // statusLabel
            //
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusLabel.Location = new System.Drawing.Point(12, 18);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(360, 25);
            this.statusLabel.TabIndex = 1;
            this.statusLabel.Text = "Initializing...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.statusLabel.ForeColor = System.Drawing.Color.Gray; // Default gray status
            //
            // QrDisplayForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(237))))); // Light gray background
            this.ClientSize = new System.Drawing.Size(384, 400);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.qrPictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog; // Non-resizable
            // this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon"))); // You might need to add a specific icon here in project resources.
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "QrDisplayForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent; // Centers on parent form
            this.Text = "Link WhatsApp";
            this.Load += new System.EventHandler(this.QrDisplayForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QrDisplayForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.qrPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox qrPictureBox;
        private System.Windows.Forms.Label statusLabel;
    }
}