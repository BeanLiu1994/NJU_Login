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
    public sealed partial class WindowBL : UserControl,INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string title = "";

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }

        
        public object InnerContent
        {
            get { return InnerView.Content; }
            set { InnerView.Content=value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InnerContent)));
            }
        }

        public WindowBL()
        {
            this.InitializeComponent();
            iswindowcollapsed = Visibility.Collapsed;
        }

        private Visibility iswindowcollapsed;
        public Visibility IsWindowCollapsed
        {
            get { return iswindowcollapsed; }
            set { iswindowcollapsed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsWindowCollapsed)));
            }
        }
        private Visibility iswindowvisible;
        public Visibility IsWindowVisible
        {
            get
            {
                return iswindowvisible;
            }
            set
            {
                iswindowvisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsWindowVisible)));
                if (iswindowvisible == Visibility.Visible)
                {
                    IsWindowCollapsed = Visibility.Collapsed;
                }
                else
                {
                    IsWindowCollapsed = Visibility.Visible;
                }
            }
        }
        
        private void shrink_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsWindowVisible == Visibility.Collapsed)
            {
                IsWindowVisible = Visibility.Visible;
            }
            else
            {
                IsWindowVisible = Visibility.Collapsed;
            }
        }
    }
}
