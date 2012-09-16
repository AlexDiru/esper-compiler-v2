using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src
{
    /// <summary>
    /// Contains all the data for a function
    /// </summary>
    class FunctionInfo
    {
        public String Name;
        public Int32 Type;
        public List<VariableInfo> Parameters = new List<VariableInfo>();
    }
}
