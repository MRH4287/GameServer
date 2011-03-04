using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XML
{
    class XMLTree
    {
        private XMLTree parent = null;
        private LinkedList<XMLTree> children = new LinkedList<XMLTree>();
        private string name = "";
        private string value = "";
        private Dictionary<string, string> atributes = new Dictionary<string, string>();


        /// <summary>
        /// Erzeugt einen neuenen XML Baum
        /// </summary>
        /// <param name="parent">Vater Element</param>
        /// <param name="name">Knoten Namen</param>

        public XMLTree(XMLTree parent, string name)
        {
            this.parent = parent;
            this.name = name;

            if (parent != null)
                parent.addChild(this);

        }


        /// <summary>
        /// Gibt den Vater des aktuellen Knotens an
        /// </summary>
        /// <returns>Vaterknoten</returns>
        public XMLTree getParent()
        {
            return parent;
        }


        /// <summary>
        /// Fügt dem Element einenen neuen Kindknoten hinzu
        /// </summary>
        /// <param name="child">der Hinzuzufügende Kindknoten</param>

        public void addChild(XMLTree child)
        {
            children.AddLast(child);
        }


        /// <summary>
        /// Setzt den Wert des aktuellen Elements auf den gesetzten Wert
        /// </summary>
        /// <param name="value">der zu setztende Wert</param>
        public void setValue(string value)
        {
            this.value = value;
        }


        /// <summary>
        /// Liefert den Schlüsselnamen des aktuellen Elements zurück
        /// </summary>
        /// <returns>Schlüsselname</returns>
        public string getKey()
        {
            return name;
        }

        /// <summary>
        /// Liefert den Wert des aktuellen Knotens zurück
        /// </summary>
        /// <returns>Aktueller Wert</returns>
        public string getValue()
        {
            return value;
        }


        /// <summary>
        /// Liefert die Kinder als LinkedList zurück, für die verwendung mit foreach
        /// </summary>
        /// <returns>Liefert eine LinkedList</returns>
        public LinkedList<XMLTree> getChildren()
        {
            return children;
        }

        /// <summary>
        /// Fragt nach, ob der aktuelle Knoten Kinder hat
        /// </summary>
        /// <returns>Hat der aktuelle Knoten Kinder</returns>
        public bool haveChildren()
        {
            return (children.Count > 0);

        }

        /// <summary>
        /// Bestitzt der aktuelle Knoten ein bestimmtes Kind
        /// </summary>
        /// <param name="child">Das zu suchende Kind als String</param>
        /// <returns>Ist das Element vorhanden</returns>
        public bool haveChild(string child)
        {
            bool test = false;

            foreach (XMLTree element in children)
            {

                if (element.getKey() == child)
                {
                    test = true;
                }

            }
            return test;

        }

        /// <summary>
        /// Liefert einen Enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public LinkedList<XMLTree>.Enumerator GetEnumerator()
        {
            return children.GetEnumerator();
        }

        /// <summary>
        /// Bestitzt der aktuelle Knoten ein bestimmtes Kind
        /// </summary>
        /// <param name="child">Das zu suchende Kind als <see cref="XMLTree"/></param>
        /// <returns>Ist das Element vorhanden</returns>
        public bool haveChild(XMLTree child)
        {
            return children.Contains(child);
        }

        /// <summary>
        /// Liefert das <see cref="XMLTree"/> Objekt mit einem bestimmten Schlüsselnamen
        /// </summary>
        /// <param name="child">Das zu suchende Kind als string</param>
        /// <returns>Das Element, wenn nicht vorhanden null</returns>
        public XMLTree getChild(string child)
        {
            foreach (XMLTree element in children)
            {

                if (element.getKey() == child)
                {
                   return element;
                }

            }

            return null;

        }

        /// <summary>
        /// Fügt ein Atribut dem aktuellem Element hinzu
        /// </summary>
        /// <param name="key">Schlüssel</param>
        /// <param name="atribut">Wert</param>
        public void addAtribute(string key, string atribut)
        {
            atributes.Add(key, atribut);

        }

        /// <summary>
        /// Gibt die Atribute in einer Generischen Tabelle zurück
        /// </summary>
        /// <returns>Generische Tabelle mit Atributen</returns>
        public Dictionary<string, string> getAtributes()
        {
            return atributes;
        }


        /// <summary>
        /// Liefert ein bestimmtes Atribut zurück
        /// </summary>
        /// <param name="key">Name des Atributes</param>
        /// <returns>Inhalt des Atributes</returns>
        public string getAtribute(string key)
        {
            if (atributes.ContainsKey(key))
            {
                return atributes[key];
            }
            else
            {
                return "";
            }

        }

    }
}
