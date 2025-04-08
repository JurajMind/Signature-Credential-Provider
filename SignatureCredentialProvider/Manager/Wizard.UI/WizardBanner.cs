using System.ComponentModel;
using System.Windows.Forms;
using Wizard.Controls;

namespace Wizard.UI
{
    /// <summary>
    ///     Summary description for WizardBanner.
    /// </summary>
    public class WizardBanner : UserControl
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private readonly Container components = null;

        private EtchedLine etchedLine1;
        private Label subtitleLabel;
        private Label titleLabel;

        public WizardBanner()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // Avoid getting the focus.
            SetStyle(ControlStyles.Selectable, false);
        }

        [Category("Appearance")]
        public string Title
        {
            get { return titleLabel.Text; }
            set { titleLabel.Text = value; }
        }

        [Category("Appearance")]
        public string Subtitle
        {
            get { return subtitleLabel.Text; }
            set { subtitleLabel.Text = value; }
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

        #region Component Designer generated code

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.titleLabel = new System.Windows.Forms.Label();
            this.subtitleLabel = new System.Windows.Forms.Label();
            this.etchedLine1 = new Wizard.Controls.EtchedLine();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point, ((System.Byte) (0)));
            this.titleLabel.Location = new System.Drawing.Point(16, 8);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(432, 16);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Title";
            // 
            // subtitleLabel
            // 
            this.subtitleLabel.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                       | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
            this.subtitleLabel.Location = new System.Drawing.Point(40, 24);
            this.subtitleLabel.Name = "subtitleLabel";
            this.subtitleLabel.Size = new System.Drawing.Size(408, 32);
            this.subtitleLabel.TabIndex = 1;
            this.subtitleLabel.Text = "Subtitle";
            // 
            // etchedLine1
            // 
            this.etchedLine1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.etchedLine1.Edge = Wizard.Controls.EtchEdge.Bottom;
            this.etchedLine1.Location = new System.Drawing.Point(0, 56);
            this.etchedLine1.Name = "etchedLine1";
            this.etchedLine1.Size = new System.Drawing.Size(456, 8);
            this.etchedLine1.TabIndex = 0;
            // 
            // WizardBanner
            // 
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.etchedLine1);
            this.Controls.Add(this.subtitleLabel);
            this.Controls.Add(this.titleLabel);
            this.Name = "WizardBanner";
            this.Size = new System.Drawing.Size(456, 64);
            this.ResumeLayout(false);
        }

        #endregion
    }
}