using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSLOXProj
{
    public class LoxInstance
    {
        private readonly LoxClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            this.klass = klass;
        }

        public object Get(Token name)
        {
            if (fields.ContainsKey(name.lexeme))
            {
                return fields[name.lexeme];
            }

            LoxFunction method = klass.FindMethod(name.lexeme);
            if (method != null) return method.Bind(this);

            throw new RuntimeError(name,
                "Undefined property '" + name.lexeme + "'.");
        }

        public void Set(Token name, Object value)
        {
            fields.Add(name.lexeme, value);
        }

        public override string ToString()
        {
            return klass.name + " instance";
        }
    }
}
