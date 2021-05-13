using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FORCEBuild.Concurrency;

namespace WpfAppTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var fromCurrentSynchronizationContext = TaskScheduler.FromCurrentSynchronizationContext();
            var actionActor = new ActionActor(fromCurrentSynchronizationContext);
            for (var i = 0; i < 10; i++)
            {
                Task.Run((async () =>
                {
                    await actionActor.PostAsync((() =>
                    {
                        Debug.WriteLine(Thread.CurrentThread.ManagedThreadId);
                    }));
                    actionActor.Post((() =>
                    {
                        Debug.WriteLine(Thread.CurrentThread.ManagedThreadId);
                    }));
                }));

            }
        }
    }

    public class ActionActor:TaskBasedActor<Action>
    {
        public ActionActor(TaskScheduler scheduler) : base(scheduler)
        {
            
        }

        protected override void ReceiveMessage(Action message)
        {
            message.Invoke();
        }
    }
}
