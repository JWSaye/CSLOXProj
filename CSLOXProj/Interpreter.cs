using System;
using System.Collections.Generic;
using System.Linq;
using static CSLOXProj.Stmt;

namespace CSLOXProj {

    public class Interpreter : Expr.IVisitor<object>, IVisitor<object> {
        public readonly LoxEnvironment globals;
        private LoxEnvironment environment;
        private readonly HashMap<Expr, int?> locals = new();

        public Interpreter() {
            globals = new LoxEnvironment();
            environment = globals;
            DefineNativeFunctions();
        }

        private void DefineNativeFunctions() {
            ILoxCallable clock = new Clocks();
            globals.Define("clock", clock);
        }

        private object LookUpVariable(Token name, Expr expr) {
            int? distance = locals.Get(expr);

            if (distance != null) {
                return environment.GetAt(distance.Value, name.lexeme);
            }
            else {
                return globals.Get(name);
            }
        }

        private object Evaluate(Expr expr) {
            return expr.Accept(this);
        }

        private object Execute(Stmt stmt) {
            if(stmt != null) return stmt.Accept(this);
            return null;
        }

        public void Resolve(Expr expr, int depth) {
            locals.Put(expr, depth);
        }

        public void ExecuteBlock(List<Stmt> statements, LoxEnvironment environment) {
            LoxEnvironment previous = this.environment;
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

        private bool IsTruthy(object truthyObject)
        {
            if (truthyObject == null) return false;
            if (truthyObject is bool boolean) return boolean;
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

        private void CheckNumberOperands(Token Operator, object left, object right)
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

            if (Object is double)
            {
                string text = Object.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return Object.ToString();
        }

        public object Visit(Expr.Literal expr)
        {
            return expr.value;
        }

        public object Visit(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);

            if (expr.Operator.type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.right);
        }

        public object Visit(Expr.Set expr)
        {
            object Object = Evaluate(expr.Object);

            if (Object is not LoxInstance)
            {
                throw new RuntimeError(expr.name, "Only instances have fields.");
            }

            object value = Evaluate(expr.value);
            ((LoxInstance)Object).Set(expr.name, value);
            return value;
        }

        public object Visit(Expr.Super expr)
        {
            int? distance = locals.Get(expr);
            LoxClass superclass = (LoxClass)environment.GetAt(distance.Value, "super");

            LoxInstance Object = (LoxInstance)environment.GetAt(distance.Value - 1, "this");

            LoxFunction method = superclass.FindMethod(Object, expr.method.lexeme);

            if (method == null)
            {
                throw new RuntimeError(expr.method, "Undefined property '" + expr.method.lexeme + "'.");
            }

            return method;
        }

        public object Visit(Expr.This expr)
        {
            return LookUpVariable(expr.keyword, expr);
        }

        public object Visit(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object Visit(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.Operator.type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr.Operator, right);
                    return -(double)right;
            }

            // Unreachable.
            return null;
        }

        public object Visit(Expr.Variable expr)
        {
            return LookUpVariable(expr.name, expr);
        }

        public object Visit(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.Operator.type)
            {
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
                    if (left is double @double && right is double double1)
                    {
                        return @double + double1;
                    }

                    if (left is string @string && right is string string1)
                    {
                        return @string + string1;
                    }

                    throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.");
            }

            // Unreachable.
            return null;
        }

        public object Visit(Block stmt) {
            ExecuteBlock(stmt.statements, new LoxEnvironment(environment));
            return null;
        }

        public object Visit(Class stmt) {
            environment.Define(stmt.name.lexeme, null);
            object superclass = null;

            if (stmt.superclass != null) {
                superclass = Evaluate(stmt.superclass);
                if (superclass is not LoxClass) {
                    throw new RuntimeError(stmt.superclass.name, "Superclass must be a class.");
                }

                environment = new LoxEnvironment(environment);
                environment.Define("super", superclass);
            }

            HashMap<string, LoxFunction> methods = new();
            foreach (Function method in stmt.methods) {
                LoxFunction function = new(method, environment, method.name.lexeme.Equals("init"));
                methods.Put(method.name.lexeme, function);
            }

            LoxClass @klass = new(stmt.name.lexeme, (LoxClass)superclass, methods);

            if (superclass != null) {
                environment = environment.enclosing;
            }

            environment.Assign(stmt.name, @klass);
            return null;
        }

        public object Visit(Expression stmt) {
            Evaluate(stmt.expression);
            return null;
        }

        public object Visit(Function stmt) {
            LoxFunction function = new(stmt, environment, false);
            environment.Define(stmt.name.lexeme, function);
            return null;
        }

        public object Visit(If stmt) {
            if (IsTruthy(Evaluate(stmt.condition))) {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null) {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object Visit(Print stmt) {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object Visit(Stmt.Return stmt) {
            object value = null;
            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new Return(value);
        }

        public object Visit(Var stmt) {
            object value = null;
            if (stmt.initializer != null) {
                value = Evaluate(stmt.initializer);
            }
            
            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object Visit(Expr.Assign expr) {
            object value = Evaluate(expr.value);

            int? distance = locals.Get(expr);
            if (distance != null) {
                environment.AssignAt(distance.Value, expr.name, value);
            }
            else {
                globals.Assign(expr.name, value);
            }

            return value;
        }

        public object Visit(While stmt) {
            while (IsTruthy(Evaluate(stmt.condition))) {
                Execute(stmt.body);
            }
            return null;
        }

        public object Visit(Expr.Call expr) {
            object callee = Evaluate(expr.callee);

            List<object> arguments = new();
            foreach(Expr argument in expr.arguments) {
                arguments.Add(Evaluate(argument));
            }

            if (callee is not ILoxCallable) {
                throw new RuntimeError(expr.paren,"Can only call functions and classes.");
            }

            ILoxCallable function = callee as ILoxCallable;

            if (arguments.Count() != function.Arity) {
                throw new RuntimeError(expr.paren, "Expected " + function.Arity + " arguments but got " + arguments.Count + ".");
            }

            return function.Call(this, arguments);
        }

        public object Visit(Expr.Get expr) {
            object Object = Evaluate(expr.Object);
            if (Object is LoxInstance instance) {
                return instance.Get(expr.name);
            }

            throw new RuntimeError(expr.name, "Only instances have properties.");
        }
    }
}
