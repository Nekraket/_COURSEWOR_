namespace Курсовая.Models;

public partial class ЗначенияИзмерения
{
    public int PkIdЗначениеИзмерения { get; set; }

    public DateTime ДатаЗаписи { get; set; }

    public double Значение { get; set; }

    public string? Заметка { get; set; }

    public int? FkIdНапоминанияИзмерения { get; set; }

    public virtual НапоминаниеИзмерения? FkIdНапоминанияИзмеренияNavigation { get; set; }
}
