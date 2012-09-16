using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//setup.h *
//common.cpp *
//stageI.cpp
//data.cpp *

namespace esper_compiler.src
{
    /// <summary>
    /// Contains all the data the compiler uses
    /// </summary>
    static class Database
    {
        static Scope GlobalScope = new Scope();
        static Scope CurrentScope;
        static List<TypeInfo> Types = new List<TypeInfo>();
        static List<OperatorInfo> Operators = new List<OperatorInfo>();
        static List<FunctionInfo> Functions = new List<FunctionInfo>();

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

        /// <summary>
        /// Given a variable type, will return true if that type is a number
        /// </summary>
        public static Boolean IsNumber(Int32 type)
        {
            return new String[] { "DOUBLE", "INT", "CHAR", "FLOAT" }.Contains(Types[type].Name);
        }

        /// <summary>
        /// Given two types, returns true if they are equal
        /// </summary>
        public static Boolean CheckTypesAreEqual(Int32 typeOne, Int32 typeTwo)
        {
            return (typeOne == typeTwo || (IsNumber(typeOne) && IsNumber(typeTwo)));
        }

        /// <summary>
        /// Given a variable name and a scope, it will check if the variable exists in the scope
        /// or any of the scope's parents
        /// </summary>
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

        /// <summary>
        /// Check if a given name is a reserved word (keyword)
        /// </summary>
        public static Boolean CheckReserved(String name)
        {
            return new String[] { "NEW", "DELETE", "IF", "ELSE", "WHILE", "RETURN" }.Contains(name);
        }

        /// <summary>
        /// Check if a given name is valid for an identifier
        /// </summary>
        public static Boolean CheckValidName(String name)
        {
            return !(CheckVarInScope(name, CurrentScope) || CheckFunction(name) || CheckType(name) ||
                     CheckReserved(name) || CheckOperator(name)) ;
        }

        /// <summary>
        /// Check if a given name (and other fields) is valid for an operator
        /// </summary>
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

        /// <summary>
        /// Checks if a variable exists in the current scope
        /// </summary>
        public static Boolean CheckVariable(String name)
        {
            return CheckVarInScope(name, CurrentScope);
        }

        /// <summary>
        /// Given a variable name and scope, will return the type of that variable
        /// </summary>
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

        /// <summary>
        /// Given a variable name and scope, will return the unique ID of that variable
        /// </summary>
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

