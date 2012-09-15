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

    class Lexer
    {
        public String Program;
        Int32 CharacterPosition;
        Int32 LineStart;
        Int32 Line;
        Int32 Lines;
        List<Token> Tokens;
        Token CurrentToken;

        public Lexer()
        {
        }

        /// <summary>
        /// Opens the code file and stores in the variable Program
        /// </summary>
        public void OpenProgram(String fileName)
        {
            System.IO.TextReader textReader = new System.IO.StreamReader(fileName);
            String inputLine = String.Empty;
            Program = String.Empty;
            Lines = 0;

            while ((inputLine = textReader.ReadLine()) != null)
            {
                Lines++;
                Program += inputLine + "\n";
            }

            textReader.Close();
        }

        /// <summary>
        /// Skip spaces, tabs and comments the parser won't need
        /// </summary>
        public void SkipWhitespace()
        {
            //Skip space
            while (" \t".Contains(Program[CharacterPosition]))
            {
                if (Program[CharacterPosition].Equals('\n'))
                {
                    break;
                }
                CharacterPosition++;
            }

            //Skip comments
            if (Program[CharacterPosition].Equals('/') && Program[CharacterPosition+1].Equals('/'))
            {
                while (Program[CharacterPosition] != '\n')
                    CharacterPosition++;
            }
        }

        /// <summary>
        /// Gets the identifier type of token
        /// </summary>
        TokenType GetIdentifier()
        {
            do
            {
                CurrentToken.Value += char.ToUpper(Program[CharacterPosition]);
                CharacterPosition++;
            }
            while (char.IsLetterOrDigit(Program[CharacterPosition]) || Program[CharacterPosition].Equals('_'));

            CurrentToken.Type = TokenType.Identifier;
            return TokenType.Identifier;
        }

        /// <summary>
        /// Gets the number type of token
        /// </summary>
        /// <returns></returns>
        TokenType GetNumber()
        {
            bool hasDecimalPlace = false;

            do
            {
                if (Program[CharacterPosition].Equals('.'))
                    hasDecimalPlace = true;

                CurrentToken.Value += Program[CharacterPosition];
                CharacterPosition++;
            }
            while (char.IsDigit(Program[CharacterPosition]) ||
                   (Program[CharacterPosition].Equals('.') && !hasDecimalPlace));

            CurrentToken.Type = TokenType.Number;
            return TokenType.Number;
        }

        /// <summary>
        /// Gets the string type of token
        /// </summary>
        TokenType GetString()
        {
            //Skip quote
            CharacterPosition++;

            do
            {
                if (Program[CharacterPosition].Equals('\n'))
                {
                    Error.Write("Expected ending quotation mark in line ", LineStart, CharacterPosition - LineStart + 1);
                }

                CurrentToken.Value += Program[CharacterPosition];
                CharacterPosition++;
            } while (Program[CharacterPosition] != '\"');

            CharacterPosition++;
            CurrentToken.Type = TokenType.String;
            return TokenType.String;
        }

        TokenType GetCharacter()
        {
            CharacterPosition++;

            if (Program[CharacterPosition].Equals('\n') || Program[CharacterPosition].Equals('\''))
            {
                Error.Write("Expected a character between", LineStart, CharacterPosition - LineStart + 1);
            }

            CurrentToken.Value += Program[CharacterPosition];
            CharacterPosition++;

            if (!Program[CharacterPosition].Equals('\''))
            {
                Error.Write("Expected ending single quote mark", LineStart, CharacterPosition - LineStart + 1);
            }

            CurrentToken.Type = TokenType.Character;
            return TokenType.Character;
        }

        TokenType GetSymbol()
        {
            CurrentToken.Value += Program[CharacterPosition];

            if ("<>!=".Contains(Program[CharacterPosition]) && Program[CharacterPosition + 1].Equals('='))
            {
                CharacterPosition++;
                CurrentToken.Value += Program[CharacterPosition];
            }

            CharacterPosition++;
            CurrentToken.Type = TokenType.Symbol;
            return TokenType.Symbol;
        }

        TokenType GetNextToken()
        {
            CurrentToken = new Token();

            SkipWhitespace();
            CurrentToken.LineStart = LineStart;
            CurrentToken.CharacterPosition = CharacterPosition - LineStart + 1;

            if (Program[CharacterPosition].Equals('\n'))
            {
                CurrentToken.Type = TokenType.EOL;
                return TokenType.EOL;
            }

            if ((char.IsLetterOrDigit(Program[CharacterPosition])) ||
                (Program[CharacterPosition].Equals('_') && char.IsLetterOrDigit(Program[CharacterPosition + 1])))
                return GetIdentifier();

            if ((char.IsDigit(Program[CharacterPosition])) ||
                (Program[CharacterPosition].Equals('.') && char.IsDigit(Program[CharacterPosition + 1])))
                return GetNumber();

            if (Program[CharacterPosition].Equals('\"'))
                return GetString();

            if ("!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~".Contains(Program[CharacterPosition]))
                return GetSymbol();

            Error.Write("Unrecognised Token", LineStart, CharacterPosition - LineStart + 1);
            return TokenType.Unknown;
        }

        public void PrepareTokensList()
        {
            Tokens = new List<Token>();

            CharacterPosition = Line = LineStart = 0;

            while (Line < Lines)
            {
                GetNextToken();
                if (CurrentToken.Type == TokenType.EOL)
                {
                    LineStart = CharacterPosition + 1;
                    Line++;
                    CharacterPosition++;
                }
                Tokens.Add(CurrentToken);
            }
        }

        public void PrintTokens()
        {
            foreach (Token token in Tokens)
            {
                Console.Write("(" + token.Value + ", " + token.Type.ToString() + ") ");
            }
        }
    }
}
