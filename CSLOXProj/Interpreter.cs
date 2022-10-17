using System;
using System.Collections.Generic;

namespace CSLOXProj
{
    public class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object>
    {
        private Environment environment = new Environment();
        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            Object left = Evaluate(expr.left);

            if (expr.Operator.type == TokenType.OR) {
                if (IsTruthy(left)) return left;
            } else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.right);
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.Operator.type) {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr.Operator, right);
                    return -(double)right;
            }

            // Unreachable.
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return environment.Get(expr.name);
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.Operator.type) {
                case TokenType.BANG_EQUAL: return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL: return IsEqual(left, right);
                case TokenType.GREATER:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left <= (double)right;
                case TokenType.MINUS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if (left is double && right is double) {
                        return (double)left + (double)right;
                    }

                    if (left is string && right is string) {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.Operator,
            "Operands must be two numbers or two strings.");
            }

            // Unreachable.
            return null;
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private object Execute(Stmt stmt)
        {
            if(stmt != null) return stmt.Accept(this);
            return null;
        }

        void ExecuteBlock(List<Stmt> statements,
                    Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            Object value = null;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            environment.Assign(expr.name, value);
            return value;
        }

        private bool IsTruthy(object truthyObject)
        {
            if (truthyObject == null) return false;
            if (truthyObject is Boolean) return (bool)truthyObject;
            return true;
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private void CheckNumberOperand(Token Operator, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(Operator, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token Operator,
                                   object left, object right)
        {
            if (left is double && right is double) return;

            throw new RuntimeError(Operator, "Operands must be numbers.");
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        private string Stringify(object Object)
        {
            if (Object == null) return "nil";

            if (Object is double) {
                string text = Object.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return Object.ToString();
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }
            return null;
        }
    }
}
