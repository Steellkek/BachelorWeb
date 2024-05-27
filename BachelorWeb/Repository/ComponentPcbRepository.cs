using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BachelorWeb.Repository;

public class ComponentPcbRepository : IComponentPcbRepository
{
    private LayoutContext _context;

    public ComponentPcbRepository(LayoutContext context)
    {
        _context = context;
    }
        
    public IEnumerable<ComponentPcb> GetList()
    {
        return _context.Components;
    }

    public ComponentPcb Get(long id)
    {
        throw new NotImplementedException();
    }

    public void Create(ComponentPcb item)
    {
        _context.Components.Add(item);
        _context.SaveChanges();
    }

    public void Update(ComponentPcb item)
    {
        var comp = _context.Components.First(x=> x.Id == item.Id);
        comp.Height = item.Height;
        comp.Width = item.Width;
        _context.Components.Update(comp);
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
    
    public void DeleteByProjectId(long projectId)
    {
        _context.Components.Where(x => x.ProjectId == projectId).ExecuteDelete();
        _context.SaveChanges();
    }

    public IEnumerable<ComponentPcb> GetListByProjectId(long projectId)
    {
        return _context.Components.Where(x => x.ProjectId == projectId).Include(x => x.FunctionalBlock);
    }
}