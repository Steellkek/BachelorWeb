using BachelorWeb.Intarfaces;
using BachelorWeb.Models;

namespace BachelorWeb.Repository;

public class ProjectRepository : IRepository<Project>
{
    private LayoutContext _context;

    public ProjectRepository(LayoutContext context)
    {
        _context = context;
    }
        
    public IEnumerable<Project> GetList()
    {
        return _context.Projects;
    }

    public Project Get(long id)
    {
        throw new NotImplementedException();
    }

    public void Create(Project item)
    {
        _context.Projects.Add(item);
        _context.SaveChanges();
    }

    public void Update(Project item)
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
}