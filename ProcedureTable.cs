/*
  Chimera compiler - Procedure Table.
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358
*/

using System;
using System.Text;
using System.Collections.Generic;

namespace Chimera {

    public class Procedures {
        public Type returnedType;
        public int noParameters;
        public string predefined;
        public SymbolTable localSymbolTable;
        public List<dynamic> parametersOrder;
        public List<string> parametersNamesOrder;

        public Procedures(Type returnedType, int noParameters, string predefined, List<dynamic> parametersOrder, SymbolTable localSymbolTable, List<string> namesParamsOrder)
        {
            this.returnedType = returnedType;
            this.noParameters = noParameters;
            this.predefined = predefined;
            this.localSymbolTable = localSymbolTable;
            this.parametersOrder = parametersOrder;
            this.parametersNamesOrder = namesParamsOrder;
        }

        public override string ToString()
        {
            string temp="";
            string inside = "[";
            string insideNames = "[";

            foreach(var a in this.parametersOrder) {
                inside += a+",";
            }
            if(inside.Length>1){
                inside= inside.Substring(0, inside.Length - 1);
            }

            foreach(var a in this.parametersNamesOrder) {
                insideNames += a+",";
            }
            if(insideNames.Length>1){
                insideNames= insideNames.Substring(0, insideNames.Length - 1);
            }
                
            inside += "]";
            temp += this.returnedType + " ";
            temp +=this.noParameters + " ";
            temp +=this.predefined+" ";
            temp +=inside;
            temp += insideNames;
            return temp;
        }

    }

    public class ProcedureTable: IEnumerable<KeyValuePair<string, Procedures>> {

        IDictionary<string, Procedures> data = new SortedDictionary<string, Procedures>();

        //-----------------------------------------------------------
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("Procedures Table\n");
            sb.Append("====================\n");
            foreach (var entry in data) {
                sb.Append(String.Format("{0}: {1}\n", 
                                        entry.Key, 
                                        entry.Value.ToString()));
            }
            sb.Append("====================\n");
            return sb.ToString();
        }

        //-----------------------------------------------------------
        public Procedures this[string key] {
            get {
                return data[key];
            }
            set {
                data[key] = value;
            }
        }

        //-----------------------------------------------------------
        public bool Contains(string key) {
            return data.ContainsKey(key);
        }

        //-----------------------------------------------------------
        public IEnumerator<KeyValuePair<string, Procedures>> GetEnumerator() {
            return data.GetEnumerator();
        }

        //-----------------------------------------------------------
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    }
}

