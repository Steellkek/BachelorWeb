using BachelorWeb.Models;

namespace BachelorWeb.Intarfaces;

public interface IPcbRepository : IRepository<PCB>
{
    public PCB GetByProjectId(long projectId);

    public void DeleteByProjectId(long projectId);
}