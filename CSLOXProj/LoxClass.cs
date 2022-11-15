using System.Collections.Generic;

namespace CSLOXProj {
    public class LoxClass : ILoxCallable {
        public readonly string name;
        private readonly LoxClass superclass;
        private readonly Dictionary<string, LoxFunction> methods;

        public LoxClass(string name, LoxClass superclass, Dictionary<string, LoxFunction> methods) {
            this.superclass = superclass;
            this.name = name;
            this.methods = methods;
        }

        public LoxFunction FindMethod(string name) {
            if (methods.ContainsKey(name)) {
                return methods[name];
            }

            if (superclass != null) {
                return superclass.FindMethod(name);
            }

            return null;
        }

        public override string ToString() {
            return name;
        }

        public object Call(Interpreter interpreter, List<object> arguments) {
            LoxInstance instance = new(this);
            LoxFunction initializer = FindMethod("init");
            if (initializer != null) {
                initializer.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }

        public int Arity {
            get {
                LoxFunction initializer = FindMethod("init");
                return (initializer == null) ? 0 : initializer.Arity;
            }
        }
    }
}
