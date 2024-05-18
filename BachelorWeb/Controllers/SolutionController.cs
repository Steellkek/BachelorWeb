using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using BachelorWeb.Utils;
using Microsoft.AspNetCore.Mvc;

namespace BachelorWeb.Controllers;

[Route("api/[controller]")]
public class SolutionController: Controller
{
    private readonly IPcbRepository _pcbRepository;
    private readonly IHardPartPcbRepository _hardPartPcbRepository;
    private readonly IFlexPartPcbRepository _flexPartPcbRepository;
    private readonly IFunctionalBlockRepository _functionalBlockRepository;
    private readonly IEmsRepository _emsRepository;
    private readonly IConnectionComponentRepository _connectionComponentRepository;

    public SolutionController(IPcbRepository pcbRepository, 
        IHardPartPcbRepository hardPartPcbRepository, 
        IFlexPartPcbRepository flexPartPcbRepository,
        IFunctionalBlockRepository functionalBlockRepository,
        IEmsRepository emsRepository,
        IConnectionComponentRepository connectionComponentRepository)
    {
        _pcbRepository = pcbRepository;
        _hardPartPcbRepository = hardPartPcbRepository;
        _flexPartPcbRepository = flexPartPcbRepository;
        _functionalBlockRepository = functionalBlockRepository;
        _emsRepository = emsRepository;
        _connectionComponentRepository = connectionComponentRepository;
    }
    
    [HttpPost("StartAlg")]
    public Task<int> StartAlg([FromBody]Solution solution)
    {
        var pcb = _pcbRepository.GetByProjectId((long)solution.ProjectId);
        var l = _hardPartPcbRepository.GetListByPcbId(pcb.Id);
        var g = _flexPartPcbRepository.GetListByPcbId(pcb.Id);
        pcb.HardPartsPcb = l.ToList();
        pcb.FlexPartsPcb = g.ToList();
        var functionalBlocks = _functionalBlockRepository
            .GetList()
            .Where(x => x.ProjectId == solution.ProjectId)
            .ToList();
        var ems = _emsRepository.GetListByProjectId((long)solution.ProjectId).ToList();
        var connectionComponents = _connectionComponentRepository.GetListByProjectId((long)solution.ProjectId).ToList();
        var genAlg = new GenAlg(solution, pcb, functionalBlocks, ems, connectionComponents);
        genAlg.Go();
        return Task.FromResult(1);
    }
}