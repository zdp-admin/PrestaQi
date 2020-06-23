using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrestaQi.Model.General
{
    public class Entity<T> : IEntity<T>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public T id { get; set; }
        object IEntity.id { get { return this.id; } set { } }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
