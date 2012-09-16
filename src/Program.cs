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
            Console.Write("Lexer\n\n");

            Lexer lexer = new Lexer();
            lexer.OpenProgram("../../esper-code/functions.esp");
            Console.WriteLine(lexer.Program);
            lexer.PrepareTokensList();
            lexer.PrintTokens();

            Console.Write("\n\nPostlexer\n\n");

            Postlexer postlexer = new Postlexer();
            postlexer.OrganiseTokens();
            postlexer.PrintTokens();

            Database.Setup();

            Preparser preparser = new Preparser();
            preparser.Preparse();
            preparser.PrintOut();
        }
    }
}
