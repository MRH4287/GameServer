using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    class Config
    {
        XML.XMLTree config;
        XML.XMLParser parser = new XML.XMLParser();

        System.Collections.Hashtable defaultValues = new System.Collections.Hashtable();

        public Config()
        {


        }



        public void parse(string path)
        {
            config = parser.read(path);
        }

        public string this[string index]
        {
            get
            {
                try
                {
                    XML.XMLTree element = config;
                    string[] split = index.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string key in split)
                    {
                        element = element.getChild(key);
                    }


                    return element.getValue();
                }
                catch
                {
                    if (defaultValues[index] != null)
                    {
                        return (string)defaultValues[index];
                    }
                    else
                    {
                        throw new Game.GameException("Wert in der Config Datei nicht gefunden: " + index);
                    }
                }
            }

        }

       

    }
}
