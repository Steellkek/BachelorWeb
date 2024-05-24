using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BachelorWeb.Controllers;

[Route("api/[controller]")]
public class PcbController : ControllerBase
{
    private readonly IPcbRepository _pcbRepository;
    private readonly IHardPartPcbRepository _hardPartPcbRepository;
    private readonly IFlexPartPcbRepository _flexPartPcbRepository;

    public PcbController(IPcbRepository pcbRepository, IHardPartPcbRepository hardPartPcbRepository, IFlexPartPcbRepository flexPartPcbRepository)
    {
        _pcbRepository = pcbRepository;
        _hardPartPcbRepository = hardPartPcbRepository;
        _flexPartPcbRepository = flexPartPcbRepository;
    }
    
    [HttpPost("GetPcb")]
    public Task<ActionResult<Tuple<PCB,List<HardPartPcb>, List<FlexPartPcb>>>> GetPcb([FromBody]long projectId)
    {
        var pcb = _pcbRepository.GetByProjectId(projectId);
        var l = _hardPartPcbRepository.GetListByPcbId(pcb.Id).ToList();
        var g = _flexPartPcbRepository.GetListByPcbId(pcb.Id).ToList();
        pcb.HardPartsPcb = l.ToList();
        pcb.FlexPartsPcb = g.ToList();
        return Task.FromResult<ActionResult<Tuple<PCB,List<HardPartPcb>, List<FlexPartPcb>>>>(new Tuple<PCB, List<HardPartPcb>, List<FlexPartPcb>>(pcb, l, g));
    }

}