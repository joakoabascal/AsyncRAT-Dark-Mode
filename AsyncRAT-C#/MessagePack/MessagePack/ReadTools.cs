using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagePackLib.MessagePack
{
    class ReadTools
    {
        public static String ReadString(Stream ms, int len)
        {
            byte[] rawBytes = new byte[len];
            ms.Read(rawBytes, 0, len);
            return BytesTools.GetString(rawBytes);
        }

        public static String ReadString(Stream ms)
        {
            byte strFlag = (byte)ms.ReadByte();
            return ReadString(strFlag, ms);
        }

        public static String ReadString(byte strFlag, Stream ms)
        {

            byte[] rawBytes = null;
            int len = 0;
            if ((strFlag >= 0xA0) && (strFlag <= 0xBF))
            {
                len = strFlag - 0xA0;
            }
            else if (strFlag == 0xD9)
            {
                len = ms.ReadByte();
            }
            else if (strFlag == 0xDA)
            {
                rawBytes = new byte[2];
                ms.Read(rawBytes, 0, 2);
                rawBytes = BytesTools.SwapBytes(rawBytes);
                len = BitConverter.ToUInt16(rawBytes, 0);
            }
            else if (strFlag == 0xDB)
            {
                rawBytes = new byte[4];
                ms.Read(rawBytes, 0, 4);
                rawBytes = BytesTools.SwapBytes(rawBytes);
                len = BitConverter.ToInt32(rawBytes, 0);
            }
            rawBytes = new byte[len];
            ms.Read(rawBytes, 0, len);
            return BytesTools.GetString(rawBytes);
        }
    }
}