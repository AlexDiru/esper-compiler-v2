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
    }
}
