using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using EXOE.CsharpHelper;
using SignatureVerification;
using WcfServiceLibraryNamedPipe;

namespace SignForm
{
    public class Speaker : INamedPipeBindingCallbackService
    {
        private SignatureInkCanvas _inkCanvas;
        private MainWindow _mainWindow;
        private DuplexChannelFactory<INamedPipeBindingService> _namedPipeBindingFactory;
        private INamedPipeBindingService _namedPipeBindingProxy;
        private Timer _timer;

        private Speaker()
        {
        }

        public SignatureInkCanvas InkCanvas
        {
            set { _inkCanvas = value; }
        }

        public void Connect(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            OpenDuplexChannel();
            _timer = new Timer(TimerTick, null, 0, 5000);
        }

        private void OpenDuplexChannel()
        {
            var context = new InstanceContext(this);
            _namedPipeBindingFactory = new DuplexChannelFactory<INamedPipeBindingService>(context,
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
            DispatcherHelper.Instance().Invoke(() =>
            {
                bool SignatureVerificationStatus;
                bool succes = bool.TryParse(composite.Message, out SignatureVerificationStatus);

                switch (composite.Message)
                {
                    case "SignatureVerified":
                        GoodLoginWindowBehavior();
                        break;

                    case "SignatureDenied":
                        BadLoginWindowBehavior();
                        break;

                    case "SignatureRecorded":
                        NewSignatureWindowBehavior();
                        break;

                    case "SignatureVerifiedSilent":
                        GoodLoginWindowBehaviorNoClose();
                        break;



                }
                
            });
        }

        public void Messages(List<CommunicatUnit> composites)
        {
            DispatcherHelper.Instance().Invoke(() =>
            {
                foreach (CommunicatUnit item in composites)
                {
                    //  _mainWindow.ListBox_Messages.Items.Add(item.Message);
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
                        //_mainWindow.ListBox_Messages.Items.Add("Processes " + name + "has also connected to the server");
                        //_mainWindow.CreateOtherClientsButtons(name);
                    }
                }
            });
        }

        private void GoodLoginWindowBehavior()
        {
            var rootLayerBrush = (SolidColorBrush) _mainWindow.InkCanvas.Background.CloneCurrentValue();
            _mainWindow.InkCanvas.Background = rootLayerBrush;
            var da = new ColorAnimation();
            da.Completed += close_Window;
            da.To = Colors.GreenYellow;
            da.AutoReverse = true;
            da.RepeatBehavior = new RepeatBehavior(1);
            da.Duration = new Duration(TimeSpan.FromMilliseconds(500));


            rootLayerBrush.BeginAnimation(SolidColorBrush.ColorProperty, da);
        }

        private void close_Window(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BadLoginWindowBehavior()
        {
            var da = new DoubleAnimation();
            da.From = _mainWindow.Left;
            da.To = _mainWindow.Left + 10;
            da.AutoReverse = true;
            da.RepeatBehavior = new RepeatBehavior(3);
            da.Duration = new Duration(TimeSpan.FromMilliseconds(50));

            _mainWindow.BeginAnimation(Window.LeftProperty, da);
        }

        private void NewSignatureWindowBehavior()
        {
            var rootLayerBrush = (SolidColorBrush)_mainWindow.InkCanvas.Background.CloneCurrentValue();
            _mainWindow.InkCanvas.Background = rootLayerBrush;
            var da = new ColorAnimation
            {
                To = Colors.DodgerBlue,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(1),
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };


            rootLayerBrush.BeginAnimation(SolidColorBrush.ColorProperty, da);
        }

        private void GoodLoginWindowBehaviorNoClose()
        {
            var rootLayerBrush = (SolidColorBrush)_mainWindow.InkCanvas.Background.CloneCurrentValue();
            _mainWindow.InkCanvas.Background = rootLayerBrush;
            var da = new ColorAnimation
            {
                To = Colors.DarkGreen,
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(1),
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };


            rootLayerBrush.BeginAnimation(SolidColorBrush.ColorProperty, da);
        }



        #endregion
    }
}