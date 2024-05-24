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
        _context.HardPartsPcb.Add(item);
        _context.SaveChanges();
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

    public void UpdateList(List<HardPartPcb> hardPartPcbs)
    {
        foreach (var hardPartPcb in hardPartPcbs)
        {
            var entity = _context.HardPartsPcb.SingleAsync(x => x.Id == hardPartPcb.Id).Result;
            var g = _context.FunctionalBlocks.Where(x => x.HardPartsPcb.Where(x => x.Id == entity.Id).Count() > 0);
            _context.Entry(entity).Collection("FunctionalBlocks").Load();

            foreach (var functionalBlock in g)
            {
                entity.FunctionalBlocks.Remove(functionalBlock);
            }
            _context.SaveChanges();
            
            foreach (var functionalBlock in hardPartPcb.FunctionalBlocks)
            {
                var entity2 = _context.FunctionalBlocks.FirstOrDefault(x => x.Id == functionalBlock.Id);
                if (entity2 != null)
                {
                    entity.FunctionalBlocks.Add(entity2);
                }
            }
            _context.SaveChanges();
        }
    }
}