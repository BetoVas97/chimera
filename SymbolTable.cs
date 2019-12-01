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

    public class Variables {
        public Type type;
        public string kind;
        public dynamic value;
        public List<dynamic> valueList;
        public bool isList;

        public Variables(Type type, string kind, dynamic value, bool isList){
            this.type = type;
            this.kind = kind;
            this.isList = isList;
            this.valueList = new List<dynamic>();

            if (value != null){
                this.value = value;
            }                

            else{
                switch (type) {
                    case Type.INT: this.value = 0; break;
                    case Type.BOOL: this.value = false; break;
                    case Type.STRING: this.value = "''";break;
                    default: this.valueList = new List<dynamic>(); break;
                }
            }
                
        }

        public Variables(Type type, string kind, List<dynamic> value,bool isList)
        {
            this.type = type;
            this.kind = kind;
            this.isList = isList;
            this.value = null;

            if (value != null){
                this.valueList = value;
            }                

            else{
                switch (type)
                {
                    case Type.INT: this.value = 0; break;
                    case Type.BOOL: this.value = false; break;
                    case Type.STRING: this.value = "''"; break;
                    default: this.valueList = new List<dynamic>(); break;
                }
            }
        }

        public override string ToString()
        {
            string temp="";
            if(!this.isList){                
                temp +=this.type+" ";
                temp +=this.kind+" ";
                temp +=this.value;
                return temp;  
            }
                          
            else {
                string temp2 = "{" + string.Join(",", this.valueList.ToArray()) + "}";
                temp +=this.type + " ";
                temp +=this.kind + " ";
                temp +=temp2;
                return temp;
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
