using System;
using System.Collections.Generic;
using System.Linq;

namespace Protogen
{
    public static class OriReachable
    {
        public static List<Node> ReachableCollecting(AreaGraph graph, Inventory inventory,
            Dictionary<string, Inventory> placements)
        {
            List<Node> reachableOrder = new List<Node>();
            HashSet<string> lastReachable = new HashSet<string>();
            bool didUpdate;
            do
            {
                var newReachable = Reachable(graph, inventory);
                didUpdate = !newReachable.SetEquals(lastReachable);
                foreach (string nodeName in newReachable.Except(lastReachable))
                {
                    if (placements.ContainsKey(nodeName))
                    {
                        inventory += placements[nodeName];
                    }

                    reachableOrder.Add(graph.NodesByName[nodeName]);
                }

                lastReachable = newReachable;
            } while (didUpdate);

            return reachableOrder;
        }

        public static HashSet<string> Reachable(AreaGraph graph, Inventory inventory, string startNode = null)
        {
            if(startNode == null || !graph.OutgoingConnections.ContainsKey(startNode))
                startNode = graph.Origin.Name;
            HashSet<string> reachable = new HashSet<string>();
            reachable.Add(startNode);

            HashSet<string> newNodes = reachable;

            List<Node> accessibleMapstones = new List<Node>();
            int accessedMapstones = 0;

            List<Connection> keystoneConnections = new List<Connection>();
            List<Connection> usedKeystoneConnections = new List<Connection>();

            bool openWorld = inventory.Unlocks.Contains("OpenWorld");
            if (openWorld && startNode == graph.Origin.Name)
            {
                reachable.Add("GladesMain");
            }

            do
            {
                foreach (var connection in newNodes.SelectMany(node => graph.OutgoingConnections[node]?.Where(conn =>
                    conn.Requirement.Keystones > 0 && !usedKeystoneConnections.Contains(conn))))
                {
                    if (!openWorld || connection.Source.Name != "SunkenGladesRunaway" || connection.Destination.Name != "GladesMain")
                    {
                        keystoneConnections.Add(connection);
                    }
                }

                newNodes = new HashSet<string>(newNodes.SelectMany(node => graph.OutgoingConnections[node]?.Where(conn =>
                        !reachable.Contains(conn.Destination.Name) && conn.Requirement.Mapstones == 0 &&
                        conn.Requirement.Keystones == 0 && inventory.Contains(conn.Requirement)))
                    .Select(conn =>
                    {
                        if (conn.Requirement.Unlocks.Contains("Mapstone"))
                            accessibleMapstones.Add(conn.Destination);

                        return conn.Destination.Name;
                    }));

                //Mapstone Logic
                var mapstonesReachable = Math.Min(inventory.Mapstones, accessibleMapstones.Count);
                if (accessedMapstones < mapstonesReachable)
                {
                    foreach (var connection in graph.OutgoingConnections[startNode].Where(conn =>
                        conn.Requirement.Mapstones > accessedMapstones &&
                        conn.Requirement.Mapstones <= mapstonesReachable))
                    {
                        newNodes.Add(connection.Destination.Name);
                    }
                    accessedMapstones = mapstonesReachable;
                }

                //Keystone Logic
                if (newNodes.Count == 0 &&
                    keystoneConnections.Union(usedKeystoneConnections).Sum(conn => conn.Requirement.Keystones) <=
                    inventory.Keystones)
                {
                    foreach (var connection in keystoneConnections)
                    {
                        usedKeystoneConnections.Add(connection);
                        newNodes.Add(connection.Destination.Name);
                    }
                    keystoneConnections.Clear();
                }

                foreach (var node in newNodes)
                {
                    reachable.Add(node);
                }
            } while (newNodes.Count != 0);

            return reachable;
        }
    }
}