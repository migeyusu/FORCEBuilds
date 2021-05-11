using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using FORCEBuild.Helper;


namespace FORCEBuild.Concurrency
{
    
    /// <summary>
    /// 资源管理器，以名称区分，在指定文件夹存放
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResourcesManager<T> : IDisposable,IDictionary<string,T> where T : class
    {
        public string Folder { get; protected set; }
        
        public List<string> Filters { get; protected set; }

        protected bool IsLazy;

        private bool _isloaded;

        protected readonly ConcurrentDictionary<string, T> _resourcesDictionary;

        protected Func<Stream, T> GetInstance;

        protected Action<T, Stream> WriteToStream;

        public IEnumerable<T> AllResources => _resourcesDictionary.Select(pair => pair.Value);

        /// <summary>
        /// 已取出的对象集合
        /// </summary>
        public ObservableCollection<T> Collection { get; set; }

        /// <summary>
        /// 对象键集合
        /// </summary>
        public ObservableCollection<string> NamesCollection { get; set; }
        
        protected ResourcesManager()
        {
            _resourcesDictionary = new ConcurrentDictionary<string, T>();
            Collection = new ObservableCollection<T>();
            NamesCollection = new ObservableCollection<string>();
            Filters = new List<string>(3);
        }

        public virtual bool ContainsName(string name)
        {
            return _resourcesDictionary.ContainsKey(name);
        }

        /// <summary>
        /// 更改名称资源
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public virtual void ChangeName(string oldName, string newName)
        {
            _resourcesDictionary.TryRemove(oldName, out T value);
            var oldPath = Folder + "\\" + oldName;
            var newPath = Folder + "\\" + newName;
            _resourcesDictionary.AddOrUpdate(newName, s => value, (s, arg2) => {
                Collection.Remove(arg2);
                if (File.Exists(newPath))
                    File.Delete(newPath);
                NamesCollection.Remove(oldName);
                return value;
            });
            if (value != null && File.Exists(oldPath)) {
                File.Move(oldPath, newPath);
            }
            NamesCollection.Add(newName);
        }

        /// <summary>
        /// 根据指定的文件夹和后缀筛选文件
        /// </summary>
        /// <param name="folder">选定扫描的文件夹,绝对路径</param>
        /// <param name="lazy">true:延迟加载</param>
        public virtual void Initialize(string folder, bool lazy = true)
        {
            if (_isloaded)
                return;
            if (this.GetInstance == null || this.WriteToStream == null) {
                throw new ArgumentNullException($"参数{nameof(WriteToStream)}或${nameof(GetInstance)}为空");
            }
            if (!Directory.Exists(folder)) {
                throw new ArgumentNullException("无效的文件夹路径");
            }
            Folder = folder;
            var files = Directory.GetFiles(folder);
            foreach (var path in files) {
                if (Filters.Any(filter => path.EndsWith(filter))) {
                    var start = path.LastIndexOf("\\", StringComparison.Ordinal) + 1;
                    var safeName = path.Substring(start);
                    var fileInfo = new FileInfo(path);
                    //不读取空文件
                    if (fileInfo.Length == 0)
                        continue;
                    if (lazy) {
                        _resourcesDictionary.TryAdd(safeName, null);
                    }
                    else {
                        using (var file = new FileStream(path, FileMode.Open)) {
                            var value = GetInstance(file);
                            _resourcesDictionary.TryAdd(safeName, value);
                            Collection.Add(value);
                        }
                    }
                    NamesCollection.Add(safeName);
                }
            }
            _isloaded = true;
        }

        /// <summary>
        /// 根据名称获取资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns>如果资源不存在则返回空值</returns>
        public virtual T Get(string name)
        {
            if (!_resourcesDictionary.ContainsKey(name))
                return null;
            //throw new ArgumentException("当前资源中不包含该文件");
            var path = Folder + "\\" + name;
            //if (!File.Exists(path))
            //    throw new ArgumentException("文件系统中不存在该文件");
            var result = _resourcesDictionary[name];
            if (result != null)
                return result;
            using (var file = new FileStream(path, FileMode.Open)) {
                if (file.Length == 0) {
                    return null;
                }
                result = GetInstance(file);
            }
            _resourcesDictionary.TryUpdate(name, result, null);
            Collection.Add(result);
            return result;
        }

        /// <summary>
        /// 如果不存在则添加，如果存在则更新对象
        /// </summary>
        /// <param name="x"></param>
        /// <param name="name"></param>
        public virtual void AddOrUpdate(T x, string name)
        {
            if (_resourcesDictionary.ContainsKey(name)) {
                var oldvalue = _resourcesDictionary[name];
                _resourcesDictionary.TryUpdate(name, x, oldvalue);
                Collection.Remove(oldvalue);
                Collection.Add(x);
            }
            else {
                name.CheckFileName();
                _resourcesDictionary.TryAdd(name, x);
                Collection.Add(x);
                NamesCollection.Add(name);
            }
            var newPath = Folder + "\\" + name;
            try {
                using (var file = new FileStream(newPath, FileMode.OpenOrCreate))
                    WriteToStream(x, file);
            }
            catch (Exception) {
                if (File.Exists(newPath))
                    File.Delete(newPath);
                throw;
            }
        }

        /// <summary>
        /// 更新资源到本地
        /// </summary>
        public virtual void FlushResource(string name)
        {
            if (_resourcesDictionary.ContainsKey(name)) {
                if (_resourcesDictionary[name] == null) {
                    throw new NotSupportedException("找不到要更新的文件");
                }
                var path = Folder + "\\" + name;
                try {
                    using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
                        WriteToStream(_resourcesDictionary[name], file);
                }
                catch (Exception) {
                    if (File.Exists(path))
                        File.Delete(path);
                    throw;
                }

            }
        }

        public bool ContainsKey(string key)
        {
            return _resourcesDictionary.ContainsKey(key);
        }

        /// <summary>
        /// 同AddOrUpdate作用相同
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, T value)
        {
            AddOrUpdate(value, key);
        }

