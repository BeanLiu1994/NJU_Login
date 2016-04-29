using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace LoggingSystem
{
    public static class ImageExtensions
    {
        /// <summary>
        /// 通过NET获取网络图片
        /// </summary>
        /// <param name="url">要访问的图片所在网址</param>
        /// <param name="requestAction">对于WebRequest需要进行的一些处理，比如代理、密码之类</param>
        /// <param name="responseFunc">如何从WebResponse中获取到图片</param>
        /// <returns></returns>
        public static async Task<UIElement> GetImageFromNet(this Uri url)
        {
            try
            {
                if (NetworkCheck.IsWwanConnectionNow())
                    throw new Exception("WwanNetworkNow");
                Image img = new Image();
                // 扩展名必须匹配 FileOpenPicker.FileTypeFilter 中的定义
                StorageFile file = await StorageFile.CreateStreamedFileFromUriAsync("mvp.gif", url, RandomAccessStreamReference.CreateFromUri(url));
                var source = new BitmapImage();
                source.SetSource(await file.OpenAsync(FileAccessMode.Read, StorageOpenOptions.None));
                img.Source = source;
                img.MaxHeight = 300;
                return img;
            }
            catch
            {
                var FailInfo = new ImageNotFound();
                FailInfo.InitializeComponent();
                return FailInfo;
            }
        }
    }
    public class AdditionalMessageFetcher
    {
        public AdditionalMessageFetcher()
        {
            DestURL = NameManager.HTTPServerAddrPrefix +"/AdditionalMessage";
        }
        protected string DestURL;
        protected string result = "";
        public AdditionalMessageContainer result_Analysed;
        protected async virtual Task<bool> GetMessage(string Uname)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(1.5));//设置延时时间1.5s
            try
            {
                HttpClient myHC = new HttpClient();
                HttpResponseMessage response = await myHC.PostAsync(new Uri(DestURL), null, cts.Token);
                result = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException e)
            {
                Debug.WriteLine("连接超时");
                Debug.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine("exception!");
                Debug.WriteLine(e.Message);
            }
            return (result!="");
        }
        public virtual async Task<bool> Run(string Uname)
        {
            result_Analysed = new AdditionalMessageContainer() { Content = "", Title = "Fail", url = null };
            if (Uname != null && Uname == "") return false;
            bool Hresult = await GetMessage(Uname);
            if (Hresult) Hresult = AnalyseContent();
            if (Hresult && CheckUser(Uname)) SendMessage();
            return Hresult && CheckUser(Uname);
        }
        protected virtual bool AnalyseContent()
        {
            var serializer = new DataContractJsonSerializer(typeof(AdditionalMessageContainer));
            var mStream = new MemoryStream(Encoding.UTF8.GetBytes(result));
            try
            {
                result_Analysed = (AdditionalMessageContainer)serializer.ReadObject(mStream);
            }
            catch(Exception e)
            {
                Debug.WriteLine("exception!");
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }
        protected virtual bool CheckUser(string Uname)
        {
            return true;
        }
        protected virtual void SendMessage()
        {
            Debug.WriteLine("收到通知:  " + result_Analysed.Title + "  " + result_Analysed.Content);
        }
        [DataContract(Namespace = "http://p.nju.edu.cn")]
        public class AdditionalMessageContainer
        {
            [DataMember(Order = 0)]
            public string Title { get; set; }
            [DataMember(Order = 1)]
            public string Content { get; set; }
            [DataMember(Order = 2)]
            public string url { get; set; }
        }
    }
    public class UsageSurvey : AdditionalMessageFetcher
    {
        public UsageSurvey()
        {
            DestURL = NameManager.HTTPServerAddrPrefix + "/cgi-bin/UsageCGI.py";
        }
        protected async override Task<bool> GetMessage(string Uname)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(1.5));//设置延时时间1.5s
            try
            {
                HttpClient myHC = new HttpClient();
                HttpResponseMessage response = await myHC.PostAsync(new Uri(DestURL + "?username=" + Uname), null, cts.Token);
                result = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException e)
            {
                Debug.WriteLine("连接超时");
                Debug.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine("exception!");
                Debug.WriteLine(e.Message);
            }
            return (result != "");
        }
        public override async Task<bool> Run(string Uname)
        {
            result_Analysed = new AdditionalMessageContainer() { Content = "", Title = "Fail", url = null };
            bool Hresult = await GetMessage(Uname);
            return Hresult;
        }
    }
    public class EasterEggMessageFetcher:AdditionalMessageFetcher
    {
        public EasterEggMessageFetcher()
        {
            DestURL = NameManager.HTTPServerAddrPrefix + "/cgi-bin/EasterEggMessageCGI.py";
        }
        protected async override Task<bool> GetMessage(string Uname)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(1.5));//设置延时时间1.5s
            try
            {
                HttpClient myHC = new HttpClient();
                HttpResponseMessage response = await myHC.PostAsync(new Uri(DestURL + "?username=" + Uname), null, cts.Token);
                result = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException e)
            {
                Debug.WriteLine("连接超时");
                Debug.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine("exception!");
                Debug.WriteLine(e.Message);
            }
            return (result != "");
        }
    }
}
