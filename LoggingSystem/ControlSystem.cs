using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LoggingSystem
{
    public class ControlSystem
    {
        public static ControlSystem Current = new ControlSystem();
        public ObservableCollection<DataType_ShowInfo> ShowingData { get; protected set; }
        public StuInfo CurrentInfo { get; protected set; }
        public ControlSystem()
        {
            Current = this;
            ShowingData = new ObservableCollection<DataType_ShowInfo>();
            CurrentInfo = new StuInfo();
            CurrentInfo.PropertyChanged += CurrentInfoChanged;
        }
        protected void CurrentInfoChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyInfo propertyInfo;
            propertyInfo = typeof(StuInfo).GetProperty(e.PropertyName, BindingFlags.Instance | BindingFlags.Public);

            var Find_result = ShowingData.Where(m => { return m.Title == e.PropertyName; });
            switch (Find_result.Count())
            {
                case 0:
                    ShowingData.Add(new DataType_ShowInfo() { Content = propertyInfo.GetValue(sender, null).ToString(), Title = e.PropertyName });
                    break;
                case 1:
                    Find_result.ElementAt(0).Content = propertyInfo.GetValue(sender, null).ToString();
                    break;
                default:
                    for (int i = 0; i < Find_result.Count();) 
                    {
                        ShowingData.Remove(Find_result.ElementAt(i));
                    }
                    ShowingData.Add(new DataType_ShowInfo() { Content = propertyInfo.GetValue(sender, null).ToString(), Title = e.PropertyName });
                    break;
            }
        }
        public void ChangeBalance(double balance)
        {
            CurrentInfo.Balance_Double = balance;
        }
    }

    // property ends with '_N' doesn't notify changes
    public class StuInfo : INotifyPropertyChanged
    {
        public string Uname_N { get; set; }
        public string FullName_N { get; set; }

        public double balance_double;
        public double Balance_Double
        {
            get { return balance_double; }
            set { balance_double = value; Balance = balance_double.ToString()+"元"; }
        }

        private string username;
        public string Username
        {
            get { return username; }
            set { username = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Username))); }
        }
        
        private string service;
        public string Service
        {
            get { return service; }
            set { service = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Service))); }
        }

        private string area;
        public string Area
        {
            get { return area; }
            set { area = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Area))); }
        }

        private string balance;
        public string Balance
        {
            get { return balance; }
            set { balance = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Balance))); }
        }

        private string mac;
        public string MAC
        {
            get { return mac; }
            set { mac = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MAC))); }
        }

        private string ipv4;
        public string IPV4
        {
            get { return ipv4; }
            set { ipv4 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IPV4))); }
        }

        private string ipv6;
        public string IPV6
        {
            get { return ipv6; }
            set { ipv6 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IPV6))); }
        }

        private DateTime onlinetime;
        public DateTime OnlineTime
        {
            get { return onlinetime; }
            set { onlinetime = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OnlineTime))); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class DataType_ShowInfo : INotifyPropertyChanged
    {
        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title))); }
        }

        private string content;
        public string Content
        {
            get { return content; }
            set { content = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content))); }
        }

        private string url;
        public string Url
        {
            get { return url; }
            set { url = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Url))); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
