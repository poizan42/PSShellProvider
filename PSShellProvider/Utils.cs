using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSShellProvider
{
    class Utils
    {
        private static string hexChars = "0123456789ABCDEF";

        public static string ByteArrayToHex(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            for (int i = 0; i < ba.Length; i++)
            {
                byte b = ba[i];
                hex.Append(hexChars[b >> 4]);
                hex.Append(hexChars[b & 0xF]);
            }
            return hex.ToString();
        }
    }
}
