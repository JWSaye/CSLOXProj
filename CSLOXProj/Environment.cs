using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSLOXProj
{
    public class Environment
    {
        private Environment enclosing;
        private Dictionary<string, object> values;

        public Environment()
        {
            values = new Dictionary<string, object>();
            enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }
        public object Get(Token name)
        {
            if (values.ContainsKey(name.lexeme))
            {
                return values[name.lexeme];
            }


            if (enclosing != null) return enclosing.Get(name);

            throw new LoxExceptions(name,
                "Undefined variable '" + name.lexeme + "'.");
        }

        public void Assign(Token name, Object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new LoxExceptions(name,
                "Undefined variable '" + name.lexeme + "'.");
        }

        public void Define(string name, object value)
        {
            values.Add(name, value);
        }


    }
}
