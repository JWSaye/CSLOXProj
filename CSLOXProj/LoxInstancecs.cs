using System.Collections.Generic;

namespace CSLOXProj {
    public class LoxInstance {
        private readonly LoxClass klass;
        private readonly HashMap<string, object> fields = new();

        public LoxInstance(LoxClass @klass) {
            this.klass = @klass;
        }

        public object Get(Token name) {
            if (fields.TryGetValue(name.lexeme, out object val)) {
                return val;
            }

            LoxFunction method = klass.FindMethod(this, name.lexeme);
            if (method != null) return method;

            throw new RuntimeError(name, "Undefined property '" + name.lexeme + "'.");
        }

        public void Set(Token name, object value) {
            fields.Put(name.lexeme, value);
        }

        public override string ToString() {
            return klass.name + " instance";
        }
    }
}
