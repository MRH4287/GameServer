using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using Game.Game;

namespace Server.Moduls
{
    /// <summary>
    /// Abstrakte Klasse, von denen alle Module erben
    /// </summary>
    [Serializable]
    abstract class Modul
    {

        /// <summary>
        /// Main-Instanz
        /// </summary>
        protected Main main;
        /// <summary>
        /// Game-Instanz
        /// </summary>
        protected GameData game;

        /// <summary>
        /// Instanzier ein neues Modul
        /// </summary>
        /// <param name="main">Main-Instanz</param>
        public Modul(Main main)
        {
            if (main != null)
            {
                this.main = main;
                this.game = main.game;
            }
        }


        /// <summary>
        /// Klont eine Generische Liste
        /// </summary>
        /// <typeparam name="T">Listen-Typ</typeparam>
        /// <param name="toClone">Liste, die geklont werden soll</param>
        /// <returns>geklonte Liste</returns>
        protected List<T> CloneList<T>(List<T> toClone)
        {

            List<T> clone = new List<T>();

            CopyList(toClone, clone);

            return clone;
        }

        /// <summary>
        /// Invertiert eine Generische Liste
        /// </summary>
        /// <typeparam name="T">Listen-Typ</typeparam>
        /// <param name="toInverse">Liste, die Invertiert werden soll</param>
        /// <returns>Invertierte Liste</returns>
        protected List<T> InverseList<T>(List<T> toInverse)
        {
            T[] array = new T[toInverse.Count];
            toInverse.CopyTo(array);

            List<T> inverse = new List<T>();

            for (int i = array.Length - 1; i >= 0; i--)
            {
                inverse.Add(array[i]);

            }

            return inverse;
        }


        /// <summary>
        /// Kopiert eine Generische Liste zu einer anderen
        /// </summary>
        /// <typeparam name="T">Listen-Typ</typeparam>
        /// <param name="toCopy">Liste die Kopiert werden soll</param>
        /// <param name="destination">Ziel für den Kopiervorgang</param>
        protected void CopyList<T>(List<T> toCopy, List<T> destination)
        {
            T[] array = new T[toCopy.Count];
            toCopy.CopyTo(array);


            foreach (T ent in array)
            {
                destination.Add(ent);
            }



        }

        /// <summary>
        /// Gibt an wie obt ein Element innerhalb einer Generischen Liste vorkommt
        /// </summary>
        /// <typeparam name="T">Listen-Typ</typeparam>
        /// <param name="list">Generische Liste</param>
        /// <param name="element">Element, das gesucht werden soll</param>
        /// <returns>Anzahl der Elemente, innerhalb der Liste</returns>
        protected int CountInList<T>(List<T> list, T element)
        {
            int count = 0;

            Parallel.ForEach(list, el =>
            {
                if (el.Equals(element))
                {
                    count++;
                }

            });
            return count;
        }


        /// <summary>
        /// Fügt ein Element zu einer Generischen Liste hinzu, wenn dieses nicht bereits enthalten ist
        /// </summary>
        /// <typeparam name="T">Listen-Typ</typeparam>
        /// <param name="list">Liste, in der das Element eingefügt werden soll</param>
        /// <param name="element">Element, das eingefügt werden soll</param>
        protected void AddToListIfNotContained<T>(List<T> list, T element)
        {
            if (!list.Contains(element))
            {
                list.Add(element);
            }
        }


        /// <summary>
        /// Kopiert den Inhalt eines Arrays in ein anderes
        /// </summary>
        /// <typeparam name="T">Array-Typ</typeparam>
        /// <param name="source">Quell-Array</param>
        /// <param name="destination">Ziel-Array</param>
        protected void copyArray<T>(T[] source, T[] destination)
        {
            try
            {
                int i = 0;

                foreach (T el in source)
                {
                    destination[i++] = el;
                }
            }
            catch
            { }

        }

    }
}
