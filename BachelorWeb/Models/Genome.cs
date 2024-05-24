namespace BachelorWeb.Models;

public class Genome
{
    public List<HardPartPcb> DecodedIndivid { get; set; } = new List<HardPartPcb>();
    public List<FunctionalBlock> EncodedIndivid { get; set; } = new List<FunctionalBlock>();
    public double FitnessEms { get; set; }
    public double FitnessInterModule { get; set; }
}