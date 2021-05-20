namespace FORCEBuild.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">class type</typeparam>
    /// <typeparam name="TK">property type</typeparam>
    public interface IPropertyGet<T, TK>
    {
        /// <summary>
        /// 广播一个类型的实例
        /// </summary>
        /// <param name="t"></param>
        TK Get(T t);
    }
}