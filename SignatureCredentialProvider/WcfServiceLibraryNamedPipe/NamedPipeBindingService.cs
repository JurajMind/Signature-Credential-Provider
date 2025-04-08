using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace WcfServiceLibraryNamedPipe
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class NamedPipeBindingService : INamedPipeBindingService
    {
        public void Message(CommunicatUnit composite)
        {
        }

        public void MessageToSomeone(CommunicatUnit composite, string someoneName)
        {
            throw new NotImplementedException();
        }


        public void MessagesToSomeone(List<CommunicatUnit> composites, string someoneName)
        {
            throw new NotImplementedException();
        }

        #region Callback Connection Management

        public void Connect(string name)
        {
        }

        public void Disconnect(string name)
        {
        }

        #endregion
    }
}