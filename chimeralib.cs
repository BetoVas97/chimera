/*
  Buttercup runtime library.
  Copyright (C) 2013 Ariel Ortiz, ITESM CEM

  To compile this module as a DLL:

                mcs /t:library bcuplib.cs

  To link this DLL to a program written in C#:

                mcs /r:bcuplib.dll someprogram.cs

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.
  
  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.
  
  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections;

namespace Chimera {
    
    using System;

    public class Utils {

        public static void WrStr(string str) {
            Console.Write(str);
        }

        public static void WrInt(int i) {
            Console.Write(i);
        }
        
        public static void WrBool(bool b) {
            Console.Write(b ? "true" : "false");
        }
        public static void WrLn() {
            Console.WriteLine();
        }
        public static string RdStr(){
            string input = Console.ReadLine();
            return input;
        }

        public static int RdInt(){
            string input = Console.ReadLine();
            try {
                Convert.ToInt32(input);
            }
            catch (System.Exception) {
                
                throw new Exception("Invalid integer at method RdInt");
            }
            return Convert.ToInt32(input);
        }
        public static int CmpStr(string s1, string s2){
            return string.Compare(s1,s2);
        }
        public static string IntToStr(int number){
            return number.ToString();
        }
        public static string CatStr(string s1, string s2){
            return s1+s2;
        }
        public static int LenStr(string input){
            return input.Length;
        }
        public static string AtStr(string input, int index){
            if(index > input.Length-1){
                throw new Exception("Error in method AtStr: index out of bounds");
            }
            return input[index].ToString();
        }
    }
}
