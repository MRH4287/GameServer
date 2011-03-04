using Server.Moduls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Server;
using Game.Game;

namespace UnitTest
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "ResourcesTest" und soll
    ///alle ResourcesTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ResourcesTest
    {


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
        ///Ein Test für "addRes"
        ///</summary>
        [TestMethod()]
        public void addResTest()
        {
            Main main = null; 
            Resources target = new Resources(main); 

            User user = new User(1, "", "", new Race(1, "1"));
            ResType type = ResType.Erz;
            double count = 10F;

            Assert.AreEqual(target.getResource(user, type), 0);
            
            target.addRes(user, type, count);

            Assert.AreEqual(target.getResource(user, type), count);


        }

        /// <summary>
        ///Ein Test für "addRes"
        ///</summary>
        [TestMethod()]
        public void addResTest1()
        {
            Main main = null; 
            Resources target = new Resources(main);
            User user = new User(1, "", "", new Race(1, "1")); 
            ResList resources = new ResList();
            resources[ResType.Erz] = 1;
            resources[ResType.Kurbidium] = 10;
            resources[ResType.Unobtanium] = 200;

            Assert.AreEqual(target.getResource(user, ResType.Erz), 0);
            Assert.AreEqual(target.getResource(user, ResType.Kurbidium), 0);
            Assert.AreEqual(target.getResource(user, ResType.Unobtanium), 0);


            target.addRes(user, resources);

            Assert.AreEqual(target.getResource(user, ResType.Erz), 1);
            Assert.AreEqual(target.getResource(user, ResType.Kurbidium), 10);
            Assert.AreEqual(target.getResource(user, ResType.Unobtanium), 200);

        }


  
        /// <summary>
        ///Ein Test für "haveRes"
        ///</summary>
        [TestMethod()]
        public void haveResTest()
        {
            Main main = null; 
            Resources target = new Resources(main);
            User user = new User(1, "", "", new Race(1, "1")); 
            ResList price = new ResList();
            price[ResType.Erz] = 100;


            bool expected = false;
            bool actual;
            actual = target.haveRes(user, price);
            Assert.AreEqual(expected, actual);


            target.addRes(user, price);

            expected = true;
            actual = target.haveRes(user, price);
            Assert.AreEqual(expected, actual);



        }

        /// <summary>
        ///Ein Test für "subRes"
        ///</summary>
        [TestMethod()]
        public void subResTest()
        {
            Main main = null;
            Resources target = new Resources(main);
            User user = new User(1, "", "", new Race(1, "1"));
            ResList resources = new ResList();
            resources[ResType.Erz] = 1;
            resources[ResType.Kurbidium] = 10;
            resources[ResType.Unobtanium] = 200;

            target.addRes(user, resources);


            ResList liste = new ResList();
            liste[ResType.Erz] = 1;
            liste[ResType.Kurbidium] = 5;
            liste[ResType.Unobtanium] = 100;


            target.subRes(user, ResType.Erz, 1);
            target.subRes(user, ResType.Kurbidium, 5);
            target.subRes(user, ResType.Unobtanium, 100);


            Assert.AreEqual(target.getResource(user, ResType.Erz), 0);
            Assert.AreEqual(target.getResource(user, ResType.Kurbidium), 5);
            Assert.AreEqual(target.getResource(user, ResType.Unobtanium), 100);
        }
    }
}
