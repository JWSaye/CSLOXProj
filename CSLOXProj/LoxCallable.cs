using System;
using System.Collections.Generic;

namespace CSLOXProj
{
    interface LoxCallable
    {
        int arity();
        object Call(Interpreter, List<object> arguments);
    }
}