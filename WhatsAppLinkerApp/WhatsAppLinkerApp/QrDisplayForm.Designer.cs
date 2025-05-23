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

        #region WinFormDesigner generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.qrPictureBox = new System.Windows.Forms.PictureBox();
            this.statusLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.qrPictureBox)).BeginInit();
            this.SuspendLayout();
            //
            // qrPictureBox
            //
            this.qrPictureBox.Location = new System.Drawing.Point(50, 50); // Adjust position as needed
            this.qrPictureBox.Name = "qrPictureBox";
            this.qrPictureBox.Size = new System.Drawing.Size(300, 300); // Adjust size as needed
            this.qrPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom; // Important for scaling QR
            this.qrPictureBox.TabIndex = 0;
            this.qrPictureBox.TabStop = false;
            //
            // statusLabel
            //
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.Location = new System.Drawing.Point(12, 9); // Adjust position as needed
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(360, 23); // Adjust size as needed
            this.statusLabel.TabIndex = 1;
            this.statusLabel.Text = "Initializing...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // QrDisplayForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 400); // Adjust form size as needed
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.qrPictureBox);
            this.Name = "QrDisplayForm";
            this.Text = "Link WhatsApp";
            ((System.ComponentModel.ISupportInitialize)(this.qrPictureBox)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox qrPictureBox;
        private System.Windows.Forms.Label statusLabel;
    }
}