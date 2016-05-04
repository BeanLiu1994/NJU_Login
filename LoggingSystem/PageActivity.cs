using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toasts;
#region 注意事项
//需要在使用时注册通知事件使用的函数
#endregion
namespace LoggingSystem
{
    #region 基本类型
    public class ReturnData
    {
        //replyinfo
        public ReturnDataCodeMeaning ReturnCodeMeaning { get; set; }
        public ReplyCodeMeaning reply_code { get; set; }
        public string reply_msg { get; set; }
        //userinfo
        public string username { get; set; }
        public string fullname { get; set; }
        public string service_name { get; set; }
        public string area_name { get; set; }
        public long acctstarttime { get; set; }
        public double balance { get; set; }//元
        public string useripv4 { get; set; }
        public string useripv6 { get; set; }
        public string mac { get; set; }
        //noticeinfo
        public List<NoticeContent> Notices { get; set; }
        //volumeinfo
        public long onlineSeconds { get; set; }
        public TimeSpan onlineTimeSpan { get; set; }
        public string onlineTime { get; set; }
        public int month { get; set; }
    }
    public enum ReturnDataCodeMeaning
    {
        NoNetWork = -1,
        Success = 0,
        Fail = 1
    }
    public abstract class GetJSON
    {
        static GetJSON()
        { NotificationSender_URL += Toasts.ToastsDef.SendNotification_OneStringAndURL; }
        //表示这个类用于哪一个页(通用)
        public Pages DestPage { get; protected set; }

        //表示这个类最近一次取得的返回消息内容(通用)
        public string RecentInfo { get; protected set; }

        //用于修改Fetcher(通用)
        protected IDataFetcher Fetcher { get; set; }
        public void ChangeFetcher(IDataFetcher RequestedFetcher)
        {
            if (RequestedFetcher != null)
                Fetcher = RequestedFetcher;
        }

        //用于得到并分析返回信息(通用)
        public async Task<bool> RunSession()
        {
            string Content = await RunPostSession();
            bool ContentVerify = HandleRecentInfo();
            Debug.WriteLine("Post方法得到的内容 " + Content + " , 验证结果: " + ContentVerify.ToString());
            return ContentVerify;
        }
        protected virtual async Task<string> RunPostSession()
        {
            RecentInfo = await Fetcher.PostToUrl_WithPagesSelector(DestPage);
            return RecentInfo;
        }
        protected abstract bool HandleRecentInfo();

        //用于获取分析出的返回信息(通用)
        public abstract ReturnData GetRecentInfo();

        //用于转换返回代码的函数(通用)
        protected ReturnDataCodeMeaning CodeMeaningTranslate(ReplyCodeMeaning input)
        {
            switch (input)
            {
                case ReplyCodeMeaning.AlreadyLoggedIn:
                case ReplyCodeMeaning.LogInSucceed:
                case ReplyCodeMeaning.LogOutSucceed:
                case ReplyCodeMeaning.QuerySucceed:
                    return ReturnDataCodeMeaning.Success;
                case ReplyCodeMeaning.NoNetWork:
                    return ReturnDataCodeMeaning.NoNetWork;
                case ReplyCodeMeaning.InvalidIPAddr:
                case ReplyCodeMeaning.NotLoggedIn:
                case ReplyCodeMeaning.PassWordORUsernameEmpty:
                case ReplyCodeMeaning.PasswordORUsernameWrongORNoBalance:
                    return ReturnDataCodeMeaning.Fail;
                default:
                    return ReturnDataCodeMeaning.Fail;
            }
        }

        //用于通知发出部分(仅Notice)
        public delegate void SendNotification_OneStringAndURL_Handler(string a, string b, string c);
        public static event SendNotification_OneStringAndURL_Handler NotificationSender_URL;
        protected void SendNotification(string title,string content,string url)
        {
            if(NotificationSender_URL!=null)
                NotificationSender_URL(title, content, url);
        }

    }
    public enum ReplyCodeMeaning
    {
        NoNetWork = -1,
        QuerySucceed = 0,
        LogInSucceed = 1,
        NotLoggedIn = 2,
        InvalidIPAddr = 4,
        AlreadyLoggedIn = 6,

        LogOutSucceed = 101,
        PasswordORUsernameWrongORNoBalance = 3,
        PassWordORUsernameEmpty = 8,

        OtherFail = -2
    }
    #endregion

