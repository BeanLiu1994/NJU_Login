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
using Windows.Networking.Connectivity;
using LoggingSystem;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace NJULoginTest
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DataType_ShowInfo PicInfoShowing = null;
        public static MainPage Current;

        public MainPage()
        {
            Current = this;
            this.InitializeComponent();
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent += SystemControl_ReturnDataEvent;
            InfoPage.Navigate(typeof(ShowInfo));
            LoginPage.Navigate(typeof(ShowLogin));
            AboutPage.Navigate(typeof(ShowAbout));
            NoticePage.Navigate(typeof(ShowNotice));
            EasterPage.Navigate(typeof(EasterEggPage));
            SettingPage.Navigate(typeof(ShowSettings));
            //BillPage.Navigate(typeof(ShowBill));
        }
        ~MainPage()
        {
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent -= SystemControl_ReturnDataEvent;
        }

        private async void SystemControl_ReturnDataEvent(Pages PageType, bool Hresult, ReturnData HArgs)
        {
            if (PageType == Pages.LoginPage || PageType == Pages.GetInfo)
                if (HArgs.ReturnCodeMeaning == ReturnDataCodeMeaning.Success)
                    if(PicInfoShowing == null)
                        await RefreshPic();
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

        private TimeChecker myChecker30Min = new TimeChecker(new TimeSpan(0, 30, 0));
        public async Task RefreshPic()
        {
            if (!myChecker30Min.Check_ReadOnly()) return;
            if (NetworkCheck.IsWwanConnectionNow()) return;
            var mypicinfo = new PictureInfo();
            PicInfoShowing = await mypicinfo.RunSession();
            if (PicInfoShowing != null)
            {
                PicBkg.InputPicInfo = PicInfoShowing;
                myChecker30Min.Check();
            }
        }

        private async void LoadedPage(object sender, RoutedEventArgs e)
        {
            await RefreshPic();
        }

        private void FourthPivotShowBkg(Pivot sender, PivotItemEventArgs args)
        {
            if(args.Item.Tag as string == "BackGround")
            {
                PicBkg.ShowInfo();
                PivotPage.Tag = Visibility.Collapsed;
            }
            else
            {
                PicBkg.HideInfo();
                PivotPage.Tag = Visibility.Visible;
            }
        }
    }
}
