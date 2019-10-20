/*
  Chimera compiler - Token categories for the scanner.
  Copyright (C) 2013 Ariel Ortiz, ITESM CEM
  
  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.
  
  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.
  
  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace Chimera {

    enum TokenCategory {
        AND, //KEY
        ASSIGN, // =
        BEGIN, //KEY
        BOOL, //KEY
        BRACE_OPEN, //{
        BRACE_CLOSE, //}
        BRACKET_OPEN, //[
        BRACKET_CLOSE, //]
        COMMENT_LINE, // //
        COMMENT_BLOCK,
        COMMENT_BLOCK_OPEN, // /*
        COMMENT_BLOCK_CLOSE, // */
        CONST, //KEY
        COLON, // :
        COLON_EQUAL, // :=
        COMA, // ,
        DIV, // /
        DO, //KEY
        ELSE, //KEY
        ELSEIF, //KEY
        END, //KEY
        EOF,
        EXIT, //KEY
        FALSE, //KEY
        FOR, //KEY
        IDENTIFIER,
        IF, //KEY
        IN, //KEY
        INTEGER, //KEY
        INT_LITERAL,
        LESS, // <
        LESS_EQUAL, // <=
        LESS_MORE, //<>
        LIST, //KEY
        LOOP, //KEY
        MINUS, // -
        MORE, // >
        MORE_EQUAL, // >=
        MUL, // *
        NOT, //KEY
        OF, //KEY
        OR, //KEY
        PARENTHESIS_OPEN, // (
        PARENTHESIS_CLOSE, // )
        PLUS, // +
        PRINT,//KEY
        PROCEDURE,//KEY
        PROGRAM,//KEY
        REM,//KEY
        RETURN,//KEY
        SEMICOLON, // ;
        STRING,//KEY
        STRING_LITERAL,
        THEN,//KEY
        TRUE,//KEY
        VAR,//KEY

        WR_INT, //KEY
        WR_STR, //KEY
        WR_BOOL, //KEY
        WR_LN, //KEY
        RD_INT, //KEY
        RD_STR, //KEY
        AT_STR, //KEY
        LEN_STR, //KEY
        CMP_STR, //KEY
        CAT_STR, //KEY
        LEN_LST_INT, //KEY
        LEN_LST_STR, //KEY
        LEN_LST_BOOL, //KEY
        NEW_LST_INT, //KEY
        NEW_LST_STR, //KEY
        NEW_LST_BOOL, //KEY
        INT_TO_STR, //KEY
        STR_TO_INT, //KEY

        XOR,//KEY

        ILLEGAL_TOKEN
    }
}

