using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using esper_compiler.src.datatypes;

namespace esper_compiler.src
{
    /// <summary>
    /// Clears up the tokens once the lexer has read them
    /// </summary>
    class Postlexer : TokenParser
    {
        public Postlexer()
        {
            ResetToken();
        }

        public void OrganiseTokens()
        {
            RemoveEndLineTokens();
            RetrieveAssignTokens();
        }

        /// <summary>
        /// Esper allows the user to use two methods of assignment
        /// a = 3
        /// a <~ 3                      
        /// This converts <~ into =
        /// </summary>
        private void RetrieveAssignTokens()
        {
            for (int tokenIndex = 0; tokenIndex < Tokens.Count - 1; tokenIndex++)
            {
                if (Tokens[tokenIndex].Value == "<")
                    if (Tokens[tokenIndex + 1].Value == "~")
                    {
                        Tokens[tokenIndex].Value = "=";
                        Tokens.RemoveAt(tokenIndex + 1);
                        tokenIndex--;
                    }
            }
        }

        /// <summary>
        /// Since Esper uses semicolons in the same way C style languages use them
        /// New lines only affect the lexical analysis of strings
        /// So after the lexical analysis stage, EOL tokens can be removed
        /// </summary>
        private void RemoveEndLineTokens()
        {
            for (int tokenIndex = 0; tokenIndex < Tokens.Count; tokenIndex++)
                if (Tokens[tokenIndex].Type.Equals(TokenType.EOL))
                    Tokens.RemoveAt(tokenIndex--);
        }
    }
}
