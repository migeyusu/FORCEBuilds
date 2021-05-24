namespace FORCEBuild.Data.ManualBinding.Abstraction
{
    /// <summary>
    ///  期望的实例类型
    /// </summary>
    /// <typeparam name="T">class type</typeparam>
    /// <typeparam name="TK">property type</typeparam>
    public interface IPropertySet<in T, in TK>
    {
        /// <summary>
        /// 接收一个实例类型
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void Set(T x, TK y);
    }
}