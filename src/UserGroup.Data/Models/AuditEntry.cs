using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace UserGroup.Data.Models;

public class AuditEntry
{
    public EntityEntry Entry { get; set; }
    public int UserId { get; set; }
    public string TableName { get; set; } = null!;
    public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
    public AuditType AuditType { get; set; }
    public List<string> ChangedColumns { get; } = new List<string>();

    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
    }

    public AuditLog ToAuditLog()
    {
        var auditLog = new AuditLog();
        auditLog.UserId = UserId;
        auditLog.Type = AuditType.ToString();
        auditLog.TableName = TableName;
        auditLog.DateTime = DateTime.UtcNow;
        auditLog.PrimaryKey = System.Text.Json.JsonSerializer.Serialize(KeyValues);
        auditLog.OldValues = OldValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(OldValues);
        auditLog.NewValues = NewValues.Count == 0 ? null! : System.Text.Json.JsonSerializer.Serialize(NewValues);
        auditLog.AffectedColumns = ChangedColumns.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(ChangedColumns);

        return auditLog;
    }
}