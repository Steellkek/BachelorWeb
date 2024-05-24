using BachelorWeb.Intarfaces;
using BachelorWeb.Models;

namespace BachelorWeb.Repository;

public class SolutionRepository : ISolutionRepository
{
    private LayoutContext _context;

    public SolutionRepository(LayoutContext context)
    {
        _context = context;
    }

    public IEnumerable<Solution> GetList()
    {
        throw new NotImplementedException();
    }

    public Solution Get(long id)
    {
        throw new NotImplementedException();
    }

    public void Create(Solution item)
    {
        throw new NotImplementedException();
    }

    public void Update(Solution item)
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

    public void CreateOrUpdate(Solution solution)
    {
        var x = _context.Solutions.FirstOrDefault(x => x.ProjectId == solution.ProjectId);
        if (x != null)
        {
            x.CountIndivid = solution.CountIndivid;
            x.CountPopulation = solution.CountPopulation;
            x.CriteriaEms = solution.CriteriaEms;
            x.CriteriaIntermodule = solution.CriteriaIntermodule;
            x.RateCrossingover = solution.RateCrossingover;
            x.RateMutation = solution.RateMutation;
            x.RateEms = solution.RateEms;
            x.RateIntermodule = solution.RateIntermodule;
        }
        else
        {
            _context.Solutions.Add(solution);
        }

        _context.SaveChanges();
    }
}