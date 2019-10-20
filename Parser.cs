/*
  Chimera compiler - This class performs the syntactic analysis,
  (a.k.a. parsing).
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358 
*/

using System;
using System.Collections.Generic;

namespace Chimera {

    class Parser {      

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
        //**************************************************** */
        //**************************************************** */
        //**************************************************** */
        
                
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
            var declList = new DeclarationList();
            var stmtList = new StatementList();

            while (firstOfDeclaration.Contains(CurrentToken)) {
                declList.Add(Declaration());
                /*Declaration();*/
            }
            Expect(TokenCategory.PROGRAM);

            while (firstOfStatement.Contains(CurrentToken)) {
                stmtList.Add(Statement());
                /*Statement();*/
            }
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            Expect(TokenCategory.EOF);

            return new Program() {
                declList,
                stmtList
            };
        }
        //Pendiente
        public Node Declaration() {
            var result = new Declaration()
            {
                AnchorToken= Type()
            };
            if (CurrentToken == TokenCategory.CONST){
                Expect(TokenCategory.CONST);
                do{
                    ConstDeclaration();
                }while(CurrentToken == TokenCategory.IDENTIFIER);
            }

            if(CurrentToken == TokenCategory.VAR){
                Expect(TokenCategory.VAR);
                do{
                    ParameterDeclaration();
                }while(CurrentToken == TokenCategory.IDENTIFIER);
            }

            while(CurrentToken == TokenCategory.PROCEDURE){
                ProcedureDeclaration();
            }

            return result;
        }

            public Node ConstDeclaration() {
                var result = new ConstDeclaration()
                {
                    AnchorToken = Expect(TokenCategory.IDENTIFIER)
                };
                    
                    Expect(TokenCategory.COLON_EQUAL);
                result.Add(Literal());
                    Expect(TokenCategory.SEMICOLON);
                return result;
            }

        //PENDIENTE
        public Node ParameterDeclaration(){
            var result = new List<Node>();
            result.Add(Expect(TokenCategory.IDENTIFIER));
            while (CurrentToken == TokenCategory.COMA){
                result.Add(Expect(TokenCategory.COMA));
                return (Expect(TokenCategory.IDENTIFIER));
            }
            Expect(TokenCategory.COLON);
            var type = Type();
            Expect(TokenCategory.SEMICOLON);
            type.Add(result);
            return type;
        }

        //Pendiente
        public Node ProcedureDeclaration() {
            Expect(TokenCategory.PROCEDURE);
            var result = new ProcedureDeclaration()
            {
                AnchorToken = Expect(TokenCategory.IDENTIFIER)

                };
            Expect(TokenCategory.PARENTHESIS_OPEN);
            while (CurrentToken == TokenCategory.IDENTIFIER){
                ParameterDeclaration();
            }
                Expect(TokenCategory.PARENTHESIS_CLOSE);
                if(CurrentToken == TokenCategory.COLON) {
                    Expect(TokenCategory.COLON);
                    Type();
                }
                Expect(TokenCategory.SEMICOLON);
                if (firstOfDeclaration.Contains(CurrentToken))
                    Declaration();
                Expect(TokenCategory.BEGIN);
            while (firstOfStatement.Contains(CurrentToken)){
                Statement();
            }
            Expect(TokenCategory.END);
                Expect(TokenCategory.SEMICOLON);
            return result;
        }

