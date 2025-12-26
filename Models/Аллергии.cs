namespace Курсовая.Models;

public partial class Аллергии
{
    public int PkIdАллергии { get; set; }

    public int? FkIdПолучателя { get; set; }

    public int? FkIdПользователя { get; set; }

    public string Аллерген { get; set; } = null!;

    public virtual ПолучателиУхода? FkIdПолучателяNavigation { get; set; }

    public virtual Пользователь? FkIdПользователяNavigation { get; set; }
}
