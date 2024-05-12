using System.Xml;
using BachelorWeb.Models;

namespace BachelorWeb.Utils;

public static class FileUtil
{
    public static List<ComponentPcb> GetComponents(XmlElement? xRoot)
    {
        if (xRoot == null)
        {
            throw new Exception();
        }
        
        var сomponents = GetNodeByName(xRoot, "Components");
        var сomponent = GetNodeByName(сomponents, "Component");
        var designs = GetNodeByName(сomponent, "Designs");
        var netlist = GetNodeByName(designs, "Netlist");
        var views = GetNodeByName(netlist, "Views");
            
        var pcbBoard = GetNodeByName(views, "PcbBoard");
        var pcbComponents = GetNodeByName(pcbBoard, "PcbComponents") ?? throw new Exception("Разместите все элементы на PCB");
        var footprintPatterns = GetNodeByName(pcbBoard, "FootprintPatterns");
            
        var schematic = GetNodeByName(views, "Schematic");
        var schematicComponents = GetNodeByName(schematic, "SchematicComponents");
            

        if (pcbComponents.ChildNodes.Count != schematicComponents.ChildNodes.Count)
        {
            throw new Exception("Разместите все элементы на PCB");
        }
            
        var footprintsProject = GetFootprintsProject(footprintPatterns);

        var componentsPcb = GetComponentsPcb(pcbComponents, footprintsProject);

        return componentsPcb;
        //var connections = GetConnections(pcbNets, componentsPcb).Distinct().ToList();
    }
    
    private static List<ComponentPcb> GetComponentsPcb(XmlNode? componentsPcbFile, List<Footprint> footprintsProject)
    {
        var componentsPcb = new List<ComponentPcb>();
        foreach (XmlNode? componentPcbFile in componentsPcbFile.ChildNodes)
        {
            var designator = GetNodeByName(componentPcbFile, "Designator").InnerText;
            var patterIdXmlNode = GetNodeByName(componentPcbFile, "PatternId");
            var patternId = patterIdXmlNode.Attributes["Name"].InnerText;
            var needFootprint = footprintsProject.FirstOrDefault(x => x.Name.Equals(patternId));
            componentsPcb.Add(new ComponentPcb(){ Designator =  designator, Height = needFootprint.Height, Width = needFootprint.Width, Name = needFootprint.Name });
        }
        return componentsPcb;
    }
    
    private static List<Footprint> GetFootprintsProject(XmlNode? footprintPatterns)
    {
        List<Footprint> components = new List<Footprint>();
        if (footprintPatterns != null)
        {
            foreach (XmlNode footprint in footprintPatterns)
            {
                Footprint new_component = new Footprint();
                var internalFootprint = GetNodeByName(footprint, "InternalFootprint");
                new_component.Name = internalFootprint.Attributes["Name"].InnerText;

                XmlNode? technologyItem = GetNodeByAttribute(internalFootprint, "TechnologyItem", "Technology", "Default");
                XmlNode? graphicData = technologyItem != null ? GetNodeByName(technologyItem, "GraphicData") : null;
                XmlNode? PlacementOutlineX =
                    graphicData != null ? GetNodeByName(graphicData, "PlacementOutlineX") : null;
                XmlNode? rectangleG = graphicData != null ? GetNodeByName(PlacementOutlineX, "RectangleG") : null;

                if (rectangleG != null)
                {
                    if (double.TryParse(rectangleG.Attributes["Width"].InnerText.Replace('.', ','), out double width))
                        new_component.Width = width;
                    if (double.TryParse(rectangleG.Attributes["Height"].InnerText.Replace('.', ','), out double height))
                        new_component.Height = height;
                }

                components.Add(new_component);
            }

        }

        return components;
    }
    
    public static List<ConnectionComponent> GetConnections(XmlNode? xRoot, List<ComponentPcb> componentsPcb)
    {
        var сomponents = GetNodeByName(xRoot, "Components");
        var сomponent = GetNodeByName(сomponents, "Component");
        var designs = GetNodeByName(сomponent, "Designs");
        var netlist = GetNodeByName(designs, "Netlist");
        var views = GetNodeByName(netlist, "Views");
        var pcbBoard = GetNodeByName(views, "PcbBoard");
        var pcbNets = GetNodeByName(pcbBoard, "PcbNets");

        var connectionsPcb = new List<ConnectionComponent>();
        int[,] Matrix = new int[componentsPcb.Count,componentsPcb.Count];
        foreach (XmlNode? pcbNet in pcbNets.ChildNodes)
        {
            var pcbPads = GetNodeByName(pcbNet, "PcbPads")
                .ChildNodes
                .Cast<XmlNode>()
                .Select(x => x.Attributes["Component"].InnerText)
                .ToList();

            for (int i = 0; i < pcbPads.Count; i++)
            {
                for (int j = i + 1; j < pcbPads.Count; j++)
                {
                    var el1 = pcbPads[i];
                    var el2 = pcbPads[j];
                    var el1Id = componentsPcb
                        .Select(x=> x.Designator)
                        .ToList()
                        .IndexOf(el1);
                    var el2Id = componentsPcb
                        .Select(x=> x.Designator)
                        .ToList()
                        .IndexOf(el2);
                    Matrix[el1Id, el2Id]++;
                    Matrix[el2Id, el1Id]++;
                }
            }
        }

        for (int i = 0; i < Matrix.GetLength(0); i++)
        {
            for (int j = i + 1; j < Matrix.GetLength(1); j++)
            {
                if (Matrix[i,j] != 0)
                {
                    connectionsPcb.Add(new ConnectionComponent()
                    {
                        ComponentPcb1Id = componentsPcb[i].Id,
                        ComponentPcb2Id = componentsPcb[j].Id,
                        CountConnection = Matrix[i,j],
                    });
                }
            }
        }

        return connectionsPcb;
    }
    
    private static XmlNode? GetNodeByName(XmlNode root, string name)
    {
        XmlNode? node = null;

        foreach (XmlNode xNode in root)
        {
            if (xNode.Name.Equals(name))
            {
                node = xNode; break;
            }
        }

        return node;
    }

    private static XmlNode? GetNodeByAttribute(XmlNode root, string nodeName, string attributeName, string attributeValue)
    {
        List<XmlNode> nodes = new List<XmlNode>();

        foreach (XmlNode xNode in root)
        {
            if (xNode.Name.Equals(nodeName))
            {
                nodes.Add(xNode);
            }
        }

        if (nodes.Count == 0) return null;

        foreach (XmlNode node in nodes)
        {
            XmlAttribute? attribute = node.Attributes[attributeName];
            if (attribute != null && attribute.Value == attributeValue)
                return node;
        }

        return null;
    }
}