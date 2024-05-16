using System.ComponentModel.DataAnnotations.Schema;

namespace BachelorWeb.Models;

public class PCB
{
    [Column("id")]
    public long Id { get; set; }

    [Column("marking")]
    public string Marking{ get; set; }
    
    [Column("rate_layout")]
    public double RateLayout { get; set; }
    
    [ForeignKey("project_id")]
    [Column("project_id")]
    public long? ProjectId { get; set; }

    internal Project Project { get; set; }
    
    internal virtual  List<HardPartPcb> HardPartsPcb { get; set; }
    
    internal virtual  List<FlexPartPcb> FlexPartsPcb { get; set; }
}