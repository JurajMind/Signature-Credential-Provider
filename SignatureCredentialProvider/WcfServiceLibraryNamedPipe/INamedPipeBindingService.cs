using System.Collections.Generic;
using System.ServiceModel;

namespace WcfServiceLibraryNamedPipe
{
    /// Callback operations are part of the service contract, and it's up to the service contract to define its own callback contract.
    /// A service conract cann have at most one callback contract. Once defined, the clients are required to support the callback and
    /// provide the callback endpoint to the service in every call. To define a callback contract, the ServiceContract attribute offers 
    /// the CallbackContract property of the type Type.
    [ServiceContract(SessionMode = SessionMode.Required,
        CallbackContract = typeof (INamedPipeBindingCallbackService))]
    public interface INamedPipeBindingService
    {
        [OperationContract(IsOneWay = true)]
        void Message(CommunicatUnit composite);

        [OperationContract(IsOneWay = true)]
        void MessageToSomeone(CommunicatUnit composite, string someoneName);

        [OperationContract(IsOneWay = true)]
        void MessagesToSomeone(List<CommunicatUnit> composites, string someoneName);

        #region Callback Connection Management

        /// Because of mechanism of callback supplied by WCF, we have to come up with ourself some application-level protocol or a 
        /// consistent parttern for managing the life cycle of the connection.
        /// The service can only call back to the client if the client-side channel is still open, which is typically achieved by
        /// not closing the proxy. Keeping the proxy open will also prevent the callback object from bien garbage-collected.
        /// You may always want to add the Connect() and Disconnect() pair on a sessionful service simply as feature, because it
        /// enables the client to decide when to start or stop receiving callbacks during the session
        [OperationContract(IsOneWay = false)]
        void Connect(string name);

        /// If the services  maintains a reference on a callback endpoint and the client-side proxy is closed or the client application 
        /// itself is gone, when the service invokes the callback it will get an ObjectDisposedException from the service channel. 
        /// It is therefore preferable for the client to inform the service when it no longer wishes to receive callbacks or when the
        /// client application is shutting down. In NamedPipeBindingService, InstanceContextMode = PerSession, read comments written on
        /// NamedPipeBindingService to get more info.
        [OperationContract(IsOneWay = true)]
        void Disconnect(string name);

        #endregion
    }
}