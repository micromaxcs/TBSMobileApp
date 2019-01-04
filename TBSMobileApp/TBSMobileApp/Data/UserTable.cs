using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TBSMobileApp.Data
{
    [Table("tblUser")]
    public class UserTable
    {
        [PrimaryKey, MaxLength(50)]
        public string ContactID { get; set; }
        [MaxLength(10)]
        public string UserID { get; set; }
        [MaxLength(10)]
        public string UserPassword { get; set; }
        public DateTime LastSync { get; set; }
        public DateTime LastUpdated { get; set; }
        public int Deleted { get; set; }
    }
}
