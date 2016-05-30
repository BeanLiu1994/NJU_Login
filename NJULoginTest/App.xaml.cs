using LoggingSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TileRefresh;
using Toasts;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace NJULoginTest
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public class DataType_ShowInfo : INotifyPropertyChanged
    {
        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title))); }
        }

        private string content;
        public string Content
        {
            get { return content; }
            set { content = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content))); }
        }

        private string url;
        public string Url
        {
            get { return url; }
            set { url = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Url))); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    interface SubPageInterface
    {
        void PageRefresh();
    }
    sealed partial class App : Application
    {
        #region 自定义的和LoggingSystem相关的部分

        TileRefreshUtils myTRU = new TileRefreshUtils();
        private async void AllRefresh()
        {            
            if (ShowLogin.Current != null) { ShowLogin.Current.PageRefresh(); }
            if (ShowNotice.Current != null) { ShowNotice.Current.PageRefresh(); }
            if (ShowSettings.Current != null) { ShowSettings.Current.PageRefresh(); }
        }
        #endregion
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
            RegisterWorks();
            LoggingSystem.LoggingSystem.HasWindow = true;
        }

        private void OnNetworkChanged()
        {
            NetworkInformation.NetworkStatusChanged += async (object sener) =>
            {
                await ShowLogin.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ShowLogin.Current.PageRefresh();
                });
            };
            NetworkInformation.NetworkStatusChanged += async (object sener) =>
            {
                await ShowNotice.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ShowNotice.Current.PageRefresh();
                });
            };
            NetworkInformation.NetworkStatusChanged += async (object sener) =>
            {
                await MainPage.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await MainPage.Current.RefreshPic();
                });
            };
        }

        private async void RegisterWorks()
        {
            var Timer_Condition = new IBackgroundCondition[]{
                new SystemCondition(SystemConditionType.FreeNetworkAvailable)
            };
            await RegisterLiveTileTask(
                NameManager.LIVETILETASK_Timer,
                typeof(TileRefreshUtils).FullName,
                new TimeTrigger(15, false),
                Timer_Condition
                );

            //调试的时候可能不能手动触发它
            await RegisterLiveTileTask(
                NameManager.LIVETILETASK_NetWorkChanged,
                typeof(TileRefreshUtils).FullName,
                new SystemTrigger(SystemTriggerType.NetworkStateChange, false),
                null
                );
            //await RegisterLiveTileTask(
            //    NameManager.LIVETILETASK_UserPresent,
            //    typeof(TileRefreshUtils).FullName,
            //    new SystemTrigger(SystemTriggerType.UserPresent, false),
            //    null
            //    );
        }

        private void OnResuming(object sender, object e)
        {
            AllRefresh();
        }
        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    OnNetworkChanged();
                    rootFrame.Navigate(typeof(ShowBill), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }

            Launch_Cases(e);
        }
        private async void Launch_Cases(LaunchActivatedEventArgs e)
        {
            if (e != null && e.Arguments.Count() > 0)
            {
                string temp = e.Arguments;
                temp = Regex.Replace(temp, "{\"", "");
                temp = Regex.Replace(temp, "\"}", "");
                temp = Regex.Replace(temp, "\",\"", " ");
                temp = Regex.Replace(temp, "\":\"", " ");
                string[] SplitResult = temp.Split(" ".ToCharArray());
                if (SplitResult[1] == "toast")
                {
                    if (SplitResult.Count() != 4) return;
                    Debug.WriteLine("目标URL: " + SplitResult[3]);
                    var uri = new Uri(SplitResult[3]);
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                }
                else if (SplitResult[1] == "BalanceReminder")
                {

                }
            }
        }

        protected async override void OnActivated(IActivatedEventArgs args)
        {
            if (ShowInfo.Current == null) { App.Current.Exit(); return; }
            if (args.Kind == ActivationKind.Protocol)
            {
                var protocalArgs = (ProtocolActivatedEventArgs)args;
                //check:
                if (protocalArgs.Uri.Scheme != "njuloginapp") return;
                switch (protocalArgs.Uri.Authority.ToLower())
                {
                    case "saveofflinecontent":
                        DataFetcher tempFetcher = new DataFetcher();
                        UserPassSaver_Roam tempUPS_R = new UserPassSaver_Roam();
                        string uname = "", upass = "";
                        tempUPS_R.Load(ref uname, ref upass);
                        if (uname != null && uname != "")
                            await tempFetcher.SaveOfflineDebuggingContent(uname, upass);
                        break;
                    case "testmode":
                        ShowSettings.Current.ShowTestModeSetting();
                        break;
                    case "toasttest":
                        ToastsDef.SendNotification_TwoString("Test", "通知测试");
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }
        //LiveTileSetting
        public static async Task RegisterLiveTileTask(string _Name, string _TaskEntryPoint, IBackgroundTrigger _Trigger, IBackgroundCondition[] _ConditionTable)
        {
            //建立builder
            var taskBuilder = new BackgroundTaskBuilder
            {
                Name = _Name,
                TaskEntryPoint = _TaskEntryPoint
            };
            //清除已有的
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Unspecified || status == BackgroundAccessStatus.Denied)
            {
                return;
            }
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.Clear();

            foreach (var t in BackgroundTaskRegistration.AllTasks)
            {
                if (t.Value.Name == taskBuilder.Name)
                {
                    t.Value.Unregister(true);
                }
            }
            //如果Trigger为null撤销这个后台任务
            if (_Trigger == null) return;

            //继续构建builder
            taskBuilder.SetTrigger(_Trigger);

            if (_ConditionTable != null)
                foreach (var m in _ConditionTable)
                {
                    taskBuilder.AddCondition(m);
                }

            //注册
            taskBuilder.Register();
            Debug.WriteLine("注册了名为" + _Name + "的后台任务");
        }
    }
}
