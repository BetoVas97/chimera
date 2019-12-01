/*
  Buttercup compiler - Symbol table class.
  Copyright (C) 2013 Ariel Ortiz, ITESM CEM
  
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

using System;
using System.Text;
using System.Collections.Generic;

namespace Chimera {

    public class Procedures {
        public Type tipoRetorno;
        public int numParametros;
        public string predefinido;
        public SymbolTable tablaLocal;
        public List<dynamic> ordenParametros;

        public Procedures(Type tipoRetorno, int numParametros, string predefinido, List<dynamic> ordenParametros, SymbolTable tablaLocal)
        {
            this.tipoRetorno = tipoRetorno;
            this.numParametros = numParametros;
            this.predefinido = predefinido;
            this.tablaLocal = tablaLocal;
            this.ordenParametros = ordenParametros;
        }

        public Type tipoRetorno { get; private set; }
        public int numParametros { get; private set;}
        public string predefinido { get; private set;}
        public List<dynamic> ordenParametros { get; private set;}
        public SymbolTable tablaLocal { get; private set;}

        public override string ToString()
        {
            string aux3 = "[";
            foreach(var a in this.ordenParametros) {
                aux3 += a+",";
            }
            if(aux3.Length>1)
                aux3= aux3.Substring(0, aux3.Length - 1);

            aux3 += "]";
            return this.tipoRetorno + " " + this.numParametros + " " + this.predefinido+" "+aux3;
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