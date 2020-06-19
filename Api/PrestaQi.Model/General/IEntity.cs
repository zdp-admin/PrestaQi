namespace PrestaQi.Model.General
{
    public interface IEntity
    {
        object id { get; set; }
    }

    public interface IEntity<TId> : IEntity
    {
        new TId id { get; set; }
    }
}
