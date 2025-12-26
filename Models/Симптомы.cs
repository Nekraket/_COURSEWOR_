namespace Курсовая.Models;

public partial class Симптомы
{
    public int PkIdСимптомы { get; set; }

    public int FkIdПозиции { get; set; }

    public int FkIdПериодичности { get; set; }

    public string Название { get; set; } = null!;

    public int? ЗаписейВДень { get; set; }

    public virtual Периодичность FkIdПериодичностиNavigation { get; set; } = null!;

    public virtual ПозицияЗаписи FkIdПозицииNavigation { get; set; } = null!;

    public virtual ICollection<НапоминаниеСимптомы> НапоминаниеСимптомыs { get; set; } = new List<НапоминаниеСимптомы>();
}