    #region LoginPart
    public class Login : GetJSON
    {
        [DataContract(Namespace = "http://p.nju.edu.cn")]
        private class CheckerConfig
        {
            [DataMember(Order = 0)]
            public int reply_code { get; set; }
            [DataMember(Order = 1)]
            public string reply_msg { get; set; }
        }
        public string username { get; set; }
        public string password { get; set; }
        public Login(string _username="",string _password="")
        {
            DestPage = Pages.LoginPage;
            username = _username;
            password = _password;
        }
        protected async override Task<string> RunPostSession()
        {
            //发送前检测是否能够进行登录 防止乱发post内容泄露消息
            ReturnDataCodeMeaning ReplyType = ReturnDataCodeMeaning.NoNetWork;
            string InfoCheck_String = await Fetcher.PostToUrl_WithPagesSelector(Pages.GetInfo);
            if (InfoCheck_String != "")
            { 
                var serializer = new DataContractJsonSerializer(typeof(CheckerConfig));
                var mStream = new MemoryStream(Encoding.UTF8.GetBytes(InfoCheck_String));
                var InfoCheck = (CheckerConfig)serializer.ReadObject(mStream);
                ReplyType = CodeMeaningTranslate((ReplyCodeMeaning)InfoCheck.reply_code);
            }
            if (ReplyType == ReturnDataCodeMeaning.Fail)
                RecentInfo = await Fetcher.PostToUrl_WithPagesSelector(DestPage, username, password);
            else if (ReplyType == ReturnDataCodeMeaning.NoNetWork)
                RecentInfo = "";
            else if (ReplyType == ReturnDataCodeMeaning.Success)
                RecentInfo = await Fetcher.PostToUrl_WithPagesSelector(DestPage, "1", "1");
            return RecentInfo;
        }
        protected override bool HandleRecentInfo()
        {
            if (RecentInfo.Length == 0)
            {
                UserConfig = new LoginConfig();
                UserConfig.reply_code = (int)ReplyCodeMeaning.NoNetWork;
                UserConfig.reply_msg = "没有校园网";
                Debug.WriteLine(UserConfig.reply_msg);
                UserConfig.userinfo = null;
                return false;
            }
            else
            {
                //如果有返回信息
                var serializer = new DataContractJsonSerializer(typeof(LoginConfig));
                var mStream = new MemoryStream(Encoding.UTF8.GetBytes(RecentInfo));
                UserConfig = (LoginConfig)serializer.ReadObject(mStream);
                Debug.WriteLine(UserConfig.reply_msg);
                return UserConfig.reply_code == (int)ReplyCodeMeaning.LogInSucceed || UserConfig.reply_code == (int)ReplyCodeMeaning.AlreadyLoggedIn;
            }
        }
        public override ReturnData GetRecentInfo()
        {
            ReturnData PreparedInfo = new ReturnData()
            {
                reply_code = (ReplyCodeMeaning)UserConfig.reply_code,
                reply_msg = UserConfig.reply_msg,
                ReturnCodeMeaning = CodeMeaningTranslate((ReplyCodeMeaning)UserConfig.reply_code)
            };
            if (PreparedInfo.ReturnCodeMeaning == ReturnDataCodeMeaning.Success)
            {
                PreparedInfo.acctstarttime = UserConfig.userinfo.acctstarttime;
                PreparedInfo.area_name = UserConfig.userinfo.area_name;
                PreparedInfo.balance = (double)UserConfig.userinfo.balance / 100.0;
                PreparedInfo.fullname = UserConfig.userinfo.fullname;
                PreparedInfo.mac = UserConfig.userinfo.mac;
                PreparedInfo.service_name = UserConfig.userinfo.service_name;
                PreparedInfo.username = UserConfig.userinfo.username;
                PreparedInfo.useripv4 = EncryptUtil.ip2Str(UserConfig.userinfo.useripv4, true);
                PreparedInfo.useripv6 = UserConfig.userinfo.useripv6;
            }
            return PreparedInfo;
        }

        LoginConfig UserConfig;
        #region LoginConfig Contract
        // Login Infomation
        [DataContract(Namespace = "http://p.nju.edu.cn")]
        private class LoginConfig
        {
            [DataMember(Order = 0)]
            public int reply_code { get; set; }
            [DataMember(Order = 1)]
            public string reply_msg { get; set; }
            [DataMember(Order = 2)]
            public UserinfoClass userinfo { get; set; }
        }

