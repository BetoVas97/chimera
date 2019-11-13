/*
  Chimera compiler - Procedure Table.
  Tomas Bravo Ortiz A01376668
  Gerardo Ezequiel Magdaleno Hernandez A01377029
  Jesus Heriberto Vasquez Sanchez A01377358
*/
using System;
using System.Text;
using System.Collections.Generic;

namespace Chimera
{

    public class ProcedureTable : IEnumerable<KeyValuePair<string, ProcedureTable.Cell>>
    {

        IDictionary<string, ProcedureTable.Cell> data = new SortedDictionary<string, ProcedureTable.Cell>();

        //Cell
        public class Cell
        {
            //Constructor
            public Cell(Type type, bool isPredefined)
            {
                this.type = type;
                this.isPredefined=isPredefined;
                symbolTable = new SymbolTable();

            }
            public Type type { get; private set; }
            public bool isPredefined { get; private set }
            public SymbolTable symbolTable { get; private set}

            //Returned it
            public override string ToString()
            {
                string temp = "";
                temp += type;
                temp += isPredefined;
                pos += symbolTable;
                return temp;
            }
        }

        //-----------------------------------------------------------
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Procedure Table\n");
            sb.Append("====================\n");
            foreach (var entry in data)
            {
                //Check predefined functions
                if(entry.Value.isPredefined){
                    continue;
                }
                else{
                    sb.Append(String.Format("{0}: {1}\n",
                                                         entry.Key,
                                                         entry.Value));
                                                         }

            }
            sb.Append("====================\n");
            return sb.ToString();
        }

        //-----------------------------------------------------------
        //Now we adapt to procedureTable
        public ProcedureTable.Cell this[string key]
        {
            get
            {
                return data[key];
            }
            set
            {
                data[key] = value;
            }
        }

        //-----------------------------------------------------------
        public bool Contains(string key)
        {
            return data.ContainsKey(key);
        }

        //-----------------------------------------------------------
        public IEnumerator<KeyValuePair<string, Type>> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        //-----------------------------------------------------------
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}