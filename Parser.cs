/*
  Chimera compiler - This class performs the syntactic analysis,
  (a.k.a. parsing).
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358 
*/

using System;
using System.Collections.Generic;

namespace Chimera
{

    class Parser
    {

        static readonly ISet<TokenCategory> firstOfDeclaration =
            new HashSet<TokenCategory>() {
                TokenCategory.VAR,
                TokenCategory.CONST,
                TokenCategory.PROCEDURE
            };

        static readonly ISet<TokenCategory> firstOfLiteral =
            new HashSet<TokenCategory>() {
                TokenCategory.INT_LITERAL,
                TokenCategory.STRING_LITERAL,
                TokenCategory.TRUE,
                TokenCategory.FALSE,
                TokenCategory.BRACE_OPEN
            };

        static readonly ISet<TokenCategory> firstOfSimpleLiteral =
            new HashSet<TokenCategory>() {
                TokenCategory.INT_LITERAL,
                TokenCategory.STRING_LITERAL,
                TokenCategory.TRUE,
                TokenCategory.FALSE,
            };

        static readonly ISet<TokenCategory> firstOfType =
            new HashSet<TokenCategory>() {
                TokenCategory.INTEGER,
                TokenCategory.STRING,
                TokenCategory.BOOL,
                TokenCategory.LIST
            };

        static readonly ISet<TokenCategory> firstOfSimpleType =
            new HashSet<TokenCategory>() {
                TokenCategory.INTEGER,
                TokenCategory.STRING,
                TokenCategory.BOOL
            };

        static readonly ISet<TokenCategory> firstOfStatement =
            new HashSet<TokenCategory>() {
                TokenCategory.IDENTIFIER,
                TokenCategory.IF,
                TokenCategory.LOOP,
                TokenCategory.FOR,
                TokenCategory.RETURN,
                TokenCategory.EXIT
            };

        static readonly ISet<TokenCategory> firstOfLogicOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.AND,
                TokenCategory.OR,
                TokenCategory.XOR
            };