        [DataContract(Namespace = "http://p.nju.edu.cn")]
        private class UserinfoClass
        {
            [DataMember(Order = 0)]
            public string username { get; set; }
            [DataMember(Order = 1)]
            public string fullname { get; set; }
            [DataMember(Order = 2)]
            public string service_name { get; set; }
            [DataMember(Order = 3)]
            public string area_name { get; set; }
            [DataMember(Order = 4)]
            public long acctstarttime { get; set; }
            [DataMember(Order = 5)]
            public int balance { get; set; }
            [DataMember(Order = 6)]
            public long useripv4 { get; set; }
            [DataMember(Order = 7)]
            public string useripv6 { get; set; }
            [DataMember(Order = 8)]
            public string mac { get; set; }
        }
        #endregion
    }
    #endregion

    #region LogoutPart
    public class Logout : GetJSON
    {
        public Logout() { DestPage = Pages.LogoutPage; }
        protected override bool HandleRecentInfo()
        {
            if (RecentInfo.Length == 0)
            {
                UserConfig = new LogoutConfig();
                UserConfig.reply_code = (int)ReplyCodeMeaning.NoNetWork;
                UserConfig.reply_msg = "没有校园网";
                Debug.WriteLine(UserConfig.reply_msg);
                return false;
            }
            else
            { 
                //如果有返回信息
                var serializer = new DataContractJsonSerializer(typeof(LogoutConfig));
                var mStream = new MemoryStream(Encoding.UTF8.GetBytes(RecentInfo));
                UserConfig = (LogoutConfig)serializer.ReadObject(mStream);
                Debug.WriteLine(UserConfig.reply_msg);
                return UserConfig.reply_code == (int)ReplyCodeMeaning.LogOutSucceed;
            }
        }
        public override ReturnData GetRecentInfo()
        {
            ReturnData PreparedInfo = new ReturnData()
            {
                reply_code = (ReplyCodeMeaning)UserConfig.reply_code,
                reply_msg = UserConfig.reply_msg,
                ReturnCodeMeaning = CodeMeaningTranslate((ReplyCodeMeaning)UserConfig.reply_code)
            };
            return PreparedInfo;
        }

        LogoutConfig UserConfig;
        #region LogoutConfig Contract
        // Logout Infomation
        [DataContract(Namespace = "http://p.nju.edu.cn")]
        private class LogoutConfig
        {
            [DataMember(Order = 0)]
            public int reply_code { get; set; }
            [DataMember(Order = 1)]
            public string reply_msg { get; set; }
        }
        #endregion
    }
    #endregion

    #region InfoGetPart
    public class InfoGet : GetJSON
    {
        public InfoGet() { DestPage = Pages.GetInfo; }
        protected override bool HandleRecentInfo()
        {
            if (RecentInfo.Length == 0)
            {
                UserConfig = new InfoGetConfig();
                UserConfig.reply_code = (int)ReplyCodeMeaning.NoNetWork;
                UserConfig.reply_msg = "没有校园网";
                UserConfig.userinfo = null;
                Debug.WriteLine(UserConfig.reply_msg);
                return false;
            }
            else
            {
                bool IsLoggedIn = RecentInfo.IndexOf("userinfo") >= 0;
                if(IsLoggedIn)
                { 
                    //如果有返回信息
                    var serializer = new DataContractJsonSerializer(typeof(InfoGetConfig));
                    var mStream = new MemoryStream(Encoding.UTF8.GetBytes(RecentInfo));
                    UserConfig = (InfoGetConfig)serializer.ReadObject(mStream);
                    UserConfig.reply_code = (int)ReplyCodeMeaning.AlreadyLoggedIn;
                    UserConfig.reply_msg = "登录成功!";
                    Debug.WriteLine(UserConfig.reply_msg);
                    return true;
                }
                else
                {
                    UserConfig = new InfoGetConfig();
                    UserConfig.reply_code = (int)ReplyCodeMeaning.NotLoggedIn;
                    UserConfig.reply_msg = "无用户portal信息";
                    UserConfig.userinfo = null;
                    Debug.WriteLine(UserConfig.reply_msg);
                    return false;
                }
            }
        }
        public override ReturnData GetRecentInfo()
        {
            ReturnData PreparedInfo = new ReturnData()
            {
                reply_code = (ReplyCodeMeaning)UserConfig.reply_code,
                reply_msg = UserConfig.reply_msg,
                ReturnCodeMeaning = CodeMeaningTranslate((ReplyCodeMeaning)UserConfig.reply_code)
            };
            if (PreparedInfo.ReturnCodeMeaning == ReturnDataCodeMeaning.Success)
            {
                PreparedInfo.acctstarttime = UserConfig.userinfo.acctstarttime;
                PreparedInfo.area_name = UserConfig.userinfo.area_name;
                PreparedInfo.balance = (double)UserConfig.userinfo.balance / 100.0;
                PreparedInfo.fullname = UserConfig.userinfo.fullname;
                PreparedInfo.mac = UserConfig.userinfo.mac;
                PreparedInfo.service_name = UserConfig.userinfo.service_name;
                PreparedInfo.username = UserConfig.userinfo.username;
                PreparedInfo.useripv4 = EncryptUtil.ip2Str(UserConfig.userinfo.useripv4);
                PreparedInfo.useripv6 = UserConfig.userinfo.useripv6;
            }
            return PreparedInfo;
        }

