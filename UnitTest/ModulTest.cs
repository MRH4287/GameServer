using Server.Moduls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UnitTest
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "ModulTest" und soll
    ///alle ModulTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ModulTest
    {

        private class ModulImplementation : Modul
        {
            public ModulImplementation()
                : base(null)
            {
             
            }

            new public List<T> CloneList<T>(List<T> toClone)
            {
                return base.CloneList(toClone);
            }

            new public List<T> InverseList<T>(List<T> toInverse)
            {
                return base.InverseList(toInverse);
            }

            new public void CopyList<T>(List<T> toCopy, List<T> destination)
            {
                base.CopyList(toCopy, destination);
            }

            new public int CountInList<T>(List<T> list, T element)
            {
                return base.CountInList(list, element);
            }

            new public void AddToListIfNotContained<T>(List<T> list, T element)
            {
                base.AddToListIfNotContained(list, element);
            }

            new public void copyArray<T>(T[] source, T[] destination)
            {
                base.copyArray(source, destination);
            }




        }




        private TestContext testContextInstance;

        /// <summary>
        ///Ruft den Testkontext auf, der Informationen
        ///über und Funktionalität für den aktuellen Testlauf bietet, oder legt diesen fest.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Zusätzliche Testattribute
        // 
        //Sie können beim Verfassen Ihrer Tests die folgenden zusätzlichen Attribute verwenden:
        //
        //Mit ClassInitialize führen Sie Code aus, bevor Sie den ersten Test in der Klasse ausführen.
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Mit ClassCleanup führen Sie Code aus, nachdem alle Tests in einer Klasse ausgeführt wurden.
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen.
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Ein Test für "AddToListIfNotContained"
        ///</summary>
        
        [TestMethod()]
        public void AddToListIfNotContainedTest()
        {
            ModulImplementation modul = new ModulImplementation();

            List<int> liste = new List<int>();

            modul.AddToListIfNotContained(liste, 1);

            Assert.IsTrue(liste.Contains(1));

            modul.AddToListIfNotContained(liste, 1);

            Assert.AreEqual(1, modul.CountInList(liste, 1));

        }


        [TestMethod()]
        public void CountInListTest()
        {
            ModulImplementation modul = new ModulImplementation();

            List<int> liste = new List<int>();

            Assert.AreEqual(0, modul.CountInList(liste, 1));

            liste.Add(1);
            liste.Add(1);
            liste.Add(1);

            Assert.AreEqual(3, modul.CountInList(liste, 1));


        }

        [TestMethod()]
        public void CloneListTest()
        {
            ModulImplementation modul = new ModulImplementation();

            List<int> liste = new List<int>();
            liste.Add(1);
            liste.Add(2);
            liste.Add(3);

            List<int> liste2 = modul.CloneList(liste);


            Assert.AreNotSame(liste, liste2);
            Assert.AreEqual(liste[0], liste2[0]);
            Assert.AreEqual(liste[1], liste2[1]);
            Assert.AreEqual(liste[2], liste2[2]);


        }


        [TestMethod()]
        public void InverseListTest()
        {

            ModulImplementation modul = new ModulImplementation();

            List<int> liste = new List<int>();
            liste.Add(1);
            liste.Add(2);
            liste.Add(3);

            List<int> liste2 = modul.InverseList(liste);
            Assert.AreEqual(liste[2], liste2[0]);
            Assert.AreEqual(liste[1], liste2[1]);
            Assert.AreEqual(liste[0], liste2[2]);


        }


        [TestMethod()]
        public void CopyListTest()
        {
            ModulImplementation modul = new ModulImplementation();

            List<int> liste = new List<int>();
            liste.Add(1);


            List<int> liste2 = new List<int>();
            liste2.Add(2);
            liste2.Add(3);


            modul.CopyList(liste2, liste);
            Assert.AreEqual(1, liste[0]);
            Assert.AreEqual(2, liste[1]);
            Assert.AreEqual(3, liste[2]);



        }



    }
}
