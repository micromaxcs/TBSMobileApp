using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TBSMobileApp.Data
{
    [Table("tblActivity")]
    public class ActivityTable
    {
        [PrimaryKey, MaxLength(50)]
        public string ActivityID { get; set; }
        [MaxLength(50)]
        public string ActivityDescription { get; set; }
        public DateTime LastSync { get; set; }
        public DateTime LastUpdated { get; set; }
        public int Deleted { get; set; }
        public string ActivityPicker
        {
            get
            {
                return $"{ActivityID}-{ActivityDescription}";
            }
        }

    }
}
