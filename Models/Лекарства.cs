namespace Курсовая.Models;

public partial class Лекарства
{
    public int PkIdЛекарства { get; set; }

    public string Название { get; set; } = null!;

    public string? Назначение { get; set; }

    public int? FkIdЛичныйАрхив { get; set; }

    public int? FkIdПозиции { get; set; }

    public int FkIdИконки { get; set; }

    public int? FkIdПериодичности { get; set; }

    public string? Фото { get; set; }

    public int FkIdСпособаПриёма { get; set; }

    public int? ПриёмовВДень { get; set; }

    public int? Дозировка { get; set; }

    public string? Комментарий { get; set; }

    public bool НапоминанияЗапас { get; set; }

    public int МинЗапас { get; set; }

    public int ТекущийЗапас { get; set; }

    public int? ДлительностьПриёма { get; set; }

    public virtual ИконкиЛекарств FkIdИконкиNavigation { get; set; } = null!;

    public virtual ЛичныйАрхив? FkIdЛичныйАрхивNavigation { get; set; }

    public virtual Периодичность? FkIdПериодичностиNavigation { get; set; }

    public virtual ПозицияЗаписи? FkIdПозицииNavigation { get; set; }

    public virtual СпособыПриёма FkIdСпособаПриёмаNavigation { get; set; } = null!;

    public virtual ICollection<НапоминаниеЛекарства> НапоминаниеЛекарстваs { get; set; } = new List<НапоминаниеЛекарства>();

    public virtual ICollection<СоставВЛекарстве> СоставВЛекарствеs { get; set; } = new List<СоставВЛекарстве>();
}
