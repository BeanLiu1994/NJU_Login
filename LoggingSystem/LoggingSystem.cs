using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;

namespace LoggingSystem
{
    public partial class LoggingSystem
    {
        public static bool HasWindow = false;
        public static LoggingSystem SystemControl = new LoggingSystem();
        public delegate void ReturnDataHandler(Pages PageType, bool Hresult, ReturnData HArgs);
        public event ReturnDataHandler ReturnDataEvent;
        private void RunReturnDataEvent(Pages PageType, bool Hresult, ReturnData HArgs)
        {
            if (ReturnDataEvent != null) ReturnDataEvent(PageType, Hresult, HArgs);
        }

        #region 初始化 和 Fetcher相关内容
        // 程序内直接通过IDataFetcher进行操作即可
        public LoggingSystem(bool TestMode = false)
        {
            ReturnDataEvent += EventAfterLoginAndUserInfoGetHandler;
            //Initial
            FetcherUsers = new Dictionary<Pages, GetJSON>();
            SetConcreteUser(new Login());
            SetConcreteUser(new Logout());
            SetConcreteUser(new InfoGet());

            var Notice = new Notice();
            SetConcreteUser(Notice);
            ReturnDataEvent += Notice.FindIfNewNotice;

            SetConcreteUser(new Volume());

            //Set Fetcher
            if (TestMode)
                ChangeFetcher(new DataFetcher_Test());
            else
                ChangeFetcher(new DataFetcher());

            //不等待的刷新notice  不过好像不必放在这里 只要保证其他位置调用时刷新就行
            //RunConcreteUser(Pages.GetNotice);
        }

        public async Task<bool> RefreshLoginSession(bool IsNetworkStateChangeTrigger)
        {
            UserPassSaver_Roam UINFOSaver = new UserPassSaver_Roam();
            RefreshLoginSetting MyAutoLoginSetting = new RefreshLoginSetting();
            NetworkStateChangeLoginSetting MyNetworkStateChangeLoginSetting = new NetworkStateChangeLoginSetting();
            {
                string username = "", password = "";
                UINFOSaver.Load(ref username, ref password);
                if (username != "" && password != "")
                    if ((IsNetworkStateChangeTrigger && MyNetworkStateChangeLoginSetting.State)||
                        (!IsNetworkStateChangeTrigger && MyAutoLoginSetting.State))
                    {
                        RegisterFetcherUser(new Login(username, password));
                        await RunConcreteUser(Pages.LoginPage);
                        await Task.Delay(200);
                        Debug.WriteLine("已尝试自动登录");
                    }
            }
            return MyAutoLoginSetting.State || MyNetworkStateChangeLoginSetting.State;
        }

        private double balance = 0;
        public async void EventAfterLoginAndUserInfoGetHandler(Pages PageType, bool Hresult, ReturnData HArgs)
        {
            if (HArgs.ReturnCodeMeaning == ReturnDataCodeMeaning.Success)
                if (PageType == Pages.LoginPage || PageType == Pages.GetInfo)
                {
                    balance = HArgs.balance;
                    await RunConcreteUser(Pages.GetVolume);//这里必须等待
                }
                else if (PageType == Pages.GetVolume)//先后顺序一定是先info(or login)再volume的
                {
                    BalanceCheck(balance, HArgs.onlineTimeSpan);
                }
        }

        public bool SetConcreteUser(GetJSON input)
        {
            input.ChangeFetcher(Fetcher_Now);
            FetcherUsers[input.DestPage] = input;
            return true;
        }

        public bool GetConcreteUser(ref GetJSON output)
        {
            if (!FetcherUsers.ContainsKey(output.DestPage)) return false;
            else { return FetcherUsers.TryGetValue(output.DestPage, out output); }
        }

        public async Task<bool> RunConcreteUser(Pages which)
        {
            GetJSON output;
            if (FetcherUsers.TryGetValue(which, out output))
                return await RunConcreteUser(output);
            else return false;
        }
        public async Task<bool> RunConcreteUser(GetJSON which, bool set_as_User = false)
        {
            if (which == null)
                return false;
            else
            {
                Debug.WriteLine("开始执行对于" + which.DestPage.ToString() + "的任务");
                if (set_as_User)
                    SetConcreteUser(which);
                bool Hresult = await which.RunSession();
                ReturnData HArgs = which.GetRecentInfo();
                Debug.WriteLine("|" + which.DestPage.ToString() + "刷新显示内容");
                RunReturnDataEvent(which.DestPage, Hresult, HArgs);
                Debug.WriteLine("|" + which.DestPage.ToString() + "结束");
                return Hresult;
            }
        }

