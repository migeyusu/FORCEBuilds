using System;

namespace FORCEBuild.IO.Compression
{
    public class ProgressArgs:EventArgs
    {
        public long Total { get; set; }
        public long Position { get; set; }
        public long PreFileTotal { get; set; }
        public long PreFileProgress { get; set; }
        public string PreFileName { get; set; }
        public string Description { get; set; }
    }
}
