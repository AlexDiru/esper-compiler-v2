using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src
{
    enum TokenType
    {
        Identifier,
        Number,
        String,
        Character,
        Symbol,
        Unknown,
        EOL
    }

    class Token
    {
        /// <summary>
        /// The type of the token
        /// </summary>
        public TokenType Type;

        /// <summary>
        /// The value the token holds
        /// </summary>
        public String Value;

        /// <summary>
        /// The line that the token first appears on
        /// </summary>
        public Int32 LineStart;

        /// <summary>
        /// The column the token first appears on
        /// </summary>
        public Int32 CharacterPosition;

        /// <summary>
        /// Set default values for the token
        /// </summary>
        public Token()
        {
            Type = TokenType.Unknown;
            Value = String.Empty;
            LineStart = -1;
            CharacterPosition = -1;
        }
    }
}
