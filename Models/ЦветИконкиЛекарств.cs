namespace Курсовая.Models;

public partial class ЦветИконкиЛекарств
{
    public int PkIdЦветИконки { get; set; }

    public string Цвет { get; set; } = null!;

    public virtual ICollection<ИконкиЛекарств> ИконкиЛекарствs { get; set; } = new List<ИконкиЛекарств>();
}
