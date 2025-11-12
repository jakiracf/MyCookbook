namespace CS101;

interface IRepository<T> where T : Entity
{
    List<T> List();
    T? GetById(Guid id);
    void Add(T entity);
    bool Remove(Guid id);
}