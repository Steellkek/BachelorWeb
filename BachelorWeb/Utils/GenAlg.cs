using BachelorWeb.Models;

namespace BachelorWeb.Utils;

public class GenAlg
{
    private Solution _solution;
    private PCB _pcb;
    private List<FunctionalBlock> _functionalBlocks;
    private List<Ems> _ems;
    private List<ConnectionComponent> _connectionComponents;
    private List<List<int>> _minPaths;
    private long _maxEms;
    private long _maxIntermodule;
    

    public GenAlg(Solution solution, PCB pcb, List<FunctionalBlock> functionalBlocks, List<Ems> emsList, List<ConnectionComponent> connectionComponents)
    {
        _solution = solution;
        _pcb = pcb;
        _functionalBlocks = functionalBlocks;
        _ems = emsList;
        _connectionComponents = connectionComponents;
    }

    public void Go()
    {
        if (_pcb.HardPartsPcb.Sum(x=>x.Square) * _pcb.RateLayout < _functionalBlocks.Sum(x=>x.ComponentsPcb.Sum(x=>x.Height*x.Width)))
        {
            throw new Exception("Сумарная площадь элементов больше сумарной площади");
        }
        
        _minPaths = GetMinPath();

        var population = CreateStartPopulation();
        GetNewParents(population);
        CrossingoverPopulation(population);
    }

    private void GetNewParents(Population population)
    {
        population.OldGenomes = Util.Clone(population.Genomes);
        population.OldGenomes= population.OldGenomes.OrderByDescending(x => x.FitnessEms + x.FitnessInterModule).ToList();
        Random ran = new Random();
        var chances = new List<decimal>();
        var x = population.OldGenomes.Count * (population.OldGenomes.Count + 1) / 2;
        chances.Add( (decimal)1 / x);
        for (int i = 1; i <population.OldGenomes.Count; i++)
        {
            chances.Add( chances[i-1]+ ((decimal)(i+1)/x));
        }

        var parents = new List<Genome>();
        for (int i = 0; i < population.OldGenomes.Count; i++)
        {
            var chanse = (decimal)ran.NextDouble();
            if (chanse<chances[0])
            {
                parents.Add(Util.Clone(population.OldGenomes[i]));
                continue;
            }

            for (int j = 1; j < chances.Count; j++)
            {
                if ((chanse<chances[j])&(chanse>chances[j-1]))
                {
                    parents.Add(Util.Clone(population.OldGenomes[j]));
                    break;
                }
            }
        }

        population.Genomes = parents;
    }

    private void CrossingoverPopulation(Population population)
    {
        var rand = new Random();
        for (int i = 0; i < population.Genomes.Count - 1; i = i + 2)
        {
            var newChance = rand.NextDouble();
            var point = rand.Next(1, _functionalBlocks.Count() + 1);
            if (_solution.RateCrossingover >= newChance && !CheckEquality(population.Genomes[i], population.Genomes[i+1]))
            {
                GetChild(population.Genomes[i], population.Genomes[i+1], point);
            }
        }
    }

    private void GetChild(Genome genome1, Genome genome2, int point)
    {
        var list1 = genome1.HardPartPcbs
            .Select(x => x.FunctionalBlocks)
            .SelectMany(i => i)
            .Distinct()
            .Take(point)
            .ToList();
        var list2 = genome2.HardPartPcbs
            .Select(x => x.FunctionalBlocks)
            .SelectMany(i => i)
            .Distinct()
            .ToList();
        for (int j = 0; j < list2.Count; j++)
        {
            var list3 = Util.Clone(list1);
            var genomeCopy = Util.Clone(genome1);
            for (int i = point; i < list2.Count; i++)
            {
                if (!ExistInGenome(list1, list2[i]))
                {
                    list1.Add(list2[i]);
                }
            }

            if (list1.Count != _functionalBlocks.Count)
            {
                for (int i = 0; i < list2.Count; i++)
                {
                    if (!ExistInGenome(list1, list2[i]))
                    {
                        list1.Add(list2[i]);
                    }
                }
            }
            
            foreach (var hardPartPcb in genomeCopy.HardPartPcbs)
            {
                hardPartPcb.FunctionalBlocks.Clear();
                for (int i = list3.Count-1; i >=0; i--)
                {
                    var sumSquareComponents = list3[i].ComponentsPcb.Sum(y => y.Square());
                    var sumSquareHardPart = hardPartPcb.FunctionalBlocks.Sum(x => x.ComponentsPcb.Sum(y => y.Square()));
                    //todo исправить площадь
                    if (sumSquareHardPart + sumSquareComponents  <= hardPartPcb.Square * _pcb.RateLayout)
                    {
                        hardPartPcb.FunctionalBlocks.Add(list3[i]);
                        list3.Remove(list3[i]);
                    }
                }
            }
            
            var check = list3.Count > 0;
            foreach (var hardPartPcb in genomeCopy.HardPartPcbs)
            {
                check = hardPartPcb.FunctionalBlocks.Count == 0;
            }

            if (check)
            {
                list2 = Util.LeftSHuffle(list2);
            }
            else
            {
                genome1 = genomeCopy;
                break;
            }
        }
        
    }

