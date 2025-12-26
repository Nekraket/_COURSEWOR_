namespace Курсовая.Models;

public partial class ТипИзмерения
{
    public int PkIdТипИзмерения { get; set; }

    public string Название { get; set; } = null!;

    public string ЕдИзмерения { get; set; } = null!;

    public int? FkIdВектор { get; set; }

    public virtual ВектораИзображений? FkIdВекторNavigation { get; set; }

    public virtual ICollection<Измерение> Измерениеs { get; set; } = new List<Измерение>();
}
