/*
  Chimera compiler - SemanticAnalyzer.
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358
*/

using System;
using System.Collections.Generic;

namespace Chimera
{
    class SemanticAnalyzer
    {
        static readonly IDictionary<TokenCategory, Type> typeMapper =
            new Dictionary<TokenCategory, Type>() {
                { TokenCategory.TRUE, Type.BOOL },
                { TokenCategory.FALSE, Type.BOOL },
                { TokenCategory.INT_LITERAL, Type.INT},
                { TokenCategory.STR_LITERAL, Type.STRING },
                { TokenCategory.BRACE_OPEN, Type.LIST },
                { TokenCategory.STRING, Type.STRING },
                { TokenCategory.INTEGER, Type.INT },
                { TokenCategory.BOOLEAN, Type.BOOL },
                { TokenCategory.LIST, Type.LISTVAR }
            };


        //Global bools and ints requierded later
        public bool inProcedure;
        public bool isParam;
        public int numParams;
        public List<dynamic> listOrderParams;
        public List<string> listOrderParamsName;
        public int inCycle; //because we can have loop inside loop
        public string procName;

        public ProcedureTable procedureTable
        {
            get;
            private set;
        }

        public SymbolTable symbolTable
        {
            get;
            private set;
        }

        public IDictionary<string, SymbolTable> localSTables
        {
            get;
            private set;
        }
    
        public SymbolTable LocalSTable
        {
            get;
            private set;
        }


        public SemanticAnalyzer()
        {
            procName = "";
            inCycle = 0;
            numParams = 0;
            listOrderParams = new List<dynamic>();
            listOrderParamsName = new List<string>();
            inProcedure = false;
            isParam = false;
            symbolTable = new SymbolTable();
            LocalSTable = new SymbolTable();
            procedureTable = new ProcedureTable();
            localSTables = new SortedDictionary<string, SymbolTable>();
            
            string predefined = "P";
            SymbolTable sTable = new SymbolTable();
            procedureTable["WrInt"] = new Procedures(Type.VOID, 1, predefined, new List<dynamic>() { Type.INT }, sTable, new List<string>() {"i"});
            procedureTable["WrStr"] = new Procedures(Type.VOID, 1, predefined, new List<dynamic>() { Type.STRING }, sTable, new List<string>() {"i"});
            procedureTable["WrBool"] = new Procedures(Type.VOID, 1, predefined, new List<dynamic>() { Type.BOOL }, sTable, new List<string>() {"i"});
            procedureTable["WrLn"] = new Procedures(Type.VOID, 0, predefined, new List<dynamic>(), sTable, new List<string>() {"i"});
            procedureTable["RdInt"] = new Procedures(Type.INT, 0, predefined, new List<dynamic>(), sTable, new List<string>() {"i"});
            procedureTable["RdStr"] = new Procedures(Type.STRING, 0, predefined, new List<dynamic>(), sTable, new List<string>() {"i"});
            procedureTable["AtStr"] = new Procedures(Type.STRING, 2, predefined, new List<dynamic>() { Type.STRING, Type.INT }, sTable, new List<string>() {"i"});
            procedureTable["LenStr"] = new Procedures(Type.INT, 1, predefined, new List<dynamic>() { Type.STRING }, sTable, new List<string>() {"i"});
            procedureTable["CmpStr"] = new Procedures(Type.INT, 2, predefined, new List<dynamic>() { Type.STRING, Type.STRING }, sTable, new List<string>() {"i"});
            procedureTable["CatStr"] = new Procedures(Type.STRING, 2, predefined, new List<dynamic>() { Type.STRING, Type.STRING }, sTable, new List<string>() {"i"});
            procedureTable["LenLstInt"] = new Procedures(Type.INT, 1, predefined, new List<dynamic>() { Type.LIST_OF_INT }, sTable, new List<string>() {"i"});
            procedureTable["LenLstStr"] = new Procedures(Type.INT, 1, predefined, new List<dynamic>() { Type.LIST_OF_STRING }, sTable, new List<string>() {"i"});
            procedureTable["LenLstBool"] = new Procedures(Type.INT, 1, predefined, new List<dynamic>() { Type.LIST_OF_BOOL }, sTable, new List<string>() {"i"});
            procedureTable["NewLstInt"] = new Procedures(Type.LIST_OF_INT, 1, predefined, new List<dynamic>() { Type.INT }, sTable, new List<string>() {"i"});
            procedureTable["NewLstStr"] = new Procedures(Type.LIST_OF_STRING, 1, predefined, new List<dynamic>() { Type.INT }, sTable, new List<string>() {"i"});
            procedureTable["NewLstBool"] = new Procedures(Type.LIST_OF_BOOL, 1, predefined, new List<dynamic>() { Type.INT }, sTable, new List<string>() {"i"});
            procedureTable["IntToStr"] = new Procedures(Type.STRING, 1, predefined, new List<dynamic>() { Type.INT }, sTable, new List<string>() {"i"});
            procedureTable["StrToInt"] = new Procedures(Type.INT, 1, predefined, new List<dynamic>() { Type.STRING }, sTable, new List<string>() {"i"});
        }

