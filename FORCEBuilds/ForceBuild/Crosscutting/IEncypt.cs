using System.Text;

namespace FORCEBuild.AOP
{
    public class IEncypt
    {
     //   static byte[] bytes = new byte[8] { 128, 255, 89, 30, 25, 34, 1, 0 };
        //public static string DESEncry(string originstr)
        //{
        //    byte[] originbyte=Encoding.Unicode.GetBytes(originstr);
        //    DES des = DES.Create();
        //    ICryptoTransform ict = des.CreateEncryptor();
        //    des.Key = bytes;
        //    MemoryStream ms = new MemoryStream();
        //    CryptoStream cs = new CryptoStream(ms, ict, CryptoStreamMode.Write);
        //    cs.Write(originbyte, 0, originbyte.Length);
        //    cs.FlushFinalBlock();
        //    return Encoding.ASCII.GetString(ms.ToArray());
        //}
        //public static string DesDecry(string str)
        //{
        //    byte[] vals = Encoding.ASCII.GetBytes(str);
        //    DES des = DES.Create();
        //    des.Key = bytes;
        //    ICryptoTransform dict = des.CreateDecryptor();
        //    MemoryStream ms = new MemoryStream();
        //    CryptoStream cs = new CryptoStream(ms, dict, CryptoStreamMode.Write);
        //    cs.Write(vals, 0, vals.Length);
        //    cs.FlushFinalBlock();
        //    return Encoding.Unicode.GetString(ms.ToArray());
        //}
        public static string Encry(string ori)
        {
            var bytes = Encoding.Unicode.GetBytes(ori);
            var vals = new byte[bytes.Length];
            for (var i = 0; i < bytes.Length; ++i)
            {
                vals[bytes.Length - 1 - i] = bytes[i];
            }
            return Encoding.Unicode.GetString(vals);
        }
        public static string Decry(string ori)
        {
            var bytes = Encoding.Unicode.GetBytes(ori);
            var vals = new byte[bytes.Length];
            for (var i = 0; i < bytes.Length; ++i)
            {
                vals[bytes.Length - 1 - i] = bytes[i];
            }
            return Encoding.Unicode.GetString(vals);
        }
    }
}
