using BachelorWeb.Models;

namespace BachelorWeb.Intarfaces;

public interface IFunctionalBlockRepository : IRepository<FunctionalBlock>
{
    public FunctionalBlock GetByProjectIdName(long projectId, string name);
}