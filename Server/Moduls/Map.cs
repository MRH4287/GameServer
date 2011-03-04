using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Communication;
using Game.Game;

using System.Threading.Tasks;

namespace Server.Moduls
{
    [Serializable]
    class MapHandler : Modul
    {


        Map loadedMap = null;

        public List<Solarsystem> systemList = new List<Solarsystem>();
        public List<Solarsystem> usersystems = new List<Solarsystem>();

        private int recreated = 0;

        public MapHandler(Main main)
            : base(main)
        {

        }



        private void generateMapData(int addCount = -1)
        {
            /* Erstellung der Map:
            
             *  1. Erstellen der Refferenzen der Sonnensystem
             *  2. Berrechnen der Abstände / Koordinaten der Systeme
             *  3. Erstellen der Verbindundungen zwischen den Systemen
             *  4. Überprüfen, ob jedes System mindestens eine Verbindung hat
             *  5. Überprüfen, ob es von jedem Punkt eine Verbindung zu jedem Punkt gibt
             */

            if (loadedMap != null)
            {

                if (loadedMap.randomArea != null)
                {
                    int systemcount = loadedMap.systemcount;
                    int min_distance = loadedMap.min_distance;

                    if (addCount != -1)
                    {
                        systemcount = addCount;
                    }


                    int min_X, min_Y, max_X, max_Y;

                    if (loadedMap.randomArea.x1 > loadedMap.randomArea.x2)
                    {
                        min_X = loadedMap.randomArea.x2;
                        max_X = loadedMap.randomArea.x1;
                    }
                    else
                    {
                        min_X = loadedMap.randomArea.x1;
                        max_X = loadedMap.randomArea.x2;
                    }


                    if (loadedMap.randomArea.y1 > loadedMap.randomArea.y2)
                    {
                        min_Y = loadedMap.randomArea.y2;
                        max_Y = loadedMap.randomArea.y1;
                    }
                    else
                    {
                        min_Y = loadedMap.randomArea.y1;
                        max_Y = loadedMap.randomArea.y2;
                    }

                    int connectrange = 100;
                    int maxconnections = 4;
                    try
                    {
                        connectrange = int.Parse(main.config["Game/Map/ConnectRange"]);
                        maxconnections = int.Parse(main.config["Game/Map/MaxConnections"]);
                    }
                    catch { }


                    //  Parallel.For(0, systemcount, i =>
                    for (int i = 0; i < systemcount; i++)
                    {
                        Random rand = new Random();
                        Solarsystem system = new Solarsystem();

                        int x = rand.Next(min_X, max_X);
                        int y = rand.Next(min_Y, max_Y);

                        system.x = x;
                        system.y = y;

                        system.name = Game.Game.Data.Names.SolarsystemNames[rand.Next(Game.Game.Data.Names.SolarsystemNames.Length - 1)];

                        int count = 0;
                        bool ok = true;
                        while (getSystemsInRange(system, min_distance).Count > 0)
                        {

                            count++;

                            x = rand.Next(min_X, max_X);
                            y = rand.Next(min_Y, max_Y);

                            system.x = x;
                            system.y = y;

                            if (count > 100)
                            {
                                ok = false;
                                break;
                            }
                        }

                        if (ok)
                        {



                            List<Solarsystem> near = getSystemsInRange(system, connectrange);
                            if (near.Count > 0)
                            {
                                int anzahl = rand.Next(1, near.Count);
                                for (int j = 0; j < anzahl; j++)
                                {
                                    try
                                    {
                                        Solarsystem nearSystem = near[j];


                                        if ((getConnectionCount(system) < maxconnections) && (getConnectionCount(nearSystem) < maxconnections) && !isConnection(system, nearSystem))
                                        {
                                            Node node = new Node();
                                            node.pointa = system;
                                            node.pointb = nearSystem;
                                            node.distance = getDistance(system, nearSystem);
                                            system.nodes.Add(node);
                                            nearSystem.nodes.Add(node);

                                        }
                                    }
                                    catch { }
                                }
                            }
                            if (system != null)
                                systemList.Add(system);
                        }
                        // });
                    }

                    // Überprüfung der Systeme

                    bool sysok = true;

                    List<Solarsystem> delet = new List<Solarsystem>();
                    Parallel.ForEach(systemList, system =>
                        {

                            if ((system != null) && sysok)
                            {
                                if (getSystemsInRange(system, 0).Count > 0)
                                {
                                    Console.WriteLine("Invalides System endteckt!");

                                    system.nodes.Clear();
                                    foreach (Node node in system.nodes)
                                    {
                                        node.pointa.nodes.Remove(node);
                                        node.pointb.nodes.Remove(node);
                                    }
                                    try
                                    {
                                        delet.Add(system);

                                    }
                                    catch
                                    {
                                    }
                                }
                                else
                                    if (system.nodes.Count == 0)
                                    {

                                        List<Solarsystem> near = getSystemsInRange(system, connectrange * 2);
                                        if (near.Count > 0)
                                        {
                                            int anzahl = (new Random()).Next(1, near.Count);
                                            for (int j = 0; j < anzahl; j++)
                                            {
                                                Solarsystem nearSystem = near[j];

                                                if ((getConnectionCount(system) < 2) && (getConnectionCount(nearSystem) < maxconnections) && !isConnection(system, nearSystem))
                                                {
                                                    Node node = new Node();
                                                    node.pointa = system;
                                                    node.pointb = nearSystem;
                                                    node.distance = getDistance(system, nearSystem);
                                                    system.nodes.Add(node);
                                                    nearSystem.nodes.Add(node);

                                                }
                                            }

                                        }
                                        if (system.nodes.Count == 0)
                                        {
                                            main.log("Unnereichbares System gefunden! Bitte Passen Sie die Map-Daten oder die AnwendungsKonfiguration an");
                                            sysok = false;
                                        }
                                    }
                            }
                        });

                    if (!sysok)
                    {
                        //  reCreateMap();
                        return;
                    }

                    //  Parallel.ForEach(delet, del => { systemList.Remove(del); });
                    //   Console.WriteLine(delet.Count + " Systeme entfernt");
                    foreach (Solarsystem del in delet)
                    {
                        if (systemList.Contains(del))
                        {

                            del.nodes.Clear();
                            foreach (Node node in del.nodes)
                            {
                                node.pointa.nodes.Remove(node);
                                node.pointb.nodes.Remove(node);
                            }

                            systemList.Remove(del);
                        }

                    }
                    systemList.Remove(null);

                    //  if (delet.Count > 0)
                    // generateMapData(delet.Count);

                }



                //Erstelle Zufalls Planeten
                List<PlanetClass> planetTypes = game.getPlanetTypes();
                if (planetTypes.Count > 0)
                {
                    Random random = new Random(DateTime.Now.Millisecond);
                    Parallel.ForEach(systemList, system =>
                           {


                               for (int i = 0; i < random.Next(15); i++)
                               {

                                   Planet planet = new Planet(Game.Game.Data.Names.PlanetNames[random.Next(Game.Game.Data.Names.PlanetNames.Length - 1)]);
                                   planet.type = planetTypes[random.Next(planetTypes.Count - 1)];

                                   if (system.planets == null)
                                   {
                                       system.planets = new List<Planet>();
                                   }
                                   system.planets.Add(planet);
                                   planet.Solarsystem = system;
                               }

                           });
                }

            }
            else
            {
                throw new GameException("Keine Map-Datei geladen");
            }
        }


