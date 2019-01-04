using Plugin.DeviceInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBSMobileApp.Data;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TBSMobileApp.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
	{
		public LoginPage ()
		{
			InitializeComponent ();
            Init();
        }

        void Init()
        {
            entHost.Text = Constants.hostname;
            entDatabase.Text = Constants.database;
            entIPAddress.Text = Constants.server_ip;
            entUser.Completed += (s, e) => entPassword.Focus();
            entPassword.Completed += (s, e) => Login();
            lblVersion.Text = Constants.appversion;
            lblRegistrationCode.Text = "Device ID: " + CrossDeviceInfo.Current.Id;
        }

        private void entIPAddress_Focused(object sender, FocusEventArgs e)
        {
            ipaddressFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void entIPAddress_Unfocused(object sender, FocusEventArgs e)
        {

        }

        private void entHost_Focused(object sender, FocusEventArgs e)
        {
            hostFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void entHost_Unfocused(object sender, FocusEventArgs e)
        {

        }

        private void entDatabase_Focused(object sender, FocusEventArgs e)
        {
            databaseFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void entDatabase_Unfocused(object sender, FocusEventArgs e)
        {

        }

        private void entUser_Focused(object sender, FocusEventArgs e)
        {
            usernameFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void entUser_Unfocused(object sender, FocusEventArgs e)
        {

        }

        private void entPassword_Focused(object sender, FocusEventArgs e)
        {
            passwordFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void entPassword_Unfocused(object sender, FocusEventArgs e)
        {

        }

        private void btnConnect_Clicked(object sender, EventArgs e)
        {

        }

        private void btnLogin_Clicked(object sender, EventArgs e)
        {

        }

        private void BtnChange_Clicked(object sender, EventArgs e)
        {

        }
    }
}