namespace Курсовая.Models;

public partial class НапоминаниеИзмерения
{
    public int PkIdНапоминанияИзмерения { get; set; }

    public int FkIdИзмерения { get; set; }

    public int Часы { get; set; }

    public int Минуты { get; set; }

    public virtual Измерение FkIdИзмеренияNavigation { get; set; } = null!;

    public virtual ICollection<ЗначенияИзмерения> ЗначенияИзмеренияs { get; set; } = new List<ЗначенияИзмерения>();
}
