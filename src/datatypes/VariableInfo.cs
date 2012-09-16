using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src
{
    class VariableInfo
    {
        public String Name;
        public Int32 Type;
        public String Id; //Unique name for each variable, an underscore is added for every new scope
    }
}
