using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BachelorWeb.Controllers;

[Route("api/[controller]")]
public class EmsController : Controller
{
    private readonly IEmsRepository _emsRepository;

    public EmsController(IEmsRepository emsRepository)
    {
        _emsRepository = emsRepository;
    }

    [HttpPost("CreateEms")]
    public Task<Ems> CreateEms([FromForm]long functionalBlockId1,
        [FromForm]long functionalBlockId2,
        [FromForm]long valueEms,
        [FromForm]long projectId)
    {
        var ems = new Ems()
        {
            Value = valueEms,
            ProjectId = projectId,
            FunctionalBlock1Id = functionalBlockId1,
            FunctionalBlock2Id = functionalBlockId2
        };
        _emsRepository.Create(ems);
        ems = _emsRepository.Get(ems.Id);
        return Task.FromResult(ems);
    }

    [HttpPost("GetEmsList")]
    public Task<IEnumerable<Ems>> GetEmsList([FromBody] long projectId)
    {
        return Task.FromResult<IEnumerable<Ems>>(_emsRepository.GetListByProjectId(projectId).ToList());
    }

    [HttpDelete("DeleteEmsById")]
    public Task<int> DeleteEmsById([FromBody] long emsId)
    {
        _emsRepository.Delete(emsId);
        return Task.FromResult(1);
    }
}