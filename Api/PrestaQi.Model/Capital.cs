using PrestaQi.Model.General;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model
{
    [Table("capitals")]
    public partial class Capital : Entity<int>
    { 
        [Column("amount")]
        public double Amount { get; set; }
        [Column("interest_rate")]
        public int Interest_Rate { get; set; }
        [Column("investment_horizon")]
        public int Investment_Horizon { get; set; }
        [Column("bussiness_day")]
        public int Bussiness_Day { get; set; }
        [Column("period_id")]
        public int period_id { get; set; }
        [Column("notice_mail")]
        public bool Notice_Mail { get; set; }
        [Column("investor_id")]
        public int investor_id { get; set; }
        [Column("created_by")]
        public int Created_By { get; set; }
        [Column("start_date")]
        public DateTime Start_Date { get; set; }
        [Column("end_date")]
        public DateTime End_Date { get; set; }
        [Column("default_interest")]
        public int Default_Interest { get; set; }
        [Column("capital_status")]
        public int Capital_Status { get; set; }
        [Column("investment_status")]
        public int Investment_Status { get; set; }
        [Column("file_name")]
        public string File_Name { get; set; }
        [Column("file_byte")]
        public byte[] File_Byte { get; set; }
        [Column("extension_day")]
        public int Extension_day { get; set; }

        [ForeignKey("investor_id")]
        public virtual Investor Investor { get; set; }
    }
}
