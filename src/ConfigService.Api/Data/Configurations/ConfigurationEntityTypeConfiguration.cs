using System.Linq;
using System.Text.Json;
using ConfigService.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConfigService.Api.Data.Configurations;

public sealed class ConfigurationEntityTypeConfiguration : IEntityTypeConfiguration<Configuration>
{
    private static readonly ValueComparer<JsonDocument> ConfigComparer = new(
        (left, right) =>
            ReferenceEquals(left, right)
            || (left != null && right != null && Canonicalize(left.RootElement) == Canonicalize(right.RootElement)),
        value => Canonicalize(value.RootElement).GetHashCode(),
        value => JsonDocument.Parse(value.RootElement.GetRawText()));

    private static string Canonicalize(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => "{" + string.Join(
            ",",
            element.EnumerateObject()
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(p => JsonSerializer.Serialize(p.Name) + ":" + Canonicalize(p.Value))) + "}",
        JsonValueKind.Array => "[" + string.Join(
            ",", element.EnumerateArray().Select(Canonicalize)) + "]",
        _ => element.GetRawText(),
    };

    public void Configure(EntityTypeBuilder<Configuration> builder)
    {
        builder.ToTable("configurations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.ApplicationId)
            .HasColumnName("application_id")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Comments)
            .HasColumnName("comments")
            .HasMaxLength(1024);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(x => x.Config)
            .HasColumnName("config")
            .HasColumnType("jsonb")
            .IsRequired()
            .Metadata.SetValueComparer(ConfigComparer);

        builder.HasOne(x => x.Application)
            .WithMany(x => x.Configurations)
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ApplicationId, x.Name }).IsUnique();
    }
}
