using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace TestWizard
{
    internal class RegistryAccess
    {
        private static readonly bool isBoxed = (IntPtr.Size == 4 &&
                                                !string.IsNullOrEmpty(
                                                    Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")));

        public static RegistryKey Primary;
        private static RegistryKey Keys;

        private readonly string signatureToken;

        public RegistryAccess(string token)
        {
            Primary =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                    (isBoxed) ? RegistryView.Registry64 : RegistryView.Default)
                    .OpenSubKey("SOFTWARE\\Juraj Hamornik\\Signature Login", true);
            Keys = Primary.OpenSubKey("Keys", true);

            signatureToken = token;
        }


        public string Username
        {
            get { return GetDecryptedValue(Encoding.ASCII.GetString((byte[]) TokenKey.GetValue("Username"))); }
            set
            {
                TokenKey.SetValue("Username", Encoding.ASCII.GetBytes(GetEncryptedValue(value)),
                    RegistryValueKind.Binary);
            }
        }

        public string Password
        {
            get { return GetDecryptedValue(Encoding.ASCII.GetString((byte[]) TokenKey.GetValue("Password"))); }
            set
            {
                TokenKey.SetValue("Password", Encoding.ASCII.GetBytes(GetEncryptedValue(value)),
                    RegistryValueKind.Binary);
            }
        }

        public string Domain
        {
            get { return GetDecryptedValue(Encoding.ASCII.GetString((byte[]) TokenKey.GetValue("Domain"))); }
            set
            {
                TokenKey.SetValue("Domain", Encoding.ASCII.GetBytes(GetEncryptedValue(value)),
                    RegistryValueKind.Binary);
            }
        }

        public bool Exist()
        {
            var userSalt = Primary.GetValue("Salt");
            var name = Hash(signatureToken + KeySalt);
            return Keys.OpenSubKey(name) != null;
        }

        private string TokenSalt
        {
            get { return (string) TokenKey.GetValue("Salt"); }
        }

        private RegistryKey TokenKey
        {
            get
            {
                if (Keys.OpenSubKey(Hash(signatureToken + KeySalt)) == null)
                {
                    Keys.CreateSubKey(Hash(signatureToken + KeySalt));
                    Keys.OpenSubKey(Hash(signatureToken + KeySalt), true)
                        .SetValue("Salt", Hash(((new Random()).Next(Int32.MaxValue) + DateTime.Now.Ticks).ToString()));
                }
                return Keys.OpenSubKey(Hash(signatureToken + KeySalt), true);
            }
        }

        private static string KeySalt
        {
            get { return (string) Keys.GetValue("Salt"); }
        }

        private string GetEncryptedValue(string toEncrypt)
        {
            char[] e = toEncrypt.ToCharArray();
            string key = Hash(signatureToken + TokenSalt);
            for (int i = 0; i < toEncrypt.Length; i++)
            {
                e[i] = (char) (e[i] ^ key[i%key.Length]);
                e[i] = (char) (e[i] + 1);
            }

            return new string(e);
        }

        private string GetDecryptedValue(string toDecrypt)
        {
            char[] e = toDecrypt.ToCharArray();
            string key = Hash(signatureToken + TokenSalt);
            for (int i = 0; i < toDecrypt.Length; i++)
            {
                e[i] = (char) (e[i] - 1);
                e[i] = (char) (e[i] ^ key[i%key.Length]);
            }

            return new string(e);
        }

        private static string Hash(string toHash)
        {
            var x =
                new SHA1CryptoServiceProvider();
            byte[] data = Encoding.ASCII.GetBytes(toHash);
            data = x.ComputeHash(data);
            string o = BitConverter.ToString(data).Replace("-", "").ToUpper();
            return o;
        }
    }
}