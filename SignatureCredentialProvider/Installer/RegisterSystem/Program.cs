using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace RegisterSystem
{
    internal class Program
    {
        private static readonly bool isBoxed = (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem);

        public static readonly RegistryKey Software =
            RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                (isBoxed) ? RegistryView.Registry64 : RegistryView.Default).OpenSubKey("SOFTWARE", true);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64EnableWow64FsRedirection(ref IntPtr ptr);

        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Generating Registry Structure...");
                RegistryKey Base = Software.CreateSubKey("Juraj Hamornik");
                Base = Base.CreateSubKey("Signature Login");

                Console.WriteLine("Adding default configuration...");

                Console.WriteLine("Reversing Entropy...");
                string randomSalt = Hash(((new Random()).Next(int.MaxValue) + DateTime.Now.Ticks).ToString());

                Console.WriteLine("Enumerating the null set...");
                Base.CreateSubKey("Token");
                Base = Base.CreateSubKey("Keys");
                Base.SetValue("Salt", randomSalt);

                Console.WriteLine("Doing magical Windows-y things...");
                string provider = (Environment.Is64BitOperatingSystem)? "64" : "32";
                provider = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\CredentialProviders\\" + provider + "\\SignatureCredentialProvider.dll";
                string to = Environment.GetEnvironmentVariable("SystemRoot") + "\\System32\\SignatureCredentialProvider.dll";

                var p = new IntPtr();

                if (isBoxed)
                {
                    Wow64DisableWow64FsRedirection(ref p);
                }

                if (File.Exists(to))
                {
                     File.Delete(to);
                }
                File.Copy(provider, to);

                if (isBoxed)
                {
                    Wow64EnableWow64FsRedirection(ref p);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                throw ex;
            }
        }

        private static string Hash(string toHash)
        {
            var x = new SHA1CryptoServiceProvider();
            byte[] data = Encoding.ASCII.GetBytes(toHash);
            data = x.ComputeHash(data);
            string o = BitConverter.ToString(data).Replace("-", "").ToUpper();
            return o;
        }
    }
}