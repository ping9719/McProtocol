using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McProtocol.Help
{
    /// <summary>
    /// Byte帮助
    /// </summary>
    public static class ByteHelp
    {
        /// <summary>
        /// byte转为字符串
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="hiLoReversal">高低位是否反转</param>
        /// <returns>字符串</returns>
        public static string ByteToString(byte[] bytes, Encoding encoding, bool hiLoReversal = false)
        {
            if (bytes == null)
                return null;

            if (hiLoReversal)
            {
                var byteList = bytes.ToList();
                var length = byteList.Count();
                for (int i = 0; i < length; i += 2)
                {
                    var a = byteList[i];
                    var b = byteList[i + 1];

                    byteList[i] = b;
                    byteList[i + 1] = a;
                }
                return encoding.GetString(byteList.ToArray());
            }
            else
            {
                return encoding.GetString(bytes);
            }
        }
    }
}
