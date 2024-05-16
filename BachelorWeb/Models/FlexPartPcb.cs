using System.ComponentModel.DataAnnotations.Schema;

namespace BachelorWeb.Models;

public class FlexPartPcb
{
    [Column("id")]
    public long Id { get; set; }
    
    [ForeignKey("pcb_id")]
    [Column("pcb_id")]
    public long? PcbId { get; set; }

    internal PCB Pcb { get; set; }
    
    [ForeignKey("hard_part_pcb1_id")]
    [Column("hard_part_pcb1_id")]
    public long? HardPartPcb1Id { get; set; }
    public HardPartPcb HardPartPcb1 { get; set; }
    
    [ForeignKey("hard_part_pcb2_id")]
    [Column("hard_part_pcb2_id")]
    public long? HardPartPcb2Id { get; set; }
    public HardPartPcb HardPartPcb2 { get; set; }
}