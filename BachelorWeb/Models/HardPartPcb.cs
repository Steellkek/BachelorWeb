using System.ComponentModel.DataAnnotations.Schema;

namespace BachelorWeb.Models;

public class HardPartPcb
{
    [Column("id")]
    public long Id { get; set; }
    [Column("square")]
    public double Square { get; set; }
    [ForeignKey("pcb_id")]
    [Column("pcb_id")]
    public long? PcbId { get; set; }

    internal PCB Pcb { get; set; }
    
    internal List<FlexPartPcb> FlexPartsPcb1 { get; set; }
    
    internal List<FlexPartPcb> FlexPartsPcb2 { get; set; }
    
    public List<FunctionalBlock> FunctionalBlocks { get; set; } = new List<FunctionalBlock>();
}