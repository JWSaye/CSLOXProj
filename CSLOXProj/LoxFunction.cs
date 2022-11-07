using System.Collections.Generic;

namespace CSLOXProj
{
    public class LoxFunction : LoxCallable
    {
        private readonly Stmt.Function declaration;
        public LoxFunction(Stmt.Function declaration)
        {
            this.declaration = declaration;
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + ">";
        }

        public object Call(Interpreter interpreter,
                     List<object> arguments)
        {
            Environment environment = new Environment(interpreter.globals);
            for (int i = 0; i < declaration.Params.Count; i++)
            {
                environment.Define(declaration.Params[i].lexeme,
                    arguments[i]);
            }

            interpreter.ExecuteBlock(declaration.body, environment);
            return null;
        }

        public int Arity
        {
            get
            {
                return declaration.Params.Count;
            }
        }
    }
}
