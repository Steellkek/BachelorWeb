using BachelorWeb.Models;
using Microsoft.EntityFrameworkCore;

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
    private List<double> _listFitness = new();
    private List<HardPartPcb> _path = new();

    public GenAlg(Solution solution, PCB pcb, List<FunctionalBlock> functionalBlocks, List<Ems> emsList, List<ConnectionComponent> connectionComponents)
    {
        _solution = solution;
        _pcb = pcb;
        _functionalBlocks = functionalBlocks;
        _ems = emsList;
        _connectionComponents = connectionComponents;
        _path = Util.Clone(_pcb.HardPartsPcb);
        foreach (var hardPartPcb in _path)
        {
            hardPartPcb.FunctionalBlocks.Clear();
        }
    }

    public Population Go()
    {
        if (_pcb.HardPartsPcb.Sum(x=>x.Square) * _pcb.RateLayout < _functionalBlocks.Sum(x=>x.ComponentsPcb.Sum(x=>x.Height*x.Width)))
        {
            throw new Exception("Сумарная площадь элементов больше сумарной площади");
        }
        
        _minPaths = GetMinPath();
        var rand = new Random();

        var population = CreateStartPopulation();

        for (int i = 0; i < _solution.CountPopulation - 1; i++)
        {
            population = GetNewParents(population);
            if (_listFitness.Count > 1 &&
                _listFitness[_listFitness.Count - 1] == _listFitness[_listFitness.Count - 2] &&
                rand.NextDouble() > 0.5)
            {
                _path = Util.Shuffle(_path);
                foreach (var genome in population.Genomes)
                {
                    genome.DecodedIndivid = Util.Clone(_path);
                }
            }

            population = CrossingoverPopulation(population);
            population = MutationPopulation(population);
            population = GetFitnessPopulation(population);
            population = Elitism(population);
        }

        return population;
    }

    private Population Elitism(Population population)
    {
        var list = Util.Clone(population.Genomes);
        foreach (var genome in population.OldGenomes)
        {
            list.Add(Util.Clone(genome));
        }

        list = list.OrderBy(x => x.FitnessEms + x.FitnessInterModule).ToList();
        population.Genomes = list.Take(_solution.CountIndivid).ToList();
        population.BestGenome = list.First();
        _listFitness.Add(population.BestGenome.FitnessEms + population.BestGenome.FitnessInterModule);
        foreach (var genome in list)
        {
            if (genome.DecodedIndivid.Select(x=>x.FunctionalBlocks).SelectMany(x=>x).Count( )== 0)
            {
                throw new Exception("Оптимальную компновку невозможно найти, увеличте площадь!");
            }
        }
        return population;
    }
    private Population GetFitnessPopulation(Population population)
    {
        for (int i = 0; i < population.Genomes.Count; i++)
        {
            population.Genomes[i].FitnessInterModule = GetFitness(population.Genomes[i]);
            population.Genomes[i].FitnessEms = GetFitnessEms(population.Genomes[i]);
        }

        return population;
    }
    private Population GetNewParents(Population population)
    {
        population.OldGenomes = Util.Clone(population.Genomes).OrderByDescending(x => x.FitnessEms + x.FitnessInterModule).ToList();
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
        return population;
    }

    private Population CrossingoverPopulation(Population population)
    {
        var rand = new Random();
        for (int i = 0; i < population.Genomes.Count - 1; i = i + 2)
        {
            var newChance = rand.NextDouble();
            var point = rand.Next(1, (_functionalBlocks.Count() + 1) / 2);
            if (_solution.RateCrossingover >= newChance)
            {
                var child1 = GetChild(population.Genomes[i], population.Genomes[i+1], point);
                var child2 = GetChild(population.Genomes[i+1], population.Genomes[i], point);
                var x = child1.DecodedIndivid.Select(x => x.FunctionalBlocks).SelectMany(x=>x).Count();
                if (x!= _functionalBlocks.Count())
                {
                    Console.WriteLine(5);
                }
                x = child2.DecodedIndivid.Select(x => x.FunctionalBlocks).SelectMany(x=>x).Count();
                if (x!= _functionalBlocks.Count())
                {
                    Console.WriteLine(5);
                }
                population.Genomes[i] = child1;
                population.Genomes[i + 1] = child2;
                

            }
            else
            {
                population.Genomes[i] = FeelHardPart(population.Genomes[i]);
                population.Genomes[i+1] = FeelHardPart(population.Genomes[i+1]);
            }
        }
        return population;
    }

    private Genome FeelHardPart(Genome genome)
    {
        var genomeCopy = Util.Clone(genome);
        var list3 = Util.Clone(genome.EncodedIndivid);
        list3.Reverse();
        
        foreach (var hardPartPcb in genomeCopy.DecodedIndivid)
        {
            hardPartPcb.FunctionalBlocks.Clear();
        }

        var h = 0;
        for (int i = list3.Count-1; i >=0; i--)
        {
            var isEnd1 = true;
            for(int k = 0; k < genomeCopy.DecodedIndivid.Count; k++)
            {
                var sumSquareComponents = list3[i].ComponentsPcb.Sum(y => y.Square());
                var sumSquareHardPart = genomeCopy.DecodedIndivid[k].FunctionalBlocks.Sum(x => x.ComponentsPcb.Sum(y => y.Square()));
                if (sumSquareHardPart + sumSquareComponents  <= genomeCopy.DecodedIndivid[k].Square * _pcb.RateLayout && CheckEms(genomeCopy.DecodedIndivid[k].FunctionalBlocks, list3[i]))
                {
                    genomeCopy.DecodedIndivid[k].FunctionalBlocks.Add(list3[i]);
                    list3.Remove(list3[i]);
                    h = k + 1;
                    isEnd1 = k != genomeCopy.DecodedIndivid.Count - 1;
                    break;
                }
            }

            if (!isEnd1)
            {
                for (int l = 0; l < h; l++)
                {
                    genomeCopy.DecodedIndivid = Util.LeftSHuffle(genomeCopy.DecodedIndivid);
                }
            }
        }
        
        var check = list3.Count > 0;
        if (!check)
        {
            foreach (var hardPartPcb in genomeCopy.DecodedIndivid)
            {
                check = hardPartPcb.FunctionalBlocks.Count == 0 || check;
            }
        }
        
        if (check)
        {
            list3 = Util.Clone(genomeCopy.EncodedIndivid);
            list3.Reverse();
            
            foreach (var hardPartPcb in genomeCopy.DecodedIndivid)
            {
                hardPartPcb.FunctionalBlocks.Clear();
            }
            
            var g = genomeCopy.DecodedIndivid.Count - 1;
            for (int i = list3.Count-1; i >=0; i--)
            {
                var isEnd2 = true;
                for(int k = genomeCopy.DecodedIndivid.Count - 1; k >=0; k--)
                {
                    var sumSquareComponents = list3[i].ComponentsPcb.Sum(y => y.Square());
                    var sumSquareHardPart = genomeCopy.DecodedIndivid[k].FunctionalBlocks.Sum(x => x.ComponentsPcb.Sum(y => y.Square()));
                    if (sumSquareHardPart + sumSquareComponents  <= genomeCopy.DecodedIndivid[k].Square * _pcb.RateLayout && CheckEms(genomeCopy.DecodedIndivid[k].FunctionalBlocks, list3[i]))
                    {
                        genomeCopy.DecodedIndivid[k].FunctionalBlocks.Add(list3[i]);
                        list3.Remove(list3[i]);
                        g = k - 1;
                        isEnd2 = k == 0;
                        break;
                    }
                }

                if (!isEnd2)
                {
                    for (int l = 0; l < genomeCopy.DecodedIndivid.Count - 1 - g; l++)
                    {
                        genomeCopy.DecodedIndivid = Util.LeftSHuffle(genomeCopy.DecodedIndivid);
                    }
                }
            }
        }
        
        check = list3.Count > 0;
        if (!check)
        {
            foreach (var hardPartPcb in genomeCopy.DecodedIndivid)
            {
                check = hardPartPcb.FunctionalBlocks.Count == 0 || check;
            }
        }

        return genomeCopy;
    }

    private Genome GetChild(Genome genome1, Genome genome2, int point)
    {
        var list1 = genome1.EncodedIndivid.Take(point).ToList();
        var list2 = genome2.EncodedIndivid.ToList();
        for (int j = 0; j < list2.Count; j++)
        {
            var list3 = Util.Clone(list1);
            var genomeCopy = Util.Clone(genome1);
            for (int i = point; i < list2.Count; i++)
            {
                if (!ExistInGenome(list1, list2[i]))
                {
                    list3.Add(list2[i]);
                }
            }

            if (list3.Count != _functionalBlocks.Count)
            {
                for (int i = 0; i < list2.Count; i++)
                {
                    if (!ExistInGenome(list3, list2[i]))
                    {
                        list3.Add(list2[i]);
                    }
                }
            }

            genomeCopy.EncodedIndivid = Util.Clone(list3);
            list3.Reverse();
            
            foreach (var hardPartPcb in genomeCopy.DecodedIndivid)
            {
                hardPartPcb.FunctionalBlocks.Clear();
            }

            var h = 0;
            for (int i = list3.Count-1; i >=0; i--)
            {
                var isEnd1 = true;
                for(int k = 0; k < genomeCopy.DecodedIndivid.Count; k++)
                {
                    var sumSquareComponents = list3[i].ComponentsPcb.Sum(y => y.Square());
                    var sumSquareHardPart = genomeCopy.DecodedIndivid[k].FunctionalBlocks.Sum(x => x.ComponentsPcb.Sum(y => y.Square()));
                    if (sumSquareHardPart + sumSquareComponents  <= genomeCopy.DecodedIndivid[k].Square * _pcb.RateLayout && CheckEms(genomeCopy.DecodedIndivid[k].FunctionalBlocks, list3[i]))
                    {
                        genomeCopy.DecodedIndivid[k].FunctionalBlocks.Add(list3[i]);
                        list3.Remove(list3[i]);
                        h = k + 1;
                        isEnd1 = k != genomeCopy.DecodedIndivid.Count - 1;
                        break;
                    }
                }

                if (!isEnd1)
                {
                    for (int l = 0; l < h; l++)
                    {
                        genomeCopy.DecodedIndivid = Util.LeftSHuffle(genomeCopy.DecodedIndivid);
                    }
                }
            }
            
            var check = list3.Count > 0;
            if (!check)
            {
                foreach (var hardPartPcb in genomeCopy.DecodedIndivid)
                {
                    check = hardPartPcb.FunctionalBlocks.Count == 0 || check;
                }
            }
            
            if (check)
            {
                list3 = Util.Clone(genomeCopy.EncodedIndivid);
                list3.Reverse();
                
                foreach (var hardPartPcb in genomeCopy.DecodedIndivid)
                {
                    hardPartPcb.FunctionalBlocks.Clear();
                }
                
                var g = genomeCopy.DecodedIndivid.Count - 1;
                for (int i = list3.Count-1; i >=0; i--)
                {
                    var isEnd2 = true;
                    for(int k = genomeCopy.DecodedIndivid.Count - 1; k >=0; k--)
                    {
                        var sumSquareComponents = list3[i].ComponentsPcb.Sum(y => y.Square());
                        var sumSquareHardPart = genomeCopy.DecodedIndivid[k].FunctionalBlocks.Sum(x => x.ComponentsPcb.Sum(y => y.Square()));
                        if (sumSquareHardPart + sumSquareComponents  <= genomeCopy.DecodedIndivid[k].Square * _pcb.RateLayout && CheckEms(genomeCopy.DecodedIndivid[k].FunctionalBlocks, list3[i]))
                        {
                            genomeCopy.DecodedIndivid[k].FunctionalBlocks.Add(list3[i]);
                            list3.Remove(list3[i]);
                            g = k - 1;
                            isEnd2 = k == 0;
                            break;
                        }
                    }

                    if (!isEnd2)
                    {
                        for (int l = 0; l < genomeCopy.DecodedIndivid.Count - 1 - g; l++)
                        {
                            genomeCopy.DecodedIndivid = Util.LeftSHuffle(genomeCopy.DecodedIndivid);
                        }
                    }
                }
            }
            
            check = list3.Count > 0;
            if (!check)
            {
                foreach (var hardPartPcb in genomeCopy.DecodedIndivid)
                {
                    check = hardPartPcb.FunctionalBlocks.Count == 0 || check;
                }
            }

            if (j == list2.Count - 2)
            {
                
            }
            
            if (check)
            {
                list2 = Util.LeftSHuffle(list2);
            }
            else
            {
                return genomeCopy;
            }
        }
        
        if (genome1.DecodedIndivid.Select(x=>x.FunctionalBlocks).SelectMany(x=>x).Count( )== 0)
        {
            throw new Exception("Оптимальную компновку невозможно найти, увеличте площадь!");
        }
        return genome1;
    }

    private bool CheckEms(List<FunctionalBlock> functionalBlocks, FunctionalBlock functionalBlock)
    {
        var listIds = functionalBlocks.Select(x => x.Id).ToList();
        var x = _ems.Where(x => ((x.FunctionalBlock1Id == functionalBlock.Id
                          && listIds.Contains((long)x.FunctionalBlock2Id)) ||
                         (x.FunctionalBlock2Id == functionalBlock.Id
                          && listIds.Contains((long)x.FunctionalBlock1Id))) && x.Value == 5);
        if (x.Count() > 0)
        {
            return false;
        }
        return true;
    }
    private Population MutationPopulation(Population population)
    {
        var rand = new Random();
        for (int i = 0; i < population.Genomes.Count; i += 1)
        {
            var newChance = rand.NextDouble();
            if (_solution.RateMutation <= newChance)
            {
                population.Genomes[i] = MutationGenome(population.Genomes[i]);
            }
        }
        foreach (var genome in population.Genomes)
        {
            if (genome.DecodedIndivid.Select(x=>x.FunctionalBlocks).SelectMany(x=>x).Count( )== 0)
            {
                throw new Exception("Оптимальную компновку невозможно найти, увеличте площадь!");
            }
        }
        return population;
    }

    private Genome MutationGenome(Genome genome)
    {
        var rand = new Random();
        var genomeCopy = Util.Clone(genome);
        var list3 = Util.Clone(genome.EncodedIndivid);
        var x = rand.Next(1, _functionalBlocks.Count() + 1) - 1;
        var y = rand.Next(1, _functionalBlocks.Count() + 1) - 1;
        while (x == y)
        {
            y = rand.Next(1, _functionalBlocks.Count() + 1) - 1;
        }
        
        (list3[x], list3[y]) = (list3[y], list3[x]);
        
        list3.Reverse();
            
        foreach (var hardPartPcb in genomeCopy.DecodedIndivid)
        {
            hardPartPcb.FunctionalBlocks.Clear();
        }

        var h = 0;
        for (int i = list3.Count-1; i >=0; i--)
        {
            var isEnd1 = true;
            for(int k = 0; k < genomeCopy.DecodedIndivid.Count; k++)
            {
                var sumSquareComponents = list3[i].ComponentsPcb.Sum(y => y.Square());
                var sumSquareHardPart = genomeCopy.DecodedIndivid[k].FunctionalBlocks.Sum(x => x.ComponentsPcb.Sum(y => y.Square()));
                if (sumSquareHardPart + sumSquareComponents  <= genomeCopy.DecodedIndivid[k].Square * _pcb.RateLayout && CheckEms(genomeCopy.DecodedIndivid[k].FunctionalBlocks, list3[i]))
                {
                    genomeCopy.DecodedIndivid[k].FunctionalBlocks.Add(list3[i]);
                    list3.Remove(list3[i]);
                    h = k + 1;
                    isEnd1 = k != genomeCopy.DecodedIndivid.Count - 1;
                    break;
                }
            }

            if (isEnd1)
            {
                for (int j = 0; j < h; j++)
                {
                    genome.DecodedIndivid = Util.LeftSHuffle(genome.DecodedIndivid);
                }
            }
        }
            
        var check = list3.Count > 0;
        if (!check)
        {
            foreach (var hardPartPcb in genomeCopy.DecodedIndivid)
            {
                check = hardPartPcb.FunctionalBlocks.Count == 0 || check;
            }
        }
        
        if (check)
        {
            return genome;
        }
        if (genomeCopy.DecodedIndivid.Select(x=>x.FunctionalBlocks).SelectMany(x=>x).Count( )== 0)
        {
            throw new Exception("Оптимальную компновку невозможно найти, увеличте площадь!");
        }
        return genomeCopy;
        
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
    private bool CheckEquality(List<FunctionalBlock> encodedInidivid1, List<FunctionalBlock> encodedInidivid2)
    {
        for (int i = 0; i < encodedInidivid1.Count; i++)
        {
            if (encodedInidivid1[i].Id != encodedInidivid2[i].Id)
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
                genome.DecodedIndivid.Add(Util.Clone(hardPartPcb));
            }
            genome = Shuffle(genome);

            _maxEms = _ems.Sum(x => x.Value);
            _maxIntermodule = _connectionComponents.Sum(x => x.CountConnection) * _minPaths.Max(x => x.Count);

            genome.FitnessEms = GetFitnessEms(genome);
            genome.FitnessInterModule = GetFitness(genome);

            population.Genomes.Add(Util.Clone(genome));
        }

        population.BestGenome = population.Genomes.OrderBy(i => i.FitnessEms + i.FitnessInterModule).First();
        _listFitness.Add(population.BestGenome.FitnessEms + population.BestGenome.FitnessInterModule);
        return population;
    }

    private double GetFitnessEms(Genome genome)
    {
        long ems = 0;
        foreach (var hardPartPcb in genome.DecodedIndivid)
        {
            var idFunctional = hardPartPcb.FunctionalBlocks.Select(x => x.Id).ToList();
            ems += _ems.Where(x =>
                    idFunctional.Contains((long)x.FunctionalBlock1Id) &&
                    idFunctional.Contains((long)x.FunctionalBlock2Id))
                .Sum(x => x.Value);
        }

        var fitnessEms = _maxEms != 0 ? (double)ems / (double)_maxEms * (double)100 * _solution.RateEms : 0;
        return fitnessEms;
    }

    private double GetFitness(Genome genome)
    {
        long fitness = 0;
        for (int i = 0; i < genome.DecodedIndivid.Count; i++)
        {
            var idComponents1 = genome.DecodedIndivid[i].FunctionalBlocks
                .Select(x => x.ComponentsPcb)
                .SelectMany(i => i)
                .Distinct()
                .Select(x => x.Id)
                .ToList();
            for (int j = i + 1; j < genome.DecodedIndivid.Count; j++)
            {
                var idComponents2 = genome.DecodedIndivid[j].FunctionalBlocks
                    .Select(x => x.ComponentsPcb)
                    .SelectMany(i => i)
                    .Distinct()
                    .Select(x => x.Id)
                    .ToList();
                var path = _minPaths
                    .FirstOrDefault(x =>
                        (x[0] == genome.DecodedIndivid[i].Id &&
                         x[x.Count - 1] == genome.DecodedIndivid[j].Id) ||
                        (x[0] == genome.DecodedIndivid[j].Id &&
                         x[x.Count - 1] == genome.DecodedIndivid[i].Id));
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
        var check = true;
        var cnt = 0;
        while (check && cnt<=2000)
        {
            var functionalBlocksCopy = Util.Clone(_functionalBlocks);
            cnt++;
            functionalBlocksCopy = Util.Shuffle(functionalBlocksCopy);
            genome.EncodedIndivid = Util.Clone(functionalBlocksCopy);
            
            foreach (var hardPartPcb in genome.DecodedIndivid)
            {
                hardPartPcb.FunctionalBlocks.Clear();
            }

            var h = 0;
            for (int i = functionalBlocksCopy.Count-1; i >=0; i--)
            {
                var isEnd1 = true;
                for(int k = 0; k < genome.DecodedIndivid.Count; k++)
                {
                    var sumSquareComponents = functionalBlocksCopy[i].ComponentsPcb.Sum(y => y.Square());
                    var sumSquareHardPart = genome.DecodedIndivid[k].FunctionalBlocks.Sum(x => x.ComponentsPcb.Sum(y => y.Square()));
                    if (sumSquareHardPart + sumSquareComponents  <= genome.DecodedIndivid[k].Square * _pcb.RateLayout && CheckEms(genome.DecodedIndivid[k].FunctionalBlocks, functionalBlocksCopy[i]))
                    {
                        genome.DecodedIndivid[k].FunctionalBlocks.Add(functionalBlocksCopy[i]);
                        functionalBlocksCopy.Remove(functionalBlocksCopy[i]);
                        h = k + 1;
                        isEnd1 = k != genome.DecodedIndivid.Count - 1;
                        break;
                    }
                }

                if (!isEnd1)
                {
                    for (int j = 0; j < h; j++)
                    {
                        genome.DecodedIndivid = Util.LeftSHuffle(genome.DecodedIndivid);
                    }
                }
            }

            
            check = functionalBlocksCopy.Count > 0;
            if (!check)
            {
                foreach (var hardPartPcb in genome.DecodedIndivid)
                {
                    check = hardPartPcb.FunctionalBlocks.Count == 0 || check;
                }
            }
            
            if (check)
            {
                functionalBlocksCopy = Util.Clone(genome.EncodedIndivid);
                foreach (var hardPartPcb in genome.DecodedIndivid)
                {
                    hardPartPcb.FunctionalBlocks.Clear();
                }
                
                var g = genome.DecodedIndivid.Count - 1;
                for (int i = functionalBlocksCopy.Count-1; i >=0; i--)
                {                    
                    var isEnd2 = true;
                    for(int k = genome.DecodedIndivid.Count - 1; k >=0; k--)
                    {
                        var sumSquareComponents = functionalBlocksCopy[i].ComponentsPcb.Sum(y => y.Square());
                        var sumSquareHardPart = genome.DecodedIndivid[k].FunctionalBlocks.Sum(x => x.ComponentsPcb.Sum(y => y.Square()));
                        if (sumSquareHardPart + sumSquareComponents  <= genome.DecodedIndivid[k].Square * _pcb.RateLayout && CheckEms(genome.DecodedIndivid[k].FunctionalBlocks, functionalBlocksCopy[i]))
                        {
                            genome.DecodedIndivid[k].FunctionalBlocks.Add(functionalBlocksCopy[i]);
                            functionalBlocksCopy.Remove(functionalBlocksCopy[i]);
                            g = k - 1;
                            isEnd2 = k == 0;
                            break;
                        }
                    }

                    if (!isEnd2)
                    {
                        for (int j = 0; j < genome.DecodedIndivid.Count - 1 - g; j++)
                        {
                            genome.DecodedIndivid = Util.LeftSHuffle(genome.DecodedIndivid);
                        }
                    }
                }
            }
            
            check = functionalBlocksCopy.Count > 0;
            if (!check)
            {
                foreach (var hardPartPcb in genome.DecodedIndivid)
                {
                    check = hardPartPcb.FunctionalBlocks.Count == 0 || check;
                }
            }

            if (cnt > 1999)
            {
            }
        }
        
        if (cnt>2000)
        {
            throw new Exception("Данная компоновка невозможна, попробуйте изменить размеры модулей, элементов или значение показателя ЭМС.");
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

    public List<FlexPartPcb> GetConnectionsHardPart(Genome genome)
    {
        foreach (var flexPartPcb in _pcb.FlexPartsPcb)
        {
            flexPartPcb.Value = 0;
        }
        for (int i = 0; i < genome.DecodedIndivid.Count; i++)
        {
            for (int j = i + 1; j < genome.DecodedIndivid.Count; j++)
            {
                var comp1 = genome.DecodedIndivid[i].FunctionalBlocks
                    .Select(x => x.ComponentsPcb)
                    .SelectMany(x => x)
                    .Select(x=> x.Id)
                    .ToList();
                var comp2 = genome.DecodedIndivid[j].FunctionalBlocks
                    .Select(x => x.ComponentsPcb)
                    .SelectMany(x => x)
                    .Select(x=> x.Id)
                    .ToList();
                var x = _connectionComponents.Where(x =>
                    comp1.Contains((long)x.ComponentPcb1Id)
                    && comp2.Contains((long)x.ComponentPcb2Id) ||
                    (comp2.Contains((long)x.ComponentPcb1Id)
                     && comp1.Contains((long)x.ComponentPcb2Id)));
                var count = _connectionComponents.Where(x =>
                    comp1.Contains((long)x.ComponentPcb1Id)
                    && comp2.Contains((long)x.ComponentPcb2Id) ||
                    (comp2.Contains((long)x.ComponentPcb1Id)
                     && comp1.Contains((long)x.ComponentPcb2Id)))
                    .Sum(x=> x.CountConnection);
                var path = _minPaths.First(x =>
                    (x[0] == genome.DecodedIndivid[i].Id &&
                     x[x.Count - 1] == genome.DecodedIndivid[j].Id) ||
                    (x[0] == genome.DecodedIndivid[j].Id &&
                     x[x.Count - 1] == genome.DecodedIndivid[i].Id));
                for (int k = 0; k < path.Count - 1; k++)
                {
                    _pcb.FlexPartsPcb.First(x => (x.HardPartPcb1Id == path[k] &&
                                                  x.HardPartPcb2Id == path[k + 1]) ||
                                                 (x.HardPartPcb1Id == path[k] &&
                                                  x.HardPartPcb2Id == path[k + 1])).Value += (int)count;
                }
            }
        }

        return _pcb.FlexPartsPcb;
    }
}