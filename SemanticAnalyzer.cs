﻿/*
  Chimera compiler - SemanticAnalyzer.
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358
*/
using System;
using System.Collections.Generic;

namespace Chimera {

    class SemanticAnalyzer {

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

            if symbolTable.Contains(variableName)) {
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
            VisitBinaryOperator("<=", node, Type.INT)
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
            VisitBinaryOperator(">=", node, Type.INT)
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