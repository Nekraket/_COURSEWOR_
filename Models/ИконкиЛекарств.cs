namespace Курсовая.Models;

public partial class ИконкиЛекарств
{
    public int PkIdИконки { get; set; }

    public int? FkIdВектора { get; set; }

    public int? FkIdЦветИконки { get; set; }

    public virtual ВектораИзображений? FkIdВектораNavigation { get; set; }

    public virtual ЦветИконкиЛекарств? FkIdЦветИконкиNavigation { get; set; }

    public virtual ICollection<Лекарства> Лекарстваs { get; set; } = new List<Лекарства>();
}
