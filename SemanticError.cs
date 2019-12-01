/*
  Chimera compiler - SemanticError.
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358
*/

using System;

namespace Chimera{

    class SemanticError : Exception
    {
        public SemanticError(string message, Token token) :
            base(String.Format(
                "Semantic Error: {0} \n" +
                "at row {1}, column {2}.",
                message,
                token.Row,
                token.Column))
        {
        }
    }
}