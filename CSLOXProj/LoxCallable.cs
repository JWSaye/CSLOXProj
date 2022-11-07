using System.Collections.Generic;

namespace CSLOXProj
{
    public interface LoxCallable
    {
        int Arity { get; }
        object Call(Interpreter interpreter, List<object> arguments);
    }
}