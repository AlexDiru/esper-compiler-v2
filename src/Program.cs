using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src
{
    class Program
    {
        static void Main(string[] args)
        {

            Lexer lexer = new Lexer();
            lexer.OpenProgram("../../esper-code/boolean-algebra.esp");
            Console.WriteLine(lexer.Program);
            lexer.PrepareTokensList();
            lexer.PrintTokens();
        }
    }
}
