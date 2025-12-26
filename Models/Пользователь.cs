namespace Курсовая.Models;

public partial class Пользователь
{
    public int PkIdПользователя { get; set; }

    public string Логин { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Аватар { get; set; }

    public DateOnly? ДатаРождения { get; set; }

    public string? Пол { get; set; }

    public virtual ICollection<Аллергии> Аллергииs { get; set; } = new List<Аллергии>();

    public virtual ICollection<АрхивРецептов> АрхивРецептовs { get; set; } = new List<АрхивРецептов>();

    public virtual ICollection<ЛичныйАрхив> ЛичныйАрхивs { get; set; } = new List<ЛичныйАрхив>();

    public virtual ICollection<ПозицияЗаписи> ПозицияЗаписиs { get; set; } = new List<ПозицияЗаписи>();

    public virtual ICollection<ПолучателиУхода> ПолучателиУходаs { get; set; } = new List<ПолучателиУхода>();
}