        private void handleData()
        {
            main.log("Erstelle Zufalls Karte ...");
            generateMapData();

            //Erstellen der Routen:
            //main.modulmanager.nodetracking.generateData();

            //Speichern der Map
            saveMap();

            main.log("Überprüfe erstellte Karte auf Korrektheit");
            // Überprüfen ob jeder Spieler zu jedem Spieler reisen kann:
            bool ok = true;
            //main.log("Warnung: Überprüfungsfunktion deaktiviert! Diese Karte könnte nicht spielbar sein! Betrachten Sie sich die Datei created.map mit dem Mapeditor um das zu überprüfen");

            List<Route> geprueft = new List<Route>();

            int count = 0;
            int countConnections = (int)((usersystems.Count * (usersystems.Count - 1)) / 2f);

            foreach (Solarsystem system in usersystems)
            {
                if (!ok)
                {
                    break;
                }

                foreach (Solarsystem system2 in usersystems)
                {

                    bool have = false;
                    foreach (Route rt in geprueft)
                    {
                        if (((rt.start == system) && (rt.end == system2)) || ((rt.start == system2) && (rt.end == system)))
                        {
                            have = true;
                            break;
                        }
                    }
                    if (have)
                    {
                        continue;
                    }

                    Route route = new Route();
                    route.start = system;
                    route.end = system2;
                    geprueft.Add(route);


                    if (system != system2)
                    {
                        bool sysok = false;
                        try
                        {
                            main.modulmanager.nodetracking.pingSolarsystem(system, system, system2, null, null, 1);
                            /*   if (test == null)
                               {

                                   sysok = true;
                                   count++;
                                   main.log("Überprüfung zu " + (((float)count / countConnections) * 100) + "% abgeschlossen");
                               }
                               else
                               {
                                   sysok = false;
                               }
                               */


                        }
                        catch (TrackingCanceledException)
                        {
                            sysok = true;
                            count++;
                            main.log("Überprüfung zu " + (((float)count / countConnections) * 100) + "% abgeschlossen");

                        }

                        if (!sysok)
                        {
                            ok = false;
                            Console.WriteLine("Keine Verbindung zwischen Spieler-Systemen (" + system.name + "->" + system2.name + ")");
                            break;
                        }

                    }




                }



            }

            if (ok)
            {
                foreach (Solarsystem system in systemList)
                {
                    if (system.planets != null)
                        foreach (Planet planet in system.planets)
                        {
                            game.addPlanet(planet);
                        }

                }

                main.log("Map-Datei geladen");

            }
            else
            {
                if (recreated < 3)
                {
                    recreated++;
                    systemList = loadedMap.solarsystems;
                    usersystems.Clear();

                    foreach (Solarsystem system in systemList)
                    {
                        if (system.userstart)
                        {
                            usersystems.Add(system);
                        }

                    }
                    main.modulmanager.nodetracking.setRoutelist(new List<Route>());

                    main.log("Karte nicht spielbar, erstelle neue Karte ...");

                    handleData();
                }
                else
                {
                    main.log("Kartenerstellung abgebrochen!");
                }
            }

        }

