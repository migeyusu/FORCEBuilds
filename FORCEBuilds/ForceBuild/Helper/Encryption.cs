using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FORCEBuild.Helper
{
    /// <summary>
    /// 加解密
    /// </summary>
    public class Encryption
    {
        /// <summary>
        /// 创建公私钥对
        /// </summary>
        /// <param name="privatekeypath"></param>
        /// <param name="publickeypath"></param>
        public static void CreateRSAFile(string privatekeypath, string publickeypath)
        {
            if (!privatekeypath.EndsWith(SoftwareRegister.EncryptionFilter)) {
                throw new Exception("目标文件类型错误");
            }
            if (!publickeypath.EndsWith(SoftwareRegister.EncryptionFilter)) {
                throw new Exception("目标文件类型错误");
            }
            //密钥长度越长，加密强度越大，但加密速度也越慢，但是必须增加密钥长度，因为最大加密大小受限于key大小
            //int keySize = rsa.KeySize/8; //KeySize in number of bits,
            // so we need to divide that by 8 to convert it 
            //to bytes.
            //int blockSize = keySize - 11; //keysize less 11 bytes = block size. 
            var provider = new RSACryptoServiceProvider(4096);
            using (var writer = new StreamWriter(privatekeypath,false)) {
                var xmlstr = provider.ToXmlString(true);
                writer.WriteLine(xmlstr);
            }
            using (var writer = new StreamWriter(publickeypath, false)) {
                var xmlstr = provider.ToXmlString(false);
                writer.WriteLine(xmlstr);
            }
        }
    }
}