using System;

namespace FORCEBuild.Persistence
{
    /* 2017.2:
    * 以INotify接口为核心，更新交由更新线程自动化完成
    * （由于只负责读取值同步到数据库，不需要额外的锁）
    * 2017.3
    * 区分读写方式：更改操作会发射到一个队列异步完成，或在接下来的读请求执行之前完成
    * 对象的更新记录会在它的notifyproperties里标记，同时发射到调度器，
    * 对同一个对象的多次变化，调度器只应该标记到最后一次变化
    * 为了尽可能提高性能，使用actor模型；
    * 依据.net的不同版本，对消息并发的资源竞争方式：
    * 2.0-4.0： 原子标记/自旋锁/系统锁
    * 4.5+：并发集合/直接消费者生产者模型
    * 调度器使用了actor模型：对象拥有一个线程安全的邮箱，接受外部消息的投递，
    * 然后对象内部不断地从邮箱取消息执行
    * dispatcher更改为unit of work
     2019.8 dispatcher 提供队列执行模式，即完全顺序化执行，以提高并发性能
     而具体的执行迁移到Field实施
    */

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    internal class AccessorDispatcher
    {
      
        internal AccessorDispatcher()
        {
           
        }


      
    }
}