    public bool ExistInGenome(List<FunctionalBlock> list1, FunctionalBlock func)
    {
        foreach (var functionalBlock in list1)
        {
            if (functionalBlock.Id == func.Id)
            {
                return true;
            }
        }
        return false;
    }
    private bool CheckEquality(Genome genome1, Genome genome2)
    {
        var list1 = genome1.HardPartPcbs
            .Select(x => x.FunctionalBlocks)
            .SelectMany(i => i)
            .Distinct()
            .ToList();
        var list2 = genome2.HardPartPcbs
            .Select(x => x.FunctionalBlocks)
            .SelectMany(i => i)
            .Distinct()
            .ToList();
        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i].Id != list2[i].Id)
            {
                return false;
            }
        }

        return true;
    }
    private List<List<int>> GetMinPath()
    {
        var matrixPcb = Util.FromListToMatrix(_pcb);
        var MinPathToModule = new List<List<int>>();
        for (int i = 0; i < matrixPcb.Count; i++)
        {
            var rast = DijkstraAlgo(matrixPcb, i, matrixPcb.Count);
            for (int j = i; j < rast.Count; j++)
            {
                if (i != j)
                {
                    var MinPath = MinPath1(j, i, rast).AsEnumerable().Reverse().ToList();
                    MinPathToModule.Add(MinPath);
                }
            }
        }

        return FromIndexToId(MinPathToModule, _pcb.HardPartsPcb);
    }

    public List<List<int>>  FromIndexToId(List<List<int>> MinPathToModule, List<HardPartPcb> hardPartPcbs)
    {
        foreach (var minPathToModule in MinPathToModule)
        {
            for (int i = 0; i < minPathToModule.Count; i++)
            {
                minPathToModule[i] = (int)hardPartPcbs[minPathToModule[i]].Id;
            }
        }

        return MinPathToModule;
    }
    public Population CreateStartPopulation()
    {
        var population = new Population();
        for (int i = 0; i < _solution.CountIndivid; i++)
        {
            var genome = new Genome();
            foreach (var hardPartPcb in _pcb.HardPartsPcb)
            {
                genome.HardPartPcbs.Add(Util.Clone(hardPartPcb));
            }
            genome = Shuffle(genome);

            _maxEms = _ems.Sum(x => x.Value);
            _maxIntermodule = _connectionComponents.Sum(x => x.CountConnection) * _minPaths.Max(x => x.Count);

            genome.FitnessEms = GetFitnessEms(genome);
            genome.FitnessInterModule = GetFitness(genome);

            population.Genomes.Add(Util.Clone(genome));
        }

        population.BestGenome = population.Genomes.OrderBy(i => i.FitnessEms + i.FitnessInterModule).First();
        return population;
    }

    private double GetFitnessEms(Genome genome)
    {
        long ems = 0;
        foreach (var hardPartPcb in genome.HardPartPcbs)
        {
            var idFunctional = hardPartPcb.FunctionalBlocks.Select(x => x.Id).ToList();
            ems += _ems.Where(x =>
                    idFunctional.Contains((long)x.FunctionalBlock1Id) &&
                    idFunctional.Contains((long)x.FunctionalBlock2Id))
                .Sum(x => x.Value);
        }

        var fitnessEms = (double)ems / (double)_maxEms * (double)100 * _solution.RateEms;
        return fitnessEms;
    }

    private double GetFitness(Genome genome)
    {
        long fitness = 0;
        for (int i = 0; i < genome.HardPartPcbs.Count; i++)
        {
            var idComponents1 = genome.HardPartPcbs[i].FunctionalBlocks
                .Select(x => x.ComponentsPcb)
                .SelectMany(i => i)
                .Distinct()
                .Select(x => x.Id)
                .ToList();
            for (int j = i + 1; j < genome.HardPartPcbs.Count; j++)
            {
                var idComponents2 = genome.HardPartPcbs[j].FunctionalBlocks
                    .Select(x => x.ComponentsPcb)
                    .SelectMany(i => i)
                    .Distinct()
                    .Select(x => x.Id)
                    .ToList();
                var path = _minPaths
                    .FirstOrDefault(x =>
                        (x[0] == genome.HardPartPcbs[i].Id &&
                         x[x.Count - 1] == genome.HardPartPcbs[j].Id) ||
                        (x[0] == genome.HardPartPcbs[j].Id &&
                         x[x.Count - 1] == genome.HardPartPcbs[i].Id));
                fitness += _connectionComponents.Where(x =>
                        (idComponents1.Contains((long)x.ComponentPcb1Id) &&
                         idComponents2.Contains((long)x.ComponentPcb2Id)) ||
                        (idComponents1.Contains((long)x.ComponentPcb2Id) &&
                         idComponents2.Contains((long)x.ComponentPcb1Id)))
                    .Sum(x => x.CountConnection) * (path.Count - 1);
            }
        }

        var fitnessIntermodule = (double)fitness / (double)_maxIntermodule * (double)100 * _solution.RateIntermodule;
        return fitnessIntermodule;
    }
    
    public Genome Shuffle(Genome genome)
    {
        Random rand = new Random();
        var check = true;
        var cnt = 0;
        var functionalBlocksCopy = new List<FunctionalBlock>();
        while (check && cnt<=2000)
        {
            functionalBlocksCopy = Util.Clone(_functionalBlocks);
            cnt++;
            Util.Shuffle(functionalBlocksCopy);
            
            foreach (var hardPartPcb in genome.HardPartPcbs)
            {
                hardPartPcb.FunctionalBlocks.Clear();
                for (int i = functionalBlocksCopy.Count-1; i >=0; i--)
                {
                    var sumSquareComponents = functionalBlocksCopy[i].ComponentsPcb.Sum(y => y.Square());
                    var sumSquareHardPart = hardPartPcb.FunctionalBlocks.Sum(x => x.ComponentsPcb.Sum(y => y.Square()));
                    if (sumSquareHardPart + sumSquareComponents  <= hardPartPcb.Square * _pcb.RateLayout)
                    {
                        hardPartPcb.FunctionalBlocks.Add(functionalBlocksCopy[i]);
                        functionalBlocksCopy.Remove(functionalBlocksCopy[i]);
                    }
                }
            }
            
            check = functionalBlocksCopy.Count > 0;
            foreach (var hardPartPcb in genome.HardPartPcbs)
            {
                check = hardPartPcb.FunctionalBlocks.Count == 0;
            }
        }
        
        if (cnt>2000)
        {
            throw new Exception("Данная компоновка невозможна, попробуйте изменить размеры модулей или элементов.");
        }

        return genome;
    }
    
    private List<int> MinPath1(int end, int start, List<int> paths)
    {
        List<int> listOfpoints = new List<int>();
        var tempp = end;
        while (tempp != start)
        {
            listOfpoints.Add(tempp);
            tempp = paths[tempp];
        }
        listOfpoints.Add(tempp);
        return listOfpoints;
    }

    
    private static int MinimumDistance(int[] distance, bool[] shortestPathTreeSet, int verticesCount)
    {
        int min = int.MaxValue;
        int minIndex = 0;
 
        for (int v = 0; v < verticesCount; ++v)
        {
            if (shortestPathTreeSet[v] == false && distance[v] <= min)
            {
                min = distance[v];
                minIndex = v;
            }
        }
 
        return minIndex;
    }
    
 
    public static List<int> DijkstraAlgo(List<List<int>> graph, int source, int verticesCount)
    {
        int[] distance = new int[verticesCount];
        bool[] shortestPathTreeSet = new bool[verticesCount];
        List<int> vert = new();
        for (int i = 0; i < verticesCount; i++)
        {
            vert.Add(source);
        }

        for (int i = 0; i < verticesCount; ++i)
        {
            distance[i] = int.MaxValue;
            shortestPathTreeSet[i] = false;
        }
 
        distance[source] = 0;
 
        for (int count = 0; count < verticesCount - 1; ++count)
        {
            int u = MinimumDistance(distance, shortestPathTreeSet, verticesCount);
            shortestPathTreeSet[u] = true;
            
            for (int v = 0; v < verticesCount; ++v)
            {
                if (!shortestPathTreeSet[v] && Convert.ToBoolean(graph[u][v]) && distance[u] != int.MaxValue &&
                    distance[u] + graph[u][v] < distance[v])
                {
                    distance[v] = distance[u] + graph[u][v];
                    vert[v] = u;
                }
            }
        }

        return vert;
    }
}