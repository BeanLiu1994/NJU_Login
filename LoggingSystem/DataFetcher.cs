using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toasts;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace LoggingSystem
{
    // Pages 是页面序号的枚举
    // 对应的内容在DataFetcher内使用
    public enum Pages
    {
        MainPage = 0, LoginPage, LogoutPage,
        GetInfo, GetVolume, GetNotice, GetList, GetAuthLog, GetUsage, GetBill, GetCharge,
        TopNum
    };
    // DataFetcher需要的基类
    public abstract class IDataFetcher
    {
        public IDataFetcher() { Init(); }
        public abstract void Init();
        public abstract Task<string> PostToUrl_WithPagesSelector(Pages PageIndex, string _username = "", string _password = "");

        //网络访问方法 封装访问实现细节
        //想用 Windows.Web.Http 的话不用改代码,直接替换System.Net.Http
        protected async Task<string> PostToUrl(string url, HttpContent content = null)
        {
            string result = "";
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(2.5));//设置延时时间2.5s
            try
            {
                HttpClient myHC = new HttpClient();
                HttpResponseMessage response = await myHC.PostAsync(new Uri(url), content, cts.Token);
                result = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException e)
            {
                Debug.WriteLine("连接超时");
                Debug.WriteLine(e.Message);
                Debug.WriteLine("可能出现的问题是: api修改 网络断开 没有使用校园网");
            }
            catch (Exception e)
            {
                Debug.WriteLine("exception!!");
                Debug.WriteLine(e.Message);
                Debug.WriteLine("可能出现的问题是: api修改 网络断开 没有使用校园网");
            }
            //返回结果网页（html）代码
            return result;
        }
    }
    public class DataFetcher : IDataFetcher
    {
        #region 初始化
        public override void Init()
        {
            InitURLs();
        }
        #region URL初始化
        private string[] PageURLs = null;
        private void InitURLs()
        {
            PageURLs = new string[(int)Pages.TopNum];
            PageURLs[(int)Pages.MainPage] = @"http://p.nju.edu.cn/";

            PageURLs[(int)Pages.LoginPage] = @"http://p.nju.edu.cn/portal_io/login";
            PageURLs[(int)Pages.LogoutPage] = @"http://p.nju.edu.cn/portal_io/logout";
            PageURLs[(int)Pages.GetInfo] = @"http://p.nju.edu.cn/portal_io/getinfo";
            PageURLs[(int)Pages.GetVolume] = @"http://p.nju.edu.cn/portal_io/selfservice/volume/getlist";//时长

            PageURLs[(int)Pages.GetNotice] = @"http://p.nju.edu.cn/portal_io/proxy/notice";//通知
            PageURLs[(int)Pages.GetList] = @"http://p.nju.edu.cn/portal_io/selfservice/online/getlist";
            PageURLs[(int)Pages.GetAuthLog] = @"http://p.nju.edu.cn/portal_io/selfservice/authlog/getlist";
            PageURLs[(int)Pages.GetUsage] = @"http://p.nju.edu.cn/portal_io/selfservice/acct/getlist";
            PageURLs[(int)Pages.GetBill] = @"http://p.nju.edu.cn/portal_io/selfservice/bill/getlist";
            PageURLs[(int)Pages.GetCharge] = @"http://p.nju.edu.cn/portal_io/selfservice/recharge/getlist";

        }
        #endregion
        #endregion

        #region Post方法
        //Debug在此处进行切换 将debug变化封装
        public override async Task<string> PostToUrl_WithPagesSelector(Pages PageIndex, string _username = "", string _password = "")
        {
            int SiteSelector = (int)PageIndex;
            if (_username == null || _password == null)
            {
                _username = "";
                _password = "";
            }
            if (_username == "" || _password == "")
                return await PostToUrl(PageURLs[SiteSelector]);
            else
            {
                var username = new KeyValuePair<string, string>("username", _username);
                var password = new KeyValuePair<string, string>("password", _password);
                var form = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>() { username, password });
                return await PostToUrl(PageURLs[SiteSelector], form);
            }
        }
        #endregion
        #region 保存接收的信息
        public async Task<string> SaveOfflineDebuggingContent(string username, string password)
        {
            string Allcontent = "{  \n";
            //0号不保存
            for (int i = 0; i < (int)Pages.TopNum; ++i)
            {
                await Task.Delay(100);
                Pages temp = (Pages)i;
                string content = "";
                switch (temp)
                {
                    case Pages.LoginPage:
                        content = await PostToUrl_WithPagesSelector(temp, username, password); ;
                        if (content.Length == 0) break; ;//说明没网,不继续这一个了
                        Allcontent += "\t" + temp.ToString() + ":" + content.RemoveEnter() + ",\n";
                        break;
                    case Pages.MainPage:
                        Allcontent += "\t" + temp.ToString() + ":{For Offline Debugging Usage.  Content Prepared on " + DateTime.Today.ToString("yyyy-MM-dd") + "},\n";
                        break;
                    case Pages.LogoutPage: break;
                    default:
                        content = await PostToUrl_WithPagesSelector(temp);
                        if (content.Length == 0) break; ;//说明没网,不继续这一个了
                        Allcontent += "\t" + temp.ToString() + ":" + content.RemoveEnter() + ",\n";
                        break;
                }
            }
            {
                //string content = await PostToUrl_WithPagesSelector(Pages.LogoutPage);
                //if (content.Length != 0)
                //    Allcontent += "\t" + Pages.LogoutPage.ToString() + ":" + content.RemoveEnter() + ",\n";
                if (Allcontent.Length != 0)
                    Allcontent += "\t" + Pages.LogoutPage.ToString() + ":" + @"{""reply_code"":101,""reply_msg"":""下线成功!""}" + ",\n";
            }
            //至此Allcontent的结尾是 "},\n" 删去两个字符可以去除逗号
            Allcontent = Allcontent.Remove(Allcontent.Count() - 2); Allcontent += "\n}";
            try
            {
                StorageFile FormmerFile = await ApplicationData.Current.LocalFolder.GetFileAsync("OfflineDebuggingContent.txt");
                await FormmerFile.DeleteAsync();
            }
            catch (Exception e) { Debug.WriteLine("将创建文件OfflineDebuggingContent.txt"); }

            StorageFile ContentFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("OfflineDebuggingContent.txt");
            try
            {
                using (Stream stream = await ContentFile.OpenStreamForWriteAsync())
                {
                    byte[] content_b = Encoding.UTF8.GetBytes(Allcontent);
                    await stream.WriteAsync(content_b, 0, content_b.Length);
                }
                ToastsDef.SendNotification_TwoString("保存数据", "已保存至: " + ContentFile.Path);
            }
            catch (Exception e) { Debug.WriteLine("未能成功写入文件"); }
            try
            {
                DataPackage pv = new DataPackage();
                pv.SetText(ContentFile.Path);
                Clipboard.SetContent(pv);
            }
            catch (Exception e) { Debug.WriteLine("未能成功复制到剪贴板"); }
            return ContentFile.Path;
        }
        #endregion
    }
    public class DataFetcher_Test : IDataFetcher
    {
        #region 初始化
        public override async void Init()
        {
            StorageFile ContentFile = null;
            try
            {
                ContentFile = await ApplicationData.Current.LocalFolder.GetFileAsync(NameManager.DebugInfoFile); ;
            }
            catch (Exception e)
            {
                InitTestingContent_OffLine();
            }
            if (ContentFile != null) await InitTestingContent(ContentFile);
        }
        #region offlineTestingContent初始化
        private string[] offlineTestingContent = null;
        private void InitTestingContent_OffLine()
        {

            offlineTestingContent = new string[(int)Pages.TopNum];
            offlineTestingContent[(int)Pages.MainPage] = @"For Offline Testing Usage.  Content Prepared on 2016-01-31.";

            offlineTestingContent[(int)Pages.LoginPage] =
                "{\"reply_code\":1,\"reply_msg\":\"操作成功\",\"userinfo\":{\"username\":\"123456789\",\"fullname\":\"姓名隐藏\",\"service_name\":\"标准学生计时服务\",\"area_name\":\"XL_电子楼\",\"acctstarttime\":680536,\"balance\":72,\"useripv4\":1926524937,\"useripv6\":\"::\",\"mac\":\"[敏感信息]\"}}";
            offlineTestingContent[(int)Pages.LogoutPage] =
                "{\"reply_code\":101,\"reply_msg\":\"登出了\"}";
            offlineTestingContent[(int)Pages.GetInfo] =
                "{\"userinfo\":{\"username\":\"123456789\",\"useripv4\":2887366998,\"useripv6\":\"::\",\"acctstarttime\":680536,\"mac\":\"[敏感信息]\",\"fullname\":\"姓名隐藏\",\"service_name\":\"学生标准计时服务\",\"area_name\":\"XL_电子楼Wlan\",\"balance\":72},\"reply_code\":0,\"reply_msg\":\"操作成功\"}";
            offlineTestingContent[(int)Pages.GetVolume] =
                "{\"total\":1,\"reply_msg\":\"操作成功\",\"rows\":[{\"id\":631239,\"ipv4_units\":\"S\",\"ipv6_units\":\"S\",\"month\":1,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"total_input_octets_ipv4\":100285236743,\"total_input_octets_ipv6\":0,\"total_ipv4_volume\":124220,\"total_ipv6_volume\":0,\"total_output_octets_ipv4\":27557609635,\"total_output_octets_ipv6\":0,\"total_refer_ipv4\":4579,\"total_refer_ipv6\":0,\"total_time\":804034,\"user_id\":13455564690171,\"username\":\"账号隐藏\"}],\"reply_code\":0}";
            offlineTestingContent[(int)Pages.GetNotice] =
                "{\"total\":2,\"notice\":[{\"title\":\"校园网络电视开通直播(beta)\",\"disttime\":12345,\"url\":\"http://baidu.com\"},{\"title\":\"宿舍小路由器设置教程\",\"disttime\":364245,\"url\":\"http://baidu.com\"}]}";
            offlineTestingContent[(int)Pages.GetList] =
                "{\"total\":1,\"reply_msg\":\"操作成功\",\"rows\":[{\"acctinputoctets_ipv4\":0,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":0,\"acctoutputoctets_ipv6\":0,\"acctsessionid\":\"14526805367790\",\"acctsessiontime\":0,\"acctstarttime\":1452680492,\"ap_id\":\"(null)\",\"area_id\":14162884395340,\"area_name\":\"XL_电子楼Wlan\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":14171059,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":4030,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3421,\"user_id\":13455564690171,\"user_ipv4\":2887366998,\"user_ipv6\":\"::\",\"username\":\"账号隐藏\"}],\"reply_code\":0}";
            offlineTestingContent[(int)Pages.GetAuthLog] =
                "{\"total\":252,\"reply_msg\":\"操作成功\",\"rows\":[{\"id\":13843922,\"acctsessionid\":\"14526805367790\",\"ap_id\":null,\"area_id\":14162884395340,\"username\":\"账号隐藏\",\"area_name\":\"XL_电子楼Wlan\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452680491,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3421,\"pvlan\":4030,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13833657,\"acctsessionid\":\"14526726229161\",\"ap_id\":null,\"area_id\":14162820264558,\"username\":\"账号隐藏\",\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452672577,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"pvlan\":899,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13807312,\"acctsessionid\":\"14526536909578\",\"ap_id\":null,\"area_id\":14162884395340,\"username\":\"账号隐藏\",\"area_name\":\"XL_电子楼Wlan\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452653645,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3421,\"pvlan\":4030,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13805972,\"acctsessionid\":\"14526526912516\",\"ap_id\":null,\"area_id\":14162884395340,\"username\":\"账号隐藏\",\"area_name\":\"XL_电子楼Wlan\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452652646,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3421,\"pvlan\":4030,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13805848,\"acctsessionid\":\"14526526021056\",\"ap_id\":null,\"area_id\":14162884395340,\"username\":\"账号隐藏\",\"area_name\":\"XL_电子楼Wlan\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452652556,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3421,\"pvlan\":4030,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13805675,\"acctsessionid\":\"14526524630752\",\"ap_id\":null,\"area_id\":14162884395340,\"username\":\"账号隐藏\",\"area_name\":\"XL_电子楼Wlan\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452652417,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3421,\"pvlan\":4030,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13805521,\"acctsessionid\":\"14526523370375\",\"ap_id\":null,\"area_id\":14162884395340,\"username\":\"账号隐藏\",\"area_name\":\"XL_电子楼Wlan\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452652291,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3421,\"pvlan\":4030,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13803370,\"acctsessionid\":\"14526507982248\",\"ap_id\":null,\"area_id\":14162903201606,\"username\":\"账号隐藏\",\"area_name\":\"XL_食堂Wlan\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452650753,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3432,\"pvlan\":4030,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13800823,\"acctsessionid\":\"14526491870737\",\"ap_id\":null,\"area_id\":14162820264558,\"username\":\"账号隐藏\",\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452649141,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"pvlan\":899,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13800653,\"acctsessionid\":\"14526490937446\",\"ap_id\":null,\"area_id\":14162820264558,\"username\":\"账号隐藏\",\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452649048,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"pvlan\":899,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13800531,\"acctsessionid\":\"14526490201844\",\"ap_id\":null,\"area_id\":14162820264558,\"username\":\"账号隐藏\",\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452648975,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"pvlan\":899,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13789897,\"acctsessionid\":\"14526162146801\",\"ap_id\":null,\"area_id\":14162820264558,\"username\":\"账号隐藏\",\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452616169,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"pvlan\":899,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13789852,\"acctsessionid\":\"14526160916014\",\"ap_id\":null,\"area_id\":14162820264558,\"username\":\"账号隐藏\",\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452616046,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"pvlan\":913,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13789196,\"acctsessionid\":\"14526148712047\",\"ap_id\":null,\"area_id\":14162820264558,\"username\":\"账号隐藏\",\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452614826,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"pvlan\":899,\"reply_code\":255,\"reply_msg\":\"登录成功\"},{\"id\":13759712,\"acctsessionid\":\"14525899373404\",\"ap_id\":null,\"area_id\":14162820264558,\"username\":\"账号隐藏\",\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"area_type_name\":\"有线_Portal\",\"datetime\":1452589892,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"pvlan\":899,\"reply_code\":255,\"reply_msg\":\"登录成功\"}],\"reply_code\":0}";
            offlineTestingContent[(int)Pages.GetUsage] =
                "{\"total\":243,\"reply_msg\":\"操作成功\",\"rows\":[{\"acctsessionid\":\"14526726229161\",\"acctsessiontime\":7949,\"acctstarttime\":1452672578,\"acctstoptime\":1452680527,\"acctterminatecause\":6,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162820264558,\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":977560,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":899,\"refer_ipv4\":45,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"user_id\":13455564690171,\"user_ipv4\":2887404728,\"user_ipv6\":\"2001:250:5002:8000::2:e429\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":1043911,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":16852213,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14526536909578\",\"acctsessiontime\":5178,\"acctstarttime\":1452653645,\"acctstoptime\":1452658823,\"acctterminatecause\":2,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162884395340,\"area_name\":\"XL_电子楼Wlan\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":948657,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":4030,\"refer_ipv4\":29,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3421,\"user_id\":13455564690171,\"user_ipv4\":2887366666,\"user_ipv6\":\"::\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":4173203,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":50104061,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14526526912516\",\"acctsessiontime\":1060,\"acctstarttime\":1452652647,\"acctstoptime\":1452653707,\"acctterminatecause\":6,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162884395340,\"area_name\":\"XL_电子楼Wlan\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":941226,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":4030,\"refer_ipv4\":6,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3421,\"user_id\":13455564690171,\"user_ipv4\":2887366284,\"user_ipv6\":\"::\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":13843454,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":248446590,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14526526021056\",\"acctsessiontime\":130,\"acctstarttime\":1452652557,\"acctstoptime\":1452652687,\"acctterminatecause\":6,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162884395340,\"area_name\":\"XL_电子楼Wlan\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":940228,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":4030,\"refer_ipv4\":1,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3421,\"user_id\":13455564690171,\"user_ipv4\":2887366666,\"user_ipv6\":\"::\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":337066,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":3771239,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14526524630752\",\"acctsessiontime\":147,\"acctstarttime\":1452652418,\"acctstoptime\":1452652565,\"acctterminatecause\":6,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162884395340,\"area_name\":\"XL_电子楼Wlan\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":940109,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":4030,\"refer_ipv4\":1,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3421,\"user_id\":13455564690171,\"user_ipv4\":2887366284,\"user_ipv6\":\"::\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":2459128,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":42419705,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14526523370375\",\"acctsessiontime\":154,\"acctstarttime\":1452652292,\"acctstoptime\":1452652446,\"acctterminatecause\":6,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162884395340,\"area_name\":\"XL_电子楼Wlan\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":939999,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":4030,\"refer_ipv4\":1,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3421,\"user_id\":13455564690171,\"user_ipv4\":2887366666,\"user_ipv6\":\"::\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":985116,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":7134133,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14526507982248\",\"acctsessiontime\":566,\"acctstarttime\":1452650752,\"acctstoptime\":1452651318,\"acctterminatecause\":2,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162903201606,\"area_name\":\"XL_食堂Wlan\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":938861,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":4030,\"refer_ipv4\":4,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 9/0/40\",\"svlan\":3432,\"user_id\":13455564690171,\"user_ipv4\":2887365152,\"user_ipv6\":\"::\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":1340975,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":5295974,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14526491870737\",\"acctsessiontime\":1436,\"acctstarttime\":1452649143,\"acctstoptime\":1452650579,\"acctterminatecause\":2,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162820264558,\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":938021,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":899,\"refer_ipv4\":8,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"user_id\":13455564690171,\"user_ipv4\":2887404728,\"user_ipv6\":\"2001:250:5002:8000::2:e429\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":2864969,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":78648948,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14526490937446\",\"acctsessiontime\":98,\"acctstarttime\":1452649048,\"acctstoptime\":1452649146,\"acctterminatecause\":6,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162820264558,\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":936571,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":899,\"refer_ipv4\":1,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"user_id\":13455564690171,\"user_ipv4\":2887417215,\"user_ipv6\":\"2001:250:5002:8000::3:300\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":152153,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":926445,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14526490201844\",\"acctsessiontime\":110,\"acctstarttime\":1452648976,\"acctstoptime\":1452649086,\"acctterminatecause\":6,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162820264558,\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":936530,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":899,\"refer_ipv4\":1,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"user_id\":13455564690171,\"user_ipv4\":2887404728,\"user_ipv6\":\"2001:250:5002:8000::2:e429\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":1073213,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":21094793,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14526162146801\",\"acctsessiontime\":32857,\"acctstarttime\":1452616170,\"acctstoptime\":1452649027,\"acctterminatecause\":6,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162820264558,\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":936468,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":899,\"refer_ipv4\":183,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"user_id\":13455564690171,\"user_ipv4\":2887417215,\"user_ipv6\":\"2001:250:5002:8000::3:300\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":6893414,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":74074502,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14526160916014\",\"acctsessiontime\":116,\"acctstarttime\":1452616046,\"acctstoptime\":1452616162,\"acctterminatecause\":2,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162820264558,\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":926436,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":913,\"refer_ipv4\":1,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"user_id\":13455564690171,\"user_ipv4\":2887417215,\"user_ipv6\":\"2001:250:5002:8000::3:2ee\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":140981,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":1337808,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14526148712047\",\"acctsessiontime\":1192,\"acctstarttime\":1452614827,\"acctstoptime\":1452616019,\"acctterminatecause\":2,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162820264558,\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":926291,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":899,\"refer_ipv4\":7,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"user_id\":13455564690171,\"user_ipv4\":2887417215,\"user_ipv6\":\"2001:250:5002:8000::eb73\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":1771267,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":71458889,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14525899373404\",\"acctsessiontime\":271,\"acctstarttime\":1452589893,\"acctstoptime\":1452590164,\"acctterminatecause\":2,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162820264558,\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":894068,\"mac\":\"[敏感信息]\",\"nas_ip\":3688591887,\"pvlan\":899,\"refer_ipv4\":2,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"user_id\":13455564690171,\"user_ipv4\":2887417215,\"user_ipv6\":\"2001:250:5002:8000::eb73\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":8491,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":62639,\"acctoutputoctets_ipv6\":0},{\"acctsessionid\":\"14525785084660\",\"acctsessiontime\":10850,\"acctstarttime\":1452578464,\"acctstoptime\":1452589314,\"acctterminatecause\":2,\"amount_ipv4\":0,\"amount_ipv6\":0,\"ap_id\":\"(null)\",\"area_id\":14162820264558,\"area_name\":\"XL_1组团1舍\",\"area_type\":3,\"fullname\":\"姓名隐藏\",\"id\":892770,\"mac\":\"9c:4e:36:5c:4d:d4\",\"nas_ip\":3688591887,\"pvlan\":899,\"refer_ipv4\":61,\"refer_ipv6\":0,\"service_id\":13455362142011,\"service_name\":\"学生标准计时服务\",\"src_ip\":0,\"subport\":\"trunk 8/0/20\",\"svlan\":3206,\"user_id\":13455564690171,\"user_ipv4\":2887405627,\"user_ipv6\":\"2001:250:5002:8000::3:a28e\",\"username\":\"账号隐藏\",\"acctinputoctets_ipv4\":32286911189,\"acctinputoctets_ipv6\":0,\"acctoutputoctets_ipv4\":1339040294,\"acctoutputoctets_ipv6\":0}],\"reply_code\":0}";
            offlineTestingContent[(int)Pages.GetBill] =
                "{\"total\":41,\"reply_msg\":\"操作成功\",\"rows\":[{\"id\":14515837821192,\"account_no\":\"120821214109017\",\"recharge_amount\":0,\"beginning_balance\":40722,\"ending_balance\":20722,\"costs_amount\":20000,\"startdate\":1448899200,\"enddate\":1451577599,\"createtime\":1451583782},{\"id\":14489536427444,\"account_no\":\"120821214109017\",\"recharge_amount\":0,\"beginning_balance\":60722,\"ending_balance\":40722,\"costs_amount\":20000,\"startdate\":1446307200,\"enddate\":1448899199,\"createtime\":1448953642},{\"id\":14463090626178,\"account_no\":\"120821214109017\",\"recharge_amount\":0,\"beginning_balance\":80722,\"ending_balance\":60722,\"costs_amount\":20000,\"startdate\":1443628800,\"enddate\":1446307199,\"createtime\":1446309062},{\"id\":14436304261146,\"account_no\":\"120821214109017\",\"recharge_amount\":100000,\"beginning_balance\":722,\"ending_balance\":80722,\"costs_amount\":20000,\"startdate\":1441036800,\"enddate\":1443628799,\"createtime\":1443630426},{\"id\":14410417771163,\"account_no\":\"120821214109017\",\"recharge_amount\":20000,\"beginning_balance\":722,\"ending_balance\":722,\"costs_amount\":20000,\"startdate\":1438358400,\"enddate\":1441036799,\"createtime\":1441041777},{\"id\":14386445778023,\"account_no\":\"120821214109017\",\"recharge_amount\":0,\"beginning_balance\":20722,\"ending_balance\":722,\"costs_amount\":20000,\"startdate\":1435680000,\"enddate\":1438358399,\"createtime\":1438644578},{\"id\":14356816456639,\"account_no\":\"120821214109017\",\"recharge_amount\":0,\"beginning_balance\":40722,\"ending_balance\":20722,\"costs_amount\":20000,\"startdate\":1433088000,\"enddate\":1435679999,\"createtime\":1435681645},{\"id\":14330925321993,\"account_no\":\"120821214109017\",\"recharge_amount\":0,\"beginning_balance\":60722,\"ending_balance\":40722,\"costs_amount\":20000,\"startdate\":1430409600,\"enddate\":1433087999,\"createtime\":1433092532},{\"id\":14304137556332,\"account_no\":\"120821214109017\",\"recharge_amount\":0,\"beginning_balance\":80722,\"ending_balance\":60722,\"costs_amount\":20000,\"startdate\":1427817600,\"enddate\":1430409599,\"createtime\":1430413755},{\"id\":14278204140240,\"account_no\":\"120821214109017\",\"recharge_amount\":0,\"beginning_balance\":100722,\"ending_balance\":80722,\"costs_amount\":20000,\"startdate\":1425139200,\"enddate\":1427817599,\"createtime\":1427820414},{\"id\":14251412192169,\"account_no\":\"120821214109017\",\"recharge_amount\":100000,\"beginning_balance\":722,\"ending_balance\":100722,\"costs_amount\":0,\"startdate\":1422720000,\"enddate\":1425139199,\"createtime\":1425141219},{\"id\":14227236311118,\"account_no\":\"120821214109017\",\"recharge_amount\":0,\"beginning_balance\":20722,\"ending_balance\":722,\"costs_amount\":20000,\"startdate\":1420041600,\"enddate\":1422719999,\"createtime\":1422723631},{\"id\":14200460258661,\"account_no\":\"120821214109017\",\"recharge_amount\":20000,\"beginning_balance\":20722,\"ending_balance\":20722,\"costs_amount\":20000,\"startdate\":1417363200,\"enddate\":1420041599,\"createtime\":1420046025},{\"id\":14173658382558,\"account_no\":\"120821214109017\",\"recharge_amount\":0,\"beginning_balance\":40722,\"ending_balance\":20722,\"costs_amount\":20000,\"startdate\":1414771200,\"enddate\":1417363199,\"createtime\":1417365838},{\"id\":14147756871340,\"account_no\":\"120821214109017\",\"recharge_amount\":0,\"beginning_balance\":60722,\"ending_balance\":40722,\"costs_amount\":20000,\"startdate\":1412092800,\"enddate\":1414771199,\"createtime\":1414775687}],\"reply_code\":0}";
            offlineTestingContent[(int)Pages.GetCharge] =
                "{\"total\":29,\"reply_msg\":\"操作成功\",\"rows\":[{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":100000,\"id\":14410661926848,\"oper_id\":100,\"oper_time\":1441066192,\"remark\":\"一卡通自助充值.一卡通流水号:20150901001132\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":20000,\"id\":14384031726623,\"oper_id\":100,\"oper_time\":1438403172,\"remark\":\"一卡通自助充值.一卡通流水号:20150801001193\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":100000,\"id\":14251274889905,\"oper_id\":100,\"oper_time\":1425127489,\"remark\":\"一卡通自助充值.一卡通流水号:20150228005265\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":20000,\"id\":14182929763650,\"oper_id\":100,\"oper_time\":1418292976,\"remark\":\"一卡通自助充值.一卡通流水号:20141211006501\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":61000,\"id\":14095781974749,\"oper_id\":100,\"oper_time\":1409578197,\"remark\":\"一卡通自助充值.一卡通流水号:20140901013747\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":23000,\"id\":14093719951817,\"oper_id\":100,\"oper_time\":1409371995,\"remark\":\"一卡通自助充值.一卡通流水号:20140830005552\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":21000,\"id\":14044661947541,\"oper_id\":100,\"oper_time\":1404466194,\"remark\":\"一卡通自助充值.一卡通流水号:20140704004740\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":40000,\"id\":13944542909063,\"oper_id\":100,\"oper_time\":1394454291,\"remark\":\"一卡通自助充值.一卡通流水号:20140310007293\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":60000,\"id\":13925445115640,\"oper_id\":100,\"oper_time\":1392544511,\"remark\":\"一卡通自助充值.一卡通流水号:20140216006244\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":20000,\"id\":13888298073092,\"oper_id\":100,\"oper_time\":1388829807,\"remark\":\"一卡通自助充值.一卡通流水号:20140104004980\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":20000,\"id\":13860674261838,\"oper_id\":100,\"oper_time\":1386067426,\"remark\":\"一卡通自助充值.一卡通流水号:20131203007640\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":17000,\"id\":13834713028274,\"oper_id\":100,\"oper_time\":1383471303,\"remark\":\"一卡通自助充值.一卡通流水号:20131103004050\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":21000,\"id\":13811898079070,\"oper_id\":100,\"oper_time\":1381189808,\"remark\":\"一卡通自助充值.一卡通流水号:20131008000278\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":3000,\"id\":13792053002989,\"oper_id\":100,\"oper_time\":1379205300,\"remark\":\"一卡通自助充值.一卡通流水号:20130915000309\"},{\"oper_username\":\"一卡通充值\",\"account_no\":\"120821214109017\",\"amount\":15000,\"id\":13784265114639,\"oper_id\":100,\"oper_time\":1378426511,\"remark\":\"一卡通自助充值.一卡通流水号:20130906000325\"}],\"reply_code\":0}";

        }
        private async Task InitTestingContent(StorageFile ContentFile)
        {
            IList<string> Content = await FileIO.ReadLinesAsync(ContentFile);

            offlineTestingContent = new string[(int)Pages.TopNum];
            foreach (var singleLine in Content)
            {
                int IndexFound = -1, start = 0;
                string Name = "";
                for (int i = 0; i < (int)Pages.TopNum; ++i)
                {
                    Name = ((Pages)i).ToString() + ":";
                    start = singleLine.IndexOf(Name);
                    if (start != -1)
                    { IndexFound = i; break; }
                }
                if (IndexFound == -1) continue;
                //找到内容
                start += Name.Length;
                offlineTestingContent[IndexFound] = singleLine.Substring(start, singleLine.LastIndexOf('}') + 1 - start);
            }
        }
        #endregion
        #endregion
        #region Post方法
        public override async Task<string> PostToUrl_WithPagesSelector(Pages PageIndex, string _username = "", string _password = "")
        {
            int SiteSelector = (int)PageIndex;
            await Task.Delay(500);//延时0.5s 保证返回类型一致 有网络延迟感
            return offlineTestingContent[SiteSelector];
        }
        #endregion

    }
}