        InfoGetConfig UserConfig;
        #region InfoGetConfig Contract
        // InfoGet Infomation
        [DataContract(Namespace = "http://p.nju.edu.cn")]
        private class InfoGetConfig
        {
            [DataMember(Order = 0)]
            public UserinfoClass userinfo { get; set; }

            public int reply_code { get; set; }
            public string reply_msg { get; set; }
        }
        [DataContract(Namespace = "http://p.nju.edu.cn")]
        private class UserinfoClass
        {
            [DataMember(Order = 0)]
            public string username { get; set; }
            [DataMember(Order = 1)]
            public string fullname { get; set; }
            [DataMember(Order = 2)]
            public string service_name { get; set; }
            [DataMember(Order = 3)]
            public string area_name { get; set; }
            [DataMember(Order = 4)]
            public long acctstarttime { get; set; }
            [DataMember(Order = 5)]
            public int balance { get; set; }
            [DataMember(Order = 6)]
            public long useripv4 { get; set; }
            [DataMember(Order = 7)]
            public string useripv6 { get; set; }
            [DataMember(Order = 8)]
            public string mac { get; set; }
        }
        #endregion
    }
    #endregion

    #region NoticePart
    public class NoticeContent
    {
        public string title { get; set; }
        public string url { get; set; }
        public string time{ get; set; }
    }
    public class Notice : GetJSON
    {
        #region Notice通知变化检测器
        public void FindIfNewNotice(Pages PageType, bool Hresult, ReturnData HArgs)
        {
            if (PageType != Pages.GetNotice) return;
            if (HArgs.ReturnCodeMeaning != ReturnDataCodeMeaning.Success) return;

            Debug.WriteLine("|检查Notice是否有新的");
            //比较通知是否有变化
            var Recorder = new SettingSaver_Local();

            string b_notice = "";
            Recorder.GetRecordString(NameManager.LastTimeNotice, ref b_notice);

            if (b_notice != RecentInfo)
            {
                Recorder.AlterRecordString(NameManager.LastTimeNotice, RecentInfo);

                if (b_notice.Length == 0)
                {
                    foreach (var m in UserConfig.notice)
                    {
                        Debug.WriteLine("|出现新通知，发送Notification.");
                        SendNotification("于 " + DistTime2String(m.disttime), m.title, m.url);
                    }
                }
                else
                {
                    var serializer = new DataContractJsonSerializer(typeof(NoticeConfig));
                    var b_mStream = new MemoryStream(Encoding.UTF8.GetBytes(b_notice));
                    var b_UserConfig = (NoticeConfig)serializer.ReadObject(b_mStream);
                    foreach (var m in UserConfig.notice)
                    {
                        bool HasThisNotice = false;
                        foreach (var n in b_UserConfig.notice)
                        {
                            if (m.title == n.title && m.url == n.url && m.disttime == n.disttime)
                            { HasThisNotice = true; break; }
                        }
                        if (HasThisNotice) continue;
                        Debug.WriteLine("|出现新通知，发送Notification.");
                        SendNotification("于 " + DistTime2String(m.disttime), m.title, m.url);
                    }
                }
            }
        }
        #endregion
        public Notice()
        {
            DestPage = Pages.GetNotice;
        }
        protected override bool HandleRecentInfo()
        {
            if (RecentInfo.Length == 0)
            {
                UserConfig = new NoticeConfig();
                UserConfig.reply_code = (int)ReplyCodeMeaning.NoNetWork;
                UserConfig.reply_msg = "没有校园网";
                Debug.WriteLine(UserConfig.reply_msg);
                return false;
            }
            else
            {
                //如果有返回信息
                var serializer = new DataContractJsonSerializer(typeof(NoticeConfig));
                var mStream = new MemoryStream(Encoding.UTF8.GetBytes(RecentInfo));
                try
                {
                    UserConfig = (NoticeConfig)serializer.ReadObject(mStream);
                }
                catch (Exception e)
                {
                    UserConfig.reply_code = (int)ReplyCodeMeaning.OtherFail;
                    UserConfig.reply_msg = "数据分析失败";
                    Debug.WriteLine(UserConfig.reply_msg);
                    return false;
                }

                if (UserConfig.notice != null)
                {
                    UserConfig.reply_code = (int)ReplyCodeMeaning.QuerySucceed;
                    UserConfig.reply_msg = "获取成功!";
                    for (int i = 0; i < UserConfig.total; ++i)
                    {
                        UserConfig.notice[i].title = Regex.Replace(UserConfig.notice[i].title, @"<.*?>", @"");
                    }
                    Debug.WriteLine(UserConfig.reply_msg);
                    return true;
                }
                else
                {
                    UserConfig.reply_code = (int)ReplyCodeMeaning.OtherFail;
                    UserConfig.reply_msg = "数据分析失败";
                    Debug.WriteLine(UserConfig.reply_msg);
                    return false;
                }
            }
        }
        public override ReturnData GetRecentInfo()
        {
            ReturnData PreparedInfo = new ReturnData()
            {
                reply_code = (ReplyCodeMeaning)UserConfig.reply_code,
                reply_msg = UserConfig.reply_msg,
                ReturnCodeMeaning = CodeMeaningTranslate((ReplyCodeMeaning)UserConfig.reply_code)
            };
            if(PreparedInfo.ReturnCodeMeaning == ReturnDataCodeMeaning.Success)
            {
                PreparedInfo.Notices = new List<NoticeContent>();
                for (int i = 0; i < UserConfig.total; ++i)
                {
                    NoticeContent tmp = new NoticeContent() { time = DistTime2String(UserConfig.notice[i].disttime), title = UserConfig.notice[i].title, url = UserConfig.notice[i].url };
                    PreparedInfo.Notices.Add(tmp);
                }
            }
            return PreparedInfo;
        }
        public static string DistTime2String(long disttime)
        {
            DateTime DateOfNotice = new DateTime(1970, 1, 1, 8, 0, 0);
            DateOfNotice = DateOfNotice.AddSeconds(disttime);
            return string.Format("{0:D4}年 {1:D2}月{2:D2}日 {3:D2}:{4:D2}", DateOfNotice.Year, DateOfNotice.Month, DateOfNotice.Day, DateOfNotice.Hour, DateOfNotice.Minute);
        }

