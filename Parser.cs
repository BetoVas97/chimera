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
            var constantList = new ConstantDeclarationList();
            var varList = new VariableDeclarationList();
            var procedureList = new ProcedureDeclarationList();
            var stmtlist = new StatementList();

            //IF-> do while because it's one or more times
            if (firstOfDeclaration.Contains(CurrentToken) && CurrentToken == TokenCategory.CONST)
            {
                constantList.AnchorToken = Expect(TokenCategory.CONST);

                do
                {
                    constantList.Add(ConstantDeclaration());
                } while (CurrentToken == TokenCategory.IDENTIFIER);
            }

            if (firstOfDeclaration.Contains(CurrentToken) && CurrentToken == TokenCategory.VAR)
            {
                varList.AnchorToken = Expect(TokenCategory.VAR);

                do
                {
                    varList.Add(VariableDeclaration());
                } while (CurrentToken == TokenCategory.IDENTIFIER);
            }
            
            //just while because it's zero or more times
            while (firstOfDeclaration.Contains(CurrentToken) && CurrentToken == TokenCategory.PROCEDURE){
                procedureList.Add(ProcedureDeclaration());
            }

            Expect(TokenCategory.PROGRAM);

            while (firstOfStatement.Contains(CurrentToken)){
                stmtlist.Add(Statement());
            }

            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);

            return new Program(){
                constantList,
                varList,
                procedureList,
                stmtlist
            };
        }

        public Node VariableDeclaration()
        {
            var result= new VariableDeclarationItems();
            
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
                
            var varFinal = new VariableDeclaration();

            if (CurrentToken != TokenCategory.LIST)
            {
                varFinal.Add(SimpleType());
            }

            else if (CurrentToken == TokenCategory.LIST)
            {
                varFinal.Add(ListType());
            }

            varFinal.Add(result);
            Expect(TokenCategory.SEMICOLON);

            return varFinal;
        }


        public Node ConstantDeclaration()
        {
            var result = new ConstantDeclaration()
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

        public Node ProcedureDeclaration()
        {
            var result = new ProcedureDeclaration()
            {
                AnchorToken = Expect(TokenCategory.PROCEDURE)

            };
            result.Add(new Identifier() { 
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            });

            Expect(TokenCategory.PARENTHESIS_OPEN);
            
            var parameterList = new ParameterDeclarationList();
            if (CurrentToken == TokenCategory.IDENTIFIER){
                while (CurrentToken == TokenCategory.IDENTIFIER){
                    parameterList.Add(VariableDeclaration());
                }
            }
            result.Add(parameterList);

            Expect(TokenCategory.PARENTHESIS_CLOSE);

            var type = new TypeNode();
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

            var constantList = new ConstantDeclarationList();
            if (firstOfDeclaration.Contains(CurrentToken) && CurrentToken == TokenCategory.CONST){

                constantList.AnchorToken = Expect(TokenCategory.CONST);

                do{
                    constantList.Add(ConstantDeclaration());
                } while (CurrentToken == TokenCategory.IDENTIFIER);

            }
            result.Add(constantList);

            var variableList = new VariableDeclarationList();
            if (firstOfDeclaration.Contains(CurrentToken) && CurrentToken == TokenCategory.VAR)
            {
                variableList.AnchorToken = Expect(TokenCategory.VAR);

                do
                {
                    variableList.Add(VariableDeclaration());
                } while (CurrentToken == TokenCategory.IDENTIFIER);
            }
            result.Add(variableList);

            Expect(TokenCategory.BEGIN);

            var statementList = new StatementList();
            if (firstOfStatement.Contains(CurrentToken)){
                while (firstOfStatement.Contains(CurrentToken))
                {
                    statementList.Add(Statement());
                }
            }
            result.Add(statementList);


            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            return result;
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
        
        public Node SimpleType()
        {
            switch (CurrentToken)
            {
                case TokenCategory.INTEGER:
                    return new IntegerNode(){
                        AnchorToken = Expect(TokenCategory.INTEGER)
                    };

                case TokenCategory.STRING:
                    return new StringNode(){
                        AnchorToken = Expect(TokenCategory.STRING)
                    };

                case TokenCategory.BOOLEAN:
                    return new BooleanNode(){
                        AnchorToken = Expect(TokenCategory.BOOLEAN)
                    };

                default:
                    throw new SyntaxError(firstOfSimpleType,
                                      tokenStream.Current);
            }
        }

        public Node ListType()
        {
            var result = new ListNode()
            {
                AnchorToken = Expect(TokenCategory.LIST)
            };

            Expect(TokenCategory.OF);

            result.Add(SimpleType());

            return result;
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

        public Node Statement(){
            switch (CurrentToken){
                case TokenCategory.IDENTIFIER:
                    var identifier = Expect(TokenCategory.IDENTIFIER);
                    if (CurrentToken == TokenCategory.PARENTHESIS_OPEN){
                        var result = CallStatement();
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
                            var result = ListAssignmentS();
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

        public Node CallStatement(){
            var result = new CallStatement()
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

        public Node ListAssignmentS() {
            var result = new ListAssignmentS();
            Expect(TokenCategory.BRACKET_OPEN);
            var list = new ListIndex();
            list.Add(Expression());
            result.Add(list);
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
                var statementList = new StatementList();
                while (firstOfStatement.Contains(CurrentToken)){
                    statementList.Add(Statement());
                }
                result.Add(statementList);
            }

            var elseIfList = new ElseIfList();
            if (CurrentToken == TokenCategory.ELSEIF){
                while (CurrentToken == TokenCategory.ELSEIF){
                    var elseIf = new ElseIf(){
                        AnchorToken = Expect(TokenCategory.ELSEIF)
                    };

                    elseIf.Add(Expression());
                    Expect(TokenCategory.THEN);
                    if (firstOfStatement.Contains(CurrentToken)){
                        var statementList = new StatementList();
                        while (firstOfStatement.Contains(CurrentToken)){
                            statementList.Add(Statement());
                        }
                        elseIf.Add(statementList);
                    }
                    elseIfList.Add(elseIf);
                }
            }
            result.Add(elseIfList);

            var els = new Else();
            if (CurrentToken == TokenCategory.ELSE){
                els.AnchorToken = Expect(TokenCategory.ELSE);
                if (firstOfStatement.Contains(CurrentToken)){
                    var statementList = new StatementList();
                    while (firstOfStatement.Contains(CurrentToken)){
                        statementList.Add(Statement());
                    }
                    els.Add(statementList);
                }
            }
            result.Add(els);

            Expect(TokenCategory.END);            
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

        public Node Loop(){
            var result = new Loop()
            {
                AnchorToken = Expect(TokenCategory.LOOP)
            };

            var statementList = new StatementList();
            if (firstOfStatement.Contains(CurrentToken))
            {
                while (firstOfStatement.Contains(CurrentToken))
                {
                    statementList.Add(Statement());
                }
            }
            result.Add(statementList);

            Expect(TokenCategory.END);
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

            var statementList = new StatementList();
            if (firstOfStatement.Contains(CurrentToken))
            {
                while (firstOfStatement.Contains(CurrentToken))
                {
                    statementList.Add(Statement());
                }
            }
            result.Add(statementList);

            Expect(TokenCategory.END);
            Expect(TokenCategory.SEMICOLON);
            return result;
        }


        public Node Return(){
            var result = new Return()
            {
                AnchorToken = Expect(TokenCategory.RETURN)
            };

		    if(CurrentToken != TokenCategory.SEMICOLON){
                result.Add(Expression());
            }            

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
        
        public Node Expression(){
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
            var result = MultExpression();
            while (firstOfSumOperator.Contains(CurrentToken))
            {
                var temp = SumOperator();
                temp.Add(result);
                temp.Add(MultExpression());
                result = temp;
            }
            return result;
        }

        public Node MultExpression()
        {
            var result = UnaryExpression();
            while (firstOfMultOperator.Contains(CurrentToken))
            {
                var temp = MultOperator();
                temp.Add(result);
                temp.Add(UnaryExpression());
                result = temp;
            }
            return result;
        }

        public Node UnaryExpression(){
            if (firstOfUnaryOperator.Contains(CurrentToken)){
                var result = UnaryOperator();

                result.Add(UnaryExpression());
                return result;
            }

            else {
                return SimpleExpression();
            }
                

        }


        //this method has the structure for an expression with brackets List
        public void auxiliarMethod(Node result) {
            var ListIndex = new ListIndex();
            ListIndex.AnchorToken = Expect(TokenCategory.BRACKET_OPEN);
            ListIndex.Add(Expression());
            result.Add(ListIndex);
            Expect(TokenCategory.BRACKET_CLOSE);
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
                        auxiliarMethod(result);
                        return result;
                    }else{
                        return expr;
                    }                        

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
                            auxiliarMethod(call);
                            return call;
                        }
                        else
                            return result;
                    }
                    else
                    {
                        if (CurrentToken == TokenCategory.BRACKET_OPEN)
                        {
                            var listI = new ListItem();
                            listI.Add(new Identifier()
                            {
                                AnchorToken = identifier
                            });
                            auxiliarMethod(listI);
                            return listI;
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
                    var iLiteral = new Int_Literal(){
                        AnchorToken = Expect(TokenCategory.INT_LITERAL)
                    };
                    if (CurrentToken == TokenCategory.BRACKET_OPEN){
                        var listI = new ListItem();
                        listI.Add(iLiteral);
                        auxiliarMethod(listI);
                        return listI;
                    }
                    else{
                        return iLiteral;
                    }                        
                case TokenCategory.STR_LITERAL:
                    var sLiteral = new Str_Literal(){
                        AnchorToken = Expect(TokenCategory.STR_LITERAL)
                    };
                    if (CurrentToken == TokenCategory.BRACKET_OPEN){
                        var listI = new ListItem();
                        listI.Add(sLiteral);
                        auxiliarMethod(listI);
                        return listI;
                    }
                    else{
                        return sLiteral;
                    }
                        
                case TokenCategory.TRUE:
                    var tLiteral = new True(){
                        AnchorToken = Expect(TokenCategory.TRUE)
                    };
                    if (CurrentToken == TokenCategory.BRACKET_OPEN){
                        var listI = new ListItem();
                        listI.Add(tLiteral);
                        auxiliarMethod(listI);
                        return listI;
                    }
                    else{
                        return tLiteral;
                    }
                        
                case TokenCategory.FALSE:
                    var fLiteral = new False(){
                        AnchorToken = Expect(TokenCategory.FALSE)
                    };
                    if (CurrentToken == TokenCategory.BRACKET_OPEN)
                    {
                        var listI = new ListItem();
                        listI.Add(fLiteral);
                        auxiliarMethod(listI);
                        return listI;
                    }
                    else{
                        return fLiteral;
                    }

                case TokenCategory.BRACE_OPEN:
                    var list = List();
                    if (CurrentToken == TokenCategory.BRACKET_OPEN){
                        var listI = new ListItem();
                        listI.Add(list);
                        auxiliarMethod(listI);
                        return listI;
                    }
                    else{
                        return list;
                    }                        
                default:
                    throw new SyntaxError(firstOfSimpleExpression,
                                          tokenStream.Current);
            }
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