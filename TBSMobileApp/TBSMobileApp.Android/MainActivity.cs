using System;

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android;
using Android.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;

namespace TBSMobileApp.Droid
{
    [Activity(Label = "TBSMobileApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            AppCenter.Start("db6315f6-be53-46f8-bf99-4e384ee1e0f7", typeof(Analytics), typeof(Crashes), typeof(Distribute));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != (int)Permission.Granted)
                {
                    RequestPermissions(new string[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.WakeLock, Manifest.Permission.AccessFineLocation, Manifest.Permission.Camera, Manifest.Permission.WriteExternalStorage, Manifest.Permission.ReadExternalStorage, Manifest.Permission.LocationHardware, Manifest.Permission.AccessMockLocation, Manifest.Permission.CaptureVideoOutput, Manifest.Permission.CaptureSecureVideoOutput, Manifest.Permission.AccessNetworkState, Manifest.Permission.AccessWifiState, Manifest.Permission.RequestInstallPackages }, 0);
                }
            }
        }
    }
}

