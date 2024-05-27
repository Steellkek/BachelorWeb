using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BachelorWeb.Repository;

public class FlexPartPcbRepository : IFlexPartPcbRepository
{
    private LayoutContext _context;

    public FlexPartPcbRepository(LayoutContext context)
    {
        _context = context;
    }
    public IEnumerable<FlexPartPcb> GetList()
    {
        throw new NotImplementedException();
    }

    public FlexPartPcb Get(long id)
    {
        throw new NotImplementedException();
    }

    public void Create(FlexPartPcb item)
    {
        _context.FlexPartsPcb.Add(item);
        _context.SaveChanges();
    }

    public void Update(FlexPartPcb item)
    {
        _context.FlexPartsPcb.Update(item);
        _context.SaveChanges();
    }

    public void Delete(long id)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<FlexPartPcb> GetListByPcbId(long pcbId)
    {
        return _context.FlexPartsPcb
            .Include(x=> x.HardPartPcb1)
            .Include(x=>x.HardPartPcb2)
            .Where(x => x.PcbId == pcbId);
    }
}