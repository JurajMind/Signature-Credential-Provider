using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using SignForm;
using WcfServiceLibraryNamedPipe;
using Wizard.UI;

namespace TestWizard
{
    public class MiddlePage : InternalWizardPage
    {
        private readonly IContainer components = null;
        private NamedPipeServer PServer1;
        private PictureBox pictureBox1;

        public MiddlePage()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
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


        private void MiddlePage_SetActive(object sender, CancelEventArgs e)
        {
            SetWizardButtons(WizardButtons.Back);
            SetWizardButtons(WizardButtons.Next);
            

            WizardNext += MiddlePage_WizardNext;
            WizardBack += MiddlePage_WizardBack;

            Speaker.Instance().Connect();


            Speaker.Instance().SendMessage(new CommunicatUnit
            {
                Message = String.Format("Record#{0}",TestWizardSheet.token)
                
            });
            Debug.WriteLine("Record send");
            Process.Start(@"C:\Signature\SignForm\SignForm.exe");
        }

        private void MiddlePage_WizardNext(object sender, WizardPageEventArgs e)
        {
            Speaker.Instance().SendMessage(new CommunicatUnit
            {
                Message = "StopRecord"
            });
            
        }

        private void MiddlePage_WizardFinish(object sender, CancelEventArgs e)
        {
            Speaker.Instance().SendMessage(new CommunicatUnit
            {
                Message = "Discard"
            });
            Speaker.Instance().DisConnect();
        }

        private void MiddlePage_WizardBack(object sender, WizardPageEventArgs e)
        {
            Speaker.Instance().SendMessage(new CommunicatUnit
            {
                Message = "Discard"
            });
            Speaker.Instance().DisConnect();
        }

        #region Designer generated code

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            var resources =
                new System.ComponentModel.ComponentResourceManager(typeof (MiddlePage));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // Banner
            // 
            this.Banner.Size = new System.Drawing.Size(465, 64);
            this.Banner.Subtitle = "Add new signature to register or update it.";
            this.Banner.Title = "Sign in";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image) (resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(59, 70);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(368, 236);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // MiddlePage
            // 
            this.Controls.Add(this.pictureBox1);
            this.Name = "MiddlePage";
            this.Size = new System.Drawing.Size(465, 320);
            this.SetActive += new System.ComponentModel.CancelEventHandler(this.MiddlePage_SetActive);
            this.Controls.SetChildIndex(this.pictureBox1, 0);
            this.Controls.SetChildIndex(this.Banner, 0);
            ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}