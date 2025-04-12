using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.RenamingObfuscation.Classes
{
    internal static class DecryptionHelper
    {
        public static string Decrypt_Base64(string dataEnc)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(dataEnc));
            }

            catch (Exception)
            {
                return null;
            }
        }
    }
}
