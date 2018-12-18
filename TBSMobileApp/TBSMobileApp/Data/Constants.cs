using Plugin.DeviceInfo;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace TBSMobileApp.Data
{
    public class Constants
    {
        public static string hostname = "Localhost";
        public static string database = "Demo";
        
        public static string server_ip = "192.168.1.17";

        public static string deviceID = CrossDeviceInfo.Current.Id;
        public static string appversion = "Version: " + VersionTracking.CurrentVersion;
        public static string email = "lawrenceagulto.317@gmail.com";

        public static string requestUrl = ":7777/TBSMobileApplication/mobile-api.php?";
    }
}
