using CoolNewProject.Domain.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoolNewProject.Domain.Catalog.DataAccess.EntityConfigurations;

internal class CatalogTypeEntityTypeConfiguration
    : IEntityTypeConfiguration<CatalogType> {
    public void Configure(EntityTypeBuilder<CatalogType> builder) {
        builder.ToTable("CatalogType");

        builder.Property(cb => cb.Type)
            .HasMaxLength(100);
    }
}
