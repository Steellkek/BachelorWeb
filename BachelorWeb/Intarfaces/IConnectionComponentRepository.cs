using BachelorWeb.Models;

namespace BachelorWeb.Intarfaces;

public interface IConnectionComponentRepository : IRepository<ConnectionComponent>
{
    public IEnumerable<ConnectionComponent> GetListByProjectId(long projectId);
}