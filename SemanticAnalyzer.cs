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

        //-----------------------------------------------------------
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
                { TokenCategory.LIST, Type.VLIST }
            };


        public SymbolTable Table
        {
            get;
            private set;
        }

    
        public SymbolTable TableTemporal
        {
            get;
            private set;
        }

        public ProcedureTable TableProcedure
        {
            get;
            private set;
        }


        public IDictionary<string, SymbolTable> localTables
        {
            get;
            private set;
        }

 
        public bool procedure;

        public bool param;

        public int numParams;

        public List<dynamic> ordenParams;

        public int ciclo;

        public string nombreProcedure;


        public SemanticAnalyzer()
        {
            nombreProcedure = "";
            ciclo = 0;
            numParams = 0;
            ordenParams = new List<dynamic>();
            procedure = false;
            param = false;
            Table = new SymbolTable();
            TableTemporal = new SymbolTable();
            TableProcedure = new ProcedureTable();
            localTables = new SortedDictionary<string, SymbolTable>();
            
            string predefinido = "P";
            SymbolTable tabla = new SymbolTable();
            TableProcedure["WrInt"] = new Procedures(Type.VOID, 1, predefinido, new List<dynamic>() { Type.INT }, tabla);
            TableProcedure["WrStr"] = new Procedures(Type.VOID, 1, predefinido, new List<dynamic>() { Type.STRING }, tabla);
            TableProcedure["WrBool"] = new Procedures(Type.VOID, 1, predefinido, new List<dynamic>() { Type.BOOL }, tabla);
            TableProcedure["WrLn"] = new Procedures(Type.VOID, 0, predefinido, new List<dynamic>(), tabla);
            TableProcedure["RdInt"] = new Procedures(Type.INT, 0, predefinido, new List<dynamic>(), tabla);
            TableProcedure["RdStr"] = new Procedures(Type.STRING, 0, predefinido, new List<dynamic>(), tabla);
            TableProcedure["AtStr"] = new Procedures(Type.STRING, 2, predefinido, new List<dynamic>() { Type.STRING, Type.INT }, tabla);
            TableProcedure["LenStr"] = new Procedures(Type.INT, 1, predefinido, new List<dynamic>() { Type.STRING }, tabla);
            TableProcedure["CmpStr"] = new Procedures(Type.INT, 2, predefinido, new List<dynamic>() { Type.STRING, Type.STRING }, tabla);
            TableProcedure["CatStr"] = new Procedures(Type.STRING, 2, predefinido, new List<dynamic>() { Type.STRING, Type.STRING }, tabla);
            TableProcedure["LenLstInt"] = new Procedures(Type.INT, 1, predefinido, new List<dynamic>() { Type.LIST_OF_INT }, tabla);
            TableProcedure["LenLstStr"] = new Procedures(Type.INT, 1, predefinido, new List<dynamic>() { Type.LIST_OF_STRING }, tabla);
            TableProcedure["LenLstBool"] = new Procedures(Type.INT, 1, predefinido, new List<dynamic>() { Type.LIST_OF_BOOL }, tabla);
            TableProcedure["NewLstInt"] = new Procedures(Type.LIST_OF_INT, 1, predefinido, new List<dynamic>() { Type.INT }, tabla);
            TableProcedure["NewLstStr"] = new Procedures(Type.LIST_OF_STRING, 1, predefinido, new List<dynamic>() { Type.INT }, tabla);
            TableProcedure["NewLstBool"] = new Procedures(Type.LIST_OF_BOOL, 1, predefinido, new List<dynamic>() { Type.INT }, tabla);
            TableProcedure["IntToStr"] = new Procedures(Type.STRING, 1, predefinido, new List<dynamic>() { Type.INT }, tabla);
            TableProcedure["StrToInt"] = new Procedures(Type.INT, 1, predefinido, new List<dynamic>() { Type.STRING }, tabla);
        }

        public Type Visit(Program node)
        {
            Visit((dynamic)node[0]);

            Visit((dynamic)node[1]);
            Visit((dynamic)node[2]);
            Visit((dynamic)node[3]);
            return Type.VOID;
        }

        public Type Visit(StatementList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(Exit node)
        {

            if (ciclo == 0)
                throw new SemanticError("Exit statement is just allowed inside a loop", node.AnchorToken);

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
                if (procedure)
                {
                    var procedureType = TableProcedure[nombreProcedure].tipoRetorno;
                    if (procedureType != Type.VOID)
                        throw new SemanticError("Return exp must match procedure's type: " + procedureType + " but is: VOID", node.AnchorToken);
                }

                else
                    return Type.VOID;
            }

            var type = Visit((dynamic)node[0]);

            if (procedure)
            {
                var procedureType = TableProcedure[nombreProcedure].tipoRetorno;
                if (procedureType != type)
                    throw new SemanticError("Return exp must match procedure's type: " + procedureType + " but is: " + type, node.AnchorToken);
            }


            else if(!procedure)
                throw new SemanticError("Return with exp type: " + type + " just can be used inside a procedure", node.AnchorToken);


                return type;
        }

        public Type Visit(For node)
        {

            ciclo += 1;
            var Stype = Visit((dynamic)node[0]);
            var Ltype = Visit((dynamic)node[1]);
            var listSinge = listSingleType(Ltype);

            if (listSinge != Stype)
                throw new SemanticError("Var type must be: " + listSinge + " because exp is of type: " + Ltype, node[0].AnchorToken);

            if (Ltype == Type.INT || Ltype == Type.BOOL || Ltype == Type.STRING || Ltype == Type.VOID)
                throw new SemanticError("Exp type cannot be of type BOOL,INT,STRING, but received: " + Ltype, node[1].AnchorToken);

            Visit((dynamic)node[2]);
            ciclo -= 1;
            return Type.VOID;
        }

        public Type Visit(Loop node)
        {
            ciclo += 1;

            Visit((dynamic)node[0]);
            ciclo -= 1;
            return Type.VOID;
        }


        public Type Visit(If node)
        {
            var condicion = Visit((dynamic)node[0]);

            if (condicion != Type.BOOL)
                throw new SemanticError("Exp must be of type BOOL, but received: " + condicion, node[0].AnchorToken);

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

            if (condicion != Type.BOOL)
                throw new SemanticError("Exp must be of type BOOL, but received: " + condicion, node[0].AnchorToken);

            Visit((dynamic)node[1]);
            return Type.VOID;
        }

        public Type Visit(Else node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(CallS node)
        {

            var nombre = node.AnchorToken.Lexeme;

            if (!TableProcedure.Contains(nombre))
                throw new SemanticError("Undeclared procedure: " + nombre, node.AnchorToken);

            var procedure = TableProcedure[nombre];

            if (procedure.tipoRetorno != Type.VOID)
                throw new SemanticError("Return value must be VOID but actual is : " + procedure.tipoRetorno, node.AnchorToken);


            List<Type> lista = new List<Type>();

            string lst = "";
            string obj = "";

            foreach (var n in node)
            {
                var temp = Visit((dynamic)n);

                lista.Add(temp);

                lst += temp + ",";
            }


            foreach (var n in procedure.ordenParametros)
            {

                obj += n + ",";
            }

            if (obj.Length > 0)
            {
                lst = lst.Substring(0, lst.Length - 1);
                obj = obj.Substring(0, obj.Length - 1);
            }

            if (lista.Count != procedure.numParametros)
                throw new SemanticError("Wrong number of parameters: " + lista.Count + " expecting: " + procedure.numParametros, node.AnchorToken);

            if (lst != obj)
                throw new SemanticError("Wrong order of parameters: " + lst + " expecting: " + obj, node.AnchorToken);

            return procedure.tipoRetorno;
        }

        public Type Visit(ListIndexAssignment node)
        {

            var variableName = node.AnchorToken.Lexeme;

            
            if (procedure)
            {
 
                if (!TableTemporal.Contains(variableName) && !Table.Contains(variableName))
                    throw new SemanticError("Undeclared variable: " + variableName, node.AnchorToken);


                if (TableTemporal.Contains(variableName))
                {

                    var expectedType = TableTemporal[variableName].tipo;

                    
                    if (expectedType != Type.LIST_OF_INT && expectedType != Type.LIST_OF_STRING && expectedType != Type.LIST_OF_BOOL)
                        throw new SemanticError("Variable '" + variableName + "' is not a list in assignment statement ", node.AnchorToken);


                    expectedType = listSingleType(expectedType);


                    var indice = Visit((dynamic)node[0]);


                    if (indice != Type.INT)
                        throw new SemanticError("Invalid index, expecting INT, recived " + indice, node[0][0].AnchorToken);

                    if (expectedType != Visit((dynamic)node[1]))
                        throw new SemanticError("Expecting type " + expectedType + " in assignment statement", node[1].AnchorToken);
                }
            }

            if ((procedure && !TableTemporal.Contains(variableName)) || !procedure)
            {
                if (!Table.Contains(variableName))
                    throw new SemanticError("Undeclared variable: " + variableName, node.AnchorToken);

                if (Table.Contains(variableName))
                {
                    var expectedType = Table[variableName].tipo;

                    if (expectedType != Type.LIST_OF_INT && expectedType != Type.LIST_OF_STRING && expectedType != Type.LIST_OF_BOOL)
                        throw new SemanticError("Variable '" + variableName + "' is not a list in assignment statement ", node.AnchorToken);

                    expectedType = listSingleType(expectedType);

                    var indice = Visit((dynamic)node[0]);

                    if (indice != Type.INT)
                        throw new SemanticError("Invalid index, expecting INT, recived " + indice, node[0][0].AnchorToken);

                    if (expectedType != Visit((dynamic)node[1]))
                        throw new SemanticError("Expecting type " + expectedType + " in assignment statement", node[1].AnchorToken);
                }
            }
            return Type.VOID;
        }


        public Type Visit(Assignment node)
        {
            var variableName = node.AnchorToken.Lexeme;

            if (procedure)
            {
                if (!TableTemporal.Contains(variableName) && !Table.Contains(variableName))
                    throw new SemanticError("Undeclared variable: " + variableName, node.AnchorToken);

                if (TableTemporal.Contains(variableName))
                {
                    if (TableTemporal[variableName].tipoVariable == "CONST")
                        throw new SemanticError("Cannot reasign constant: '" + variableName + "' ", node.AnchorToken);

                    Type expectedType = TableTemporal[variableName].tipo;

                    if (expectedType != Visit((dynamic)node[0]))
                        throw new SemanticError("Expecting type " + expectedType + " in assignment statement", node.AnchorToken);
                }
            }

            if ((procedure && !TableTemporal.Contains(variableName)) || !procedure)
            {
                if (!Table.Contains(variableName))
                    throw new SemanticError("Undeclared variable: " + variableName, node.AnchorToken);

                if (Table[variableName].tipoVariable == "CONST")
                    throw new SemanticError("Cannot reasign constant: '" + variableName + "' ", node.AnchorToken);

                Type expectedType = Table[variableName].tipo;

                if (expectedType != Visit((dynamic)node[0]))
                    throw new SemanticError("Expecting type " + expectedType + " in assignment statement", node.AnchorToken);
            }

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

            if (TableProcedure.Contains(procedureName))
                throw new SemanticError("Duplicated identifier: " + procedureName, node[0].AnchorToken);

            else
            {
                
                procedure = true;
                param = true;

                Visit((dynamic)node[1]);

                param = false;

                Visit((dynamic)node[3]);

                Visit((dynamic)node[4]);


                var type = Visit((dynamic)node[2]);
                string noPredefinido = "NP";
                localTables[procedureName] = TableTemporal;
                TableProcedure[procedureName] = new Procedures(type, ordenParams.Count, noPredefinido, ordenParams, TableTemporal);
                nombreProcedure = procedureName;

                Visit((dynamic)node[5]);

                TableTemporal = new SymbolTable();
                ordenParams = new List<dynamic>();
                procedure = false;
                nombreProcedure = "";
            }
            return Type.VOID;
        }

        public Type Visit(ParameterDeclarationList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(Tipo node)
        {
            try
            {
                var type = typeMapper[node[0].AnchorToken.Category];

                
                if (type == Type.VLIST)
                    return Visit((dynamic)node[0]);

                else
                    return type;
            }
            catch (ArgumentOutOfRangeException)
            {
                return Type.VOID;
            }
        }


        public Type Visit(VarDeclarationList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(VarDeclaration node)
        {
            var category = node[0].AnchorToken.Category;
            var esLista = false; 
            var tipoVariable = "VAR";
            var type = typeMapper[category];
            dynamic valor = null;
            SymbolTable tabla = Table;
           
            if (procedure)
                tabla = TableTemporal;

            if (param)
                tipoVariable = "PARAM";

            foreach (var n in node[1])
            {
                var temp = n.AnchorToken.Lexeme;
                if (type == Type.VLIST)
                {
                    esLista = true;
                    type = Visit((dynamic)node[0]);
                }

                if (tabla.Contains(temp))
                    throw new SemanticError("Duplicated variable: " + temp, n.AnchorToken);

                else
                {
                    tabla[temp] = new Variables(type, tipoVariable, valor, esLista);

                    if (param)
                    {
                        numParams++;
                        ordenParams.Add(type);
                    }
                }
            }
            return Type.VOID;
        }

        public Type Visit(ConstDeclarationList node)
        {
            VisitChildren(node);
            return Type.VOID;
        }

        public Type Visit(ConstDeclaration node)
        {
            var constName = node.AnchorToken.Lexeme;
            var type = Visit((dynamic)node[0]);
            var tipoVariable = "CONST";
            dynamic valor = node[0].AnchorToken.Lexeme;
            bool esLista = false;
            SymbolTable tabla = Table;

            if (procedure)
                tabla = TableTemporal;

            if (type != Type.BOOL && type != Type.STRING && type != Type.INT && type != Type.VOID)
                try
                {
                    valor = valorLista((dynamic)node[0]);
                    esLista = true;
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new SemanticError("Constant list must have at least 1 value: " + node[0].AnchorToken.Category, node[0].AnchorToken);
                }

            if (tabla.Contains(constName))
                throw new SemanticError("Duplicated constant: " + constName, node.AnchorToken);

            else
                tabla[constName] = new Variables(type, tipoVariable, valor, esLista);

            return Type.VOID;
        }

        public Type Visit(List node)
        {
            var listType = Visit((dynamic)node[0]);

            foreach (var n in node)
            {
                var temp = Visit((dynamic)n);

                if (temp != listType)
                    throw new SemanticError("Wrong type: " + temp, n.AnchorToken);
            }

            if (listType == Type.INT)
                return Type.LIST_OF_INT;

            else if (listType == Type.STRING)
                return Type.LIST_OF_STRING;

            else if (listType == Type.BOOL)
                return Type.LIST_OF_BOOL;
            else
                return Type.LIST;
        }

        public Type Visit(ListN node)
        {
            var category = node[0].AnchorToken.Category;

            if (category == TokenCategory.INTEGER)
                return Type.LIST_OF_INT;

            else if (category == TokenCategory.STRING)
                return Type.LIST_OF_STRING;

            else if (category == TokenCategory.BOOLEAN)
                return Type.LIST_OF_BOOL;

            else
                return Type.LIST;
        }

        public Type Visit(Int_Literal node)
        {

            var intStr = node.AnchorToken.Lexeme;

            try
            {
                Convert.ToInt32(intStr);
            }
            catch (OverflowException)
            {
                throw new SemanticError("Integer literal too large: " + intStr, node.AnchorToken);
            }

            return Type.INT;
        }

        public Type Visit(Str_Literal node)
        {
            var str = node.AnchorToken.Lexeme;

            try
            {
                Convert.ToString(str);
            }
            catch (OverflowException)
            {
                throw new SemanticError("String literal too large: " + str, node.AnchorToken);
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

        void VisitChildren(Node node)
        {
            foreach (var n in node)
            {
                Visit((dynamic)n);
            }
        }

        public Type Visit(Identifier node)
        {
            var variableName = node.AnchorToken.Lexeme;

            if (procedure)
            {
                if (!TableTemporal.Contains(variableName) && !Table.Contains(variableName))
                    throw new SemanticError("Undeclared variable: " + variableName, node[0].AnchorToken);

                if (TableTemporal.Contains(variableName))
                {
                    Type expectedType = TableTemporal[variableName].tipo;

                    return expectedType;
                }
            }
        
            if ((procedure && !TableTemporal.Contains(variableName)) || !procedure)
            {
                if (!Table.Contains(variableName))
                    throw new SemanticError("Undeclared variable: " + variableName, node[0].AnchorToken);

                if (Table.Contains(variableName))
                {

                    Type expectedType = Table[variableName].tipo;

                    return expectedType;
                }
            }
            return Type.VOID;

        }

        public Type listSingleType(Type expectedType)
        {
            if (expectedType == Type.LIST_OF_INT)
                return Type.INT;

            else if (expectedType == Type.LIST_OF_BOOL)
                return Type.BOOL;

            else
                return Type.STRING;
        }

        public List<dynamic> valorLista(List node)
        {
            List<dynamic> aux = new List<dynamic>();
            foreach (var n in node)
            {
                Visit((dynamic)n);
                aux.Add(n.AnchorToken.Lexeme);
            }
            return aux;
        }

        public Type Visit(ListIndex node)
        {
            var index = Visit((dynamic)node[0]);
            return index;
        }

        public Type Visit(ListItem node)
        {
            var variableName = node[0].AnchorToken.Lexeme;

            if (procedure)
            {
                if (!TableTemporal.Contains(variableName) && !Table.Contains(variableName))
                    throw new SemanticError("Undeclared variable: " + variableName, node[0].AnchorToken);

                if (TableTemporal.Contains(variableName))
                {
                    Type expectedType = TableTemporal[variableName].tipo;

                    if (expectedType != Type.LIST_OF_INT && expectedType != Type.LIST_OF_STRING && expectedType != Type.LIST_OF_BOOL)
                        throw new SemanticError("Variable '" + variableName + "' is not a list in assignment statement ", node[0].AnchorToken);

                    var index = Visit((dynamic)node[1]);

                    if (index != Type.INT)
                        throw new SemanticError("Invalid index, INT expected, received: " + index, node[1].AnchorToken);

                    expectedType = listSingleType(expectedType);
                    return expectedType;
                }
            }
            if ((procedure && !TableTemporal.Contains(variableName)) || !procedure)
            {
                if (!Table.Contains(variableName))
                    throw new SemanticError("Undeclared variable: " + variableName, node[0].AnchorToken);

                if (Table.Contains(variableName))
                {
                    Type expectedType = Table[variableName].tipo;

                    if (expectedType != Type.LIST_OF_INT && expectedType != Type.LIST_OF_STRING && expectedType != Type.LIST_OF_BOOL)
                        throw new SemanticError("Variable '" + variableName + "' is not a list in assignment statement ", node[0].AnchorToken);

                    var index = Visit((dynamic)node[1]);

                    if (index != Type.INT)
                        throw new SemanticError("Invalid index, INT expected, received: " + index, node[1].AnchorToken);

                    expectedType = listSingleType(expectedType);
                    return expectedType;
                }
            }
            return Type.VOID;
        }


        void VisitBinaryOperator(string op, Node node, Type type)
        {
            if (Visit((dynamic)node[0]) != type || Visit((dynamic)node[1]) != type)
                throw new SemanticError(String.Format("Operator {0} requires two operands of type {1}",op,type),node.AnchorToken);
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

        public Type Visit(Call node)
        {
            var nombre = node.AnchorToken.Lexeme;

            if (!TableProcedure.Contains(nombre))
                throw new SemanticError("Undeclared procedure: " + nombre, node.AnchorToken);

            var procedure = TableProcedure[nombre];

            List<Type> lista = new List<Type>();
            int contador = 0;
            string lst = "";
            string obj = "";
            foreach (var n in node)
            {
                var temp = Visit((dynamic)n);
                lista.Add(temp);
                contador++;
                lst += temp + ",";
            }

            foreach (var n in procedure.ordenParametros)
            {
                obj += n + ",";
            }

            if (obj.Length > 0)
            {
                lst = lst.Substring(0, lst.Length - 1);
                obj = obj.Substring(0, obj.Length - 1);
            }

            if (contador != procedure.numParametros)
                throw new SemanticError("Wrong number of parameters: " + contador + " expecting: " + procedure.numParametros, node.AnchorToken);

            if (lst != obj)
                throw new SemanticError("Wrong order of parameters: " + lst + " expecting: " + obj, node.AnchorToken);

            return procedure.tipoRetorno;
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

        public Type Visit(GreaterEqual node)
        {
            VisitBinaryOperator(">=", node, Type.INT);

            return Type.BOOL;
        }

        public Type Visit(LessEqual node)
        {
            VisitBinaryOperator("<=", node, Type.INT);

            return Type.BOOL;
        }

        public Type Visit(GreaterThan node)
        {
            VisitBinaryOperator(">", node, Type.INT);

            return Type.BOOL;
        }

        public Type Visit(LessThan node)
        {
            VisitBinaryOperator("<", node, Type.INT);

            return Type.BOOL;
        }

        public Type Visit(Inequality node)
        {
            var op1 = Visit((dynamic)node[0]);

            if (op1 == Type.INT)
                VisitBinaryOperator("<>", node, Type.INT);

            else
                VisitBinaryOperator("<>", node, Type.BOOL);

            return Type.BOOL;
        }

        public Type Visit(Equality node)
        {
            var op1 = Visit((dynamic)node[0]);

            if (op1 == Type.INT)
                VisitBinaryOperator("=", node, Type.INT);

            else
                VisitBinaryOperator("=", node, Type.BOOL);

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

    }
}