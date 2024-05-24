using BachelorWeb.Models;

namespace BachelorWeb.Intarfaces;

public interface ISolutionRepository : IRepository<Solution>
{
    public void CreateOrUpdate(Solution solution);
}