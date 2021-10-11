using System;

namespace Protogen
{
    public class Connection
    {
        public Connection(Node source, Node destination, Inventory req)
        {
            this.Source = source;

            this.Destination = destination;

            this.Requirement = req;
        }

        public Node Source;

        public Node Destination;

        public Inventory Requirement;
    }
}