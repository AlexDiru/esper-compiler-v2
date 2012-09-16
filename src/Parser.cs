using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using esper_compiler.src.datatypes;

namespace esper_compiler.src
{
    class Parser : TokenParser
    {
        private Node Root;

        public Parser()
        {
            ResetToken();
        }

        private void ParseParameters(Node node, String functionName, Int32 parameterIndex)
        {
            //If end of parameter parsing
            if (CurrentToken.Value.Equals(")"))
                return;

            node = new Node();
            node.Value = "PARAMETER";

            //Parse the expression in the node's left child
            Int32 type = ParseExpression(node.Left, 0);

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

                ParseParameters(node.Right, functionName, parameterIndex + 1);
            }
        }

        private Int32 ParseMember(Node node, Int32 type)
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
                memberType = ParseMember(node.Right, memberType);

            node.Attributes[2] = Database.GetTypeName(memberType);

            return memberType;
        }

        private int ParseExpression(Node node, int p)
        {
            throw new NotImplementedException();
        }

        private Int32 ParseVariableFactor(Node node)
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
                type = ParseMember(node, type);

            return type;
        }

        private Int32 ParseFactor(Node node)
        {
            //A factor can be an expression in brackets
            if (CurrentToken.Value.Equals("("))
            {
                NextToken();
                Int32 type = ParseExpression(node, 0);

                if (!CurrentToken.Value.Equals(")"))
                    Error("Expected ending parenthesis: )", CurrentToken.LineStart);
                NextToken();

                //A bracketed expression can also have a member
                if (CurrentToken.Value.Equals("."))
                    type = ParseMember(node, type);

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
                    ParseParameters(node.Right, node.Value, 0);

                if (!CurrentToken.Value.Equals(")"))
                    Error("Expected ending parenthesis: )", CurrentToken.LineStart);
                NextToken();

                node.Attributes[0] = "FUNCTION";
                node.Attributes[2] = Database.GetTypeName(node.Value);

                return Database.GetType(node.Attributes[2]);
            }
            else
                return ParseVariableFactor(node);

            return -1;
        }
    }
}
