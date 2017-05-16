using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace NJULoginTest
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ShowAbout : Page
    {

        public ShowAbout()
        {
            this.InitializeComponent();
            PageRefresh();
        }

        public void PageRefresh()
        {
            Windows.ApplicationModel.Package ThisPackage = Windows.ApplicationModel.Package.Current;
            Windows.ApplicationModel.PackageId ThisId = ThisPackage.Id;
            Windows.ApplicationModel.PackageVersion ThisVersion = ThisId.Version;
            NameOfApp.Text = ThisPackage.DisplayName;
            VersionOfApp.Text = String.Format("版本: {0}.{1}.{2}.{3} [{4}]",
                ThisVersion.Major, ThisVersion.Minor, ThisVersion.Build, ThisVersion.Revision,
                ThisId.Architecture);
            InstalledDateOfApp.Text = Windows.ApplicationModel.Package.Current.InstalledDate.ToString("安装日期: yyyy-MM-dd");
            Debug.WriteLine("刷新了" + this.GetType().ToString() + "的内容");
        }

        private void PageRefresh(object sender, TappedRoutedEventArgs e)
        {
            PageRefresh();
        }

        private async void OpenStore(object sender, TappedRoutedEventArgs e)
        {
            var uri = new Uri("ms-windows-store://pdp/?ProductId=9NBLGGH5JDWG");
            await Windows.System.Launcher.LaunchUriAsync(uri);
            Debug.WriteLine("打开了商店页面");
        }

        private async void OpenReview(object sender, TappedRoutedEventArgs e)
        {
            var uri = new Uri("ms-windows-store://review/?ProductId=9NBLGGH5JDWG");
            await Windows.System.Launcher.LaunchUriAsync(uri);
            Debug.WriteLine("打开了商店页面");
        }

        private async void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var uri = new Uri("https://github.com/BeanLiu1994/NJU_Login/wiki/privacy_policy");
            await Windows.System.Launcher.LaunchUriAsync(uri);
            Debug.WriteLine("打开了商店页面");
        }
    }
}
