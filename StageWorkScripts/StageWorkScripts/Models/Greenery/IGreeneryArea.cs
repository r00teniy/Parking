namespace StageWorkScripts.Models
{
    public interface IGreeneryArea : IGreenery
    {
        public double NumberOfPlantsPerSQM { get; }
        public int NumberOfPlants { get; }
    }
}
