using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FORCEBuild;
using FORCEBuild.Concurrency;

namespace WpfAppTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SynchronizationHelper.Initialize();
            /*var fromUiSynchronizationContext = SynchronizationHelper.FromUISynchronizationContext();
            var defaultTaskBasedActor = new DefaultTaskBasedActor<string>(fromUiSynchronizationContext,i => this.ListBox.Items.Add(i));
            await defaultTaskBasedActor.PostAsync("asdf");*/
            var thread = new Thread((async o =>
            {
                Thread.Sleep(1000);
                // defaultTaskBasedActor.Post("asdfasfd");
                await SynchronizationHelper.UiContext;
                ListBox.Items.Add("asdg");
            }));
            thread.Start();
            
        }   
    }
}
