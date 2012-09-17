using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using esper_compiler.src.datatypes;

namespace esper_compiler.src
{
    class Parser : TokenParser
    {
        public Node Root;
        private readonly Int32 MaxPrecedenceLevel = 6;
        private static Int32 RetType;

        public Parser()
        {
            ResetToken();
        }

        private void ParseParameters(ref Node node, String functionName, Int32 parameterIndex)
        {
            //If end of parameter parsing
            if (CurrentToken.Value.Equals(")"))
                return;

            node = new Node();
            node.Value = "PARAMETER";

            //Parse the expression in the node's left child
            Int32 type = ParseExpression(ref node.Left, 0);

            //Check if the types of the expression and parameter match
            if (!Database.CheckTypesAreEqual(type,
                                             Database.GetParameterType(functionName, parameterIndex)))
                Error("Invalid parameter type", CurrentToken.LineStart, CurrentToken.CharacterPosition);

            //This parameter has been parsed, now check for future parameters
            if (parameterIndex != Database.GetParameterNumber(functionName) - 1)
            {
                //Two parameters must be separated by a comma
                if (!CurrentToken.Value.Equals(","))
                    Error("Expected comma to separate more parameters", CurrentToken.LineStart);

                NextToken();

                if (CurrentToken.Value.Equals(")"))
                    Error("Expected more parameters", CurrentToken.LineStart);

                ParseParameters(ref node.Right, functionName, parameterIndex + 1);
            }
        }

        private Int32 ParseMember(ref Node node, Int32 type)
        {
            //The node's value is MEMBER, the left is the parent factor, right the member
            Node current = node;
            node.Left = new Node();
            node.Right = new Node();
            node.Left = current;
            node.Value = "MEMBER";
            node.Attributes[0] = String.Empty;

            //Check if it is a valid member and get the data type
            NextToken();

            if (!Database.CheckMember(type, CurrentToken.Value))
                Error("Invalid member", CurrentToken.LineStart, CurrentToken.CharacterPosition);

            Int32 memberType = Database.GetMemberType(type, CurrentToken.Value);

            //The right value is the member's name
            node.Right.Value = CurrentToken.Value;

            NextToken();

            //Check if another type is incoming
            if (CurrentToken.Value.Equals("."))
                memberType = ParseMember(ref node.Right, memberType);

            node.Attributes[2] = Database.GetTypeName(memberType);

            return memberType;
        }

        private Int32 ParseVariableFactor(ref Node node)
        {
            //Not an identifier or variable name
            if (!CurrentToken.Type.Equals(TokenType.Identifier))
                Error("Invalid factor", CurrentToken.LineStart, CurrentToken.CharacterPosition);

            if (!Database.CheckVariable(CurrentToken.Value))
                Error("Invalid variable name", CurrentToken.LineStart, CurrentToken.CharacterPosition);

            //The node's value is the variable's unique ID, the factor type is the variable's data type
            node.Value = Database.GetVarId(CurrentToken.Value, null);
            Int32 type = Database.GetVarType(CurrentToken.Value, null);
            node.Attributes[0] = "VARIABLE";
            node.Attributes[2] = Database.GetTypeName(type);
            NextToken();

            //A . means a member is incoming
            if (CurrentToken.Value.Equals("."))
                type = ParseMember(ref node, type);

            return type;
        }

