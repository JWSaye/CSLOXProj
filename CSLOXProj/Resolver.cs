using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSLOXProj
{
    class Resolver : Expr.IVisitor<Void>, Stmt.IVisitor<Void> {
        private readonly Interpreter interpreter;
        private readonly List<Dictionary<string, bool>> scopes;
        private FunctionType currentFunction = FunctionType.NONE;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
            scopes = new List<Dictionary<string, bool>>();
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach(Stmt statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);
            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Lox.Error(stmt.keyword, "Can't return from top-level code.");
            }

            if (stmt.value != null)
            {
                Resolve(stmt.value);
            }

            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.name);
            
            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }

            Define(stmt.name);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.callee);

            foreach (Expr argument in expr.arguments)
            {
                Resolve(argument);
            }

            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.expression);
            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.right);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            if (scopes.Count == 0 &&
                scopes[0][expr.name.lexeme] == false)
            {
                Lox.Error(expr.name,
                    "Can't read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.name);
            return null;
        }

        private void BeginScope()
        {
            scopes.Append(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.RemoveAt(scopes.Count);
        }

        private void Declare(Token name)
        {
            if (scopes.Count == 0) return;

            Dictionary<string, bool> scope = scopes[0];

            if (scope.ContainsKey(name.lexeme))
            {
                Lox.Error(name,
                    "Already a variable with this name in this scope.");
            }

            scope[name.lexeme] = false;
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0) return;
            Dictionary<string, bool> scope = scopes[0];
            scope[name.lexeme] = true;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i].ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count - 1 - i);
                    return;
                }
            }
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;
            BeginScope();
            foreach(Token param in function.Params)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.body);
            EndScope();
            currentFunction = enclosingFunction;
        }
    }
}
