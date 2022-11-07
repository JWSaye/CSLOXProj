﻿using System.Collections.Generic;
using static CSLOXProj.Expr;
using static CSLOXProj.Stmt;

namespace CSLOXProj
{
    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitBlockStmt(Block stmt);
            //R VisitClassStmt(Class stmt);
            R VisitExpressionStmt(Expression stmt);
            R VisitFunctionStmt(Function stmt);
            R VisitIfStmt(If stmt);
            R VisitPrintStmt(Print stmt);
            //R VisitReturnStmt(Return stmt);
            R VisitVarStmt(Var stmt);
            R VisitWhileStmt(While stmt);
        }

        // Nested Stmt classes here...
        public class Block : Stmt
        {
            public Block(List<Stmt> statements) {
                this.statements = statements;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }

            public readonly List<Stmt> statements;
        }
        /*
        public class Class : Stmt
        {
            Class(Token name,
                  Expr.Variable superclass,
                  List<Stmt.Function> methods) {
                this.name = name;
                this.superclass = superclass;
                this.methods = methods;
            }
            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitClassStmt(this);
            }

            public readonly Token name;
            public readonly Expr.Variable superclass;
            public readonly List<Stmt.Function> methods;
        }
        */
        public class Expression : Stmt
        {
            public Expression(Expr expression) {
              this.expression = expression;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }

            public readonly Expr expression;
        }
        public class If : Stmt
        {
            public If(Expr condition, Stmt thenBranch, Stmt elseBranch) {
              this.condition = condition;
              this.thenBranch = thenBranch;
              this.elseBranch = elseBranch;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
            }

            public readonly Expr condition;
            public readonly Stmt thenBranch;
            public readonly Stmt elseBranch;
        }

        public class Print : Stmt
        {
            public Print(Expr expression) {
                this.expression = expression;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }

            public readonly Expr expression;
        }
        /*
        public class Return : Stmt
        {
            public Return(Token keyword, Expr value) {
                this.keyword = keyword;
                this.value = value;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitReturnStmt(this);
            }

            public readonly Token keyword;
            public readonly Expr value;
        }
        */
        public class Var : Stmt
        {
            public Var(Token name, Expr initializer) {
                this.name = name;
                this.initializer = initializer;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }

            public readonly Token name;
            public readonly Expr initializer;
        }

        public class While : Stmt
        {
            public While(Expr condition, Stmt body) {
                this.condition = condition;
                this.body = body;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }

            public readonly Expr condition;
            public readonly Stmt body;
        }

        public class Function : Stmt
        {
            public Function(Token name, List<Token> Params, List<Stmt> body) {
              this.name = name;
              this.Params = Params;
              this.body = body;
            }

            public override R Accept<R>(IVisitor<R> visitor) { 
                return visitor.VisitFunctionStmt(this);
            }

            public readonly Token name;
            public readonly List<Token> Params;
            public readonly List<Stmt> body;
        }
public abstract R Accept<R>(IVisitor<R> visitor);
    }
}
