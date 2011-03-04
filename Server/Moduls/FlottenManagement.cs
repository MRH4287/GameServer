using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game.Game;
using System.Threading.Tasks;

namespace Server.Moduls
{
    /// <summary>
    /// Diese Klasse regelt das Flottenmanageent und bietet Funktionen zur Überprüfung von Flottenbewegungen 
    /// </summary>
    [Serializable]
    class FlottenManagement : Modul
    {

        [Serializable]
        struct TravelData
        {
            public Solarsystem origin;
            public Solarsystem destination;
            public Route route;
            public List<Ship> ship;
            public int fleet;
            public double speed;
            public double timeRemaining;
            public double distance;

        }

        private List<TravelData> travelList = new List<TravelData>();



        public FlottenManagement(Main main)
            : base(main)
        {

        }




        public int findFreeFleet()
        {
            List<Ship> shiplist = game.getShips();

            List<int> belegt = new List<int>();

            foreach (Ship ship in shiplist)
            {
                belegt.Add(ship.Fleet);
            }

            for (int i = 1; i <= belegt.Count; i++)
            {
                if (!belegt.Contains(i))
                {
                    return i;
                }
            }
            return belegt.Count + 1;


        }


        public void travel(List<Ship> ships, Route route)
        {
            int fleet = findFreeFleet();
            Solarsystem start = route.start;
            Solarsystem end = route.end;

            List<TravelData> fleetData = new List<TravelData>();

            double speed = double.MaxValue;

            foreach (Ship ship in ships)
            {
                if (ship.speed < speed) { speed = ship.speed; }


            }


            TravelData neu = new TravelData();
            neu.destination = end;
            neu.fleet = fleet;
            neu.origin = start;
            neu.route = route;
            neu.speed = speed;
            neu.ship = ships;
            neu.distance = route.distance;
            neu.timeRemaining = route.distance / speed;

            travelList.Add(neu);




        }

        public void HandleMovement()
        {
            List<TravelData> dataList = CloneList(travelList);

            Parallel.ForEach(dataList, data =>
                {



                    double newdistance = data.distance - data.speed;
                    if (newdistance <= 0)
                    {
                        // Ankunft im System:

                        foreach (Ship ship in data.ship)
                        {
                            ship.Position = data.destination;
                        }

                        travelList.Remove(data);

                    }
                    else
                    {
                        TravelData neu = new TravelData();

                        neu.destination = data.destination;
                        neu.fleet = data.fleet;
                        neu.origin = data.origin;
                        neu.route = data.route;
                        neu.ship = data.ship;
                        neu.speed = data.speed;
                        neu.distance = newdistance;
                        neu.timeRemaining = newdistance / data.speed;

                    }


                });

            main.log("Modul FlottenManagement: Schiffsbewegungen abgeschlossen");

        }


    }
}
