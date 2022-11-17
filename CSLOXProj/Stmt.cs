using System.Collections.Generic;

namespace CSLOXProj {
    public abstract class Stmt {
        public interface IVisitor<R> {
            R Visit(Block stmt);
            R Visit(Class stmt);
            R Visit(Expression stmt);
            R Visit(Function stmt);
            R Visit(If stmt);
            R Visit(Print stmt);
            R Visit(Return stmt);
            R Visit(Var stmt);
            R Visit(While stmt);
        }

        // Nested Stmt classes here...
        public class Block : Stmt {
            public Block(List<Stmt> statements) {
                this.statements = statements;
            }

            public override R Accept<R>(IVisitor<R> visitor) {
                return visitor.Visit(this);
            }

            public readonly List<Stmt> statements;
        }
        
        public class Class : Stmt {
            public Class(Token name, Expr.Variable superclass, List<Function> methods) {
                this.name = name;
                this.superclass = superclass;
                this.methods = methods;
            }
            public override R Accept<R>(IVisitor<R> visitor) {
                return visitor.Visit(this);
            }

            public readonly Token name;
            public readonly Expr.Variable superclass;
            public readonly List<Function> methods;
        }
        
        public class Expression : Stmt {
            public Expression(Expr expression) {
              this.expression = expression;
            }

            public override R Accept<R>(IVisitor<R> visitor) {
                return visitor.Visit(this);
            }

            public readonly Expr expression;
        }
        public class If : Stmt {
            public If(Expr condition, Stmt thenBranch, Stmt elseBranch) {
              this.condition = condition;
              this.thenBranch = thenBranch;
              this.elseBranch = elseBranch;
            }

            public override R Accept<R>(IVisitor<R> visitor) {
                return visitor.Visit(this);
            }

            public readonly Expr condition;
            public readonly Stmt thenBranch;
            public readonly Stmt elseBranch;
        }

        public class Print : Stmt {
            public Print(Expr expression) {
                this.expression = expression;
            }

            public override R Accept<R>(IVisitor<R> visitor) {
                return visitor.Visit(this);
            }

            public readonly Expr expression;
        }
        
        public class Return : Stmt {
            public Return(Token keyword, Expr value) {
                this.keyword = keyword;
                this.value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor) {
                return visitor.Visit(this);
            }

            public readonly Token keyword;
            public readonly Expr value;
        }
        
        public class Var : Stmt {
            public Var(Token name, Expr initializer) {
                this.name = name;
                this.initializer = initializer;
            }

            public override R Accept<R>(IVisitor<R> visitor) {
                return visitor.Visit(this);
            }

            public readonly Token name;
            public readonly Expr initializer;
        }

        public class While : Stmt {
            public While(Expr condition, Stmt body) {
                this.condition = condition;
                this.body = body;
            }

            public override R Accept<R>(IVisitor<R> visitor) {
                return visitor.Visit(this);
            }

            public readonly Expr condition;
            public readonly Stmt body;
        }

        public class Function : Stmt {
            public Function(Token name, List<Token> Params, List<Stmt> body) {
              this.name = name;
              this.Params = Params;
              this.body = body;
            }

            public override R Accept<R>(IVisitor<R> visitor) { 
                return visitor.Visit(this);
            }

            public readonly Token name;
            public readonly List<Token> Params;
            public readonly List<Stmt> body;
        }
        public abstract R Accept<R>(IVisitor<R> visitor);
    }
}
