using BachelorWeb.Models;

namespace BachelorWeb.Intarfaces;

public interface IHardPartPcbRepository : IRepository<HardPartPcb>
{
    public IEnumerable<HardPartPcb> GetListByPcbId(long pcbId);
    public void UpdateList(List<HardPartPcb> hardPartPcbs);
}