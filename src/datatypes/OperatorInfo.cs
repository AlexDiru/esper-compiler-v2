using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src.datatypes
{
    class OperatorInfo
    {
        public String Name;
        public Boolean Unary;

        public Int32 LType;
        public Int32 RType;
        public Int32 RetType;
        public Int32 Precedance;

        public FunctionInfo OperatorFunction;
        public Boolean Series;

        public OperatorInfo(String name, Boolean unary, Int32 lType, Int32 rType, Int32 retType, Int32 precedance)
        {
            Name = name;
            Unary = unary;
            LType = lType;
            RType = rType;
            RetType = retType;
            Precedance = precedance;
        }
    }
}
