using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game.Game;
using System.Threading.Tasks;

namespace Server.Moduls
{
    [Serializable]
    class NodeTrackingSystem
    {
        Main main;
        GameData game;

        List<Route> routelist = new List<Route>();

        // diese Daten werden für die Berechnung benötigt:
        public List<Solarsystem> used = new List<Solarsystem>();

        public NodeTrackingSystem(Main main)
        {
            if (main != null)
            {
                this.main = main;
                this.game = main.game;
            }
        }

        public struct VisitedList
        {
            public Solarsystem system;
            public double distance;
            public List<Node> nodelist;
        }


        public void generateData()
        {
            if (main != null)
            {
                List<Solarsystem> list = main.modulmanager.map.systemList;

                int checkCount = 3;

                if (list.Count > 0)
                    // for (int i = 0; i < checkCount; i++ )
                    Parallel.For(0, checkCount, i =>
                {

                    Solarsystem system = list[(new Random()).Next(0, list.Count - 1)];

                    checkNodesNP(system, new List<VisitedList>());
                    used.Clear();
                });


                checkRoutes();


                main.log("Modul NodeTrackingSystem: Wegfindungs-KI Daten geladen");
            }
        }


        public Route getRoute(Solarsystem start, Solarsystem end)
        {
            foreach (Route element in routelist)
            {
                if ((start == element.start) && (end == element.end))
                {
                    return element;
                }
            }

            throw new GameException("Route nicht in der Wegfindungs-KI gefunden!");
        }


        private void checkNodesParallel(Solarsystem system, List<VisitedList> visited)
        {
            //  if (CountInList(used, system) > 1)
            if (used.Contains(system))
            {
                return;
            }

            lock (used)
            {
                used.Add(system);
            }

            Parallel.ForEach(system.nodes, node =>
            {
                checkNodes(node, system, visited, true);
            });

        }

        private void checkNodesNP(Solarsystem system, List<VisitedList> visited)
        {
            // if (CountInList(used, system) > 2)
            if (used.Contains(system))
            {
                return;
            }

            lock (used)
            {
                used.Add(system);
            }

            foreach (Node node in system.nodes)
            {
                checkNodes(node, system, visited, false);
            }


        }




        private void checkNodes(Node node, Solarsystem system, List<VisitedList> visited, bool parallel)
        {

            // Es gibt diese Routen, die vom System aus gehen ...
            // Erstelle eine Neue Route, die das zeigt.
            Solarsystem otherSystem;
            if (node.pointa == system)
            {
                otherSystem = node.pointb;
            }
            else
            {
                otherSystem = node.pointa;
            }

            Route route;
            if (used.Contains(otherSystem))
            {
                // Zuerst ausgehend für das letzte System:
                route = new Route();
                route.distance = node.distance;
                route.start = system;
                route.end = otherSystem;
                route.nodelist.Add(node);
                AddToListIfNotContained(routelist, route);

                // Zuerst ausgehend für das letzte System:
                route = new Route();
                route.distance = node.distance;
                route.end = system;
                route.start = otherSystem;
                route.nodelist.Add(node);
                AddToListIfNotContained(routelist, route);

            }



            // Dannach für alle bereits besuchten Systeme:
            foreach (VisitedList visit in visited)
            {
                // Route vom Startpunkt zu hier:
                route = new Route();
                route.distance = visit.distance;
                route.start = visit.system;
                route.end = otherSystem;

                foreach (Node ent in visit.nodelist)
                {
                    AddToListIfNotContained(route.nodelist, ent);
                }
                AddToListIfNotContained(route.nodelist, node);

                AddToListIfNotContained(routelist, route);


                // Route von hier zum Startpunkt:
                route = new Route();
                route.distance = visit.distance;
                route.start = system;
                route.end = visit.system;

                AddToListIfNotContained(route.nodelist, node);
                foreach (Node ent in visit.nodelist)
                {
                    AddToListIfNotContained(route.nodelist, ent);
                }


                AddToListIfNotContained(routelist, route);


            }


            //Die abstände von allen Elementen in der VisitedList erhöhen und den Besuchten Node eintragen
            List<VisitedList> newVisited = new List<VisitedList>();
            foreach (VisitedList visit in visited)
            {
                VisitedList element = new VisitedList();
                element.system = visit.system;
                element.nodelist = new List<Node>();
                CopyList(visit.nodelist, element.nodelist);
                element.nodelist.Add(node);
                element.distance = visit.distance + node.distance;
                newVisited.Add(element);

            }

            //Trage das aktuelle Sonnensystem bei der Visited List eim 
            VisitedList element2 = new VisitedList();
            element2.system = system;
            element2.distance = node.distance;
            element2.nodelist = new List<Node>();
            element2.nodelist.Add(node);
            newVisited.Add(element2);



            // Rekursiver Aufruf

            if (parallel)
            {
                checkNodesParallel(otherSystem, newVisited);
            }
            else
            {
                checkNodesNP(otherSystem, newVisited);
            }



        }

        private void checkRoutes()
        {
            //Checke die Abstände, da die eh nie passen ;)
            Parallel.ForEach(routelist, route =>
                {

                    double distance = 0;

                    if (route != null)
                    {
                        if (route.nodelist != null)
                            foreach (Node node in route.nodelist)
                            {
                                distance += node.distance;
                            }

                        route.distance = distance;
                    }

                });

            // Jetzt die besten Routen ausfiltern
            List<Route> besteRouten = new List<Route>();


            foreach (Route route in routelist)
            {
                if (route != null)
                {
                    List<Route> besteRoutentmp = new List<Route>();
                    CopyList(besteRouten, besteRoutentmp);

                    if (Contains(besteRouten, route))
                    {
                        foreach (Route check in besteRoutentmp)
                        {
                            if ((route.start == check.start) && (route.end == check.end) && (route.distance < check.distance))
                            {
                                besteRouten.Remove(check);
                                besteRouten.Add(route);
                                break;
                            }
                        }
                    }
                    else if (route.start != route.end)
                    {
                        besteRouten.Add(route);
                    }
                }
            }

            routelist = besteRouten;
        }




        private List<T> CloneList<T>(List<T> toClone)
        {
            T[] array = new T[toClone.Count];
            toClone.CopyTo(array);

            List<T> clone = new List<T>();


            foreach (T ent in array)
            {
                clone.Add(ent);
            }

            return clone;
        }

        private List<T> InverseList<T>(List<T> toInverse)
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


        private void CopyList<T>(List<T> toCopy, List<T> destination)
        {
            T[] array = new T[toCopy.Count];
            toCopy.CopyTo(array);

            List<T> clone = new List<T>();


            foreach (T ent in array)
            {
                destination.Add(ent);
            }



        }

        private int CountInList<T>(List<T> list, T element)
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


        private void AddToListIfNotContained<T>(List<T> list, T element)
        {
            if (!list.Contains(element))
            {
                list.Add(element);
            }
        }

        private bool Contains(List<Route> list, Route element)
        {
            if ((list == null) || (element == null))
            {
                return false;
            }

            foreach (Route check in list)
            {
                if ((check.start == element.start) && (check.end == element.end))
                {
                    return true;
                }

            }

            return false;
        }


        public void setRoutelist(List<Route> routelist)
        {
            if (routelist != null)
            {
                this.routelist = routelist;
            }


        }

        public List<Route> getRouteList()
        {
            return routelist;
        }

    }
}
