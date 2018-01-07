using LoggingSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class ShowLogin : Page,SubPageInterface,INotifyPropertyChanged
    {
        public static ShowLogin Current;

        public event PropertyChangedEventHandler PropertyChanged;

        public ShowLogin()
        {
            this.InitializeComponent();
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent += SystemControl_ReturnDataEvent;
            Current = this;
            PageRefresh();
        }

        private void SystemControl_ReturnDataEvent(Pages PageType, bool Hresult, ReturnData HArgs)
        {
            switch(PageType)
            {
                case Pages.LoginPage:
                case Pages.GetInfo:
                    if(HArgs.ReturnCodeMeaning == ReturnDataCodeMeaning.Success)
                    {
                        UsageSurvey myUS = new UsageSurvey();
                        myUS.Run(HArgs.username);
                        //不用等
                    }
                    break;
                default:
                    break;
            }
        }

        public void PageRefresh()
        {
            LoginPanel.PageRefresh();
            Debug.WriteLine("刷新了" + this.GetType().ToString() + "的内容");
        }
    }
}
