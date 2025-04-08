using System;
using System.Collections.Generic;

namespace WcfServiceLibraryNamedPipe
{
    public class NamedPipeBindingCallbackService : INamedPipeBindingCallbackService
    {
        public void Message(CommunicatUnit composite)
        {
        }


        public void HostClientList(List<string> clientsAssemblyFriendNames)
        {
            throw new NotImplementedException();
        }


        public void Messages(List<CommunicatUnit> composite)
        {
            throw new NotImplementedException();
        }
    }
}