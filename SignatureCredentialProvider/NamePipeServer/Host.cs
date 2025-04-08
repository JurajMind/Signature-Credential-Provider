using System;
using System.ServiceModel;
using EXOE.CsharpHelper;

namespace WpfWcfNamedPipeBinding
{
    public class Host
    {
        private readonly ServiceHost _serviceHost;

        private Host()
        {
            _serviceHost = new ServiceHost(typeof(NamedPipeBindingService));
            _serviceHost.Faulted += _serviceHost_Faulted;
        }

        private void _serviceHost_Faulted(object sender, EventArgs e)
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