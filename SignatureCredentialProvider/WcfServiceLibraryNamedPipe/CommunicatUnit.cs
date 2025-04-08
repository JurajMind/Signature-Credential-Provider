using System.Runtime.Serialization;

namespace WcfServiceLibraryNamedPipe
{
    [DataContract]
    public class CommunicatUnit
    {
        private string _message = "Message empty";

        [DataMember]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }
}