        static readonly ISet<TokenCategory> firstOfRelationalOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.ASSIGN,
                TokenCategory.LESS_MORE,
                TokenCategory.LESS,
                TokenCategory.LESS_EQUAL,
                TokenCategory.MORE,
                TokenCategory.MORE_EQUAL
            };

        static readonly ISet<TokenCategory> firstOfSumOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.PLUS,
                TokenCategory.MINUS
            };

        static readonly ISet<TokenCategory> firstOfMulOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.MUL,
                TokenCategory.DIV,
                TokenCategory.REM
            };

        static readonly ISet<TokenCategory> firstOfUnaryOperator =
            new HashSet<TokenCategory>() {
                TokenCategory.MINUS,
                TokenCategory.NOT
            };

        static readonly ISet<TokenCategory> firstOfSimpleExpression =
            new HashSet<TokenCategory>() {
                TokenCategory.PARENTHESIS_OPEN,
                TokenCategory.IDENTIFIER,
                TokenCategory.INT_LITERAL,
                TokenCategory.STRING_LITERAL,
                TokenCategory.TRUE,
                TokenCategory.FALSE,
                TokenCategory.BRACE_OPEN
            };


        IEnumerator<Token> tokenStream;

        public Parser(IEnumerator<Token> tokenStream)
        {
            this.tokenStream = tokenStream;
            this.tokenStream.MoveNext();
        }

        public TokenCategory CurrentToken
        {
            get { return tokenStream.Current.Category; }
        }

        public Token Expect(TokenCategory category)
        {
            if (CurrentToken == category)
            {
                Token current = tokenStream.Current;
                tokenStream.MoveNext();
                return current;
            }
            else
            {
                throw new SyntaxError(category, tokenStream.Current);
            }
        }

        public Node Program()
        {
            var constantList = new ConstantDeclarationList();
            var varList = new VariableDeclarationList();
            var procList = new ProcedureDeclarationList();
            var stmtList = new StatementList();

            //IF-> do while because it's one or more times
            if (CurrentToken == TokenCategory.CONST)
            {
                do
                {
                    constantList.Add(ConstDeclaration());
                } while (CurrentToken == TokenCategory.CONST);
            }
            if (CurrentToken == TokenCategory.VAR)
            {
                do
                {
                    varList.Add(VariableDeclaration());
                } while (CurrentToken == TokenCategory.IDENTIFIER);
            }

            //just while because it's zero or more times
            while (CurrentToken == TokenCategory.PROCEDURE)
            {
                procList.Add(ProcedureDeclaration());
            }
            Expect(TokenCategory.PROGRAM);

            while (firstOfStatement.Contains(CurrentToken))
            {
                stmtList.Add(Statement());
            }
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            Expect(TokenCategory.EOF);

            return new Program() {
                constantList,
                varList,
                procList,
                stmtList
            };
        }

        public Node VariableDeclaration()
        {
            var varD = new VariableDeclarationList()
            {
                AnchorToken = Expect(TokenCategory.VAR)
            };
            do
            {
                var ide = new Identifier()
                {
                    AnchorToken = Expect(TokenCategory.IDENTIFIER)
                };

                while (CurrentToken == TokenCategory.COMA)
                {
                    Expect(TokenCategory.COMA);
                    
                    var ide2 = new Identifier()
                    {
                        AnchorToken = Expect(TokenCategory.IDENTIFIER)
                    };
                    varD.Add(ide);
                    varD.Add(ide2);
                }
                Expect(TokenCategory.COLON);

                var varFinal = new VariableDeclarationList();
                varFinal.Add(Type());
                varD.Add(varFinal);
                Expect(TokenCategory.SEMICOLON);

            } while (CurrentToken == TokenCategory.IDENTIFIER);



            return varD;
        }

        public Node ConstDeclaration()
        {
            var result = new ConstDeclaration()
            {
                AnchorToken = Expect(TokenCategory.CONST)
            };
            Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.COLON_EQUAL);
            result.Add(Literal());
            Expect(TokenCategory.SEMICOLON);
            return result;
        }


        public Node ParameterDeclaration()
        {
            var result = Expect(TokenCategory.IDENTIFIER);
            var list = new IdentifierList();
            while (CurrentToken == TokenCategory.COMA)
            {
                list.Add(new Identifier()
                {
                    AnchorToken = Expect(TokenCategory.IDENTIFIER)
                });

            }
            Expect(TokenCategory.COLON);
            var type = Type();
            Expect(TokenCategory.SEMICOLON);
            var temp = new ParameterDeclaration() { list, type };
            temp.AnchorToken = result;
            return temp;
        }
        public Node ProcedureDeclaration()
        {
            var procT = Expect(TokenCategory.PROCEDURE);
            var result = new ProcedureDeclaration()
            {
                AnchorToken = Expect(TokenCategory.IDENTIFIER)

            };
            var parameterList = new ParamaterDeclarationList();
            var type = new Type();
            var constantList = new ConstantDeclarationList();
            var varList = new VariableDeclarationList();
            var statementList = new StatementList();

            Expect(TokenCategory.PARENTHESIS_OPEN);
            while (CurrentToken == TokenCategory.IDENTIFIER)
            {
                parameterList.Add(ParameterDeclaration());
            }
            Expect(TokenCategory.PARENTHESIS_CLOSE);
            if (CurrentToken == TokenCategory.COLON)
            {
                Expect(TokenCategory.COLON);
               type.Add(Type());
            }
            Expect(TokenCategory.SEMICOLON);
            if (CurrentToken == TokenCategory.CONST)
            {
                do
                {
                    constantList.Add(ConstDeclaration());
                } while (CurrentToken == TokenCategory.IDENTIFIER);
            }
            if (CurrentToken == TokenCategory.VAR)
            {
                do
                {
                    varList.Add(VariableDeclaration());
                } while (CurrentToken == TokenCategory.IDENTIFIER);
            }

            Expect(TokenCategory.BEGIN);
            while (firstOfStatement.Contains(CurrentToken))
            {
                statementList.Add(Statement());
            }
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            var temp= new ProcedureDeclaration(){result,parameterList,type,
            constantList,varList,statementList
                            };
            temp.AnchorToken = procT;
            return temp;
        }

        public Node Literal()
        {
            if (CurrentToken == TokenCategory.BRACE_OPEN)
                return List();

            else if (CurrentToken != TokenCategory.BRACE_OPEN)
                return SimpleLiteral();

            else
            {
                throw new SyntaxError(firstOfLiteral,
                                          tokenStream.Current);
            }

        }

        public Node SimpleLiteral()
        {
            switch (CurrentToken)
            {
                case TokenCategory.INT_LITERAL:
                    return new IntegerLiteral()
                    {
                        AnchorToken = Expect(TokenCategory.INT_LITERAL)
                    };

                case TokenCategory.STRING_LITERAL:
                    return new StringLiteral()
                    {
                        AnchorToken = Expect(TokenCategory.STRING_LITERAL)
                    };

                case TokenCategory.TRUE:
                    return new TrueLiteral()
                    {
                        AnchorToken = Expect(TokenCategory.TRUE)
                    };

                case TokenCategory.FALSE:
                    return new FalseLiteral()
                    {
                        AnchorToken = Expect(TokenCategory.FALSE)
                    };


                default:
                    throw new SyntaxError(firstOfSimpleLiteral,
                                          tokenStream.Current);
            }
        }

       

        public Token SimpleType()
        { 
            switch (CurrentToken)
            {
                case TokenCategory.INTEGER:
                    return Expect(TokenCategory.INTEGER);
                case TokenCategory.STRING:
                    return Expect(TokenCategory.STRING);
                case TokenCategory.BOOL:
                    return Expect(TokenCategory.BOOL);
                default:
                    throw new SyntaxError(firstOfSimpleExpression,
                                        tokenStream.Current);
            }
        }

        public Node Type()
        {
            switch (CurrentToken)
            {
                case TokenCategory.LIST:
                    return ListType();
                case TokenCategory.STRING:
                    return new Type()
                    {
                        AnchorToken = SimpleType()
                    };
                case TokenCategory.BOOL:
                    return new Type()
                    {
                        AnchorToken = SimpleType()
                    };
                case TokenCategory.INTEGER:
                    return new Type()
                    {
                        AnchorToken = SimpleType()
                    };
                default:
                    throw new SyntaxError(firstOfSimpleExpression,
                                        tokenStream.Current);
            }
        }

        public Node ListType()
        {
            Expect(TokenCategory.LIST);
            Expect(TokenCategory.OF);
            return new SimpleType()
            {
                AnchorToken = SimpleType()
            };
        }


        public Node Statement()
        {
            switch (CurrentToken)
            {
                case TokenCategory.IDENTIFIER:
                    return AssignmentCallStatement();
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

        public Node AssignmentCallStatement()
        {
            var ident = Expect(TokenCategory.IDENTIFIER);
            var expList = new ExpressionList();
            var exp = new Expression();          
            if (CurrentToken == TokenCategory.PARENTHESIS_OPEN)
            {
                Expect(TokenCategory.PARENTHESIS_OPEN);
                if (firstOfSimpleExpression.Contains(CurrentToken))
                {
                    exp.Add(Expression());
                    while (CurrentToken == TokenCategory.COMA)
                    {
                        Expect(TokenCategory.COMA);
                        expList.Add(Expression());
                    }
                }
                Expect(TokenCategory.PARENTHESIS_CLOSE);
                Expect(TokenCategory.SEMICOLON);
            }
            else if (CurrentToken == TokenCategory.BRACKET_OPEN || CurrentToken == TokenCategory.COLON_EQUAL)
            {
                if (CurrentToken == TokenCategory.BRACKET_OPEN)
                {
                    Expect(TokenCategory.BRACKET_OPEN);
                    exp.Add(Expression());
                    Expect(TokenCategory.BRACKET_CLOSE);
                }
                Expect(TokenCategory.COLON_EQUAL);
                expList.Add(Expression());
                Expect(TokenCategory.SEMICOLON);
            }
            var result = new AssignmentCallStatement() { exp, expList };
            result.AnchorToken = ident;
            return result;
        }
//ExpressionLst not need just expression.
        // public Node AssignmentStatement()
        // {
        //     var ident = Expect(TokenCategory.IDENTIFIER);
        //     var expList = new ExpressionList();
        //     var exp = new Expression();          
        //     if (CurrentToken == TokenCategory.BRACKET_OPEN || CurrentToken == TokenCategory.COLON_EQUAL)
        //     {
        //         if (CurrentToken == TokenCategory.BRACKET_OPEN)
        //         {
        //             Expect(TokenCategory.BRACKET_OPEN);
        //             exp.Add(Expression());
        //             Expect(TokenCategory.BRACKET_CLOSE);
        //         }
        //         Expect(TokenCategory.COLON_EQUAL);
        //         expList.Add(Expression());
        //         Expect(TokenCategory.SEMICOLON);
        //     }
        //     var result = new AssignmentStatement() { exp, expList };
        //     result.AnchorToken = ident;
        //     return result;
        // }

        // public Node CallStatement()
        // {
        //     var ident = Expect(TokenCategory.IDENTIFIER);
        //     var expList = new ExpressionList();
        //     var exp = new Expression();          
        //     if (CurrentToken == TokenCategory.PARENTHESIS_OPEN)
        //     {
        //         Expect(TokenCategory.PARENTHESIS_OPEN);
        //         if (firstOfSimpleExpression.Contains(CurrentToken))
        //         {
        //             exp.Add(Expression());
        //             while (CurrentToken == TokenCategory.COMA)
        //             {
        //                 Expect(TokenCategory.COMA);
        //                 expList.Add(Expression());
        //             }
        //         }
        //         Expect(TokenCategory.PARENTHESIS_CLOSE);
        //         Expect(TokenCategory.SEMICOLON);
        //     }
            
        //     var result = new CallStatement() { exp, expList };
        //     result.AnchorToken = ident;
        //     return result;
        // }

        public Node List()
        {
            var simpleList = new SimpleLiteralList();
            var simpleLit = new SimpleLiteral();
            var result = Expect(TokenCategory.BRACE_OPEN);
            if (firstOfSimpleLiteral.Contains(CurrentToken))
            {
                simpleLit.Add(SimpleLiteral());
                while (CurrentToken == TokenCategory.COMA)
                {
                    Expect(TokenCategory.COMA);
                    simpleList.Add(SimpleLiteral());
                }
               
            }
            Expect(TokenCategory.BRACE_CLOSE);
            var temp = new List { simpleLit, simpleList };
            temp.AnchorToken = result;
            return temp;
        }

        public Node Call()
        {
            var result = new CallStatement();
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

        public Node CallStatement()
        {           
            Expect(TokenCategory.PARENTHESIS_OPEN);
            var result = new Call();
            if (CurrentToken != TokenCategory.PARENTHESIS_CLOSE)
            {
                result.Add(Expression());
                while (CurrentToken == TokenCategory.COMA)
                {
                    Expect(TokenCategory.COMA);
                    result.Add(Expression());
                }
            }
            Expect(TokenCategory.PARENTHESIS_CLOSE);
            return result;
        }

        // //Not requiered
        // public Node AssignmentStatement()
        // {
        //     var result = new Assignment();
        //     if (CurrentToken == TokenCategory.BRACKET_OPEN)
        //     {
        //         Expect(TokenCategory.BRACKET_OPEN);
        //         result.Add(Expression());
        //         Expect(TokenCategory.BRACKET_CLOSE);
        //     }
        //     Expect(TokenCategory.COLON_EQUAL);
        //     result.Add(Expression());
        //     Expect(TokenCategory.SEMICOLON);
        //     return result;
        // }

        public Node If()
        {
            var tkIF = Expect(TokenCategory.IF);           
            var exprList = new ExpressionList();
            var statementList = new StatementList();
            exprList.Add(Expression());
            Expect(TokenCategory.THEN);
            while (firstOfStatement.Contains(CurrentToken))
            {
                statementList.Add(Statement());
            }
            while (CurrentToken == TokenCategory.ELSEIF)
            {
                Expect(TokenCategory.ELSEIF);
                exprList.Add(Expression());
                Expect(TokenCategory.THEN);
                while (firstOfStatement.Contains(CurrentToken))
                {
                    statementList.Add(Statement());
                }
            }
            if (CurrentToken == TokenCategory.ELSE)
            {
                Expect(TokenCategory.ELSE);
                while (firstOfStatement.Contains(CurrentToken))
                {
                    statementList.Add(Statement());
                }
            }
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            var result=new If () {exprList,statementList};
            result.AnchorToken = tkIF;
            return result;
        }

        public Node Loop()
        {
            var result = new Loop()
            {
                AnchorToken = Expect(TokenCategory.LOOP)
            };

            while (firstOfStatement.Contains(CurrentToken))
            {
                result.Add(Statement());
            }
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

        public Node For()
        {
            var result = new For()
            {
                AnchorToken = Expect(TokenCategory.FOR)
            };
            result.Add(new Identifier()
            {
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            });
            Expect(TokenCategory.IN);
            result.Add(Expression());

            Expect(TokenCategory.DO);
            var statementList = new StatementList();
            while (firstOfStatement.Contains(CurrentToken))
            {
                result.Add(Statement());
            }
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

        public Node Return()
        {
            var result = new Return()
            {
                AnchorToken = Expect(TokenCategory.RETURN)
            };

            result.Add(Expression());
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

        public Node Exit()
        {
            var result = new Exit()
            {
                AnchorToken = Expect(TokenCategory.EXIT)
            };
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

        public Node Expression()
        {
            return LogicExpression();
        }

        public Node LogicExpression()
        {
            var result = RelationalExpression();
            while (firstOfLogicOperator.Contains(CurrentToken))
            {
                var temp = LogicOperator();
                temp.Add(result);
                temp.Add(RelationalExpression());
                result = temp;
            }
            return result;
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

        public Node RelationalExpression()
        {
            var result = SumExpression();
            while (firstOfRelationalOperator.Contains(CurrentToken))
            {
                var temp = RelationalOperator();
                temp.Add(result);
                temp.Add(SumExpression());
                result = temp;
            }
            return result;
        }

        public Node SumExpression()
        {
            var result = MulExpression();
            while (firstOfSumOperator.Contains(CurrentToken))
            {
                var temp = SumOperator();
                temp.Add(result);
                temp.Add(MulExpression());
                result = temp;
            }
            return result;
        }

        public Node SumOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.PLUS:
                    return new AdditionOperator()
                    {
                        AnchorToken = Expect(TokenCategory.PLUS)
                    };
                case TokenCategory.MINUS:

                    return new SubstractionOperator()
                    {
                        AnchorToken = Expect(TokenCategory.MINUS)
                    };

                default:
                    throw new SyntaxError(firstOfRelationalOperator,
                                          tokenStream.Current);
            }
        }

        public Node MulExpression()
        {
            var result = UnaryExpression();
            while (firstOfMulOperator.Contains(CurrentToken))
            {
                var temp = MulOperator();
                temp.Add(result);
                temp.Add(UnaryExpression());
                result = temp;
            }
            return result;
        }

        public Node MulOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.MUL:
                    return new MultiplicantOperator()
                    {
                        AnchorToken = Expect(TokenCategory.MUL)
                    };

                case TokenCategory.DIV:
                    return new DivOperator()
                    {
                        AnchorToken = Expect(TokenCategory.DIV)
                    };


                case TokenCategory.REM:
                    return new RemOperator()
                    {
                        AnchorToken = Expect(TokenCategory.REM)
                    };
                default:
                    throw new SyntaxError(firstOfMulOperator,
                                          tokenStream.Current);
            }
        }

        public Node UnaryExpression(){

            if (firstOfUnaryOperator.Contains(CurrentToken)){
                var expr = UnaryOperator();
                while (firstOfUnaryOperator.Contains(CurrentToken)){
                    var expr2 = UnaryOperator();
                    expr2.Add(expr);
                    expr = expr2;
                }
                var result = SimpleExpression();

                expr.Add(result);

                return result;
            }

            else 
            return SimpleExpression();

        }

        public Node UnaryOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.NOT:
                    return new NotOperator()
                    {
                        AnchorToken = Expect(TokenCategory.NOT)
                    };

                case TokenCategory.MINUS:
                    return new SubstractionOperator()
                    {
                        AnchorToken = Expect(TokenCategory.MINUS)
                    };

                default:
                    throw new SyntaxError(firstOfUnaryOperator,
                                          tokenStream.Current);
            }
        }

        public Node SimpleExpression()
        {
            switch (CurrentToken)
            {
                case TokenCategory.PARENTHESIS_OPEN:
                    Expect(TokenCategory.PARENTHESIS_OPEN);
                    var result = Expression();
                    Expect(TokenCategory.PARENTHESIS_CLOSE);

                    return result;

                case TokenCategory.IDENTIFIER:
                    var resultID = new Identifier()
                    {
                        AnchorToken = Expect(TokenCategory.IDENTIFIER)
                    };
                    if (CurrentToken == TokenCategory.PARENTHESIS_OPEN)
                    {

                        resultID.Add(CallStatement());
                    }
                    if (CurrentToken == TokenCategory.BRACKET_OPEN)
                    {
                        Expect(TokenCategory.BRACKET_OPEN);
                        resultID.Add(Expression());
                        Expect(TokenCategory.BRACKET_CLOSE);
                    }
                    return resultID;

                case TokenCategory.INT_LITERAL:
                    return SimpleLiteral();
                case TokenCategory.STRING_LITERAL:
                    return SimpleLiteral();
                case TokenCategory.TRUE:
                    return SimpleLiteral();
                   //return TrueLiteral();
                case TokenCategory.FALSE:
                    return SimpleLiteral();
                    //return FalseLiteral();
                case TokenCategory.BRACE_OPEN:
                    return Literal();

                default:
                    throw new SyntaxError(firstOfSimpleExpression, tokenStream.Current);
            }
        }

        public Node RelationalOperator()
        {
            switch (CurrentToken)
            {

                case TokenCategory.ASSIGN:
                    return new AssignOperator()
                    {
                        AnchorToken = Expect(TokenCategory.ASSIGN)
                    };

                case TokenCategory.LESS_MORE:
                    return new LessMoreOperator()
                    {
                        AnchorToken = Expect(TokenCategory.LESS_MORE)
                    };


                case TokenCategory.LESS:
                    return new LessOperator()
                    {
                        AnchorToken = Expect(TokenCategory.LESS)
                    };


                case TokenCategory.MORE:
                    return new MoreOperator()
                    {
                        AnchorToken = Expect(TokenCategory.MORE)
                    };

                case TokenCategory.LESS_EQUAL:
                    return new LessEqualOperator()
                    {
                        AnchorToken = Expect(TokenCategory.LESS_EQUAL)
                    };


                case TokenCategory.MORE_EQUAL:
                    return new MoreEqualOperator()
                    {
                        AnchorToken = Expect(TokenCategory.MORE_EQUAL)
                    };

                default:
                    throw new SyntaxError(firstOfRelationalOperator,
                                          tokenStream.Current);
            }
        }

    }
}
