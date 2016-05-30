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
using Windows.UI.Xaml.Shapes;

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
                //DateTime DateOfNotice = new DateTime(1970, 1, 1, 8, 0, 0);
                //DateOfNotice = DateOfNotice.AddSeconds(u);
                var DateOfNotice = Datecalc(u);
                return DateOfNotice.ToString("yy-MM");
            };
            //PointChart.XYCount = new Size(4,3);
        }
        private DateTime Datecalc(double seconds)
        {
            DateTime DateOfNotice = new DateTime(1970, 1, 1, 8, 0, 0);
            return DateOfNotice.AddSeconds(seconds);
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
                        RefreshContentInTable(HArgs.Bills);
                        break;
                    case ReturnDataCodeMeaning.Fail:
                    case ReturnDataCodeMeaning.NoNetWork:
                        PointChart.Background = new SolidColorBrush(Colors.Crimson);
                        break;
                }
            }
        }

        public void RefreshContentInTable(Bill.BillContainer[] input)
        {
            TableOfData.Children.Clear();
            TableOfData.RowDefinitions.Clear();
            { 
                var rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                TableOfData.RowDefinitions.Add(rd);

                var rect = new Rectangle();
                Grid.SetRow(rect, 0);
                Grid.SetColumnSpan(rect, 6);
                rect.Fill = Application.Current.Resources["SystemControlHighlightAltListAccentLowBrush"] as SolidColorBrush;
                TableOfData.Children.Add(rect);
            }
            var TitleList = new List<string>() { "开始日期", "结束日期", "初期余额(元)", "本期充值金额(元)", "本期使用金额(元)", "期末余额(元)" };
            for (int i = 0; i < 6; ++i)
            {
                var thisline = new TextBlock();
                thisline.Text = TitleList[i];
                Grid.SetColumn(thisline, i);
                TableOfData.Children.Add(thisline);
            }
            for (int i = 0; i < input.Count(); ++i)
            {
                var rect = new Rectangle();
                Grid.SetRow(rect, i + 1);
                Grid.SetColumnSpan(rect, 6);
                if (i % 10 > 4)
                    rect.Fill = Application.Current.Resources["SystemControlHighlightAltListAccentLowBrush"] as SolidColorBrush;
                TableOfData.Children.Add(rect);

                var rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                TableOfData.RowDefinitions.Add(rd);
                var thisline_data = input[i];
                for (int j = 0; j < 6; ++j)
                {
                    var thisline = new TextBlock();
                    switch(j)
                    {
                        case 0:
                            thisline.Text = Datecalc(thisline_data.startdate).ToString("yyyy-MM-dd");
                            break;
                        case 1:
                            thisline.Text = Datecalc(thisline_data.enddate).ToString("yyyy-MM-dd");
                            break;
                        case 2:
                            thisline.Text = ((double)thisline_data.beginning_balance / 1000).ToString() + "元";
                            break;
                        case 3:
                            thisline.Text = ((double)thisline_data.recharge_amount / 1000).ToString() + "元";
                            break;
                        case 4:
                            thisline.Text = ((double)thisline_data.costs_amount / 1000).ToString() + "元";
                            break;
                        case 5:
                            thisline.Text = ((double)thisline_data.ending_balance / 1000).ToString() + "元";
                            break;
                    }
                    Grid.SetColumn(thisline, j);
                    Grid.SetRow(thisline, i + 1);
                    TableOfData.Children.Add(thisline);
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
