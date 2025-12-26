namespace Курсовая.Models;

public partial class КатегорияАрхива
{
    public int PkIdКатегорииАрхива { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<ЛичныйАрхив> ЛичныйАрхивs { get; set; } = new List<ЛичныйАрхив>();
}
