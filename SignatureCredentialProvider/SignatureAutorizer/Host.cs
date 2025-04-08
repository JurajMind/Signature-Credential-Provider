using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EXOE.CsharpHelper;
using System.ServiceModel;
using System.Windows;

namespace WpfWcfNamedPipeBinding
{
    public class Host
    {
        ServiceHost _serviceHost;

        private Host()
        {
            _serviceHost = new ServiceHost(typeof(NamedPipeBindingService));
            _serviceHost.Faulted += new EventHandler(_serviceHost_Faulted);
        }

        void _serviceHost_Faulted(object sender, EventArgs e)
        {
             throw new NotImplementedException();
        }

        public void OpenHost(Action updateUI)
        {
            _serviceHost.Open();
            if (_serviceHost.State == CommunicationState.Opened)
                updateUI();
        }

        public string HostState()
        {
            return _serviceHost.State.ToString();
        }

        public static Host Instance()
        {
            return Singleton<Host>.Instance;
        }
    }
}
