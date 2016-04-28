using LoggingSystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class ShowNotice : Page
    {
        public static ShowNotice Current;
        private ObservableCollection<DataType_ShowInfo> mydata = new ObservableCollection<DataType_ShowInfo>();
        public ShowNotice()
        {
            this.InitializeComponent();
            Current = this;
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent += NoticeRefresh_ReturnDataEvent;
            PageRefresh();
        }
        ~ShowNotice()
        {
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent -= NoticeRefresh_ReturnDataEvent;
        }

        private void NoticeRefresh_ReturnDataEvent(Pages PageType, bool Hresult, ReturnData HArgs)
        {
            switch (PageType)
            {
                case Pages.GetNotice:
                    if (HArgs.ReturnCodeMeaning == ReturnDataCodeMeaning.Success)
                    {
                        mydata.Clear();
                        foreach (var m in HArgs.Notices)
                            mydata.Add(new DataType_ShowInfo() { Title = m.time, Content = m.title, Url = m.url });
                    }
                    else
                    {
                        mydata.Clear();
                        mydata.Add(new DataType_ShowInfo() { Title = HArgs.reply_msg, Content = "请尝试刷新或稍后再试", Url = NameManager.RefreshPageNote });
                    }
                    break;
                default: break;
            }
        }

        public async void PageRefresh()
        {
            if (mydata != null) mydata.Clear();
            //NoticeInfoPanel.Items.Clear();
            await LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.GetNotice);
            Debug.WriteLine("刷新了" + this.GetType().ToString() + "的内容");

            //登入成功时进行 之后尽量改成一个event事件
            var myAMF = new AdditionalMessageFetcher();
            string Uname = "anoymous";
            if (LoginControl.Current != null)
            {
                Uname = LoginControl.Current.Username;
            }
            bool Runresult = await myAMF.Run(Uname);
            if (Runresult)
            {
                mydata.Add(new DataType_ShowInfo() { Title = myAMF.result_Analysed.Title, Content = myAMF.result_Analysed.Content, Url = myAMF.result_Analysed.url });
            }
        }
        
        private async void Notice_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var t = sender as Panel;
            string uriToLaunch = t.Tag as string;
            if (uriToLaunch == null)
            {
                Debug.WriteLine("这个通知没有对应的链接");
            }
            else if (uriToLaunch == NameManager.RefreshPageNote)
            {
                PageRefresh();
            }
            else
            {
                var uri = new Uri(uriToLaunch);
                await Windows.System.Launcher.LaunchUriAsync(uri);
                Debug.WriteLine("打开了通知的链接:" + uriToLaunch);
            }
        }

        private void RefreshPage(object sender, TappedRoutedEventArgs e)
        {
            PageRefresh();
        }
    }
}
