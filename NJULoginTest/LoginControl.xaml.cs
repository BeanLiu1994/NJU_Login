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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NJULoginTest
{
    public sealed partial class LoginControl : UserControl,INotifyPropertyChanged,SubPageInterface
    {
        private UserPassSaver_Roam UINFOSaver;
        public static LoginControl Current;
        private const string InitTitlestr = "登入登出";
        public LoginControl()
        {
            Current = this;
            this.InitializeComponent();

            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent += LoginInfoDataHandler;
            UINFOSaver = new UserPassSaver_Roam();
            {
                string username = "", password = "";
                UINFOSaver.Load(ref username, ref password);
                if (username != "" && password != "")
                {
                    Username = username;
                    Password = password;
                    SavePassword.IsChecked = true;
                }
            }
            
            TitleStr = InitTitlestr;
            reply_msg = "";

        }
        ~LoginControl()
        {
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent -= LoginInfoDataHandler;
        }

        public string ServiceName { get; private set; }
        public void LoginInfoDataHandler(Pages PageType, bool Hresult, ReturnData HArgs)
        {
            switch (PageType)
            {
                case Pages.GetInfo:
                case Pages.LoginPage:
                    ServiceName = "";
                    reply_msg = HArgs.reply_msg;
                    switch (HArgs.ReturnCodeMeaning)
                    {
                        case ReturnDataCodeMeaning.Success:
                            CurrentState = LoginUIState.LoggedIn;
                            ServiceName = HArgs.service_name;
                            if(PageType == Pages.LoginPage)
                                if ((SavePassword.IsChecked.HasValue ? SavePassword.IsChecked.Value : false))
                                {
                                    UINFOSaver.Save(Username, Password);
                                }
                                else
                                {
                                    UINFOSaver.Delete();
                                }
                            break;
                        case ReturnDataCodeMeaning.NoNetWork:
                            CurrentState = LoginUIState.NoNetwork;
                            break;
                        case ReturnDataCodeMeaning.Fail:
                            if (PageType == Pages.GetInfo)
                            {
                                CurrentState = LoginUIState.Normal;
                            }
                            else
                            {
                                CurrentState = LoginUIState.LogInFailed;
                            }
                            break;
                    }
                    break;
                case Pages.LogoutPage:
                    reply_msg = HArgs.reply_msg;
                    ServiceName = "";
                    switch (HArgs.ReturnCodeMeaning)
                    {
                        case ReturnDataCodeMeaning.Success:
                            CurrentState = LoginUIState.LoggedOut;
                            break;
                        case ReturnDataCodeMeaning.Fail:
                            CurrentState = LoginUIState.LogOutFailed;
                            break;
                        case ReturnDataCodeMeaning.NoNetWork:
                            CurrentState = LoginUIState.NoNetwork;
                            break;
                    }
                    break;
                //需要在另一种情况下添加在线时间
                default:
                    break;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private string replymsg;
        public string reply_msg
        {
            get { return replymsg; }
            set { replymsg = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(reply_msg))); }
        }

        private string username;
        public string Username
        {
            get { return username; }
            set { username = value; PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Username))); }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password))); }
        }
        private string title;
        public string TitleStr
        {
            get { return title; }
            set { title = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TitleStr))); }
        }

        private LoginUIState currentstate;
        public LoginUIState CurrentState
        {
            get { return currentstate; }
            set
            {
                if (currentstate == value) return;
                Debug.WriteLine("切换了状态" + currentstate.ToString() +  " => " + value.ToString());
                currentstate = value;StateSwitch(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentState)));
            }
        }

        private void StateSwitch(LoginUIState dest_state)
        {
            VisualStateManager.GoToState(this, dest_state.ToString(), true);
        }

        public async Task Login()
        {
            LoggingSystem.LoggingSystem.SystemControl.RegisterFetcherUser(new Login(Username, Password));
            await LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.LoginPage);
        }
        public async Task Logout()
        {
            await LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.LogoutPage);
        }
        public async void PageRefresh()
        {
            await LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.GetInfo);
        }
        private bool PrompLogout = true;
        private async void ActButton_Click(object sender, RoutedEventArgs e)
        {
            var stateNow = CurrentState;
            CurrentState = LoginUIState.Waiting;
            switch (stateNow)
            {
                case LoginUIState.Normal:
                case LoginUIState.LoggedOut:
                case LoginUIState.LogInFailed:
                    await Login();
                    break;
                case LoginUIState.LoggedIn:
                case LoginUIState.LogOutFailed:
                    if (ServiceName == NameManager.DirectOutServiceName && PrompLogout)
                    {
                        TitleStr = "当前是"+ NameManager.DirectOutServiceName + " 不建议登出";
                        PrompLogout = false;
                        CurrentState = LoginUIState.LoggedIn;
                    }
                    else if(!PrompLogout)
                    {
                        TitleStr = InitTitlestr;
                        PrompLogout = true;
                    }
                    if (PrompLogout)
                        await Logout();
                    break;
                case LoginUIState.NoNetwork:
                    PageRefresh();
                    break;
                case LoginUIState.Waiting:
                default:
                    break;
            }
        }
        private void KeyPress(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ActButton_Click(ActButton as object, null);
            }
        }
        
        private void Title_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PageRefresh();
        }
    }
    public enum LoginUIState
    {
        Normal = 0,
        NoNetwork,
        LoggedIn,
        LoggedOut,
        Waiting,
        LogOutFailed,
        LogInFailed
    }
}
