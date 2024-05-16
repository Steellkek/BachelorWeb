using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BachelorWeb.Controllers;

[Route("api/[controller]")]
public class SolutionController: Controller
{
    private readonly IPcbRepository _pcbRepository;

    public SolutionController(IPcbRepository pcbRepository)
    {
        _pcbRepository = pcbRepository;
    }
    
    [HttpPost("StartAlg")]
    public Task<int> StartAlg([FromBody]Solution solution)
    {
        var x = _pcbRepository.GetByProjectId((long)solution.ProjectId);
        return Task.FromResult(1);
    }
}