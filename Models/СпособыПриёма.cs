namespace Курсовая.Models;

public partial class СпособыПриёма
{
    public int PkIdСпособаПриёма { get; set; }

    public string Тип { get; set; } = null!;

    public virtual ICollection<Лекарства> Лекарстваs { get; set; } = new List<Лекарства>();
}
