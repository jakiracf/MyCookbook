namespace CS101;

public class Entity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }

    public Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public virtual string Describe() => $"{Id} @ {CreatedAt:O}";
}
