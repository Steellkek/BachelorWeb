using System.Xml;
using System.Xml.Serialization;
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
    
    public ProjectController(IRepository<Project>  projects, IComponentPcbRepository componentsPcb, IConnectionComponentRepository connectionComponentRepository)
    {
        _projects = projects;
        _componentsPcb = componentsPcb;
        _connectionComponentRepository = connectionComponentRepository;
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
        return Task.FromResult<ActionResult<int>>(1);
    }

}
