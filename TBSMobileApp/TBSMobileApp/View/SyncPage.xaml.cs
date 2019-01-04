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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TBSMobileApp.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SyncPage : ContentPage
	{
        string contact;
        string host;
        string database;
        string ipaddress;
        byte[] pingipaddress;

        public SyncPage (string host, string database, string contact, string ipaddress, byte[] pingipaddress)
		{
			InitializeComponent ();
            this.contact = contact;
            this.host = host;
            this.database = database;
            this.ipaddress = ipaddress;
            this.pingipaddress = pingipaddress;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            //Check if there is an internet connection
            if (CrossConnectivity.Current.IsConnected)
            {
                var ping = new Ping();
                var reply = ping.Send(new IPAddress(pingipaddress), 5000);

                if (reply.Status == IPStatus.Success)
                {
                    SyncUser(host, database, contact, ipaddress, pingipaddress);
                }
                else
                {
                    Application.Current.MainPage.Navigation.PushAsync(new MainMenu(host, database, contact, ipaddress, pingipaddress));
                }
            }
            else
            {
                Application.Current.MainPage.Navigation.PushAsync(new MainMenu(host, database, contact, ipaddress, pingipaddress));
            }
        }

        public class UserData
        {
            public string ContactID { get; set; }
            public string UserID { get; set; }
            public string UserPassword { get; set; }
            public DateTime LastSync { get; set; }
            public int Deleted { get; set; }
            public DateTime LastUpdated { get; set; }
        }

        public class ContactsData
        {
            public string ContactID { get; set; }
            public string FileAs { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public DateTime LastSync { get; set; }
            public int Deleted { get; set; }
            public DateTime LastUpdated { get; set; }
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

        public class ActivityData
        {
            public string ActivityID { get; set; }
            public string ActivityDescription { get; set; }
            public DateTime LastSync { get; set; }
            public DateTime LastUpdated { get; set; }
            public int Deleted { get; set; }
        }

        public class SystemSerialData
        {
            public string SerialNumber { get; set; }
            public DateTime DateStart { get; set; }
            public string NoOfDays { get; set; }
            public int Trials { get; set; }
            public string InputSerialNumber { get; set; }
            public DateTime LastSync { get; set; }
            public DateTime LastUpdated { get; set; }
            public int Deleted { get; set; }
        }

        public async void SyncUser(string host, string database, string contact, string ipaddress, byte[] pingipaddress)
        {
            var ping = new Ping();
            var reply = ping.Send(new IPAddress(pingipaddress), 5000);

            if (reply.Status == IPStatus.Success)
            {
                try
                {
                    syncStatus.Text = "Initializing user data sync";

                    var db = DependencyService.Get<ISQLiteDB>();
                    var conn = db.GetConnection();

                    var sql = "SELECT * FROM tblUser WHERE ContactID = '" + contact + "' AND Deleted != '1'";
                    var getUser = conn.QueryAsync<UserTable>(sql);
                    var resultCount = getUser.Result.Count;
                    var current_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (resultCount > 0)
                    {
                        try
                        {
                            syncStatus.Text = "Checking server updates";

                            var chlink = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Contact=" + contact + "&Request=79MbtQ";
                            string chcontentType = "application/json";
                            JObject json = new JObject
                            {
                                { "ContactID", contact }
                            };

                            HttpClient chclient = new HttpClient();
                            var chresponse = await chclient.PostAsync(chlink, new StringContent(json.ToString(), Encoding.UTF8, chcontentType));

                            if (chresponse.IsSuccessStatusCode)
                            {
                                var chcontent = await chresponse.Content.ReadAsStringAsync();

                                if (!string.IsNullOrEmpty(chcontent))
                                {
                                    var chuserresult = JsonConvert.DeserializeObject<List<UserData>>(chcontent);
                                    for (int chi = 0; chi < chuserresult.Count; chi++)
                                    {
                                        var item = chuserresult[chi];
                                        var chcontactID = item.ContactID;
                                        var chuserID = item.UserID;
                                        var chuserPassword = item.UserPassword;
                                        var chlastSync = DateTime.Parse(current_datetime);
                                        var chlastUpdated = item.LastUpdated;
                                        var chdltd = item.Deleted;

                                        var chsql = "SELECT chcontactID FROM tblUser WHERE LastUpdated = '" + chlastUpdated + "' AND ContactID = '"+ chcontactID + "'";
                                        var chgetUser = conn.QueryAsync<UserTable>(chsql);
                                        var chresultCount = chgetUser.Result.Count;

                                        if (chresultCount == 0)
                                        {
                                            var cheuser = new UserTable
                                            {
                                                ContactID = chcontactID,
                                                UserID = chuserID,
                                                UserPassword = chuserPassword,
                                                LastSync = chlastSync,
                                                LastUpdated = chlastUpdated,
                                                Deleted = chdltd
                                            };

                                            await conn.InsertOrReplaceAsync(cheuser);
                                            syncStatus.Text = "Syncing user update of " + chuserID;
                                        }
                                    }
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
                        try
                        {
                            syncStatus.Text = "Getting user data from server";
                            var link = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Contact=" + contact + "&Request=8qApc8";
                            string contentType = "application/json";
                            JObject json = new JObject
                            {
                                { "ContactID", contact }
                            };

                            HttpClient client = new HttpClient();
                            var response = await client.PostAsync(link, new StringContent(json.ToString(), Encoding.UTF8, contentType));

                            if (response.IsSuccessStatusCode)
                            {
                                var content = await response.Content.ReadAsStringAsync();

                                if (!string.IsNullOrEmpty(content))
                                {
                                    var userresult = JsonConvert.DeserializeObject<List<UserData>>(content);
                                    for (int i = 0; i < userresult.Count; i++)
                                    {
                                        syncStatus.Text = "Syncing user " + (i + 1) + " out of " + userresult.Count;
                                        var item = userresult[i];
                                        var contactID = item.ContactID;
                                        var userID = item.UserID;
                                        var userPassword = item.UserPassword;
                                        var lastSync = DateTime.Parse(current_datetime);
                                        var lastUpdated = item.LastUpdated;
                                        var deleted = item.Deleted;

                                        var user = new UserTable
                                        {
                                            ContactID = contactID,
                                            UserID = userID,
                                            UserPassword = userPassword,
                                            LastSync = lastSync,
                                            Deleted = deleted,
                                            LastUpdated = lastUpdated
                                        };

                                        await conn.InsertOrReplaceAsync(user);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Crashes.TrackError(ex);
                        }
                    }

                    SyncContacts(host, database, contact, ipaddress, pingipaddress);
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            }
            else
            {
                syncStatus.Text = "Syncing user failed. Server is unreachable.";
                btnBack.IsVisible = true;
            }
        }

        public async void SyncContacts(string host, string database, string contact, string ipaddress, byte[] pingipaddress)
        {
            var ping = new Ping();
            var reply = ping.Send(new IPAddress(pingipaddress), 5000);

            if (reply.Status == IPStatus.Success)
            {
                try
                {
                    syncStatus.Text = "Initializing contact data sync";

                    var db = DependencyService.Get<ISQLiteDB>();
                    var conn = db.GetConnection();

                    var sql = "SELECT * FROM tblContacts WHERE Deleted != '1'";
                    var getUser = conn.QueryAsync<UserTable>(sql);
                    var resultCount = getUser.Result.Count;
                    var current_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (resultCount > 0)
                    {
                        try
                        {
                            syncStatus.Text = "Checking server updates";

                            var chlink = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Request=kq7K5P";
                            string chcontentType = "application/json";
                            JObject json = new JObject
                            {
                                { "ContactID", contact }
                            };

                            HttpClient chclient = new HttpClient();
                            var chresponse = await chclient.PostAsync(chlink, new StringContent(json.ToString(), Encoding.UTF8, chcontentType));

                            if (chresponse.IsSuccessStatusCode)
                            {
                                var chcontent = await chresponse.Content.ReadAsStringAsync();

                                if (!string.IsNullOrEmpty(chcontent))
                                {
                                    var chcontactresult = JsonConvert.DeserializeObject<List<ContactsData>>(chcontent);
                                    for (int i = 0; i < chcontactresult.Count; i++)
                                    {
                                        var chitem = chcontactresult[i];
                                        var chcontactID = chitem.ContactID;
                                        var chfileAs = chitem.FileAs;
                                        var chfirstName = chitem.FirstName;
                                        var chlastName = chitem.LastName;
                                        var chmiddleName = chitem.MiddleName;
                                        var chlastSync = DateTime.Parse(current_datetime);
                                        var chlastUpdated = chitem.LastUpdated;
                                        var chdltd = chitem.Deleted;

                                        var chsql = "SELECT chcontactID FROM tblContacts WHERE LastUpdated = '" + chlastUpdated + "' AND ContactID = '" + chcontactID + "'";
                                        var chgetContact = conn.QueryAsync<ContactsTable>(chsql);
                                        var chresultCount = chgetContact.Result.Count;

                                        if (chresultCount == 0)
                                        {
                                            var checontacts = new ContactsTable
                                            {
                                                ContactID = chcontactID,
                                                FileAs = chfileAs,
                                                FirstName = chfirstName,
                                                LastName = chlastName,
                                                MiddleName = chmiddleName,
                                                LastSync = chlastSync,
                                                LastUpdated = chlastUpdated,
                                                Deleted = chdltd
                                            };

                                            await conn.InsertOrReplaceAsync(checontacts);
                                            syncStatus.Text = "Syncing contact update of " + chfileAs;
                                        }
                                    }
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
                        try
                        {
                            syncStatus.Text = "Getting contact data from server";
                            var link = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Request=9DpndD";
                            string contentType = "application/json";
                            JObject json = new JObject
                            {
                                { "ContactID", contact }
                            };

                            HttpClient client = new HttpClient();
                            var response = await client.PostAsync(link, new StringContent(json.ToString(), Encoding.UTF8, contentType));

                            if (response.IsSuccessStatusCode)
                            {
                                var content = await response.Content.ReadAsStringAsync();

                                if (!string.IsNullOrEmpty(content))
                                {
                                    var jsonsettings = new JsonSerializerSettings {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        MissingMemberHandling = MissingMemberHandling.Ignore
                                    };
                                    var contactresult = JsonConvert.DeserializeObject<List<ContactsData>>(content,jsonsettings);
                                    for (int i = 0; i < contactresult.Count; i++)
                                    {
                                        syncStatus.Text = "Syncing contact " + (i + 1) + " out of " + contactresult.Count;
                                        var item = contactresult[i];
                                        var contactID = item.ContactID;
                                        var fileAs = item.FileAs;
                                        var firstName = item.FirstName;
                                        var lastName = item.LastName;
                                        var middleName = item.MiddleName;
                                        var lastSync = DateTime.Parse(current_datetime);
                                        var lastUpdated = item.LastUpdated;
                                        var deleted = item.Deleted;

                                        var chcontact = new ContactsTable
                                        {
                                            ContactID = contactID,
                                            FileAs = fileAs,
                                            FirstName = firstName,
                                            LastName = lastName,
                                            MiddleName = middleName,
                                            LastSync = lastSync,
                                            Deleted = deleted,
                                            LastUpdated = lastUpdated
                                        };

                                        await conn.InsertOrReplaceAsync(chcontact);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Crashes.TrackError(ex);
                        }
                    }

                    SyncCaf(host, database, contact, ipaddress, pingipaddress);
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            }
            else
            {
                syncStatus.Text = "Syncing contacts failed. Server is unreachable.";
                btnBack.IsVisible = true;
            }
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
                                    syncStatus.Text = "Sending customer activity data to server " + (i + 1) + " out of " + changesresultCount;

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
                            syncStatus.Text = "Getting customer activity data from server";

                            var link = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Contact=" + contact + "&Request=fqV2Vb";
                            string contentType = "application/json";
                            JObject json = new JObject
                            {
                                { "ContactID", contact }
                            };

                            HttpClient client = new HttpClient();
                            var response = await client.PostAsync(link, new StringContent(json.ToString(), Encoding.UTF8, contentType));

                            if (response.IsSuccessStatusCode)
                            {
                                var content = await response.Content.ReadAsStringAsync();

                                if (!string.IsNullOrEmpty(content))
                                {
                                    var cafresult = JsonConvert.DeserializeObject<List<CAFData>>(content);
                                    for (int i = 0; i < cafresult.Count; i++)
                                    {
                                        syncStatus.Text = "Syncing customer activity data " + (i + 1) + " out of " + cafresult.Count;

                                        var item = cafresult[i];
                                        var cafNo = item.CAFNo;
                                        var employeeID = item.EmployeeID;
                                        var contactPerson = item.ContactPersonID;
                                        var cafDate = item.CAFDate;
                                        var customerID = item.CustomerID;
                                        var activtyID = item.ActivityID;
                                        var startTime = item.StartTime;
                                        var endTime = item.EndTime;
                                        var bfast = item.Breakfast;
                                        var lnch = item.Lunch;
                                        var dnnr = item.Dinner;
                                        var hotelAccommodation = item.HotelAccommodation;
                                        var transportation = item.TransportationFare;
                                        var cashAdvance = item.CashAdvance;
                                        var remarks = item.Remarks;
                                        var otherConcern = item.OtherConcern;
                                        var signature = item.Signature;
                                        var mobileSignature = item.MobileSignature;
                                        var lastSync = DateTime.Parse(current_datetime);
                                        var lastUpdated = item.LastUpdated;
                                        var deleted = item.Deleted;

                                        var caf = new CAFTable
                                        {
                                            CAFNo = cafNo,
                                            EmployeeID = employeeID,
                                            ContactPersonID = contactPerson,
                                            CAFDate = cafDate,
                                            CustomerID = customerID,
                                            StartTime = startTime,
                                            EndTime = endTime,
                                            Breakfast = bfast,
                                            Lunch = lnch,
                                            Dinner = dnnr,
                                            HotelAccommodation = hotelAccommodation,
                                            TransportationFare = transportation,
                                            CashAdvance = cashAdvance,
                                            Remarks = remarks,
                                            OtherConcern = otherConcern,
                                            Signature = signature,
                                            MobileSignature = mobileSignature,
                                            LastSync = lastSync,
                                            Deleted = deleted,
                                            LastUpdated = lastUpdated
                                        };

                                        await conn.InsertOrReplaceAsync(caf);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Crashes.TrackError(ex);
                        }
                    }

                    SyncActivities(host, database, contact, ipaddress, pingipaddress);
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            }
            else
            {
                syncStatus.Text = "Syncing customer activity failed. Server is unreachable.";
                btnBack.IsVisible = true;
            }
        }

        public async void SyncActivities(string host, string database, string contact, string ipaddress, byte[] pingipaddress)
        {
            var ping = new Ping();
            var reply = ping.Send(new IPAddress(pingipaddress), 5000);

            if (reply.Status == IPStatus.Success)
            {
                try
                {
                    syncStatus.Text = "Initializing contact data sync";

                    var db = DependencyService.Get<ISQLiteDB>();
                    var conn = db.GetConnection();

                    var sql = "SELECT * FROM tblActivity WHERE Deleted != '1'";
                    var getActivity = conn.QueryAsync<ActivityTable>(sql);
                    var resultCount = getActivity.Result.Count;
                    var current_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (resultCount > 0)
                    {
                        try
                        {
                            syncStatus.Text = "Checking server updates";

                            var chlink = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Request=b7Q9XU";
                            string chcontentType = "application/json";
                            JObject json = new JObject
                            {
                                { "ContactID", contact }
                            };

                            HttpClient chclient = new HttpClient();
                            var chresponse = await chclient.PostAsync(chlink, new StringContent(json.ToString(), Encoding.UTF8, chcontentType));

                            if (chresponse.IsSuccessStatusCode)
                            {
                                var chcontent = await chresponse.Content.ReadAsStringAsync();

                                if (!string.IsNullOrEmpty(chcontent))
                                {
                                    var chactivityresult = JsonConvert.DeserializeObject<List<ActivityData>>(chcontent);
                                    for (int i = 0; i < chactivityresult.Count; i++)
                                    {
                                        var chitem = chactivityresult[i];
                                        var chactivityID = chitem.ActivityID;
                                        var chactivityDesc = chitem.ActivityDescription;
                                        var chlastSync = DateTime.Parse(current_datetime);
                                        var chlastUpdated = chitem.LastUpdated;
                                        var chdltd = chitem.Deleted;

                                        var chsql = "SELECT chcontactID FROM tblActivity WHERE LastUpdated = '" + chlastUpdated + "' AND ActivityID = '" + chactivityID + "'";
                                        var chgetActivity = conn.QueryAsync<ActivityTable>(chsql);
                                        var chresultCount = chgetActivity.Result.Count;

                                        if (chresultCount == 0)
                                        {
                                            var cheactivity = new ActivityTable
                                            {
                                                ActivityID = chactivityID,
                                                ActivityDescription = chactivityDesc,
                                                LastSync = chlastSync,
                                                LastUpdated = chlastUpdated,
                                                Deleted = chdltd
                                            };

                                            await conn.InsertOrReplaceAsync(cheactivity);
                                            syncStatus.Text = "Syncing activity update of " + chactivityDesc;
                                        }
                                    }
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
                        try
                        {
                            syncStatus.Text = "Getting activities data from server";
                            var link = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Request=M2T3w2";
                            string contentType = "application/json";
                            JObject json = new JObject
                            {
                                { "ContactID", contact }
                            };

                            HttpClient client = new HttpClient();
                            var response = await client.PostAsync(link, new StringContent(json.ToString(), Encoding.UTF8, contentType));

                            if (response.IsSuccessStatusCode)
                            {
                                var content = await response.Content.ReadAsStringAsync();

                                if (!string.IsNullOrEmpty(content))
                                {
                                    var activityresult = JsonConvert.DeserializeObject<List<ActivityData>>(content);
                                    for (int i = 0; i < activityresult.Count; i++)
                                    {
                                        syncStatus.Text = "Syncing activities " + (i + 1) + " out of " + activityresult.Count;
                                        var item = activityresult[i];
                                        var activityID = item.ActivityID;
                                        var activtyDesc = item.ActivityDescription;
                                        var lastSync = DateTime.Parse(current_datetime);
                                        var lastUpdated = item.LastUpdated;
                                        var deleted = item.Deleted;

                                        var chactivities = new ActivityTable
                                        {
                                            ActivityID = activityID,
                                            ActivityDescription = activtyDesc,
                                            LastSync = lastSync,
                                            Deleted = deleted,
                                            LastUpdated = lastUpdated
                                        };

                                        await conn.InsertOrReplaceAsync(chactivities);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Crashes.TrackError(ex);
                        }
                    }

                    SyncSerial(host, database, contact, ipaddress, pingipaddress);
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            }
            else
            {
                syncStatus.Text = "Syncing activities failed. Server is unreachable.";
                btnBack.IsVisible = true;
            }
        }

        public async void SyncSerial(string host, string database, string contact, string ipaddress, byte[] pingipaddress)
        {
            var ping = new Ping();
            var reply = ping.Send(new IPAddress(pingipaddress), 5000);

            if (reply.Status == IPStatus.Success)
            {
                try
                {
                    syncStatus.Text = "Initializing contact data sync";

                    var db = DependencyService.Get<ISQLiteDB>();
                    var conn = db.GetConnection();

                    var sql = "SELECT * FROM tblSystemSerial WHERE Deleted != '1'";
                    var getSubscription = conn.QueryAsync<SystemSerialTable>(sql);
                    var resultCount = getSubscription.Result.Count;
                    var current_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (resultCount > 0)
                    {
                        try
                        {
                            syncStatus.Text = "Checking server updates";

                            var chlink = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Request=b7Q9XU";
                            string chcontentType = "application/json";
                            JObject json = new JObject
                            {
                                { "Device", Constants.deviceID }
                            };

                            HttpClient chclient = new HttpClient();
                            var chresponse = await chclient.PostAsync(chlink, new StringContent(json.ToString(), Encoding.UTF8, chcontentType));

                            if (chresponse.IsSuccessStatusCode)
                            {
                                var chcontent = await chresponse.Content.ReadAsStringAsync();

                                if (!string.IsNullOrEmpty(chcontent))
                                {
                                    var chsubscriptionresult = JsonConvert.DeserializeObject<List<SystemSerialData>>(chcontent);
                                    for (int i = 0; i < chsubscriptionresult.Count; i++)
                                    {
                                        var chitem = chsubscriptionresult[i];
                                        var chserialNumber = chitem.SerialNumber;
                                        var chdateStart = chitem.DateStart;
                                        var chdays = chitem.NoOfDays;
                                        var chtrials = chitem.Trials;
                                        var chinputserial = chitem.InputSerialNumber;
                                        var chlastSync = DateTime.Parse(current_datetime);
                                        var chlastUpdated = chitem.LastUpdated;
                                        var chdeleted = chitem.Deleted;

                                        var chsql = "SELECT chcontactID FROM tblSystemSerial WHERE LastUpdated = '" + chlastUpdated + "' AND SerialNumber = '" + chserialNumber + "'";
                                        var chgetSubscription = conn.QueryAsync<SystemSerialTable>(chsql);
                                        var chresultCount = chgetSubscription.Result.Count;

                                        if (chresultCount == 0)
                                        {
                                            var chsubscription = new SystemSerialTable
                                            {
                                                SerialNumber = chserialNumber,
                                                DateStart = chdateStart,
                                                NoOfDays = chdays,
                                                Trials = chtrials,
                                                InputSerialNumber = chinputserial,
                                                LastSync = chlastSync,
                                                Deleted = chdeleted,
                                                LastUpdated = chlastUpdated
                                            };

                                            await conn.InsertOrReplaceAsync(chsubscription);
                                            syncStatus.Text = "Syncing subscription update of " + chserialNumber;
                                        }
                                    }
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
                        try
                        {
                            syncStatus.Text = "Getting subscription data from server";
                            var link = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Request=qtF5Ej";
                            string contentType = "application/json";
                            JObject json = new JObject
                            {
                                { "Device", Constants.deviceID }
                            };

                            HttpClient client = new HttpClient();
                            var response = await client.PostAsync(link, new StringContent(json.ToString(), Encoding.UTF8, contentType));

                            if (response.IsSuccessStatusCode)
                            {
                                var content = await response.Content.ReadAsStringAsync();

                                if (!string.IsNullOrEmpty(content))
                                {
                                    var jsonsettings = new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        MissingMemberHandling = MissingMemberHandling.Ignore
                                    };
                                    var subscriptionresult = JsonConvert.DeserializeObject<List<SystemSerialData>>(content, jsonsettings);
                                    for (int i = 0; i < subscriptionresult.Count; i++)
                                    {
                                        syncStatus.Text = "Syncing subscription " + (i + 1) + " out of " + subscriptionresult.Count;
                                        var item = subscriptionresult[i];
                                        var serialNumber = item.SerialNumber;
                                        var dateStart = item.DateStart;
                                        var days = item.NoOfDays;
                                        var trials = item.Trials;
                                        var inputserial = item.InputSerialNumber;
                                        var lastSync = DateTime.Parse(current_datetime);
                                        var lastUpdated = item.LastUpdated;
                                        var deleted = item.Deleted;

                                        var chsubscription = new SystemSerialTable
                                        {
                                            SerialNumber = serialNumber,
                                            DateStart = dateStart,
                                            NoOfDays = days,
                                            Trials = trials,
                                            InputSerialNumber = inputserial,
                                            LastSync = lastSync,
                                            Deleted = deleted,
                                            LastUpdated = lastUpdated
                                        };

                                        await conn.InsertOrReplaceAsync(chsubscription);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Crashes.TrackError(ex);
                        }
                    }

                    OnSyncComplete();
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            }
        }

        public void OnSyncComplete()
        {
            syncStatus.Text = "Data sync successfully";
            btnContinue.IsVisible = true;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private async void btnContinue_Clicked(object sender, EventArgs e)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new MainMenu(host, database, contact, ipaddress, pingipaddress));
        }

        private async void btnBack_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}