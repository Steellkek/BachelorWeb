using BachelorWeb.Intarfaces;
using BachelorWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BachelorWeb.Repository;

public class FunctionalBlockRepository : IFunctionalBlockRepository
{
    private LayoutContext _context;

    public FunctionalBlockRepository(LayoutContext context)
    {
        _context = context;
    }
    public IEnumerable<FunctionalBlock> GetList()
    {
        return _context.FunctionalBlocks
            .Include(x => x.ComponentsPcb)
            .ThenInclude(x => x.ConnectionComponents1)
            .Include(x => x.ComponentsPcb)
            .ThenInclude(x => x.ConnectionComponents2);
    }

    public FunctionalBlock Get(long id)
    {
        throw new NotImplementedException();
    }

    public void Create(FunctionalBlock item)
    {
        _context.FunctionalBlocks.Add(item);
        _context.SaveChanges();
    }

    public void Update(FunctionalBlock item)
    {
        throw new NotImplementedException();
    }

    public void Delete(long id)
    {
        _context.FunctionalBlocks.Where(x => x.Id == id).ExecuteDelete();
        _context.SaveChanges();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public FunctionalBlock GetByProjectIdName(long projectId, string name)
    {
        return _context.FunctionalBlocks.FirstOrDefault(x => x.ProjectId == projectId && x.Name == name);
    }

    public void DeleteByProjectId(long projectId)
    {
        _context.FunctionalBlocks.Where(x => x.ProjectId == projectId).ExecuteDelete();
    }
}