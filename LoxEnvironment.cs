using System;
using System.Collections.Generic;

namespace CSLOXProj
{
    public class LoxEnvironment
    {
        public LoxEnvironment enclosing;
        private readonly Dictionary<string, object> values;

        public LoxEnvironment()
        {
            values = new();
            enclosing = null;
        }

        public LoxEnvironment(LoxEnvironment enclosing)
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

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }

        public void Assign(Token name, object value)
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

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }

        public void Define(string name, object value)
        {
            values[name] = value;
        }

        public LoxEnvironment Ancestor(int distance)
        {
            LoxEnvironment LoxEnvironment = this;
            for (int i = 0; i < distance; i++)
            {
                LoxEnvironment = environment.enclosing;
            }

            return environment;
        }

        public object GetAt(int distance, string name)
        {
            return Ancestor(distance).values[name];
        }

        public void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance).values[name.lexeme] = value;
        }
    }
}
