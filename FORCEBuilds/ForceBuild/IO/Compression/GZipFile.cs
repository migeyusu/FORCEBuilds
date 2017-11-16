using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FORCEBuild.IO.Compression
{
    /// <summary>
    /// ���ļ���ѹ������һ������±��������ļ����ļ��У�4byte�������·���ļ������ȣ�+�ļ��ַ�������+8byte���ļ�ʵ�ʴ�С��
    /// ���ļ���ѹ������4byte(�ļ���byte����)+�ļ���+�ļ�
    /// </summary>
    public class GZipFile
    {
        /// <summary>
        /// </summary>
        /// <param name="sourceFile">��������·���ļ�</param>
        /// <param name="destinationFile">���gzip</param>
        public static void FileCompression(string sourceFile, string destinationFile, Action<ProgressArgs> ProgressCallback)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException();
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (var ms = new MemoryStream())
                {
                    using (var destinationStream = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        using (var compressStream = new GZipStream(destinationStream, CompressionMode.Compress, true))
                        {
                            var position = sourceFile.LastIndexOf('\\') + 1;
                            var filename = sourceFile.Substring(position, sourceFile.Length - position);
                            var name = Encoding.Unicode.GetBytes(filename);
                            var pa = new ProgressArgs { Description = "����������", PreFileName = filename, Position = 0, Total = sourceStream.Length };
                            ProgressCallback(pa);
                            var tag = BitConverter.GetBytes(name.Length);
                            ms.Write(tag, 0, tag.Length);
                            ms.Write(name, 0, name.Length);
                            pa.Description = "����ѹ����";
                            sourceStream.Seek(0, SeekOrigin.Begin);
                            var buffer = new byte[4096];
                            var count = 0;
                            while ((count = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                compressStream.Write(buffer, 0, count);
                                compressStream.Flush();
                                pa.Position += count;
                                ProgressCallback(pa);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="sourceFile">��������·���ļ�</param>
        /// <param name="destinationFile">���gzip</param>
        public static void FileCompression(string sourceFile, string destinationFile)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException();
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (var ms = new MemoryStream())
                {
                    using (var destinationStream = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        using (var compressStream = new GZipStream(destinationStream, CompressionMode.Compress, true))
                        {
                            var position = sourceFile.LastIndexOf('\\') + 1;
                            var filename = sourceFile.Substring(position, sourceFile.Length - position);
                            var name = Encoding.Unicode.GetBytes(filename);
                            var tag = BitConverter.GetBytes(name.Length);
                            ms.Write(tag, 0, tag.Length);
                            ms.Write(name, 0, name.Length);
                            sourceStream.CopyTo(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            ms.CopyTo(compressStream);
                            compressStream.Flush();
                        }
                    }
                }
            }
        }

        public static void FileDecompression(string sourceFile, string destinationFile, bool specify = false)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException();
            using (var sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None))
            {
                using (var ms = new MemoryStream())
                {
                    using (var compressStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        compressStream.CopyTo(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        var tag = new byte[4];
                        ms.Read(tag, 0, tag.Length);
                        var name = new byte[BitConverter.ToInt32(tag, 0)];
                        ms.Read(name, 0, name.Length);
                        if (!specify)
                            destinationFile += "\\" + Encoding.Unicode.GetString(name);
                        var destinationStream = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write);
                        var count = 0;
                        var buffer = new byte[4096];
                        while ((count = ms.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            destinationStream.Write(buffer, 0, count);
                            destinationStream.Flush();
                        }
                    }
                }
            }
        }

        public static void FileDecompression(string sourceFile, string destinationFile, Action<ProgressArgs> ProgressCallback, bool specify = false)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException();
            using (var sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None))
            {
                using (var ms = new MemoryStream())
                {
                    using (var compressStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        compressStream.CopyTo(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        var tag = new byte[4];
                        ms.Read(tag, 0, tag.Length);
                        var name = new byte[BitConverter.ToInt32(tag, 0)];
                        ms.Read(name, 0, name.Length);
                        if (!specify)
                            destinationFile += "\\" + Encoding.Unicode.GetString(name);
                        var p = destinationFile.LastIndexOf("\\") + 1;
                        var destinationStream = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write);
                        var pa = new ProgressArgs {
                            Description = "���ڽ�ѹ",
                            Position = 0,
                            PreFileName = destinationFile.Substring(p,
                                destinationFile.Length - p),
                            Total = destinationStream.Length
                        };
                        ProgressCallback(pa);
                        var count = 0;
                        var buffer = new byte[4096];
                        while ((count = ms.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            destinationStream.Write(buffer, 0, count);
                            destinationStream.Flush();
                            pa.Position += count;
                            ProgressCallback(pa);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// �ļ������ļ�����ѹ����
        /// </summary>
        /// <param name="source">�ļ���·��</param>
        /// <param name="ouputpath">���ļ�����·��</param>
        public static void DirCompression(string source, string ouputpath)
        {
            if (!Directory.Exists(source))
                throw new FileNotFoundException();
            using (var output = new FileStream(ouputpath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var gz = new GZipStream(output, CompressionMode.Compress, true))
                {
                    ChildDirCompression(source, "", gz);
                    gz.Close();
                }
            }
        }
        /// <summary>
        /// ��Ŀ¼�����·������������б��"\"
        /// </summary>
        /// <param name="rootdir">��Ŀ¼,�������ķ�б��</param>
        /// <param name="relapath">��һ���ļ��е����·��������ǰ�ķ�б��</param>
        /// <param name="gz"></param>
        private static void ChildDirCompression(string rootdir, string relapath, GZipStream gz)
        {
            var files = Directory.GetFiles(rootdir + relapath);
            int p0;
            string filename;
            foreach (var file in files)
            {
                var ms = new MemoryStream();
                var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);
                p0 = file.LastIndexOf("\\") + 1;
                filename = file.Substring(p0, file.Length - p0);
                filename = relapath + "\\" + filename;
                var name = Encoding.Unicode.GetBytes(filename);
                var tag = BitConverter.GetBytes(name.Length);
                var filelen = BitConverter.GetBytes(fs.Length);//8byte
                ms.Write(tag, 0, tag.Length);
                ms.Write(name, 0, name.Length);
                ms.Write(filelen, 0, filelen.Length);
                fs.CopyTo(ms);
                fs.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(gz);
                ms.Flush();
                ms.Close();
                fs.Close();
            }
            var folders = Directory.GetDirectories(rootdir + relapath);
            string foldername;
            int p;
            foreach (var folder in folders)
            {
                p = folder.LastIndexOf("\\") + 1;
                foldername = folder.Substring(p, folder.Length - p);
                ChildDirCompression(rootdir, relapath + "\\" + foldername, gz);
            }
        }

        public static void DirDecompression(string source, string outputdir)
        {
            var sourcestream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None);
            var gz = new GZipStream(sourcestream, CompressionMode.Decompress);
            var count = 0;
            var namelen = 0;
            var buffer = new byte[4096];
            byte[] namebyte;
            string name, upfolder;
            var tag = new byte[4];
            var lenbyte = new byte[8];
            long filelen = 0;
            while ((count = gz.Read(tag, 0, tag.Length)) > 0)
            {
                var ms = new MemoryStream();
                namelen = BitConverter.ToInt32(tag, 0);
                namebyte = new byte[namelen];
                gz.Read(namebyte, 0, namebyte.Length);
                name = Encoding.Unicode.GetString(namebyte);
                upfolder = outputdir + name.Substring(0, name.LastIndexOf("\\"));//�ϼ�Ŀ¼��ȫ·��
                name = outputdir + name;
                gz.Read(lenbyte, 0, lenbyte.Length);
                filelen = BitConverter.ToInt64(lenbyte, 0);
                ms = new MemoryStream();
                var terms = (int)filelen / 4096;
                var tail = (int)filelen % 4096;
                for (var i = 0; i < terms; ++i)
                {
                    gz.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, buffer.Length);
                }
                gz.Read(buffer, 0, tail);
                ms.Write(buffer, 0, tail);
                ms.Seek(0, SeekOrigin.Begin);
                if (!Directory.Exists(upfolder))
                    Directory.CreateDirectory(upfolder);
                var fs = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.None);
                ms.CopyTo(fs);
                fs.Flush();
                fs.Close();
                ms.Close();
            }
            gz.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="outputdir"></param>
        /// <param name="ProgressCallback">�����޷�ֱ�ӻ�ȡ�����ѹ��������Ĵ�С�����û��totoalֵ</param>
        public static void DirDecompression(string source, string outputdir, Action<ProgressArgs> ProgressCallback, Action Complete)
        {
            var sourcestream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None);
            var gz = new GZipStream(sourcestream, CompressionMode.Decompress);
            var count = 0;
            var namelen = 0;
            var buffer = new byte[4096];
            byte[] namebyte;
            string name, upfolder;
            var tag = new byte[4];
            var lenbyte = new byte[8];
            long filelen = 0;
            while ((count = gz.Read(tag, 0, tag.Length)) > 0)
            {
                var memoryStream = new MemoryStream();
                namelen = BitConverter.ToInt32(tag, 0);
                namebyte = new byte[namelen];
                gz.Read(namebyte, 0, namebyte.Length);
                name = Encoding.Unicode.GetString(namebyte);
                upfolder = outputdir + name.Substring(0, name.LastIndexOf("\\"));//�ϼ�Ŀ¼��ȫ·��
                name = outputdir + name;//���ļ���ȫ·��
                gz.Read(lenbyte, 0, lenbyte.Length);
                filelen = BitConverter.ToInt64(lenbyte, 0);
                var p = name.LastIndexOf("\\") + 1;
                var pa = new ProgressArgs {
                    Description = "���ڽ�ѹ��",
                    Position = 0,
                    PreFileName =
                        name.Substring(p, name.Length - p),
                    PreFileProgress = 0,
                    PreFileTotal = filelen,
                    Total = 0
                };
                memoryStream = new MemoryStream();
                var terms = (int)filelen / 4096;
                var tail = (int)filelen % 4096;
                for (var i = 0; i < terms; ++i)
                {
                    gz.Read(buffer, 0, buffer.Length);
                    pa.PreFileProgress += buffer.Length;
                    ProgressCallback(pa);
                    memoryStream.Write(buffer, 0, buffer.Length);
                }
                gz.Read(buffer, 0, tail);
                pa.PreFileProgress += tail;
                ProgressCallback(pa);
                memoryStream.Write(buffer, 0, tail);
                memoryStream.Seek(0, SeekOrigin.Begin);
                if (!Directory.Exists(upfolder))
                    Directory.CreateDirectory(upfolder);
                var fs = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.None);
                memoryStream.CopyTo(fs);
                fs.Flush();
                fs.Close();
                memoryStream.Close();
                Complete();
            }
            gz.Close();
        }
    }
}