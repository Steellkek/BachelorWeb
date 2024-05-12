using BachelorWeb.Intarfaces;
using BachelorWeb.Models;

namespace BachelorWeb.Repository;

public class ConnectionComponentRepository : IConnectionComponentRepository
{
    private LayoutContext _context;

    public ConnectionComponentRepository(LayoutContext context)
    {
        _context = context;
    }
    public IEnumerable<ConnectionComponent> GetList()
    {
        return _context.ConnectionsComponent;
    }

    public ConnectionComponent Get(long id)
    {
        throw new NotImplementedException();
    }

    public void Create(ConnectionComponent item)
    {
        _context.ConnectionsComponent.Add(item);
        _context.SaveChanges();
    }

    public void Update(ConnectionComponent item)
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

    public IEnumerable<ConnectionComponent> GetListByProjectId(long projectId)
    {
        return _context.ConnectionsComponent.Where(x => x.ProjectId == projectId);
    }
}