        private void saveMap()
        {
            // Speichern der Datei:
            Translator tr = new Translator();

            List<ClassContainer> liste = new List<ClassContainer>();
            Map map2 = new Map();
            map2.solarsystems = systemList;
            map2.randomArea = loadedMap.randomArea;

            map2.systemcount = loadedMap.systemcount;
            map2.min_distance = loadedMap.min_distance;
            ClassContainer container2 = new ClassContainer();
            container2.objekt = map2;
            container2.type = ClassType.Map;
            liste.Add(container2);


            tr.writeData(liste, "created.map");

        }


        public void reCreateMap()
        {

            systemList = loadedMap.solarsystems;
            usersystems.Clear();
            game.planets.Clear();

            foreach (Solarsystem system in systemList)
            {
                if (system.userstart)
                {
                    usersystems.Add(system);
                }

            }
            main.modulmanager.nodetracking.setRoutelist(new List<Route>());

            handleData();

        }


        public List<Solarsystem> getSystemsInRange(Solarsystem start, double range)
        {
            List<Solarsystem> list = new List<Solarsystem>();

            if (start != null)
            {
                Parallel.ForEach(systemList, system =>
                          {
                              if ((system != null) && (start != system))
                              {
                                  double distance = getDistance(start, system);

                                  if (distance <= range)
                                  {
                                      lock (list)
                                      {
                                          list.Add(system);
                                      }
                                  }
                              }
                          });
            }
            return list;
        }

        public double getDistance(Solarsystem start, Solarsystem end)
        {
            double distance = Math.Sqrt(Math.Pow((start.x - end.x), 2) + Math.Pow((start.y - end.y), 2));
            return distance;
        }

        public int getConnectionCount(Solarsystem system)
        {
            return system.nodes.Count;
        }


        public bool isConnection(Solarsystem start, Solarsystem end)
        {
            bool connection = false;
            Parallel.ForEach(start.nodes, node =>
             {
                 if ((node.pointa == end) || (node.pointb == end))
                 {
                     connection = true;
                 }
             });

            return connection;
        }


        public void load(string filename)
        {
            if (File.Exists(filename))
            {
                Translator tr = new Translator();
                List<ClassContainer> list = tr.readData(filename);

                if (list.Count > 0)
                {
                    ClassContainer container = list[0];

                    if (container.type == ClassType.Map)
                    {
                        Map map = (Map)container.objekt;

                        setMap(map);

                    }
                }
                if (list.Count > 1)
                {
                    ClassContainer container = list[1];

                    if (container.objekt is List<Route>)
                    {
                        List<Route> routelist = (List<Route>)container.objekt;


                    }


                }

                handleData();





            }
            else
            {
                throw new GameException("Datei exsistiert nicht");
            }
        }

        /// <summary>
        /// Liefert die aktuell geladene Map zurück
        /// </summary>
        /// <returns>Liefert die Map zurück</returns>
        public Map getCurrentMap()
        {
            Map map2 = new Map();
            map2.solarsystems = systemList;
            map2.randomArea = loadedMap.randomArea;

            map2.systemcount = loadedMap.systemcount;
            map2.min_distance = loadedMap.min_distance;

            return map2;
        }


        /// <summary>
        /// Läd ein bestimmtes Map-Objekt in da System
        /// </summary>
        /// <param name="map">Map die geladen werden soll</param>
        public void setMap(Map map)
        {
            loadedMap = map;

            systemList = map.solarsystems;
            usersystems.Clear();

            foreach (Solarsystem system in systemList)
            {
                if (system.userstart)
                {
                    usersystems.Add(system);
                }

            }
        }

    }
}
