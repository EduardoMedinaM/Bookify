using Bookify.Domain.Apartments;
using Bookify.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Configurations;

internal sealed class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
{
    public void Configure(EntityTypeBuilder<Apartment> builder)
    {
        // EF Fluent configuration

        // maps Apartment model into apartments table
        builder.ToTable("apartments");

        // defines a PK
        builder.HasKey(apartment => apartment.Id);

        // maps an Address Value Object. EF will map the Value Object in a set of columns
        // iow. Address columns will be in the apartments table
        builder.OwnsOne(apartment => apartment.Address);

        // Constraints plus conversion to the primitive type
        builder.Property(apartment => apartment.Name)
            .HasMaxLength(200)
            .HasConversion(name => name.Value, value => new Name(value));

        builder.Property(apartment => apartment.Description)
            .HasMaxLength(2000)
            .HasConversion(description => description.Value, value => new Description(value));

        // Value Object mapping
        builder.OwnsOne(apartment => apartment.Price, priceBuilder =>
        {
            // maps the currency code to the DB
            priceBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });

        builder.OwnsOne(apartment => apartment.CleaningFee, priceBuilder =>
        {
            priceBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });

        // optimistic concurrency support
        builder.Property<uint>("Version").IsRowVersion();
    }
}
