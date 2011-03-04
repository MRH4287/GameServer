using Server.Moduls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Server;
using Game.Game;
using System.Collections.Generic;

namespace UnitTest
{


    /// <summary>
    ///Dies ist eine Testklasse für "NodeTrackingSystemTest" und soll
    ///alle NodeTrackingSystemTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class NodeTrackingSystemTest
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
        ///Ein Test für "getRoute"
        ///</summary>
        [TestMethod()]
        public void getRouteTest()
        {

            Solarsystem A = new Solarsystem();
            A.name = "A";
            Solarsystem B = new Solarsystem();
            B.name = "B";
            Solarsystem C = new Solarsystem();
            C.name = "C";

            Solarsystem D = new Solarsystem();
            D.name = "D";

            List<Solarsystem> systems = new List<Solarsystem>();
            systems.Add(A);
            systems.Add(B);
            systems.Add(C);
            systems.Add(D);

            Node AB = new Node();
            AB.distance = 1;
            AB.pointa = A;
            AB.pointb = B;
            A.nodes.Add(AB);
            B.nodes.Add(AB);

            Node AC = new Node();
            AC.distance = 10;
            AC.pointa = A;
            AC.pointb = C;
            A.nodes.Add(AC);
            C.nodes.Add(AC);

            Node BD = new Node();
            BD.distance = 100;
            BD.pointa = B;
            BD.pointb = D;
            B.nodes.Add(BD);
            D.nodes.Add(BD);


            Node CD = new Node();
            CD.distance = 5;
            CD.pointa = C;
            CD.pointb = D;
            C.nodes.Add(CD);
            D.nodes.Add(CD);


            Main main = null;
            NodeTrackingSystem target = new NodeTrackingSystem(main);

            Solarsystem start = A;
            Solarsystem end = D;
            bool dontUsePing = false;
            Route actual;
            actual = target.getRoute(start, end, dontUsePing);

            Assert.AreEqual(A, actual.start);
            Assert.AreEqual(D, actual.end);
            Assert.AreEqual(15, actual.distance);
        }



    }
}