        NoticeConfig UserConfig;
        #region NoticeConfig Contract
        // Notice Infomation
        [DataContract(Namespace = "http://p.nju.edu.cn")]
        private class NoticeConfig
        {
            [DataMember(Order = 0)]
            public int total { get; set; }
            [DataMember(Order = 1)]
            public NoticeContainer[] notice { get; set; }

            public int reply_code { get; set; }
            public string reply_msg { get; set; }
        }
        [DataContract(Namespace = "http://p.nju.edu.cn")]
        private class NoticeContainer
        {
            [DataMember(Order = 0)]
            public string title { get; set; }
            [DataMember(Order = 1)]
            public long disttime { get; set; }
            [DataMember(Order = 2)]
            public string url { get; set; }
        }
        #endregion
    }
    #endregion

    #region VolumePart
    public class Volume : GetJSON
    {
        public Volume()
        {
            DestPage = Pages.GetVolume;
        }
        protected override bool HandleRecentInfo()
        {
            if (RecentInfo.Length == 0)
            {
                UserConfig = new VolumeConfig();
                UserConfig.reply_code = (int)ReplyCodeMeaning.NoNetWork;
                UserConfig.reply_msg = "没有校园网";
                Debug.WriteLine(UserConfig.reply_msg);
                return false;
            }
            else
            {
                //如果有返回信息
                var serializer = new DataContractJsonSerializer(typeof(VolumeConfig));
                var mStream = new MemoryStream(Encoding.UTF8.GetBytes(RecentInfo));
                try
                { 
                    UserConfig = (VolumeConfig)serializer.ReadObject(mStream);
                }
                catch(Exception e)
                {
                    UserConfig = new VolumeConfig();
                    UserConfig.reply_code = (int)ReplyCodeMeaning.OtherFail;
                    UserConfig.reply_msg = "数据分析失败";
                    Debug.WriteLine(UserConfig.reply_msg);
                    return false;
                }

                if (UserConfig.total >= 0 && UserConfig.rows != null) 
                {
                    UserConfig.reply_code = (int)ReplyCodeMeaning.QuerySucceed;
                    UserConfig.reply_msg = "获取成功!";
                    Debug.WriteLine(UserConfig.reply_msg);
                    return true;
                }
                else
                {
                    UserConfig.reply_code = (int)ReplyCodeMeaning.OtherFail;
                    UserConfig.reply_msg = "数据分析失败";
                    Debug.WriteLine(UserConfig.reply_msg);
                    return false;
                }
            }
        }
        public override ReturnData GetRecentInfo()
        {
            ReturnData PreparedInfo = new ReturnData()
            {
                reply_code = (ReplyCodeMeaning)UserConfig.reply_code,
                reply_msg = UserConfig.reply_msg,
                ReturnCodeMeaning = CodeMeaningTranslate((ReplyCodeMeaning)UserConfig.reply_code)
            };
            if (PreparedInfo.ReturnCodeMeaning == ReturnDataCodeMeaning.Success)
            {
                PreparedInfo.month = UserConfig.rows[0].month;
                PreparedInfo.onlineSeconds = UserConfig.rows[0].total_ipv4_volume;
                PreparedInfo.onlineTimeSpan = new TimeSpan(0, 0, (int)PreparedInfo.onlineSeconds);
                PreparedInfo.onlineTime = OnlineTime2String(PreparedInfo.onlineTimeSpan);
            }
            return PreparedInfo;
        }
        public string OnlineTime2String(TimeSpan ConcreteTimeSpan)
        {//使用的数据不正确
            string AccTimeString = "";
            if (ConcreteTimeSpan.TotalDays > 10)
                AccTimeString = "约" + ((int)ConcreteTimeSpan.TotalDays).ToString() + "天" + ConcreteTimeSpan.Hours.ToString() + "小时";
            else if (ConcreteTimeSpan.TotalHours >= 1)
                AccTimeString = "约" + ((int)ConcreteTimeSpan.TotalHours).ToString() + "小时" + ConcreteTimeSpan.Minutes.ToString() + "分钟";
            else
                AccTimeString = "约" + ((int)ConcreteTimeSpan.TotalMinutes).ToString() + "分钟";
            return AccTimeString;
        }

