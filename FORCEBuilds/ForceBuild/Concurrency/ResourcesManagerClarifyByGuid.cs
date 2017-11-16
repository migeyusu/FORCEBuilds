using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FORCEBuild.Concurrency
{
    /// <summary>
    /// 资源管理器，以统一标识符区分，在指定文件夹存放
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResourcesManagerClarifyByGuid<T>
    {

        protected string Folder;

        protected string Filter;

        private bool isloaded;

        private static ResourcesManagerClarifyByGuid<T> _subjectResourceManager;

        protected readonly ConcurrentDictionary<Guid, T> _resourcesDictionary;

        protected Func<Stream, T> GetInstance;

        protected Action<T, Stream> GetStream;

        protected ResourcesManagerClarifyByGuid()
        {
            _resourcesDictionary=new ConcurrentDictionary<Guid, T>();
        }  

        public static ResourcesManagerClarifyByGuid<T> Current => _subjectResourceManager ??
                                                        (_subjectResourceManager =
                                    new ResourcesManagerClarifyByGuid<T>());

        /// <summary>
        /// 根据指定的文件夹和后缀筛选文件
        /// </summary>
        /// <param name="folder">选定扫描的文件夹</param>
        public void Initialize(string folder)
        {
            if (isloaded)
                return;
            Folder = folder;
            if (this.GetInstance == null || this.GetStream == null)
            {
                throw new ArgumentNullException($"参数{nameof(GetStream)}或${nameof(GetInstance)}为空");
            }
            var list = Directory.GetFiles(folder);
            foreach (var str in list)
            {
                if (str.EndsWith(Filter))
                {
                    var start = str.LastIndexOf("\\", StringComparison.Ordinal) + 1;
                    var end = str.LastIndexOf(Filter, StringComparison.Ordinal);
                    var result = str.Substring(start, end - start);
                    if (Guid.TryParse(result, out Guid guid))
                        _resourcesDictionary.TryAdd(guid, default(T));
                }
            }
            isloaded = true;
        }

        public T Get(Guid guid)
        {
            if (!_resourcesDictionary.ContainsKey(guid))
            {
                return default(T);
            }
            var path = Folder + "\\" + guid + Filter;
            if (!File.Exists(path))
            {
                return default(T);
            }
            using (var file=new FileStream(path,FileMode.Open))
            {
                var t = GetInstance(file);
                _resourcesDictionary.TryUpdate(guid, t, default(T));
                return t;
            }
        }

        public void Add(T x)
        {
            var guid = Guid.NewGuid();
            using (var file=new FileStream(Folder + "\\" + guid + Filter,FileMode.Create))
            {
                GetStream(x,file);
                _resourcesDictionary.TryAdd(guid, x);
            }
        }

        public void Remove(Guid guid)
        {
            if (_resourcesDictionary.ContainsKey(guid))
            {
                var path = Folder + "\\" + guid + Filter;
                if (File.Exists(path))
                {
                    File.Delete(Filter);
                }
                _resourcesDictionary.TryRemove(guid, out T _);
            }
        }
        /// <summary>
        /// 如果存在则更新，不存在则创建
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="x"></param>
        public void Update(Guid guid, T x)
        {
            //if (_resourcesDictionary.ContainsKey(guid))
            //{

            //}

            using (var file = new FileStream(Folder + "\\" + guid.ToString() + Filter, FileMode.OpenOrCreate))
            {
                GetStream(x, file);
                _resourcesDictionary.AddOrUpdate(guid, x, ((guid1, arg2) => x));
            }
        }

        public IEnumerable<T> AllResources => _resourcesDictionary.Select(pair => pair.Value);
    }
}