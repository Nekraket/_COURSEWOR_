namespace Курсовая.Models;

public partial class КатегорииОтслеживания
{
    public int PkIdКатегорииОтслеж { get; set; }

    public string Тип { get; set; } = null!;

    public virtual ICollection<ВектораИзображений> ВектораИзображенийs { get; set; } = new List<ВектораИзображений>();

    public virtual ICollection<ПозицияЗаписи> ПозицияЗаписиs { get; set; } = new List<ПозицияЗаписи>();
}
