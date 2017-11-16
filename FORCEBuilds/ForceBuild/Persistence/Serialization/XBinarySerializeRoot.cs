namespace FORCEBuild.Persistence.Serialization
{
    public class XBinarySerializeRoot
    {
        //public Type ClassType { get; set; }
        public XBinarySerializePair[] Pairs { get; set; }
        public XBinarySerializeRoot Clone()
        {
            var sps = new XBinarySerializeRoot {
                Pairs = new XBinarySerializePair[this.Pairs.Length]
            };//{ ClassType = this.ClassType };
            for (var i = 0; i < Pairs.Length; i++)
            {
                sps.Pairs[i] = this.Pairs[i].Clone();
            }
            return sps;
        }
    }
}