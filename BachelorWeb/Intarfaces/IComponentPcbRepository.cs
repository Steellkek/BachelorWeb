using BachelorWeb.Models;

namespace BachelorWeb.Intarfaces;

public interface IComponentPcbRepository : IRepository<ComponentPcb>
{
    public void DeleteByProjectId(long projectId);
    public IEnumerable<ComponentPcb> GetListByProjectId(long projectId);
}