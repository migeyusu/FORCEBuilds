namespace FORCEBuild.Persistence
{
    public class OrmMix:IOrmModel
    {
        /// <summary>
        /// 类定义，同cache内命名一致
        /// </summary>
        public ClassDefine ClassDefine { get; set; }
        /// <summary>
        /// 标记为已入列的“脏”数据
        /// </summary>
        public bool IsInTask { get; set; }

        public ModelStatus ModelStatus { get; set; }

        public OrmInterceptor OrmInterceptor { get; set; }

        public OrmMix(OrmInterceptor interceptor,ClassDefine classDefine)
        {
            this.ClassDefine = classDefine;
            this.OrmInterceptor = interceptor;
            ModelStatus = ModelStatus.Default;
        }
    }
}