using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;
using SignatureVerification;

namespace TokenizerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var token = "";

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(SupportFunctions.GenerateToken("shadow17108"));
            }

            Console.ReadKey();

        }
    }
}
