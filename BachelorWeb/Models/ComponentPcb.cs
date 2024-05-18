using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BachelorWeb.Models;

public class ComponentPcb : Footprint
{
    [Column("id")]
    public long Id;
    
    [Column("designator")]
    public string Designator { get; set; }

    [ForeignKey("project_id")]
    [Column("project_id")]
    public long? ProjectId { get; set; }

    internal Project Project { get; set; }
    
    [JsonIgnore]
    public virtual List<FunctionalBlock> FunctionalBlock { get; set; }
    
    internal  virtual List<ConnectionComponent> ConnectionComponents1 { get; set; }
    internal  virtual List<ConnectionComponent> ConnectionComponents2 { get; set; }

    public double Square()
    {
        return Width * Height;
    }
}