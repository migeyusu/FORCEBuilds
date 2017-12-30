using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using Castle.Core.Internal;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FORCEBuild.Concurrency;

namespace FORCEBuild.Helper
{
    public static class Extension
    {
        public static object Default(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                var valueProperty = type.GetProperty("Value");
                if (valueProperty != null) type = valueProperty.PropertyType;
            }
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static readonly BinaryFormatter Formatter = new BinaryFormatter();

        public static void Save<T>(this T instance, string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write)) {
                Formatter.Serialize(fs, instance);
            }
        }

        public static T Deserialize<T>(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return (T) Formatter.Deserialize(stream);
        }

        public static float Median<TSource>(this IEnumerable<TSource> source,Func<TSource, int> selector)
        {
            // Create a copy of the input, and sort the copy
            var temp = source.Select(selector).ToArray();
            var count = temp.Length;
            if (count == 0)
                throw new InvalidOperationException("Empty collection");
            Array.Sort(temp);
            if (count % 2 == 0)
            {
                // count is even, average two middle elements
                var a = temp[count / 2 - 1];
                var b = temp[count / 2];
                return (a + b) / 2f;
            }
            // count is odd, return the middle element
            return temp[count / 2];
        }

        public static float Median<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            // Create a copy of the input, and sort the copy
            var temp = source.Select(selector).ToArray();
            var count = temp.Length;
            if (count == 0)
                throw new InvalidOperationException("Empty collection");
            Array.Sort(temp);
            if (count % 2 == 0)
            {
                // count is even, average two middle elements
                var a = temp[count / 2 - 1];
                var b = temp[count / 2];
                return (a + b) / 2;
            }
            // count is odd, return the middle element
            return temp[count / 2];
        }

        public static double Median<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            // Create a copy of the input, and sort the copy
            var temp = source.Select(selector).ToArray();
            var count = temp.Length;
            if (count == 0)
                throw new InvalidOperationException("Empty collection");
            Array.Sort(temp);
            if (count % 2 == 0)
            {
                // count is even, average two middle elements
                var a = temp[count / 2 - 1];
                var b = temp[count / 2];
                return (a + b) / 2;
            }
            // count is odd, return the middle element
            return temp[count / 2];
        }

        public static bool IsEmpty<T>(this IEnumerable<T> iEnumerable)
        {
            return iEnumerable.IsNullOrEmpty();
        }

        public static Type GetEnumerableType(this Type type)
        {
            return (from intType in type.GetInterfaces() where intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IEnumerable<>) select intType.GetGenericArguments()[0]).FirstOrDefault();
        }

        public static bool IsGenericEnumerableType(this Type type)
        {
            return type.GetInterfaces().Any(intType => intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public static bool IsGenericEnumerable(this Type type)
        {
            return true;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> iEnumerable)
        {
            return new ObservableCollection<T>(iEnumerable);
        }

        public static ObservableConcurrentList<T> ToObservableConcurrentList<T>(this IEnumerable<T> iEnumerable)
        {
            return new ObservableConcurrentList<T>(iEnumerable);
        }
        
        public static IEnumerable<T> ToGenericEnumerable<T>(this IEnumerable enumerable)
        {
            return (from T x in enumerable where x != null select x);
        }

        /// <summary>
        /// 注册RPC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TK"></typeparam>
        /// <param name="container"></param>
        /// <param name="singleton"></param>
        /// <returns></returns>
        public static IWindsorContainer Register<T, TK>(this IWindsorContainer container, bool singleton = false)
            where T : class where TK : T
        {
            container.Register(singleton
                ? Component.For<T>().ImplementedBy<TK>().LifestyleSingleton()
                : Component.For<T>().ImplementedBy<TK>());
            return container;
        }

        public static ConcurrentDictionary<TKey, TElement> ToConcurrencyDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> sources,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return new ConcurrentDictionary<TKey, TElement>(sources.Select
            (source => new KeyValuePair<TKey, TElement>(keySelector(source),
                elementSelector(source))));

        }
            
        public static object ToSpecificCollection(this IEnumerable enumerable, Type collectionType)
        {
            var generictype = collectionType.GenericTypeArguments[0];
            var listType = typeof(List<>);
            listType = listType.MakeGenericType(generictype);
            var list = Convert.ChangeType(Activator.CreateInstance(listType), listType);
            var methodAdd = listType.GetMethod("Add", new[] { generictype });
            foreach (var en in enumerable)
            {
                methodAdd.Invoke(list, new[] { en });
            }
            var member = listType.InvokeMember("ToArray", BindingFlags.Default | BindingFlags.InvokeMethod,
                null, list, null);
            return Activator.CreateInstance(collectionType, member);
        }

        public static bool IsEmpty(this Guid uuid)
        {
            return uuid == Guid.Empty;
        }

        public static string ToCommaArray(this Array ary)
        {
            var stringBuilder = new StringBuilder();
            foreach (var x in ary)
            {
                stringBuilder.Append(x + ",");
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 是否符合windows文件名标准
        /// </summary>
        /// <param name="testName"></param>
        public static void CheckFileName(this string testName)
        {
            if (string.IsNullOrEmpty(testName))
                throw new ArgumentException("名称不能为空!");
            var badChars = new string(System.IO.Path.GetInvalidPathChars());
            var badCharsRegex = new Regex("[" + Regex.Escape(badChars) + "]");
            if (badCharsRegex.IsMatch(testName))
                throw new ArgumentException($"文件路径不能包含{badChars}等字符");
        }

    }
}