        private Int32 ParseFactor(ref Node node)
        {
            //A factor can be an expression in brackets
            if (CurrentToken.Value.Equals("("))
            {
                NextToken();
                Int32 type = ParseExpression(ref node, 0);

                if (!CurrentToken.Value.Equals(")"))
                    Error("Expected ending parenthesis: )", CurrentToken.LineStart);
                NextToken();

                //A bracketed expression can also have a member
                if (CurrentToken.Value.Equals("."))
                    type = ParseMember(ref node, type);

                return type;
            }

            node = new Node();

            if (CurrentToken.Type.Equals(TokenType.String))
            {
                node.Value = CurrentToken.Value;
                node.Attributes[0] = "VALUE";
                node.Attributes[2] = "STRING";
                NextToken();
                return Database.GetType(node.Attributes[2]);
            }
            else if (CurrentToken.Type.Equals(TokenType.Number))
            {
                node.Value = CurrentToken.Value;
                node.Attributes[0] = "VALUE";
                if (CurrentToken.Value.Contains('.'))
                    node.Attributes[2] = "DOUBLE";
                else
                    node.Attributes[2] = "INT";
                NextToken();
                return Database.GetType(node.Attributes[2]);
            }
            else if (CurrentToken.Type.Equals(TokenType.Character))
            {
                node.Value = CurrentToken.Value;
                node.Attributes[0] = "VALUE";
                node.Attributes[2] = "CHAR";
                NextToken();
                return Database.GetType(node.Attributes[2]);
            }
            else if (CurrentToken.Value.Equals("TRUE") || CurrentToken.Value.Equals("FALSE"))
            {
                node.Value = CurrentToken.Value;
                node.Attributes[0] = "VALUE";
                node.Attributes[2] = "BOOL";
                NextToken();
                return Database.GetType(node.Attributes[2]);
            }
            else if (Database.CheckFunction(CurrentToken.Value))
            {
                node.Value = CurrentToken.Value;
                NextToken();

                if (!CurrentToken.Value.Equals("("))
                    Error("Expected parameters list for function: " + CurrentToken.Value,
                          CurrentToken.LineStart);
                NextToken();

                if (Database.GetParameterNumber(node.Value) != 0)
                    ParseParameters(ref node.Right, node.Value, 0);

                if (!CurrentToken.Value.Equals(")"))
                    Error("Expected ending parenthesis: )", CurrentToken.LineStart);
                NextToken();

                node.Attributes[0] = "FUNCTION";
                node.Attributes[2] = Database.GetTypeName(Database.GetFunctionType(node.Value));

                return Database.GetType(node.Attributes[2]);
            }
            else
                return ParseVariableFactor(ref node);

            return -1;
        }

        /// <summary>
        /// Factors can be prefixed or postfixed with unary operators
        /// </summary>
        private Int32 ParseFactorEx(ref Node node)
        {
            //If a pre-unary operator exists
            if (Database.CheckUnaryOperator(CurrentToken.Value, true))
            {
                node = new Node();
                node.Value = CurrentToken.Value;
                node.Attributes[0] = "UNARY_OPERATOR";
                NextToken();

                Int32 type = ParseFactorEx(ref node.Right);
                node.Attributes[2] = Database.GetTypeName(Database.GetReturnType(node.Value, type, true));
                return Database.GetType(node.Attributes[2]);
            }

            //No more pre-operators, parse the actual factor
            ParseFactor(ref node);

            //Check for post-unary operators
            while (Database.CheckUnaryOperator(CurrentToken.Value, false))
            {
                Node temp;
                temp = node;
                node.Left = new Node();
                node.Left = temp;

                node.Value = CurrentToken.Value;
                node.Attributes[0] = "UNARY_OPERATOR";
                NextToken();

                Int32 type = Database.GetReturnType(node.Value, 
                                                    Database.GetType(node.Left.Attributes[2]), 
                                                    false);

                node.Attributes[2] = Database.GetTypeName(type);
            }

            return Database.GetType(node.Attributes[2]);
        }

