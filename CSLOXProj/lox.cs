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
                Console.WriteLine("Usage: jlox [script]");
                Environment.Exit(64);
            }

            else if (args.Length == 1)
            {
                runFile(args[0]);
            }

            else
            {
                runPrompt();
            }
        }

        static private void runFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            run(BitConverter.ToString(bytes));

            if (hadError) Environment.Exit(65);
        }

        static private void runPrompt()
        {
            for (; ; )
            {
                Console.WriteLine("> ");
                string line = Console.ReadLine();
                if (line == null) break;
                run(line);
                hadError = false;
            }
        }

        static private void run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            // For now, just print the tokens.
            foreach (Token token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        static public void error(int line, string message)
        {
            report(line, "", message);
        }

        static private void report(int line, string where,
                                    string message)
        {
            Console.WriteLine(
                "[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }
    }
}