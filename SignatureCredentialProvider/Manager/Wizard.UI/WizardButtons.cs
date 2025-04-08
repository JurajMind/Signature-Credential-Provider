using System;

namespace Wizard.UI
{
    [Flags]
    public enum WizardButtons
    {
        None = 0x0000,
        Back = 0x0001,
        Next = 0x0002,
        Finish = 0x0004,
    }
}