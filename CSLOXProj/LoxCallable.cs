using System;
using System.Collections.Generic;
using System.Text;

namespace CSLOXProj
{
    interface ILoxCallable
    {
        int Arity { get; }
        object Call(Interpreter interpreter, List<object> arguments);
    }
}