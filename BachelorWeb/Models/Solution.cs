using System.ComponentModel.DataAnnotations.Schema;

namespace BachelorWeb.Models;

public class Solution
{
    [Column("id")]
    public long Id { get; set; }
    
    [Column("count_population")]
    public int CountPopulation { get; set; }
    
    [Column("count_individ")]
    public int CountIndivid { get; set; }
    
    [Column("rate_mutation")]
    public double RateMutation { get; set; }
    
    [Column("rate_crossingover")]
    public double RateCrossingover { get; set; }
    
    [Column("rate_intermodule")]
    public double RateIntermodule { get; set; }
    
    [Column("rate_ems")]
    public double RateEms { get; set; }
    
    [Column("criteria_ems")]
    public double CriteriaEms { get; set; }
    
    [Column("criteria_intermodule")]
    public double CriteriaIntermodule { get; set; }
    
    [ForeignKey("project_id")]
    [Column("project_id")]
    public long? ProjectId { get; set; }

    internal Project Project { get; set; }
}