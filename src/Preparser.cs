using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using esper_compiler.src.datatypes;

namespace esper_compiler.src
{
    /// <summary>
    /// Used before the parser, to build a database of all types, global variables and functions
    /// </summary>
    class Preparser : TokenParser
    {
        /// <summary>
        /// Parse a type
        /// Syntax:
        /// struct type_name
        /// {
        ///     datatype identifier;
        ///     ...
        /// }
        /// </summary>
        public void ParseTypeDeclaration()
        {
            //Ignore the word TYPE
            NextToken();

            if (CurrentToken.Type != TokenType.Identifier || !Database.CheckValidName(CurrentToken.Value))
                Error("Invalid Type Name", CurrentToken.LineStart);

            //Set type info
            TypeInfo type = new TypeInfo();
            type.Name = CurrentToken.Value;
            
            //Keep a record of all the members to ensure none are repeated
            List<String> memberNames = new List<String>();

            NextToken();

            if (CurrentToken.Value != "{")
                Error("Expected member list { ... }", CurrentToken.LineStart);

            //NEW and DELETE of the type are functions, if they are incoming, skip and parse later
            while (CheckFunctionIncoming())
            {
                SkipBlock();
                NextToken();
            }

            NextToken();

            //Get member declarations
            do
            {
                VariableInfo var = new VariableInfo();
                ParseDeclaration(ref var);

                //Check if valid member
                if (memberNames.Contains(var.Name))
                    Error("Type " + type.Name + " already contains a member named " + var.Name,
                          CurrentToken.LineStart);

                memberNames.Add(var.Name);

                //If the next token isn't a semicolon we have an error
                if (CurrentToken.Value != ";")
                {
                    Error("Expected semicolon", CurrentToken.LineStart);
                }

                NextToken();

                //Skip any function
                while (CheckFunctionIncoming())
                {
                    SkipBlock();
                    NextToken();
                }

                //Add new member to the members list
                type.Members.Add(var);
            } while (CurrentToken.Value != "}");

            //Add the new type to the database
            Database.AddType(type);

            NextToken();
        }

        /// <summary>
        /// Reads through the tokens and looks for any type definitions. When found inserts them into the
        /// type list
        /// </summary>
        public void ParseTypesDeclarations()
        {
            for (int Line = 0; Line < Lines && CurrentToken != null; Line++)
            {
                if (CurrentToken.Value != ";")
                {
                    //Skip any function definition
                    if (CheckFunctionIncoming())
                    {
                        SkipBlock();
                    }
                    else if (!CurrentToken.Value.Equals("STRUCT"))
                        //No type found
                        continue;
                    else
                        //Found a type!
                        ParseTypeDeclaration();
                }

                NextToken();
            }
        }

        /// <summary>
        /// Parses all global variable declarations (not in functions or structs)
        /// Syntax:
        /// datatype identifier;
        /// </summary>
        public void ParseGlobalVariableDeclarations()
        {
            for (int Line = 0; Line < Lines && CurrentToken != null; Line++)
            {
                if (CurrentToken.Value != ";")
                {
                    if (CurrentToken.Value.Equals("STRUCT") || CheckFunctionIncoming())
                    {
                        SkipBlock();
                        PreviousToken();
                    }
                    else if (Database.CheckType(CurrentToken.Value))
                    {
                        VariableInfo var = new VariableInfo();
                        ParseDeclaration(ref var);
                        Database.AddVariable(var, null);
                    }
                }

                NextToken();
            }
        }

        /// <summary>
        /// Parses a function declaration
        /// Syntax:
        /// returntype identifer ( param_list )
        /// {
        ///     statements
        /// }
        /// </summary>
        public FunctionInfo ParseFunctionDeclaration()
        {
            Int32 type = -1;

            type = ParseType();

            //Validity
            if (CurrentToken.Type != TokenType.Identifier)
                Error("Invalid function name", CurrentToken.LineStart);

            FunctionInfo func = new FunctionInfo();
            func.Name = CurrentToken.Value;
            func.Type = type;

            List<String> paramNames = new List<String>();

            NextToken();
            NextToken();

            if (CurrentToken.Value != ")")
            {
                while (true)
                {
                    VariableInfo var = new VariableInfo();
                    ParseDeclaration(ref var);

                    if (!"),".Contains(CurrentToken.Value))
                        Error("Expected comma as param seperator", CurrentToken.LineStart);

                    if (paramNames.Contains(var.Name))
                        Error("Already got parameter", CurrentToken.LineStart, CurrentToken.CharacterPosition);

                    paramNames.Add(var.Name);
                    func.Parameters.Add(var);

                    if (CurrentToken.Value.Equals(")"))
                        break;

                    if (CurrentToken.Value.Equals(","))
                        NextToken();
                }
            }

            NextToken();
            return func;
        }

        /// <summary>
        /// Parses a function and inserts it in the database
        /// </summary>
        public void ParseFunction()
        {
            Database.AddFunction(ParseFunctionDeclaration());
            SkipBlock();
        }

        /// <summary>
        /// Parses all function declarations in the program
        /// </summary>
        public void ParseFunctionDeclarations()
        {
            while (true)
            {
                if (CurrentToken.Value != ";")
                    if (CheckFunctionIncoming())
                        ParseFunction();

                NextToken();

                if (CurrentToken.LineStart.Equals(Int32.MaxValue))
                    break;
            }
        }

        /// <summary>
        /// Does all the preparsing of the three sections
        /// </summary>
        public void Preparse()
        {
            ResetToken();
            ParseTypesDeclarations();
            ResetToken();
            ParseGlobalVariableDeclarations();
            ResetToken();
            ParseFunctionDeclarations();
        }

        /// <summary>
        /// Outputs the result from preparsing
        /// </summary>
        public void PrintOut()
        {
            Database.PrintOutTypes();
            Database.PrintOutGlobals();
            Database.PrintOutFunctions();
            Database.PrintOutOperators();
        }
    }
}
