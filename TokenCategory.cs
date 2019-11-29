/*
  Chimera compiler - Token categories for the scanner.
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358 
*/

namespace Chimera {

    enum TokenCategory {
        AND, //KEY
        ASSIGN, // :=
        BEGIN, //KEY
        BOOLEAN, //KEY
        BRACE_OPEN, //{
        BRACE_CLOSE, //}
        BRACKET_OPEN, //[
        BRACKET_CLOSE, //]
        CONST, //KEY
        COLON, // :
        COMMA, // ,
        COMMENT_LINE,
        DIV, // /
        DO, //KEY
        ELSE, //KEY
        ELSEIF, //KEY
        END, //KEY
        EOF,
        EQUALITY, //=
        EXIT, //KEY
        FALSE, //KEY
        FOR, //KEY
        IDENTIFIER,
        IF, //KEY
        IN, //KEY
        INEQUALITY, //<>
        INTEGER, //KEY
        INT_LITERAL,
        LESS, // <
        LESS_EQUAL, // <=
        LESS_MORE, //<>
        LIST, //KEY
        LIST_INDEX,
        LOOP, //KEY
        MINUS, // -
        MORE, // >
        MORE_EQUAL, // >=
        MULT, // *
        NOT, //KEY
        NEGATION, 
        OF, //KEY
        OR, //KEY
        PARENTHESIS_OPEN, // (
        PARENTHESIS_CLOSE, // )
        SUM, // +
        PRINT,//KEY
        PROCEDURE,//KEY
        PROGRAM,//KEY
        REM,//KEY
        RETURN,//KEY
        SEMICOLON, // ;
        STRING,//KEY
        STR_LITERAL,
        THEN,//KEY
        TRUE,//KEY
        VAR,//KEY
        XOR,//KEY
        ILLEGAL_TOKEN
    }
}
