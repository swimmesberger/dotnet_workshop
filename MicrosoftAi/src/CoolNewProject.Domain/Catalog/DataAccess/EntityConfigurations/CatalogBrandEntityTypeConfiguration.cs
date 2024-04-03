﻿using CoolNewProject.Domain.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoolNewProject.Domain.Catalog.DataAccess.EntityConfigurations;

internal class CatalogBrandEntityTypeConfiguration
    : IEntityTypeConfiguration<CatalogBrand> {
    public void Configure(EntityTypeBuilder<CatalogBrand> builder) {
        builder.ToTable("CatalogBrand");

        builder.Property(cb => cb.Brand)
            .HasMaxLength(100);
    }
}
