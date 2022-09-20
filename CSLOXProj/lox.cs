using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace CSLOXProj
{
    public class Lox
    {
        static bool hadError = false;

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

            Console.WriteLine(new AstPrinter().Print(expression));
        }

        static public void Error(int line, string message)
        {
            Report(line, "", message);
        }

        static private void Report(int line, string where,
                                    string message)
        {
            Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }

        static public void Error(Token token, String message)
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
    }
}