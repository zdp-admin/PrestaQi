using System;
using System.Collections.Generic;
using System.Text;

namespace PrestaQi.Model.Dto.Output.Pliis
{
    public class DataUser
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string companyName { get; set; }
        public string rfc { get; set; }
        public string birthDate { get; set; }
        public int age { get; set; }
        public int genderId { get; set; }
        public bool enabled { get; set; }
        public string mail { get; set; }
        public string address { get; set; }
        public string colony { get; set; }
        public string municipality { get; set; }
        public string grossMonthlySalary { get; set; }

        public float parseGrossMonthlySalary()
        {
            float.TryParse(this.grossMonthlySalary, out float result);

            return result;
        }
    }
}
