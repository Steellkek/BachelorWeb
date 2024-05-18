namespace BachelorWeb.Models;

public class Genome
{
    public List<HardPartPcb> HardPartPcbs { get; set; } = new List<HardPartPcb>();
    public double FitnessEms { get; set; }
    public double FitnessInterModule { get; set; }
}