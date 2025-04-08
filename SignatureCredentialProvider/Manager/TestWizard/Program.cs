using System;
using System.Windows.Forms;

namespace TestWizard
{
    public class Program
    {
        [STAThread]
        private static void Main()
        {
            var wizard = new TestWizardSheet();
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            Application.EnableVisualStyles();
            Application.DoEvents();
            Application.Run(wizard);
        }


        private static void OnProcessExit(object sender, EventArgs e)
        {
            //Speaker.Instance().Connect();
            //Speaker.Instance().SendMessage(new CommunicatUnit()
            //{
            //    Message = "Discard"
            //});
            //Speaker.Instance().DisConnect();
        }
    }
}