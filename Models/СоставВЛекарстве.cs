namespace Курсовая.Models;

public partial class СоставВЛекарстве
{
    public int PkIdСоставаВлекарстве { get; set; }

    public int FkIdЛекарства { get; set; }

    public int FkIdСоставаЛекарства { get; set; }

    public virtual Лекарства FkIdЛекарстваNavigation { get; set; } = null!;

    public virtual СоставЛекарства FkIdСоставаЛекарстваNavigation { get; set; } = null!;
}
