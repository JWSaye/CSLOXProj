using System.Collections.Generic;
using System.Linq;

namespace CSLOXProj {
    public class LoxFunction : ILoxCallable {
        private readonly Stmt.Function declaration;
        private readonly LoxEnvironment closure;
        private readonly bool isInitializer;

        public LoxFunction(Stmt.Function declaration, LoxEnvironment closure, bool isInitializer) {
            this.isInitializer = isInitializer;
            this.closure = closure;
            this.declaration = declaration;
        }

        public LoxFunction Bind(LoxInstance instance) {
            LoxEnvironment environment = new(closure);
            environment.Define("this", instance);
            return new LoxFunction(declaration, environment, isInitializer);
        }

        public override string ToString() {
            return "<fn " + declaration.name.lexeme + ">";
        }

        public object Call(Interpreter interpreter, List<object> arguments) {
            LoxEnvironment environment = new(closure);
            for (int i = 0; i < declaration.Params.Count(); i++) {
                environment.Define(declaration.Params[i].lexeme, arguments[i]);
            }

            try {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            catch (Return returnValue) {
                if (isInitializer) return closure.GetAt(0, "this");

                return returnValue.Value;
            }

            if (isInitializer) return closure.GetAt(0, "this");
            return null;
        }

        public int Arity {
            get {
                return declaration.Params.Count;
            }
        }
    }
}
