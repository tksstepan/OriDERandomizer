using System;
using System.Collections.Generic;
using System.Linq;

namespace Protogen
{
    public class AreaGraph
    {
        public AreaGraph(Node origin, List<Node> nodes, List<Connection> connections)
        {
            this.Origin = origin;
            this.Nodes = nodes;
            this.Connections = connections;

            foreach (Node node in this.Nodes)
            {
                this.NodesByName[node.Name] = node;
            }

            this.OutgoingConnections = Nodes.ToDictionary(node => node.Name, node => Connections.Where(connection => connection.Source.Name == node.Name).ToList<Connection>());
        }

        public Node Origin;

        public List<Node> Nodes;

        public List<Connection> Connections;

        public Dictionary<string, Node> NodesByName = new Dictionary<string, Node>();

        public Dictionary<string, List<Connection>> OutgoingConnections = new Dictionary<string, List<Connection>>();
    }
}