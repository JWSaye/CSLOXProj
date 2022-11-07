using System.Collections.Generic;

namespace CSLOXProj
{
    public class LoxFunction : LoxCallable
    {
        private readonly Stmt.Function declaration;
        private readonly Environment closure;
        public LoxFunction(Stmt.Function declaration, Environment closure)
        {
            this.closure = closure;
            this.declaration = declaration;
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + ">";
        }

        public object Call(Interpreter interpreter,
                     List<object> arguments)
        {
            Environment environment = new Environment(closure);
            for (int i = 0; i < declaration.Params.Count; i++)
            {
                environment.Define(declaration.Params[i].lexeme,
                    arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            catch (Return returnValue)
            {
                return returnValue.value;
            }
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
