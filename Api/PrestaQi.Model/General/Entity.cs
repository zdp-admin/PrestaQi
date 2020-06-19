using System;

namespace PrestaQi.Model.General
{
    public class Entity<T> : IEntity<T>
    {
        public T id { get; set; }
        object IEntity.id { get { return this.id; } set { } }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
