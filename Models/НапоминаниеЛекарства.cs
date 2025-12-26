namespace Курсовая.Models;

public partial class НапоминаниеЛекарства
{
    public int PkIdНапоминанияЛекарства { get; set; }

    public int FkIdЛекарства { get; set; }

    public int Часы { get; set; }

    public int Минуты { get; set; }

    public virtual Лекарства FkIdЛекарстваNavigation { get; set; } = null!;

    public virtual ICollection<ФиксацияПриёма> ФиксацияПриёмаs { get; set; } = new List<ФиксацияПриёма>();
}
