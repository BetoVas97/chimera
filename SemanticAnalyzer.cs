/*
  Chimera compiler - SemanticAnalyzer.
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358
*/
using System;
using System.Collections.Generic;

namespace Chimera {

    class SemanticAnalyzer {

        static readonly IDictionary<TokenCategory, Type> categoryToType =
            new Dictionary<TokenCategory, Type>() {
                { TokenCategory.TRUE, Type.BOOLEAN },
                { TokenCategory.FALSE, Type.BOOLEAN },
                { TokenCategory.BOOL, Type.BOOLEAN },
                
                { TokenCategory.INT_LITERAL, Type.INTEGER},
                { TokenCategory.INTEGER, Type.INTEGER},

                { TokenCategory.STRING, Type.STRING },
                { TokenCategory.STRING_LITERAL, Type.STRING },
            };

        //-----------------------------------------------------------
        static readonly IDictionary<TokenCategory, Type> typeMapper =
            new Dictionary<TokenCategory, Type>() {
                { TokenCategory.BOOL, Type.BOOLEAN },
                { TokenCategory.INTEGER, Type.INTEGER },
                { TokenCategory.STRING, Type.STRING }

            };

        //-----------------------------------------------------------
        public ProcedureTable procedureTable
        {
            get;
            private set;
        }

        public SymbolTable symbolTable {
            get;
            private set;
        }

        //-----------------------------------------------------------
        public SemanticAnalyzer() {
            symbolTable = new SymbolTable();
            procedureTable = new ProcedureTable();
            bool inProcedure = false;
            string procName = "";
            //predefined functions
            procedureTable["WrInt"] = new ProcedureTable.Cell(Type.VOID, true);
            procedureTable["WrInt"].symbolT["i"] = new SymbolTable.Cell(Type.INTEGER, Kind.PARAM, 0);

            procedureTable["WrStr"] = new ProcedureTable.Cell(Type.VOID, true);
            procedureTable["WrStr"].symbolT["s"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, 0);

            procedureTable["WrBool"] = new ProcedureTable.Cell(Type.VOID, true);
            procedureTable["WrBool"].symbolT["b"] = new SymbolTable.Cell(Type.BOOLEAN, Kind.PARAM, 0);

            procedureTable["WrLn"] = new ProcedureTable.Cell(Type.VOID, true);

            procedureTable["RdInt"] = new ProcedureTable.Cell(Type.INTEGER, true);
            procedureTable["RdStr"] = new ProcedureTable.Cell(Type.STRING, true);

            procedureTable["AtStr"] = new ProcedureTable.Cell(Type.STRING, true);
            procedureTable["AtStr"].symbolT["s"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 0);
            procedureTable["AtStr"].symbolT["i"] = new SymbolTable.Cell(Type.INTEGER, Kind.PARAM, pos: 1);

            procedureTable["LenStr"] = new ProcedureTable.Cell(Type.INTEGER, true);
            procedureTable["LenStr"].symbolT["s"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 0);

            procedureTable["CmpStr"] = new ProcedureTable.Cell(Type.INTEGER, true);
            procedureTable["CmpStr"].symbolT["s1"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 0);
            procedureTable["CmpStr"].symbolT["s2"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 1);

            procedureTable["CatStr"] = new ProcedureTable.Cell(Type.STRING, true);
            procedureTable["CatStr"].symbolT["s1"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 0);
            procedureTable["CatStr"].symbolT["s2"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 1);

            procedureTable["LenLstInt"] = new ProcedureTable.Cell(Type.INTEGER, true);
            procedureTable["LenLstInt"].symbolT["loi"] = new SymbolTable.Cell(Type.LIST_OF_INTEGER, Kind.PARAM, pos: 0);

            procedureTable["LenLstStr"] = new ProcedureTable.Cell(Type.INTEGER, true);
            procedureTable["LenLstStr"].symbolT["los"] = new SymbolTable.Cell(Type.LIST_OF_STRING, Kind.PARAM, pos: 0);

            procedureTable["LenLstBool"] = new ProcedureTable.Cell(Type.INTEGER, true);
            procedureTable["LenLstBool"].symbolT["lob"] = new SymbolTable.Cell(Type.LIST_OF_BOOLEAN, Kind.PARAM, pos: 0);

            procedureTable["NewLstInt"] = new ProcedureTable.Cell(Type.LIST_OF_INTEGER, true);
            procedureTable["NewLstInt"].symbolT["size"] = new SymbolTable.Cell(Type.INTEGER, Kind.PARAM, pos: 0);

            procedureTable["NewLstStr"] = new ProcedureTable.Cell(Type.LIST_OF_STRING, true);
            procedureTable["NewLstStr"].symbolT["size"] = new SymbolTable.Cell(Type.INTEGER, Kind.PARAM, pos: 0);

            procedureTable["NewLstBool"] = new ProcedureTable.Cell(Type.LIST_OF_BOOLEAN, true);
            procedureTable["NewLstBool"].symbolT["size"] = new SymbolTable.Cell(Type.INTEGER, Kind.PARAM, pos: 0);

            procedureTable["IntToStr"] = new ProcedureTable.Cell(Type.STRING, true);
            procedureTable["IntToStr"].symbolT["i"] = new SymbolTable.Cell(Type.INTEGER, Kind.PARAM, pos: 0);

            procedureTable["StrToInt"] = new ProcedureTable.Cell(Type.INTEGER, true);
            procedureTable["StrToInt"].symbolT["s"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 0);

        }

