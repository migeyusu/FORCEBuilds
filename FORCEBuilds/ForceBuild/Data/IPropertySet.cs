namespace FORCEBuild.Data
{
    /// <summary>
    ///  期望的实例类型
    /// </summary>
    /// <typeparam name="K">class type</typeparam>
    /// <typeparam name="KS">property type</typeparam>
    public interface IPropertySet<K, KS>
    {
        /// <summary>
        /// 接收一个实例类型
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void Set(K x, KS y);
    }
}