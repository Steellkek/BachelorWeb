using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BachelorWeb.Repository;

public class EmsRepository : IEmsRepository
{
    private LayoutContext _context;

    public EmsRepository(LayoutContext context)
    {
        _context = context;
    }

    public IEnumerable<Ems> GetList()
    {
        throw new NotImplementedException();
    }

    public Ems Get(long id)
    {
         return _context.Ems
             .Include(x => x.FunctionalBlock1)
             .Include(x => x.FunctionalBlock2)
             .FirstOrDefault(x => x.Id == id);
    }

    public void Create(Ems item)
    {
        _context.Ems.Add(item);
        _context.SaveChanges();
    }

    public void Update(Ems item)
    {
        throw new NotImplementedException();
    }

    public void Delete(long id)
    {
        _context.Ems.Where(x => x.Id == id).ExecuteDelete();
        _context.SaveChanges();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Ems> GetListByProjectId(long projectId)
    {
        return _context.Ems
            .Where(x => x.ProjectId == projectId)
            .Include(x => x.FunctionalBlock1)
            .Include(x => x.FunctionalBlock2);
    }
}