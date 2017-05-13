using LoggingSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace NJULoginTest
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class EasterEggPage : Page,SubPageInterface,INotifyPropertyChanged
    {
        public static EasterEggPage Current;
        public EasterEggPage()
        {
            this.InitializeComponent();
            Current = this;
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent += EasterEgg_ReturnDataEvent;
        }
        ~EasterEggPage()
        {
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent -= EasterEgg_ReturnDataEvent;
        }
        private TimeChecker myChecker5Sec = new TimeChecker(new TimeSpan(0, 0, 5));

        public event PropertyChangedEventHandler PropertyChanged;
        private string eastereggtext;
        public string EasterEggText
        {
            get { return eastereggtext; }
            set
            {
                eastereggtext = value;
                if (value != "")
                {
                    EasterEggAnalyse(EasterEggContainer.Blocks, EasterEggText);
                    EasterEggContainer.Visibility = Visibility.Visible;
                }
                else
                {
                    EasterEggContainer.Visibility = Visibility.Collapsed;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EasterEggText)));
            }
        }
        private void RectRefresh()
        {
            //ClipRectangle.Rect = new Rect(0,0,ActualWidth,ActualHeight);
        }

        private async void EasterEgg_ReturnDataEvent(Pages PageType, bool Hresult, ReturnData HArgs)
        {
            switch (PageType)
            {
                case Pages.LoginPage:
                case Pages.GetInfo:
                    if (HArgs.ReturnCodeMeaning == ReturnDataCodeMeaning.Success)
                    {
                        await GetEasterEggText(HArgs.username);
                    }
                    else
                    {
                        EasterEggText = "";
                    }
                    break;
                case Pages.LogoutPage:
                    EasterEggText = "";
                    break;
                default:
                    break;
            }
        }

        private async Task GetEasterEggText(string username)
        {
            if (username == null) return;
            EasterEggMessageFetcher myEEMF = new EasterEggMessageFetcher();
            bool RunResult = await myEEMF.Run(username);
            if (myEEMF.result_Analysed.Title == "Success")
            {
                EasterEggText = myEEMF.result_Analysed.Content;
            }
            else
            {
                EasterEggText = "";
            }
        }

        private async void EasterEggAnalyse(BlockCollection DestBlocks, string EasterEggText)
        {
            // 输入例如 'test text here{Image|/visglogo.jpg}this is a test'
            var SplitResult = EasterEggText.Split(new char[] { '{', '}' });
            if (myChecker5Sec.Check())
            {
                DestBlocks.Clear();
                foreach (var m in SplitResult)
                {
                    var a = new Paragraph();
                    var b = a.Inlines;
                    var c = new InlineUIContainer();
                    int imageIndex = m.ToLower().IndexOf("image");
                    if (imageIndex >= 0)
                    {
                        // Image Part 输入例如 '{Image|/visglogo.jpg}'
                        var ImageUri_string = m.Split('|').ElementAt(1);
                        // 收到的的链接参数应为类似于 '/visglogo.jpg' 的格式
                        var ImageUri = new Uri(NameManager.HTTPServerAddrPrefix + ImageUri_string);
                        var d = await ImageUri.GetImageFromNet();
                        c.Child = d;
                        b.Add(c);
                    }
                    else
                    {
                        //Text Part 输入例如 'test text here' 'this is a test'
                        var d = new TextBlock();
                        d.TextWrapping = TextWrapping.Wrap;
                        d.Text = m;
                        d.FontSize = 20;
                        d.Margin = new Thickness(5);
                        c.Child = d;
                        b.Add(c);
                    }
                    DestBlocks.Add(a);
                }
            }
        }

        public async void PageRefresh()
        {
            await GetEasterEggText(LoginControl.Current.LoginUsername);
        }

        private void RefreshContent(object sender, TappedRoutedEventArgs e)
        {
            PageRefresh();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RectRefresh();
        }
    }
}
