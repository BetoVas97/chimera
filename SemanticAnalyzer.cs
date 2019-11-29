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
                { TokenCategory.TRUE, Type.BOOL },
                { TokenCategory.FALSE, Type.BOOL },
                { TokenCategory.BOOL, Type.BOOL },
                
                { TokenCategory.INT_LITERAL, Type.INT},
                { TokenCategory.INTEGER, Type.INT},

                { TokenCategory.STRING, Type.STRING },
                { TokenCategory.STRING_LITERAL, Type.STRING },
            };

        //-----------------------------------------------------------
        static readonly IDictionary<TokenCategory, Type> typeMapper =
            new Dictionary<TokenCategory, Type>() {
                { TokenCategory.BOOL, Type.BOOL },
                { TokenCategory.INTEGER, Type.INT },
                { TokenCategory.STRING, Type.STRING }

            };

        //-----------------------------------------------------------
        public ProcedureTable procedureTable
        {
            get;
            private set;
            
        }

        public bool inProcedure{
            get;
            private set;
        }
        public bool inCycle{
            get;
            private set;
        }

        public string procName{
            get;
            private set;
        }

        //globalTable
        public SymbolTable symbolTable {
            get;
            private set;
        }

        public SymbolTable localTable{
            get; 
            private set;
        }

        //-----------------------------------------------------------
        public SemanticAnalyzer() {
            symbolTable = new SymbolTable();
            procedureTable = new ProcedureTable();
            localTable=new SymbolTable();
            inCycle=false;
            inProcedure=false;
            procName="";
            
            //predefined functions
            procedureTable["WrInt"] = new ProcedureTable.Cell(Type.VOID, true,1, new List<Type>());
            procedureTable["WrInt"].symbolT["i"] = new SymbolTable.Cell(Type.INT, Kind.PARAM, 0);

            procedureTable["WrStr"] = new ProcedureTable.Cell(Type.VOID, true,1,new List<Type>());
            procedureTable["WrStr"].symbolT["s"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, 0);

            procedureTable["WrBool"] = new ProcedureTable.Cell(Type.VOID, true,1,new List<Type>());
            procedureTable["WrBool"].symbolT["b"] = new SymbolTable.Cell(Type.BOOL, Kind.PARAM, 0);

            procedureTable["WrLn"] = new ProcedureTable.Cell(Type.VOID, true,0,new List<Type>());

            procedureTable["RdInt"] = new ProcedureTable.Cell(Type.INT, true,0,new List<Type>());
            procedureTable["RdStr"] = new ProcedureTable.Cell(Type.STRING, true,0,new List<Type>());

            procedureTable["AtStr"] = new ProcedureTable.Cell(Type.STRING, true,2,new List<Type>());
            procedureTable["AtStr"].symbolT["s"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 0);
            procedureTable["AtStr"].symbolT["i"] = new SymbolTable.Cell(Type.INT, Kind.PARAM, pos: 1);

            procedureTable["LenStr"] = new ProcedureTable.Cell(Type.INT, true,1,new List<Type>());
            procedureTable["LenStr"].symbolT["s"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 0);

            procedureTable["CmpStr"] = new ProcedureTable.Cell(Type.INT, true,2,new List<Type>());
            procedureTable["CmpStr"].symbolT["s1"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 0);
            procedureTable["CmpStr"].symbolT["s2"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 1);

            procedureTable["CatStr"] = new ProcedureTable.Cell(Type.STRING, true,2,new List<Type>());
            procedureTable["CatStr"].symbolT["s1"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 0);
            procedureTable["CatStr"].symbolT["s2"] = new SymbolTable.Cell(Type.STRING, Kind.PARAM, pos: 1);

            procedureTable["LenLstInt"] = new ProcedureTable.Cell(Type.INT, true,1,new List<Type>());
            procedureTable["LenLstInt"].symbolT["loi"] = new SymbolTable.Cell(Type.LIST_OF_INT, Kind.PARAM, pos: 0);

            procedureTable["LenLstStr"] = new ProcedureTable.Cell(Type.INT, true,1,new List<Type>());
            procedureTable["LenLstStr"].symbolT["los"] = new SymbolTable.Cell(Type.LIST_OF_STRING, Kind.PARAM, pos: 0);

            procedureTable["LenLstBool"] = new ProcedureTable.Cell(Type.INT, true,1,new List<Type>());
            procedureTable["LenLstBool"].symbolT["lob"] = new SymbolTable.Cell(Type.LIST_OF_BOOL, Kind.PARAM, pos: 0);

            procedureTable["NewLstInt"] = new ProcedureTable.Cell(Type.LIST_OF_INT, true,1,new List<Type>());
            procedureTable["NewLstInt"].symbolT["size"] = new SymbolTable.Cell(Type.INT, Kind.PARAM, pos: 0);

            procedureTable["NewLstStr"] = new ProcedureTable.Cell(Type.LIST_OF_STRING, true,1,new List<Type>());
            procedureTable["NewLstStr"].symbolT["size"] = new SymbolTable.Cell(Type.INT, Kind.PARAM, pos: 0);

            procedureTable["NewLstBool"] = new ProcedureTable.Cell(Type.LIST_OF_BOOL, true,1,new List<Type>());
            procedureTable["NewLstBool"].symbolT["size"] = new SymbolTable.Cell(Type.INT, Kind.PARAM, pos: 0);

            procedureTable["IntToStr"] = new ProcedureTable.Cell(Type.STRING, true,1,new List<Type>());
            procedureTable["IntToStr"].symbolT["i"] = new SymbolTable.Cell(Type.INT, Kind.PARAM, pos: 0);

            procedureTable["StrToInt"] = new ProcedureTable.Cell(Type.INT, true,1,new List<Type>());
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

        //-----------------------------------------------------------
        public Type Visit(ExpressionList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type SingleType(Type type){
            if(type == Type.LIST_OF_INT){
                return Type.INT;
            }
            else if (type == Type.LIST_OF_BOOL){
                return Type.BOOL;
            }
            else{
                return Type.STRING;
            }
        }

        /*public Type Visit(Identifier node)
        {
            //obtenemos el nombre del identificador
            var variableName = node.AnchorToken.Lexeme;

            //si estamos en un procedimiento
            if (inProcedure)
            {
                //si no esta en ninguna tabla, no existe la variable
                if (!TableTemporal.Contains(variableName) && !Table.Contains(variableName))
                    throw new SemanticError("Undeclared variable: " + variableName, node[0].AnchorToken);

                //si existe en la tabla local
                if (TableTemporal.Contains(variableName))
                {
                    //obtenemos el tipo
                    Type expectedType = TableTemporal[variableName].tipo;

                    return expectedType;
                }
            }
            //si no es un procedimiento o estamos en un procedimiento pero la tabla temporal no la contiene
            if ((procedure && !TableTemporal.Contains(variableName)) || !procedure)
            {
                //si la tabla global no la contiene es error
                if (!Table.Contains(variableName))
                    throw new SemanticError("Undeclared variable: " + variableName, node[0].AnchorToken);

                //si la tabla la contiene
                if (Table.Contains(variableName))
                {
                    //obtenemos el tipo
                    Type expectedType = Table[variableName].tipo;

                    return expectedType;
                }
            }
            return Type.VOID;

        }*/

        //Tommy
        public Type Visit(SimpleLiteralList node) {
            VisitChildren(node);
            return Type.VOID;
        }
        //-----------------------------------------------------------
        public Type Visit(ParameterDeclaration node)
        {   
            var typeOfParamenters = categoryToType[node[1].AnchorToken.Category];
            if (node[1] is SimpleType){ //lista
                switch (node[1].AnchorToken.Category) {
                    case (TokenCategory.INTEGER):
                        typeOfParamenters = Type.LIST_OF_INT;
                        break;
                    case (TokenCategory.INT_LITERAL):
                        typeOfParamenters = Type.LIST_OF_INT;
                        break;
                    case (TokenCategory.BOOL):
                        typeOfParamenters = Type.LIST_OF_BOOL;
                        break;
                    case (TokenCategory.FALSE):
                        typeOfParamenters = Type.LIST_OF_BOOL;
                        break;
                    case (TokenCategory.TRUE):
                        typeOfParamenters = Type.LIST_OF_BOOL;
                        break;
                    case (TokenCategory.STRING ):
                        typeOfParamenters = Type.LIST_OF_STRING;
                        break;
                    case (TokenCategory.STRING_LITERAL):
                        typeOfParamenters = Type.LIST_OF_STRING;
                        break;
                    default:
                        throw new SemanticError("None of this cases",node.AnchorToken);
                        break;
                }
            }

            procedureTable[procName].symbolT[node.AnchorToken.Lexeme] = new SymbolTable.Cell(typeOfParamenters, Kind.PARAM, pos: 0);

            //IEnumerator<Node> listOfIdentifiers = node[0].GetEnumerator();
            var counter = 1;
            foreach(Node identifier in node[0] ){
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

            //IEnumerator<Node> children = node.GetEnumerator();
            int count = 0;

            
            foreach (Node item in node)
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
                        List<Type> listOfTypes = new List<Type>();
                        int countProc = 0;
                        foreach (var parameter in node[1])
                        {
                            Type typeOfParameter = Visit((dynamic) parameter);
                            listOfTypes.Add(typeOfParameter);
                            countProc++;
                        }
                        procedureTable[item.AnchorToken.Lexeme] = new ProcedureTable.Cell(Type.VOID, true, countProc, listOfTypes);                    } else {
                        var typeOfProc = categoryToType[item.AnchorToken.Category];
                        if (item is SimpleType){ //lista
                            switch (item.AnchorToken.Category) {
                                case (TokenCategory.INTEGER):
                                typeOfProc = Type.LIST_OF_INT;
                                break;
                            case (TokenCategory.INT_LITERAL):
                                typeOfProc = Type.LIST_OF_INT;
                                break;
                            case (TokenCategory.BOOL):
                                typeOfProc = Type.LIST_OF_BOOL;
                                break;
                            case (TokenCategory.FALSE):
                                typeOfProc = Type.LIST_OF_BOOL;
                                break;
                            case (TokenCategory.TRUE):
                                typeOfProc = Type.LIST_OF_BOOL;
                                break;
                            case (TokenCategory.STRING ):
                                typeOfProc = Type.LIST_OF_STRING;
                                break;
                            case (TokenCategory.STRING_LITERAL):
                                typeOfProc = Type.LIST_OF_STRING;
                                break;
                            default:
                                throw new SemanticError("None of this cases",node.AnchorToken);
                                break;
                            }
                        }

                        List<Type> listOfTypes = new List<Type>();
                        int countProc = 0;
                        foreach (var parameter in node[1])
                        {
                            Type typeOfParameter = Visit((dynamic) parameter);
                            listOfTypes.Add(typeOfParameter);
                            countProc++;
                        }
                        
                        procedureTable[item.AnchorToken.Lexeme] = new ProcedureTable.Cell(typeOfProc, false, countProc, listOfTypes);
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
            Type listType = Visit((dynamic) node[0][0]);
            //IEnumerator<Node> types = node.GetEnumerator();
            foreach (Node type in node[0]) {
                Type nodeType = Visit((dynamic) type);
                if(nodeType != listType) {
                    throw new SemanticError("Invalid type: " + nodeType, type.AnchorToken);
                }
            }

            var listCategory = node[0][0].AnchorToken.Category;

            switch (listCategory) {
                    case (TokenCategory.INT_LITERAL):
                        return Type.LIST_OF_INT;
                        break;
                    case (TokenCategory.FALSE):
                        return Type.LIST_OF_BOOL;
                        break;
                    case (TokenCategory.TRUE):
                        return Type.LIST_OF_BOOL;
                        break;
                    case (TokenCategory.STRING_LITERAL):
                        return Type.LIST_OF_STRING;
                        break;
                    default:
                        throw new SemanticError("None of this cases",node.AnchorToken);
                        break;
                }
        }

        //-----------------------------------------------------------
        public Type Visit(SimpleLiteral node) {
            return categoryToType[node.AnchorToken.Category];
        }

        //-----------------------------------------------------------
        public Type Visit(IntegerLiteral node) {
            var intStr = node.AnchorToken.Lexeme;

            try {
                Convert.ToInt32(intStr);

            } catch (OverflowException) {
                throw new SemanticError(
                    "Integer literal too large: " + intStr, 
                    node.AnchorToken);
            }
            return categoryToType[node.AnchorToken.Category];
        }

        //-----------------------------------------------------------
        public Type Visit(StringLiteral node) {
            return categoryToType[node.AnchorToken.Category];
        }

        //-----------------------------------------------------------
        public Type Visit(TrueLiteral node) {
            return categoryToType[node.AnchorToken.Category];
        }

        //-----------------------------------------------------------
        public Type Visit(FalseLiteral node) {
            return categoryToType[node.AnchorToken.Category];
        }


        //-----------------------------------------------------------
        public Type Visit(TypeNode node) {
            Type nodeType = categoryToType[node.AnchorToken.Category];
            if(node is SimpleType){ //list
                switch (node.AnchorToken.Category) {
                    case (TokenCategory.INTEGER):
                        nodeType = Type.LIST_OF_INT;
                        break;
                    case (TokenCategory.BOOL):
                        nodeType = Type.LIST_OF_BOOL;
                        break;
                    case (TokenCategory.STRING):
                        nodeType = Type.LIST_OF_STRING;
                        break;
                    case (TokenCategory.INT_LITERAL):
                        nodeType = Type.LIST_OF_INT;
                        break;
                    case (TokenCategory.TRUE):
                        nodeType = Type.LIST_OF_BOOL;
                        break;
                    case (TokenCategory.STRING_LITERAL):
                        nodeType = Type.LIST_OF_STRING;
                        break;
                    case (TokenCategory.FALSE):
                        nodeType = Type.LIST_OF_BOOL;
                        break;
                    default:
                        throw new SemanticError("None of this cases",node.AnchorToken);
                        break;

                }
            }
            return nodeType;
        }
        //-----------------------------------------------------------
        public Type Visit(SimpleType node) {
            return categoryToType[node.AnchorToken.Category];
        }

        //-----------------------------------------------------------
        public Type Visit(VariableDeclaration node) {
            Type typeOfVariables = Visit((dynamic) node[0]);

            //IEnumerator<Node> variablesNames = node[1].GetEnumerator();
            int count = 0;
            foreach(Node variable in node[1]){
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
        public Type Visit(If node)
        {
            //Revisar expression list (El primero es del if y todos los siguientes son del elseif)
            //IEnumerator<Node> statements = node[3].GetEnumerator();
            foreach (var statement in node[3]) {
                Type typeOfStatement = Visit((dynamic)statement);
                if(typeOfStatement != Type.BOOL){
                    throw new SemanticError(
                            "Wrong type in if-elseif, expected a boolean, but received:  " + typeOfStatement ,
                            statement.AnchorToken);
                }
            }

            Visit((dynamic)node[0]);
            Visit((dynamic)node[1]);
            Visit((dynamic)node[2]);
            return Type.VOID;
        }
        //-----------------------------------------------------------
        public Type Visit(NegationOperator node)
        {
            VisitUnaryOperator("-",node,Type.INT);
            return Type.INT;
        }

        //-----------------------------------------------------------
        public Type Visit(NotOperator node)
        {
            VisitUnaryOperator("-",node,Type.BOOL);
            return Type.BOOL;
        }

        //Beto
        //-----------------------------------------------------------
        public Type Visit(AssignmentStatement node){
            var varName=node.AnchorToken.Lexeme;

            if(inProcedure){
                if(!procedureTable.Contains(varName) && !symbolTable.Contains(varName)){
                    throw new SemanticError("Undeclared variables: "+varName,node.AnchorToken); 
                }
                if(symbolTable.Contains(varName)){
                    if(symbolTable[varName].kind == Kind.CONST){
                        throw new SemanticError("Constant: "+varName+" already defined",node.AnchorToken);
                    }
                    Type expectType = symbolTable[varName].type;
                    
                    if(expectType!=Visit((dynamic)node[0])){
                        throw new SemanticError("Expecting: "+expectType+" type in assignment statement", node.AnchorToken);
                    }
                }
            }

            if ((inProcedure && !symbolTable.Contains(varName)) || !inProcedure)
            {
                if (!symbolTable.Contains(varName))
                    throw new SemanticError("Undeclared variable: " + varName, node.AnchorToken);

                if (symbolTable[varName].kind == Kind.CONST)
                    throw new SemanticError("Constant: '" + varName + "already defined ", node.AnchorToken);

                Type expectType = symbolTable[varName].type;

                if (expectType != Visit((dynamic)node[0]))
                    throw new SemanticError("Expecting: " + expectType + " type in assignment statement", node.AnchorToken);
            }
            return Type.VOID;

        }
        //-----------------------------------------------------------
        public Type Visit(CallStatement node){
            var procName=node.AnchorToken.Lexeme;
            //Not procedure declared
            if(!procedureTable.Contains(procName)){
                throw new SemanticError("Procedure: "+procName+" undeclared",node.AnchorToken);
            }

            //Declared then bring name
            var procedure=procedureTable[procName];
            if(procedureTable[procName].type != Type.VOID){
                throw new SemanticError("Return value must be VOID but is : " + procedure.type, node.AnchorToken);
            }

            //Store
            List<Type> list=new List<Type>();

            string tempLst = "";
            string obj = "";
            //Iterate between nodes, parameters
            foreach (var n in node)
            {
                var temp = Visit((dynamic)n);
                list.Add(temp);
                tempLst += temp + ","; //addString
            }

            //Iterate  between 
            foreach (var n in procedureTable[procName].orderParameter)
            {
                obj += n + ","; //add to string
            }

            // Manage and delete last ","
            if (obj.Length > 0)
            {
                tempLst = tempLst.Substring(0, tempLst.Length - 1);
                obj = obj.Substring(0, obj.Length - 1);
            }

            //Does count match?
            if (list.Count != procedure.noParameters)
                throw new SemanticError("Wrong number of parameters: " + list.Count + " expecting: " + procedure.noParameters, node.AnchorToken);

            //Si las cadenas son diferentes, se recibieron parametros en un orden erroneo
            if (tempLst != obj)
                throw new SemanticError("Wrong order of parameters: " + tempLst + " expecting: " + obj, node.AnchorToken);

            return procedure.type;
            
        }

        public Type Visit(Call node){
            var procName=node.AnchorToken.Lexeme;
            //Not procedure declared
            if(!procedureTable.Contains(procName)){
                throw new SemanticError("Procedure: "+procName+" undeclared",node.AnchorToken);
            }

            //Declared then bring name
            var procedure=procedureTable[procName];
            if(procedureTable[procName].type != Type.VOID){
                throw new SemanticError("Return value must be VOID but is : " + procedure.type, node.AnchorToken);
            }

            //Store
            List<Type> list=new List<Type>();

            string tempLst = "";
            string obj = "";
            //Iterate between nodes, parameters
            foreach (var n in node)
            {
                var temp = Visit((dynamic)n);
                list.Add(temp);
                tempLst += temp + ","; //addString
            }

            //Iterate  between 
            foreach (var n in procedureTable[procName].orderParameter)
            {
                obj += n + ","; //add to string
            }

            // Manage and delete last ","
            if (obj.Length > 0)
            {
                tempLst = tempLst.Substring(0, tempLst.Length - 1);
                obj = obj.Substring(0, obj.Length - 1);
            }

            //Does count match?
            if (list.Count != procedure.noParameters)
                throw new SemanticError("Wrong number of parameters: " + list.Count + " expecting: " + procedure.noParameters, node.AnchorToken);

            //Si las cadenas son diferentes, se recibieron parametros en un orden erroneo
            if (tempLst != obj)
                throw new SemanticError("Wrong order of parameters: " + tempLst + " expecting: " + obj, node.AnchorToken);

            return procedure.type;

        }



        //-----------------------------------------------------------
        public Type Visit(For node){
            inCycle=true;

            //Type of for
            var typeCycle=Visit((dynamic)node[0]);
            var typeListC=Visit((dynamic)node[1]); //Type of list for

            var listType=SingleType(typeListC);//Extract single type of list

            if (listType!=typeCycle){
                throw new SemanticError("Type of var must be: "+listType+" since the expression is of type"+typeCycle,node[0].AnchorToken);
            }
            if(typeListC!=Type.LIST_OF_INT || typeListC!= Type.LIST_OF_STRING || typeListC!= Type.LIST_OF_BOOL){
                throw new SemanticError("Expresion type must be of List type. No: "+typeCycle,node[0].AnchorToken);
            } 

            Visit((dynamic)node[2]); //Now visit the statements
            inCycle=false;
            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(Loop node){
            inCycle=true;
            Visit((dynamic)node[0]); //Now visit the statements
            inCycle=false;
            return Type.VOID;
        }
        //-----------------------------------------------------------
        public Type Visit(Exit node) {          
            if (inCycle == false) {
                throw new SemanticError("Exit statement just for loops ",node.AnchorToken);
            }
            return Type.VOID;
        }

        //-----------------------------------------------------------
        public Type Visit(Return node) {          
            try{
                Visit((dynamic)node[0]); //if return ; just visit.
            }
            //Check when it's void
            catch(ArgumentOutOfRangeException){
                if(inProcedure==true){
                    var procedureType= procedureTable[procName].type;
                    if(procedureType!=Type.VOID){
                        throw new SemanticError("Return exp and procedure's type: "+procedureType+"must match but return exp is: VOID", node.AnchorToken);
                    }
                }
                else{ 
                    return Type.VOID; 
                }
            }
            //Now check other types that are not VOID
            var type= Visit((dynamic)node[0]);
            if(inProcedure==true){
                var procedureType= procedureTable[procName].type;
                if(procedureType!=type){
                    throw new SemanticError("Return exp and procedure's type: "+procedureType+"must match but return is: "+type, node.AnchorToken);
                }
            }
            else if(inProcedure==false){ 
                throw new SemanticError("Return exp type: "+type+"not valid outside a procedure",node.AnchorToken);
            
            }
            //else{
                return type;
            //}
        }

        //--------------------------------------------------
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

        void VisitUnaryOperator(string op, Node node, Type type)
        {
            if (Visit((dynamic)node[0]) != type)
            {
                throw new SemanticError(
                    String.Format(
                        "Operator {0} requires anf operand of type {1}",
                        op,
                        type),
                    node[0].AnchorToken);
            }
        }
    
    }
}
