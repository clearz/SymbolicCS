using System;
using System.Diagnostics;
using SymbolicCS.Parsing;
using SymbolicCS.Util;

namespace SymbolicCS
{
    class Program
    {
        private const string PROMPT = ":> ";
        static void Main(string[] args)
        {
            var parser = Parser.GetInstance();
            while (true)
            {
                try
                {
                    Console.Write(PROMPT);
                    var str = Console.ReadLine();
                    if (HandleCmd(str)) continue;

                    var exp = parser.Parse(str);
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("Parsed:\t");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    exp.Print();
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    exp.DisplayTree();
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("\nSimplified: ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    exp = exp.Simplify();
                    exp.Print();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    if (!(exp is Num || exp is Var))
                    {
                        Console.WriteLine();
                        exp.DisplayTree();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    Console.ResetColor();
                }

                bool HandleCmd(string str)
                {
                    switch (str)
                    {
                        case "exit":
                        case "quit":
                            Environment.Exit(0);
                            break;
                        case "cls":
                            Console.Clear();
                            break;
                        case "":
                            return true;
                        default:
                            return false;
                    }
                    return true;
                }
            }
        }
        

    }
}
