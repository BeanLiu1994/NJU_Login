using LoggingSystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
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
    public sealed partial class ShowInfo : Page, SubPageInterface
    {
        public static ShowInfo Current;
        private ObservableCollection<DataType_ShowInfo> mydata;
        public ShowInfo()
        {
            this.InitializeComponent();
            Current = this;
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent += InfoRefresh_ReturnDataEvent;
            mydata = ControlSystem.Current.ShowingData;
        }
        ~ShowInfo()
        {
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent -= InfoRefresh_ReturnDataEvent;
        }

        private void InfoRefresh_ReturnDataEvent(Pages PageType, bool Hresult, ReturnData HArgs)
        {
            switch (PageType)
            {
                case Pages.GetInfo:
                case Pages.LoginPage:
                    mydata.Clear();
                    switch (HArgs.ReturnCodeMeaning)
                    {
                        case ReturnDataCodeMeaning.Fail:
                            mydata.Add(new DataType_ShowInfo() { Title = HArgs.reply_msg, Content = "请尝试登陆后再试", Url = NameManager.RefreshPageNote });
                            break;
                        case ReturnDataCodeMeaning.NoNetWork:
                            mydata.Add(new DataType_ShowInfo() { Title = HArgs.reply_msg, Content = "请尝试刷新或稍后再试", Url = NameManager.RefreshPageNote });
                            break;
                        case ReturnDataCodeMeaning.Success:
                            AddItem("用户名", HArgs.fullname + "(" + HArgs.username + ")");
                            AddItem("服务名称", HArgs.service_name);
                            AddItem("登陆区域", HArgs.area_name);
                            AddItem("余额", HArgs.balance.ToString() + "元");
                            AddItem("MAC地址", HArgs.mac);
                            AddItem("IPV4", HArgs.useripv4);
                            AddItem("IPV6", HArgs.useripv6);
                            break;
                    }
                    break;
                case Pages.GetVolume:
                    if (HArgs.ReturnCodeMeaning == ReturnDataCodeMeaning.Success)
                    {
                        AddItem("在线时间", HArgs.onlineTime);
                    }
                    else
                    {
                        AddItem("在线时间", "未知");
                    }
                    break;
                case Pages.LogoutPage:
                    if (HArgs.ReturnCodeMeaning == ReturnDataCodeMeaning.Success)
                    {
                        mydata.Clear();
                        mydata.Add(new DataType_ShowInfo() { Title = HArgs.reply_msg, Content = "您已登出....", Url = NameManager.RefreshPageNote });
                    }
                    break;
                default: break;
            }
        }
        private void AddItem(string title, string content)
        {
            mydata.Add(new DataType_ShowInfo() { Title = title, Content = content, Url = content });
        }

        private void PageLoadEnd(object sender, RoutedEventArgs e)
        {
            PageRefresh();
        }
        public async void PageRefresh()
        {
            await LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.GetInfo);
        }
        
        private void AddToPasteBoard(object sender, RoutedEventArgs e)
        {
            var panel = sender as Panel;
            string tag = panel.Tag as string;
            if (tag == NameManager.RefreshPageNote)
            {
                PageRefresh();
            }
            else
            {
                DataPackage pv = new DataPackage();
                pv.SetText(tag);
                Clipboard.SetContent(pv);
                Debug.WriteLine("复制了信息内容:" + tag);
            }
        }

    }
}
