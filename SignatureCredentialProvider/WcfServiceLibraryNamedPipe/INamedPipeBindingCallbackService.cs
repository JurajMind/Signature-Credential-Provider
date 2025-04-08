using System.Collections.Generic;
using System.ServiceModel;

namespace WcfServiceLibraryNamedPipe
{
    [ServiceContract]
    public interface INamedPipeBindingCallbackService
    {
        /// IsOneWay = true  
        /// Because : The service may want to invoke the callback reference that's passed in during the execution of a 
        /// contract operation. How ever such invocations are disallowed by defaut. By defaut, the service class is 
        /// configured for single-threaded access: the service instance context is associated with a lock, and only 
        /// one thread at a time can own the lock and access the service instance inside that context. Calling out to 
        /// the client during an operation call requires blocking the service thread and invoking the callback. The problem
        /// is that processing the reply message from the client on the same channel once the callback returns requires
        /// reentering the same context and negotiating ownership of the same lock, which will result in a deadlock. Note that
        /// the service may still invoke callbacks to other clients or call other services; it is the callback to its calling client 
        /// that will cause the deadlock.
        /// Set IsOneWay = true is method to avoid deadlock. Instead of two other method, set IsOneWay = true can maintain the service
        /// as single-threaded without release the lock.  Set IsOneWay = true enables the service to call back even when the
        /// concurrency mode is set to single-threaded, because there will not be any reply message to contend for the lock.
        [OperationContract(IsOneWay = true)]
        // If set IsOneWay = true, we cannot return a result. so  void Message();
        void Message(CommunicatUnit composite);

        [OperationContract(IsOneWay = true)]
        void Messages(List<CommunicatUnit> composite);

        [OperationContract(IsOneWay = true)]
        void HostClientList(List<string> clientsAssemblyFriendNames);
    }
}