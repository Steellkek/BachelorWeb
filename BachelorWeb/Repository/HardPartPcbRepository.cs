using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BachelorWeb.Repository;

public class HardPartPcbRepository : IHardPartPcbRepository
{
    private LayoutContext _context;

    public HardPartPcbRepository(LayoutContext context)
    {
        _context = context;
    }
    public IEnumerable<HardPartPcb> GetList()
    {
        throw new NotImplementedException();
    }

    public HardPartPcb Get(long id)
    {
        throw new NotImplementedException();
    }

    public void Create(HardPartPcb item)
    {
        throw new NotImplementedException();
    }

    public void Update(HardPartPcb item)
    {
        throw new NotImplementedException();
    }

    public void Delete(long id)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<HardPartPcb> GetListByPcbId(long pcbId)
    {
        return _context.HardPartsPcb
            .Include(x=> x.FlexPartsPcb1)
            .Include(x=> x.FlexPartsPcb1)
            .Where(x => x.PcbId == pcbId);
    }
}