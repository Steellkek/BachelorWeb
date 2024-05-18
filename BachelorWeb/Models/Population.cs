namespace BachelorWeb.Models;

public class Population
{
    public List<Genome> Genomes { get; set; } = new();
    public List<Genome> OldGenomes { get; set; } = new();
    public Genome BestGenome = new();
}