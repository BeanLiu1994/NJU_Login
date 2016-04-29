using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace LoggingSystem
{
    public class NetworkCheck
    {
        public static bool IsWwanConnectionNow()
        {
            return NetworkInformation.GetInternetConnectionProfile().IsWwanConnectionProfile;
        }
        public static bool IsWlanConnectionNow()
        {
            return NetworkInformation.GetInternetConnectionProfile().IsWlanConnectionProfile;
        }
    }
}
