namespace Курсовая.Models;

public partial class ФиксацияПриёма
{
    public int PkIdФиксПриём { get; set; }

    public DateTime ДатаПриёма { get; set; }

    public int? FkIdНапоминанияЛекарства { get; set; }

    public virtual НапоминаниеЛекарства? FkIdНапоминанияЛекарстваNavigation { get; set; }
}
