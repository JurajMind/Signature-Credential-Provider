using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfWcfNamedPipeBinding;

namespace CmdAutentificator
{
    class Program
    {
        static Host _host;

        static DateTime _maxDateTime = DateTime.Now;
        static private NamedPipeServer PServer2;
        static DispatcherTimer _dispatcherTimer;
        
        static void Main(string[] args)
        {
            PServer2 = new NamedPipeServer(@"\\.\pipe\zero", 1);
            PServer2.Start();
            _host.OpenHost(new Action(() =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    ListBox_Messages.Items.Add("Host started");
                }));
            }));
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Tick += new EventHandler(_dispatcherTimer_Tick);
            _dispatcherTimer.Start();
        }
    }
}
