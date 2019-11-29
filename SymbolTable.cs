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

    public class Variables {
        public Type tipo;
        public string tipoVariable;
        public dynamic valor;
        public List<dynamic> valorLista;
        public bool esLista;

        public Variables(Type tipo, string tipoVariable, dynamic valor, bool esLista){
            this.tipo = tipo;
            this.tipoVariable = tipoVariable;
            this.esLista = esLista;
            this.valorLista = new List<dynamic>();

            if (valor != null)
                this.valor = valor;

            else
                switch (tipo) {
                    case Type.INT: this.valor = 0; break;
                    case Type.BOOL: this.valor = false;break;
                    case Type.STRING: this.valor = "''";break;
                    default: this.valorLista = new List<dynamic>(); break;
                    }
        }

        public Variables(Type tipo, string tipoVariable, List<dynamic> valor,bool esLista)
        {
            this.tipo = tipo;
            this.tipoVariable = tipoVariable;
            this.esLista = esLista;
            this.valor = null;

            if (valor != null)
                this.valorLista = valor;

            else
                switch (tipo)
                {
                    case Type.INT: this.valor = 0; break;
                    case Type.BOOL: this.valor = false; break;
                    case Type.STRING: this.valor = "''"; break;
                    default: this.valorLista = new List<dynamic>(); break;
                }
        }

        public override string ToString()
        {
            if(!this.esLista)
                return this.tipo+" " + this.tipoVariable+" "+this.valor;
            else {
                string aux = "{" + string.Join(",", this.valorLista.ToArray()) + "}";
                return this.tipo + " " + this.tipoVariable + " " +aux;
            }

        }

    }

    public class SymbolTable: IEnumerable<KeyValuePair<string, Variables>> {

        IDictionary<string, Variables> data = new SortedDictionary<string, Variables>();

        //-----------------------------------------------------------
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("Symbol Table\n");
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
        public Variables this[string key] {
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
        public IEnumerator<KeyValuePair<string, Variables>> GetEnumerator() {
            return data.GetEnumerator();
        }

        //-----------------------------------------------------------
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    }
}
