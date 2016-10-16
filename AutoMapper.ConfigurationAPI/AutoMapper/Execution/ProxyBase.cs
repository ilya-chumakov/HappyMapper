using System.ComponentModel;

namespace HappyMapper.AutoMapper.ConfigurationAPI.Execution
{
    public abstract class ProxyBase
    {
        public ProxyBase()
        {
            
        }

        protected void NotifyPropertyChanged(PropertyChangedEventHandler handler, string method)
        {
            handler?.Invoke(this, new PropertyChangedEventArgs(method));
        }
    }
}