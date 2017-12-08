using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingSystem
{
    public class NameManager
    {
        #region LocalFolder DebugContent(TestMode)文件:
        public const string DebugInfoFile = "OfflineDebuggingContent.txt";
        #endregion

        #region RoamingSetting userinfo Key:
        public const string uname = "3%8dFw6bCi#";
        public const string upass = "6*kRO3C&%vR";
        #endregion

        #region LocalSetting 最近一次通知内容 Key:
        public const string LastTimeNotice = "5r$Y3<Fsek&";
        #endregion

        #region 自定义字符串
        public const string RefreshPageNote = "7Uo2f*d(3Nb";
        public const string BalanceChecker = "Hy@tG(7%2Vd";
        public const string LIVETILETASK_Timer = "LiveTileTask_Timer";
        public const string LIVETILETASK_NetWorkChanged = "LiveTileTask_OnNetworkChanged";
        public const string LIVETILETASK_UserPresent = "LiveTileTask_OnUserPresent";
        #endregion

        #region 设置字符串
        public const string ThemeSettingString = "ThemeSetting";
        public const string AutoLoginSettingString = "AutoLoginSetting";
        public const string RefreshLoginSettingString = "RefreshLoginSetting";
        public const string TestModeSettingString = "TestModeSetting";
        public const string NetworkStateChangeLoginSettingString = "NetworkStateChangeLoginSetting";
        public const string PrivacyUploadSettingString = "PrivacyUploadSettingString";
        public const string BGTransparentSettingString = "BGTransparentSettingString";
        #endregion

        public const string HTTPServerAddrPrefix = "http://visg.nju.edu.cn:16043";

        public const string DirectOutServiceName = "直出服务";
    }

    public static class StringHelper
    {
        //扩展方法必须是静态的，第一个参数必须加上this
        public static string RemoveEnter(this string _input)
        {
            var pos = _input.LastIndexOf('\n');
            if (pos >= 0)
                _input = _input.Remove(pos);
            return _input;
        }
    }
}
