using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//setup.h *
//common.cpp *
//stageI.cpp
//data.cpp

namespace esper_compiler.src
{
    /// <summary>
    /// Contains all the data the compiler uses
    /// </summary>
    static class Database
    {
        static Scope GlobalScope;
        static Scope CurrentScope;
        static List<TypeInfo> Types;
        static List<OperatorInfo> Operators;

        public Database()
        {
            GlobalScope.Id = "_";
            GlobalScope.ParentScope = null;

            CurrentScope = GlobalScope;

            Setup();
        }

        /// <summary>
        /// Setup database with default types and operators
        /// </summary>
        public void Setup()
        {
            //Setup default types
            Types = new List<TypeInfo>();
            Types.Add(new TypeInfo() { Name = "INT" });
            Types.Add(new TypeInfo() { Name = "CHAR" });
            Types.Add(new TypeInfo() { Name = "FLOAT" });
            Types.Add(new TypeInfo() { Name = "BOOL" });
            Types.Add(new TypeInfo() { Name = "DOUBLE" });
            Types.Add(new TypeInfo() { Name = "VOID" });
            Types.Add(new TypeInfo() { Name = "STRING" });

            //Operators
            Operators = new List<OperatorInfo>();
            Operators.Add(new OperatorInfo("+", false, GetType("DOUBLE"), GetType("DOUBLE"), GetType("DOUBLE"), 4));
            Operators.Add(new OperatorInfo("-", false, GetType("DOUBLE"), GetType("DOUBLE"), GetType("DOUBLE"), 4));
            Operators.Add(new OperatorInfo("*", false, GetType("DOUBLE"), GetType("DOUBLE"), GetType("DOUBLE"), 5));
            Operators.Add(new OperatorInfo("/", false, GetType("DOUBLE"), GetType("DOUBLE"), GetType("DOUBLE"), 4));
            Operators.Add(new OperatorInfo("<", false, GetType("DOUBLE"), GetType("DOUBLE"), GetType("BOOL"), 3));
            Operators.Add(new OperatorInfo(">", false, GetType("DOUBLE"), GetType("DOUBLE"), GetType("BOOL"), 3));
            Operators.Add(new OperatorInfo("<=", false, GetType("DOUBLE"), GetType("DOUBLE"), GetType("BOOL"), 3));
            Operators.Add(new OperatorInfo(">=", false, GetType("DOUBLE"), GetType("DOUBLE"), GetType("BOOL"), 3));
            Operators.Add(new OperatorInfo("!=", false, GetType("DOUBLE"), GetType("DOUBLE"), GetType("BOOL"), 3));
            Operators.Add(new OperatorInfo("==", false, GetType("BOOL"), GetType("BOOL"), GetType("BOOL"), 3));
            Operators.Add(new OperatorInfo("==", false, GetType("DOUBLE"), GetType("DOUBLE"), GetType("BOOL"), 3));
            Operators.Add(new OperatorInfo("&&", false, GetType("BOOL"), GetType("BOOL"), GetType("BOOL"), 2));
            Operators.Add(new OperatorInfo("||", false, GetType("BOOL"), GetType("BOOL"), GetType("BOOL"), 2));
            Operators.Add(new OperatorInfo("!", true, GetType("VOID"), GetType("BOOL"), GetType("BOOL"), -1));
            Operators.Add(new OperatorInfo("-", true, GetType("VOID"), GetType("DOUBLE"), GetType("DOUBLE"), -1));
        }

        public Int32 GetType(String s)
        {
            return 0;
        }
    }
}