        public Type Visit(Program node)
        {
            Visit((dynamic)node[0]); //Constants
            Visit((dynamic)node[1]); //Variables
            Visit((dynamic)node[2]); //Procs
            Visit((dynamic)node[3]); //Statements
            return Type.VOID;
        }

        public Type Visit(StatementList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(Exit node)
        {
            if (inCycle==0){
                throw new SemanticError("Exit statement is just allowed inside a loop", node.AnchorToken);
            }
            return Type.VOID;
        }

        public Type Visit(Return node)
        {
            try
            {
                Visit((dynamic)node[0]);
            }
            catch (ArgumentOutOfRangeException)
            {
                if (inProcedure)
                {
                    var procedureType = procedureTable[procName].returnedType;
                    if (procedureType != Type.VOID){
                        throw new SemanticError("Return exp must be of procedure's type: " + procedureType + " but is: VOID", node.AnchorToken);
                    }
                }
                else{
                    return Type.VOID;
                }
                    
            }
            var type = Visit((dynamic)node[0]);
            if (inProcedure)
            {
                var procedureType = procedureTable[procName].returnedType;
                if (procedureType != type){
                    throw new SemanticError("Return exp must be of procedure's type: " + procedureType + " but is: " + type, node.AnchorToken);
                }
            }
            else if(!inProcedure){
                throw new SemanticError("Return with exp type: " + type + " just can be used inside a procedure", node.AnchorToken);
            } 
            return type;
        }

        public Type Visit(For node)
        {
            inCycle +=1 ;
            var stType = Visit((dynamic)node[0]);
            var ltType = Visit((dynamic)node[1]);
            var singleTypeL = singleTypeList(ltType);

            string valueOfTable="";
            string valueOfLTable="";

            //Console.WriteLine("Printing");
            //Console.WriteLine(node[0].AnchorToken.Lexeme);
            if(!inProcedure){
                valueOfTable=symbolTable[node[0].AnchorToken.Lexeme].kind;
                if(valueOfTable!="VAR"){
                    throw new SemanticError("For cycle just use VAR",node.AnchorToken);
                }
                //Console.WriteLine(Table[node[0].AnchorToken.Lexeme].kind);
            }
            else{
                valueOfLTable=LocalSTable[node[0].AnchorToken.Lexeme].kind;
                if(valueOfLTable!="VAR"){
                    throw new SemanticError("For cycle just use VAR",node.AnchorToken);
                }
            }

            //Console.WriteLine("Finish Printing");
            if (singleTypeL != stType){
                throw new SemanticError("Var type must be: " + singleTypeL + " since exp is of type: " + ltType, node[0].AnchorToken);
            }
            if (ltType == Type.INT || ltType == Type.BOOL || ltType == Type.STRING || ltType == Type.VOID){
                throw new SemanticError("Exp type cannot be BOOL,INT,STRING, found: " + ltType, node[1].AnchorToken);
            }                
            Visit((dynamic)node[2]);
            inCycle -=0;
            return Type.VOID;
        }

        public Type Visit(Loop node)
        {
            inCycle += 1;
            Visit((dynamic)node[0]);
            inCycle -= 1;
            return Type.VOID;
        }


        public Type Visit(If node)
        {
            var condicion = Visit((dynamic)node[0]);
            if (condicion != Type.BOOL){
                throw new SemanticError("Exp must be of type BOOL, found: " + condicion, node[0].AnchorToken);
            }

            Visit((dynamic)node[1]);
            Visit((dynamic)node[2]);
            Visit((dynamic)node[3]);

            return Type.VOID;
        }

        public Type Visit(ElseIfList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(ElseIf node)
        {
            var condicion = Visit((dynamic)node[0]);
            if (condicion != Type.BOOL){
                throw new SemanticError("Exp must be of type BOOL, found: " + condicion, node[0].AnchorToken);
            }
            Visit((dynamic)node[1]);
            return Type.VOID;
        }

        public Type Visit(Else node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(ProcedureDeclarationList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(ProcedureDeclaration node)
        {
            var procedureName = node[0].AnchorToken.Lexeme;
            if (procedureTable.Contains(procedureName)){

                throw new SemanticError("Duplicated identifier: " + procedureName, node[0].AnchorToken);
            }
            else
            {
                inProcedure = true;
                isParam = true;
                Visit((dynamic)node[1]);
                isParam = false;
                Visit((dynamic)node[3]);
                Visit((dynamic)node[4]);
                var type = Visit((dynamic)node[2]);
                string noP= "NP";
                localSTables[procedureName] = LocalSTable;
                procedureTable[procedureName] = new Procedures(type, listOrderParams.Count, noP, listOrderParams, LocalSTable, listOrderParamsName);
                procName = procedureName;
                Visit((dynamic)node[5]);
                LocalSTable = new SymbolTable();
                listOrderParams = new List<dynamic>();
                listOrderParamsName = new List<string>();
                inProcedure = false;
                procName = "";
            }
            return Type.VOID;
        }

        public Type Visit(ParameterDeclarationList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(TypeNode node)
        {
            try
            {
                var type = typeMapper[node[0].AnchorToken.Category];
                if (type == Type.LISTVAR){
                    return Visit((dynamic)node[0]);
                }
                else{
                    return type;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return Type.VOID;
            }
        }


        public Type Visit(VariableDeclarationList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(VariableDeclaration node)
        {
            var category = node[0].AnchorToken.Category;
            var isList = false; 
            var kind = "VAR";
            var type = typeMapper[category];
            dynamic value = null;
            SymbolTable sTable = symbolTable;
           
            if (inProcedure){
                sTable = LocalSTable;
            }              

            if (isParam){
                kind = "PARAM";
            }                

            foreach (var n in node[1])
            {
                var temp = n.AnchorToken.Lexeme;
                if (type == Type.LISTVAR)
                {
                    isList = true;
                    type = Visit((dynamic)node[0]);
                }

                if (sTable.Contains(temp)){
                    throw new SemanticError("Duplicated variable: " + temp, n.AnchorToken);
                } else {
                    sTable[temp] = new Variables(type, kind, value, isList);
                    if (isParam)
                    {
                        numParams++;
                        listOrderParams.Add(type);
                        listOrderParamsName.Add(temp);
                    }
                }
            }
            return Type.VOID;
        }

        public Type Visit(ConstantDeclarationList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(ConstantDeclaration node)
        {
            var constName = node.AnchorToken.Lexeme;
            var type = Visit((dynamic)node[0]);
            var kind = "CONST";
            dynamic value = node[0].AnchorToken.Lexeme;
            bool isList = false;
            SymbolTable symbolT = symbolTable;

            if (inProcedure){
                symbolT = LocalSTable;
            }               

            if (type != Type.BOOL && type != Type.STRING && type != Type.INT && type != Type.VOID)
                try
                {
                    //Extract value of list
                    List<dynamic> temp=new List<dynamic>();
                    foreach(var n in ((dynamic)node[0])){
                        Visit((dynamic)n);
                        temp.Add(n.AnchorToken.Lexeme);
                    }
                    value=temp;
                    isList = true;
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new SemanticError("Constant list needs at least 1 value: " + node[0].AnchorToken.Category, node[0].AnchorToken);
                }

            if (symbolT.Contains(constName)){
                throw new SemanticError("Duplicated constant: " + constName, node.AnchorToken);
            }               

            else{
                symbolT[constName] = new Variables(type, kind, value, isList);
            }                
            return Type.VOID;
        }

        /*public List<dynamic> valorLista(List node)
        {
            List<dynamic> aux = new List<dynamic>();
            foreach (var n in node)
            {
                Visit((dynamic)n);
                aux.Add(n.AnchorToken.Lexeme);
            }
            return aux;
        }*/

        public Type Visit(List node)
        {
            var listType = Visit((dynamic)node[0]);

            foreach (var n in node)
            {
                var temp = Visit((dynamic)n);

                if (temp != listType){
                    throw new SemanticError("Incorrect type: " + temp, n.AnchorToken);
                }
                    
            }

            if (listType == Type.INT){
                return Type.LIST_OF_INT;
            }                
            else if (listType == Type.STRING){
                return Type.LIST_OF_STRING;
            }               
            else if (listType == Type.BOOL){
                return Type.LIST_OF_BOOL;
            }                
            else{
                return Type.LIST;
            }   
                
        }

        public Type Visit(ListNode node)
        {
            var category = node[0].AnchorToken.Category;

            if (category == TokenCategory.INTEGER){
                return Type.LIST_OF_INT;
            }
            else if (category == TokenCategory.STRING){
                return Type.LIST_OF_STRING;
            }            
            else if (category == TokenCategory.BOOLEAN){
                return Type.LIST_OF_BOOL;
            }            
            else{
                return Type.LIST;
            }   
        }

        public Type Visit(Int_Literal node)
        {
            var tempInt = node.AnchorToken.Lexeme;
            try
            {
                Convert.ToInt32(tempInt);
            }
            catch (OverflowException)
            {
                throw new SemanticError("Integer literal: "+tempInt+ " too large " , node.AnchorToken);
            }
            return Type.INT;
        }

        public Type Visit(Str_Literal node)
        {
            var tempString = node.AnchorToken.Lexeme;
            try
            {
                Convert.ToString(tempString);
            }
            catch (OverflowException)
            {
                throw new SemanticError("String literal: "+tempString +" too large ", node.AnchorToken);
            }
            return Type.STRING;
        }

        public Type Visit(True node)
        {
            return Type.BOOL;
        }

        public Type Visit(False node)
        {
            return Type.BOOL;
        }


        public Type Visit(Identifier node)
        {
            var varName = node.AnchorToken.Lexeme;
            if (inProcedure)
            {
                if (!LocalSTable.Contains(varName) && !symbolTable.Contains(varName))
                    throw new SemanticError("Undeclared variable: " + varName, node[0].AnchorToken);

                if (LocalSTable.Contains(varName))
                {
                    Type expectedType = LocalSTable[varName].type;
                    return expectedType;
                }
            }
        
            if ((inProcedure && !LocalSTable.Contains(varName)) || !inProcedure)
            {
                if (!symbolTable.Contains(varName)){
                    throw new SemanticError("Undeclared variable: " + varName, node[0].AnchorToken);
                }                   
            if (symbolTable.Contains(varName))
                {
                    Type expectedType = symbolTable[varName].type;
                    return expectedType;
                }
            }
            return Type.VOID;
        }

        public Type singleTypeList(Type expectedType)
        {
            if (expectedType == Type.LIST_OF_INT){
                return Type.INT;
            }               
            else if (expectedType == Type.LIST_OF_BOOL){
                return Type.BOOL;
            }
            else{
                return Type.STRING;
            }                
        }

 

        public Type Visit(ListIndex node)
        {
            var index = Visit((dynamic)node[0]);
            return index;
        }

        public Type Visit(ListItem node)
        {
            var varName = node[0].AnchorToken.Lexeme;
            if (inProcedure)
            {
                if (!LocalSTable.Contains(varName) && !symbolTable.Contains(varName)){
                    throw new SemanticError("Undeclared variable: " + varName, node[0].AnchorToken);
                }

                if (LocalSTable.Contains(varName))
                {
                    Type expectedType = LocalSTable[varName].type;
                    if (expectedType != Type.LIST_OF_INT && expectedType != Type.LIST_OF_STRING && expectedType != Type.LIST_OF_BOOL){
                        throw new SemanticError("Variable '" + varName + "' is not a list in assignment statement ", node[0].AnchorToken);
                    }

                    var index = Visit((dynamic)node[1]);
                    if (index != Type.INT){
                        throw new SemanticError("Incorrect index, INT expected, found " + index, node[1].AnchorToken);
                    }                      

                    expectedType = singleTypeList(expectedType);
                    return expectedType;
                }
            }
            if ((inProcedure && !LocalSTable.Contains(varName)) || !inProcedure)
            {
                if (!symbolTable.Contains(varName)){
                    throw new SemanticError("Undeclared variable: " + varName, node[0].AnchorToken);
                }                   
                if (symbolTable.Contains(varName))
                {
                    Type expectedType = symbolTable[varName].type;
                    if (expectedType != Type.LIST_OF_INT && expectedType != Type.LIST_OF_STRING && expectedType != Type.LIST_OF_BOOL){
                        throw new SemanticError("Variable '" + varName + "' is not a list in assignment statement ", node[0].AnchorToken);
                    }

                    var index = Visit((dynamic)node[1]);
                    if (index != Type.INT){
                        throw new SemanticError("Incorrect index, INT expected, found " + index, node[1].AnchorToken);
                    }                       
                    expectedType = singleTypeList(expectedType);
                    return expectedType;
                }
            }
            return Type.VOID;
        }


        public Type Visit(Assignment node)
        {
            var varName = node.AnchorToken.Lexeme;

            if (inProcedure)
            {
                if (!LocalSTable.Contains(varName) && !symbolTable.Contains(varName)){
                    throw new SemanticError("Undeclared variable: " + varName, node.AnchorToken);
                }                    

                if (LocalSTable.Contains(varName))
                {
                    if (LocalSTable[varName].kind == "CONST"){
                        throw new SemanticError("Constant: '" + varName + "' cannot be reasign", node.AnchorToken);
                    }                       

                    Type expectedType = LocalSTable[varName].type;
                    if (expectedType != Visit((dynamic)node[0])){
                        throw new SemanticError("Expecting type " + expectedType + " in assignment statement", node.AnchorToken);
                    }                        
                }
            }

            if ((inProcedure && !LocalSTable.Contains(varName)) || !inProcedure)
            {
                if (!symbolTable.Contains(varName)){
                    throw new SemanticError("Undeclared variable: " + varName, node.AnchorToken);

                }

                if (symbolTable[varName].kind == "CONST"){
                    throw new SemanticError("Constant: '" + varName + "' cannot be reasign", node.AnchorToken);
                }                    

                Type expectedType = symbolTable[varName].type;
                if (expectedType != Visit((dynamic)node[0])){
                    throw new SemanticError("Expecting type " + expectedType + " in assignment statement", node.AnchorToken);
                }                    
            }

            return Type.VOID;
        }

        public Type Visit(ListAssignmentS node)
        {
            var varName = node.AnchorToken.Lexeme;            
            if (inProcedure)
            { 
                if (!LocalSTable.Contains(varName) && !symbolTable.Contains(varName)){
                    throw new SemanticError("Undeclared variable: " + varName, node.AnchorToken);
                }             

                if (LocalSTable.Contains(varName))
                {
                    var expectedType = LocalSTable[varName].type;
                    if (expectedType != Type.LIST_OF_INT && expectedType != Type.LIST_OF_STRING && expectedType != Type.LIST_OF_BOOL){
                        throw new SemanticError("Variable '" + varName + "' is not a list in assignment statement ", node.AnchorToken);
                    }                       

                    expectedType = singleTypeList(expectedType);
                    var indice = Visit((dynamic)node[0]);
                    if (indice != Type.INT){
                        throw new SemanticError("Incorrect index, expecting INT, found " + indice, node[0][0].AnchorToken);
                    }                       
                    if (expectedType != Visit((dynamic)node[1])){
                        throw new SemanticError("Expecting type " + expectedType + " in assignment statement", node[1].AnchorToken);
                    }                        
                }
            }

            if ((inProcedure && !LocalSTable.Contains(varName)) || !inProcedure)
            {
                if (!symbolTable.Contains(varName)){
                    throw new SemanticError("Undeclared variable: " + varName, node.AnchorToken);
                }                    

                if (symbolTable.Contains(varName))
                {
                    var expectedType = symbolTable[varName].type;
                    if (expectedType != Type.LIST_OF_INT && expectedType != Type.LIST_OF_STRING && expectedType != Type.LIST_OF_BOOL){
                        throw new SemanticError("Variable '" + varName + "' not a list in assignment statement ", node.AnchorToken);
                    }                       

                    expectedType = singleTypeList(expectedType);
                    var index = Visit((dynamic)node[0]);
                    if (index != Type.INT){
                        throw new SemanticError("Incorrect index, expecting INT, found " + index, node[0][0].AnchorToken);
                    }                      

                    if (expectedType != Visit((dynamic)node[1])){
                        throw new SemanticError("Expecting type " + expectedType + " in assignment statement", node[1].AnchorToken);
                    }                        
                }
            }
            return Type.VOID;
        }

        public Type Visit(CallStatement node)
        {
            var procName = node.AnchorToken.Lexeme;
            //Not procedure declared
            if (!procedureTable.Contains(procName)){
                throw new SemanticError("Undeclared procedure: " + procName, node.AnchorToken);
            }                

            //Declared then bring name
            var procedure = procedureTable[procName];
            if (procedure.returnedType != Type.VOID){
                throw new SemanticError("Return value must be VOID but is : " + procedure.returnedType, node.AnchorToken);
            }             

            List<Type> list = new List<Type>();
            string lst = "";
            string obj = "";
            //Iterate between nodes, parameters
            foreach (var n in node)
            {
                var temp = Visit((dynamic)n);
                list.Add(temp);
                lst += temp + ",";
            }
            foreach (var n in procedure.parametersOrder)
            {
                obj += n + ",";
            }
            
            if (obj.Length > 0)
            {
                //validate the lenght in lst too
                if(lst.Length>0){
                    lst = lst.Substring(0, lst.Length - 1);     
                }                
                obj = obj.Substring(0, obj.Length - 1);
            }

            if (list.Count != procedure.noParameters){
                throw new SemanticError("Incorrect number of parameters: " + list.Count + " expecting: " + procedure.noParameters, node.AnchorToken);
            }
            if (lst != obj){
                throw new SemanticError("Incorrect order of parameters: " + lst + " expecting: " + obj, node.AnchorToken);
            }                

            return procedure.returnedType;
        }


        public Type Visit(Call node)
        {
            var procName = node.AnchorToken.Lexeme;

            if (!procedureTable.Contains(procName)){
                throw new SemanticError("Undeclared procedure: " + procName, node.AnchorToken);
            }
                
            //Declared then bring name
            var procedure = procedureTable[procName];

            List<Type> tempList = new List<Type>();
            int cont = 0;
            string lst = "";
            string obj = "";
            foreach (var n in node)
            {
                var temp = Visit((dynamic)n);
                tempList.Add(temp);
                cont++;
                lst += temp + ",";
            }

            foreach (var n in procedure.parametersOrder)
            {
                obj += n + ",";
            }

            if (obj.Length > 0)
            {
                //validate the lenght in lst too
                if(lst.Length>0){
                    lst = lst.Substring(0, lst.Length - 1);
                }                
                obj = obj.Substring(0, obj.Length - 1);
            }
    
            if (cont != procedure.noParameters){
                throw new SemanticError("Incorrect number of parameters: " + cont + " expecting: " + procedure.noParameters, node.AnchorToken);
            }

            if (lst != obj){
                throw new SemanticError("Incorrect order of parameters: " + lst + " expecting: " + obj, node.AnchorToken);
            }                

            return procedure.returnedType;
        }


        public Type Visit(Not node)
        {
            VisitUnaryOperator("NOT", node, Type.BOOL);
            return Type.BOOL;
        }

        public Type Visit(Rem node)
        {
            VisitBinaryOperator("REM", node, Type.INT);
            return Type.INT;
        }

        public Type Visit(Div node)
        {
            VisitBinaryOperator("DIV", node, Type.INT);
            return Type.INT;
        }

        public Type Visit(Mult node)
        {
            VisitBinaryOperator("*", node, Type.INT);
            return Type.INT;
        }

        public Type Visit(Negation node)
        {
            try
            {
                Visit((dynamic)node[1]);
            }
            catch (ArgumentOutOfRangeException)
            {
                VisitUnaryOperator("-", node, Type.INT);
                return Type.INT;
            }

            VisitBinaryOperator("-", node, Type.INT);
            return Type.INT;
        }

        public Type Visit(Sum node)
        {
            VisitBinaryOperator("+", node, Type.INT);
            return Type.INT;
        }

        public Type Visit(Minus node)
        {
            VisitBinaryOperator("-", node, Type.INT);
            return Type.INT;
        }

        public Type Visit(MoreEqualOperator node)
        {
            VisitBinaryOperator(">=", node, Type.INT);
            return Type.BOOL;
        }

        public Type Visit(LessEqualOperator node)
        {
            VisitBinaryOperator("<=", node, Type.INT);
            return Type.BOOL;
        }

        public Type Visit(MoreOperator node)
        {
            VisitBinaryOperator(">", node, Type.INT);
            return Type.BOOL;
        }

        public Type Visit(LessOperator node)
        {
            VisitBinaryOperator("<", node, Type.INT);
            return Type.BOOL;
        }

        public Type Visit(Inequality node)
        {
            var op = Visit((dynamic)node[0]);

            if (op == Type.INT){
                VisitBinaryOperator("<>", node, Type.INT);
            }
            else{
                VisitBinaryOperator("<>", node, Type.BOOL);
            }            
            return Type.BOOL;
        }

        public Type Visit(Equality node)
        {
            var op = Visit((dynamic)node[0]);

            if (op == Type.INT){
                VisitBinaryOperator("=", node, Type.INT);
            }             
            else{
                VisitBinaryOperator("=", node, Type.BOOL);
            }              
            return Type.BOOL;
        }

        public Type Visit(And node)
        {
            VisitBinaryOperator("AND", node, Type.BOOL);
            return Type.BOOL;
        }

        public Type Visit(Or node)
        {
            VisitBinaryOperator("OR", node, Type.BOOL);
            return Type.BOOL;
        }

        public Type Visit(Xor node)
        {
            VisitBinaryOperator("XOR", node, Type.BOOL);
            return Type.BOOL;
        }

        void VisitChildren(Node node)
        {
            foreach (var n in node)
            {
                Visit((dynamic)n);
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

        void VisitBinaryOperator(string op, Node node, Type type)
        {
            if (Visit((dynamic)node[0]) != type || 
                Visit((dynamic)node[1]) != type){
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