        //-----------------------------------------------------------
        public Type Visit(Program node)
        {
            Visit((dynamic)node[0]);
            Visit((dynamic)node[1]);
            Visit((dynamic)node[2]);
            Visit((dynamic)node[3]);
            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(DeclarationList node) {
            VisitChildren(node);
            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(ConstantDeclarationList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }



        //-----------------------------------------------------------
        public Type Visit(Declaration node) {

            var variableName = node[0].AnchorToken.Lexeme;

            if (Table.Contains(variableName)) {
                throw new SemanticError(
                    "Duplicated variable: " + variableName,
                    node[0].AnchorToken);

            } else {
                Table[variableName] = 
                    typeMapper[node.AnchorToken.Category];              
            }

            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(VariableDeclarationList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(ProcedureDeclarationList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(StatementList node) {
            VisitChildren(node);
            return Type.VOID;
        }

        //TRABAJO TOMMY 24/11/2019
        //-----------------------------------------------------------
        public Type Visit(ParameterDeclaration node)
        {   
            var typeOfParamenters = categoryToType[node[1].AnchorToken.Category];
            if (node[1] is SimpleType){ //lista
                switch (node[1].AnchorToken.Category) {
                    case (TokenCategory.INTEGER || TokenCategory.INT_LITERAL):
                        typeOfParamenters = Type.LIST_OF_INTEGER;
                    case (TokenCategory.BOOL || TokenCategory.TRUE || TokenCategory.FALSE):
                        typeOfParamenters = Type.LIST_OF_BOOLEAN;
                    case (TokenCategory.STRING || TokenCategory.STRING_LITERAL):
                        typeOfParamenters = Type.LIST_OF_STRING;
                }
            }

            procedureTable[procName].symbolT[node.AnchorToken.Lexeme] = new SymbolTable.Cell(typeOfParamenters, DateTimeKind.PARAM, pos: 0);

            IEnumerator<Node> listOfIdentifiers = node[0].GetEnumerator();
            var counter = 1;
            foreach(Node identifier in listOfIdentifiers) {
                if(procedureTable[procName].symbolT.Contains(identifier.AnchorToken.Lexeme)){
                    throw new SemanticError(
                        "Duplicated parameter declaration " + identifier.AnchorToken.Lexeme 
                        + " in method " + procName,
                        identifier.AnchorToken);
                }
                procedureTable[procName].symbolT[identifier.AnchorToken.Lexeme] = new SymbolTable.Cell(typeOfParamenters, Kind.PARAM, pos: counter);
                counter ++;
            }
            
            return Type.VOID;
        }

        
        //-----------------------------------------------------------
        public Type Visit(ProcedureDeclaration node) {

            IEnumerator<Node> children = node.GetEnumerator();
            int count = 0;

            
            foreach (Node item in children)
            {
                if(count != 0){
                    Visit((dynamic) item);
                }else{
                    if(procedureTable.Contains(item.AnchorToken.Lexeme)){
                        throw new SemanticError(
                        "Duplicated procedure declaration " + node.AnchorToken.Lexeme,
                        node.AnchorToken);
                    }
                    if(node[2].AnchorToken == null){
                        procedureTable[item.AnchorToken.Lexeme] = new ProcedureTable.Cell(Type.VOID, true);
                    } else {
                        var typeOfProc = categoryToType[item.AnchorToken.Category];
                        if (item is SimpleType){ //lista
                            switch (item.AnchorToken.Category) {
                                case (TokenCategory.INTEGER || TokenCategory.INT_LITERAL):
                                    typeOfParamenters = Type.LIST_OF_INTEGER;
                                case (TokenCategory.BOOL || TokenCategory.TRUE || TokenCategory.FALSE):
                                    typeOfParamenters = Type.LIST_OF_BOOLEAN;
                                case (TokenCategory.STRING || TokenCategory.STRING_LITERAL):
                                    typeOfParamenters = Type.LIST_OF_STRING;
                            }
                        }
                        procedureTable[item.AnchorToken.Lexeme] = new ProcedureTable.Cell(typeOfProc, false);
                    }
                    procName = item.AnchorToken.Lexeme;
                    inProcedure = true;
                    
                }
                count++;
            }
            procName = "";
            inProcedure = false;
            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(ConstDeclaration node)
        {
            if(inProcedure){
                if(procedureTable[procName].symbolT.Contains(node[0].AnchorToken.Lexeme)){
                    throw new SemanticError(
                        "Duplicated const or variable declaration " + node[0].AnchorToken.Lexeme 
                        + " in method " + procName,
                        node[0].AnchorToken);
                }
                procedureTable[procName].symbolT[node[0].AnchorToken.Lexeme] = new SymbolTable.Cell(Visit((dynamic) node[1]), Kind.CONST, pos: 0);
            } else {
                if(symbolTable.Contains(node[0].AnchorToken.Lexeme)) {
                    throw new SemanticError(
                        "Duplicated const or variable declaration " + node[0].AnchorToken.Lexeme,
                        node[0].AnchorToken);
                }
                symbolTable[node[0].AnchorToken.Lexeme] = new SymbolTable.Cell(Visit((dynamic) node[1]), Kind.CONST, pos: 0);
            }
            return Type.VOID;
        }
        
        //-----------------------------------------------------------
        public Type Visit(List node) {
            Type listType = Visit((dynamic) node[0]);
            IEnumerator<Node> types = node.GetEnumerator();
            foreach (Node type in types) {
                Type nodeType = Visit((dynamic) type);
                if(nodeType != listType) {
                    throw new SemanticError("Invalid type: " + nodeType, type.AnchorToken);
                }
            }

            switch (listType) {
                    case (TokenCategory.INT_LITERAL):
                        return Type.LIST_OF_INTEGER;
                    case (TokenCategory.TRUE || TokenCategory.FALSE):
                        return Type.LIST_OF_BOOLEAN;
                    case (TokenCategory.STRING_LITERAL):
                        return Type.LIST_OF_STRING;
                }
        }

        //-----------------------------------------------------------
        public Type Visit(SimpleLiteral node) {
            return categoryToType[node.AnchorToken.Category];
        }

        //-----------------------------------------------------------
        public Type Visit(Type node) {
            Type nodeType = categoryToType[node.AnchorToken.Category];
            if(node is SimpleType){ //list
                switch (node.AnchorToken.Category) {
                    case (TokenCategory.INTEGER || TokenCategory.INT_LITERAL):
                        nodeType = Type.LIST_OF_INTEGER;
                    case (TokenCategory.BOOL || TokenCategory.TRUE || TokenCategory.FALSE):
                        nodeType = Type.LIST_OF_BOOLEAN;
                    case (TokenCategory.STRING || TokenCategory.STRING_LITERAL):
                        nodeType = Type.LIST_OF_STRING;
                }
            }
            return nodeType;
        }

        //-----------------------------------------------------------
        public Type Visit(ListType node) {
            switch (node.AnchorToken.Category) {
                case (TokenCategory.INTEGER || TokenCategory.INT_LITERAL):
                    return Type.LIST_OF_INTEGER;
                case (TokenCategory.BOOL || TokenCategory.TRUE || TokenCategory.FALSE):
                    return Type.LIST_OF_BOOLEAN;
                case (TokenCategory.STRING || TokenCategory.STRING_LITERAL):
                    return Type.LIST_OF_STRING;
            }
        }

        //-----------------------------------------------------------
        public Type Visit(SimpleType node) {
            return categoryToType[node.Category];
        }

        //-----------------------------------------------------------
        public Type Visit(VariableDeclaration node) {
            Type typeOfVariables = Visit((dynamic) node[0]);

            IEnumerator<Node> variablesNames = node[1].GetEnumerator();
            int count = 0;
            foreach(Node variable in variablesNames){
                if(inProcedure){
                    if(procedureTable[procName].symbolT.Contains(variable.AnchorToken.Lexeme)){
                        throw new SemanticError(
                            "Duplicated variable declaration " + variable.AnchorToken.Lexeme 
                            + " in method " + procName,
                            variable.AnchorToken);
                    }
                    procedureTable[procName].symbolT[variable.AnchorToken.Lexeme] = new SymbolTable.Cell(typeOfVariables, Kind.VAR, pos: count);
                } else {
                    if(symbolTable.Contains(variable.AnchorToken.Lexeme)) {
                        throw new SemanticError(
                            "Duplicated const or variable declaration " + variable.AnchorToken.Lexeme,
                            variable.AnchorToken);
                    }
                    symbolTable[variable.AnchorToken.Lexeme] = new SymbolTable.Cell(typeOfVariables, Kind.VAR, pos: count);
                }
                count++;
            }

            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(Assignment node) {

            var variableName = node.AnchorToken.Lexeme;

            if (Table.Contains(variableName)) {

                var expectedType = Table[variableName];

                if (expectedType != Visit((dynamic) node[0])) {
                    throw new SemanticError(
                        "Expecting type " + expectedType 
                        + " in assignment statement",
                        node.AnchorToken);
                }

            } else {
                throw new SemanticError(
                    "Undeclared variable: " + variableName,
                    node.AnchorToken);
            }

            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(AssignmentCallStatement node)
        {
            Visit((dynamic)node[0]);
            Visit((dynamic)node[1]);
            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(ExpressionList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(Print node) {
            node.ExpressionType = Visit((dynamic) node[0]);
            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(If node) {
            if (Visit((dynamic) node[0]) != Type.BOOL) {
                throw new SemanticError(
                    "Expecting type " + Type.BOOL 
                    + " in conditional statement",                   
                    node.AnchorToken);
            }
            VisitChildren(node[1]);
            return Type.VOID;
        }  

        //-----------------------------------------------------------
        public Type Visit(Identifier node) {

            //nombre
            var variableName = node.AnchorToken.Lexeme;

            if (symbolTable.Contains(variableName)) {
                return Table[variableName];
            }

            throw new SemanticError(
                "Undeclared variable: " + variableName,
                node.AnchorToken);
        }

        //-----------------------------------------------------------
        public Type Visit(IntLiteral node) {

            var intStr = node.AnchorToken.Lexeme;

            try {
                Convert.ToInt32(intStr);

            } catch (OverflowException) {
                throw new SemanticError(
                    "Integer literal too large: " + intStr, 
                    node.AnchorToken);
            }

            return Type.INT;
        }

        //-----------------------------------------------------------
        public Type Visit(True node) {
            return Type.BOOL;
        }

        //-----------------------------------------------------------
        public Type Visit(False node) {
            return Type.BOOL;
        }

        //-----------------------------------------------------------
        public Type Visit(Neg node) {          
            if (Visit((dynamic) node[0]) != Type.INT) {
                throw new SemanticError(
                    "Operator - requires an operand of type " + Type.INT,
                    node.AnchorToken);
            }
            return Type.INT;
        }

        //-----------------------------------------------------------
        public Type Visit(And node)
        {
            VisitBinaryOperator("and", node, Type.BOOL);
            return Type.BOOL;
        }

        //-----------------------------------------------------------
        public Type Visit(Or node)
        {
            VisitBinaryOperator("or", node, Type.BOOL);
            return Type.BOOL;
        }

        //-----------------------------------------------------------
        public Type Visit(Xor node)
        {
            VisitBinaryOperator("xor", node, Type.BOOL);
            return Type.BOOL;
        }

        //-----------------------------------------------------------
        public Type Visit(AssignOperator node)
        {
            if (Visit((dynamic)node[0]) == Type.INT)
            {
                VisitBinaryOperator("=", node, Type.INT);
            }
            else
            {
                VisitBinaryOperator("=", node, Type.BOOL);
            }

            return Type.BOOL;
        }

        //-----------------------------------------------------------
        public Type Visit(LessMoreOperator node)
        {
            if (Visit((dynamic)node[0]) == Type.INT)
            {
                VisitBinaryOperator("<>", node, Type.INT);
            }
            else
            {
                VisitBinaryOperator("<>", node, Type.BOOL);
            }

            return Type.BOOL;
        }

        //-----------------------------------------------------------
        public Type Visit(LessOperator node)
        {
            VisitBinaryOperator("<", node, Type.INT);
            return Type.BOOL;
        }

        //-----------------------------------------------------------
        public Type Visit(LessEqualOperator node)
        {
            VisitBinaryOperator("<=", node, Type.INT);
            return Type.BOOL;
        }

        //-----------------------------------------------------------
        public Type Visit(MoreOperator node)
        {
            VisitBinaryOperator(">", node, Type.INT);
            return Type.BOOL;
        }

        //-----------------------------------------------------------
        public Type Visit(MoreEqualOperator node)
        {
            VisitBinaryOperator(">=", node, Type.INT);
            return Type.BOOL;
        }

        //-----------------------------------------------------------
        public Type Visit(AdditionOperator node) {
            VisitBinaryOperator("+", node, Type.INT);
            return Type.INT;
        }

        //-----------------------------------------------------------
        public Type Visit(SubstractionOperator node)
        {
            VisitBinaryOperator("-", node, Type.INT);
            return Type.INT;
        }

        //-----------------------------------------------------------
        public Type Visit(MultiplicantOperator node) {
            VisitBinaryOperator("*", node, Type.INT);
            return Type.INT;
        }

        //-----------------------------------------------------------
        void VisitChildren(Node node) {
            foreach (var n in node) {
                Visit((dynamic) n);
            }
        }

        //-----------------------------------------------------------
        void VisitBinaryOperator(string op, Node node, Type type) {
            if (Visit((dynamic) node[0]) != type || 
                Visit((dynamic) node[1]) != type) {
                throw new SemanticError(
                    String.Format(
                        "Operator {0} requires two operands of type {1}",
                        op, 
                        type),
                    node.AnchorToken);
            }
        }
    }
}
