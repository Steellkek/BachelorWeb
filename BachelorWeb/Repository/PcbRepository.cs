using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BachelorWeb.Repository;

public class PcbRepository : IPcbRepository
{
    private LayoutContext _context;

    public PcbRepository(LayoutContext context)
    {
        _context = context;
    }
    
    public IEnumerable<PCB> GetList()
    {
        throw new NotImplementedException();
    }

    public PCB Get(long id)
    {
        throw new NotImplementedException();
    }

    public void Create(PCB item)
    {
        _context.Pcbs.Add(item);
        _context.SaveChanges();
    }

    public void Update(PCB item)
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

    public PCB GetByProjectId(long projectId)
    {
        var x = _context
            .Pcbs
            .FirstOrDefault(x => x.ProjectId == projectId);

        return x;
    }

    public void DeleteByProjectId(long projectId)
    {
        _context.Pcbs.Where(x => x.ProjectId == projectId).ExecuteDelete();
    }
}