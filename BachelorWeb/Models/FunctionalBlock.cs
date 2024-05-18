using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BachelorWeb.Models;

public class FunctionalBlock
{
    [Column("id")]
    public long Id { get; set; }
    
    [Column("name")]
    public string Name { get; set; }
    
        
    [ForeignKey("project_id")]
    [Column("project_id")]
    public long? ProjectId { get; set; }
    
    public Project Project { get; set; }

    public virtual List<ComponentPcb> ComponentsPcb { get; set; }
    
    internal  virtual List<Ems> ValueEms1 { get; set; }
    internal  virtual List<Ems> ValueEms2 { get; set; }
    
    internal  virtual List<HardPartPcb> HardPartsPcb { get; set; }

}