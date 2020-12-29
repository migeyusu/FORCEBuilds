using System.Timers;

namespace FORCEBuild.Net.DistributedService
{
    public class RealTimeInfoCounter
    {
        private readonly Timer timer;
        
        public RealTimeInfoCounter()
        {
            timer=new Timer(1200);
            timer.Elapsed += Timer_Elapsed;

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
        }

        public void End()
        {
            timer.Stop();
        }
        public void Start()
        {
            timer.Start();
        }
    }
}