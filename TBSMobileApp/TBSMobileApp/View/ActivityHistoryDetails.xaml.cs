using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBSMobileApp.Data;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TBSMobileApp.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ActivityHistoryDetails : ContentPage
	{
		public ActivityHistoryDetails (CAFTable item)
		{
			InitializeComponent ();
            GetCafDetails(item.CAFNo);
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

        public void GetCafDetails(string caf)
        {
            try
            {
                var db = DependencyService.Get<ISQLiteDB>();
                var conn = db.GetConnection();

                var getCaf = conn.QueryAsync<CAFTable>("SELECT * FROM tblCaf WHERE CAFNo=?", caf);
                var contactResultCount = getCaf.Result.Count;

                if (contactResultCount > 0)
                {
                    var cafResult = getCaf.Result[0];
                    lblCafNo.Text = cafResult.CAFNo;
                    lblBreakfast.Text = cafResult.Breakfast.ToString();
                    lblLunch.Text = cafResult.Lunch.ToString();
                    lblDinner.Text = cafResult.Dinner.ToString();
                    lblHotel.Text = cafResult.HotelAccommodation.ToString();
                    lblTransportation.Text = cafResult.TransportationFare.ToString();
                    lblCash.Text = cafResult.CashAdvance.ToString();
                    lblTotal.Text = ((cafResult.Breakfast + cafResult.Lunch + cafResult.Dinner + cafResult.HotelAccommodation + cafResult.TransportationFare + cafResult.CashAdvance)).ToString();
                    lblDate.Text = cafResult.CAFDate.ToString("MM/dd/yyyy");
                    lblStartTime.Text = cafResult.StartTime.ToString("hh:mm:ss");
                    lblEndTime.Text = cafResult.EndTime.ToString("hh:mm:ss");
                    lblOtherConcern.Text = cafResult.OtherConcern;
                    lblRemarks.Text = cafResult.Remarks;
                    lblLastUpdated.Text = cafResult.LastUpdated.ToString("MM/dd/yyyy HH:mm:ss");
                    lblLastSync.Text = cafResult.LastSync.ToString("MM/dd/yyyy HH:mm:ss");

                    var getActivity = conn.QueryAsync<ActivityTable>("SELECT ActivityDescription FROM tblActivity WHERE ActivityID=?", cafResult.ActivityID);
                    var activityResultCount = getActivity.Result.Count;

                    if(activityResultCount > 0)
                    {
                        var activityResult = getActivity.Result[0];
                        lblActivity.Text = activityResult.ActivityDescription;
                    }

                    var getCustomer = conn.QueryAsync<ContactsTable>("SELECT FileAs FROM tblContacts WHERE ContactID=?", cafResult.CustomerID);
                    var customerResultCount = getCustomer.Result.Count;

                    if (customerResultCount > 0)
                    {
                        var customerResult = getCustomer.Result[0];
                        lblCustomerName.Text = customerResult.FileAs;
                    }

                    var getContactPerson= conn.QueryAsync<ContactsTable>("SELECT FileAs FROM tblContacts WHERE ContactID=?", cafResult.ContactPersonID);
                    var contactPersonResultCount = getContactPerson.Result.Count;

                    if (contactPersonResultCount > 0)
                    {
                        var contactPersonResult = getContactPerson.Result[0];
                        lblContactPerson.Text = contactPersonResult.FileAs;
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