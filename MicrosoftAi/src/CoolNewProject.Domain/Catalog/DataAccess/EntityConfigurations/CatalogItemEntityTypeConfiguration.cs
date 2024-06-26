﻿using CoolNewProject.Domain.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoolNewProject.Domain.Catalog.DataAccess.EntityConfigurations;

internal class CatalogItemEntityTypeConfiguration
    : IEntityTypeConfiguration<CatalogItem> {
    public void Configure(EntityTypeBuilder<CatalogItem> builder) {
        builder.ToTable("Catalog");

        builder.Property(ci => ci.Name)
            .HasMaxLength(50);

        builder.Ignore(ci => ci.PictureUri);

        builder.Property(ci => ci.Embedding)
            .HasColumnType($"vector({CatalogConstants.AiVectorSize})");

        builder.HasOne(ci => ci.CatalogBrand)
            .WithMany();

        builder.HasOne(ci => ci.CatalogType)
            .WithMany();

        builder.HasIndex(ci => ci.Name);
    }
}
