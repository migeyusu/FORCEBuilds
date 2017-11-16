using System.ComponentModel;

namespace FORCEBuild.Crosscutting
{
    public class NotifyBase:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(object obj,string propertyName)
        {
            PropertyChanged?.Invoke(obj, new PropertyChangedEventArgs(propertyName));
            //SynchronizationHelper.InvokeAsync(o => {
                
            //},null);
        }
    }
}