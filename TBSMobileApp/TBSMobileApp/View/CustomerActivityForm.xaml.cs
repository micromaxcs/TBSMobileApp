using Microsoft.AppCenter.Crashes;
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
	public partial class CustomerActivityForm : ContentPage
	{
        string contact;
        string host;
        string database;
        string ipaddress;
        byte[] pingipaddress;

        public CustomerActivityForm (string host, string database, string contact, string ipaddress, byte[] pingipaddress)
		{
			InitializeComponent ();
            this.contact = contact;
            this.host = host;
            this.database = database;
            this.ipaddress = ipaddress;
            this.pingipaddress = pingipaddress;
            Init();
        }

        void Init()
        {
            tpTime.Text = DateTime.Now.ToString("HH:mm:ss");
            SetCAFNo();
            dpDate.Text = DateTime.Now.ToString("yyyy/MM/dd");
            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#3498db");
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

                        var db = DependencyService.Get<ISQLiteDB>();
                        var conn = db.GetConnection();

                        var getCustomer = conn.QueryAsync<ContactsTable>("SELECT * FROM tblContacts");
                        var customerresultCount = getCustomer.Result.Count;
                        if (customerresultCount > 0)
                        {
                            var customerresult = getCustomer.Result;
                            customerPicker.ItemsSource = customerresult;
                            contactpersonPicker.ItemsSource = customerresult;
                        }

                        var getActivity = conn.QueryAsync<ActivityTable>("SELECT * FROM tblActivity");
                        var activityresultCount = getActivity.Result.Count;
                        if (activityresultCount > 0)
                        {
                            var activityresult = getActivity.Result;
                            activityPicker.ItemsSource = activityresult;
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

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await this.DisplayAlert("Discard Confirmation", "Are you sure you want to discard this form?", "Yes", "No");

                if (result == true)
                {
                    await this.Navigation.PopAsync();
                }
            });

            return true;
        }

        public void SetCAFNo()
        {
            var numbers = "0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < 8; i++)
            {
                stringChars[i] = numbers[random.Next(numbers.Length)];
            }

            var finalString = new String(stringChars);
            entCafNo.Text = "AF" + finalString;
        }

        private void CustomerPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (customerPicker.SelectedIndex > -1)
                {
                    var pickedCustomer = customerPicker.Items[customerPicker.SelectedIndex];
                    string[] picked = pickedCustomer.Split(new char[] { '-' });
                    entCustomerID.Text = picked[0];
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
            }
        }

        private void CustomerPicker_Unfocused(object sender, FocusEventArgs e)
        {
            if (String.IsNullOrEmpty(entCustomerID.Text))
            {
                CustomerFrame.BorderColor = Color.FromHex("#e74c3c");
            }
            else
            {
                CustomerFrame.BorderColor = Color.FromHex("#e8eaed");
            }
        }

        private void ContactpersonPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (contactpersonPicker.SelectedIndex > -1)
                {
                    var pickedContactPerson = contactpersonPicker.Items[contactpersonPicker.SelectedIndex];
                    string[] picked = pickedContactPerson.Split(new char[] { '-' });
                    entContactPerson.Text = picked[0];
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
            }
        }

        private void ContactpersonPicker_Unfocused(object sender, FocusEventArgs e)
        {
            if (String.IsNullOrEmpty(entContactPerson.Text))
            {
                ContactPersonFrame.BorderColor = Color.FromHex("#e74c3c");
            }
            else
            {
                ContactPersonFrame.BorderColor = Color.FromHex("#e8eaed");
            }
        }

        private void ActivityPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (activityPicker.SelectedIndex > -1)
                {
                    var pickedActivity = activityPicker.Items[activityPicker.SelectedIndex];
                    string[] picked = pickedActivity.Split(new char[] { '-' });
                    entActivity.Text = picked[0];
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
            }
        }

        private void ActivityPicker_Unfocused(object sender, FocusEventArgs e)
        {
            if (String.IsNullOrEmpty(entActivity.Text))
            {
                ActivityFrame.BorderColor = Color.FromHex("#e74c3c");
            }
            else
            {
                ActivityFrame.BorderColor = Color.FromHex("#e8eaed");
            }
        }

        private async void BtnGotoPage2_Clicked(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(entCustomerID.Text) || String.IsNullOrEmpty(entContactPerson.Text) || String.IsNullOrEmpty(entActivity.Text))
            {
                if (String.IsNullOrEmpty(entActivity.Text))
                {
                    ActivityFrame.BorderColor = Color.FromHex("#e74c3c");
                }
                else
                {
                    ActivityFrame.BorderColor = Color.FromHex("#e8eaed");
                }

                if (String.IsNullOrEmpty(entContactPerson.Text))
                {
                    ContactPersonFrame.BorderColor = Color.FromHex("#e74c3c");
                }
                else
                {
                    ContactPersonFrame.BorderColor = Color.FromHex("#e8eaed");
                }

                if (String.IsNullOrEmpty(entCustomerID.Text))
                {
                    CustomerFrame.BorderColor = Color.FromHex("#e74c3c");
                }
                else
                {
                    CustomerFrame.BorderColor = Color.FromHex("#e8eaed");
                }

                await DisplayAlert("Form Required", "Please fill-up the required field", "Got it");
            }
            else
            {
                fafPage1.IsVisible = false;
                fafPage2.IsVisible = true;
                page1nav.IsVisible = false;
                page2nav.IsVisible = true;
            }
        }

        private void BtnGoBacktoPage1_Clicked(object sender, EventArgs e)
        {
            fafPage1.IsVisible = true;
            fafPage2.IsVisible = false;
            page1nav.IsVisible = true;
            page2nav.IsVisible = false;
        }

        private void BtnGotoPage3_Clicked(object sender, EventArgs e)
        {
            fafPage3.IsVisible = true;
            fafPage2.IsVisible = false;
            page3nav.IsVisible = true;
            page2nav.IsVisible = false;
        }

        private void BtnGoBacktoPage3_Clicked(object sender, EventArgs e)
        {
            fafPage3.IsVisible = false;
            fafPage2.IsVisible = true;
            page3nav.IsVisible = false;
            page2nav.IsVisible = true;
        }

        private async void BtnSend_Clicked(object sender, EventArgs e)
        {
            if (Signature.IsBlank)
            {
                await DisplayAlert("Form Required", "Please fill-up the required field", "Got it");
            }
            else
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
                            try
                            {
                                var confirm = await DisplayAlert("Sending Confirmation", "Are you sure you want to send this form?", "Yes", "No");
                                if (confirm == true)
                                {
                                    btnGoBacktoPage3.IsEnabled = false;
                                    btnSend.IsEnabled = false;
                                    Signature.IsEnabled = false;

                                    if (CrossConnectivity.Current.IsConnected)
                                    {
                                        var ping = new Ping();
                                        var reply = ping.Send(new IPAddress(pingipaddress), 5000);

                                        if (reply.Status == IPStatus.Success)
                                        {
                                            var optimalSpeed = 300000;
                                            var connectionTypes = CrossConnectivity.Current.ConnectionTypes;

                                            if (connectionTypes.Any(speed => Convert.ToInt32(speed) < optimalSpeed))
                                            {
                                                var sendconfirm = await DisplayAlert("Slow Connection Connection Warning", "Slow connection detected. Do you want to send the data?", "Send to server", "Save offline");

                                                if (sendconfirm == true)
                                                {
                                                    Send_online();
                                                }
                                                else
                                                {
                                                    Send_offline();
                                                }
                                            }
                                            else
                                            {
                                                Send_online();
                                            }
                                        }
                                        else
                                        {
                                            Send_offline();
                                        }
                                    }
                                    else
                                    {
                                        Send_offline();
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
            }
        }

        public async void Send_online()
        {
            var cafNo = entCafNo.Text;
            var customerID = entCustomerID.Text;
            var contactPerson = entContactPerson.Text;
            var activityID = entActivity.Text;
            var date = dpDate.Text;
            var startTime = tpTime.Text;
            var endTime = DateTime.Now.ToString("HH:mm:ss");
            var remarks = entRemarks.Text;
            var others = entOthers.Text;
            var breakfast = entBreakfast.Text;
            var lunch = entLunch.Text;
            var dinner = entDinner.Text;
            var hotel = entHotel.Text;
            var transportation = entTransportation.Text;
            var cash = entCash.Text;
            var deleted = 0;
            var current_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var signatureFile = cafNo + ".jpg";

            Stream sigimage = await Signature.GetImageStreamAsync(SignaturePad.Forms.SignatureImageFormat.Jpeg);

            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), signatureFile);

            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                await sigimage.CopyToAsync(fileStream);
            }

            string url = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Request=k5N7PE";
            string contentType = "application/json";
            JObject json = new JObject
            {
                { "CAFNo", cafNo },
                { "EmployeeID", contact },
                { "ContactPersonID", contactPerson },
                { "CAFDate", DateTime.Parse(date) },
                { "CustomerID", customerID },
                { "ActivityID", activityID },
                { "StartTime", DateTime.Parse(startTime) },
                { "EndTime", DateTime.Parse(endTime) },
                { "Breakfast", breakfast },
                { "Lunch", lunch },
                { "Dinner", dinner },
                { "HotelAccommodation", hotel },
                { "TransportationFare", transportation },
                { "CashAdvance", cash },
                { "MobileSignature", fileName },
                { "Remarks", remarks },
                { "OtherConcern", others },
                { "Deleted", deleted },
                { "LastUpdated", DateTime.Parse(current_datetime) }
            };

            HttpClient client = new HttpClient();
            var response = await client.PostAsync(url, new StringContent(json.ToString(), Encoding.UTF8, contentType));

            if (response.IsSuccessStatusCode)
            {
                byte[] crSignatureData = File.ReadAllBytes(fileName);

                var siglink = "http://" + ipaddress + Constants.requestUrl + "Host=" + host + "&Database=" + database + "&Contact=" + contact + "&Request=N4f5GL";
                string sigcontentType = "application/json";
                JObject sigjson = new JObject
                {
                   { "CAFNo", cafNo },
                   { "CAFDate", DateTime.Parse(date) },
                   { "Signature", crSignatureData }
                };

                HttpClient sigclient = new HttpClient();
                var sigresponse = await sigclient.PostAsync(siglink, new StringContent(sigjson.ToString(), Encoding.UTF8, sigcontentType));

                if (sigresponse.IsSuccessStatusCode)
                {
                    try
                    {
                        var db = DependencyService.Get<ISQLiteDB>();
                        var conn = db.GetConnection();

                        var caf_insert = new CAFTable
                        {
                            CAFNo = cafNo,
                            EmployeeID = contact,
                            CAFDate = DateTime.Parse(date),
                            CustomerID = customerID,
                            ContactPersonID = contactPerson,
                            ActivityID = activityID,
                            Breakfast = float.Parse(breakfast),
                            Lunch = float.Parse(lunch),
                            Dinner = float.Parse(dinner),
                            HotelAccommodation = float.Parse(hotel),
                            TransportationFare = float.Parse(transportation),
                            CashAdvance = float.Parse(cash),
                            StartTime = DateTime.Parse(startTime),
                            EndTime = DateTime.Parse(endTime),
                            Remarks = remarks,
                            MobileSignature = fileName,
                            Signature = fileName,
                            OtherConcern = others,
                            Deleted = deleted,
                            LastSync = DateTime.Parse(current_datetime),
                            LastUpdated = DateTime.Parse(current_datetime)
                        };

                        await conn.InsertAsync(caf_insert);

                        await DisplayAlert("Data Sent", "Your activity has been sent to the server", "Got it");
                        await Application.Current.MainPage.Navigation.PopAsync();
                    }
                    catch (Exception ex)
                    {
                        Crashes.TrackError(ex);
                        Send_offline();
                    }
                }
            }
        }

        public async void Send_offline()
        {
            try
            {
                var cafNo = entCafNo.Text;
                var customerID = entCustomerID.Text;
                var contactPerson = entContactPerson.Text;
                var activityID = entActivity.Text;
                var date = dpDate.Text;
                var startTime = tpTime.Text;
                var endTime = DateTime.Now.ToString("HH:mm:ss");
                var remarks = entRemarks.Text;
                var others = entOthers.Text;
                var breakfast = entBreakfast.Text;
                var lunch = entLunch.Text;
                var dinner = entDinner.Text;
                var hotel = entHotel.Text;
                var transportation = entTransportation.Text;
                var cash = entCash.Text;
                var deleted = 0;
                var current_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var signatureFile = cafNo + ".jpg";

                Stream sigimage = await Signature.GetImageStreamAsync(SignaturePad.Forms.SignatureImageFormat.Jpeg);

                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), signatureFile);

                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    await sigimage.CopyToAsync(fileStream);
                }

                var db = DependencyService.Get<ISQLiteDB>();
                var conn = db.GetConnection();

                var caf_insert = new CAFTable
                {
                    CAFNo = cafNo,
                    EmployeeID = contact,
                    CAFDate = DateTime.Parse(date),
                    CustomerID = customerID,
                    ContactPersonID = contactPerson,
                    ActivityID = activityID,
                    Breakfast = float.Parse(breakfast),
                    Lunch = float.Parse(lunch),
                    Dinner = float.Parse(dinner),
                    HotelAccommodation = float.Parse(hotel),
                    TransportationFare = float.Parse(transportation),
                    CashAdvance = float.Parse(cash),
                    StartTime = DateTime.Parse(startTime),
                    EndTime = DateTime.Parse(endTime),
                    Remarks = remarks,
                    MobileSignature = fileName,
                    Signature = fileName,
                    OtherConcern = others,
                    Deleted = deleted,
                    LastUpdated = DateTime.Parse(current_datetime)
                };

                await conn.InsertAsync(caf_insert);

                await DisplayAlert("Offline Save", "Your activity has been saved offline. Connect to the server to send your activity", "Got it");
                await Application.Current.MainPage.Navigation.PopAsync();

                //if (File.Exists(fileName))
                //{
                //    await DisplayAlert("File", "exist", "ok");
                //}
                //else
                //{
                //    await DisplayAlert("ad", "not exist", "ok");
                //}
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
            }

        }

        private void CustomerPicker_Focused(object sender, FocusEventArgs e)
        {
            CustomerFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void ContactpersonPicker_Focused(object sender, FocusEventArgs e)
        {
            ContactPersonFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void ActivityPicker_Focused(object sender, FocusEventArgs e)
        {
            ActivityFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void EntRemarks_Focused(object sender, FocusEventArgs e)
        {
            RemarksFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void EntOthers_Focused(object sender, FocusEventArgs e)
        {
            OthersFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void EntRemarks_Unfocused(object sender, FocusEventArgs e)
        {
            RemarksFrame.BorderColor = Color.FromHex("#f2f2f5");
        }

        private void EntOthers_Unfocused(object sender, FocusEventArgs e)
        {
            OthersFrame.BorderColor = Color.FromHex("#f2f2f5");
        }

        private void EntBreakfast_TextChanged(object sender, TextChangedEventArgs e)
        {
            double breakfast;

            if (String.IsNullOrEmpty(entBreakfast.Text))
            {
                breakfast = 0;
            }
            else
            {
                breakfast = Convert.ToDouble(entBreakfast.Text);
            }

            double lunch;

            if (String.IsNullOrEmpty(entLunch.Text))
            {
                lunch = 0;
            }
            else
            {
                lunch = Convert.ToDouble(entLunch.Text);
            }

            double dinner;

            if (String.IsNullOrEmpty(entDinner.Text))
            {
                dinner = 0;
            }
            else
            {
                dinner = Convert.ToDouble(entDinner.Text);
            }

            double hotel;

            if (String.IsNullOrEmpty(entHotel.Text))
            {
                hotel = 0;
            }
            else
            {
                hotel = Convert.ToDouble(entHotel.Text);
            }

            double cash;

            if (String.IsNullOrEmpty(entCash.Text))
            {
                cash = 0;
            }
            else
            {
                cash = Convert.ToDouble(entCash.Text);
            }

            var total = (breakfast + lunch + dinner + hotel + cash);

            entTotal.Text = total.ToString();
        }

        private void EntBreakfast_Focused(object sender, FocusEventArgs e)
        {
            BreakfastFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void EntBreakfast_Unfocused(object sender, FocusEventArgs e)
        {
            BreakfastFrame.BorderColor = Color.FromHex("#e8eaed");
        }

        private void EntLunch_Focused(object sender, FocusEventArgs e)
        {
            LunchFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void EntLunch_Unfocused(object sender, FocusEventArgs e)
        {
            LunchFrame.BorderColor = Color.FromHex("#e8eaed");
        }

        private void EntLunch_TextChanged(object sender, TextChangedEventArgs e)
        {
            double breakfast;

            if (String.IsNullOrEmpty(entBreakfast.Text))
            {
                breakfast = 0;
            }
            else
            {
                breakfast = Convert.ToDouble(entBreakfast.Text);
            }

            double lunch;

            if (String.IsNullOrEmpty(entLunch.Text))
            {
                lunch = 0;
            }
            else
            {
                lunch = Convert.ToDouble(entLunch.Text);
            }

            double dinner;

            if (String.IsNullOrEmpty(entDinner.Text))
            {
                dinner = 0;
            }
            else
            {
                dinner = Convert.ToDouble(entDinner.Text);
            }

            double hotel;

            if (String.IsNullOrEmpty(entHotel.Text))
            {
                hotel = 0;
            }
            else
            {
                hotel = Convert.ToDouble(entHotel.Text);
            }

            double cash;

            if (String.IsNullOrEmpty(entCash.Text))
            {
                cash = 0;
            }
            else
            {
                cash = Convert.ToDouble(entCash.Text);
            }

            var total = (breakfast + lunch + dinner + hotel + cash);

            entTotal.Text = total.ToString();
        }

        private void EntDinner_Focused(object sender, FocusEventArgs e)
        {
            DinnerFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void EntDinner_Unfocused(object sender, FocusEventArgs e)
        {
            DinnerFrame.BorderColor = Color.FromHex("#e8eaed");
        }

        private void EntDinner_TextChanged(object sender, TextChangedEventArgs e)
        {
            double breakfast;

            if (String.IsNullOrEmpty(entBreakfast.Text))
            {
                breakfast = 0;
            }
            else
            {
                breakfast = Convert.ToDouble(entBreakfast.Text);
            }

            double lunch;

            if (String.IsNullOrEmpty(entLunch.Text))
            {
                lunch = 0;
            }
            else
            {
                lunch = Convert.ToDouble(entLunch.Text);
            }

            double dinner;

            if (String.IsNullOrEmpty(entDinner.Text))
            {
                dinner = 0;
            }
            else
            {
                dinner = Convert.ToDouble(entDinner.Text);
            }

            double hotel;

            if (String.IsNullOrEmpty(entHotel.Text))
            {
                hotel = 0;
            }
            else
            {
                hotel = Convert.ToDouble(entHotel.Text);
            }

            double cash;

            if (String.IsNullOrEmpty(entCash.Text))
            {
                cash = 0;
            }
            else
            {
                cash = Convert.ToDouble(entCash.Text);
            }

            var total = (breakfast + lunch + dinner + hotel + cash);

            entTotal.Text = total.ToString();
        }

        private void EntHotel_Focused(object sender, FocusEventArgs e)
        {
            HotelFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void EntHotel_Unfocused(object sender, FocusEventArgs e)
        {
            HotelFrame.BorderColor = Color.FromHex("#e8eaed");
        }

        private void EntHotel_TextChanged(object sender, TextChangedEventArgs e)
        {
            double breakfast;

            if (String.IsNullOrEmpty(entBreakfast.Text))
            {
                breakfast = 0;
            }
            else
            {
                breakfast = Convert.ToDouble(entBreakfast.Text);
            }

            double lunch;

            if (String.IsNullOrEmpty(entLunch.Text))
            {
                lunch = 0;
            }
            else
            {
                lunch = Convert.ToDouble(entLunch.Text);
            }

            double dinner;

            if (String.IsNullOrEmpty(entDinner.Text))
            {
                dinner = 0;
            }
            else
            {
                dinner = Convert.ToDouble(entDinner.Text);
            }

            double hotel;

            if (String.IsNullOrEmpty(entHotel.Text))
            {
                hotel = 0;
            }
            else
            {
                hotel = Convert.ToDouble(entHotel.Text);
            }

            double cash;

            if (String.IsNullOrEmpty(entCash.Text))
            {
                cash = 0;
            }
            else
            {
                cash = Convert.ToDouble(entCash.Text);
            }

            var total = (breakfast + lunch + dinner + hotel + cash);

            entTotal.Text = total.ToString();
        }

        private void EntCash_Focused(object sender, FocusEventArgs e)
        {
            CashFrame.BorderColor = Color.FromHex("#b0b0b5");
        }

        private void EntCash_Unfocused(object sender, FocusEventArgs e)
        {
            CashFrame.BorderColor = Color.FromHex("#e8eaed");
        }

        private void EntCash_TextChanged(object sender, TextChangedEventArgs e)
        {
            double breakfast;

            if (String.IsNullOrEmpty(entBreakfast.Text))
            {
                breakfast = 0;
            }
            else
            {
                breakfast = Convert.ToDouble(entBreakfast.Text);
            }

            double lunch;

            if (String.IsNullOrEmpty(entLunch.Text))
            {
                lunch = 0;
            }
            else
            {
                lunch = Convert.ToDouble(entLunch.Text);
            }

            double dinner;

            if (String.IsNullOrEmpty(entDinner.Text))
            {
                dinner = 0;
            }
            else
            {
                dinner = Convert.ToDouble(entDinner.Text);
            }

            double hotel;

            if (String.IsNullOrEmpty(entHotel.Text))
            {
                hotel = 0;
            }
            else
            {
                hotel = Convert.ToDouble(entHotel.Text);
            }

            double cash;

            if (String.IsNullOrEmpty(entCash.Text))
            {
                cash = 0;
            }
            else
            {
                cash = Convert.ToDouble(entCash.Text);
            }

            var total = (breakfast + lunch + dinner + hotel + cash);

            entTotal.Text = total.ToString();
        }

        private void EntTransportation_Focused(object sender, FocusEventArgs e)
        {

        }

        private void EntTransportation_Unfocused(object sender, FocusEventArgs e)
        {

        }

        private void EntTransportation_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}