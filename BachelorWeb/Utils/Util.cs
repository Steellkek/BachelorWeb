using BachelorWeb.Models;
using Newtonsoft.Json;

namespace BachelorWeb.Utils;

public static class Util
{
    public static T Clone<T>(T source)
    {
        var serialized = JsonConvert.SerializeObject(source, Formatting.Indented, 
            new JsonSerializerSettings 
            { 
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            });
        return JsonConvert.DeserializeObject<T>(serialized);
    }
    
    
    public static List<T> LeftSHuffle<T>(List<T> list)
    {
        var newElements = list.GetRange(1, list.Count-1);
        newElements.Add(list[0]);
        return newElements;
    }

    public static void Shuffle<T>(List<T> list)
    {
        var rand = new Random();
        for (int i = list.Count - 1; i >= 1; i--)
        {
            int j = rand.Next(i + 1);
            (list[j], list[i]) = (list[i], list[j]);
        }
    }

    public static List<List<int>> FromListToMatrix(PCB pcb)
    {
        var g = new int[pcb.HardPartsPcb.Count, pcb.HardPartsPcb.Count];
        foreach (var flexPart in pcb.FlexPartsPcb)
        {
            g[pcb.HardPartsPcb.FindIndex(x => x.Id == flexPart.HardPartPcb1Id),
                pcb.HardPartsPcb.FindIndex(x => x.Id == flexPart.HardPartPcb2Id)] = 1;
            g[pcb.HardPartsPcb.FindIndex(x => x.Id == flexPart.HardPartPcb2Id),
                pcb.HardPartsPcb.FindIndex(x => x.Id == flexPart.HardPartPcb1Id)] = 1;
        }

        var matrix = new List<List<int>>();
        for (int i = 0; i < pcb.HardPartsPcb.Count; i++)
        {
            var dim = new List<int>();
            for (int j = 0; j < pcb.HardPartsPcb.Count; j++)
            {
                dim.Add(g[i,j]);
            }
            matrix.Add(dim);
        }
        return matrix;
    }
}