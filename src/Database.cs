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
        static List<FunctionInfo> Functions;

        /// <summary>
        /// Setup database with default types and operators
        /// </summary>
        public static void Setup()
        {
            GlobalScope.Id = "_";
            GlobalScope.ParentScope = null;

            CurrentScope = GlobalScope;

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

        public static Boolean IsNumber(Int32 type)
        {
            return new String[] { "DOUBLE", "INT", "CHAR", "FLOAT" }.Contains(Types[type].Name);
        }

        public static Boolean CheckTypesAreEqual(Int32 typeOne, Int32 typeTwo)
        {
            return (typeOne == typeTwo || (IsNumber(typeOne) && IsNumber(typeTwo)));
        }

        public static Boolean CheckVarInScope(String Name, Scope scope)
        {
            foreach (var variable in scope.Variables)
            {
                if (variable.Name.Equals(Name))
                    return true;
            }

            //Check parent scopes
            if (scope.ParentScope != null)
                return CheckVarInScope(Name, scope.ParentScope);

            return false;
        }

        public static Boolean CheckReserved(String name)
        {
            return new String[] { "NEW", "DELETE", "IF", "ELSE", "WHILE", "RETURN" }.Contains(name);
        }

        public static Boolean CheckValidName(String name)
        {
            return !(CheckVarInScope(name, CurrentScope) || CheckFunction(name) || CheckType(name) ||
                     CheckReserved(name) || CheckOperator(name)) ;
        }

        public static Boolean CheckValidOperator(String name, Int32 lType, Int32 rType, Int32 precedance)
        {
            if (CheckVarInScope(name, CurrentScope) || CheckFunction(name) || CheckType(name) ||
                CheckReserved(name))
                return false;

            foreach (var op in Operators)
            {
                if (op.Name.Equals(name) && op.Precedance.Equals(precedance) &&
                    CheckTypesAreEqual(op.LType, lType) && CheckTypesAreEqual(op.RType, rType))
                    return false;
            }

            return true;
        }

        public static Boolean CheckVariable(String name)
        {
            return CheckVarInScope(name, CurrentScope);
        }

        public static Int32 GetVarType(String name, Scope scope)
        {
            foreach (var variable in scope.Variables)
            {
                if (variable.Name.Equals(name))
                    return variable.Type;
            }

            if (scope.ParentScope != null)
                return GetVarType(name, scope.ParentScope);

            return -1;
        }

        public static String GetVarId(String name, Scope scope)
        {
            foreach (var variable in scope.Variables)
            {
                if (variable.Name.Equals(name))
                    return variable.Id;
            }

            if (scope.ParentScope != null)
                return GetVarId(name, scope.ParentScope);

            return String.Empty;
        }

        public static Boolean CheckFunction(String name)
        {
            foreach (var function in Functions)
            {
                if (function.Name.Equals(name))
                    return true;
            }

            return false;
        }

        public static Int32 GetFunctionType(String name)
        {
            foreach (var function in Functions)
            {
                if (function.Name.Equals(name))
                    return function.Type;
            }

            return -1;
        }

        public static Int32 GetParameterNumber(String functionName)
        {
            foreach (var function in Functions)
            {
                if (function.Name.Equals(function))
                    return function.Parameters.Count;
            }

            return -1;
        }

        public static Boolean CheckParameter(String functionName, String parameterName)
        {
            foreach (var function in Functions)
            {
                if (function.Name.Equals(functionName))
                {
                    foreach (var parameter in function.Parameters)
                    {
                        if (parameter.Name.Equals(parameterName))
                            return true;
                    }
                    return false;
                }
            }
            return false;
        }

        public static Int32 GetParameterId(String functionName, String parameterName)
        {
            foreach (var function in Functions)
            {
                if (function.Name.Equals(functionName))
                {
                    for (int p = 0; p < function.Parameters.Count; p++)
                    {
                        if (function.Parameters[p].Name.Equals(parameterName))
                            return p;
                    }
                    return -1;
                }
            }
            return -1;
        }

        public static String GetParameterName(String functionName, Int32 paramIndex)
        {
            foreach (var function in Functions)
            {
                if (function.Name.Equals(functionName))
                {
                    return function.Parameters[paramIndex].Name;
                }
            }

            return String.Empty;
        }

        public static Int32 GetParameterType(String functionName, Int32 paramIndex)
        {
            foreach (var function in Functions)
            {
                if (function.Name.Equals(functionName))
                {
                    return function.Parameters[paramIndex].Type;
                }
            }

            return -1;
        }

        public static Int32 GetParameterType(String functionName, String parameterName)
        {
            foreach (var function in Functions)
            {
                if (function.Name.Equals(functionName))
                {
                    foreach (var parameter in function.Parameters)
                    {
                        if (parameter.Name.Equals(parameterName))
                        {
                            return parameter.Type;
                        }
                        return -1;
                    }
                }
            }
            return -1;
        }

        public static Boolean CheckType(String name)
        {
            foreach (var type in Types)
            {
                if (type.Name.Equals(name))
                    return true;
            }
            return false;
        }

        public static Int32 GetType(String name)
        {
            for (int i = 0; i < Types.Count; i++)
            {
                if (Types[i].Name.Equals(name))
                    return i;
            }
            return -1;
        }

        public static String GetTypeName(Int32 type)
        {
            if (type < 0 || type >= Types.Count)
                return String.Empty;
            return Types[type].Name;
        }

        public static Int32 GetNumberOfMembers(Int32 type)
        {
            if (type < 0 || type >= Types.Count)
                return -1;
            return Types[type].Members.Count;
        }

        public static Boolean CheckMember(Int32 type, String name)
        {
            if (type < 0 || type >= Types.Count)
                return false;

            foreach (var member in Types[type].Members)
                if (member.Name.Equals(name))
                    return true;

            return false;
        }

        public static Int32 GetMemberId(Int32 type, String name)
        {
            if (type < 0 || type >= Types.Count)
                return -1;

            for (int m = 0; m < Types[type].Members.Count; m++ )
                if (Types[type].Members[m].Equals(name))
                    return m;

            return -1;
        }

        public static String GetMemberName(Int32 type, Int32 memberId)
        {
            if (type < 0 || type >= Types.Count)
                return String.Empty;

            return Types[type].Members[memberId].Name;
        }

        public static Int32 GetMemberType(Int32 type, Int32 memberId)
        {
            if (type < 0 || type >= Types.Count)
                return -1;

            return Types[type].Members[memberId].Type;
        }

        public static Int32 GetMemberType(Int32 type, String name)
        {
            if (type < 0 || type >= Types.Count)
                return -1;

            foreach (var member in Types[type].Members)
                if (member.Name.Equals(name))
                    return member.Type;

            return -1;
        }

        public static Boolean CheckOperator(String name)
        {
            foreach (var op in Operators)
            {
                if (op.Name.Equals(name))
                    return true;
            }
            return false;
        }

        public static Boolean CheckOperator(String name, Int32 precedance)
        {
            foreach (var op in Operators)
            {
                if (op.Name.Equals(name) && op.Precedance.Equals(precedance) && !op.Unary)
                    return true;
            }
            return false;
        }

        public static Boolean CheckUnaryOperator(String name, Boolean pre)
        {
            foreach (var op in Operators)
            {
                if (op.Name.Equals(name) && op.Unary &&
                    (pre ? op.LType.Equals(GetType("VOID")) : op.RType.Equals(GetType("VOID"))))
                    return true;
            }
            return false;
        }

        public static Int32 GetReturnType(String opName, Int32 lType, Int32 rType, Int32 precedance)
        {
            foreach (var op in Operators)
            {
                if (op.Name.Equals(opName) && precedance.Equals(op.Precedance) &&
                    CheckTypesAreEqual(op.LType, lType) && CheckTypesAreEqual(op.RType, rType))
                    return op.RetType;
            }
            return -1;
        }

        public static Int32 GetReturnType(String opName, Int32 type, Boolean pre)
        {
            foreach (var op in Operators)
            {
                if (op.Name.Equals(opName) &&
                    (pre ? (op.LType.Equals(GetType("VOID")) && CheckTypesAreEqual(op.RType, type)) :
                           (op.RType.Equals(GetType("VOID")) && CheckTypesAreEqual(op.LType, type))))
                    return op.RetType;
            }

            return -1;
        }

        public static Int32 AddType(TypeInfo type)
        {
            Types.Add(type);
            return Types.Count - 1;
        }

        public static void ChangeType(Int32 typeIndex, TypeInfo type)
        {
            Types[typeIndex] = type;
        }

        public static void AddTypeNewFunc(Int32 typeIndex, FunctionInfo func)
        {
            Types[typeIndex].NewFunction = func;
        }

        public static void AddTypeDelFunc(Int32 typeIndex, FunctionInfo func)
        {
            Types[typeIndex].DeleteFunction = func;
        }

        public static void AddOperator(OperatorInfo op)
        {
            Operators.Add(op);
        }

        public static void AddFunction(FunctionInfo func)
        {
            Functions.Add(func);
        }

        public static void AddVar(VariableInfo var, Scope scope)
        {
            var.Id = scope.Id + var.Name;
            scope.Variables.Add(var);
        }

        public static void NewScope()
        {
            Scope scope = new Scope();
            scope.ParentScope = CurrentScope;
            scope.Id = scope.ParentScope.Id + "_";
            CurrentScope.ChildScope.Add(scope);
            CurrentScope = scope;
        }

        public static void ParentScope()
        {
            CurrentScope = CurrentScope.ParentScope;
        }

        public static void AddFunctionToScope(String functionName)
        {
            foreach (var function in Functions)
            {
                if (function.Name.Equals(functionName))
                {
                    foreach (var parameter in function.Parameters)
                    {
                        parameter.Id = CurrentScope.Id + parameter.Name;
                        CurrentScope.Variables.Add(parameter);
                    }
                }
            }
        }

        public static void AddOperatorFunctionToScope(String opName)
        {
            foreach (var op in Operators)
            {
                if (op.Name.Equals(opName))
                {
                    foreach (var parameter in op.OperatorFunction.Parameters)
                    {
                        parameter.Id = CurrentScope.Id + parameter.Name;
                        CurrentScope.Variables.Add(parameter);
                    }
                }
            }
        }

        public static void AddTypeFunctionToScope(Int32 type, String functionName)
        {
            if (functionName.Equals("NEW"))
            {
                foreach (var parameter in Types[type].NewFunction.Parameters)
                {
                    parameter.Id = CurrentScope.Id + parameter.Name;
                    CurrentScope.Variables.Add(parameter);
                }
            }
            else
            {
                foreach (var parameter in Types[type].DeleteFunction.Parameters)
                {
                    parameter.Id = CurrentScope.Id + parameter.Name;
                    CurrentScope.Variables.Add(parameter);
                }
            }
        }

        public static void PrintOutTypes()
        {
            foreach (var type in Types)
            {
                Console.Write("\n\tFOUND TYPE: " + type.Name);

                if (type.Members.Count > 0)
                    Console.Write("\n\t\tMembers:");

                foreach (var member in type.Members)
                    Console.Write("\n\t\t\t" + member.Name + "==>" + GetTypeName(member.Type));
            }
        }

        public static void PrintOutGlobals()
        {
            foreach (var variable in CurrentScope.Variables)
            {
                Console.Write("FOUND GLOBAL VARIABLE");
                Console.Write("\n\t\t" + variable.Name + "==>" + GetTypeName(variable.Type));
            }
        }

        public static void PrintOutFunctions()
        {
            foreach (var function in Functions)
            {
                Console.Write("FOUND FUNCTION: " + function.Name);
                Console.Write("\n\t\tType: " + GetTypeName(function.Type));

                if (function.Parameters.Count > 0)
                    Console.Write("\n\t\tParameters:");

                foreach (var parameter in function.Parameters)
                {
                    Console.Write("\n\t\t\t" + parameter.Name + "==>" + GetTypeName(parameter.Type));
                }
            }
        }

        public static void PrintOutOperators()
        {
            foreach (var op in Operators)
            {
                Console.Write("\n\tFOUND OPERATOR: " + op.Name);
                Console.Write("\n\tRetType: " + op.RetType);
                Console.Write("\n\tLType: " + op.LType);
                Console.Write("\n\tRType: " + op.RType);
                Console.Write("\n\tPrecedence: " + op.Precedance);
            }
        }
    }
}
