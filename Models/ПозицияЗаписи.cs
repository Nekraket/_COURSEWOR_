namespace Курсовая.Models;

public partial class ПозицияЗаписи
{
    public int PkIdПозиции { get; set; }

    public int FkIdПользователя { get; set; }

    public int? FkIdПолучателя { get; set; }

    public int FkIdКатегорииОтслеж { get; set; }

    public DateTime ДатаСоздания { get; set; }

    public bool Активность { get; set; }

    public virtual КатегорииОтслеживания FkIdКатегорииОтслежNavigation { get; set; } = null!;

    public virtual ПолучателиУхода? FkIdПолучателяNavigation { get; set; }

    public virtual Пользователь FkIdПользователяNavigation { get; set; } = null!;

    public virtual ICollection<Измерение> Измерениеs { get; set; } = new List<Измерение>();

    public virtual ICollection<Лекарства> Лекарстваs { get; set; } = new List<Лекарства>();

    public virtual ICollection<Симптомы> Симптомыs { get; set; } = new List<Симптомы>();
}
