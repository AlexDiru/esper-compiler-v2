using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using esper_compiler.src.datatypes;

namespace esper_compiler.src
{
    class Lexer : TokenParser
    {
        /// <summary>
        /// The code of the program
        /// </summary>
        public String Program;

        /// <summary>
        /// The character position being read in the program string
        /// </summary>
        Int32 CharacterPosition;

        /// <summary>
        /// The line in which the token starts
        /// </summary>
        Int32 LineStart;

        

        /// <summary>
        /// Opens the code file and stores in the variable Program
        /// </summary>
        /// <param name="fileName">The file to read the code from</param>
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
            //Skip space and/or tabs
            while (" \t".Contains(Program[CharacterPosition]))
            {
                //New line encountered, stop skipping
                if (Program[CharacterPosition].Equals('\n'))
                {
                    break;
                }

                CharacterPosition++;
            }

            //Skip comments
            if (Program[CharacterPosition].Equals('/') && Program[CharacterPosition+1].Equals('/'))
            {
                //New line encountered, end of comment
                while (Program[CharacterPosition] != '\n')
                    CharacterPosition++;
            }
        }

        /// <summary>
        /// Gets the identifier type of token
        /// </summary>
        TokenType GetIdentifier()
        {
            //While the character is alphanumeric or an underscore, append it to the identifier's name
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
        TokenType GetNumber()
        {
            //Whether the number being read is a decimal or an integer
            bool hasDecimalPlace = false;

            //While a digit being read or first (and only) decimal place being read, add the number to the token
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

            //While end quote not found, keep adding to string contents
            do
            {
                if (Program[CharacterPosition].Equals('\n'))
                {
                    Error("Expected ending quotation mark in line ", LineStart, CharacterPosition - LineStart + 1);
                }

                CurrentToken.Value += Program[CharacterPosition];
                CharacterPosition++;
            } 
            while (Program[CharacterPosition] != '\"');

            CharacterPosition++;
            CurrentToken.Type = TokenType.String;
            return TokenType.String;
        }

        /// <summary>
        /// Gets the character type of token
        /// </summary>
        TokenType GetCharacter()
        {
            CharacterPosition++;

            if (Program[CharacterPosition].Equals('\n') || Program[CharacterPosition].Equals('\''))
            {
                Error("Expected a character between", LineStart, CharacterPosition - LineStart + 1);
            }

            //Get the character in between the single quotes
            CurrentToken.Value += Program[CharacterPosition];
            CharacterPosition++;

            if (!Program[CharacterPosition].Equals('\''))
            {
                Error("Expected ending single quote mark", LineStart, CharacterPosition - LineStart + 1);
            }

            CurrentToken.Type = TokenType.Character;
            return TokenType.Character;
        }

        /// <summary>
        /// Gets the symbol type of token
        /// </summary>
        TokenType GetSymbol()
        {
            CurrentToken.Value += Program[CharacterPosition];

            //If <= or >= or != or == then read an extra character
            if ("<>!=".Contains(Program[CharacterPosition]) && Program[CharacterPosition + 1].Equals('='))
            {
                CharacterPosition++;
                CurrentToken.Value += Program[CharacterPosition];
            }

            CharacterPosition++;
            CurrentToken.Type = TokenType.Symbol;
            return TokenType.Symbol;
        }

        /// <summary>
        /// Reads the next token in the program
        /// </summary>
        TokenType GetNextToken()
        {
            CurrentToken = new Token();

            SkipWhitespace();

            //Setup the base of the token
            CurrentToken.LineStart = LineStart;
            CurrentToken.CharacterPosition = CharacterPosition - LineStart + 1;

            //EOL token
            if (Program[CharacterPosition].Equals('\n'))
            {
                CurrentToken.Type = TokenType.EOL;
                return TokenType.EOL;
            }

            //Identifier token
            if ((char.IsLetterOrDigit(Program[CharacterPosition])) ||
                (Program[CharacterPosition].Equals('_') && char.IsLetterOrDigit(Program[CharacterPosition + 1])))
                return GetIdentifier();

            //Number token
            if ((char.IsDigit(Program[CharacterPosition])) ||
                (Program[CharacterPosition].Equals('.') && char.IsDigit(Program[CharacterPosition + 1])))
                return GetNumber();

            //Character token
            if (Program[CharacterPosition].Equals('\"'))
                return GetString();

            //Symbol token
            if ("!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~".Contains(Program[CharacterPosition]))
                return GetSymbol();

            Error("Unrecognised Token", LineStart, CharacterPosition - LineStart + 1);
            return TokenType.Unknown;
        }

        /// <summary>
        /// Translates the code into tokens
        /// </summary>
        public void PrepareTokensList()
        {
            Tokens = new List<Token>();

            CharacterPosition = Line = LineStart = 0;

            while (Line < Lines)
            {
                GetNextToken();
                if (CurrentToken.Type == TokenType.EOL)
                {
                    LineStart = Line;
                    Line++;
                    CharacterPosition++;
                }
                Tokens.Add(CurrentToken);
            }

        }
    }
}
