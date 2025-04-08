using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using EXOE.CsharpHelper;
using WcfServiceLibraryNamedPipe;

namespace Client2
{
    public class Speaker : INamedPipeBindingCallbackService
    {
        private MainWindow _mainWindow;
        private ChannelFactory<INamedPipeBindingService> _namedPipeBindingFactory;
        private INamedPipeBindingService _namedPipeBindingProxy;
        private Timer _timer;

        private Speaker()
        {
        }

        public void Connect(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            OpenDuplexChannel();
            _timer = new Timer(TimerTick, null, 0, 5000);
        }

        private void OpenDuplexChannel()
        {
            _namedPipeBindingFactory = new DuplexChannelFactory<INamedPipeBindingService>(this,
                "NamedPipeBindingServiceEndpoint");
            _namedPipeBindingProxy = _namedPipeBindingFactory.CreateChannel();
            ((IClientChannel) _namedPipeBindingProxy).Faulted += Speaker_Faulted;
            ((IClientChannel) _namedPipeBindingProxy).Opened += Speaker_Opened;
            ((IClientChannel) _namedPipeBindingProxy).Open();
        }

        private void TimerTick(Object stateInfo)
        {
            ReConnect();
        }

        public void ReConnect()
        {
            if (_namedPipeBindingProxy != null &&
                (((IClientChannel) _namedPipeBindingProxy).State == CommunicationState.Opened ||
                 ((IClientChannel) _namedPipeBindingFactory).State == CommunicationState.Opening))
                return;
            OpenDuplexChannel();
        }

        public void DisConnect()
        {
            _namedPipeBindingProxy.Disconnect(AppDomain.CurrentDomain.FriendlyName);
            ((IClientChannel) _namedPipeBindingProxy).Close(new TimeSpan(0, 0, 5));
        }

        private void Speaker_Opened(object sender, EventArgs e)
        {
            _namedPipeBindingProxy.Connect(AppDomain.CurrentDomain.FriendlyName);
        }

        private void Speaker_Faulted(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(CommunicatUnit messageUnit)
        {
            _namedPipeBindingProxy.Message(messageUnit);
        }

        public void SendMessageToClient(CommunicatUnit messageUnit, string destinator)
        {
            _namedPipeBindingProxy.MessageToSomeone(messageUnit, destinator);
        }

        public void SendMessageToClient(List<CommunicatUnit> messages, string destinator)
        {
            _namedPipeBindingProxy.MessagesToSomeone(messages, destinator);
        }

        public static Speaker Instance()
        {
            return Singleton<Speaker>.Instance;
        }

        #region message from server

        public void Message(CommunicatUnit composite)
        {
            DispatcherHelper.Instance().Invoke(() => { _mainWindow.ListBox_Messages.Items.Add(composite.Message); });
        }

        public void Messages(List<CommunicatUnit> composites)
        {
            DispatcherHelper.Instance().Invoke(() =>
            {
                foreach (CommunicatUnit item in composites)
                {
                    _mainWindow.ListBox_Messages.Items.Add(item.Message);
                }
            });
        }

        public void HostClientList(List<string> clientsAssemblyFriendNames)
        {
            DispatcherHelper.Instance().Invoke(() =>
            {
                if (clientsAssemblyFriendNames != null && clientsAssemblyFriendNames.Count > 0)
                {
                    foreach (string name in clientsAssemblyFriendNames)
                    {
                        _mainWindow.ListBox_Messages.Items.Add("Processes " + name + "has also connected to the server");
                        _mainWindow.CreateOtherClientsButtons(name);
                    }
                }
            });
        }

        #endregion
    }
}