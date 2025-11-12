namespace CS101;

class InMemoryRepository<T> : IRepository<T>
    where T : Entity
{
    private readonly Dictionary<Guid, T> _store = new();

    public List<T> List() => _store.Values.ToList();

    public T? GetById(Guid id)
    {
        return _store.TryGetValue(id, out var entity) ? entity : null;
    }

    public void Add(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));
        if (!_store.TryAdd(entity.Id, entity))
            throw new ArgumentException($"Entity with id {entity.Id} already exists");
    }

    public bool Remove(Guid id) => _store.Remove(id);
}
