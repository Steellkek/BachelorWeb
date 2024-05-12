using System.ComponentModel.DataAnnotations.Schema;

namespace BachelorWeb.Models;

public class Ems
{
    [Column("id")]
    public long Id { get; set; }
    
    [Column("value")]
    public long Value { get; set; }
    [ForeignKey("project_id")]
    [Column("project_id")]
    public long? ProjectId { get; set; }

    internal Project Project { get; set; }
    
    [ForeignKey("component_pcb1_id")]
    [Column("component_pcb1_id")]
    public long? FunctionalBlock1Id { get; set; }
    public FunctionalBlock FunctionalBlock1 { get; set; }
    
    [ForeignKey("component_pcb2_id")]
    [Column("component_pcb2_id")]
    public long? FunctionalBlock2Id { get; set; }
    public FunctionalBlock FunctionalBlock2 { get; set; }
}