        bool IDictionary<string, T>.Remove(string key)
        {
            try {
                Remove(key);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public bool TryGetValue(string key, out T value)
        {
            return _resourcesDictionary.TryGetValue(key, out value);
        }

        public T this[string key] {
            get => _resourcesDictionary.TryGetValue(key, out var value) ? value : default(T);
            set => AddOrUpdate(value,key);
        }

        public ICollection<string> Keys => _resourcesDictionary.Keys;

        public ICollection<T> Values => _resourcesDictionary.Values;

        public virtual void Remove(string name)
        {
            if (_resourcesDictionary.ContainsKey(name)) {
                var path = Folder + "\\" + name;
                if (File.Exists(path))
                    File.Delete(path);
                _resourcesDictionary.TryRemove(name, out T value);
                Collection.Remove(value);
                NamesCollection.Remove(name);
            }
            else {
                throw new ArgumentException("当前资源中不包含该文件");
            }
        }

        public virtual void Remove(T x)
        {
            var index = Collection.IndexOf(x);
            if (index > -1) {
                var name = NamesCollection[index];
                Remove(name);
            }
        }

        public virtual void RemoveAt(int index)
        {
            if (index < NamesCollection.Count && index > -1) {
                var name = NamesCollection[index];
                Remove(name);
            }
        }

        //todo 如果覆写流时出错，需要将之前加载到内存的文件重置入
        /// <summary>
        /// 由于值都为引用类型，所以update特指替换值,
        /// 如果不存在则替换
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key">键</param>
        public virtual void AddFile(string path, string key)
        {
            T newValue;
            using (var file = new FileStream(path, FileMode.Open)) {
                newValue = GetInstance(file);
            }
            File.Copy(path, Folder + "\\" + key, true);
            _resourcesDictionary.AddOrUpdate(key, s => {
                Collection.Add(newValue);
                NamesCollection.Add(key);
                return newValue;
            }, (s, arg2) => {
                Collection.Remove(arg2);
                Collection.Add(newValue);
                return newValue;
            });
            //AddOrUpdate(newValue,safename); 
        }

        public virtual void AddFileWithoutLoad(string path, string safename)
        {
            File.Copy(path, Folder + "\\" + safename, true);
            _resourcesDictionary.AddOrUpdate(safename, s => {
                NamesCollection.Add(safename);
                return null;
            }, (s, arg2) => {
                Collection.Remove(arg2);
                return null;
            });
        }
        

        public void Dispose()
        {
            var files = Directory.GetFiles(Folder)
                .Where(s => Filters.Any(s.EndsWith))
                .Select(s => s.Substring(s.LastIndexOf('\\') + 1));
            //同步到硬盘，增加硬盘没有的文件，不执行重写
            foreach (var key in _resourcesDictionary.Keys) {
                if (!files.Contains(key)) {
                    using (var file = new FileStream(Folder + "\\" + key, FileMode.Create)) {
                        WriteToStream(_resourcesDictionary[key], file);
                    } 
                }
            }
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return _resourcesDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, T> item)
        {
            AddOrUpdate(item.Value,item.Key);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return ContainsName(item.Key);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            try {
                Remove(item.Key);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public int Count => _resourcesDictionary.Count;

        public bool IsReadOnly => false;
    }

}