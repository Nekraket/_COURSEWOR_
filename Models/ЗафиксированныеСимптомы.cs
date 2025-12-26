namespace Курсовая.Models;

public partial class ЗафиксированныеСимптомы
{
    public int PkIdЗафСимптомы { get; set; }

    public DateTime ДатаЗаписи { get; set; }

    public int? ОценкаСамочувствия { get; set; }

    public string? Заметка { get; set; }

    public int? FkIdНапоминанияСимптомы { get; set; }

    public virtual НапоминаниеСимптомы? FkIdНапоминанияСимптомыNavigation { get; set; }
}