        /// <summary>
        /// Given a function name, it is determined whether that function exists
        /// </summary>
        public static Boolean CheckFunction(String name)
        {
            foreach (var function in Functions)
            {
                if (function.Name.Equals(name))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Given the name of a function, returns the type of it
        /// </summary>
        public static Int32 GetFunctionType(String name)
        {
            foreach (var function in Functions)
            {
                if (function.Name.Equals(name))
                    return function.Type;
            }

            return -1;
        }

        /// <summary>
        /// Given the name of a function, returns the number of functions it has
        /// </summary>
        public static Int32 GetParameterNumber(String functionName)
        {
            foreach (var function in Functions)
            {
                if (function.Name.Equals(function))
                    return function.Parameters.Count;
            }

            return -1;
        }

        /// <summary>
        /// Given a function name and a parameter name, it will check if the parameter exists
        /// </summary>
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

        /// <summary>
        /// Given a function name and a parameter name, it will return the unique id of the parameter
        /// </summary>
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

        /// <summary>
        /// Given a function name and the parameter index it will return the name of the parameter
        /// </summary>
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

        /// <summary>
        /// Given a function name and the parameter index it will return the type of that parameter
        /// </summary>
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

        /// <summary>
        /// Given a function name and parameter name, it will return the type of the parameter
        /// </summary>
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

        /// <summary>
        /// Given the potential name of a type, returns the existance of a type with that name
        /// </summary>
        public static Boolean CheckType(String name)
        {
            foreach (var type in Types)
            {
                if (type.Name.Equals(name))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Given the name of a type returns the type index of it
        /// </summary>
        public static Int32 GetType(String name)
        {
            for (int i = 0; i < Types.Count; i++)
            {
                if (Types[i].Name.Equals(name))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Given the index of a type, returns it's name
        /// </summary>
        public static String GetTypeName(Int32 type)
        {
            if (type < 0 || type >= Types.Count)
                return String.Empty;
            return Types[type].Name;
        }

        /// <summary>
        /// Given the index of a type, returns the number of members the type has
        /// </summary>
        public static Int32 GetNumberOfMembers(Int32 type)
        {
            if (type < 0 || type >= Types.Count)
                return -1;
            return Types[type].Members.Count;
        }

        /// <summary>
        /// Given a type index and name of a potential members, returns the existance of the member
        /// </summary>
        public static Boolean CheckMember(Int32 type, String name)
        {
            if (type < 0 || type >= Types.Count)
                return false;

            foreach (var member in Types[type].Members)
                if (member.Name.Equals(name))
                    return true;

            return false;
        }

        /// <summary>
        /// Given the type index and name of a member, will return the unique id of the member
        /// </summary>
        public static Int32 GetMemberId(Int32 type, String name)
        {
            if (type < 0 || type >= Types.Count)
                return -1;

            for (int m = 0; m < Types[type].Members.Count; m++ )
                if (Types[type].Members[m].Equals(name))
                    return m;

            return -1;
        }

        /// <summary>
        /// Given the type index and ID of a member, returns the name of the member
        /// </summary>
        public static String GetMemberName(Int32 type, Int32 memberId)
        {
            if (type < 0 || type >= Types.Count)
                return String.Empty;

            return Types[type].Members[memberId].Name;
        }

        /// <summary>
        /// Given the type and member index, returns the type of that member
        /// </summary>
        public static Int32 GetMemberType(Int32 type, Int32 memberId)
        {
            if (type < 0 || type >= Types.Count)
                return -1;

            return Types[type].Members[memberId].Type;
        }

        /// <summary>
        /// Given the type index and name of a member, returns the type of the member
        /// </summary>
        public static Int32 GetMemberType(Int32 type, String name)
        {
            if (type < 0 || type >= Types.Count)
                return -1;

            foreach (var member in Types[type].Members)
                if (member.Name.Equals(name))
                    return member.Type;

            return -1;
        }

        /// <summary>
        /// Given the name of an operator, checks if it exists
        /// </summary>
        public static Boolean CheckOperator(String name)
        {
            foreach (var op in Operators)
            {
                if (op.Name.Equals(name))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Given the name and precedence of an operator, checks if it exists
        /// </summary>
        public static Boolean CheckOperator(String name, Int32 precedance)
        {
            foreach (var op in Operators)
            {
                if (op.Name.Equals(name) && op.Precedance.Equals(precedance) && !op.Unary)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Given the name and whether the unary operator is a prefix, returns whether it exists
        /// </summary>
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

        /// <summary>
        /// Given the properties of an operator, gets its return type
        /// </summary>
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

        /// <summary>
        /// Given the properties of a unary operator, gets its return type
        /// </summary>
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

        /// <summary>
        /// Adds a type to the type list
        /// </summary>
        public static Int32 AddType(TypeInfo type)
        {
            Types.Add(type);
            return Types.Count - 1;
        }

        /// <summary>
        /// Changes a type in the type list
        /// </summary>
        public static void ChangeType(Int32 typeIndex, TypeInfo type)
        {
            Types[typeIndex] = type;
        }

        /// <summary>
        /// Adds the new function to the type
        /// </summary>
        public static void AddTypeNewFunc(Int32 typeIndex, FunctionInfo func)
        {
            Types[typeIndex].NewFunction = func;
        }

        /// <summary>
        /// Adds the delete function to the type
        /// </summary>
        public static void AddTypeDelFunc(Int32 typeIndex, FunctionInfo func)
        {
            Types[typeIndex].DeleteFunction = func;
        }

        /// <summary>
        /// Adds an operator to the operator list
        /// </summary>
        public static void AddOperator(OperatorInfo op)
        {
            Operators.Add(op);
        }

        /// <summary>
        /// Adds a function to the function list
        /// </summary>
        public static void AddFunction(FunctionInfo func)
        {
            Functions.Add(func);
        }

        /// <summary>
        /// Adds a variable to the variable list
        /// </summary>
        public static void AddVariable(VariableInfo var, Scope scope)
        {
            if (scope != null)
                var.Id = scope.Id + var.Name;
            else
            {
                scope = CurrentScope;
                var.Id = var.Name;
            }
            scope.Variables.Add(var);
        }

        /// <summary>
        /// Creates a new scope
        /// </summary>
        public static void NewScope()
        {
            Scope scope = new Scope();
            scope.ParentScope = CurrentScope;
            scope.Id = scope.ParentScope.Id + "_";
            CurrentScope.ChildScope.Add(scope);
            CurrentScope = scope;
        }

        /// <summary>
        /// Moves the current scope back to its parent scope
        /// </summary>
        public static void ParentScope()
        {
            CurrentScope = CurrentScope.ParentScope;
        }

        /// <summary>
        /// Adds a function to the current scope
        /// </summary>
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

        /// <summary>
        /// Adds an operator to the current scope
        /// </summary>
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

        /// <summary>
        /// Adds a type to the current scope
        /// </summary>
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

        /// <summary>
        /// Prints out the types found
        /// </summary>
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

        /// <summary>
        /// Prints out all the globals found
        /// </summary>
        public static void PrintOutGlobals()
        {
            foreach (var variable in CurrentScope.Variables)
            {
                Console.Write("\n\tFOUND GLOBAL VARIABLE");
                Console.Write("\n\t\t" + variable.Name + "==>" + GetTypeName(variable.Type));
            }
        }

        /// <summary>
        /// Prints out all the functions found
        /// </summary>
        public static void PrintOutFunctions()
        {
            foreach (var function in Functions)
            {
                Console.Write("\n\tFOUND FUNCTION: " + function.Name);
                Console.Write("\n\t\tType: " + GetTypeName(function.Type));

                if (function.Parameters.Count > 0)
                    Console.Write("\n\t\tParameters:");

                foreach (var parameter in function.Parameters)
                {
                    Console.Write("\n\t\t\t" + parameter.Name + "==>" + GetTypeName(parameter.Type));
                }
            }
        }

        /// <summary>
        /// Prints out all the operators found
        /// </summary>
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
