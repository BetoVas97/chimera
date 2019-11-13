/*
  Chimera compiler - Symbol Table.
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358
*/
using System;
using System.Text;
using System.Collections.Generic;

namespace Chimera {

    public class SymbolTable: IEnumerable<KeyValuePair<string, SymbolTable.Cell>> {

        IDictionary<string, SymbolTable.Cell> data = new SortedDictionary<string, SymbolTable.Cell>();

        //Cell
        public class Cell
        {
            //Constructor
            public Cell(Type ty, Kind ki, int pos)
            {
                this.type = ty;
                this.kind = ki;
                this.position = pos;

            }
            public Type type { get; private set; }
            public Kind kind { get; private set; }
            public int pos {get;}

            //Returned it
            public override string ToString()
            {
                string temp = "";
                temp += type;
                temp += kind;
                pos += pos;
                return temp;
            }
        }

        //-----------------------------------------------------------
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("Symbol Table\n");
            sb.Append("====================\n");
            foreach (var entry in data) {
                sb.Append(String.Format("{0}: {1}\n", 
                                        entry.Key, 
                                        entry.Value));
            }
            sb.Append("====================\n");
            return sb.ToString();
        }

        //-----------------------------------------------------------
        //Now we adapt to symbol table.
        public SymbolTable.Cell this[string key] {
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
        public IEnumerator<KeyValuePair<string, Type>> GetEnumerator() {
            return data.GetEnumerator();
        }

        //-----------------------------------------------------------
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    }
}

