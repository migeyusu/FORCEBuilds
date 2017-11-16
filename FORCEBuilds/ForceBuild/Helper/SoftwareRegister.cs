using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Web.UI.WebControls;
using FORCEBuild.Windows.CIM;
using Xunit;

namespace FORCEBuild.Helper
{

    /* 1.查询用户机的mac、硬盘序列号、CPUID
     * 2.生成哈希码（数字摘要），然后比较实现授权检测
     * 如果用户反编译成功软件，得到了生成算法并替换了原有摘要，将导致被破解
     * 所以:3.将摘要利用私钥生成数字签名，客户端保留公钥，检查数字签名
     * 但是，如果公钥也被替换，将再次导致被破解，即使采用数字证书，离线状态下不可避免的被破解
     * 所以假定用户只能够反编译，但是无法重建软件，假设能够重建软件，任何措施都无效。
     * 所以使用私钥加密生成数字签名即可。
     */

    /// <summary>   
    /// 软件注册服务
    /// </summary>
    public class SoftwareRegister
    {
        public const string SignatureSourceFilter = ".signaturesource";

        public const string SignatureFilter = ".signature";

        public const string EncryptionFilter = ".key";
                
        private static ComputerInfo GetCurrentComputerInfo()
        {
            //经过测试，在不同的操作系统，对同一个类型的字段支持不同，所以不使用反射查找
            string macadd = null;
            var searcher = new ManagementObjectSearcher("select * from Win32_NetworkAdapterConfiguration");
            foreach (ManagementObject managebase in searcher.Get()) {
                foreach (var property in managebase.Properties)
                    if (property.Name == "IPEnabled") {
                        if ((bool) property.Value) {
                            PropertyData data;
                            if ((data = managebase.Properties.Cast<PropertyData>()
                                    .FirstOrDefault(managebaseProperty => managebaseProperty.Name ==
                                                                          nameof(Win32_NetworkAdapterConfiguration
                                                                              .MACAddress))) != null)
                                macadd = data.Value.ToString();
                        }
                        break;
                    }
                if (macadd != null)
                    break;
            }
            if (macadd == null)
                throw new Exception("无法找到可用网卡！");

            string cpuid = null;
            searcher = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (ManagementObject managementObject in searcher.Get()) {
                foreach (var property in managementObject.Properties)
                    if (property.Name == nameof(Win32_Processor.ProcessorId))
                        cpuid = property.Value.ToString();
                break;
            }
            if (cpuid == null)
                throw new Exception("无法获取CPU信息！");

            string diskid = null;
            searcher=new ManagementObjectSearcher("select * from Win32_DiskDrive");
            foreach (ManagementObject managementObject in searcher.Get()) {
                foreach (var property in managementObject.Properties)
                    if (property.Name == nameof(Win32_DiskDrive.SerialNumber))
                        diskid = property.Value.ToString();
                //检查首个硬盘
                break;
            }
            if (diskid == null)
                throw new Exception("无法找到可用硬盘！");

            var info = new ComputerInfo {
                CPUID = cpuid,
                MacAddress = macadd,
                DiskID = diskid
            };
            
            return info;
        }