        private Int32 ParseExpression(ref Node node, int precedenceLevel)
        {
            //If we have reached over the maximum precedence level, we need to parse a factor
            if (precedenceLevel > MaxPrecedenceLevel)
                return ParseFactorEx(ref node);

            //Parse another expression of higher precedence and set it to LType
            Int32 lType = ParseExpression(ref node, precedenceLevel+1);

            Int32 temporaryToken;

            //Check if an operator of current precedence level exists
            while (Database.CheckOperator(CurrentToken.Value, precedenceLevel))
            {
                //An operator exists but it may not be what we want, mark the token in case we want
                //to return to it
                temporaryToken = TokenIndex;

                //Then the operator is the node and the previously parsed expression is the left child node
                String op = CurrentToken.Value;

                Node temp;
                temp = node;
                node.Left = new Node();
                node.Left = temp.Clone();

                node.Value = op;
                node.Attributes[0] = "OPERATOR";
                NextToken();

                //Right child node is the other expression of high precedence level
                Int32 rType = ParseExpression(ref node.Right, precedenceLevel + 1);

                //Return type retrieved from database
                Int32 retType = Database.GetReturnType(op, lType, rType, precedenceLevel);

                //Catch here
                //If retType == -1 then the operator with LType, RType and Precedence level DO NOT exist
                //There may exist an operator with such LType and RType but a different precedence level
                //do not match
                //In such case, we parsed the operator in the wrong precedence level
                //We have to reset the previously saved token's position and also the expression tree
                if (retType.Equals(-1))
                {
                    node.Right = null;
                    node = temp;
                    TokenIndex = temporaryToken;
                    CurrentToken = Tokens[TokenIndex];
                    break;
                }

                //Valid return type is the type of the expression
                node.Attributes[2] = Database.GetTypeName(retType);
                lType = retType;
            }

            return lType;
        }

        private void ParseAssign(ref Node node)
        {
            node = new Node();
            node.Value = "ASSIGN";
            node.Left = new Node();

            //Parse the variable
            Int32 type = ParseVariableFactor(ref node.Left);

            node.Attributes[2] = Database.GetTypeName(type);

            //Assignment statement must be followed by an equal sign
            if (CurrentToken.Value != "=")
                Error("Expected assignment operator: =", CurrentToken.LineStart);

            NextToken();

            //Expression is on the right child
            Int32 exprType = ParseExpression(ref node.Right, 0);

            //Check type match between variable and expression
            if (!Database.CheckTypesAreEqual(exprType, type))
                Error("Assignment types mismatched", CurrentToken.LineStart);
        }

        private void ParseBlock(ref Node node)
        {
            //A block is a number of statements enclosed in {...}
            if (CurrentToken.Value.Equals("{"))
            {
                //Each block starts a new scope
                Database.NewScope();
                NextToken();

                ParseStatements(ref node);

                if (!CurrentToken.Value.Equals("}"))
                    Error("Expected ending of the block: }", CurrentToken.LineStart);

                Database.ParentScope();
            }
            //Or it can just be a single statement
            else
                ParseStatement(ref node);
        }

        private void ParseWhile(ref Node node)
        {
            node = new Node();
            node.Value = "WHILE";

            NextToken();

            if (!CurrentToken.Value.Equals("("))
                Error("Enclose the condition in (...)", CurrentToken.LineStart);

            NextToken();

            if (ParseExpression(ref node.Left, 0) != Database.GetType("BOOL"))
                Error("Expression must be a boolean in the while", CurrentToken.LineStart);

            if (!CurrentToken.Value.Equals(")"))
                Error("Enclose the condition in (...)", CurrentToken.LineStart);

            NextToken();

            ParseBlock(ref node.Right);
        }

        private void ParseIf(ref Node node)
        {
            node = new Node();
            node.Value = "IF";

            NextToken();

            if (!CurrentToken.Value.Equals("("))
                Error("Enclose the condition in (...)", CurrentToken.LineStart);

            NextToken();

            if (ParseExpression(ref node.Left, 0) != Database.GetType("BOOL"))
                Error("IF expressions must be boolean in type", CurrentToken.LineStart);

            if (!CurrentToken.Value.Equals(")"))
                Error("Enclose the condition in (...)", CurrentToken.LineStart);

            NextToken();

            ParseBlock(ref node.Right);

            //If followed by an else statement, parse the else tree
            //Node's value is else
            //If -> Else If then left and right are both if trees
            //If -> Else, left child is previous IF tree and right child is the series of else statements
            NextToken();
            if (CurrentToken.Value.Equals("ELSE"))
            {
                NextToken();
                NextToken();
                Node temp = node;
                node.Left = new Node();
                node.Left = temp;
                node.Value = "ELSE";

                if (CurrentToken.Value.Equals("IF"))
                    ParseIf(ref node.Right);
                else
                    ParseBlock(ref node.Right);
            }
        }

