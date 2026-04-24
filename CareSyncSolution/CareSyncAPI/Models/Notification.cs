using System;
using System.Collections.Generic;

namespace CareSyncAPI.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string Type { get; set; } = null!;

    public bool IsRead { get; set; }

    public int? RelatedEntityId { get; set; }

    public string? RelatedEntityType { get; set; }

    public DateTime CreatedAt { get; set; }
}
