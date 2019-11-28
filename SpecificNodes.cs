/*
  Chimera compiler - SpecificNodes.cs
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358
*/

namespace Chimera {
    class List : Node { }
    class DeclarationList : Node { }
    class ConstantDeclarationList : Node { }
    class VariableDeclarationList : Node { }
    class ParamaterDeclarationList : Node { }
    class ProcedureDeclarationList : Node { }
    class StatementList : Node { }
    class ExpressionList: Node { }
    class SimpleLiteralList:Node {}
    class IdentifierList : Node { }
    class Declaration : Node { }
    class VariableDeclaration : Node { }
    class VariableNames : Node { }
    class ConstDeclaration : Node { }
    class ParameterDeclaration: Node { }
    class ProcedureDeclaration : Node { }
    class Program : Node { }
    class SimpleType: Node { }
    class Call : Node {}
    class Expression: Node { }
    class AssignmentStatement: Node {}
    class CallStatement: Node { }
    class Assignment : Node { }
    class If : Node {}
    class Loop : Node { }
    class For : Node { }
    class Return: Node { }
    class Exit : Node { }
    class Identifier : Node { }
    class And : Node { }
    class Or: Node {}
    class Xor: Node {}
    class Type : Node { }
    class SimpleLiteral : Node { }
    class IntegerLiteral: Node {}
    class StringLiteral: Node {}
    class BooleanLiteral: Node {}
    class FalseLiteral: Node { }
    class TrueLiteral: Node { }
    class AdditionOperator: Node { }
    class SubstractionOperator: Node { }
    class MultiplicantOperator: Node { }
    class DivOperator: Node { }
    class RemOperator: Node { }
    class NotOperator: Node { }
    class NegationOperator: Node { }
    class MoreOperator: Node { }
    class LessOperator: Node { }
    class LessEqualOperator: Node {}
    class MoreEqualOperator: Node {}
    class LessMoreOperator: Node { }
    class AssignOperator: Node { }
}