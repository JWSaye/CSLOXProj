using System;
using System.Collections.Generic;
using static CSLOXProj.Stmt;

namespace CSLOXProj {
    public class Interpreter : Expr.IVisitor<object>, IVisitor<object> {
        public Environment globals;
        private Environment environment;
        private readonly Dictionary<Expr, int> locals = new();

        public Interpreter() {
            globals = new Environment();
            environment = globals;
            DefineNativeFunctions();
        }

        private void DefineNativeFunctions() {
            ILoxCallable clock = new Clocks();
            globals.Define("clock", clock);
        }

        public object VisitLiteralExpr(Expr.Literal expr) {
            return expr.value;
        }

        public object VisitLogicalExpr(Expr.Logical expr) {
            object left = Evaluate(expr.left);

            if (expr.Operator.type == TokenType.OR) {
                if (IsTruthy(left)) return left;
            } 
            else {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.right);
        }

        public object VisitSetExpr(Expr.Set expr) {
            object Object = Evaluate(expr.Object);

            if (Object is not LoxInstance) {
                throw new RuntimeError(expr.name, "Only instances have fields.");
            }

            object value = Evaluate(expr.value);
            ((LoxInstance)Object).Set(expr.name, value);
            return value;
        }

        public object VisitSuperExpr(Expr.Super expr) {
            int distance = locals[expr];
            LoxClass superclass = (LoxClass)environment.GetAt(distance, "super");

            LoxInstance Object = (LoxInstance)environment.GetAt(distance - 1, "this");

            LoxFunction method = superclass.FindMethod(expr.method.lexeme);

            if (method == null) {
                throw new RuntimeError(expr.method, "Undefined property '" + expr.method.lexeme + "'.");
            }

            return method.Bind(Object);
        }

        public object VisitThisExpr(Expr.This expr) {
            return LookUpVariable(expr.keyword, expr);
        }

        public object VisitGroupingExpr(Expr.Grouping expr) {
            return Evaluate(expr.expression);
        }

        public object VisitUnaryExpr(Expr.Unary expr) {
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

        public object VisitVariableExpr(Expr.Variable expr) {
            return LookUpVariable(expr.name, expr);
        }

        private object LookUpVariable(Token name, Expr expr) {
            int? distance = locals[expr];

            if (distance != null) {
                return environment.GetAt((int)distance, name.lexeme);
            }
            else {
                return globals.Get(name);
            }
        }

        public object VisitBinaryExpr(Expr.Binary expr) {
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
                    if (left is double @double && right is double double1) {
                        return @double + double1;
                    }

                    if (left is string @string && right is string string1) {
                        return @string + string1;
                    }

                    throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.");
            }

            // Unreachable.
            return null;
        }

        private object Evaluate(Expr expr) {
            return expr.Accept(this);
        }

        private object Execute(Stmt stmt) {
            if(stmt != null) return stmt.Accept(this);
            return null;
        }

        public void Resolve(Expr expr, int depth) {
            locals[expr] = depth;
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment) {
            Environment previous = this.environment;
            try {
                this.environment = environment;

                foreach (Stmt statement in statements) {
                    Execute(statement);
                }
            }
            finally {
                this.environment = previous;
            }
        }

        public object VisitBlockStmt(Block stmt) {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public object VisitClassStmt(Class stmt) {
            object superclass = null;
            if (stmt.superclass != null) {
                superclass = Evaluate(stmt.superclass);
                if (superclass is not LoxClass) {
                    throw new RuntimeError(stmt.superclass.name, "Superclass must be a class.");
                }
            }

            environment.Define(stmt.name.lexeme, null);

            if (stmt.superclass != null) {
                environment = new Environment(environment);
                environment.Define("super", superclass);
            }

            Dictionary<string, LoxFunction> methods = new();
            foreach (Function method in stmt.methods) {
                LoxFunction function = new(method, environment, method.name.lexeme.Equals("init"));
                methods[method.name.lexeme] = function;
            }

            LoxClass klass = new(stmt.name.lexeme, (LoxClass)superclass, methods);

            if (superclass != null) {
                environment = environment.enclosing;
            }

            environment.Assign(stmt.name, klass);
            return null;
        }

        public object VisitExpressionStmt(Expression stmt) {
            Evaluate(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Function stmt) {
            LoxFunction function = new(stmt, environment, false);
            environment.Define(stmt.name.lexeme, function);
            return null;
        }

        public object VisitIfStmt(If stmt) {
            if (IsTruthy(Evaluate(stmt.condition))) {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null) {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object VisitPrintStmt(Print stmt) {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt) {
            object value = null;
            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new Return(value);
        }

        public object VisitVarStmt(Var stmt) {
            object value = null;
            if (stmt.initializer != null) {
                value = Evaluate(stmt.initializer);
            }

            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr) {
            object value = Evaluate(expr.value);

            int? distance = locals[expr];
            if (distance != null) {
                environment.AssignAt((int)distance, expr.name, value);
            }
            else {
                globals.Assign(expr.name, value);
            }

            return value;
        }

        private bool IsTruthy(object truthyObject) {
            if (truthyObject == null) return false;
            if (truthyObject is bool boolean) return boolean;
            return true;
        }

        private bool IsEqual(object a, object b) {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private void CheckNumberOperand(Token Operator, object operand) {
            if (operand is double) return;
            throw new RuntimeError(Operator, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token Operator, object left, object right) {
            if (left is double && right is double) return;

            throw new RuntimeError(Operator, "Operands must be numbers.");
        }

        public void Interpret(List<Stmt> statements) {
            try {
                foreach (Stmt statement in statements) {
                    Execute(statement);
                }
            }
            catch (RuntimeError error) {
                Lox.RuntimeError(error);
            }
        }

        private string Stringify(object Object) {
            if (Object == null) return "nil";

            if (Object is double) {
                string text = Object.ToString();
                if (text.EndsWith(".0")) {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return Object.ToString();
        }

        public object VisitWhileStmt(While stmt) {
            while (IsTruthy(Evaluate(stmt.condition))) {
                Execute(stmt.body);
            }
            return null;
        }

        public object VisitCallExpr(Expr.Call expr) {
            object callee = Evaluate(expr.callee);

            List<object> arguments = new();
            foreach(Expr argument in expr.arguments) {
                arguments.Add(Evaluate(argument));
            }

            ILoxCallable function = callee as ILoxCallable;

            if (callee is ILoxCallable) {
                throw new RuntimeError(expr.paren,"Can only call functions and classes.");
            }

            if (arguments.Count != function.Arity) {
                throw new RuntimeError(expr.paren, "Expected " + function.Arity + " arguments but got " + arguments.Count + ".");
            }

            return function.Call(this, arguments);
        }

        public object VisitGetExpr(Expr.Get expr) {
            object Object = Evaluate(expr.Object);
            if (Object is LoxInstance instance) {
                return instance.Get(expr.name);
            }

            throw new RuntimeError(expr.name, "Only instances have properties.");
        }
    }
}
