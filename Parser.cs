/*
  Chimera compiler - This class performs the syntactic analysis,
  (a.k.a. parsing).
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358 
*/

using System;
using System.Collections.Generic;

namespace Chimera{

    class Parser {

        static readonly ISet<TokenCategory> firstOfDeclaration =
            new HashSet<TokenCategory>() {
                TokenCategory.VAR,
                TokenCategory.CONST,
                TokenCategory.PROCEDURE
            };

        static readonly ISet<TokenCategory> firstOfSimpleLiteral =
            new HashSet<TokenCategory>() {
                TokenCategory.INT_LITERAL,
                TokenCategory.STR_LITERAL,
                TokenCategory.TRUE,
                TokenCategory.FALSE,
            };

        static readonly ISet<TokenCategory> firstOfSimpleType =
            new HashSet<TokenCategory>() {
                TokenCategory.INTEGER,
                TokenCategory.STRING,
                TokenCategory.BOOLEAN
    };


        static readonly ISet<TokenCategory> firstOfStatement =
            new HashSet<TokenCategory>() {
                TokenCategory.IDENTIFIER,
                TokenCategory.IF,
                TokenCategory.LOOP,
                TokenCategory.FOR,
                TokenCategory.RETURN,
                TokenCategory.EXIT,
            };

        static readonly ISet<TokenCategory> firstOfLogicOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.AND,
                TokenCategory.OR,
                TokenCategory.XOR
            };

