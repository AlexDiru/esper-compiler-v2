using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src
{
    class OperatorInfo
    {
        String Name;
        Boolean Unary;

        Int32 LType;
        Int32 RType;
        Int32 RetType;
        Int32 Precedance;

        FunctionInfo OperatorFunction;
        Boolean Series;

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
