using System;

namespace WpfWcfNamedPipeBinding
{
    public class DataUnit
    {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public string Sender { get; set; }
    }
}