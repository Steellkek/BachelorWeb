using BachelorWeb.Models;

namespace BachelorWeb.Intarfaces;

public interface IFlexPartPcbRepository : IRepository<FlexPartPcb>
{
    public IEnumerable<FlexPartPcb> GetListByPcbId(long pcbId);
}