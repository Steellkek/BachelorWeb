using System.ComponentModel.DataAnnotations.Schema;

namespace BachelorWeb.Models;

public class Footprint
{
    [Column("name")]
    public string? Name { get; set; }

    [Column("width")]
    public double Width { get; set; }
    [Column("height")]
    public double Height { get; set; }
}