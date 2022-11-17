using System.Collections.Generic;

namespace CSLOXProj {
    public class LoxClass : ILoxCallable {
        public readonly string name;
        public readonly LoxClass superclass;
        private readonly HashMap<string, LoxFunction> methods;

        public LoxClass(string name, LoxClass superclass, HashMap<string, LoxFunction> methods) {
            this.superclass = superclass;
            this.name = name;
            this.methods = methods;
        }

        public LoxFunction FindMethod(LoxInstance instance,  string name) {
            if (methods.ContainsKey(name)) {
                return methods.Get(name).Bind(instance);
            }

            if (superclass != null) {
                return superclass.FindMethod(instance, name);
            }

            return null;
        }

        public override string ToString() {
            return name;
        }

        public object Call(Interpreter interpreter, List<object> arguments) {
            LoxInstance instance = new(this);
            LoxFunction initializer = methods.Get("init");
            if (initializer != null) {
                initializer.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }

        public int Arity {
            get {
                LoxFunction initializer = methods.Get("init");
                return (initializer == null) ? 0 : initializer.Arity;
            }
        }
    }
}
