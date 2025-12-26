namespace Курсовая.Models;

public partial class Периодичность
{
    public int PkIdПериодичности { get; set; }

    public string Название { get; set; } = null!;

    public int Период { get; set; }

    public virtual ICollection<Измерение> Измерениеs { get; set; } = new List<Измерение>();

    public virtual ICollection<Лекарства> Лекарстваs { get; set; } = new List<Лекарства>();

    public virtual ICollection<Симптомы> Симптомыs { get; set; } = new List<Симптомы>();
}
