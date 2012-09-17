using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src.datatypes
{
    /// <summary>
    /// Node for the parse tree
    /// </summary>
    public class Node
    {
        public Node Left;
        public Node Right;
        public String[] Attributes = new String[3];
        public String Value;

        public Node()
        {
            Left = Right = null;
            Attributes[0] = Attributes[1] = Attributes[2] = Value = String.Empty;
        }

        public Node Clone()
        {
            Node n = new Node();
            n.Attributes = Attributes;
            n.Value = Value;
            n.Left = Left;
            n.Right = Right;
            return n;
        }

        public void Print(Node node, String dash)
        {
            Console.Write("\n" + dash + "-> " + node.Value);

            String attributes = "";

            if (node.Attributes[0] != "")
                attributes += " " + node.Attributes[0] + ",";
            if (node.Attributes[1] != "")
                attributes += " " + node.Attributes[1] + ",";
            if (node.Attributes[2] != "")
                attributes += " " + node.Attributes[2] + ",";

            if (node.Left != null)
                node.Left.Print(node.Left, dash + "-");
            if (node.Right != null)
                node.Right.Print(node.Right, dash + "-");
        }
    }
}