        protected IDataFetcher Fetcher_Now;
        protected Dictionary<Pages, GetJSON> FetcherUsers;//每一页都有固定位置
        public void RegisterFetcherUser(GetJSON User, bool AddAnyWay = false)
        {
            if (!AddAnyWay)
                foreach (var m in FetcherUsers)
                {
                    if (m.Value == User || m.Key == User.DestPage) break;
                }
            Debug.WriteLine("已加入一个FetcherUser Type:" + User.DestPage.ToString());
            User.ChangeFetcher(Fetcher_Now);
            FetcherUsers[User.DestPage] = User;
        }
        public void ChangeFetcher(IDataFetcher RequestedFetcher)
        {
            if (RequestedFetcher != null)
            {
                Fetcher_Now = RequestedFetcher;
                foreach (var m in FetcherUsers)
                {
                    m.Value.ChangeFetcher(Fetcher_Now);
                }
                Debug.WriteLine("已改变Fetcher为" + Fetcher_Now.ToString());
            }
        }


        //BalanceChecker
        private void BalanceCheckToastPart(string Title, string Content)
        {
            //规则 每天在6点(start)到23点(end)之间进行余额提醒
            //     每次间隔为sep的值(小时)
            //      打开应用时会进行提醒不会受这里影响(因为在正常打开应用界面时会将设置里的stamp重置)
            var today = DateTime.Now;
            int sep = 3;
            int start = 6, end = 23;
            var today_stamp = (today.Day * (end - start) / sep + (today.Hour - start) / ((end - start) / sep)).ToString();
            SettingSaver_Local temp = new SettingSaver_Local();
            string BalanceCheckerResult = "";
            temp.GetRecordString(NameManager.BalanceChecker, ref BalanceCheckerResult);
            if (HasWindow || BalanceCheckerResult != today_stamp && today.Hour < end && today.Hour > start)
            {
                temp.AlterRecordString(NameManager.BalanceChecker, today_stamp);
                Toasts.ToastsDef.SendNotification_TwoString(Title, Content, "BalanceReminder", null);
            }
        }
        private const double Price = 0.2;//per hour

        public bool BalanceCheckResult = false;//仅用在下面的函数内
        private void BalanceCheck(double balance, TimeSpan timesp)
        {
            if (balance < -1000) return;//可能是数据获取错了
            if (!BalanceCheckResult)
            {
                Debug.WriteLine("检查了余额");
                if (balance < 0)
                {
                    BalanceCheckResult = true;
                    BalanceCheckToastPart("本月余额提醒", "当前余额为" + balance.ToString() + "元，注意充值哦~");
                }
                else if (timesp.TotalHours < 30)
                {
                    //免费时段
                    if (balance < 20)
                    {
                        BalanceCheckResult = true;
                        BalanceCheckToastPart("本月余额提醒", "当前余额为" + balance.ToString() + "元，注意及时充值防止突然掉线哦~");
                    }
                }
                else if (timesp.TotalHours < 130)
                {
                    //收费时段 保留两位小数
                    if (balance < Math.Round((Price * (130 - timesp.TotalHours))*100)/100)
                    {
                        BalanceCheckResult = true;
                        BalanceCheckToastPart("本月余额提醒", "当前余额为" + balance.ToString() + "元，注意及时充值防止突然掉线哦~");
                    }
                }
                else
                {
                    //收费封顶免费使用
                    if (balance < 20 && DateTime.Now.Day > 26)
                    {
                        BalanceCheckResult = true;
                        BalanceCheckToastPart("下月余额提醒", "当前余额为" + balance.ToString() + "元，注意及时充值防止下月掉线哦~");
                    }
                }
                Debug.WriteLine("余额检查结果为" + BalanceCheckResult.ToString());
            }
        }
        #endregion
    }
}