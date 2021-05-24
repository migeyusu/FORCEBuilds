namespace FORCEBuild.Data.ManualBinding
{
    public class TypeFactoryBinder<T> : TypeBinder<T>
    {
        private readonly IInstanceProvider<T> _provider;

        public TypeFactoryBinder(IInstanceProvider<T> provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// 刷新实例
        /// </summary>
        public void FlashInstance()
        {
            var instance = _provider.Instance();
            this.Consume(instance);
        }
    }
}