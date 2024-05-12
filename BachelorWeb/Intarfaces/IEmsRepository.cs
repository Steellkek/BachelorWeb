using BachelorWeb.Models;

namespace BachelorWeb.Intarfaces;

public interface IEmsRepository : IRepository<Ems>
{
    public IEnumerable<Ems> GetListByProjectId(long projectId);
}