using System;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using SignatureVerification;
using TestWizard.Properties;
using Wizard.UI;


namespace TestWizard
{
    public class PasswordWerification : InternalWizardPage
    {
     
        private readonly IContainer components = null;
        private TextBox domain;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox password;
        private Button VerifyPassword;
        private Label passwordLabel;
        private Label oldPasswordLabel;
        private TextBox oldPasswordBox;
        private Button createNew;
        private Button oldPasswordVerify;
        private Button skipsignature;
        private TextBox username;

        public PasswordWerification()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();


     
           
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void PasswordVerification_SetActive(object sender, CancelEventArgs e)
        {
            SetWizardButtons(WizardButtons.Back );

            WizardNext += PasswordVerification_SetActive_WizardNext;

           username.Text = Environment.UserName;

           domain.Text = Environment.UserDomainName;
           
        }

        private void PasswordVerification_SetActive_WizardNext(object sender, WizardPageEventArgs e)
        {
            

        }

        #region Designer generated code

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.domain = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.username = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.password = new System.Windows.Forms.TextBox();
            this.VerifyPassword = new System.Windows.Forms.Button();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.oldPasswordLabel = new System.Windows.Forms.Label();
            this.oldPasswordBox = new System.Windows.Forms.TextBox();
            this.createNew = new System.Windows.Forms.Button();
            this.oldPasswordVerify = new System.Windows.Forms.Button();
            this.skipsignature = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Banner
            // 
            this.Banner.Size = new System.Drawing.Size(468, 64);
            this.Banner.Subtitle = "Please verify yourself";
            this.Banner.Title = "Password Verification";
            this.Banner.Load += new System.EventHandler(this.Banner_Load);
            // 
            // domain
            // 
            this.domain.Location = new System.Drawing.Point(95, 82);
            this.domain.Name = "domain";
            this.domain.Size = new System.Drawing.Size(352, 20);
            this.domain.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Domain";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 111);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Username";
            // 
            // username
            // 
            this.username.Location = new System.Drawing.Point(95, 108);
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(352, 20);
            this.username.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 137);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Password";
            // 
            // password
            // 
            this.password.Location = new System.Drawing.Point(95, 134);
            this.password.Name = "password";
            this.password.Size = new System.Drawing.Size(260, 20);
            this.password.TabIndex = 5;
            this.password.UseSystemPasswordChar = true;
            // 
            // VerifyPassword
            // 
            this.VerifyPassword.Location = new System.Drawing.Point(372, 134);
            this.VerifyPassword.Name = "VerifyPassword";
            this.VerifyPassword.Size = new System.Drawing.Size(75, 23);
            this.VerifyPassword.TabIndex = 7;
            this.VerifyPassword.Text = "Verify";
            this.VerifyPassword.UseVisualStyleBackColor = true;
            this.VerifyPassword.Click += new System.EventHandler(this.button1_Click);
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(76, 220);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(7, 13);
            this.passwordLabel.TabIndex = 8;
            this.passwordLabel.Text = "\r\n";
            // 
            // oldPasswordLabel
            // 
            this.oldPasswordLabel.AutoSize = true;
            this.oldPasswordLabel.Location = new System.Drawing.Point(18, 168);
            this.oldPasswordLabel.Name = "oldPasswordLabel";
            this.oldPasswordLabel.Size = new System.Drawing.Size(71, 13);
            this.oldPasswordLabel.TabIndex = 9;
            this.oldPasswordLabel.Text = "Old password";
            this.oldPasswordLabel.Visible = false;
            // 
            // oldPasswordBox
            // 
            this.oldPasswordBox.Location = new System.Drawing.Point(95, 168);
            this.oldPasswordBox.Name = "oldPasswordBox";
            this.oldPasswordBox.Size = new System.Drawing.Size(260, 20);
            this.oldPasswordBox.TabIndex = 10;
            this.oldPasswordBox.UseSystemPasswordChar = true;
            this.oldPasswordBox.Visible = false;
            // 
            // createNew
            // 
            this.createNew.Location = new System.Drawing.Point(263, 270);
            this.createNew.Name = "createNew";
            this.createNew.Size = new System.Drawing.Size(127, 23);
            this.createNew.TabIndex = 11;
            this.createNew.Text = "Create new credentials";
            this.createNew.UseVisualStyleBackColor = true;
            this.createNew.Visible = false;
            this.createNew.Click += new System.EventHandler(this.createNew_Click);
            // 
            // oldPasswordVerify
            // 
            this.oldPasswordVerify.Location = new System.Drawing.Point(372, 166);
            this.oldPasswordVerify.Name = "oldPasswordVerify";
            this.oldPasswordVerify.Size = new System.Drawing.Size(75, 23);
            this.oldPasswordVerify.TabIndex = 12;
            this.oldPasswordVerify.Text = "Verify";
            this.oldPasswordVerify.UseVisualStyleBackColor = true;
            this.oldPasswordVerify.Visible = false;
            this.oldPasswordVerify.Click += new System.EventHandler(this.oldPasswordVerify_Click);
            // 
            // skipsignature
            // 
            this.skipsignature.Location = new System.Drawing.Point(95, 270);
            this.skipsignature.Name = "skipsignature";
            this.skipsignature.Size = new System.Drawing.Size(162, 22);
            this.skipsignature.TabIndex = 13;
            this.skipsignature.Text = "Skip signature recording";
            this.skipsignature.UseVisualStyleBackColor = true;
            this.skipsignature.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // PasswordWerification
            // 
            this.Controls.Add(this.skipsignature);
            this.Controls.Add(this.oldPasswordVerify);
            this.Controls.Add(this.createNew);
            this.Controls.Add(this.oldPasswordBox);
            this.Controls.Add(this.oldPasswordLabel);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.VerifyPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.password);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.username);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.domain);
            this.Name = "PasswordWerification";
            this.Size = new System.Drawing.Size(468, 325);
            this.SetActive += new System.ComponentModel.CancelEventHandler(this.PasswordVerification_SetActive);
            this.Controls.SetChildIndex(this.domain, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.Banner, 0);
            this.Controls.SetChildIndex(this.username, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.password, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.VerifyPassword, 0);
            this.Controls.SetChildIndex(this.passwordLabel, 0);
            this.Controls.SetChildIndex(this.oldPasswordLabel, 0);
            this.Controls.SetChildIndex(this.oldPasswordBox, 0);
            this.Controls.SetChildIndex(this.createNew, 0);
            this.Controls.SetChildIndex(this.oldPasswordVerify, 0);
            this.Controls.SetChildIndex(this.skipsignature, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private void Banner_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool valid = false;
            var storedCredentials = false;
            using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
            {
                try
                {
                   

                    valid = context.ValidateCredentials(username.Text, password.Text);
                    var ra = new RegistryAccess(SupportFunctions.GenerateToken(password.Text));
                    try
                    {
                            storedCredentials = ra.Exist();
                    }
                    finally
                    {
                        TestWizardSheet.token = SupportFunctions.GenerateToken(password.Text);
                    }
                }
                catch (Exception)
                {


                }

            }

            if (valid && storedCredentials)
            {
                SetWizardButtons(WizardButtons.Next);
                this.passwordLabel.Text = Resources.PasswordWerification_button1_Click_Autorization_OK;
                passwordLabel.ForeColor = System.Drawing.Color.Green;
             
            }
            else
            {
                if (valid)
                {
                    createNew.Visible = true;
                    oldPasswordBox.Visible = true;
                    oldPasswordLabel.Visible = true;
                    oldPasswordVerify.Visible = true;
                    VerifyPassword.Enabled = false;

                    this.passwordLabel.Text = Resources.PasswordWerification_button1_Click_Autorization_OK_CredentialsNotFound;
                    passwordLabel.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    this.passwordLabel.Text = Resources.PasswordVerification_button1_Click_Authorization_failed;
                    passwordLabel.ForeColor = System.Drawing.Color.Red;
                }
                
            }
        }

    
       
        private void createNew_Click(object sender, EventArgs e)
        {
            TestWizardSheet.token = SupportFunctions.GenerateToken(password.Text);
            SetWizardButtons(WizardButtons.Next);
        }

        private void oldPasswordVerify_Click(object sender, EventArgs e)
        {
            var storedCredentials = false;
            try
            {
                
                var ra = new RegistryAccess(SupportFunctions.GenerateToken(oldPasswordBox.Text));
                try
                {
                
                    storedCredentials = ra.Exist();
                }
                finally
                {
                    
                }
            }
            catch (Exception)
            {


            }

            if (storedCredentials)
            {
                TestWizardSheet.token = SupportFunctions.GenerateToken(oldPasswordBox.Text);

                SetWizardButtons(WizardButtons.Next);
                this.passwordLabel.Text = Resources.PasswordWerification_button1_Click_Autorization_OK_OldPassword;
                passwordLabel.ForeColor = System.Drawing.Color.Green;
                oldPasswordVerify.Enabled = false;

            }
            else
            {
                this.passwordLabel.Text = Resources.PasswordWerification_button1_Click_Autorization_OK_OldPassword_NOOK;
                passwordLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }
    }
}