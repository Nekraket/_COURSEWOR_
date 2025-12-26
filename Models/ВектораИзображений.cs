namespace Курсовая.Models;

public partial class ВектораИзображений
{
    public int PkIdВектора { get; set; }

    public string? Название { get; set; }

    public string Вектор { get; set; } = null!;

    public int? FkIdКатегорииОтслеж { get; set; }

    public virtual КатегорииОтслеживания? FkIdКатегорииОтслежNavigation { get; set; }

    public virtual ICollection<ИконкиЛекарств> ИконкиЛекарствs { get; set; } = new List<ИконкиЛекарств>();

    public virtual ICollection<ТипИзмерения> ТипИзмеренияs { get; set; } = new List<ТипИзмерения>();
}
