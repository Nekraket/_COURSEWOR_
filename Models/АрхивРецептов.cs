namespace Курсовая.Models;

public partial class АрхивРецептов
{
    public int PkIdАрхива { get; set; }

    public string Изображение { get; set; } = null!;

    public DateTime ДатаЗагрузки { get; set; }

    public int FkIdПользователя { get; set; }

    public virtual Пользователь FkIdПользователяNavigation { get; set; } = null!;
}
