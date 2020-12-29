using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FORCEBuild.Net.Base
{
    /// <summary>
    /// FTP node for mvvm ui 
    /// </summary>
    public class FtpItem
    {
        private Uri _selfUrl;
        public FtpItemType FtpItemType { get; set; }
                
        public string Name { get; set; }

        /// <summary>
        /// 表示当前文件/夹路径；为绝对路径，文件夹后缀带/
        /// ————可以作为RestFul形式的基础
        /// </summary>
        public Uri AbsoluteUri
        {
            get => _selfUrl;
            set => _selfUrl = value.ToString().Contains("#") ? new Uri(value.ToString().Replace("#", Uri.HexEscape('#'))) : value;
        }

        public List<FtpItem> FolderItems { get; set; }

        public FtpItem()
        {
            FolderItems = new List<FtpItem>();  
        }
        
        /// <summary>
        /// 供UI读取的大小
        /// </summary>
        public string DisplaySize
        {
            get
            {
                if (OriSize<1000)
                {
                    return $"{OriSize}.Bytes";

                }
                OriSize /= 1000;
                if (OriSize<1000)
                {
                    return $"{OriSize / 1000}.{OriSize % 1000} KB";
                }
                OriSize /= 1000;
                return OriSize<1000 ? $"{OriSize / 1000}.{OriSize % 1000} MB" : $"{OriSize / 1000}.{OriSize % 1000} GB";
            }
        }

        public long OriSize { get; set; }

        public string ReviseTimestamp { get; set; }
    }
}
