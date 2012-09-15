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
        public TokenType Type;
        public String Value;
        public Int32 LineStart;
        public Int32 CharacterPosition;
    }
}
