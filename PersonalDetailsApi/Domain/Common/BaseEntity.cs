namespace PersonalDetailsApi.Domain.Common;

/// <summary>
/// Root of every persisted entity. Demonstrates an <c>abstract</c> class:
/// it cannot be instantiated directly, only inherited from.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Time-ordered (version 7) GUID, so new rows sort naturally and index well.
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();
}

/// <summary>An entity whose creation and modification times are tracked automatically.</summary>
public interface IAuditable
{
    DateTime CreatedAtUtc { get; set; }
    DateTime? UpdatedAtUtc { get; set; }
}

/// <summary>An entity that is flagged as deleted instead of being physically removed.</summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAtUtc { get; set; }
}

/// <summary>Base class combining an identity with audit fields (inheritance + interface implementation).</summary>
public abstract class AuditableEntity : BaseEntity, IAuditable
{
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
