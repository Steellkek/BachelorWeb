using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BachelorWeb.Controllers;

[Route("api/[controller]")]
public class SchemaController : Controller
{
    private readonly IComponentPcbRepository _componentsPcb;
    private readonly IConnectionComponentRepository _connectionComponentRepository;
    private readonly IFunctionalBlockRepository _functionalBlockRepository;
    
    public SchemaController( IComponentPcbRepository componentsPcb, 
        IConnectionComponentRepository connectionComponentRepository,
        IFunctionalBlockRepository functionalBlockRepository)
    {
        _componentsPcb = componentsPcb;
        _connectionComponentRepository = connectionComponentRepository;
        _functionalBlockRepository = functionalBlockRepository;
    }


    [HttpPost("GetSchema")]
    public Task<ActionResult<Schema>> GetSchema([FromBody]long projectId)
    {
        long longProjectId = Convert.ToInt64(projectId);
        var components = _componentsPcb.GetListByProjectId(longProjectId);
        var connetions = _connectionComponentRepository.GetListByProjectId(longProjectId);
        var schema = new Schema() {ComponentsPcb = components.ToList(), ConnectionsComponent = connetions.ToList()};
        return Task.FromResult<ActionResult<Schema>>(schema);
    }

    [HttpPost("GetComponentsForFunctionalBlocks")]
    public Task<List<ComponentPcb>> GetComponentsForFunctionalBlocks([FromBody]long projectId)
    {
        var components = _componentsPcb
            .GetListByProjectId(projectId)
            .Where(x => x.FunctionalBlock.Count == 0)
            .ToList();
        return Task.FromResult(components);
    }

    [HttpPost("CreateFunctionalBlock")]
    public Task<FunctionalBlock> CreateFunctionalBlock([FromForm]List<long> componentIds,
        [FromForm]string nameFunctionalBlock,
        [FromForm]long projectId)
    {
        var functionalBlock = _functionalBlockRepository.GetByProjectIdName(projectId, nameFunctionalBlock);
        if (functionalBlock != null)
        {
            throw new Exception("Функциональный блок с таким именем уже есть!");
        }

        var componentPcbs = _componentsPcb.GetList().Where(x => componentIds.Contains(x.Id)).ToList();
        functionalBlock = new FunctionalBlock()
            {Name = nameFunctionalBlock, ProjectId = projectId, ComponentsPcb = componentPcbs};
        _functionalBlockRepository.Create(functionalBlock);
        return Task.FromResult<FunctionalBlock>(functionalBlock);
    }

    [HttpPost("GetFunctionalBlocks")]
    public Task<List<FunctionalBlock>> GetFunctionalBlocks([FromBody]long projectId)
    {
        var x = _functionalBlockRepository
            .GetList()
            .Where(x => x.ProjectId == projectId)
            .ToList();
        return Task.FromResult(x);
    }
    
    [HttpDelete("DeleteFunctionalBlock")]
    public Task<int> DeleteFunctionalBlock([FromBody]long functionalBlockId)
    {
        _functionalBlockRepository
            .Delete(functionalBlockId);
        return Task.FromResult(1);
    }
}