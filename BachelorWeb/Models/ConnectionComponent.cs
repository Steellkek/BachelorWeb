using System.ComponentModel.DataAnnotations.Schema;

namespace BachelorWeb.Models;

public class ConnectionComponent
{
    [Column("id")]
    public long? Id { get; set; }
    [Column("count_connection")]
    public long CountConnection { get; set; }
    [ForeignKey("project_id")]
    [Column("project_id")]
    public long? ProjectId { get; set; }
    
    internal Project Project { get; set; }
    
    [ForeignKey("component_pcb1_id")]
    [Column("component_pcb1_id")]
    public long? ComponentPcb1Id { get; set; }
    public ComponentPcb ComponentPcb1 { get; set; }
    
    [ForeignKey("component_pcb2_id")]
    [Column("component_pcb2_id")]
    public long? ComponentPcb2Id { get; set; }
    public ComponentPcb ComponentPcb2 { get; set; }
}