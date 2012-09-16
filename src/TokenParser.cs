using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src
{

    

    /// <summary>
    /// Base class containing functions all the parsers use
    /// </summary>
    class TokenParser
    {
        Int32 TokenIndex;

        /// <summary>
        /// The current line the lexer is on
        /// </summary>
        protected Int32 Line;

        /// <summary>
        /// The number of lines in the program
        /// </summary>
        protected Int32 Lines;

        /// <summary>
        /// A list of all the tokens the lexer has produced
        /// </summary>
        protected List<Token> Tokens;

        /// <summary>
        /// Storage for the current token the lexer is producing
        /// </summary>
        protected Token CurrentToken;

        /// <summary>
        /// Moves onto the next token
        /// </summary>
        protected void NextToken()
        {
            TokenIndex++;
        }

        /// <summary>
        /// Displays an error on screen when something isn't correct
        /// </summary>
        /// <param name="message">The base message to produce</param>
        /// <param name="lineStart">The line the error occured on</param>
        /// <param name="column">The number of characters across the line the error occurred</param>
        public static void Error(String message, Int32 lineStart = -1, Int32 column = -1)
        {
            String errorMessage = message;

            if (lineStart != -1)
            {
                message += " Line: " + lineStart;
            }

            if (column != -1)
            {
                message += " Col: " + column;
            }

            Console.WriteLine(errorMessage);
        }

        /// <summary>
        /// Skips a block { ... } of statements
        /// </summary>
        protected void SkipBlock()
        {
            //Record the number of started and unclosed blocks
            Int32 blockNumber = 0;

            while (CurrentToken.Value != "{")
            {
                if (Line >= Lines || CurrentToken.Value.Equals("}"))
                {
                    Error("Invalid file ending: expected '{'", CurrentToken.LineStart);
                }
                NextToken();
            }
            NextToken();

            blockNumber++;

            while (blockNumber != 0)
            {
                if (CurrentToken.Value.Equals("{"))
                {
                    blockNumber++;
                }

                if (CurrentToken.Value.Equals("}"))
                {
                    blockNumber++;
                }

                if (Line >= Lines)
                    Error("Invalid file ending: expected '}'", CurrentToken.LineStart);

                NextToken();
            }
        }

        /// <summary>
        /// Check if a function declaration is incoming
        /// </summary>
        /// <returns></returns>
        public Boolean CheckFunctionIncoming()
        {
            //Starting with a type can mean a function
            if (!Database.CheckType(CurrentToken.Value))
                return false;

            //Record the index of the present token so we look forward and come back again
            Int32 currentIndex = TokenIndex;

            NextToken();

            //Skip the *s
            //while (CurrentToken.Value.Equals("*"))
                //NextToken();

            NextToken();

            //A '(' after a type name mean's the function's parameter list is incoming, so this is a function
            //declaration
            //Reset the tokenIndex
            TokenIndex = currentIndex;
            return CurrentToken.Value.Equals("(");
        }

        public Int32 ParseType()
        {
            return -1;
        }

        public void ParseDeclaration(ref VariableInfo var)
        {
            if (!Database.CheckType(CurrentToken.Value))
                Error("Invalid type", CurrentToken.LineStart, CurrentToken.CharacterPosition);

            Int32 type = ParseType();

            //Check for validity in name
            if (!CurrentToken.Type.Equals(TokenType.Identifier))
                Error("Invalid Name: " + CurrentToken.Value, CurrentToken.LineStart, CurrentToken.CharacterPosition);

            if (!Database.CheckValidName(CurrentToken.Value))
                Error("Invalid Name, name already used: " + CurrentToken.Value,
                      CurrentToken.LineStart,
                      CurrentToken.CharacterPosition);

            String name = CurrentToken.Value;

            NextToken();

            var.Name = name;
            var.Type = type;
     
        }
    }
}
