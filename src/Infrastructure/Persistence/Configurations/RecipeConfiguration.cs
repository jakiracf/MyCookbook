using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCookbook.Domain.Entities;

namespace MyCookbook.Infrastructure.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> b)
    {
        b.ToTable("recipes");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(200);

        b.Property(x => x.PrepMinutes).HasDefaultValue(0);

        b.Property(x => x.Popularity).HasDefaultValue(0);

        b.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");

        b.Property(x => x.ExternalId).HasMaxLength(50);
        b.Property(x => x.ExternalSource).HasMaxLength(50);

        // Prevent importing the same MealDB recipe twice
        b.HasIndex(x => new { x.ExternalSource, x.ExternalId })
            .IsUnique()
            .HasFilter("\"ExternalSource\" IS NOT NULL AND \"ExternalId\" IS NOT NULL");

        b.HasIndex(x => x.Name);

        // Owned collection Ã¢â€ â€™ separate table recipe_ingredients
        b.OwnsMany(
            x => x.Ingredients,
            nb =>
            {
                nb.ToTable("recipe_ingredients");
                nb.WithOwner().HasForeignKey("recipe_id");

                nb.Property<int>("key");
                nb.HasKey("key");

                nb.Property(p => p.Name).IsRequired().HasMaxLength(100);

                nb.Property(p => p.Measure).HasMaxLength(100);

                nb.HasIndex("recipe_id");
            }
        );
    }
}
