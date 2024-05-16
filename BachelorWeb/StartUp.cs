using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using BachelorWeb.Repository;
using Microsoft.EntityFrameworkCore;

namespace BachelorWeb;

public class StartUp
{
    public StartUp(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var connection = Configuration.GetConnectionString("DefaultConnection");
        
        
        services.AddTransient<IRepository<Project>, ProjectRepository>();
        services.AddTransient<IComponentPcbRepository, ComponentPcbRepository>();
        services.AddTransient<IConnectionComponentRepository, ConnectionComponentRepository>();
        services.AddTransient<IFunctionalBlockRepository, FunctionalBlockRepository>();
        services.AddTransient<IEmsRepository, EmsRepository>();
        services.AddTransient<IPcbRepository, PcbRepository>();
        services.AddDbContext<LayoutContext>(options => options.UseNpgsql(connection));
        
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        
        services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddMvcCore();
        services.AddControllersWithViews()
            .AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseCors(x => x
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
        

        app.UseHttpsRedirection();

        app.UseRouting();
        
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
            endpoints.MapControllers();
        });
    }
}
