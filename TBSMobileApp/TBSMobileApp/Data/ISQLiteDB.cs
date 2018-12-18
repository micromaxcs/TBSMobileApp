using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TBSMobileApp.Data
{
    public interface ISQLiteDB
    {
        SQLiteAsyncConnection GetConnection();
    }
}
