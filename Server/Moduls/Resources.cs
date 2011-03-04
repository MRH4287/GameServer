using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game.Game;
using System.Threading.Tasks;

namespace Server.Moduls
{
    /// <summary>
    /// Resourcen Management Klasse
    /// </summary>
    [Serializable]
    class Resources : Modul
    {
    

        double maxres;


        /// <summary>
        /// Dictonary in dem die Resourcen Daten gespeichert werden.
        /// </summary>
        Dictionary<User, ResList> resourceList = new Dictionary<User, ResList>();



        /// <summary>
        /// Resourcen Management Klasse
        /// </summary>
        /// <param name="main">Main</param>
        public Resources(Main main) : base(main)
        {
            //Wegen Unit-Tests
            if (main != null)
            {
                this.maxres = double.Parse(main.config["Game/System/max_resources"]);
            }
            else
            {
                this.maxres = 10000;
            }
        }


        /// <summary>
        /// Startet die Resourcenüberprüfung
        /// </summary>
        public void checkRes()
        {
            List<User> userList = game.getUsers();


            Parallel.ForEach<User>(userList, user =>
                {
                    fkt_checkRes(user);
                });

            main.log("Modul Resourcen: Resourcen Überprüfung abgeschlossen");

        }


        /// <summary>
        /// Liefert die Resourcen eines Spielers zurück
        /// </summary>
        /// <param name="user">Benutzer für den die Resourcen abgefragt werden sollen</param>
        /// <returns>Die Resourcen in einer ResList. 0 sollte Spieler nicht in der Liste sein.</returns>
        public ResList getResources(User user)
        {
            if (resourceList.ContainsKey(user))
            {
                return resourceList[user];
            }
            else
            {
                return new ResList();
            }
        }


        /// <summary>
        /// Liefert eine ganz bestimmte Resource eines Spieler zurück
        /// </summary>
        /// <param name="user">Benutzer für den die Resource abgefragt werden soll</param>
        /// <param name="type">Resourcen Typ</param>
        /// <returns>Anzahl der entsprechenden Resource</returns>
        public double getResource(User user, ResType type)
        {
            if (resourceList.ContainsKey(user))
            {
                ResList list = resourceList[user];
                return list[type];
            }
            return 0;

        }


        /// <summary>
        /// Fügt einem Spieler einen bestimmten Resourcen Betrag hinzu
        /// </summary>
        /// <param name="user">Benutzer, dem die Resourcen hinzugefügt werden sollen</param>
        /// <param name="type">Resourcen Typ</param>
        /// <param name="count">Resourcen Betrag</param>
        public void addRes(User user, ResType type, double count)
        {
            if (count < 0)
            {
                throw new ArgumentException("Der Wert von Count darf nicht kleiner als 0 sein!");
            }

            if (!resourceList.ContainsKey(user))
            {
                addUser(user);
            }


            lock (resourceList)
            {

                resourceList[user][type] += count;
            }
        }


        /// <summary>
        /// Fügt einem Spieler einen bestimmten Resourcen Betrag hinzu
        /// </summary>
        /// <param name="user">Benutzer, dem die Resourcen hinzugefügt werden sollen</param>
        /// <param name="resources">Liste der Resourcen</param>
        public void addRes(User user, ResList resources)
        {
            lock (resourceList)
            {

                foreach (KeyValuePair<ResType, double> get in resources)
                {
                    addRes(user, get.Key, get.Value);
                }
            }

        }


        /// <summary>
        /// Zieht einem Spieler einen bestimmten Resourcen Betrag ab
        /// </summary>
        /// <param name="user">Benutzer, von dem die Resourcen abgezogen werden sollen</param>
        /// <param name="type">Resourcen Typ</param>
        /// <param name="count">Resourcen Betrag</param>
        public void subRes(User user, ResType type, double count)
        {
            if (count < 0)
            {
                throw new ArgumentException("Der Wert von Count darf nicht kleiner als 0 sein!");
            }

            if (!resourceList.ContainsKey(user))
            {
                addUser(user);
            }

            if ((resourceList[user][type] - count) < 0)
            {
                throw new GameException("Der Spieler hat nicht genügend Resourcen");
            }

            lock (resourceList)
            {

                resourceList[user][type] -= count;

            }
        }


        /// <summary>
        /// Überprüft ob ein Spieler mehr als einen bestimmten Betrag an Resourcen besitzt.
        /// </summary>
        /// <param name="user">Benutzer, für den die abfrage durchgeführt werden soll</param>
        /// <param name="price">ResList Objekt mit den Kosten</param>
        /// <returns>Boolscher Wert, der angibt, ob ein Spieler die benötigten Resouren besitzt</returns>
        public bool haveRes(User user, ResList price)
        {
            ResList have = getResources(user);

            foreach (KeyValuePair<ResType, double> want in price)
            {
                if (want.Value > have[want.Key])
                {
                    return false;
                }

            }

            return true;
        }


        /// <summary>
        /// Helferfunktion, die den Resourcengewinn der Spieler regelt
        /// </summary>
        /// <param name="user">Benutzer</param>
        private void fkt_checkRes(User user)
        {


            List<Station> stationList = game.getStations();

            foreach (Station stat in stationList)
            {
                if (stat.Uid == user)
                {
                    ResList gain = stat.Type.create_res;

                    addRes(user, gain);

                }
            }



            List<Planet> planetenList = game.getPlanets();

            foreach (Planet planet in planetenList)
            {
                if (planet.UID == user)
                {
                    ResList gain = planet.type.create_res;

                    addRes(user, gain);


                }
            }


            foreach (KeyValuePair<ResType, double> res in resourceList[user])
            {
                if (res.Value > maxres)
                {
                    resourceList[user][res.Key] = maxres;
                }


            }

        }


        /// <summary>
        /// Fügt einen Spieler der Liste hinzu
        /// </summary>
        /// <param name="user">Spieler</param>
        public void addUser(User user)
        {
            resourceList.Add(user, new ResList());
        }



    }
}
