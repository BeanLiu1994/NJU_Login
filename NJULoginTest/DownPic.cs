using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LoggingSystem;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace NJULoginTest
{
    //http://lab.dobyi.com/api/bing.php 也是别人做的一个接口
    //http://www.dujin.org/sys/bing/1920.php 这是别人做的一个接口api

    public class PictureInfo : LoggingSystem.DataFetcher
    {
        private const string url = NameManager.HTTPServerAddrPrefix + "/cgi-bin/BingImageCGI.py";
        private const string url_bak = "http://www.dujin.org/sys/bing/1920.php";
        private string RecentInfo;
        public DataType_ShowInfo PreparedData { get; private set; }
        public PictureInfo()
        {
            PreparedData = new DataType_ShowInfo();
        }
        public async Task<DataType_ShowInfo> RunSession()
        {
            RecentInfo = await PostToUrl(url);
            if (RecentInfo == "")
                RecentInfo = await PostToUrl(url_bak);
            bool Connectivity = HandleRecentInfo();
            return GetPicInfo();
        }
        protected bool HandleRecentInfo()
        {
            if (RecentInfo.Length == 0)
            {
                PicInfo = null;
                Debug.WriteLine("PicInfo -- NoNetwork");
                return false;
            }
            else
            {
                //如果有返回信息
                var serializer = new DataContractJsonSerializer(typeof(PicUrlConfig));
                var mStream = new MemoryStream(Encoding.UTF8.GetBytes(RecentInfo));
                try
                {
                    PicInfo = (PicUrlConfig)serializer.ReadObject(mStream);
                }
                catch
                {
                    PicInfo = null;
                    Debug.WriteLine("PicInfo -- FormatError");
                    return false;
                }

                return true;
            }
        }

        public DataType_ShowInfo GetPicInfo()
        {
            if (PicInfo != null)
            {
                if (PicInfo.copyrightinfo == null) PicInfo.copyrightinfo = "© 2016 Microsoft";
                PreparedData.Content = PicInfo.copyrightinfo + "\r\n\r\n" + PicInfo.desc;
                PreparedData.Title = PicInfo.title;
                PreparedData.Url = PicInfo.url;
                return PreparedData;
            }
            else return null;
        }
        
        private PicUrlConfig PicInfo;
        #region PicUrlConfig Contract
        // Volume Infomation
        [DataContract(Namespace = "http://p.nju.edu.cn")]
        private class PicUrlConfig
        {
            [DataMember(Order = 0)]
            public string title { get; set; }
            [DataMember(Order = 1)]
            public string desc { get; set; }
            [DataMember(Order = 2)]
            public string url { get; set; }
            [DataMember(Order = 3)]
            public string copyrightinfo { get; set; }
        }
        #endregion
    }
}
