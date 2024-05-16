using BachelorWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BachelorWeb;

public class LayoutContext: DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<ComponentPcb> Components { get; set; }
    public DbSet<ConnectionComponent> ConnectionsComponent { get; set; }
    public DbSet<FunctionalBlock> FunctionalBlocks { get; set; }
    public DbSet<Ems> Ems { get; set; }
    
    public DbSet<PCB> Pcbs { get; set; }
    
    public DbSet<HardPartPcb> HardPartsPcb { get; set; }
    
    public DbSet<FlexPartPcb> FlexPartsPcb { get; set; }
    
    public DbSet<Solution> Solutions { get; set; }
    
    public LayoutContext(DbContextOptions<LayoutContext> options)
        : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>().HasKey(x => x.Id);
        modelBuilder.Entity<ComponentPcb>().HasKey(x => x.Id);
        modelBuilder.Entity<ConnectionComponent>().HasKey(x => x.Id);
        modelBuilder.Entity<FunctionalBlock>().HasKey(x => x.Id);
        modelBuilder.Entity<Ems>().HasKey(x => x.Id);
        modelBuilder.Entity<PCB>().HasKey(x => x.Id);
        modelBuilder.Entity<HardPartPcb>().HasKey(x => x.Id);
        modelBuilder.Entity<FlexPartPcb>().HasKey(x => x.Id);
        modelBuilder.Entity<Solution>().HasKey(x => x.Id);
        
        modelBuilder.Entity<Project>()
            .HasMany(x => x.Components)
            .WithOne(x => x.Project);

        modelBuilder.Entity<Project>()
            .HasMany(x => x.ConnectionsComponent)
            .WithOne(x => x.Project);

        modelBuilder.Entity<Project>()
            .HasMany(x => x.FunctionalBlocks)
            .WithOne(x => x.Project);
        
        modelBuilder.Entity<Project>()
            .HasMany(x => x.Ems)
            .WithOne(x => x.Project);
                
        modelBuilder.Entity<Project>()
            .HasMany(x => x.Solutions)
            .WithOne(x => x.Project);
        
        modelBuilder.Entity<ConnectionComponent>()
            .HasOne(x => x.ComponentPcb1)
            .WithMany(x => x.ConnectionComponents1)
            .HasForeignKey(x => x.ComponentPcb1Id);
        
        modelBuilder.Entity<ConnectionComponent>()
            .HasOne(x => x.ComponentPcb2)
            .WithMany(x => x.ConnectionComponents2)
            .HasForeignKey(x => x.ComponentPcb2Id);
        
        modelBuilder.Entity<Ems>()
            .HasOne(x => x.FunctionalBlock1)
            .WithMany(x => x.ValueEms1)
            .HasForeignKey(x => x.FunctionalBlock1Id);
        
        modelBuilder.Entity<Ems>()
            .HasOne(x => x.FunctionalBlock2)
            .WithMany(x => x.ValueEms2)
            .HasForeignKey(x => x.FunctionalBlock2Id);

        modelBuilder.Entity<PCB>()
            .HasOne(x => x.Project)
            .WithMany(x=>x.Pcb)
            .HasForeignKey(x=>x.ProjectId);
        
        modelBuilder.Entity<PCB>()
            .HasMany(x => x.FlexPartsPcb)
            .WithOne(x => x.Pcb);

        modelBuilder.Entity<PCB>()
            .HasMany(x => x.HardPartsPcb)
            .WithOne(x => x.Pcb);

        modelBuilder.Entity<HardPartPcb>()
            .HasMany(x => x.FunctionalBlocks)
            .WithMany(x => x.HardPartsPcb);
    }
}