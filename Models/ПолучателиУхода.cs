namespace Курсовая.Models;

public partial class ПолучателиУхода
{
    public int PkIdПолучателя { get; set; }

    public string Имя { get; set; } = null!;

    public string? Аватар { get; set; }

    public int FkIdПользователя { get; set; }

    public virtual Пользователь FkIdПользователяNavigation { get; set; } = null!;

    public virtual ICollection<Аллергии> Аллергииs { get; set; } = new List<Аллергии>();

    public virtual ICollection<ЛичныйАрхив> ЛичныйАрхивs { get; set; } = new List<ЛичныйАрхив>();

    public virtual ICollection<ПозицияЗаписи> ПозицияЗаписиs { get; set; } = new List<ПозицияЗаписи>();
}
