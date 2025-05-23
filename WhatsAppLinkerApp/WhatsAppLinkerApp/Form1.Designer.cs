// Form1.Designer.cs (Find your InitializeComponent() method and add this)
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
            this.linkWhatsappButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // linkWhatsappButton
            //
            this.linkWhatsappButton.Location = new System.Drawing.Point(100, 100); // Adjust position
            this.linkWhatsappButton.Name = "linkWhatsappButton";
            this.linkWhatsappButton.Size = new System.Drawing.Size(150, 40);
            this.linkWhatsappButton.TabIndex = 0;
            this.linkWhatsappButton.Text = "Link WhatsApp";
            this.linkWhatsappButton.UseVisualStyleBackColor = true;
            this.linkWhatsappButton.Click += new System.EventHandler(this.linkWhatsappButton_Click);
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450); // Adjust form size as needed
            this.Controls.Add(this.linkWhatsappButton); // Make sure button is added to controls
            this.Name = "Form1";
            this.Text = "Main Application";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button linkWhatsappButton;
    }
}
    #endregion

