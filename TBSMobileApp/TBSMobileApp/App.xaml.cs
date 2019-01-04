using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using System;
using System.Threading.Tasks;
using TBSMobileApp.View;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace TBSMobileApp
{
	public partial class App : Application
	{
		public App ()
		{
			InitializeComponent();

            MainPage = new NavigationPage(new LoginPage());
        }

		protected override void OnStart ()
		{
            AppCenter.Start("android=db6315f6-be53-46f8-bf99-4e384ee1e0f7;", typeof(Analytics), typeof(Crashes), typeof(Distribute));
            Analytics.SetEnabledAsync(true);

            Distribute.ReleaseAvailable = OnReleaseAvailable;

            bool OnReleaseAvailable(ReleaseDetails releaseDetails)
            {
                string versionName = releaseDetails.ShortVersion;
                string versionCodeOrBuildNumber = releaseDetails.Version;
                string releaseNotes = releaseDetails.ReleaseNotes;
                Uri releaseNotesUrl = releaseDetails.ReleaseNotesUrl;

                var title = "Version " + versionName + " available!";
                Task answer;

                if (releaseDetails.MandatoryUpdate)
                {
                    answer = Current.MainPage.DisplayAlert(title, releaseNotes, "Download and Install");
                }
                else
                {
                    answer = Current.MainPage.DisplayAlert(title, releaseNotes, "Download and Install", "Ask Later");
                }
                answer.ContinueWith((task) =>
                {
                    if (releaseDetails.MandatoryUpdate || (task as Task<bool>).Result)
                    {
                        Distribute.NotifyUpdateAction(UpdateAction.Update);
                    }
                    else
                    {
                        Distribute.NotifyUpdateAction(UpdateAction.Postpone);
                    }
                });

                return true;
            }
        }

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
