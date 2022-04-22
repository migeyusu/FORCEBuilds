using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
using FORCEBuild.Crosscutting;
using FORCEBuild.Net.NamedPipe;
using FORCEBuild.Net.RPC;
using TestProject;

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
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var proxyServiceFactory =
                    new ProxyServiceFactory(new NamedPipeMessageClient("testPi", TimeSpan.FromMilliseconds(1000d),
                        new BinaryFormatter()));
                var testRpc = proxyServiceFactory.CreateService<ITestRPC>();
                var add = testRpc.Add(1, 1);
                MessageBox.Show(add.ToString());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }
    }
}