using BachelorWeb.Models;

namespace BachelorWeb.Intarfaces;

public interface ISolutionRepository : IRepository<Solution>
{
    public void CreateOrUpdate(Solution solution);

    public Solution GetByProjectId(long projectId);

    public void DeleteByProjectId(long projectId);
}