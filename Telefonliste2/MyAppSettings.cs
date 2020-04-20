using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace FK.Telefonliste
{
    public class MyAppSettings
    {
        

        private SerializableDictionary<string, string> _usedFiles = new SerializableDictionary<string,string>();



        public SerializableDictionary<string, string> UsedFiles
        {
            get 
            {
                return _usedFiles;
            }
            set
            {
                _usedFiles = value;
            }
        }
        

        [XmlIgnore]
        public string LastFile
        {
            get
            {
                string s;

                _usedFiles.TryGetValue(Environment.MachineName, out s);

                return s;
            }

            set
            {
                if (value == null)
                {
                    if (_usedFiles.ContainsKey(Environment.MachineName))
                    {
                        _usedFiles.Remove(Environment.MachineName);
                    }
                }
                else
                {
                    _usedFiles[Environment.MachineName] = value;
                }
            }
        }
    }
}
