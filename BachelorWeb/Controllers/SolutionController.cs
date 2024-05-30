using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using BachelorWeb.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

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
    private readonly ISolutionRepository _solutionRepository;
    private IWebHostEnvironment _hostingEnvironment;

    public SolutionController(IPcbRepository pcbRepository, 
        IHardPartPcbRepository hardPartPcbRepository, 
        IFlexPartPcbRepository flexPartPcbRepository,
        IFunctionalBlockRepository functionalBlockRepository,
        IEmsRepository emsRepository,
        IConnectionComponentRepository connectionComponentRepository, 
        ISolutionRepository solutionRepository, IWebHostEnvironment hostingEnvironment)
    {
        _pcbRepository = pcbRepository;
        _hardPartPcbRepository = hardPartPcbRepository;
        _flexPartPcbRepository = flexPartPcbRepository;
        _functionalBlockRepository = functionalBlockRepository;
        _emsRepository = emsRepository;
        _connectionComponentRepository = connectionComponentRepository;
        _solutionRepository = solutionRepository;
        _hostingEnvironment = hostingEnvironment;
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
        var population = genAlg.Go();
        _hardPartPcbRepository.UpdateList(population.BestGenome.DecodedIndivid);
        solution.CriteriaEms = population.BestGenome.FitnessEms;
        solution.CriteriaIntermodule = population.BestGenome.FitnessInterModule;
        _solutionRepository.CreateOrUpdate(solution);
        var flexPartPcbs= genAlg.GetConnectionsHardPart(population.BestGenome);
        foreach (var flexPartPcb in flexPartPcbs)
        {
            _flexPartPcbRepository.Update(flexPartPcb);
        }
        string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Files");
        
        string filePath = Path.Combine(uploads, solution.ProjectId.ToString());
        FileUtil.ReWriteFile(filePath, population.BestGenome.DecodedIndivid);
        return Task.FromResult(1);
    }
    
    [HttpGet("download/{projectId}")]
    public IActionResult GetBlobDownload([FromRoute] long projectId)
    {
        string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Files");
        
        string filePath = Path.Combine(uploads, projectId.ToString());
 
        var stream = System.IO.File.OpenRead(filePath);
        return File(stream, "application/force-download", Path.GetFileName(filePath) + ".DDC");

    }

    [HttpPost("GetSolution")]
    public Task<ActionResult<Tuple<List<HardPartPcb>, List<FlexPartPcb>, List<ConnectionComponent>>>> GetSolution([FromBody] long projectId)
    {
        var solution = _solutionRepository.GetByProjectId(projectId);
        if (solution == null)
        {
            return Task.FromResult<ActionResult<Tuple<List<HardPartPcb>, List<FlexPartPcb>, List<ConnectionComponent>>>>(new Tuple<List<HardPartPcb>, List<FlexPartPcb>, List<ConnectionComponent>>(new List<HardPartPcb>(), new List<FlexPartPcb>(), new List<ConnectionComponent>()));

        }
        var pcb = _pcbRepository.GetByProjectId(projectId);
        var l = _hardPartPcbRepository.GetListByPcbId(pcb.Id).ToList();
        var g = _flexPartPcbRepository.GetListByPcbId(pcb.Id).ToList();
        var m = _connectionComponentRepository.GetListByProjectId(projectId).ToList();
        var list = new List<ConnectionComponent>();
        foreach (var hardPartPcb in l)
        {
            var b = hardPartPcb.FunctionalBlocks.SelectMany(x => x.ComponentsPcb).Select(x=> x.Id).ToList();
            foreach (var connectionComponent in m)
            {
                if (b.Contains((long)connectionComponent.ComponentPcb1Id) && b.Contains((long)connectionComponent.ComponentPcb2Id))
                {
                    list.Add(connectionComponent);
                }
            }
            
        }
        return Task.FromResult<ActionResult<Tuple<List<HardPartPcb>, List<FlexPartPcb>, List<ConnectionComponent>>>>(new Tuple<List<HardPartPcb>, List<FlexPartPcb>, List<ConnectionComponent>>(l, g, list));
    }
}