using LoggingSystem;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
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
    public class ThemeSetting : BoolSetting_Local
    {
        public ThemeSetting()
        {
            base.SetID(NameManager.ThemeSettingString);
            LoadSetting();
        }
        public override void ApplySetting()
        {
            switch (State)
            {
                case true:
                    MainPage.Current.RequestedTheme = ElementTheme.Dark;
                    break;
                case false:
                    MainPage.Current.RequestedTheme = ElementTheme.Light;
                    break;
            }
        }
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ShowSettings : Page
    {
        public static ShowSettings Current;
        ThemeSetting MyThemeSetting = new ThemeSetting();
        AutoLoginSetting MyAutoLoginSetting = new AutoLoginSetting();
        RefreshLoginSetting MyRefreshLoginSetting = new RefreshLoginSetting();
        NetworkStateChangeLoginSetting MyNetworkStateChangeLoginSetting = new NetworkStateChangeLoginSetting();
        PrivacyUploadSetting MyPrivacyUploadSetting = new PrivacyUploadSetting();
        BGTransparentSetting MyBGTransparentSetting = new BGTransparentSetting();
        public ShowSettings()
        {
            Current = this;
            this.InitializeComponent();
            PageRefresh();
        }
        public void PageRefresh()
        {
            MyThemeSetting.LoadSetting();
            ThemeSetting_UI.IsOn = (MyThemeSetting.State);

            MyAutoLoginSetting.LoadSetting();
            AutoLoginSetting_UI.IsOn = (MyAutoLoginSetting.State);

            MyRefreshLoginSetting.LoadSetting();
            RefreshLoginSetting_UI.IsOn = (MyRefreshLoginSetting.State);

            MyNetworkStateChangeLoginSetting.LoadSetting();
            NetworkStateChangeLoginSetting_UI.IsOn = (MyNetworkStateChangeLoginSetting.State);

            MyPrivacyUploadSetting.LoadSetting();
            MyPrivacyUploadSetting_UI.IsOn = (MyPrivacyUploadSetting.State);

            MyBGTransparentSetting.LoadSetting();
            MyBGTransparentSetting_UI.IsOn = (MyBGTransparentSetting.State);

            Debug.WriteLine("刷新了" + this.GetType().ToString() + "的内容");
        }
        private void PageRefresh(object sender, TappedRoutedEventArgs e)
        {
            PageRefresh();
        }

        private void ThemeSetter(object original_sender, RoutedEventArgs e)
        {
            var sender = original_sender as ToggleSwitch;
            MyThemeSetting.ApplySetting(sender.IsOn);
        }

        private void AutoLoginSetter(object original_sender, RoutedEventArgs e)
        {
            var sender = original_sender as ToggleSwitch;
            MyAutoLoginSetting.ApplySetting(sender.IsOn);
        }

        private void RefreshLoginSetter(object original_sender, RoutedEventArgs e)
        {
            var sender = original_sender as ToggleSwitch;
            MyRefreshLoginSetting.ApplySetting(sender.IsOn);
        }

        private void NetworkStateChangeLoginSetter(object original_sender, RoutedEventArgs e)
        {
            var sender = original_sender as ToggleSwitch;
            MyNetworkStateChangeLoginSetting.ApplySetting(sender.IsOn);
            if (sender.IsOn)
            {
                if (!BackgroundTaskHelper.IsBackgroundTaskRegistered(NameManager.LIVETILETASK_NetWorkChanged))
                {
                    BackgroundTaskHelper.Register(
                        NameManager.LIVETILETASK_NetWorkChanged,
                        typeof(TileRefresh.TileRefreshUtils).FullName,
                        new SystemTrigger(SystemTriggerType.NetworkStateChange, false));
                }
                //RefreshLoginSetting_UI.IsEnabled = true;
                //await App.RegisterLiveTileTask(
                //    NameManager.LIVETILETASK_UserPresent,
                //    typeof(TileRefresh.TileRefreshUtils).FullName,
                //    new SystemTrigger(SystemTriggerType.UserPresent, false),
                //    null
                //);
            }
            else
            {
                //RefreshLoginSetting_UI.IsOn = false;
                //MyRefreshLoginSetting.ApplySetting(false);
                //RefreshLoginSetting_UI.IsEnabled = false;
                //if (BackgroundTaskHelper.IsBackgroundTaskRegistered(NameManager.LIVETILETASK_NetWorkChanged))
                //{
                //    BackgroundTaskHelper.Unregister(NameManager.LIVETILETASK_NetWorkChanged);
                //}
                //await App.RegisterLiveTileTask(
                //    NameManager.LIVETILETASK_UserPresent,
                //    typeof(TileRefresh.TileRefreshUtils).FullName,
                //    null,
                //    null
                //);
            }
        }

        private async void TestModeSetter(object original_sender, RoutedEventArgs e)
        {
            var sender = original_sender as ToggleSwitch;
            if (sender.IsOn)
            {
                LoggingSystem.LoggingSystem.SystemControl.ChangeFetcher(new DataFetcher_Test());
            }
            else
            {
                LoggingSystem.LoggingSystem.SystemControl.ChangeFetcher(new DataFetcher());
            }
            LoggingSystem.LoggingSystem.SystemControl.BalanceCheckResult = false;
            await LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.GetInfo);
            await LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.GetNotice);
        }

        public void ShowTestModeSetting()
        {
            TestModeSetting_UI.Visibility = Visibility.Visible;
        }

        private void MyPrivacyUploadSetting_UI_Toggled(object original_sender, RoutedEventArgs e)
        {
            var sender = original_sender as ToggleSwitch;
            MyPrivacyUploadSetting.ApplySetting(sender.IsOn);
        }

        private void BGTransparentSetter(object original_sender, RoutedEventArgs e)
        {
            var sender = original_sender as ToggleSwitch;
            MyBGTransparentSetting.ApplySetting(sender.IsOn);
        }
    }
}
