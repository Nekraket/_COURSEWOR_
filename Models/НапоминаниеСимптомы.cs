namespace Курсовая.Models;

public partial class НапоминаниеСимптомы
{
    public int PkIdНапоминанияСимптомы { get; set; }

    public int FkIdСимптомы { get; set; }

    public int Часы { get; set; }

    public int Минуты { get; set; }

    public virtual Симптомы FkIdСимптомыNavigation { get; set; } = null!;

    public virtual ICollection<ЗафиксированныеСимптомы> ЗафиксированныеСимптомыs { get; set; } = new List<ЗафиксированныеСимптомы>();
}
