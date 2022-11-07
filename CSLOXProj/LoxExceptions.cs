using System;

namespace CSLOXProj
{
    public abstract class LoxExceptions : Exception
    {
        public LoxExceptions() : base()
        {

        }

        public LoxExceptions(string message) : base(message)
        {

        }
    }

    public class ParserException : LoxExceptions
    {
    }

    public class RuntimeError : LoxExceptions
    {
        public Token token { get; protected set; }

        public RuntimeError(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
}
