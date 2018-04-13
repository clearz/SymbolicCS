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
                    Console.Write("Parsed:\n\t");
                    exp.Print();
                    Console.Write("Simplified:\n\t");
                    exp = exp.Simplify();
                    exp.Print();
                    Console.WriteLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
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
                        default:
                            return false;
                    }
                    return true;
                }
            }
        }
        

    }
}
