﻿using PrestaQi.Model.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrestaQi.Model
{
    [Table("genders")]
    public class Gender : Entity<int>
    {
        [Column("description")]
        public string Description { get; set; }
        [Column("enabled")]
        public bool Enabled { get; set; }
    }
}