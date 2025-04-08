using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using WcfServiceLibraryNamedPipe;

namespace WpfWcfNamedPipeBinding
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class NamedPipeBindingService : INamedPipeBindingService
    {
        static Dictionary<INamedPipeBindingCallbackService, string> _clientCallBackService = new Dictionary<INamedPipeBindingCallbackService, string>();

        public void Message(CommunicatUnit composite)
        {
            var id = OperationContext.Current.GetCallbackChannel<INamedPipeBindingCallbackService>();
            DataPool.Instance().AddData(composite.Message, _clientCallBackService[id]);
        }

        public void MessageToSomeone(CommunicatUnit composite, string someoneName)
        {
            if (_clientCallBackService.Values.Contains(someoneName))
            {
                var sender = OperationContext.Current.GetCallbackChannel<INamedPipeBindingCallbackService>();
                string senderName = _clientCallBackService[sender];
                var serve = _clientCallBackService.Where(p => p.Value == someoneName).First();
                CommunicatUnit cu = new CommunicatUnit();
                cu.Message = senderName + " : " + composite.Message;
                serve.Key.Message(cu);
                DataPool.Instance().AddData(composite.Message, senderName);
            }
        }

        public void MessagesToSomeone(List<CommunicatUnit> composites, string someoneName)
        {
            if (_clientCallBackService.Values.Contains(someoneName))
            {
                var sender = OperationContext.Current.GetCallbackChannel<INamedPipeBindingCallbackService>();
                string senderName = _clientCallBackService[sender];
                var serve = _clientCallBackService.Where(p => p.Value == someoneName).First();
                serve.Key.Messages(composites);

                CommunicatUnit cu = new CommunicatUnit();
                cu.Message = senderName + " has sent a volume to " + someoneName;
                DataPool.Instance().AddData(cu.Message, senderName);
            }
        }

        /// <summary>
        /// This functions is used for sending to all clients a same message 
        /// </summary>
        public static void CallClients(CommunicatUnit message)
        {
            Action<INamedPipeBindingCallbackService> action = callback => callback.Message(message);
            if (_clientCallBackService.Count > 0)
                _clientCallBackService.Keys.ToList().ForEach(action);
        }

        public static void DiffuseClientList()
        {
            if (_clientCallBackService.Count > 0)
            {
                Action<INamedPipeBindingCallbackService> action = 
                    callback => callback.HostClientList(_clientCallBackService.Values.Distinct().ToList());
                _clientCallBackService.Keys.ToList().ForEach(action);
            }
        }

        #region Callback Connection Management
        public void Connect(string name)
        {
            INamedPipeBindingCallbackService callback = OperationContext.Current.GetCallbackChannel<INamedPipeBindingCallbackService>();
            if (_clientCallBackService.Values.Contains(name))
            {
                _clientCallBackService.Remove(_clientCallBackService.Single(p => p.Value == name).Key);
            }
            _clientCallBackService.Add(callback, name);
            //DataPool.Instance().AddData(name + " entered", _clientCallBackService[callback]);
        }

        public void Disconnect(string name)
        {
            INamedPipeBindingCallbackService callback = OperationContext.Current.GetCallbackChannel<INamedPipeBindingCallbackService>();
            if (_clientCallBackService.Keys.Contains(callback) == false)
                throw new InvalidOperationException("Cannot find callback");
            else
                _clientCallBackService.Remove(callback);
        }
        #endregion
    }
}