        private void ParseReturn(ref Node node)
        {
            node = new Node();
            node.Value = "RETURN";

            NextToken();
            
            if (RetType != Database.GetType("VOID"))
            {
                if (!Database.CheckTypesAreEqual(ParseExpression(ref node.Right, 0), RetType))
                {
                    Error("Return and function types mistmatched", CurrentToken.LineStart);
                }
            }
        }

        private void ParseVariableDeclaration(ref Node node)
        {
            VariableInfo var = new VariableInfo();

            ParseDeclaration(ref var);

            node = new Node();
            node.Value = "DECLARE";

            node.Attributes[0] = Database.GetVarId(var.Name, null);
            node.Attributes[1] = Database.GetTypeName(var.Type);
        }

        private Boolean CheckAssignComing()
        {
            //If a variable, return true
            return Database.CheckVariable(CurrentToken.Value);
        }

        private void ParseStatement(ref Node node)
        {
            //Check the first token to determine what to do
            if (CurrentToken.Value.Equals("IF"))
                ParseIf(ref node);
            else if (CurrentToken.Value.Equals("WHILE"))
                ParseWhile(ref node);
            else if (CurrentToken.Value.Equals("RETURN"))
                ParseReturn(ref node);
            else if (Database.CheckType(CurrentToken.Value))
                ParseVariableDeclaration(ref node);
            else if (CheckAssignComing())
                ParseAssign(ref node);
            else
                ParseFactorEx(ref node);//Error("Unknown statement", CurrentToken.LineStart);
        }

        private void ParseStatements(ref Node node)
        {
            if (CurrentToken.Value.Equals("}"))
                return;

            node = new Node();
            node.Value = "STATEMENT";

            if (!CurrentToken.Value.Equals(";"))
                ParseStatement(ref node.Left);

            //if (!CurrentToken.Value.Equals(";"))
                //Error("Expected semicolon", CurrentToken.LineStart);

            NextToken();
            ParseStatements(ref node.Right);
        }

        private void ParseFunction(ref Node node)
        {
            RetType = ParseType();

            node.Value = "FUNCTION";
            node.Attributes[0] = CurrentToken.Value;

            Database.AddFunctionToScope(CurrentToken.Value);

            //Skip parameter list - preparser took care of this
            while (!CurrentToken.Value.Equals(")"))
                NextToken();

            NextToken();

            //{ must exist
            if (!CurrentToken.Value.Equals("{"))
                Error("Expected function block {...}", CurrentToken.LineStart);

            NextToken();

            ParseStatements(ref node.Left);

            if (!CurrentToken.Value.Equals("}"))
                Error("Invalid ending: expected '}'", CurrentToken.LineStart);

            NextToken();
        }


        private void ParseFunctionsDefinitions(ref Node node)
        {
            RetType = -1;
            Node currentNode;

            Boolean firstFunc = true;

            ResetToken();
            
                while (CurrentToken.LineStart != Int32.MaxValue)
                {
                    if (CheckFunctionIncoming())
                    {
                        Database.NewScope();

                        if (firstFunc)
                        {
                            node = new Node();
                            currentNode = node;
                            firstFunc = false;
                        }
                        else
                        {
                            node.Right = new Node();
                            node = node.Right;
                        }

                        ParseFunction(ref node);

                        Database.ParentScope();
                    }
                    NextToken();
                }
        }

        public void Parse()
        {
            

            ResetToken();
            ParseFunctionsDefinitions(ref Root);
        }
    }
}
