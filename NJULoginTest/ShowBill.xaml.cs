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
using Windows.UI;
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
    public sealed partial class ShowBill : Page, SubPageInterface
    {
        public static ShowBill Current;
        public ShowBill()
        {
            Current = this;
            this.InitializeComponent();
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent += BillRefresh_ReturnDataEvent;
            PageRefresh();
            PointsSource = new PointCollection();
            PointChart.XStringFormatFunc = (u) =>
            {
                DateTime DateOfNotice = new DateTime(1970, 1, 1, 8, 0, 0);
                DateOfNotice = DateOfNotice.AddSeconds(u);
                return DateOfNotice.ToString("yy-MM");
            };
            //PointChart.XYCount = new Size(4,3);
        }
        ~ShowBill()
        {
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent -= BillRefresh_ReturnDataEvent;
        }
        private PointCollection PointsSource { get; set; } 
        private void BillRefresh_ReturnDataEvent(LoggingSystem.Pages PageType, bool Hresult, LoggingSystem.ReturnData HArgs)
        {
            if (PageType == Pages.GetBill)
            {
                switch (HArgs.ReturnCodeMeaning)
                {
                    case ReturnDataCodeMeaning.Success:
                        PointsSource.Clear();
                        foreach (var m in HArgs.Bills)
                        {
                            var X = m.enddate;
                            var Y = ((double)m.ending_balance / 1000);
                            PointsSource.Add(new Point() { X = X, Y = Y });
                        }
                        PointChart.PointsSource = PointsSource;
                        break;
                    case ReturnDataCodeMeaning.Fail:
                    case ReturnDataCodeMeaning.NoNetWork:
                        PointChart.Background = new SolidColorBrush(Colors.Crimson);
                        break;
                }
            }
        }

        public async void PageRefresh()
        {
            await LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.GetBill);
            Debug.WriteLine("刷新了" + this.GetType().ToString() + "的内容");
        }

        private void RefreshPage(object sender, TappedRoutedEventArgs e)
        {
            PageRefresh();
        }
    }
}
