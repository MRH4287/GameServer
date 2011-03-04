using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game.Game;
using System.Threading.Tasks;

namespace Server.Moduls
{
    [Serializable]
    class NodeTrackingSystem : Modul
    {


        public Dictionary<Solarsystem, Dictionary<Solarsystem, Route[]>> pingResponse = new Dictionary<Solarsystem, Dictionary<Solarsystem, Route[]>>();

        private List<Route> routelist = new List<Route>();

        int maxdepth = 6;

        public NodeTrackingSystem(Main main)
            : base(main)
        {
            if (main != null)
            {

                try
                {
                    maxdepth = int.Parse(main.config["Game/Map/MaxDepth"]);
                }
                catch
                {
                }
            }
        }




        public Route getRoute(Solarsystem start, Solarsystem end, bool dontUsePing = false)
        {
            foreach (Route element in routelist)
            {
                if ((start == element.start) && (end == element.end))
                {
                    return element;
                }
            }

            if (dontUsePing)
            {
                Route startS = new Route();
                startS.systems.Add(start);
                Route result = searchRoute(start, end, startS, 0);

                result.start = start;
                result.end = end;
                routelist.Add(result);

                Route route2 = new Route();
                route2.start = end;
                route2.end = start;
                route2.nodelist = InverseList(result.nodelist);
                route2.systems = InverseList(result.systems);
                routelist.Add(route2);

                return result;
            }
            else
            {
                try
                {
                    this.pingSolarsystem(start, start, end);
                    Route[] results = pingResponse[start][end];

                    Route best = null;
                    foreach (Route rt in results)
                    {
                        double length = 0;
                        foreach (Node nd in rt.nodelist)
                        {
                            length += nd.distance;
                        }

                        rt.distance = length;

                        if ((best == null) || (best.distance > rt.distance))
                        {
                            best = rt;
                        }

                    }

                    best.systems.Add(end);

                    return best;
                }
                catch (TrackingCanceledException)
                {
                    return null;
                }




            }
        }



        private Route searchRoute(Solarsystem position, Solarsystem end, Route list, int depth)
        {
            Route result = null;
            //    Console.WriteLine("Bin im System: " + position);


            if (depth < maxdepth)

                //   Parallel.ForEach(position.nodes, node =>
                foreach (Node node in position.nodes)
                {
                    //    Console.WriteLine("Checke Node: " + node);

                    //  bool stop = false;

                    Solarsystem other;
                    other = (node.pointa == position) ? node.pointb : node.pointa;

                    Route VList = new Route();
                    VList.nodelist = CloneList(list.nodelist);
                    VList.nodelist.Add(node);
                    VList.systems = CloneList(list.systems);
                    VList.systems.Add(other);

                    if (list.systems.Contains(other))
                    {
                        //      Console.WriteLine("Bereits besucht");
                        // stop = true;
                        continue;
                    }


                    // if (!stop)
                    //  {

                    if (other == end)
                    {
                        double distance = 0;
                        foreach (Node tmp in VList.nodelist)
                        {
                            distance += tmp.distance;
                        }
                        VList.distance = distance;

                        //    Console.WriteLine("Ergebnis gefunden");
                        // result = VList;
                        return VList;


                    }
                    else
                    {
                        //     Console.WriteLine("Suche Rekursiv");
                        Route search = searchRoute(other, end, VList, depth + 1);
                        if (search.distance != -1)
                        {
                            //  result = search;
                            return search;
                        }
                        else
                        {
                            //            Console.WriteLine("Ergebnis nicht gefunden");
                        }

                        //    }
                    }
                }
            // });

            if (result == null)
            {
                result = new Route();
                result.distance = -1;
            }
            return result;

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


        public void pingSolarsystem(Solarsystem current, Solarsystem source, Solarsystem destination, Solarsystem[] visited = null, Node[] visitedNode = null, int maxResults = 3, int depth = 0)
        {
            if (visited == null || visitedNode == null)
            {
                visited = new Solarsystem[0];
                visitedNode = new Node[0];
                //pingResponse = new Dictionary<Solarsystem, Dictionary<Solarsystem, Route[]>>();
            }
            
                

            if ((main != null) && (!main.run()))
            {
                return;
            }

            if (!pingResponse.ContainsKey(source))
            {
                pingResponse.Add(source, new Dictionary<Solarsystem, Route[]>());
            }


            if (pingResponse[source].ContainsKey(destination) && (pingResponse[source][destination].Length >= maxResults))
            {
                return;
            }


            if (depth > maxdepth)
            {
                throw new TrackingCanceledException();
            }



            if (current == destination)
            {
                try
                {
                    Route route = new Route();
                    route.start = source;
                    route.end = destination;
                    route.systems = new List<Solarsystem>();
                    route.nodelist = new List<Node>();

                    foreach (Solarsystem system in visited)
                    {
                        route.systems.Add(system);
                    }

                    foreach (Node node in visitedNode)
                    {
                        route.nodelist.Add(node);
                    }


                    if (!pingResponse[source].ContainsKey(destination))
                    {
                        pingResponse[source].Add(destination, new Route[0]);
                    }

                    Route[] routes = new Route[pingResponse[source][destination].Length + 1];
                    copyArray(pingResponse[source][destination], routes);
                    routes[routes.Length - 1] = route;
                    pingResponse[source][destination] = routes;
                }
                catch
                {
                    // Das ist sehr seltsam, dass hier was fliegt ...
                }

            }
            else if (visited.Contains(current))
            {
                // Das System wurde bereits besucht ...
                return;
            }
            else
            {
                //Trage das aktuelle System in die Liste, der besuchten Systeme ein
                Solarsystem[] visited2 = new Solarsystem[visited.Length + 1];
                copyArray(visited, visited2);
                visited2[visited2.Length - 1] = current;

                List<Node> nodelist = current.nodes;

                bool error = false;

                Parallel.ForEach(nodelist, node =>
                //  foreach (Node node in nodelist)
                {
                    try
                    {

                        Solarsystem other = (node.pointa == current) ? node.pointb : node.pointa;

                        Node[] visitedNode2 = new Node[visitedNode.Length + 1];
                        copyArray(visitedNode, visitedNode2);
                        visitedNode2[visitedNode2.Length - 1] = node;

                        pingSolarsystem(other, source, destination, visited2, visitedNode2, maxResults, depth + 1);
                        //}


                    }
                    catch (TrackingCanceledException)
                    {
                        error = true;
                    }
                });


                if (error)
                {
                    throw new TrackingCanceledException();
                }


                
            }


            if (depth == 0)
            {
               // if (!pingResponse.ContainsKey(source) || !(pingResponse[source].ContainsKey(destination)) || !(pingResponse[source][destination].Length > 0))
              //  {
              //      throw new TrackingCanceledException();
              //  }
              //  else
              //  {
                    return;
              //  }
            }
            else
            {

                return;
            }
        }

    }

    class TrackingCanceledException : Exception
    {
    }

}
