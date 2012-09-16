using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src.datatypes
{
    /// <summary>
    /// Contains all the data for a function
    /// </summary>
    public class FunctionInfo
    {
        public String Name;
        public Int32 Type;
        public List<VariableInfo> Parameters = new List<VariableInfo>();
    }
}
