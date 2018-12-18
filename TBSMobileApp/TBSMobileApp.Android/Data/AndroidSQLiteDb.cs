using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;
using TBSMobileApp.Data;
using TBSMobileApp.Droid.Data;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidSQLiteDb))]

namespace TBSMobileApp.Droid.Data
{
    public class AndroidSQLiteDb : ISQLiteDB
    {
        public SQLiteAsyncConnection GetConnection()
        {
            var dbFileName = "backend.db3";
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentsPath, dbFileName);

            return new SQLiteAsyncConnection(path);
        }
    }
}