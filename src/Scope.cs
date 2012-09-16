using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src
{
    class Scope
    {
        public Scope ParentScope;
        public List<Scope> ChildScope;
        public List<VariableInfo> Variables;

        //Scope ID to prefix variable IDs
        public String Id;

        public Scope()
        {
            ChildScope = new List<Scope>();
            Variables = new List<VariableInfo>();
        }
    }
}
