using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Castle.DynamicProxy;


namespace FORCEBuild.Net.Base
{
    //public class ProgressEventArgs : EventArgs
    //{
    //    public string PreFileName { get; set; }
    //    public int TotalProgress { get; set; }
    //    public int PreRate { get; set; }
    //}

    //public class ProduceState
    //{
    //    public bool IsProduce { get; set; }
    //}


    public delegate void RateDelegate(long complished, long total);

    //对URL的直接修改如重命名、删除不检查，错误抛出由基本类实现

    /// <summary>
    /// IIS FTP操作单元
    /// </summary>
    public class FtpGroup
    {
        private const string DirTag = "<DIR>";
        private const int BufferSize = 4096;
        private string _userName, _passWord;
        private const int CallbackInterval = 1000;

        /// <summary>
        /// 基本地址
        /// </summary>
        public string BaseUrl { get; private set; }

        /// <summary>
        /// ftp通用操作功能
        /// </summary>
        /// <param name="baseurl">基础地址，带反斜杠</param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        public FtpGroup(string baseurl = "", string userName = "", string passWord = "")
        {
            BaseUrl = baseurl;
            _userName = userName;
            _passWord = passWord;
        }

        #region static method   

        /// <summary>
        /// todo:重试次数
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool FtpExist(string url,int trytimes=3)
        {
            var repeat = 1;
            while (repeat<trytimes) {
                try {
                    var ftpWebRequest = (FtpWebRequest) WebRequest.Create(url);
                    ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                    ftpWebRequest.Timeout = 600;
                    ftpWebRequest.UseBinary = true;
                    using (ftpWebRequest.GetResponse()) { }
                    return true;
                }
                catch {
                    // ignored
                }
                finally {
                    repeat += 1;
                }
            }
            return false;
        }

        public static bool FileExist(string path)
        {
            try {
                var request = (FtpWebRequest) WebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                request.Timeout = 200;
                request.UseBinary = true;
                using (request.GetResponse()) { }
            }
            catch {
                return false;
            }
            return true;
        }

        public static bool FolderExist(string path)
        {
            try {
                var request = (FtpWebRequest) WebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Timeout = 200;
                request.UseBinary = true;
                using (request.GetResponse()) { }
            }
            catch {
                return false;
            }
            return true;
        }

        #endregion

        #region instance method

