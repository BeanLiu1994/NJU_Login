using LoggingSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace TileRefresh
{
    public sealed class TileRefreshUtils: IBackgroundTask
    {
        private const string TileXml_LoggedIn = @"<tile>
  <visual displayName='{0}'>
    <binding template = 'TileSmall' hint-textStacking='center' hint-overlay='30'>
      <text hint-style='caption' hint-align='center'>{0}</text>
    </binding>

    <binding template = 'TileMedium' >
      <text hint-align='left' hint-wrap='true' hint-style='body'>{1}</text>
      <group>
        <subgroup hint-weight='1'>     
          <text hint-align='center'>余额</text>          
          <text hint-align='center'>时长</text>
        </subgroup>
        <subgroup hint-weight='2'>
          <text hint-style='captionsubtle' hint-align='center'>{3}</text>
          <text hint-style='captionsubtle' hint-wrap='true' hint-align='center'>{4}</text>
        </subgroup>
      </group>
    </binding>

    <binding template = 'TileWide' >
      <text hint-style='body'>{1}({2})</text>
      <group>
        <subgroup hint-weight='1'>  
          <text hint-align='center'>账户余额</text>          
          <text hint-align='center'>登陆时长</text>
          <text hint-align='center'>登陆区域</text>
        </subgroup>
        <subgroup hint-weight='2'>
          <text hint-style='captionsubtle'>{3}</text>
          <text hint-style='captionsubtle'>{4}</text>
          <text hint-style='captionsubtle'>{5}</text>
        </subgroup>
      </group>
    </binding>

    <binding template = 'TileLarge' >
      <text hint-style='body'>{1}({2})</text>
      <group>
        <subgroup hint-weight='1'>  
          <text hint-align='center'>账户余额</text>          
          <text hint-align='center'>登陆时长</text>
          <text hint-align='center'>登陆区域</text>
        </subgroup>
        <subgroup hint-weight='2'>
          <text hint-style='captionsubtle'>{3}</text>
          <text hint-style='captionsubtle'>{4}</text>
          <text hint-style='captionsubtle'>{5}</text>
        </subgroup>
      </group>
     <text/>
      <text hint-style='body' hint-align='left'>公告信息</text>

      <text hint-wrap='true' hint-style='captionsubtle'> {6}</text>   
      <text hint-wrap='true' hint-style='captionsubtle'> {7}</text>  
      <text hint-wrap='true' hint-style='captionsubtle'> {8}</text>
      
    </binding>
  </visual>
</tile>
";
        private const string TileXml_NotLoggedIn = @"<tile>
  <visual displayName='{0}'>
    <binding template='TileSmall' hint-textStacking='center' hint-overlay='30'>
      <text hint-style='caption' hint-align='center'>{0}</text>
    </binding>
    <binding template='TileMedium' > 
      <text hint-align='left' hint-wrap='true' hint-style='body'>{0}</text>      
      <text hint-style='captionsubtle' hint-align='left'>{1}</text>
    </binding>
    <binding template='TileWide'>      
      <text hint-style='body'>{0}</text>
      <text hint-style='captionsubtle' hint-align='left'>{1}</text>
    </binding>
    <binding template='TileLarge'>
      <text hint-style='body'>{0}</text>
      <text hint-style='captionsubtle' hint-align='left'>{1}</text>
      <text/>
      <text hint-style='body' hint-align='left'>公告信息 </text>
      <text hint-wrap='true' hint-style='captionsubtle'> {2}</text>   
      <text hint-wrap='true' hint-style='captionsubtle'> {3}</text>  
      <text hint-wrap='true' hint-style='captionsubtle'> {4}</text>
    </binding>
  </visual>
</tile>
";
        private const string TileXml_Notice = @"<tile>
  <visual displayName='{0}'>
    <binding template='TileSmall' hint-textStacking='center' hint-overlay='30'>
      <text hint-style='caption' hint-align='center'>{0}</text>
    </binding>
    <binding template='TileMedium' > 
      <text hint-align='left' hint-wrap='true' hint-style='body'>公告信息</text>    
      <text hint-wrap='true' hint-style='captionsubtle'> {2}</text>   
    </binding>
    <binding template='TileWide'>      
      <text hint-style='body'>公告信息</text>
      <text hint-wrap='true' hint-style='captionsubtle'> {2}</text>   
      <text hint-wrap='true' hint-style='captionsubtle'> {3}</text> 
    </binding>
  </visual>
</tile>
";
        private const string TileXml_NoNetwork = @"<tile>
  <visual displayName='{0}'>
    <binding template='TileSmall' hint-textStacking='center' hint-overlay='30'>
      <text hint-style='caption' hint-align='center'>{0}</text>
    </binding>
    <binding template='TileMedium' > 
      <text hint-align='left' hint-wrap='true' hint-style='body'>{0}</text>      
      <text hint-style='captionsubtle' hint-align='left'>{1}</text>
    </binding>
    <binding template='TileWide'>      
      <text hint-style='body'>{0}</text>
      <text hint-style='captionsubtle' hint-align='left'>{1}</text>
    </binding>
    <binding template='TileLarge'>
      <text hint-style='body'>{0}</text>
      <text hint-style='captionsubtle' hint-align='left'>{1}</text>
      <text/>
    </binding>
  </visual>
</tile>
";
        public TileRefreshUtils()
        {
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent += RefreshTileAPI;
        }
        ~TileRefreshUtils()
        {
            LoggingSystem.LoggingSystem.SystemControl.ReturnDataEvent -= RefreshTileAPI;
        }
        //如果外边用大概就长这样了
        //IList内含  [0]FullName [1]UserName [2]Balance [3]Area
        ReturnData HArgs_uinfo = new ReturnData() { reply_msg = "未初始化", reply_code = ReplyCodeMeaning.OtherFail, ReturnCodeMeaning = ReturnDataCodeMeaning.Fail, username = "", fullname = "", onlineTime = "未知", balance = 0 };
        List<string> notice = new List<string> { "", "", "" };

        List<bool?> StateChecker_Result = new List<bool?>() { null, null, null };
        enum check_enum { notice_checker,uinfo_checker,volume_checker};
        private void StateChecker(ReturnData HArgs)
        {
            bool reset = true;
            if(HArgs.ReturnCodeMeaning == ReturnDataCodeMeaning.Success && HArgs.reply_code == ReplyCodeMeaning.LogOutSucceed)
            {
                UpdateTile_NotLoggedIn();
            }
            else if (StateChecker_Result[0] == true && StateChecker_Result[1] == true && StateChecker_Result[2] != null)
            {
                UpdateTile_LoggedIn(HArgs_uinfo);
            }
            else if(StateChecker_Result[0] == true && StateChecker_Result[1] == false)
            {
                UpdateTile_NotLoggedIn();
            }
            else if(StateChecker_Result[0] == false && StateChecker_Result[1] == false)
            {
                UpdateTile_NoNetWork();
            }
            else
            {
                reset = false;
            }

            //if (reset)
            //    StateChecker_Result = new List<bool?>() { null, null, null };
        }
        private void RefreshTileAPI(Pages PageType, bool Hresult, ReturnData HArgs)
        {
            if (PageType == Pages.GetNotice)
            {
                StateChecker_Result[(int)check_enum.notice_checker] = Hresult;
                if (HArgs.Notices != null)
                {
                    notice = new List<string> { "", "", "" };
                    for (var i = 0; i < HArgs.Notices.Count; ++i)
                    {
                        notice[i] = HArgs.Notices[i].title;
                    }
                }
            }

            if (HArgs.ReturnCodeMeaning == ReturnDataCodeMeaning.Success)
                switch (PageType)
                {
                    case Pages.LogoutPage:
                        //UpdateTile_NotLoggedIn();
                        break;
                    case Pages.LoginPage:
                    case Pages.GetInfo:
                        StateChecker_Result[(int)check_enum.uinfo_checker] = Hresult;
                        HArgs_uinfo = HArgs;
                        HArgs_uinfo.onlineTime = "未知";
                        break;
                    case Pages.GetVolume:
                        StateChecker_Result[(int)check_enum.volume_checker] = Hresult;
                        HArgs_uinfo.onlineSeconds = HArgs.onlineSeconds;
                        HArgs_uinfo.onlineTime = HArgs.onlineTime;
                        HArgs_uinfo.onlineTimeSpan = HArgs.onlineTimeSpan;
                        break;
                }
            else if(HArgs.ReturnCodeMeaning == ReturnDataCodeMeaning.NoNetWork)
            {
                if (PageType == Pages.GetInfo || PageType == Pages.LoginPage)
                {
                    StateChecker_Result[(int)check_enum.uinfo_checker] = false;
                    StateChecker_Result[(int)check_enum.volume_checker] = false;
                    if (StateChecker_Result[(int)check_enum.notice_checker] == null)
                        LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.GetNotice);
                    //这里是真的不用去等待
                }
                else if (PageType == Pages.GetVolume)
                {
                    StateChecker_Result[(int)check_enum.volume_checker] = false;
                    if (StateChecker_Result[(int)check_enum.notice_checker] == null)
                        LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.GetNotice);
                    //这里是真的不用去等待
                }
            }
            else
            {
                if (PageType == Pages.GetInfo || PageType == Pages.LoginPage)
                {
                    StateChecker_Result[(int)check_enum.uinfo_checker] = false;
                    StateChecker_Result[(int)check_enum.volume_checker] = false;
                }
                else if(PageType == Pages.GetVolume)
                {
                    StateChecker_Result[(int)check_enum.volume_checker] = false;
                }
            }

            StateChecker(HArgs);
            
        }


        TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();

        private void UpdateTile_LoggedIn(ReturnData e)
        {
            string BalanceString = e.balance.ToString() + "元";
            string AccTimeString = e.onlineTime;

            UpdateTile(new List<string>()
            {
                string.Format(TileXml_LoggedIn, "已登陆", e.fullname, e.username, BalanceString, AccTimeString, e.area_name, notice[0], notice[1], notice[2]),
                string.Format(TileXml_Notice, "已登陆", "",  notice[0], notice[1], notice[2])
            });
            Debug.WriteLine("更新磁贴 已登录");
        }
        private void UpdateTile_NoNetWork()
        {
            UpdateTile(new List<string>()
            {
                string.Format(TileXml_NoNetwork, "无连接","找不到校园网络", "","","")
            });
            Debug.WriteLine("更新磁贴 无连接");
        }
        private void UpdateTile_NotLoggedIn()
        {
            UpdateTile(new List<string>()
            {
                string.Format(TileXml_NotLoggedIn, "未连接", "打开应用以登陆",  notice[0], notice[1], notice[2]),
                string.Format(TileXml_Notice, "未连接", "打开应用以登陆",  notice[0], notice[1], notice[2])
            });
            Debug.WriteLine("更新磁贴 未登录");
        }
        private void UpdateTile(IList<string> UserInfo)
        {
            try
            {
                updater.EnableNotificationQueue(true);
                updater.Clear();
                foreach (string iter_string in UserInfo)
                    updater.Update(new TileNotification(AddToDoc(iter_string)));
            }
            catch (Exception)
            {
                // ignored
            }
        }
        private XmlDocument AddToDoc(string toAppend)
        {
            XmlDocument newXmlDoc = new XmlDocument();
            newXmlDoc.LoadXml(WebUtility.HtmlDecode(toAppend), new XmlLoadSettings
            {
                ProhibitDtd = false,
                ValidateOnParse = false,
                ElementContentWhiteSpace = false,
                ResolveExternals = false
            });
            return newXmlDoc;
        }

        //系统调用部分
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            bool IsNetworkStateChangeTrigger = false;
            switch(taskInstance.Task.Name)
            {
                case NameManager.LIVETILETASK_NetWorkChanged:
                    var NetworkDetails = (Windows.Networking.Connectivity.NetworkStateChangeEventDetails)taskInstance.TriggerDetails;
                    if (NetworkDetails != null)
                    //if (!NetworkDetails.HasNewInternetConnectionProfile)
                    {
                        if (!(NetworkDetails.HasNewInternetConnectionProfile|| NetworkDetails.HasNewNetworkConnectivityLevel))
                        {
                            deferral.Complete();
                            Debug.WriteLine("不进行刷新");
                            return;
                        }
                        Debug.WriteLine("网络连接变化");
                        IsNetworkStateChangeTrigger = true;
                    }  
                    break;
                case NameManager.LIVETILETASK_UserPresent:
                    IsNetworkStateChangeTrigger = true;
                    break;
                default:break;
            }

            Debug.WriteLine("磁铁任务开始");            
            bool Hresult_notice = await LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.GetNotice);
            if(!Hresult_notice)
                UpdateTile_NoNetWork();//如果无法刷新
            else
                Hresult_notice = !await LoggingSystem.LoggingSystem.SystemControl.RefreshLoginSession(IsNetworkStateChangeTrigger);
            if(Hresult_notice)
            {
                Hresult_notice = await LoggingSystem.LoggingSystem.SystemControl.RunConcreteUser(Pages.GetInfo);
                // 这里因为await在 Event的地方停止了所以没有继续等待GetVolume的执行 此处手动等待1.5s 这是最长的可能延时
                await Task.Delay(1000);
            }
            Debug.WriteLine("磁铁任务完成");
            deferral.Complete();
        }
    }
}
