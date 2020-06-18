using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using System.Text;

namespace PrestaQi.Model
{
    [Table("usercapitals")]
    public class UserCapital : Entity<int>
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
        public int Period_Id { get; set; }
        [Column("notice_mail")]
        public bool Notice_Mail { get; set; }
        [Column("user_capital_id")]
        public int User_Capital_Id { get; set; }
        [Column("created_by")]
        public int Created_By { get; set; }
    }
}