        /// <summary>
        /// 检查签名
        /// </summary>
        /// <param name="signaturefilepath">签名路径</param>
        /// <param name="xmlpk">公钥</param>
        /// <returns></returns>
        public static bool CheckSignature(string signaturefilepath,string xmlpk)
        {
            
            var info = GetCurrentComputerInfo();
            var serializer = new BinaryFormatter();
            var stream = new MemoryStream();
            serializer.Serialize(stream, info);
            var data = stream.ToArray();
            using (var provider = new RSACryptoServiceProvider()) {
                provider.FromXmlString(xmlpk);
                using (var fs = new FileStream(signaturefilepath, FileMode.Open)) {
                    var sig = new byte[fs.Length];
                    fs.Read(sig, 0, sig.Length);
                    return provider.VerifyData(data, CryptoConfig.MapNameToOID("SHA256"), sig);
                }
            }
        }
        /// <summary>
        /// 检查签名
        /// </summary>
        /// <param name="signaturefilepath">签名路径</param>
        /// <param name="xmlpk">公钥</param>
        /// <returns></returns>
        public static bool CheckSignatureFile(string signaturefilepath, string pkpath)
        {
            if (!File.Exists(signaturefilepath)||!File.Exists(pkpath)) {
                throw new Exception("签名或公钥文件不存在");
            }   
            using (var streamReader = new StreamReader(pkpath)) {
                var info = GetCurrentComputerInfo();
                var serializer = new BinaryFormatter();
                var stream = new MemoryStream();
                serializer.Serialize(stream, info);
                var data = stream.ToArray();
                using (var provider = new RSACryptoServiceProvider()) {
                    provider.FromXmlString(streamReader.ReadToEnd());
                    using (var fs = new FileStream(signaturefilepath, FileMode.Open)) {
                        var sig = new byte[fs.Length];
                        fs.Read(sig, 0, sig.Length);
                        return provider.VerifyData(data, CryptoConfig.MapNameToOID("SHA256"), sig);
                    }
                }
            }
        }


        /* 1.私钥解密
         * 2.私钥签名
         */

        /// <summary>
        /// 将签名源签名
        /// </summary>
        /// <param name="signaturesource">签名源路径</param>
        /// <param name="signature">导出的签名路径</param>
        /// <param name="xmlprivatekey">xml私钥</param>
        public static void SaveSignature(string signaturesource, string signature, string xmlprivatekey)
        {   
            if (!signaturesource.EndsWith(SignatureSourceFilter)) {
                throw new Exception("错误的签名源路径！");
            }
            if (!signature.EndsWith(SignatureFilter)) {
                throw new Exception("错误的签名导出路径！");
            }
            using (var provider = new RSACryptoServiceProvider()) {
                provider.FromXmlString(xmlprivatekey);
                using (var inputStream = new FileStream(signaturesource, FileMode.Open, FileAccess.Read)) {
                    var inputbytes = new byte[inputStream.Length];
                    inputStream.Read(inputbytes, 0, inputbytes.Length);
                    using (var outputsStream = new FileStream(signature, FileMode.Create, FileAccess.Write)) {
                        var outputbytes = provider.Decrypt(inputbytes, false);
                        var sig = provider.SignData(outputbytes, CryptoConfig.MapNameToOID("SHA256"));
                        outputsStream.Write(sig, 0, sig.Length);
                    }
                }
            }
        }

        /* 1.取得uid
         * 2.序列化
         * 3.公钥加密
         */

        /// <summary>
        /// 根据本机创建签名源
        /// </summary>
        /// <param name="signaturesourcepath">导出的签名源</param>
        /// <param name="publickey">xml格式的公钥</param>
        public static void SaveSignatureSource(string signaturesourcepath,string publickey)
        {
            if (!signaturesourcepath.EndsWith(SignatureSourceFilter)) {
                throw new Exception("错误的路径！");
            }
            var memoryStream = new MemoryStream();
            var info = GetCurrentComputerInfo();
            var formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, info);
            using (var provider = new RSACryptoServiceProvider()) {
                provider.FromXmlString(publickey);
                var encrypt = provider.Encrypt(memoryStream.ToArray(), false);
                using (var fileStream = new FileStream(signaturesourcepath, FileMode.Create, FileAccess.ReadWrite)) {
                    fileStream.Write(encrypt, 0, encrypt.Length);
                }
            }
        }
    }


    /// <summary>
    /// 对应签名源文件
    /// </summary>
    [Serializable]
    public class ComputerInfo
    {
        public string MacAddress { get; set; }
        public string CPUID { get; set; }
        public string DiskID { get; set; }
    }

}