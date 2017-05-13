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
            var Profile = NetworkInformation.GetInternetConnectionProfile();
            return Profile != null && Profile.IsWwanConnectionProfile;
        }
        public static bool IsWlanConnectionNow()
        {
            var Profile = NetworkInformation.GetInternetConnectionProfile();
            return Profile != null && Profile.IsWlanConnectionProfile;
        }
    }
}