        /// <summary>
        /// 当前绑定的baseurl是否可连接
        /// </summary>
        /// <returns>true：baseftp存在，false：无法连上baseftp</returns>
        public bool IsAlive()
        {
            try {
                var ftpWebRequest = (FtpWebRequest) WebRequest.Create(BaseUrl);
                ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpWebRequest.Timeout = 200;
                ftpWebRequest.UseBinary = true;
                using (ftpWebRequest.GetResponse()) { }
            }
            catch {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 相对路径的文件是否存在
        /// </summary>
        /// <param name="relativepath"></param>
        /// <returns></returns>
        public bool IsFileExist(string relativepath)
        {
            try {
                var request = (FtpWebRequest) WebRequest.Create(BaseUrl + relativepath);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                request.Timeout = 200;
                request.UseBinary = true;
                using (request.GetResponse()) { }
            }
            catch {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 相对路径的文件夹是否存在
        /// </summary>
        /// <param name="relativepath"></param>
        /// <returns></returns>
        public bool IsFolderExist(string relativepath)
        {
            try {
                var request = (FtpWebRequest) WebRequest.Create(BaseUrl + relativepath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Timeout = 200;
                request.UseBinary = true;
                using (request.GetResponse()) { }
            }
            catch {
                return false;
            }
            return true;
        }


        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="preFileName">目标文件路径</param>
        /// <param name="newFileName">修改后文件名</param>
        public void RenameRelative(string preFileName, string newFileName)
        {
            RenameAbsolute(BaseUrl + preFileName, newFileName);
        }

        public static void RenameAbsolute(string targetPath, string newName)
        {
            var ftpRequest = (FtpWebRequest) WebRequest.Create(targetPath);
            ftpRequest.Method = WebRequestMethods.Ftp.Rename;
            ftpRequest.RenameTo = newName;
            ftpRequest.UseBinary = true;
            using (ftpRequest.GetResponse()) { }
        }

        #region delete

        public void DeleteRelativeFile(string targetFilePath)
        {
            DeleteAbsoluteFile(BaseUrl + targetFilePath);
        }

        public static void DeleteAbsoluteFile(string path)
        {
            var ftpRequest = (FtpWebRequest) WebRequest.Create(path);
            ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            using (ftpRequest.GetResponse()) { }
        }

        /// <summary>
        /// 
        /// <summary>
        /// <param name="path">不带前缀反斜杠从一级目录开始的地址</param>
        public void DeleteRelativeDirectory(string path)
        {

            DeleteAbsoluteDirectory(BaseUrl + path);
        }

        public static void DeleteAbsoluteDirectory(string path)
        {
            var ftpRequest = (FtpWebRequest) WebRequest.Create(path);
            ftpRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
            using (ftpRequest.GetResponse()) { }
        }

        public void CleanRelativeFolder(string relativeDir)
        {
            DeleteRelativeDirectory(relativeDir);
            CreateDirectoryRelative(relativeDir);
        }

        public static void CleanAbsoluteFolder(string dir)
        {
            DeleteAbsoluteDirectory(dir);
            CreateDirectoryAbsolute(dir);
        }

        #endregion

        #region Append

        /// <summary>
        /// 创建相对路径文件夹
        /// </summary>
        /// <param name="listPath">不带前缀反斜杠从一级目录开始的地址</param>
        public void CreateDirectoryRelative(string listPath)
        {
            CreateDirectoryAbsolute(BaseUrl + listPath);
        }

        public static void CreateDirectoryAbsolute(string dirpath, string username = "", string password = "")
        {
            var ftpRequest = (FtpWebRequest) WebRequest.Create(dirpath);
            if (string.IsNullOrEmpty(username))
                ftpRequest.Credentials = new NetworkCredential(username, password);
            ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            using (ftpRequest.GetResponse()) { }
        }

        /// <summary>
        /// 上传/更新文件，如果不存在则创建，如果存在目标文件名则更新
        /// </summary>
        /// <param name="relativeFilePath">带文件名的目标路径</param>
        /// <param name="localFile">本地路径</param>
        /// <param name="rateCallback"></param>
        public void UploadFileRelative(string relativeFilePath, string localFile, RateDelegate rateCallback = null)
        {
            UploadFileAbsolute(BaseUrl + relativeFilePath, localFile, rateCallback);
        }
        
        /// <summary>
        /// 上传本地文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="localFile"></param>
        /// <param name="rateCallback"></param>
        public static void UploadFileAbsolute(string path, string localFile, RateDelegate rateCallback = null)
        {
            if (!File.Exists(localFile)) {
                throw new ArgumentException("上传的源文件已不存在或地址错误");
            }
            var fileInfo = new FileInfo(localFile);
            using (var fileStream = fileInfo.OpenRead()) {
                UploadStreamAbsolute(path, fileStream, fileInfo.Length, rateCallback);
            }
        }

        /// <summary>
        /// 上传内存流
        /// </summary>
        /// <param name="relativeFilePath"></param>
        /// <param name="stream"></param>
        /// <param name="len"></param>
        /// <param name="rateCallback"></param>
        public void UploadStreamRelative(string relativeFilePath, Stream stream, long len = 0,
            RateDelegate rateCallback = null)
        {
            UploadStreamAbsolute(BaseUrl + relativeFilePath, stream, len, rateCallback);
        }

        /// <summary>
        /// 上传内存流
        /// </summary>
        /// <param name="path"></param>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <param name="rateCallback"></param>
        public static void UploadStreamAbsolute(string path, Stream stream, long length = 0,
            RateDelegate rateCallback = null)
        {
            var request = (FtpWebRequest) WebRequest.Create(path);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.UsePassive = true;
            request.ContentLength = stream.Length;
            var content = new byte[BufferSize - 1 + 1];
            stream.Position = 0;
            try {
                using (var requestStream = request.GetRequestStream())
                {
                    long position = 0;
                    int dataRead;
                    do
                    {
                        dataRead = stream.Read(content, 0, BufferSize);
                        position += dataRead;
                        rateCallback?.Invoke(position, length);
                        requestStream.Write(content, 0, dataRead);
                    } while (!(dataRead < BufferSize));
                }
            }
            catch (Exception) {
                DeleteAbsoluteFile(path);
                throw;
            }


        }

        #endregion


        #region download

        /// <summary>
        /// 若UsePassive设置为 true，FTP服务器可能不会发送文件的大小，且下载进度可能始终为零。
        /// 若UsePassive设置为 false，防火墙可能会引发警报并阻止文件下载。
        /// </summary>
        /// <param name="localPath">文件路径</param>
        /// <param name="relativeFilePath">ftp文件相对地址</param>
        /// <param name="rateCallback"></param>
        public void DownloadRelativeFile(string localPath, string relativeFilePath, RateDelegate rateCallback = null)
        {
            DownLoadAbsoluteFile(localPath, BaseUrl + relativeFilePath, rateCallback);
        }

        public static void DownLoadAbsoluteFile(string local, string target, RateDelegate rateCallback = null)
        {
            using (var fileStream = new FileStream(local, FileMode.Create)) {
                try {
                    DownLoadAbsoluteFile(fileStream, target, rateCallback);
                }
                catch (Exception) {
                    File.Delete(local);
                    throw;
                }
            }

        }

        public static void DownLoadAbsoluteFile(Stream stream, string path, RateDelegate rateCallBack = null)
        {
            long fileLength = 0;
            if (rateCallBack != null) {
                var request = (FtpWebRequest) WebRequest.Create(path);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                request.UseBinary = true;
                using (var ftpWebResponse = (FtpWebResponse) request.GetResponse()) {
                    fileLength = ftpWebResponse.ContentLength;
                }
            }
            var ftpWebRequest = (FtpWebRequest) WebRequest.Create(path);
            ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            ftpWebRequest.UseBinary = true;
            ftpWebRequest.UsePassive = false;
            using (var response = (FtpWebResponse) ftpWebRequest.GetResponse()) {
                using (var responseStream = response.GetResponseStream()) {
                    var buffer = new byte[BufferSize];
                    int read;
                    long position = 0;
                    do {
                        read = responseStream.Read(buffer, 0, buffer.Length);
                        position += read;
                        rateCallBack?.Invoke(position, fileLength);
                        stream.Write(buffer, 0, read);
                    } while (read != 0);
                }
            }
        }

        /// <summary>
        /// 下载整个文件夹
        /// </summary>
        /// <param name="localFolder">文件夹路径，无尾斜杠</param>
        /// <param name="relativeFolder">默认包含最后一个斜杠</param>
        public void DownloadRelativeFolder(string localFolder, string relativeFolder)
        {
            DownloadAbsoluteFolder(localFolder, BaseUrl + relativeFolder);
        }

        public static void DownloadAbsoluteFolder(string localFolder, string folder)
        {
            var list = DetailAbsoluteFileDirList(folder);
            if (list.Count == 0)
                return;
            foreach (var x in list) {
                string detailname;
                if (x.LastIndexOf(DirTag, StringComparison.Ordinal) == -1) {
                    detailname = x.Substring(39).Trim();
                    DownLoadAbsoluteFile(localFolder + "\\" + detailname, folder + detailname);
                }
                else {
                    detailname = x.Substring(x.LastIndexOf(DirTag, StringComparison.Ordinal) + 5).Trim();
                    var newpath = localFolder + "\\" + detailname;
                    if (!Directory.Exists(newpath))
                        Directory.CreateDirectory(newpath);
                    DownloadAbsoluteFolder(newpath, folder + detailname + "/");
                }
            }
        }


        #endregion


        /// <summary>
        /// 返回文件/文件夹名数组
        /// </summary>
        /// <param name="relativeDir"></param>
        /// <returns></returns>4
        public List<string> FileDirList(string relativeDir)
        {
            var ftpRequest = (FtpWebRequest) WebRequest.Create(BaseUrl + relativeDir);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            var list = new List<string>();
            using (var reader = new StreamReader(ftpRequest.GetResponse().GetResponseStream())) {
                var str = reader.ReadLine();
                while (str != null) {
                    list.Add(str);
                    str = reader.ReadLine();
                }
            }
            return list;
        }

        /// <summary>
        /// 文件夹下的所有文件
        /// </summary>
        /// <param name="relative"></param>
        /// <returns>只包含文件名</returns>
        public List<string> FileList(string relative)
        {
            var reg = new Regex(@" \d+? ([\s\S]+)");
            var ftpRequest = (FtpWebRequest) WebRequest.Create(BaseUrl + relative);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            var list = new List<string>();
            using (var response = (FtpWebResponse) ftpRequest.GetResponse()) {
                using (var reader = new StreamReader(response.GetResponseStream())) {
                    string line;
                    while ((line = reader.ReadLine()) != null) {
                        if (line.IndexOf(DirTag) == -1) {
                            var mat = reg.Match(line);
                            line = mat.Groups[1].Value;
                            list.Add(line);
                        }
                    }

                }
            }
            return list;

        }

        public List<string> FolderList(string relative)
        {
            var ftpRequest = (FtpWebRequest) WebRequest.Create(BaseUrl + relative);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            var fwr = (FtpWebResponse) ftpRequest.GetResponse();
            var list = new List<string>();
            using (var sr = new StreamReader(fwr.GetResponseStream())) {
                string str;
                while ((str = sr.ReadLine()) != null) {
                    if (str.IndexOf(DirTag) == -1) continue;
                    str = str.Substring(str.LastIndexOf(DirTag) + 5).TrimStart();
                    list.Add(str);
                }
            }
            return list;
        }

        ///  <summary>
        ///  以FTP为基准同步,只同步文件夹下一级文件
        ///  </summary>
        /// <param name="localdir">指向文件夹</param>
        /// <param name="ftpdir">ftp文件夹相对地址</param>
        /// <param name="increment">只增加文件</param>
        public void FtpSync(string ftpdir, string localdir, bool increment = true)
        {
            var remotelist = FileList(ftpdir);
            var localfiles = Directory.GetFiles(localdir);
            var locallist = localfiles.Select(t => t.Substring(t.LastIndexOf('\\') + 1)).ToList();
            foreach (var t in remotelist) {
                if (locallist.Contains(t))
                    locallist.Remove(t);
                else {
                    DownloadRelativeFile(localdir + "\\" + t, ftpdir);
                }
            }
            if (locallist.Count > 0 && !increment) {
                foreach (var t in locallist)
                    File.Delete(localdir + "\\" + t);
            }
        }

        /// <summary>
        /// 以本地为基准同步
        /// </summary>
        /// <param name="ftpdir"></param>
        /// <param name="locdir"></param>
        /// <param name="increment">true：只添加不删除
        /// false:多余的</param>
        public void LocalSync(string ftpdir, string locdir, bool increment = true)
        {
            var localfiles = Directory.GetFiles(locdir);
            var remotelist = FileList(ftpdir);
            var locallist = localfiles.Select(t => t.Substring(t.LastIndexOf('\\') + 1)).ToList();
            for (var i = 0; i < locallist.Count; ++i) {
                if (remotelist.Contains(locallist[i])) {
                    remotelist.Remove(locallist[i]);
                }
                else {
                    UploadFileRelative(ftpdir + "/" + locallist[i], localfiles[i]);
                }
            }
            if (!increment && remotelist.Count > 0) {
                foreach (var t in remotelist)
                    DeleteRelativeFile(ftpdir + "/" + t);
            }
        }

        private List<string> DetailRelativeFileDirList(string relativeFolder)
        {
            return DetailAbsoluteFileDirList(BaseUrl + relativeFolder);
        }

        private static List<string> DetailAbsoluteFileDirList(string folder)
        {
            var ftpRequest = (FtpWebRequest) WebRequest.Create(folder);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            var list = new List<string>();
            using (var reader = new StreamReader(ftpRequest.GetResponse().GetResponseStream())) {
                var line = reader.ReadLine();
                while (line != null) {
                    list.Add(line);
                    line = reader.ReadLine();
                }
            }
            return list;
        }

        #region based on Item

        /// <summary>
        /// 根据baseurl生成一条ftp链
        /// </summary>
        public FtpItem GetFtpLink()
        {
            var link = new FtpItem {
                //Name = "",
                AbsoluteUri = new Uri(BaseUrl),
                FtpItemType = FtpItemType.Folder,
            };
            link.FolderItems = GetFtpItems(link);
            GetAllChilds(link);
            return link;
        }

        private void GetAllChilds(FtpItem fpItem)
        {
            foreach (var item in fpItem.FolderItems) {
                if (item.FtpItemType == FtpItemType.Folder) {
                    item.FolderItems = GetFtpItems(item);
                    GetAllChilds(item);
                }
            }
        }

        public List<FtpItem> GetFtpItemsByRelativeUrl(string relativePath)
        {
            return GetFtpItemsByAbsolutelyUrl(BaseUrl + relativePath);
        }

        public List<FtpItem> GetFtpItems(FtpItem ftpItem)
        {
            if (ftpItem.FtpItemType == FtpItemType.Folder) {
                return GetFtpItemsByAbsolutelyUrl(ftpItem.AbsoluteUri.ToString());
            }
            throw new Exception("该方法只用于目录项");
        }

        public List<FtpItem> GetFtpItemsByAbsolutelyUrl(string preurl)
        {
            var reg2 = new Regex("\\s\\d+?\\s");
            var list = new List<FtpItem>();
            var ftpRequest = (FtpWebRequest) WebRequest.Create(preurl);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            using (var reader = new StreamReader(ftpRequest.GetResponse().GetResponseStream())) {
                string str;
                while ((str = reader.ReadLine()) != null) {
                    var item = new FtpItem {
                        ReviseTimestamp = str.Substring(0, 16),
                    };
                    if (str.IndexOf(DirTag) == -1) {
                        item.FtpItemType = FtpItemType.File;
                        var sizestr = reg2.Match(str).Groups[0].Value;
                        item.OriSize = long.Parse(sizestr);
                        item.Name = str.Substring(str.IndexOf(sizestr) + sizestr.Length);
                        item.AbsoluteUri = new Uri(preurl + item.Name);
                    }
                    else {
                        item.FtpItemType = FtpItemType.Folder;
                        item.Name = str.Substring(str.LastIndexOf(DirTag) + 5).TrimStart();
                        item.AbsoluteUri = new Uri(preurl + item.Name + "/");
                    }
                    list.Add(item);
                }
            }
            return list;
        }

        public void DeleteItem(FtpItem item)
        {
            switch (item.FtpItemType) {
                case FtpItemType.File:
                    DeleteAbsoluteFile(item.AbsoluteUri.ToString());
                    break;
                case FtpItemType.Folder:
                    DeleteAbsoluteDirectory(item.AbsoluteUri.ToString());
                    break;
                default:
                    throw new ArgumentException("未定义的枚举值");
            }
        }

        public static bool Exist(FtpItem item)
        {
            switch (item.FtpItemType) {
                case FtpItemType.File:
                    return FileExist(item.AbsoluteUri.ToString());
                case FtpItemType.Folder:
                    return FolderExist(item.AbsoluteUri.ToString());
            }
            throw new ArgumentException("未定义的枚举");
        }

        #endregion
    }
}
