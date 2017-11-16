using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FORCEBuild.Crosscutting;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Message;
using FORCEBuild.Net.Base;

namespace FORCEBuild.DistributedStorage.Cache
{

    public class L1Cache //:IStoreage
    {
        public const string Tag = "CMD1";

        public const string Tag1 = "CMD2";

        public IPEndPoint MESIEndPoint { get; set; }

        public ILog Log { get; set; }

        private MessageBusClient messageBusClient;

        /// <summary>
        /// 类型、ormid和缓存单元
        /// </summary>
        public ConcurrentDictionary<int, CacheCell> LocalDatas { get; set; }

        public L1Cache()
        {
            messageBusClient = new MessageBusClient();
            LocalDatas = new ConcurrentDictionary<int, CacheCell>(2048, 2048);
        }

        private bool work, working;

        /// <summary>
        /// tcp形式接收其他缓存端过来的信息
        /// </summary>
        public void Start()
        {
            if (working)
                return;
            work = true;
            //检查消息总线
            MESIEndPoint = null; //messageBusClient.KnowledgeTopic(L2Cache.MESI) as IPEndPoint;
            if (MESIEndPoint == null) {
                throw new NullReferenceException("一级缓存指令终结点为空");
            }
            Task.Run(() => { MESIListen(); });
        }

        public void End()
        {
            if (working)
                work = false;
        }

        /// <summary>
        /// 监听mesi协议，一条指令只有几个字节，只需一个通道接收
        /// 后期添加单指令多数据类型
        /// </summary>
        private void MESIListen()
        {
            working = true;
            TcpClient client = null;
            try {
                client = new TcpClient();
                client.Connect(MESIEndPoint);
                while (work) {
                    var broadcast = client.Client.GetStruct<OperationBroadcast>();
                    if (broadcast.IsCorect) {
                        LocalDatas.TryGetValue(broadcast.OperationKey, out CacheCell value);

                        //remote read/write不影响invalid
                        value?.Post(new Operation() {
                            OperationKey = broadcast.OperationKey,
                            OperationType = broadcast.Operation
                        });
                    }
                }
            }
            catch (Exception e) {
                Log.Write(e);
            }
            finally {
                client?.Close();
                working = false;
            }
        }

        //public void Enqueue(List<TransactionMember> producers)
        //{


        //}

        //public void Enqueue(TransactionMember producer)
        //{


        //}

        //public void Enqueue(ITranscationProducer producer)
        //{



        //}

        public void Save(IDistributedData data)
        {
            CacheCell cell;
            if (!LocalDatas.ContainsKey(data.SyncKey)) {
                cell = new CacheCell() {PreStatus = MESIStatus.Invalid};
                LocalDatas.TryAdd(data.SyncKey,cell);
            }
            //cell.Post(new Operation() {
                
            //});
        }

        public void Delete(IDistributedData data)
        {

        }

        public void Update(IDistributedData data)
        {

        }

        public T Get<T>(int key)
        {
            if (working) {

            }

            return default(T);
        }

        public dynamic Get(int ormid, string property)
        {


            return null;
        }

        
    }
}