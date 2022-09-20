using System.Text;

namespace CSLOXProj
{
    public class AstPrinter : Expr.Visitor<string> {
        public string Print(Expr expr) {
            return expr.Accept(this);
        }

        string Expr.Visitor<string>.VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.Operator.lexeme,
                                expr.left, expr.right);
        }

        string Expr.Visitor<string>.VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        string Expr.Visitor<string>.VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.value == null) return "nil";
            return expr.value.ToString();
        }

        string Expr.Visitor<string>.VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.Operator.lexeme, expr.right);
        }

        private string Parenthesize(string name,params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}
