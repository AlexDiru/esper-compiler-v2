using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler.src
{
    static class Error
    {
        public static void Write(String message, Int32 lineStart, Int32 column)
        {
            Console.WriteLine(message + " Line: " + lineStart + " Col: " + column + " }");
        }
    }
}
