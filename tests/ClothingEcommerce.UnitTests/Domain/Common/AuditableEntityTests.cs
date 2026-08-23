using ClothingEcommerce.Domain.Common;
using FluentAssertions;

namespace ClothingEcommerce.UnitTests.Domain.Common;

public class AuditableEntityTests
{
    [Fact]
    public void Constructor_ShouldGenerateNewGuidId()
    {
        var entity = new TestAuditableEntity();

        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreatedOnUtc_ShouldBeSettable()
    {
        var now = DateTime.UtcNow;
        var entity = new TestAuditableEntity { CreatedOnUtc = now };

        entity.CreatedOnUtc.Should().Be(now);
    }

    [Fact]
    public void ModifiedOnUtc_ShouldBeNullByDefault()
    {
        var entity = new TestAuditableEntity();

        entity.ModifiedOnUtc.Should().BeNull();
    }

    [Fact]
    public void ModifiedOnUtc_ShouldBeSettable()
    {
        var now = DateTime.UtcNow;
        var entity = new TestAuditableEntity { ModifiedOnUtc = now };

        entity.ModifiedOnUtc.Should().Be(now);
    }

    private class TestAuditableEntity : AuditableEntity { }
}
