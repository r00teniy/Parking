namespace StageWorkScripts.Models;

public interface IElement
{
    public string Code { get; }
    public double Amount { get; }
    public bool IsInsidePlot { get; }
}
