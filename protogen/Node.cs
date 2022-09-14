using System;
using System.Collections.Generic;

namespace Protogen
{
    public class Node
    {
        public Node(string name, NodeType type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name;

        public NodeType Type;
    }
}