            public Node SimpleLiteral(){
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

        public Node Literal(){
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

        public Node SimpleType(){
            switch (CurrentToken){
                case TokenCategory.INTEGER:
                        return new IntegerLiteral()
                        {
                            AnchorToken = Expect(TokenCategory.INTEGER)
                            };

                case TokenCategory.STRING:
                    return new StringLiteral()
                    {
                        AnchorToken = Expect(TokenCategory.STRING)
                    };

                case TokenCategory.BOOL:
                    return new BooleanLiteral()
                    {
                        AnchorToken= Expect(TokenCategory.BOOL)
                    };
                default:
                    throw new SyntaxError(firstOfSimpleType,
                                          tokenStream.Current);
            }
            }

        public Node Type(){

            if (CurrentToken == TokenCategory.LIST)
            {
                Expect(TokenCategory.LIST);
                Expect(TokenCategory.OF);
                return SimpleType();
            }

            else if (CurrentToken != TokenCategory.LIST)
                return SimpleType();

            else
            {
                throw new SyntaxError(firstOfType,
                                              tokenStream.Current);
            }
        }


        public Node Statement(){
            switch (CurrentToken){
                case TokenCategory.IDENTIFIER:
                    Expect(TokenCategory.IDENTIFIER);
                    if (CurrentToken == TokenCategory.PARENTHESIS_OPEN)
                        return Call();
                    else if (CurrentToken == TokenCategory.BRACKET_OPEN || CurrentToken == TokenCategory.COLON_EQUAL)
                        return Assignment();
                case TokenCategory.IF:
                    return If() ;
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

        //Pendiente
        public Node List()
        {
            var result = new List();
            Expect(TokenCategory.BRACE_OPEN);
            if (firstOfSimpleLiteral.Contains(CurrentToken))
            {
                Literal();
                while (CurrentToken == TokenCategory.COMA)
                {
                    Expect(TokenCategory.COMA);
                    SimpleLiteral();
                }
                Expect(TokenCategory.BRACE_CLOSE);
            }
            return result;
        }
        public Node Call(){
            var result = null;
            CallStatement();
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

        public Node CallStatement()
        {
            var result = null;
            Expect(TokenCategory.PARENTHESIS_OPEN);
            if (CurrentToken != TokenCategory.PARENTHESIS_CLOSE)
            {
                Expression();
                while (CurrentToken == TokenCategory.COMA)
                {
                    Expect(TokenCategory.COMA);
                    Expression();
                }
            }
            Expect(TokenCategory.PARENTHESIS_CLOSE);
            return result;
        }

        public Node Assignment(){
            var result = null;
            if (CurrentToken == TokenCategory.BRACKET_OPEN){
                Expect(TokenCategory.BRACKET_OPEN);
                Expression();
                Expect(TokenCategory.BRACKET_CLOSE);
            }

            Expect(TokenCategory.COLON_EQUAL);
            Expression();
            Expect(TokenCategory.SEMICOLON);

            return result;
        }

        //PENDIENTE
        public Node If(){
            var result = new If()
            {
                AnchorToken = Expect(TokenCategory.IF)
        };
            result.Add(Expression());
            Expect(TokenCategory.THEN);
            while (firstOfStatement.Contains(CurrentToken)){
                Statement();
            }
            while (CurrentToken == TokenCategory.ELSEIF){
                Expect(TokenCategory.ELSEIF);
                Expression();
                Expect(TokenCategory.THEN);
                while (firstOfStatement.Contains(CurrentToken)){
                    Statement();
                }
            }
            if (CurrentToken == TokenCategory.ELSE){
                Expect(TokenCategory.ELSE);
                while (firstOfStatement.Contains(CurrentToken)){
                    Statement();
                }
            }
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

        //
        public Node Loop(){
            var result = new Loop()
            {
                AnchorToken = Expect(TokenCategory.LOOP)
                };

            while (firstOfStatement.Contains(CurrentToken)){
                result.Add(Statement()); 
            }
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            return result;
        }
        //
        public Node For(){
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
            while (firstOfStatement.Contains(CurrentToken)){
                result.Add(Statement());
            }
            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

        public Node Return(){
            var result = new Return()
            {
                AnchorToken = Expect(TokenCategory.RETURN)
                };
           
            result.Add(Expression());
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

            public Node Exit(){
            var result = new Exit()
            {
            AnchorToken = Expect(TokenCategory.EXIT)
            };
                Expect(TokenCategory.SEMICOLON);
                return result;
            }

        public Node Expression(){
            return LogicExpression();
        }

        //PENDIENTE
        public Node LogicExpression(){
            var result = null;
            RelationalExpression();
            while (firstOfLogicOperator.Contains(CurrentToken)){
                LogicOperator();
                RelationalExpression();
            }
            return result;
        }

        public Node LogicOperator(){    
            switch (CurrentToken){
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

        //PENDIENTE
        public Node RelationalExpression(){
            var result = null;
            SumExpression();
            while (firstOfRelationalOperator.Contains(CurrentToken)){
                RelationalOperator();
                SumExpression();
            }
            return result;
        }

        //
        public Node SumExpression(){
            var result = MulExpression();
            while (firstOfSumOperator.Contains(CurrentToken))
            {
                var temp = SumOperator();
                temp.Add(result);
                temp.Add(MulExpression());
                result = temp;
            }
            return result;
            /*MulExpression();
            while (firstOfSumOperator.Contains(CurrentToken)){
                SumOperator();
                MulExpression();
            }*/
        }

        public Node SumOperator(){
            switch (CurrentToken){

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
//Posible fallo
        public Node MulExpression(){
            var result = UnaryExpression();
            while (firstOfMulOperator.Contains(CurrentToken))
            {
                var temp = MulOperator();
                temp.Add(result);
                temp.Add(UnaryExpression());
                result = temp;
            }
            return result;
                /*UnaryExpression();
                while (firstOfMulOperator.Contains(CurrentToken)){
                    MulOperator();
                    UnaryExpression();
                }*/
            }

        public Node MulOperator(){
            switch (CurrentToken){

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
            while (firstOfUnaryOperator.Contains(CurrentToken)){
                switch (CurrentToken){

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
                    throw new SyntaxError(firstOfUnaryOperator,tokenStream.Current);
                }
            }
            return SimpleExpression();
        }

        //PENDIENTE
        public Node SimpleExpression(){
            Node node = null;
            switch (CurrentToken){
                case TokenCategory.PARENTHESIS_OPEN:
                    Expect(TokenCategory.PARENTHESIS_OPEN);
                    Expression();
                    Expect(TokenCategory.PARENTHESIS_CLOSE);
                    break;

                case TokenCategory.IDENTIFIER:
                    Expect(TokenCategory.IDENTIFIER);
                    if (CurrentToken == TokenCategory.PARENTHESIS_OPEN)
                    CallStatement();
                    break;

                case TokenCategory.INT_LITERAL:
                case TokenCategory.STRING_LITERAL:
                case TokenCategory.TRUE:
                case TokenCategory.FALSE:
                case TokenCategory.BRACE_OPEN:
                    Literal();
                    break;

                default:
                    throw new SyntaxError(firstOfSimpleExpression,tokenStream.Current);
            }

            if (CurrentToken == TokenCategory.BRACKET_OPEN){
                Expect(TokenCategory.BRACKET_OPEN);
                Expression();
                Expect(TokenCategory.BRACKET_CLOSE);
            }

            return node;
        }
        
        //PENDIENTE
        public Node RelationalOperator(){
            var result = null;
            switch (CurrentToken){

                case TokenCategory.ASSIGN:
                    Expect(TokenCategory.ASSIGN);
                    break;

                case TokenCategory.LESS_MORE:
                    Expect(TokenCategory.LESS_MORE);
                    break;

                case TokenCategory.LESS:
                    Expect(TokenCategory.LESS);
                    break;

                case TokenCategory.MORE:
                    Expect(TokenCategory.MORE);
                    break;

                case TokenCategory.LESS_EQUAL:
                    Expect(TokenCategory.LESS_EQUAL);
                    break;

                case TokenCategory.MORE_EQUAL:
                    Expect(TokenCategory.MORE_EQUAL);
                    break;

                default:
                    throw new SyntaxError(firstOfRelationalOperator,
                                          tokenStream.Current);
            }
            return result;
        }
    }
}