using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TBSMobileApp.Data
{
    [Table("tblContacts")]
    public class ContactsTable
    {
        [PrimaryKey, MaxLength(50)]
        public string ContactID { get; set; }
        [MaxLength(300)]
        public string FileAs { get; set; }
        [MaxLength(100)]
        public string FirstName { get; set; }
        [MaxLength(100)]
        public string MiddleName { get; set; }
        [MaxLength(100)]
        public string LastName { get; set; }
        public DateTime LastSync { get; set; }
        public DateTime LastUpdated { get; set; }
        public int Deleted { get; set; }
        public string CustomerPicker
        {
            get
            {
                return $"{ContactID}-{FileAs}";
            }
        }
    }
}
