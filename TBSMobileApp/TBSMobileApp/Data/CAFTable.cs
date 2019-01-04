using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TBSMobileApp.Data
{
    [Table("tblCaf")]
    public class CAFTable
    {
        [PrimaryKey, MaxLength(50)]
        public string CAFNo { get; set; }
        [MaxLength(50)]
        public string EmployeeID { get; set; }
        [MaxLength(50)]
        public string ContactPersonID { get; set; }
        public DateTime CAFDate { get; set; }
        [MaxLength(50)]
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
        [MaxLength(1000)]
        public string Remarks { get; set; }
        [MaxLength(1000)]
        public string OtherConcern { get; set; }
        public string Signature { get; set; }
        public string MobileSignature { get; set; }
        public DateTime LastSync { get; set; }
        public DateTime LastUpdated { get; set; }
        public int Deleted { get; set; }
        public string ActNumber
        {
            get
            {
                return $"Activity Number: {CAFNo}";
            }
        }

        public string ActDate
        {
            get
            {
                return $"Activity Date: { CAFDate.Date.ToString("MM/dd/yyyy")}";
            }
        }
    }
}