        VolumeConfig UserConfig;
        #region VolumeConfig Contract
        // Volume Infomation
        [DataContract(Namespace = "http://p.nju.edu.cn")]
        private class VolumeConfig
        {
            [DataMember(Order = 0)]
            public int total { get; set; }
            [DataMember(Order = 1)]
            public string reply_msg { get; set; }
            [DataMember(Order = 2)]
            public VolumeContainer[] rows { get; set; }
            [DataMember(Order = 3)]
            public int reply_code { get; set; }
        }
        [DataContract(Namespace = "http://p.nju.edu.cn")]
        private class VolumeContainer
        {
            [DataMember(Order = 0)]
            public long id { get; set; }
            [DataMember(Order = 1)]
            public string ipv4_units { get; set; }
            [DataMember(Order = 2)]
            public string ipv6_units { get; set; }
            [DataMember(Order = 3)]
            public int month { get; set; }
            [DataMember(Order = 4)]
            public long service_id { get; set; }
            [DataMember(Order = 5)]
            public string service_name { get; set; }
            [DataMember(Order = 6)]
            public long total_input_octets_ipv4 { get; set; }
            [DataMember(Order = 7)]
            public long total_input_octets_ipv6 { get; set; }
            [DataMember(Order = 8)]
            public long total_ipv4_volume { get; set; }
            [DataMember(Order = 9)]
            public long total_ipv6_volume { get; set; }
            [DataMember(Order = 10)]
            public long total_output_octets_ipv4 { get; set; }
            [DataMember(Order = 11)]
            public long total_output_octets_ipv6 { get; set; }
            [DataMember(Order = 12)]
            public long total_refer_ipv4 { get; set; }
            [DataMember(Order = 13)]
            public long total_refer_ipv6 { get; set; }
            [DataMember(Order = 14)]
            public long total_time { get; set; }
            [DataMember(Order = 15)]
            public long user_id { get; set; }
            [DataMember(Order = 16)]
            public string username { get; set; }
        }
        #endregion
    }
    #endregion
}