        static readonly ISet<TokenCategory> firstOfRelationalOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.EQUALITY,
                TokenCategory.INEQUALITY,
                TokenCategory.LESS,
                TokenCategory.LESS_EQUAL,
                TokenCategory.MORE,
                TokenCategory.MORE_EQUAL
            };

        static readonly ISet<TokenCategory> firstOfSumOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.SUM,
                TokenCategory.MINUS
            };

        static readonly ISet<TokenCategory> firstOfMultOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.MULT,
                TokenCategory.DIV,
                TokenCategory.REM
            };

        static readonly ISet<TokenCategory> firstOfUnaryOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.NEGATION,
                TokenCategory.NOT
            };

        static readonly ISet<TokenCategory> firstOfSimpleExpression =
            new HashSet<TokenCategory>() {
                TokenCategory.PARENTHESIS_OPEN,
                TokenCategory.IDENTIFIER,
                TokenCategory.INT_LITERAL,
                TokenCategory.STR_LITERAL,
                TokenCategory.TRUE,
                TokenCategory.FALSE,
                TokenCategory.BRACE_OPEN
            };

        IEnumerator<Token> tokenStream;

        public Parser(IEnumerator<Token> tokenStream) {
            this.tokenStream = tokenStream;
            this.tokenStream.MoveNext();
        }

        public TokenCategory CurrentToken {
            get { return tokenStream.Current.Category; }
        }

        public Token Expect(TokenCategory category) {
            if (CurrentToken == category) {
                Token current = tokenStream.Current;
                tokenStream.MoveNext();
                return current;
            } else {
                throw new SyntaxError(category, tokenStream.Current);
            }
        }

        public Node Program() {
            var constDecList = new ConstDeclarationList();
            var varDecList = new VarDeclarationList();
            var procedureDecList = new ProcedureDeclarationList();
            var stmlist = new StatementList();

            if (firstOfDeclaration.Contains(CurrentToken) && CurrentToken == TokenCategory.CONST)
            {
                constDecList.AnchorToken = Expect(TokenCategory.CONST);

                do
                {
                    constDecList.Add(ConstDeclaration());
                } while (CurrentToken == TokenCategory.IDENTIFIER);
            }

            if (firstOfDeclaration.Contains(CurrentToken) && CurrentToken == TokenCategory.VAR)
            {
                varDecList.AnchorToken = Expect(TokenCategory.VAR);

                do
                {
                    varDecList.Add(VarDeclaration());
                } while (CurrentToken == TokenCategory.IDENTIFIER);
            }

            while (firstOfDeclaration.Contains(CurrentToken) && CurrentToken == TokenCategory.PROCEDURE){
                procedureDecList.Add(ProcedureDeclaration());
            }

            Expect(TokenCategory.PROGRAM);

            while (firstOfStatement.Contains(CurrentToken)){
                stmlist.Add(Statement());
            }

            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);

            return new Program(){
                constDecList,
                varDecList,
                procedureDecList,
                stmlist
            };
        }

        public Node ConstDeclaration()
        {
            var result = new ConstDeclaration()
            {
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            };

            Expect(TokenCategory.ASSIGN);

            if (CurrentToken != TokenCategory.BRACE_OPEN)
            {
                result.Add(SimpleLiteral());
            }

            else if (CurrentToken == TokenCategory.BRACE_OPEN)
            {
                result.Add(List());
            }

            Expect(TokenCategory.SEMICOLON);

            return result;
        }

        public Node VarDeclaration()
        {
            var result = new VarDeclarationItems();

            result.Add(new Identifier(){
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            });
        
            while (CurrentToken == TokenCategory.COMMA)
            {
                Expect(TokenCategory.COMMA);

                result.Add(new Identifier()
                {
                    AnchorToken = Expect(TokenCategory.IDENTIFIER)
                });
            }

            Expect(TokenCategory.COLON);

            var final = new VarDeclaration();


            if (CurrentToken != TokenCategory.LIST)
            {
                final.Add(SimpleType());
            }

            else if (CurrentToken == TokenCategory.LIST)
            {
                final.Add(ListType());
            }

            final.Add(result);

            Expect(TokenCategory.SEMICOLON);
            return final;
        }

        public Node ListType()
        {
            var result = new ListN()
            {
                AnchorToken = Expect(TokenCategory.LIST)
            };

            Expect(TokenCategory.OF);

            result.Add(SimpleType());

            return result;
        }

        public Node SimpleType()
        {
            switch (CurrentToken)
            {
                case TokenCategory.INTEGER:
                    return new IntegerN(){
            AnchorToken = Expect(TokenCategory.INTEGER)};

                case TokenCategory.STRING:
                    return new StringN(){
            AnchorToken = Expect(TokenCategory.STRING)};

                case TokenCategory.BOOLEAN:
                    return new BooleanN(){
            AnchorToken = Expect(TokenCategory.BOOLEAN)};

                default:
                    throw new SyntaxError(firstOfSimpleType,
                                      tokenStream.Current);
            }
        }

        public Node SimpleLiteral()
        {
            switch (CurrentToken)
            {
                case TokenCategory.INT_LITERAL:
                    return new Int_Literal()
                    {
                        AnchorToken = Expect(TokenCategory.INT_LITERAL)
                    };

                case TokenCategory.STR_LITERAL:
                    return new Str_Literal()
                    {
                        AnchorToken = Expect(TokenCategory.STR_LITERAL)
                    };

                case TokenCategory.TRUE:
                    return new True()
                    {
                        AnchorToken = Expect(TokenCategory.TRUE)
                    };

                case TokenCategory.FALSE:
                    return new False()
                    {
                        AnchorToken = Expect(TokenCategory.FALSE)
                    };

                default:
                    throw new SyntaxError(firstOfSimpleLiteral,
                                          tokenStream.Current);
            }
        }

        public Node List()
        {
            var result = new List()
            {
                AnchorToken = Expect(TokenCategory.BRACE_OPEN)
            };

            if (firstOfSimpleLiteral.Contains(CurrentToken))
            {

                result.Add(SimpleLiteral());

                while (CurrentToken == TokenCategory.COMMA)
                {
                    Expect(TokenCategory.COMMA);

                    result.Add(SimpleLiteral());
                }
                Expect(TokenCategory.BRACE_CLOSE);

                return result;
            }

            Expect(TokenCategory.BRACE_CLOSE);
            return result;
        }

        public Node ProcedureDeclaration(){
            var result = new ProcedureDeclaration()
            {
                AnchorToken = Expect(TokenCategory.PROCEDURE)
            };

            result.Add(new Identifier() { 
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            });

            Expect(TokenCategory.PARENTHESIS_OPEN);

            var parametros = new ParameterDeclarationList();
            if (CurrentToken == TokenCategory.IDENTIFIER){
                while (CurrentToken == TokenCategory.IDENTIFIER){
                    parametros.Add(VarDeclaration());
                }
            }
    
        result.Add(parametros);

            Expect(TokenCategory.PARENTHESIS_CLOSE);

            var type = new Tipo();
            if (CurrentToken == TokenCategory.COLON){
                Expect(TokenCategory.COLON);

                if (CurrentToken != TokenCategory.LIST){
                    type.Add(SimpleType());
                }

                else if (CurrentToken == TokenCategory.LIST){
                    type.Add(ListType());
                }
            }

            result.Add(type);

            Expect(TokenCategory.SEMICOLON);

            var constDecList = new ConstDeclarationList();
            if (firstOfDeclaration.Contains(CurrentToken) && CurrentToken == TokenCategory.CONST){

                constDecList.AnchorToken = Expect(TokenCategory.CONST);

                do{
                    constDecList.Add(ConstDeclaration());
                } while (CurrentToken == TokenCategory.IDENTIFIER);

            }
            result.Add(constDecList);

            var varDecList = new VarDeclarationList();
            if (firstOfDeclaration.Contains(CurrentToken) && CurrentToken == TokenCategory.VAR)
            {
                varDecList.AnchorToken = Expect(TokenCategory.VAR);

                do
                {
                    varDecList.Add(VarDeclaration());
                } while (CurrentToken == TokenCategory.IDENTIFIER);
            }
            result.Add(varDecList);

            Expect(TokenCategory.BEGIN);

            var stmlist = new StatementList();
            if (firstOfStatement.Contains(CurrentToken)){
                while (firstOfStatement.Contains(CurrentToken))
                {
                    stmlist.Add(Statement());
                }
            }
            result.Add(stmlist);


            Expect(TokenCategory.END);

            Expect(TokenCategory.SEMICOLON);

            return result;
        }

        public Node Statement(){
            switch (CurrentToken){
                case TokenCategory.IDENTIFIER:
                    var identifier = Expect(TokenCategory.IDENTIFIER);
                    if (CurrentToken == TokenCategory.PARENTHESIS_OPEN){
                        var result = CallS();
                        result.AnchorToken = identifier;
                        return result;
                    }else{
                        if (CurrentToken == TokenCategory.ASSIGN)
                        {
                            var result = Assignment();
                            result.AnchorToken = identifier;
                            return result;
                        }

                        else{
                            var result = ListIndexAssignment();
                            result.AnchorToken = identifier;

                            return result;
                        }
                    }

                case TokenCategory.IF:
                    return If();

                case TokenCategory.LOOP:
                    return Loop();

                case TokenCategory.FOR:
                    return For();

                case TokenCategory.RETURN:
                    return Return();

                case TokenCategory.EXIT:
                    return Exit();

                default:
                    throw new SyntaxError(firstOfStatement,
                                          tokenStream.Current);
            }
        }

        public Node Call(){
            var result = new Call(){
                AnchorToken = Expect(TokenCategory.PARENTHESIS_OPEN)
            };

            if (CurrentToken != TokenCategory.PARENTHESIS_CLOSE){
                result.Add(Expression());

                while (CurrentToken == TokenCategory.COMMA){
                    Expect(TokenCategory.COMMA);

                    result.Add(Expression());
                }
            }

            Expect(TokenCategory.PARENTHESIS_CLOSE);
            
            return result;
        }

        public Node CallS(){
            var result = new CallS()
            {
                AnchorToken = Expect(TokenCategory.PARENTHESIS_OPEN)
            };


            if (CurrentToken != TokenCategory.PARENTHESIS_CLOSE){
                result.Add(Expression());

                while (CurrentToken == TokenCategory.COMMA){
                    Expect(TokenCategory.COMMA);

                    result.Add(Expression());
                }
            }

            Expect(TokenCategory.PARENTHESIS_CLOSE);

            Expect(TokenCategory.SEMICOLON);

            return result;
        }

        public Node Assignment(){
            var result = new Assignment();

            Expect(TokenCategory.ASSIGN);
            result.Add(Expression());
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

        public Node ListIndexAssignment() {
            var result = new ListIndexAssignment();
            Expect(TokenCategory.BRACKET_OPEN);
            var lista = new ListIndex();
            lista.Add(Expression());
            result.Add(lista);
            Expect(TokenCategory.BRACKET_CLOSE);

            Expect(TokenCategory.ASSIGN);
            result.Add(Expression());
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

        public Node If(){
            var result = new If(){
                AnchorToken = Expect(TokenCategory.IF)
            };

            
            result.Add(Expression());
            Expect(TokenCategory.THEN);

            if (firstOfStatement.Contains(CurrentToken)){
                var stmlist = new StatementList();
                while (firstOfStatement.Contains(CurrentToken)){
                    stmlist.Add(Statement());
                }
                result.Add(stmlist);
            }

                var eiList = new ElseIfList();
            if (CurrentToken == TokenCategory.ELSEIF){


                while (CurrentToken == TokenCategory.ELSEIF){
                    var ei = new ElseIf(){
                        AnchorToken = Expect(TokenCategory.ELSEIF)
                    };

                    ei.Add(Expression());

                    Expect(TokenCategory.THEN);

                    if (firstOfStatement.Contains(CurrentToken)){
                        var stmlist = new StatementList();
                        while (firstOfStatement.Contains(CurrentToken)){
                            stmlist.Add(Statement());
                        }
                        ei.Add(stmlist);
                    }
                    eiList.Add(ei);
                }
            }
                result.Add(eiList);

                var e = new Else();
            if (CurrentToken == TokenCategory.ELSE){
                e.AnchorToken = Expect(TokenCategory.ELSE);

                if (firstOfStatement.Contains(CurrentToken)){
                    var stmlist = new StatementList();
                    while (firstOfStatement.Contains(CurrentToken)){
                        stmlist.Add(Statement());
                    }
                    e.Add(stmlist);
                }
            }
                result.Add(e);

            Expect(TokenCategory.END);
            
            Expect(TokenCategory.SEMICOLON);

            return result;
        }

        public Node Return(){
            var result = new Return()
            {
                AnchorToken = Expect(TokenCategory.RETURN)
            };

		if(CurrentToken != TokenCategory.SEMICOLON)
            result.Add(Expression());

            Expect(TokenCategory.SEMICOLON);

            return result;
        }

        public Node Exit(){
            var result = new Exit(){
                AnchorToken = Expect(TokenCategory.EXIT)
            };

            Expect(TokenCategory.SEMICOLON);

            return result;
        }

        public Node For(){
            var result = new For(){
                AnchorToken = Expect(TokenCategory.FOR)
            };

            result.Add(new Identifier()
            {
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            });

            Expect(TokenCategory.IN);

            result.Add(Expression());

            Expect(TokenCategory.DO);

            var stmlist = new StatementList();
            if (firstOfStatement.Contains(CurrentToken))
            {
                while (firstOfStatement.Contains(CurrentToken))
                {
                    stmlist.Add(Statement());
                }
            }
            result.Add(stmlist);

            Expect(TokenCategory.END);

            Expect(TokenCategory.SEMICOLON);

            return result;
        }

        public Node Loop(){
            var result = new Loop()
            {
                AnchorToken = Expect(TokenCategory.LOOP)
            };

            var stmList = new StatementList();
            if (firstOfStatement.Contains(CurrentToken))
            {
                while (firstOfStatement.Contains(CurrentToken))
                {
                    stmList.Add(Statement());
                }
            }
            result.Add(stmList);

            Expect(TokenCategory.END);

            Expect(TokenCategory.SEMICOLON);

            return result;
        }

        public Node Expression(){
            return LogicExpression();
        }

        public Node LogicExpression()
        {
            var expr = RelationalExpression();
            while (firstOfLogicOperator.Contains(CurrentToken))
            {
                var expr2 = LogicOperator();
                expr2.Add(expr);
                expr2.Add(RelationalExpression());
                expr = expr2;
            }
            return expr;
        }

        public Node RelationalExpression()
        {
            var expr = SumExpression();
            while (firstOfRelationalOperator.Contains(CurrentToken))
            {
                var expr2 = RelationalOperator();
                expr2.Add(expr);
                expr2.Add(SumExpression());
                expr = expr2;
            }

            return expr;
        }

        public Node SumExpression()
        {
            var expr = MultExpression();
            while (firstOfSumOperator.Contains(CurrentToken))
            {
                var expr2 = SumOperator();
                expr2.Add(expr);
                expr2.Add(MultExpression());
                expr = expr2;
            }
            return expr;
        }

        public Node MultExpression()
        {
            var expr = UnaryExpression();
            while (firstOfMultOperator.Contains(CurrentToken))
            {
                var expr2 = MultOperator();
                expr2.Add(expr);
                expr2.Add(UnaryExpression());
                expr = expr2;
            }
            return expr;
        }

        public Node UnaryExpression(){
            if (firstOfUnaryOperator.Contains(CurrentToken)){
                var expr = UnaryOperator();

                expr.Add(UnaryExpression());
                return expr;
            }

            else 
                return SimpleExpression();

        }

        public Node SimpleExpression(){
            switch (CurrentToken) {
                case TokenCategory.PARENTHESIS_OPEN:
                    Expect(TokenCategory.PARENTHESIS_OPEN);
                    var expr = Expression();
                    Expect(TokenCategory.PARENTHESIS_CLOSE);
                    if (CurrentToken == TokenCategory.BRACKET_OPEN){
                        var result = new ListItem();
                        result.Add(expr);
                        Aux2(result);
                        return result;
                    }else
                        return expr;

                case TokenCategory.IDENTIFIER:
                    var identifier = Expect(TokenCategory.IDENTIFIER);
                    if (CurrentToken == TokenCategory.PARENTHESIS_OPEN)
                    {
                        var result = Call();
                        result.AnchorToken = identifier;

                        if (CurrentToken == TokenCategory.BRACKET_OPEN)
                        {
                            var call = new ListItem();
                            call.Add(result);
                            Aux2(call);
                            return call;
                        }
                        else
                            return result;
                    }
                    else
                    {
                        if (CurrentToken == TokenCategory.BRACKET_OPEN)
                        {
                            var id = new ListItem();
                            id.Add(new Identifier()
                            {
                                AnchorToken = identifier
                            });
                            Aux2(id);
                            return id;
                        }
                        else
                        {
                            var id = new Identifier()
                            {
                                AnchorToken = identifier
                            };
                            return id;
                        }
                    }
                case TokenCategory.INT_LITERAL:
                    var literal = new Int_Literal(){
                        AnchorToken = Expect(TokenCategory.INT_LITERAL)
                    };
                    if (CurrentToken == TokenCategory.BRACKET_OPEN){
                        var i_literal = new ListItem();
                        i_literal.Add(literal);
                        Aux2(i_literal);
                        return i_literal;
                    }
                    else
                        return literal;
                case TokenCategory.STR_LITERAL:
                    var sliteral = new Str_Literal(){
                        AnchorToken = Expect(TokenCategory.STR_LITERAL)
                    };
                    if (CurrentToken == TokenCategory.BRACKET_OPEN){
                        var s_literal = new ListItem();
                        s_literal.Add(sliteral);
                        Aux2(s_literal);
                        return s_literal;
                    }
                    else
                        return sliteral;
                case TokenCategory.TRUE:
                    var tLiteralToken = new True(){
                        AnchorToken = Expect(TokenCategory.TRUE)
                    };
                    if (CurrentToken == TokenCategory.BRACKET_OPEN){
                        var t_literal = new ListItem();
                        t_literal.Add(tLiteralToken);
                        Aux2(t_literal);
                        return t_literal;
                    }
                    else
                        return tLiteralToken;
                case TokenCategory.FALSE:
                    var fLiteralToken = new False(){
                        AnchorToken = Expect(TokenCategory.FALSE)
                    };
                    if (CurrentToken == TokenCategory.BRACKET_OPEN)
                    {
                        var f_literal = new ListItem();
                        f_literal.Add(fLiteralToken);
                        Aux2(f_literal);
                        return f_literal;
                    }
                    else
                        return fLiteralToken;
                case TokenCategory.BRACE_OPEN:
                    var lista = List();
                    if (CurrentToken == TokenCategory.BRACKET_OPEN){
                        var l_literal = new ListItem();
                        l_literal.Add(lista);
                        Aux2(l_literal);
                        return l_literal;
                    }
                    else
                        return lista;
                default:
                    throw new SyntaxError(firstOfSimpleExpression,
                                          tokenStream.Current);
            }
        }

        public void Aux2(Node result) {
            var ListIndex = new ListIndex();
            ListIndex.AnchorToken = Expect(TokenCategory.BRACKET_OPEN);
            ListIndex.Add(Expression());
            result.Add(ListIndex);
            Expect(TokenCategory.BRACKET_CLOSE);
        }

        public Node LogicOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.AND:
                    return new And()
                    {
                        AnchorToken = Expect(TokenCategory.AND)
                    };

                case TokenCategory.OR:
                    return new Or()
                    {
                        AnchorToken = Expect(TokenCategory.OR)
                    };

                case TokenCategory.XOR:
                    return new Xor()
                    {
                        AnchorToken = Expect(TokenCategory.XOR)
                    };

                default:
                    throw new SyntaxError(firstOfLogicOperator,
                                          tokenStream.Current);
            }
        }

        public Node RelationalOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.EQUALITY:
                    return new Equality()
                    {
                        AnchorToken = Expect(TokenCategory.EQUALITY)
                    };

                case TokenCategory.INEQUALITY:
                    return new Inequality()
                    {
                        AnchorToken = Expect(TokenCategory.INEQUALITY)
                    };

                case TokenCategory.LESS:
                    return new LessThan()
                    {
                        AnchorToken = Expect(TokenCategory.LESS)
                    };

                case TokenCategory.MORE:
                    return new GreaterThan()
                    {
                        AnchorToken = Expect(TokenCategory.MORE)
                    };

                case TokenCategory.LESS_EQUAL:
                    return new LessEqual()
                    {
                        AnchorToken = Expect(TokenCategory.LESS_EQUAL)
                    };

                case TokenCategory.MORE_EQUAL:
                    return new GreaterEqual()
                    {
                        AnchorToken = Expect(TokenCategory.MORE_EQUAL)
                    };

                default:
                    throw new SyntaxError(firstOfRelationalOperator,
                                          tokenStream.Current);
            }
        }

        public Node SumOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.SUM:
                    return new Sum()
                    {
                        AnchorToken = Expect(TokenCategory.SUM)
                    };

                case TokenCategory.MINUS:
                    return new Minus()
                    {
                        AnchorToken = Expect(TokenCategory.MINUS)
                    };

                default:
                    throw new SyntaxError(firstOfRelationalOperator,
                                          tokenStream.Current);
            }
        }

        public Node MultOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.MULT:
                    return new Mult()
                    {
                        AnchorToken = Expect(TokenCategory.MULT)
                    };

                case TokenCategory.DIV:
                    return new Div()
                    {
                        AnchorToken = Expect(TokenCategory.DIV)
                    };

                case TokenCategory.REM:
                    return new Rem()
                    {
                        AnchorToken = Expect(TokenCategory.REM)
                    };

                default:
                    throw new SyntaxError(firstOfMultOperator,
                                          tokenStream.Current);
            }
        }

        public Node UnaryOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.NOT:
                    return new Not()
                    {
                        AnchorToken = Expect(TokenCategory.NOT)
                    };

                case TokenCategory.NEGATION:
                    return new Negation()
                    {
                        AnchorToken = Expect(TokenCategory.NEGATION)
                    };

                default:
                    throw new SyntaxError(firstOfUnaryOperator,
                                          tokenStream.Current);
            }
        }
    }
}



