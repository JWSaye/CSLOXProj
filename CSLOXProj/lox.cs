using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace CSLOXProj
{
    public class Lox
    {
        private static readonly Interpreter interpreter = new Interpreter();
        static bool hadError = false;
        static bool hadRuntimeError = false;

        static public void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: .\\CSLOXProj [script]");
                Environment.Exit(64);
            }

            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }

            else
            {
                RunPrompt();
            }
        }
      
        static private void RunFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Run(Encoding.UTF8.GetString(bytes, 0, bytes.Length));

            if (hadError) Environment.Exit(65);
            if (hadRuntimeError) Environment.Exit(70);
        }

        static private void RunPrompt()
        {
            for (; ; )
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line == null) break;
                Run(line);
                hadError = false;
            }
        }

        static private void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);
            Expr expression = parser.Parse();

            if (hadError) return;

            interpreter.Interpret(expression);
        }

        static public void Error(int line, string message)
        {
            Report(line, "", message);
        }

        static private void Report(int line, string where,
                                    string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
            Console.ResetColor();
        }

        static public void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, " at end", message);
            }
            else
            {
                Report(token.line, " at '" + token.lexeme + "'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(error.Message +
                "\n[line " + error.token.line + "]");

            hadRuntimeError = true;
            Console.ResetColor();
        }
    }
}