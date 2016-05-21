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
        }
        ~ShowBill()
        {
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent -= BillRefresh_ReturnDataEvent;
        }
        private PointCollection PointsSource { get; set; } 
        private DateTime timetool(long disttime)
        {
            DateTime DateOfNotice = new DateTime(1970, 1, 1, 8, 0, 0);
            DateOfNotice = DateOfNotice.AddSeconds(disttime);
            return DateOfNotice;
        }
        private void BillRefresh_ReturnDataEvent(LoggingSystem.Pages PageType, bool Hresult, LoggingSystem.ReturnData HArgs)
        {
            if(PageType== Pages.GetBill)
            {
                PointsSource.Clear();
                switch (HArgs.ReturnCodeMeaning)
                {
                    case ReturnDataCodeMeaning.Success:
                        foreach (var m in HArgs.Bills)
                        {
                            var X_span = new DateTime(((DateTime.Today - timetool(m.enddate)).Ticks));
                            var X = (X_span.Year * 12 + X_span.Month)*20;
                            var Y = ((double)m.ending_balance / 1000);
                            PointsSource.Add(new Point() { X = X, Y = Y});

                            var thisrow = new TextBlock();
                            string recharged = "没有充值";
                            if (m.recharge_amount != 0)
                                recharged = "充值了 [" + ((double)m.recharge_amount/1000).ToString() +"]";
                            thisrow.Text = "[" + Notice.DistTime2String(m.startdate) + "] 到 [" +
                                Notice.DistTime2String(m.enddate) + "] 时间段内,由 [" + 
                                ((double)m.beginning_balance/1000).ToString() + "] 用到了 [" + ((double)m.ending_balance / 1000).ToString() + "] 期间" + 
                                recharged                                
                                ;
                            BillInfo.Children.Add(thisrow);
                        }
                        break;
                    case ReturnDataCodeMeaning.Fail:
                    case ReturnDataCodeMeaning.NoNetWork:
                        var thisrowEr = new TextBlock();
                        thisrowEr.Text = "Error Ocurred";
                        BillInfo.Children.Add(thisrowEr);
                        break;
                }
            }
        }

        public async void PageRefresh()
        {
            await LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.GetBill);
            Debug.WriteLine("刷新了" + this.GetType().ToString() + "的内容");
        }



    }
}
