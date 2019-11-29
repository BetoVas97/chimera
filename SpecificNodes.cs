/*
  Chimera compiler - SpecificNodes.cs
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358
*/

namespace Chimera {    

    class Program: Node {}

    class DeclarationList: Node {}
    class ConstDeclarationList: Node {}
    class ConstDeclaration: Node {}
    class ProcedureDeclarationList: Node {}

    class Equality: Node {}
    class Inequality: Node {}
    class LessThan: Node {}
    class GreaterThan: Node {}
    class LessEqual: Node {}
    class GreaterEqual: Node {}
    class Sum: Node {}
    class Minus: Node {}
    class Negation: Node {}
    class Div: Node {}
    class Rem: Node {}
    class Not: Node {}
    class Mult: Node {}

    class ElseIfList: Node {}
    class ElseIf: Node {}
    class Else: Node {}
    class And: Node {}
    class Or: Node {}
    class Xor: Node {}

    class Tipo: Node {}

    class ParameterDeclarationList: Node {}
    class VarDeclarationList: Node {}
    class VarDeclaration: Node {}
    class VarDeclarationItems: Node {}
    class ProcedureDeclaration: Node {}

    class List: Node {}

    class StatementList: Node {}
    class Statement: Node {}
    class If: Node {}
    class Assignment: Node {}
    class CallS: Node {}
    class Loop: Node {}
    class For: Node {}
    class Return: Node {}
    class Exit: Node {}

    class AssignmentType: Node {}
    class ListIndex: Node {}
    class ListItem: Node {}
    class ListIndexAssignment: Node {}
    class SimpleAssignment: Node {}
    class Identifier: Node {}
    class Call: Node {}
		
    class ListN: Node {}
    class IntegerN: Node {}
    class StringN: Node {}
    class BooleanN: Node {}

    class Int_Literal: Node {}
    class Str_Literal: Node {}
    class False: Node {}
    class True: Node {}
}
