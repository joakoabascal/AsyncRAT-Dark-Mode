using Server.RenamingObfuscation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.RenamingObfuscation.Classes
{
    public class Base64 : ICrypto
    {
        public string Encrypt(string dataPlain)
        {
            try
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(dataPlain));
            }

            catch (Exception)
            {
                return null;
            }
        }
    }
}
