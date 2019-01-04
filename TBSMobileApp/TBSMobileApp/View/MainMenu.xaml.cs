using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TBSMobileApp.Data;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TBSMobileApp.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainMenu : ContentPage
	{
        string contact;
        string host;
        string database;
        string ipaddress;
        byte[] pingipaddress;

        public MainMenu (string host, string database, string contact, string ipaddress, byte[] pingipaddress)
		{
			InitializeComponent ();
            this.contact = contact;
            this.host = host;
            this.database = database;
            this.ipaddress = ipaddress;
            this.pingipaddress = pingipaddress;
            CheckConnectionContinuously();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            var appdate = Preferences.Get("appdatetime", String.Empty, "private_prefs");

            if (string.IsNullOrEmpty(appdate))
            {
                Preferences.Set("appdatetime", DateTime.Now.ToString(), "private_prefs");
            }
            else
            {
                try
                {
                    if (DateTime.Now >= DateTime.Parse(Preferences.Get("appdatetime", String.Empty, "private_prefs")))
                    {
                        Preferences.Set("appdatetime", DateTime.Now.ToString(), "private_prefs");

                        if (CrossConnectivity.Current.IsConnected)
                        {
                            var ping = new Ping();
                            var reply = ping.Send(new IPAddress(pingipaddress), 5000);

                            if (reply.Status == IPStatus.Success)
                            {
                                var db = DependencyService.Get<ISQLiteDB>();
                                var conn = db.GetConnection();

                                var cafchangessql = "SELECT * FROM tblCaf WHERE EmployeeID = '" + contact + "' AND LastUpdated > LastSync AND Deleted != '1'";
                                var getcafchanges = conn.QueryAsync<CAFTable>(cafchangessql);
                                var cafchangesresultCount = getcafchanges.Result.Count;

                                if (cafchangesresultCount > 0)
                                {
                                    var optimalSpeed = 300000;
                                    var connectionTypes = CrossConnectivity.Current.ConnectionTypes;
                                    var speeds = CrossConnectivity.Current.Bandwidths;

                                    if (connectionTypes.Any(speed => Convert.ToInt32(speed) < optimalSpeed))
                                    {
                                        lblStatus.Text = "Initializing data sync";
                                        lblStatus.BackgroundColor = Color.FromHex("#27ae60");

                                        var confirm = await DisplayAlert("Auto-sync Connection Warning", "Slow connection detected. Do you want to sync the data?", "Yes", "No");
                                        if (confirm == true)
                                        {
                                            SyncCaf(host, database, contact, ipaddress, pingipaddress);
                                            btnFAF.IsEnabled = false;
                                            btnAH.IsEnabled = false;
                                            btnLogout.IsEnabled = false;
                                            btnUI.IsEnabled = false;
                                        }
                                        else
                                        {
                                            lblStatus.Text = "Online - Connected to server";
                                            lblStatus.BackgroundColor = Color.FromHex("#2ecc71");
                                            btnFAF.IsEnabled = true;
                                            btnAH.IsEnabled = true;
                                            btnLogout.IsEnabled = true;
                                            btnUI.IsEnabled = true;
                                        }
                                    }
                                    else
                                    {
                                        SyncCaf(host, database, contact, ipaddress, pingipaddress);
                                        btnFAF.IsEnabled = false;
                                        btnAH.IsEnabled = false;
                                        btnLogout.IsEnabled = false;
                                        btnUI.IsEnabled = false;
                                    }
                                }
                                else
                                {
                                    lblStatus.Text = "Online - Connected to server";
                                    lblStatus.BackgroundColor = Color.FromHex("#2ecc71");
                                    btnFAF.IsEnabled = true;
                                    btnAH.IsEnabled = true;
                                    btnLogout.IsEnabled = true;
                                    btnUI.IsEnabled = true;
                                }
                            }
                            else
                            {
                                lblStatus.Text = "Online - Server unreachable.";
                                lblStatus.BackgroundColor = Color.FromHex("#e67e22");
                                btnFAF.IsEnabled = true;
                                btnAH.IsEnabled = true;
                                btnLogout.IsEnabled = true;
                                btnUI.IsEnabled = true;
                            }
                        }
                        else
                        {
                            lblStatus.Text = "Offline - Connect to internet";
                            lblStatus.BackgroundColor = Color.FromHex("#e74c3c");
                            btnFAF.IsEnabled = true;
                            btnAH.IsEnabled = true;
                            btnLogout.IsEnabled = true;
                            btnUI.IsEnabled = true;
                        }
                    }
                    else
                    {
                        await DisplayAlert("Application Error", "It appears you change the time/date of your phone. Please restore the correct time/date", "Got it");
                        await Navigation.PopToRootAsync();
                    }
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            }
        }

        public class CAFData
        {
            public string CAFNo { get; set; }
            public string EmployeeID { get; set; }
            public string ContactPersonID { get; set; }
            public DateTime CAFDate { get; set; }
            public string CustomerID { get; set; }
            public string ActivityID { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public float Breakfast { get; set; }
            public float Lunch { get; set; }
            public float Dinner { get; set; }
            public float HotelAccommodation { get; set; }
            public float TransportationFare { get; set; }
            public float CashAdvance { get; set; }
            public string Remarks { get; set; }
            public string OtherConcern { get; set; }
            public string Signature { get; set; }
            public string MobileSignature { get; set; }
            public DateTime LastSync { get; set; }
            public DateTime LastUpdated { get; set; }
            public int Deleted { get; set; }
        }

        public async void SyncCaf(string host, string database, string contact, string ipaddress, byte[] pingipaddress)
        {
            var ping = new Ping();
            var reply = ping.Send(new IPAddress(pingipaddress), 5000);

            if (reply.Status == IPStatus.Success)
            {
                try
                {
                    var db = DependencyService.Get<ISQLiteDB>();
                    var conn = db.GetConnection();

                    var sql = "SELECT * FROM tblCaf WHERE EmployeeID = '" + contact + "'";
                    var getCAF = conn.QueryAsync<CAFTable>(sql);
                    var resultCount = getCAF.Result.Count;
                    var current_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (resultCount > 0)
                    {
                        var changessql = "SELECT * FROM tblCaf WHERE EmployeeID = '" + contact + "' AND LastUpdated > LastSync AND Deleted != '1'";
                        var getCAFChanges = conn.QueryAsync<CAFTable>(changessql);
                        var changesresultCount = getCAFChanges.Result.Count;

                        if (changesresultCount > 0)
                        {
                            for (int i = 0; i < changesresultCount; i++)
                            {
                                try
                                {
                                    lblStatus.Text = "Sending customer activity data to server " + (i + 1) + " out of " + changesresultCount;

                                    var crresult = getCAFChanges.Result[i];
                                    var crcafNo = crresult.CAFNo;
                                    var cremployeeID = crresult.EmployeeID;
                                    var crcontactPerson = crresult.ContactPersonID;
                                    var crcafDate = crresult.CAFDate;
                                    var crcustomerID = crresult.CustomerID;
                                    var cractivtyID = crresult.ActivityID;
                                    var crstartTime = crresult.StartTime;
                                    var crendTime = crresult.EndTime;
                                    var crbfast = crresult.Breakfast;
                                    var crlnch = crresult.Lunch;
                                    var crdnnr = crresult.Dinner;
                                    var crhotelAccommodation = crresult.HotelAccommodation;
                                    var crtransportation = crresult.TransportationFare;
                                    var crcashAdvance = crresult.CashAdvance;
                                    var crremarks = crresult.Remarks;
                                    var crotherConcern = crresult.OtherConcern;
                                    var crsignature = crresult.Signature;
                                    var crmobileSignature = crresult.MobileSignature;
                                    var crlastSync = DateTime.Parse(current_datetime);
                                    var crlastUpdated = crresult.LastUpdated;
                                    var crdeleted = crresult.Deleted;

                                    var crlink = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Request=k5N7PE";
                                    string crcontentType = "application/json";
                                    JObject crjson = new JObject
                                    {
                                        { "CAFNo", crcafNo },
                                        { "EmployeeID", cremployeeID },
                                        { "ContactPerson", crcontactPerson },
                                        { "CAFDate", crcafDate },
                                        { "CustomerID", crcustomerID },
                                        { "ActivityID", cractivtyID },
                                        { "StartTime", crstartTime },
                                        { "EndTime", crendTime },
                                        { "Breakfast", crbfast },
                                        { "Lunch", crlnch },
                                        { "Dinner", crdnnr },
                                        { "Hotel", crhotelAccommodation },
                                        { "Transportation", crtransportation },
                                        { "CashAdvance", crcashAdvance },
                                        { "MobileSignature", crmobileSignature },
                                        { "Remarks", crremarks },
                                        { "OtherConcern", crotherConcern },
                                        { "Deleted", crdeleted },
                                        { "LastUpdated", crlastUpdated }
                                    };

                                    HttpClient crclient = new HttpClient();
                                    var crresponse = await crclient.PostAsync(crlink, new StringContent(crjson.ToString(), Encoding.UTF8, crcontentType));

                                    if (crresponse.IsSuccessStatusCode)
                                    {
                                        byte[] crSignatureData = File.ReadAllBytes(crsignature);

                                        var siglink = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Contact=" + contact + "&Request=N4f5GL";

                                        string sigcontentType = "application/json";
                                        JObject sigjson = new JObject
                                        {
                                            { "CAFNo", crcafNo },
                                            { "CAFDate", crcafDate },
                                            { "Signature", crSignatureData }
                                        };

                                        HttpClient sigclient = new HttpClient();
                                        var sigresponse = await sigclient.PostAsync(siglink, new StringContent(sigjson.ToString(), Encoding.UTF8, sigcontentType));

                                        if (sigresponse.IsSuccessStatusCode)
                                        {
                                            await conn.QueryAsync<CAFTable>("UPDATE tblCaf SET LastSync = ? WHERE CAFNo = ?", DateTime.Parse(current_datetime), crcafNo);
                                            OnSyncComplete();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Crashes.TrackError(ex);
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            var changessql = "SELECT * FROM tblCaf WHERE EmployeeID = '" + contact + "' AND LastUpdated > LastSync AND Deleted != '1'";
                            var getCAFChanges = conn.QueryAsync<CAFTable>(changessql);
                            var changesresultCount = getCAFChanges.Result.Count;

                            if (changesresultCount > 0)
                            {
                                for (int i = 0; i < changesresultCount; i++)
                                {
                                    try
                                    {
                                        lblStatus.Text = "Sending customer activity data to server " + (i + 1) + " out of " + changesresultCount;

                                        var crresult = getCAFChanges.Result[i];
                                        var crcafNo = crresult.CAFNo;
                                        var cremployeeID = crresult.EmployeeID;
                                        var crcontactPerson = crresult.ContactPersonID;
                                        var crcafDate = crresult.CAFDate;
                                        var crcustomerID = crresult.CustomerID;
                                        var cractivtyID = crresult.ActivityID;
                                        var crstartTime = crresult.StartTime;
                                        var crendTime = crresult.EndTime;
                                        var crbfast = crresult.Breakfast;
                                        var crlnch = crresult.Lunch;
                                        var crdnnr = crresult.Dinner;
                                        var crhotelAccommodation = crresult.HotelAccommodation;
                                        var crtransportation = crresult.TransportationFare;
                                        var crcashAdvance = crresult.CashAdvance;
                                        var crremarks = crresult.Remarks;
                                        var crotherConcern = crresult.OtherConcern;
                                        var crsignature = crresult.Signature;
                                        var crmobileSignature = crresult.MobileSignature;
                                        var crlastSync = DateTime.Parse(current_datetime);
                                        var crlastUpdated = crresult.LastUpdated;
                                        var crdeleted = crresult.Deleted;

                                        var crlink = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Request=k5N7PE";
                                        string crcontentType = "application/json";
                                        JObject crjson = new JObject
                                        {
                                            { "CAFNo", crcafNo },
                                            { "EmployeeID", cremployeeID },
                                            { "ContactPerson", crcontactPerson },
                                            { "CAFDate", crcafDate },
                                            { "CustomerID", crcustomerID },
                                            { "ActivityID", cractivtyID },
                                            { "StartTime", crstartTime },
                                            { "EndTime", crendTime },
                                            { "Breakfast", crbfast },
                                            { "Lunch", crlnch },
                                            { "Dinner", crdnnr },
                                            { "Hotel", crhotelAccommodation },
                                            { "Transportation", crtransportation },
                                            { "CashAdvance", crcashAdvance },
                                            { "MobileSignature", crmobileSignature },
                                            { "Remarks", crremarks },
                                            { "OtherConcern", crotherConcern },
                                            { "Deleted", crdeleted },
                                            { "LastUpdated", crlastUpdated }
                                        };

                                        HttpClient crclient = new HttpClient();
                                        var crresponse = await crclient.PostAsync(crlink, new StringContent(crjson.ToString(), Encoding.UTF8, crcontentType));

                                        if (crresponse.IsSuccessStatusCode)
                                        {
                                            byte[] crSignatureData = File.ReadAllBytes(crsignature);

                                            var siglink = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Contact=" + contact + "&Request=N4f5GL";

                                            string sigcontentType = "application/json";
                                            JObject sigjson = new JObject
                                            {
                                                { "CAFNo", crcafNo },
                                                { "CAFDate", crcafDate },
                                                { "Signature", crSignatureData }
                                            };

                                            HttpClient sigclient = new HttpClient();
                                            var sigresponse = await sigclient.PostAsync(siglink, new StringContent(sigjson.ToString(), Encoding.UTF8, sigcontentType));

                                            if (sigresponse.IsSuccessStatusCode)
                                            {
                                                await conn.QueryAsync<CAFTable>("UPDATE tblCaf SET LastSync = ? WHERE CAFNo = ?", DateTime.Parse(current_datetime), crcafNo);
                                                OnSyncComplete();

                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Crashes.TrackError(ex);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Crashes.TrackError(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            }
            else
            {
                lblStatus.Text = "Syncing activity failed. Server is unreachable.";
                btnFAF.IsEnabled = true;
                btnAH.IsEnabled = true;
                btnLogout.IsEnabled = true;
                btnUI.IsEnabled = true;
            }
        }

        public void OnSyncComplete()
        {
            var ping = new Ping();
            var reply = ping.Send(new IPAddress(pingipaddress), 5000);

            if (CrossConnectivity.Current.IsConnected)
            {
                if (reply.Status == IPStatus.Success)
                {
                    DisplayAlert("Sync Completed", "Sync successfully", "Got it");
                    lblStatus.Text = "Online - Connected to server";
                    lblStatus.BackgroundColor = Color.FromHex("#2ecc71");
                }
                else
                {
                    lblStatus.Text = "Online - Server unreachable.";
                    lblStatus.BackgroundColor = Color.FromHex("#e67e22");
                }
            }
            else
            {
                lblStatus.Text = "Offline - Connect to internet";
                lblStatus.BackgroundColor = Color.FromHex("#e74c3c");
            }

            btnFAF.IsEnabled = true;
            btnAH.IsEnabled = true;
            btnLogout.IsEnabled = true;
            btnUI.IsEnabled = true;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        public void CheckConnectionContinuously()
        {
            CrossConnectivity.Current.ConnectivityChanged += async (sender, args) =>
            {
                var appdate = Preferences.Get("appdatetime", String.Empty, "private_prefs");

                if (string.IsNullOrEmpty(appdate))
                {
                    Preferences.Set("appdatetime", DateTime.Now.ToString(), "private_prefs");
                }
                else
                {
                    if (DateTime.Now >= DateTime.Parse(Preferences.Get("appdatetime", String.Empty, "private_prefs")))
                    {
                        Preferences.Set("appdatetime", DateTime.Now.ToString(), "private_prefs");

                        if (CrossConnectivity.Current.IsConnected)
                        {
                            var ping = new Ping();
                            var reply = ping.Send(new IPAddress(pingipaddress), 5000);

                            if (reply.Status == IPStatus.Success)
                            {
                                var db = DependencyService.Get<ISQLiteDB>();
                                var conn = db.GetConnection();

                                var cafchangessql = "SELECT * FROM tblCaf WHERE EmployeeID = '" + contact + "' AND LastUpdated > LastSync AND Deleted != '1'";
                                var getcafchanges = conn.QueryAsync<CAFTable>(cafchangessql);
                                var cafchangesresultCount = getcafchanges.Result.Count;

                                if (cafchangesresultCount > 0)
                                {
                                    var optimalSpeed = 300000;
                                    var connectionTypes = CrossConnectivity.Current.ConnectionTypes;

                                    if (connectionTypes.Any(speed => Convert.ToInt32(speed) < optimalSpeed))
                                    {
                                        lblStatus.Text = "Initializing data sync";
                                        lblStatus.BackgroundColor = Color.FromHex("#27ae60");

                                        var confirm = await DisplayAlert("Auto-sync Connection Speed Warning", "Slow connection detected. Do you want to sync the data?", "Yes", "No");
                                        if (confirm == true)
                                        {
                                            SyncCaf(host, database, contact, ipaddress, pingipaddress);
                                            btnFAF.IsEnabled = false;
                                            btnAH.IsEnabled = false;
                                            btnLogout.IsEnabled = false;
                                            btnUI.IsEnabled = false;
                                        }
                                        else
                                        {
                                            lblStatus.Text = "Online - Connected to server";
                                            lblStatus.BackgroundColor = Color.FromHex("#2ecc71");
                                            btnFAF.IsEnabled = true;
                                            btnAH.IsEnabled = true;
                                            btnLogout.IsEnabled = true;
                                            btnUI.IsEnabled = true;
                                        }
                                    }
                                    else
                                    {
                                        SyncCaf(host, database, contact, ipaddress, pingipaddress);
                                        btnFAF.IsEnabled = false;
                                        btnAH.IsEnabled = false;
                                        btnLogout.IsEnabled = false;
                                        btnUI.IsEnabled = false;
                                    }
                                }
                                else
                                {
                                    lblStatus.Text = "Online - Connected to server";
                                    lblStatus.BackgroundColor = Color.FromHex("#2ecc71");
                                    btnFAF.IsEnabled = true;
                                    btnAH.IsEnabled = true;
                                    btnLogout.IsEnabled = true;
                                    btnUI.IsEnabled = true;
                                }
                            }
                            else
                            {
                                lblStatus.Text = "Online - Server unreachable.";
                                lblStatus.BackgroundColor = Color.FromHex("#e67e22");
                                btnFAF.IsEnabled = true;
                                btnAH.IsEnabled = true;
                                btnLogout.IsEnabled = true;
                                btnUI.IsEnabled = true;
                            }
                        }
                        else
                        {
                            lblStatus.Text = "Offline - Connect to internet";
                            lblStatus.BackgroundColor = Color.FromHex("#e74c3c");
                            btnFAF.IsEnabled = true;
                            btnAH.IsEnabled = true;
                            btnLogout.IsEnabled = true;
                            btnUI.IsEnabled = true;
                        }
                    }
                    else
                    {
                        await DisplayAlert("Application Error", "It appears you change the time/date of your phone. Please restore the correct time/date", "Got it");
                        await Navigation.PopToRootAsync();
                    }
                }

            };
        }

        private async void btnFAF_Clicked(object sender, EventArgs e)
        {
            var appdate = Preferences.Get("appdatetime", String.Empty, "private_prefs");

            if (string.IsNullOrEmpty(appdate))
            {
                Preferences.Set("appdatetime", DateTime.Now.ToString(), "private_prefs");
            }
            else
            {
                try
                {
                    if (DateTime.Now >= DateTime.Parse(Preferences.Get("appdatetime", String.Empty, "private_prefs")))
                    {
                        Preferences.Set("appdatetime", DateTime.Now.ToString(), "private_prefs");

                        Analytics.TrackEvent("Opened Field Activity Form");
                        await Navigation.PushAsync(new CustomerActivityForm(host, database, contact, ipaddress, pingipaddress));
                    }
                    else
                    {
                        await DisplayAlert("Application Error", "It appears you change the time/date of your phone. Please restore the correct time/date", "Got it");
                        await Navigation.PopToRootAsync();
                    }
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            }
        }

        private async void btnLogout_Clicked(object sender, EventArgs e)
        {
            var appdate = Preferences.Get("appdatetime", String.Empty, "private_prefs");

            if (string.IsNullOrEmpty(appdate))
            {
                Preferences.Set("appdatetime", DateTime.Now.ToString(), "private_prefs");
            }
            else
            {
                try
                {
                    if (DateTime.Now >= DateTime.Parse(Preferences.Get("appdatetime", String.Empty, "private_prefs")))
                    {
                        Preferences.Set("appdatetime", DateTime.Now.ToString(), "private_prefs");

                        Analytics.TrackEvent("Logged Out");
                        await Application.Current.MainPage.Navigation.PopToRootAsync();
                    }
                    else
                    {
                        await DisplayAlert("Application Error", "It appears you change the time/date of your phone. Please restore the correct time/date", "Got it");
                        await Navigation.PopToRootAsync();
                    }
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            }
        }

        private async void btnUI_Clicked(object sender, EventArgs e)
        {
            var appdate = Preferences.Get("appdatetime", String.Empty, "private_prefs");

            if (string.IsNullOrEmpty(appdate))
            {
                Preferences.Set("appdatetime", DateTime.Now.ToString(), "private_prefs");
            }
            else
            {
                try
                {
                    if (DateTime.Now >= DateTime.Parse(Preferences.Get("appdatetime", String.Empty, "private_prefs")))
                    {
                        Preferences.Set("appdatetime", DateTime.Now.ToString(), "private_prefs");

                        await Navigation.PushAsync(new UnsyncedData(host, database, contact, ipaddress, pingipaddress));
                    }
                    else
                    {
                        await DisplayAlert("Application Error", "It appears you change the time/date of your phone. Please restore the correct time/date", "Got it");
                        await Navigation.PopToRootAsync();
                    }
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            }
        }

        private async void btnAH_Clicked(object sender, EventArgs e)
        {
            var appdate = Preferences.Get("appdatetime", String.Empty, "private_prefs");

            if (string.IsNullOrEmpty(appdate))
            {
                Preferences.Set("appdatetime", DateTime.Now.ToString(), "private_prefs");
            }
            else
            {
                try
                {
                    if (DateTime.Now >= DateTime.Parse(Preferences.Get("appdatetime", String.Empty, "private_prefs")))
                    {
                        Preferences.Set("appdatetime", DateTime.Now.ToString(), "private_prefs");

                        Analytics.TrackEvent("Opened Activity History");
                        await Navigation.PushAsync(new ActivityHistoryList(host, database, contact, ipaddress, pingipaddress));
                    }
                    else
                    {
                        await DisplayAlert("Application Error", "It appears you change the time/date of your phone. Please restore the correct time/date", "Got it");
                        await Navigation.PopToRootAsync();
                    }
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            }
        }
    }
}