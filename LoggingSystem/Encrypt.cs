using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace LoggingSystem
{
    public sealed class EncryptUtil
    {
        #region Base64加密解密
        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="input">需要加密的字符串</param>
        /// <returns></returns>
        public static string Base64Encrypt(string input)
        {
            return Base64Encrypt(input, new UTF8Encoding());
        }

        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="input">需要加密的字符串</param>
        /// <param name="encode">字符编码</param>
        /// <returns></returns>
        public static string Base64Encrypt(string input, Encoding encode)
        {
            return Convert.ToBase64String(encode.GetBytes(input));
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="input">需要解密的字符串</param>
        /// <returns></returns>
        public static string Base64Decrypt(string input)
        {
            return Base64Decrypt(input, new UTF8Encoding());
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="input">需要解密的字符串</param>
        /// <param name="encode">字符的编码</param>
        /// <returns></returns>
        public static string Base64Decrypt(string input, Encoding encode)
        {
            ce();
            return encode.GetString(Convert.FromBase64String(input));
        }
        #endregion

        #region MD5
        public static void ce()
        {
            //可以选择MD5 Sha1 Sha256 Sha384 Sha512
            string strAlgName = HashAlgorithmNames.Md5;

            // 创建一个 HashAlgorithmProvider 对象
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);

            // 创建一个可重用的CryptographicHash对象           
            CryptographicHash objHash = objAlgProv.CreateHash();


            string strMsg1 = "这是一段待加密的字符串";
            IBuffer buffMsg1 = CryptographicBuffer.ConvertStringToBinary(strMsg1, BinaryStringEncoding.Utf16BE);
            objHash.Append(buffMsg1);
            IBuffer buffHash1 = objHash.GetValueAndReset();
            string strHash1 = CryptographicBuffer.EncodeToBase64String(buffHash1);


            string strMsg2 = "和前面一串不相同的字符串";
            IBuffer buffMsg2 = CryptographicBuffer.ConvertStringToBinary(strMsg2, BinaryStringEncoding.Utf16BE);
            objHash.Append(buffMsg2);
            IBuffer buffHash2 = objHash.GetValueAndReset();
            string strHash2 = CryptographicBuffer.EncodeToBase64String(buffHash2);


            string strMsg3 = "每个都不相同";
            IBuffer buffMsg3 = CryptographicBuffer.ConvertStringToBinary(strMsg3, BinaryStringEncoding.Utf16BE);
            objHash.Append(buffMsg3);
            IBuffer buffHash3 = objHash.GetValueAndReset();
            string strHash3 = CryptographicBuffer.EncodeToBase64String(buffHash3);

        }
        #endregion

        #region ipv4转换整数

        private const long PART1 = 0xff000000;
        private const long PART2 = 0xff0000;
        private const long PART3 = 0xff00;
        private const long PART4 = 0xff;

        /** 将IP地址长整型数值转化为IPv4字符串 */
        public static string ip2Str(long ip, bool ByteReverse = false)
        {
            string ipStr = "";
            if (ByteReverse)
            {
                ipStr += (ip & PART4).ToString();
                ipStr += "." + ((ip & PART3) >> 8);
                ipStr += "." + ((ip & PART2) >> 16);
                ipStr += "." + ((ip & PART1) >> 24);
            }
            else
            {
                ipStr += ((ip & PART1) >> 24).ToString();
                ipStr += "." + ((ip & PART2) >> 16);
                ipStr += "." + ((ip & PART3) >> 8);
                ipStr += "." + (ip & PART4);
            }
            return ipStr;
        }

        /** 将IPv4字符串转化为对应的长整型整数 */
        public static long ip2Long(String ip)
        {
            String[] p4 = Regex.Split(ip, ".");
            long ipInt = 0;
            long part = long.Parse(p4[0]);
            ipInt = ipInt | (part << 24);
            part = long.Parse(p4[1]);
            ipInt = ipInt | (part << 16);
            part = long.Parse(p4[2]);
            ipInt = ipInt | (part << 8);
            part = long.Parse(p4[3]);
            ipInt = ipInt | (part);
            return ipInt;
        }
        #endregion
    }
}
