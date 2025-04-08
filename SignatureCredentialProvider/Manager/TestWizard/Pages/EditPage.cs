using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using SignForm;
using WcfServiceLibraryNamedPipe;
using Wizard.UI;

namespace TestWizard
{
    public class EditPage : InternalWizardPage
    {
        private  RegistryAccess Access;
        private  string Id;
        private readonly IContainer components = null;
        private TextBox domain;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox password;
        private TextBox username;

        public EditPage()
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
        

        private void EditPage_SetActive(object sender, CancelEventArgs e)
        {
            Id = TestWizardSheet.token;
            Access = new RegistryAccess(Id);

            SetWizardButtons(WizardButtons.Back | WizardButtons.Next);
            GetWizard().Pages.Add(new CompletePage());

            WizardNext += EditPage_WizardNext;

            Banner.Subtitle = Id;
            try
            {
                username.Text = Access.Username;
            }
            catch
            {
                username.Text = Environment.UserName;
            }

            try
            {
                password.Text = Access.Password;
            }
            catch
            {
            }

            try
            {
                domain.Text = Access.Domain;
            }
            catch
            {
                domain.Text = Environment.UserDomainName;
            }
        }

        private void EditPage_WizardNext(object sender, WizardPageEventArgs e)
        {
            Access.Username = username.Text;
            Access.Password = password.Text;
            Access.Domain = domain.Text;
        {
            Speaker.Instance().Connect();
            Speaker.Instance().SendMessage(new CommunicatUnit
            {
                Message = "Save"
            });
            //Speaker.Instance().DisConnect();
        }
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
            this.SuspendLayout();
            // 
            // Banner
            // 
            this.Banner.Size = new System.Drawing.Size(468, 64);
            this.Banner.Subtitle = "";
            this.Banner.Title = "Edit";
            // 
            // domain
            // 
            this.domain.Location = new System.Drawing.Point(79, 82);
            this.domain.Name = "domain";
            this.domain.Size = new System.Drawing.Size(368, 20);
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
            this.username.Location = new System.Drawing.Point(79, 108);
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(368, 20);
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
            this.password.Location = new System.Drawing.Point(79, 134);
            this.password.Name = "password";
            this.password.Size = new System.Drawing.Size(368, 20);
            this.password.TabIndex = 5;
            this.password.UseSystemPasswordChar = true;
            // 
            // EditPage
            // 
            this.Controls.Add(this.label3);
            this.Controls.Add(this.password);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.username);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.domain);
            this.Name = "EditPage";
            this.Size = new System.Drawing.Size(468, 325);
            this.SetActive += new System.ComponentModel.CancelEventHandler(this.EditPage_SetActive);
            this.Controls.SetChildIndex(this.domain, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.Banner, 0);
            this.Controls.SetChildIndex(this.username, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.password, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}