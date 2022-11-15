using System;
using System.Collections.Generic;

namespace CSLOXProj
{
    public class Clocks : ILoxCallable
    {
        public int Arity
        {
            get { return 0; }
        }

        public object Call(Interpreter interpreter, List<object> arguements)
        {
            return DateTime.Now.Second;
        }

        public override string ToString() { return "<native fn>"; }
    }
}
