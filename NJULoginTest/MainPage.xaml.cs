using System;
using System.Collections.Generic;
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
using LoggingSystem;
using System.Collections.ObjectModel;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace NJULoginTest
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            InfoPage.Navigate(typeof(ShowInfo));
            LoginPage.Navigate(typeof(ShowLogin));
            LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.GetNotice);
        }

        public void ChangeTheme()
        {
            switch (RequestedTheme)
            {
                case ElementTheme.Dark:
                    this.RequestedTheme = ElementTheme.Light;
                    break;
                case ElementTheme.Default:
                case ElementTheme.Light:
                    this.RequestedTheme = ElementTheme.Dark;
                    break;
            }
        }

        private async void LoadedPage(object sender, RoutedEventArgs e)
        {
            PictureInfo mypicinfo = new PictureInfo();
            var result = await mypicinfo.RunSession();
            PicBkg.InputPicInfo = result;
        }
    }
}
