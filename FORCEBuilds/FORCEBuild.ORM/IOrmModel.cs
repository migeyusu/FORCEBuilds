namespace FORCEBuild.ORM
{
    public interface IOrmModel
    {
        /// <summary>
        /// 类定义，同cache内命名一致
        /// </summary>
         ClassDefine ClassDefine { get; set; }
        /// <summary>
        /// 标记为已入列的“脏”数据
        /// </summary>
         bool IsInTask { get; set; }

         ModelStatus ModelStatus { get; set; }

         OrmInterceptor OrmInterceptor { get; set; }
    }
}