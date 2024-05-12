using System.ComponentModel.DataAnnotations.Schema;

namespace BachelorWeb.Models;

public class Project
{
    [Column("id")]
    public long? Id { get; set; }
    
    [Column("name_project")]
    public string? NameProject { get; set; }
    [Column("created_on")]
    public DateTime CreatedOn { get; set; }
    
    internal List<ComponentPcb> Components { get; set; }
    
    internal List<ConnectionComponent> ConnectionsComponent { get; set; }
    
    internal List<FunctionalBlock> FunctionalBlocks { get; set; }
    
    internal List<Ems> Ems { get; set; }
}