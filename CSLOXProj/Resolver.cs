using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSLOXProj {

    class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object> {
        private readonly Interpreter interpreter;
        private readonly StackList<HashMap<string, bool?>> scopes;

        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;

        public Resolver(Interpreter interpreter) {
            this.interpreter = interpreter;
            scopes = new StackList<HashMap<string, bool?>>();
        }

        private enum FunctionType {
            NONE,
            FUNCTION,
            INITIALIZER,
            METHOD
        }

        private enum ClassType {
            NONE,
            CLASS,
            SUBCLASS
            
        }

        public void Resolve(List<Stmt> statements) {
            foreach(Stmt statement in statements) {
                Resolve(statement);
            }
        }

        private void Resolve(Stmt stmt) {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr) {
            expr.Accept(this);
        }

        public object Visit(Stmt.Block stmt) {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        public object Visit(Stmt.Class stmt) {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            Declare(stmt.name);
            Define(stmt.name);

            if (stmt.superclass != null && stmt.name.lexeme.Equals(stmt.superclass.name.lexeme)) {
                Lox.Error(stmt.superclass.name, "A class can't inherit from itself.");
            }

            if (stmt.superclass != null) {
                currentClass = ClassType.SUBCLASS;
                Resolve(stmt.superclass);

                BeginScope();
                scopes.Peek().Put("super", true);
            }

            BeginScope();
            scopes.Peek().Put("this", true);

            foreach (Stmt.Function method in stmt.methods) {
                FunctionType declaration = FunctionType.METHOD;
                if (method.name.lexeme.Equals("init")) {
                    declaration = FunctionType.INITIALIZER;
                }

                ResolveFunction(method, declaration);
            }

            EndScope();

            if (stmt.superclass != null) EndScope();

            currentClass = enclosingClass;
            return null;
        }


        public object Visit(Stmt.Expression stmt) {
            Resolve(stmt.expression);
            return null;
        }

        public object Visit(Stmt.Function stmt) {

            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object Visit(Stmt.If stmt) {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
            return null;
        }

        public object Visit(Stmt.Print stmt) {
            Resolve(stmt.expression);
            return null;
        }

        public object Visit(Stmt.Return stmt) {
            if (currentFunction == FunctionType.NONE) {
                Lox.Error(stmt.keyword, "Can't return from top-level code.");
            }

            if (stmt.value != null) {
                if (currentFunction == FunctionType.INITIALIZER) {
                    Lox.Error(stmt.keyword, "Can't return a value from an initializer.");
                }

                Resolve(stmt.value);
            }

            return null;
        }

        public object Visit(Stmt.Var stmt) {
            Declare(stmt.name);
            
            if (stmt.initializer != null) {
                Resolve(stmt.initializer);
            }

            Define(stmt.name);
            return null;
        }

        public object Visit(Stmt.While stmt) {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }

        public object Visit(Expr.Assign expr) {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object Visit(Expr.Binary expr) {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object Visit(Expr.Call expr) {
            Resolve(expr.callee);

            foreach (Expr argument in expr.arguments) {
                Resolve(argument);
            }

            return null;
        }

        public object Visit(Expr.Get expr) {
            Resolve(expr.Object);
            return null;
        }

        public object Visit(Expr.Grouping expr) {
            Resolve(expr.expression);
            return null;
        }

        public object Visit(Expr.Literal expr) {
            return null;
        }

        public object Visit(Expr.Logical expr) {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object Visit(Expr.Set expr) {
            Resolve(expr.value);
            Resolve(expr.Object);
            return null;
        }

        public object Visit(Expr.Super expr) {
            if (currentClass == ClassType.NONE) {
                Lox.Error(expr.keyword, "Can't use 'super' outside of a class.");
            }

            else if (currentClass != ClassType.SUBCLASS) {
                Lox.Error(expr.keyword, "Can't use 'super' in a class with no superclass.");
            }

            ResolveLocal(expr, expr.keyword);
            return null;
        }

        public object Visit(Expr.This expr) {
            if (currentClass == ClassType.NONE) {
                Lox.Error(expr.keyword, "Can't use 'this' outside of a class.");
                return null;
            }

            ResolveLocal(expr, expr.keyword);
            return null;
        }

        public object Visit(Expr.Unary expr) {
            Resolve(expr.right);
            return null;
        }

        public object Visit(Expr.Variable expr) {
            if (!scopes.IsEmpty() && scopes.Peek().Get(expr.name.lexeme) == false) {
                Lox.Error(expr.name, "Can't read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.name);
            return null;
        }

        private void BeginScope() {
            scopes.Push(new HashMap<string, bool?>());
        }

        private void EndScope() {
            scopes.Pop();
        }

        private void Declare(Token name) {
            if (!scopes.Any()) return;

            HashMap<string, bool?> scope = scopes.Peek();

            if (scope.ContainsKey(name.lexeme))
            {
                Lox.Error(name, "Already a variable with this name in this scope.");
            }

            scope[name.lexeme] = false;
        }

        private void Define(Token name) {
            if (scopes.Count == 0) return;
            scopes.Peek().Put(name.lexeme, true);
        }

        private void ResolveLocal(Expr expr, Token name) {
            for (int i = scopes.Count() - 1; i >= 0; i--) {
                if (scopes[i].ContainsKey(name.lexeme)) {
                    interpreter.Resolve(expr, scopes.Count() - 1 - i);
                    return;
                }
            }
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type) {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();

            foreach(Token param in function.Params) {
                Declare(param);
                Define(param);
            }

            Resolve(function.body);
            EndScope();

            currentFunction = enclosingFunction;
        }
    }
}