using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src
{
    class TypeInfo
    {
        public String Name;
        public List<VariableInfo> Members;

        public FunctionInfo NewFunction;
        public FunctionInfo DeleteFunction;

        public TypeInfo()
        {
            NewFunction = null;
            DeleteFunction = null;
            Members = new List<VariableInfo>();
            Name = String.Empty;
        }
    }
}
