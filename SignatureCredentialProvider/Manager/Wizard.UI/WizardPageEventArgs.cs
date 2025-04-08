using System.ComponentModel;

namespace Wizard.UI
{
    public class WizardPageEventArgs : CancelEventArgs
    {
        public WizardPageEventArgs()
        {
            NewPage = null;
        }

        public string NewPage { get; set; }
    }
}