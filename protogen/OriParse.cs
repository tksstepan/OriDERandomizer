using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Protogen
{
    public static class OriParse
    {
        public static AreaGraph Parse(string filename, HashSet<string> logicSets)
        {
            var nodeDictionary = new Dictionary<string, Node>();
            var connections = new List<Connection>();
            Node currentHome = null;
            Node currentDestination = null;
            var hasPath = false;

            HashSet<string> cachedPathsets = new HashSet<string>();
            HashSet<string> cachedExcludePathsets = new HashSet<string>();

            if (!File.Exists(filename))
            {
                return null;
            }

            List<string> logicLines = File.ReadAllLines(filename).ToList();

            foreach (string rawLine in logicLines)
            {
                int commStart = rawLine.IndexOf("--");
                string line = ((commStart == -1) ? rawLine : rawLine.Substring(0, commStart)).Trim();
                if (line.StartsWith("--") || line == "")
                    continue;
                var segments = line.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                var first = segments.First().Trim();
                switch (first)
                {
                case "loc:":
                    nodeDictionary[segments[1]] = new Node(segments[1].Trim(), NodeType.Pickup);
                    break;
                case "home:":
                    if (currentDestination != null && currentHome != null && !hasPath)
                    {
                        connections.Add(new Connection(currentHome, currentDestination, new Inventory()));
                        hasPath = true;
                    }

                    currentHome = nodeDictionary.GetOrPut(segments[1].Trim(), () => new Node(segments[1].Trim(), NodeType.Anchor));
                    break;
                case "pickup:":
                case "conn:":
                    if (currentDestination != null && currentHome != null && !hasPath)
                    {
                        connections.Add(new Connection(currentHome, currentDestination, new Inventory()));
                    }

                    currentDestination = nodeDictionary.GetOrPut(segments[1].Trim(), () => new Node(segments[1].Trim(), NodeType.Anchor));
                    hasPath = false;
                    break;
                default:
                    bool found = false;
                    if (cachedPathsets.Contains(first))
                    {
                        found = true;
                    }
                    else if (!cachedExcludePathsets.Contains(first))
                    {
                        if (logicSets.Any(it => first.StartsWith(it)))
                        {
                            cachedPathsets.Add(first);
                            found = true;
                        }
                        else
                        {
                            cachedExcludePathsets.Add(first);
                        }
                    }

                    if (found)
                    {
                        var inventory = ParseRequirement(segments.Skip(1));
                        connections.Add(new Connection(currentHome, currentDestination, inventory));
                    }
                    hasPath = true;
                    break;
                }
            }

            for (var i = 1; i <= 9; i++)
            {
                var adjustedCount = (i == 9) ? 11 : ((i == 8) ? 9 : i);
                var node = new Node("Map" + i, NodeType.Pickup);
                nodeDictionary[node.Name] = node;
                connections.Add(new Connection(nodeDictionary[Origin], node, new Inventory{Mapstones = adjustedCount}));
            }

            return new AreaGraph(
                nodeDictionary[Origin],
                nodeDictionary.Values.ToList(),
                connections
            );
        }

        private static Inventory ParseRequirement(IEnumerable<string> requirements)
        {
            var resultInventory = new Inventory();
            foreach (var req in requirements)
            {
                var trimmed = req.Trim();
                if (trimmed.Contains("="))
                {
                    var parts = trimmed.Split(new char[]{'='});
                    var resource = parts[0].Trim();
                    var value = int.Parse(parts[1].Trim());
                    switch (resource)
                    {
                        case "Health":
                            resultInventory.Health = value;
                            break;
                        case "Energy":
                            resultInventory.Energy = value;
                            break;
                        case "Ability":
                            resultInventory.Acs = value;
                            break;
                        case "Keystone":
                            resultInventory.Keystones = value;
                            break;
                    }
                }
                else
                {
                    resultInventory.Unlocks.Add(trimmed);
                }
            }

            return resultInventory;
        }

        public const string Origin = "SunkenGladesRunaway";
    }
}