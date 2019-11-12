/*
  Chimera compiler - Program driver.
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358
*/

using System;
using System.IO;
using System.Text;

namespace Chimera {

    public class Driver {

        const string VERSION = "0.3";

        //-----------------------------------------------------------
        static readonly string[] ReleaseIncludes = {
            "Lexical analysis","Syntax analysis","AST construction"
        };

        //-----------------------------------------------------------
        void PrintAppHeader() {
            Console.WriteLine("Chimera compiler, version " + VERSION);
            Console.WriteLine("Copyright \u00A9 2013 by A. Ortiz, ITESM CEM."                
            );
            Console.WriteLine("This program is free software; you may "
                + "redistribute it under the terms of");
            Console.WriteLine("the GNU General Public License version 3 or "
                + "later.");
            Console.WriteLine("This program has absolutely no warranty.");
        }

        //-----------------------------------------------------------
        void PrintReleaseIncludes() {
            Console.WriteLine("Included in this release:");            
            foreach (var phase in ReleaseIncludes) {
                Console.WriteLine("   * " + phase);
            }
        }

        //-----------------------------------------------------------
        void Run(string[] args) {

            PrintAppHeader();
            Console.WriteLine();
            PrintReleaseIncludes();
            Console.WriteLine();

            if (args.Length != 1) {
                Console.Error.WriteLine(
                    "Please specify the name of the input file.");
                Environment.Exit(1);
            }

            string errorLine = "";
            try {            
                var inputPath = args[0];                
                var input = File.ReadAllText(inputPath);
                


                //Console.WriteLine(String.Format(
                //    "===== Tokens from: \"{0}\" =====", inputPath)
                //);
                //Comentado, código errores.
                /*var count = 1;
                foreach (var tok in new Scanner(input).Start()) {
                     //Console.WriteLine(String.Format("[{0}] {1}", count++, tok));
                     errorLine = String.Format("[{0}] {1}", count++, tok);
                }*/


                var parser = new Parser(new Scanner(input).Start().GetEnumerator());
                var program = parser.Program();
                Console.WriteLine(program.ToStringTree());

            } catch (Exception e) {

                if (e is FileNotFoundException || e is SyntaxError) {
                    Console.WriteLine(errorLine);
                    Console.Error.WriteLine(e.Message);
                    Environment.Exit(1);
                }

                throw;
            }        
        }

        //-----------------------------------------------------------
        public static void Main(string[] args) {
            new Driver().Run(args);
        }
    }
}
