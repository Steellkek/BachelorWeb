using System.Xml;
using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using BachelorWeb.Utils;
using Microsoft.AspNetCore.Mvc;

namespace BachelorWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly IRepository<Project> _projects;
    private readonly IComponentPcbRepository _componentsPcb;
    private readonly IConnectionComponentRepository _connectionComponentRepository;
    private readonly IPcbRepository _pcb;
    private readonly IHardPartPcbRepository _hardPartPcb;
    private readonly IFlexPartPcbRepository _flexPartPcb;
    private readonly IFunctionalBlockRepository _functionalBlockRepository;
    private IWebHostEnvironment _hostingEnvironment;
    
    public ProjectController(IRepository<Project>  projects, 
        IComponentPcbRepository componentsPcb, 
        IConnectionComponentRepository connectionComponentRepository,
        IPcbRepository pcbRepository,
        IHardPartPcbRepository hardPartPcbRepository,
        IFlexPartPcbRepository flexPartPcb, IWebHostEnvironment hostingEnvironment, 
        IFunctionalBlockRepository functionalBlockRepository)
    {
        _projects = projects;
        _componentsPcb = componentsPcb;
        _connectionComponentRepository = connectionComponentRepository;
        _pcb = pcbRepository;
        _hardPartPcb = hardPartPcbRepository;
        _flexPartPcb = flexPartPcb;
        _hostingEnvironment = hostingEnvironment;
        _functionalBlockRepository = functionalBlockRepository;
    }
    
    [HttpPost("CreateProject")]
    public Task<ActionResult<Project>> CreateProject(object projectName)
    {
        var project = new Project()
        {
            NameProject = projectName.ToString(),
            CreatedOn = DateTime.Now
        };
        _projects.Create(project);
        return Task.FromResult<ActionResult<Project>>(project);
    }
    
    [HttpGet("GetListProject")]
    public Task<ActionResult<List<Project>>> GetListProject()
    {
        var list = _projects.GetList().ToList();
        return Task.FromResult<ActionResult<List<Project>>>(list);
    }

    [HttpPost("UploadProject")]
    public Task<ActionResult<int>> UploadProject([FromForm]long projectId)
    {
        var file = Request.Form.Files[0];
        var x = new XmlDocument();
        x.Load(file.OpenReadStream());
        var componentsPcb = FileUtil.GetComponents(x.DocumentElement);
        _componentsPcb.DeleteByProjectId(projectId);
        foreach (var componentPcb in componentsPcb)
        {
            componentPcb.ProjectId = projectId;
            _componentsPcb.Create(componentPcb);
        }
        componentsPcb = _componentsPcb.GetList().Where(x=> x.ProjectId == projectId).ToList();

        var connectionsCompopnents = FileUtil.GetConnections(x.DocumentElement, componentsPcb);
        foreach (var connectionComponent in connectionsCompopnents)
        {
            connectionComponent.ProjectId = projectId;
            _connectionComponentRepository.Create(connectionComponent);
        }

        _pcb.DeleteByProjectId(projectId);
        var project = _projects.Get(projectId);
        var pcb = new PCB() { RateLayout = 0.65, Marking = "asd", Project = project};
        _pcb.Create(pcb);

        var hardPartPcbs = FileUtil.GetHardPart(x.DocumentElement);
        var flexPartPcbs = FileUtil.GetFlexPart(x.DocumentElement, hardPartPcbs);

        foreach (var hardPartPcb in hardPartPcbs)
        {
            hardPartPcb.Pcb = pcb;
            _hardPartPcb.Create(hardPartPcb);
        }

        foreach (var flexPartPcb in flexPartPcbs)
        {
            flexPartPcb.PcbId = pcb.Id;
            _flexPartPcb.Create(flexPartPcb);
            
        }
        
        string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Files");
        
        if (file.Length > 0)
        {
            string filePath = Path.Combine(uploads, projectId.ToString());
            if((System.IO.File.Exists(filePath))) 
            {
                System.IO.File.Delete(filePath);
            }

            using (Stream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                file.CopyTo(fileStream);
            }
        }

        _functionalBlockRepository.DeleteByProjectId(projectId);
        return Task.FromResult<ActionResult<int>>(1);
    }

}
