using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;


namespace XML
{
    class XMLParser
    {
        private XMLTree element = null;
        private XmlDocument document = new XmlDocument();


        /// <summary>
        /// Liest eine XML Datei ein und liefert ein <see cref="XMLTree"/> Objekt
        /// </summary>
        /// <param name="path">relative Pfad Angabe</param>
        /// <returns>Liefert ein <see cref="XMLTree"/> Objekt</returns>
        public XMLTree read(string path)
        {
            XmlTextReader reader = new XmlTextReader(path);
            document.Load(reader);
            element = null;

            foreach (XmlLinkedNode read in document)
            {
                parse(read);
            }
            reader.Close();
           
            return element;
        }

        /// <summary>
        /// Liest einen String ein und liefert den XMLTree
        /// </summary>
        /// <param name="input">XML Eingabetext</param>
        /// <returns>XMLTree Instanz</returns>
        public XMLTree readString(string input)
        {
            StringReader reader = new StringReader(input);
            XmlReader xmlreader = XmlReader.Create(reader);
            document.Load(xmlreader);

            element = null;

            foreach (XmlLinkedNode read in document)
            {
                parse(read);
            }
            return element;


        }

        /// <summary>
        /// Verarbeitet die XML Datei
        /// </summary>
        private void parse(XmlLinkedNode read)
        {

            if (read.NodeType == XmlNodeType.Element)
            {

                XMLTree newElement = new XMLTree(element, read.Name);

                element = newElement;

                foreach (XmlAttribute atribute in read.Attributes)
                {
                    element.addAtribute(atribute.Name, atribute.Value);
                }

                foreach (XmlLinkedNode node in read.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Text)
                    {
                        element.setValue(node.Value);
                    }
                    else
                    {
                        parse(node);
                    }
                }

                XMLTree parent = element.getParent();
                if (parent != null)
                {
                    element = parent;
                }

            }


        }

        /// <summary>
        /// Speicher den Baum in ein XMLDocument Objekt
        /// </summary>
        /// <param name="element">Das zu speichernde Objekt</param>
        /// <returns>XMLObjekt Instanz</returns>
        public XmlDocument save(XMLTree element)
        {
            document = new XmlDocument();

            document.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            
            XmlNode el = write(element, null);
            document.AppendChild(el);

            return document;
            
           

        }

        /// <summary>
        /// Speichert den Baum in eine Datei
        /// </summary>
        /// <param name="element">Das zu speichernde Objekt</param>
        /// <param name="filename">Pfad zur Datei</param>
        /// <returns></returns>
        public XmlDocument save(XMLTree element, string filename)
        {
            XmlWriter writer = XmlWriter.Create(filename);
            XmlDocument document = save(element);

            document.WriteTo(writer);
            writer.Close();

            return document;


        }

        /// <summary>
        /// Rekursiver Speicheralgorithmus
        /// </summary>
        /// <param name="element">Das zu speichernde Objekt</param>
        /// <param name="parent">Vater Knoten</param>
        /// <returns>erstellter Hauptknoten</returns>
        private XmlNode write(XMLTree element, XmlNode parent)
        {
            XmlNode node = document.CreateNode(XmlNodeType.Element, element.getKey(), "");

            string value = element.getValue();
            if (value != "")
            {
                XmlNode text = document.CreateNode(XmlNodeType.Text, value, "");

                text.Value = value;
                node.AppendChild(text);
            }

            foreach (KeyValuePair<string, string> key in element.getAtributes())
            {
                XmlAttribute atr = document.CreateAttribute(key.Key);
                atr.InnerText = key.Value;
                node.Attributes.Append(atr);

            }

            foreach (XMLTree child in element.getChildren())
            {
                write(child, node);
            }

            if (parent != null)
            {
                parent.AppendChild(node);
            }

            return node;

        }


    }
}
