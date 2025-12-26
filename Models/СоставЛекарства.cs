namespace Курсовая.Models;

public partial class СоставЛекарства
{
    public int PkIdСостава { get; set; }

    public string НазваниеСоставляющей { get; set; } = null!;

    public virtual ICollection<СоставВЛекарстве> СоставВЛекарствеs { get; set; } = new List<СоставВЛекарстве>();
}
