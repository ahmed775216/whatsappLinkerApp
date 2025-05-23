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
            this.linkWhatsappButton = new System.Windows.Forms.Button();
            this.labelApiUsername = new System.Windows.Forms.Label();
            this.textBoxApiUsername = new System.Windows.Forms.TextBox();
            this.labelApiPassword = new System.Windows.Forms.Label();
            this.textBoxApiPassword = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            // linkWhatsappButton
            //
            this.linkWhatsappButton.Location = new System.Drawing.Point(100, 200); // Adjust position as needed
            this.linkWhatsappButton.Name = "linkWhatsappButton";
            this.linkWhatsappButton.Size = new System.Drawing.Size(150, 40);
            this.linkWhatsappButton.TabIndex = 0;
            this.linkWhatsappButton.Text = "Link WhatsApp";
            this.linkWhatsappButton.UseVisualStyleBackColor = true;
            this.linkWhatsappButton.Click += new System.EventHandler(this.linkWhatsappButton_Click);
            //
            // labelApiUsername
            //
            this.labelApiUsername.AutoSize = true;
            this.labelApiUsername.Location = new System.Drawing.Point(50, 50);
            this.labelApiUsername.Name = "labelApiUsername";
            this.labelApiUsername.Size = new System.Drawing.Size(89, 15);
            this.labelApiUsername.TabIndex = 1;
            this.labelApiUsername.Text = "API Username:";
            //
            // textBoxApiUsername
            //
            this.textBoxApiUsername.Location = new System.Drawing.Point(150, 47);
            this.textBoxApiUsername.Name = "textBoxApiUsername";
            this.textBoxApiUsername.Size = new System.Drawing.Size(200, 23);
            this.textBoxApiUsername.TabIndex = 2;
            this.textBoxApiUsername.Text = "781028068"; // Default value for testing
            //
            // labelApiPassword
            //
            this.labelApiPassword.AutoSize = true;
            this.labelApiPassword.Location = new System.Drawing.Point(50, 90);
            this.labelApiPassword.Name = "labelApiPassword";
            this.labelApiPassword.Size = new System.Drawing.Size(87, 15);
            this.labelApiPassword.TabIndex = 3;
            this.labelApiPassword.Text = "API Password:";
            //
            // textBoxApiPassword
            //
            this.textBoxApiPassword.Location = new System.Drawing.Point(150, 87);
            this.textBoxApiPassword.Name = "textBoxApiPassword";
            this.textBoxApiPassword.PasswordChar = '*'; // Hide password input
            this.textBoxApiPassword.Size = new System.Drawing.Size(200, 23);
            this.textBoxApiPassword.TabIndex = 4;
            this.textBoxApiPassword.Text = "781028068"; // Default value for testing
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 300); // Adjust form size as needed
            this.Controls.Add(this.textBoxApiPassword);
            this.Controls.Add(this.labelApiPassword);
            this.Controls.Add(this.textBoxApiUsername);
            this.Controls.Add(this.labelApiUsername);
            this.Controls.Add(this.linkWhatsappButton);
            this.Name = "Form1";
            this.Text = "Main Application";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button linkWhatsappButton;
        private System.Windows.Forms.Label labelApiUsername;
        private System.Windows.Forms.TextBox textBoxApiUsername;
        private System.Windows.Forms.Label labelApiPassword;
        private System.Windows.Forms.TextBox textBoxApiPassword;
    }
}
#endregion