using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src
{
    /// <summary>
    /// Used before the parser, to build a database of all types, global variables and functions
    /// </summary>
    class Preparser
    {

        //Function syntax
        //FUNCTION ::= 'Function', TYPE, IDENTIFIER, '(', PARAMETER_LIST, ')' '{' STATEMENT_LIST '}'
        //PARAMETER_LIST ::= PARAMETER | PARAMETER ',' PARAMETER_LIST
        //PARAMETER ::= TYPE IDENTIFIER
    }
}
