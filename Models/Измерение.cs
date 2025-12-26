namespace Курсовая.Models;

public partial class Измерение
{
    public int PkIdИзмерения { get; set; }

    public int FkIdПозиции { get; set; }

    public int FkIdТипИзмерения { get; set; }

    public int FkIdПериодичности { get; set; }

    public int ЗаписейВДень { get; set; }

    public virtual Периодичность FkIdПериодичностиNavigation { get; set; } = null!;

    public virtual ПозицияЗаписи FkIdПозицииNavigation { get; set; } = null!;

    public virtual ТипИзмерения FkIdТипИзмеренияNavigation { get; set; } = null!;

    public virtual ICollection<НапоминаниеИзмерения> НапоминаниеИзмеренияs { get; set; } = new List<НапоминаниеИзмерения>();
}
