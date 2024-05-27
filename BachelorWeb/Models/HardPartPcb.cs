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

    public Point P1;
    public Point P2;

    [Column("width")]
    public double Width { get; set; }
    
    [Column("height")]
    public double Height { get; set; }
    
    [Column("x")]
    public double X { get; set; }
    
    [Column("y")]
    public double Y { get; set; }
    
    internal List<FlexPartPcb> FlexPartsPcb1 { get; set; }
    
    internal List<FlexPartPcb> FlexPartsPcb2 { get; set; }
    
    public virtual List<FunctionalBlock> FunctionalBlocks { get; set; } = new List<FunctionalBlock>();
}