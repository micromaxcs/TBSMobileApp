using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TBSMobileApp.Data
{
    [Table("tblSystemSerial")]
    public class SystemSerialTable
    {
        [MaxLength(50)]
        public string SerialNumber { get; set; }
        public DateTime DateStart { get; set; }
        [MaxLength(50)]
        public string NoOfDays { get; set; }
        [MaxLength(2)]
        public int Trials { get; set; }
        [MaxLength(100)]
        public string InputSerialNumber { get; set; }
        public DateTime LastSync { get; set; }
        public DateTime LastUpdated { get; set; }
        public int Deleted { get; set; }
    }
}
