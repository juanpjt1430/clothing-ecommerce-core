namespace ClothingEcommerce.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
}
