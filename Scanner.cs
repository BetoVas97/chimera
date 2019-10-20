/*
  Chimera compiler - This class performs the lexical analysis, 
  (a.k.a. scanning).
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Chimera {

    class Scanner {

        readonly string input;

        static readonly Regex regex = new Regex(
            @"                             
                (?<PosibleKeyword>      [a-zA-Z]+               )
              | (?<Assign>              [=]                     )
              | (?<ColonEqual>          [:][=]                  )
              | (?<CommentLine>         [/][/]                  )
              | (?<CommentBlockOpen>    [/][*]                  )
              | (?<CommentBlockClose>   [*][/]                  )
              | (?<IntLiteral>          \d+                     )
              | (?<LessEqual>           [<][=]                  )
              | (?<LessMore>            [<][>]                  )
              | (?<MoreEqual>           [>][=]                  )              
              | (?<Newline>             \n                      )
              | (?<BraceOpen>           [{]                     )
              | (?<BraceClose>          [}]                     )
              | (?<BracketOpen>         [[]                     )
              | (?<BracketClose>        []]                     )
              | (?<Colon>               [:]                     ) 
              | (?<Coma>                [,]                     ) 
              | (?<ParenthesisOpen>     [(]                     )
              | (?<ParenthesisClose>    [)]                     )
              | (?<Div>                 [/]                     ) 
              | (?<Identifier>          [A-Za-z][A-Za-z0-9_]*   ) 
              | (?<Less>                [<]                     ) 
              | (?<Minus>               [-]                     ) 
              | (?<More>                [>]                     )
              | (?<Mul>                 [*]                     )
              | (?<Plus>                [+]                     )
              | (?<Semicolon>           [;]                     )
              | (?<StringLiteral>       [""]                    )           
              | (?<WhiteSpace>          \s                      )     # Must go anywhere after Newline.
              | (?<Other>               .                       )     # Must be last: match any other character.
            ", 
            RegexOptions.IgnorePatternWhitespace 
                | RegexOptions.Compiled
                | RegexOptions.Multiline
            );

        static readonly IDictionary<string, TokenCategory> keywords =
            new Dictionary<string, TokenCategory>() {
                {"and", TokenCategory.AND},
                {"begin", TokenCategory.BEGIN},
                {"bool", TokenCategory.BOOL},
                {"const", TokenCategory.CONST},
                {"do", TokenCategory.DO},
                {"else", TokenCategory.ELSE},
                {"elseif", TokenCategory.ELSEIF},
                {"end", TokenCategory.END},
                {"exit", TokenCategory.EXIT},
                {"false", TokenCategory.FALSE},
                {"for", TokenCategory.FOR},
                {"if", TokenCategory.IF},
                {"in", TokenCategory.IN},
                {"integer", TokenCategory.INTEGER},
                {"list", TokenCategory.LIST},
                {"loop", TokenCategory.LOOP},
                {"not", TokenCategory.NOT},
                {"of", TokenCategory.OF},
                {"or", TokenCategory.OR},
                {"print", TokenCategory.PRINT},
                {"procedure", TokenCategory.PROCEDURE},
                {"program", TokenCategory.PROGRAM},
                {"rem", TokenCategory.REM},
                {"return", TokenCategory.RETURN},
                {"string", TokenCategory.STRING},
                {"then", TokenCategory.THEN},
                {"true", TokenCategory.TRUE},
                {"var", TokenCategory.VAR},
                
                {"WrInt", TokenCategory.WR_INT},
                {"WrStr", TokenCategory.WR_STR},
                {"WrBool", TokenCategory.WR_BOOL},
                {"WrLn", TokenCategory.WR_LN},
                {"RdInt", TokenCategory.RD_INT},
                {"RdStr", TokenCategory.RD_STR},
                {"AtStr", TokenCategory.AT_STR},
                {"LenStr", TokenCategory.LEN_STR},
                {"CmpStr", TokenCategory.CMP_STR},
                {"CatStr", TokenCategory.CAT_STR},
                {"LenLstInt", TokenCategory.LEN_LST_INT},
                {"LenLstStr", TokenCategory.LEN_LST_STR},
                {"LenLstBool", TokenCategory.LEN_LST_BOOL},
                {"NewLstInt", TokenCategory.NEW_LST_INT},
                {"NewLstStr", TokenCategory.NEW_LST_STR},
                {"NewLstBool", TokenCategory.NEW_LST_BOOL},
                {"IntToStr", TokenCategory.INT_TO_STR},
                {"StrToInt", TokenCategory.STR_TO_INT},

                {"xor", TokenCategory.XOR},
            };

        static readonly IDictionary<string, TokenCategory> nonKeywords =
            new Dictionary<string, TokenCategory>() {
                {"Assign", TokenCategory.ASSIGN},
                {"BraceOpen", TokenCategory.BRACE_OPEN},
                {"BraceClose", TokenCategory.BRACE_CLOSE},
                {"BracketOpen", TokenCategory.BRACKET_OPEN},
                {"BracketClose", TokenCategory.BRACKET_CLOSE},
                {"CommentLine", TokenCategory.COMMENT_LINE},
                {"CommentBlockOpen", TokenCategory.COMMENT_BLOCK_OPEN},
                {"CommentBlockClose", TokenCategory.COMMENT_BLOCK_CLOSE},
                {"Colon", TokenCategory.COLON},
                {"ColonEqual", TokenCategory.COLON_EQUAL},
                {"Coma", TokenCategory.COMA},
                {"Div", TokenCategory.DIV},
                {"Identifier", TokenCategory.IDENTIFIER},
                {"IntLiteral", TokenCategory.INT_LITERAL},
                {"Less", TokenCategory.LESS},
                {"LessEqual", TokenCategory.LESS_EQUAL},
                {"LessMore", TokenCategory.LESS_MORE},
                {"Minus", TokenCategory.MINUS},
                {"More", TokenCategory.MORE},
                {"MoreEqual", TokenCategory.COMMENT_LINE},
                {"Mul", TokenCategory.MUL},
                {"ParenthesisOpen", TokenCategory.PARENTHESIS_OPEN},
                {"ParenthesisClose", TokenCategory.PARENTHESIS_CLOSE},
                {"Plus", TokenCategory.PLUS},
                {"Semicolon", TokenCategory.SEMICOLON},
                {"StringLiteral", TokenCategory.STRING_LITERAL},
            };

        public Scanner(string input) {
            this.input = input;
        }

        public IEnumerable<Token> Start() {

            var row = 1;
            var columnStart = 0;
            var state = State.NORMAL;
            var concatenatedString = "";
            var commentStringIndex = 0;
            var commentStringRow = 0;
            var commentStringColumnStart = 0;

            Func<Match, TokenCategory, Token> newTok = (m, tc) =>
                new Token(m.Value, tc, row, m.Index - columnStart + 1);
            
            Func<string, TokenCategory, int, Token> newTokenCommentString = (lexeme, category, index) => {
                var newToken = new Token(lexeme, category, commentStringRow, index - commentStringColumnStart + 1);
                concatenatedString="";
                return newToken;
            };
                

            foreach (Match m in regex.Matches(input)) {
                if(state == State.NORMAL){

                    if (m.Groups["Newline"].Success) {

                        // Found a new line.
                        row++;
                        columnStart = m.Index + m.Length;

                    } else if (m.Groups["WhiteSpace"].Success ) {

                        // Skip white space.

                    } else if (m.Groups["PosibleKeyword"].Success) {

                        if (keywords.ContainsKey(m.Value)) {

                            // Matched string is a Chimera keyword.
                            yield return newTok(m, keywords[m.Value]);                                               

                        } else { 

                            // Otherwise it's just a plain identifier.
                            yield return newTok(m, TokenCategory.IDENTIFIER);
                        }

                    } else if (m.Groups["Other"].Success) {

                        // Found an illegal character.
                        yield return newTok(m, TokenCategory.ILLEGAL_TOKEN);

                    } else if (m.Groups["CommentLine"].Success) {

                        // Found start of a line comment
                        state = State.READING_COMMENT_LINE;
                        commentStringIndex = m.Index;
                        commentStringRow = row;
                        commentStringColumnStart = columnStart;
                        continue;

                    } else if (m.Groups["CommentBlockOpen"].Success) {

                        // Found start of a block comment
                        state = State.READING_COMMENT_BLOCK;
                        commentStringIndex = m.Index;
                        commentStringRow = row;
                        commentStringColumnStart = columnStart;

                        continue;

                    } else if (m.Groups["StringLiteral"].Success) {

                        // Found start of a string
                        state = State.READING_STRING_FIRST_QUOTE;
                        commentStringIndex = m.Index;
                        commentStringRow = row;
                        commentStringColumnStart = columnStart;
                        continue;

                    } else {

                        // Match must be one of the non keywords.
                        foreach (var name in nonKeywords.Keys) {
                            if (m.Groups[name].Success) {
                                yield return newTok(m, nonKeywords[name]);
                                break;
                            }
                        }
                    }
                
                } else if(state == State.READING_COMMENT_LINE){
                    if (m.Groups["Newline"].Success) {

                        // Found a new line.
                        columnStart = m.Index + m.Length;
                        state = State.NORMAL;
                        var newToken = newTokenCommentString(concatenatedString, TokenCategory.COMMENT_LINE, commentStringIndex);
                        commentStringIndex = 0;
                        row ++;
                        continue;
                    } else {
                        concatenatedString+=m.Value;
                        continue;
                    }

                } else if(state == State.READING_COMMENT_BLOCK){
                    if (m.Groups["CommentBlockClose"].Success) {


                        state = State.NORMAL;
                        var newToken = newTokenCommentString(concatenatedString, TokenCategory.COMMENT_BLOCK, commentStringIndex);
                        commentStringIndex = 0;
                        continue;
                    } else {
                        if (m.Groups["Newline"].Success) {

                            // Found a new line.

                            row++;
                            columnStart = m.Index + m.Length;

                        }
                        concatenatedString+=m.Value;
                        continue;
                    }
                } else if(state == State.READING_STRING_FIRST_QUOTE){
                    if (m.Groups["Newline"].Success) {

                        // Found a new line.
                        columnStart = m.Index + m.Length;
                        state = State.NORMAL;
                        var newToken = newTokenCommentString(concatenatedString, TokenCategory.ILLEGAL_TOKEN, commentStringIndex);
                        commentStringIndex = 0;

                        row ++;
                        yield return newToken;
                    } else if (m.Groups["StringLiteral"].Success) {

                        // Found the second double quote
                        state = State.READING_STRING_SECOND_QUOTE;
                        continue;
                    }else{
                        concatenatedString+=m.Value;
                        continue;
                    }
                } else{  //Case State.READING_STRING_SECOND_QUOTE
                    if (m.Groups["StringLiteral"].Success) {

                        // Found another double quoted
                        state = State.READING_STRING_FIRST_QUOTE;
                        concatenatedString+= "\"";
                        continue;
                    } else{
                        var newToken = newTokenCommentString(concatenatedString, TokenCategory.STRING_LITERAL, commentStringIndex);
                        commentStringIndex = 0;

                        if (m.Groups["Newline"].Success) {

                            // Found a new line.
                            row++;
                            columnStart = m.Index + m.Length;

                        }
                        state = State.NORMAL;
                        yield return newToken;

                    }
                }
            }
            
            if(state != State.NORMAL){
                var category = TokenCategory.ILLEGAL_TOKEN;
                if(state == State.READING_COMMENT_LINE){
                    category = TokenCategory.COMMENT_LINE;
                } 
                yield return newTokenCommentString(concatenatedString, category, commentStringIndex);
            }

            yield return new Token(null, 
                                        TokenCategory.EOF, 
                                        row, 
                                        input.Length - columnStart + 1);
        }
    }
}