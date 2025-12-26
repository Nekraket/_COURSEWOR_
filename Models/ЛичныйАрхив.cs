namespace Курсовая.Models;

public partial class ЛичныйАрхив
{
    public int PkIdЛичногоАрхива { get; set; }

    public int FkIdКатегорииАрхива { get; set; }

    public int? FkIdПользователя { get; set; }

    public int? FkIdПолучателя { get; set; }

    public virtual КатегорияАрхива FkIdКатегорииАрхиваNavigation { get; set; } = null!;

    public virtual ПолучателиУхода? FkIdПолучателяNavigation { get; set; }

    public virtual Пользователь? FkIdПользователяNavigation { get; set; }

    public virtual ICollection<Лекарства> Лекарстваs { get; set; } = new List<Лекарства>();
}
