using System.ComponentModel;
using Wizard.UI;

namespace TestWizard
{
    public class TestWizardSheet : WizardSheet
    {
        private readonly IContainer components = null;

        public static string token = "";

        public TestWizardSheet()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
            
            Pages.Add(new WelcomePage());
            Pages.Add(new PasswordWerification());
            Pages.Add(new MiddlePage());
            Pages.Add(new EditPage());
            Pages.Add(new CompletePage());
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

        #region Designer generated code

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestWizardSheet));
            this.SuspendLayout();
            // 
            // TestWizardSheet
            // 

            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(384, 141);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "TestWizardSheet";
            this.Text = "Credential Manager";
            this.ResumeLayout(false);

        }

        #endregion
    }
}