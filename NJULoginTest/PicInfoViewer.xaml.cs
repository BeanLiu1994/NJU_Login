using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NJULoginTest
{
    public sealed partial class PicInfoViewer : UserControl, INotifyPropertyChanged
    {
        public PicInfoViewer()
        {
            this.InitializeComponent();
            InputPicInfo = new DataType_ShowInfo() { Content="",Title = "欢迎使用", Url = "/Asets/SplashScrenn.scale-200.png" };
        }
        
        public void ShowInfo()
        {
            VisualStateManager.GoToState(this, "W860", true);
        }
        public void HideInfo()
        {
            VisualStateManager.GoToState(this, "W0", true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private DataType_ShowInfo inputpicinfo;
        public DataType_ShowInfo InputPicInfo
        {
            get { return inputpicinfo; }
            set
            {
                inputpicinfo = value;
                PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(InputPicInfo)));
            }
        }

        private void ShowInfo(object sender, TappedRoutedEventArgs e)
        {
            ShowInfo();
        }
    }
}
