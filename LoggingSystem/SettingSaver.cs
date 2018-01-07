using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LoggingSystem
{
    //private static StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
    #region SettingSaver 定义
    public class SettingSaver
    {
        public virtual void DeleteRecordObject(string Name) { }
        public virtual void AlterRecordObject<T>(string Name, T value) { }
        public virtual bool GetRecordObject<T>(string Name, ref T value) { return false; }
        #region 衍生
        public void DeleteRecordString(string Name)
        {
            DeleteRecordObject(Name);
        }
        public void AlterRecordString(string Name, string value)
        {
            AlterRecordObject(Name, value);
        }
        public void GetRecordString(string Name, ref string value)
        {
            GetRecordObject(Name, ref value);
        }
        #endregion
    }
    public class SettingSaver_Local : SettingSaver
    {
        private static ApplicationDataContainer Settings = ApplicationData.Current.LocalSettings;
        #region Object-Local settings
        public override void DeleteRecordObject(string Name)
        {
            Settings.Values.Remove(Name);
        }
        public override void AlterRecordObject<T>(string Name, T value)
        {
            Settings.Values[Name] = value;
        }
        public override bool GetRecordObject<T>(string Name, ref T value)
        {
            if (Settings.Values.ContainsKey(Name))
            {
                value = (T)Settings.Values[Name];
                return true;
            }
            else
                return false;
        }
        #endregion
    }
    public class SettingSaver_Roam : SettingSaver
    {

        private static ApplicationDataContainer Settings = ApplicationData.Current.RoamingSettings;
        #region Object-Roaming settings
        public override void DeleteRecordObject(string Name)
        {
            Settings.Values.Remove(Name);
        }
        public override void AlterRecordObject<T>(string Name, T value)
        {
            Settings.Values[Name] = value;
        }
        public override bool GetRecordObject<T>(string Name, ref T value)
        {
            if (Settings.Values.ContainsKey(Name))
            {
                value = (T)Settings.Values[Name];
                return true;
            }
            else
                return false;
        }

        #endregion
    }
    #endregion
    #region UserPass Saver定义部分 [此处使用了username和password对应的key]
    public class UserPassSaver
    {
        protected static string uname = NameManager.uname;
        protected static string upass = NameManager.upass;

        protected static SettingSaver SaverUsing = null;

        protected UserPassSaver() { }
        // 不允许实例化 不知道在c#里是否同样可以这样写

        public virtual void Save(string username, string password)
        {
            SaverUsing.AlterRecordString(uname, username);
            SaverUsing.AlterRecordString(upass, password);
        }
        public virtual void Load(ref string username, ref string password)
        {
            SaverUsing.GetRecordString(uname, ref username);
            SaverUsing.GetRecordString(upass, ref password);
        }
        public virtual void Delete()
        {
            SaverUsing.DeleteRecordString(uname);
            SaverUsing.DeleteRecordString(upass);
        }
    }
    public class UserPassSaver_Local : UserPassSaver
    {
        public UserPassSaver_Local()
        {
            SaverUsing = new SettingSaver_Local();
        }
    }
    public class UserPassSaver_Roam : UserPassSaver
    {
        public UserPassSaver_Roam()
        {
            SaverUsing = new SettingSaver_Roam();
        }
    }
    public class UserPassSaver_None : UserPassSaver
    {
        public override void Save(string username, string password)
        {
            //不保存
        }
        public override void Load(ref string username, ref string password)
        {
            username = null;
            password = null;
            //不保存
        }
        public override void Delete()
        {
            //不保存
        }
    }
    #endregion
    #region Setting定义
    public enum SaveMethodEnum { None, Local, Roaming };
    public abstract class Setting
    {
        protected SettingSaver Saver;
        public Setting()
        {
        }
        public void SetSaver(SaveMethodEnum SaveMethod)
        {
            switch (SaveMethod)
            {
                case SaveMethodEnum.Local:
                    Saver = new SettingSaver_Local();
                    break;
                case SaveMethodEnum.Roaming:
                    Saver = new SettingSaver_Roam();
                    break;
                case SaveMethodEnum.None:
                    Saver = new SettingSaver();
                    break;
                default:
                    Saver = new SettingSaver_Local();
                    break;
            }
        }
        public abstract void LoadSetting();
        public abstract void SaveSetting();
        public abstract void ApplySetting();
        public abstract void ApplySetting(object OutSetting);
    }
    public abstract class BoolSetting_Local : Setting
    {
        string ID { get; set; }
        public bool State { get; set; }
        public BoolSetting_Local()
        {
            State = false;
            SetSaver(SaveMethodEnum.Local);
        }
        public override void LoadSetting()
        {
            bool State_sender = State;
            Saver.GetRecordObject(ID, ref State_sender);
            State = State_sender;
            ApplySetting();
        }
        public override void SaveSetting()
        {
            Saver.AlterRecordObject(ID, State);
        }
        public override void ApplySetting(object OutSetting)
        {
            State = (bool)OutSetting;
            SaveSetting();
            ApplySetting();
            Debug.WriteLine("修改了某个设置的值为:" + OutSetting.ToString());
        }

        public void SetID(string _ID)
        {
            ID = _ID;
        }
    }
    public class AutoLoginSetting : BoolSetting_Local
    {
        public AutoLoginSetting()
        {
            base.SetID(NameManager.AutoLoginSettingString);
            LoadSetting();
        }
        public override void ApplySetting()
        {
            // do nothing here. just save something.
        }
    }
    public class RefreshLoginSetting : BoolSetting_Local
    {
        public RefreshLoginSetting()
        {
            base.SetID(NameManager.RefreshLoginSettingString);
            LoadSetting();
        }
        public override void ApplySetting()
        {
            // do nothing here. just save something.
        }
    }
    public class NetworkStateChangeLoginSetting : BoolSetting_Local
    {
        public NetworkStateChangeLoginSetting()
        {
            base.SetID(NameManager.NetworkStateChangeLoginSettingString);
            LoadSetting();
        }
        public override void ApplySetting()
        {
            // do nothing here. just save something.
        }
    }
    public class UesrPresentLoginSetting : BoolSetting_Local
    {
        public UesrPresentLoginSetting()
        {
            base.SetID(NameManager.UesrPresentLoginSettingString);
            LoadSetting();
        }
        public override void ApplySetting()
        {
            // do nothing here. just save something.
        }
    }
    public class PrivacyUploadSetting : BoolSetting_Local
    {
        public PrivacyUploadSetting()
        {
            base.SetID(NameManager.PrivacyUploadSettingString);
            LoadSetting();
        }
        public override void ApplySetting()
        {
            // do nothing here. just save something.
        }
    }
    public class BGTransparentSetting : BoolSetting_Local
    {
        public BGTransparentSetting()
        {
            base.SetID(NameManager.BGTransparentSettingString);
            LoadSetting();
        }
        public override void ApplySetting()
        {
            // do nothing here. just save something.
        }
    }
    #endregion
}
