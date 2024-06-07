namespace UserGroup.Data.Models;

public class FingerPrintEntityBase : EntityBase
{
    public DateTime? CreatedOn { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public int ModifiedBy { get; set; }
}