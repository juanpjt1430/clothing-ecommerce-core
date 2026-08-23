using ClothingEcommerce.Domain.Common;
using FluentAssertions;

namespace ClothingEcommerce.UnitTests.Domain.Common;

public class BaseEntityTests
{
    [Fact]
    public void Constructor_ShouldGenerateNewGuidId()
    {
        var entity = new TestEntity();

        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void TwoEntities_ShouldHaveDifferentIds()
    {
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        entity1.Id.Should().NotBe(entity2.Id);
    }

    private class TestEntity : BaseEntity { }
}
