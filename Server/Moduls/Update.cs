using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game.Game;


namespace Server.Moduls
{
    /// <summary>
    /// Update System
    /// Managed die Schiffs / Stations Updates
    /// </summary>
    /// 
    [Serializable]
    class UpdateSystem : Modul
    {
  

        /// <summary>
        /// Liste in der die Updates gespeichert sind, die ein Spieler besitzt
        /// </summary>
        Dictionary<User, List<Update>> updatelist = new Dictionary<User, List<Update>>();


        public UpdateSystem(Main main) : base(main)
        {
         
        }

        /// <summary>
        /// Liefert alle Updates eines Spielers zurück
        /// </summary>
        /// <param name="user">Spieler</param>
        /// <returns>Alle Updates eines Spielers</returns>
        public List<Update> getUpdates(User user)
        {

            if (updatelist.ContainsKey(user))
            {
                return updatelist[user];
            }
            else
            {
                return new List<Update>();
            }
        }


        /// <summary>
        /// Überpprüft die Verbesserungen die die Updates auslösen
        /// </summary>
        public void HandleUpdates()
        {
            List<User> userlist = game.getUsers();

            System.Threading.Tasks.Parallel.ForEach(userlist, user =>
            {
                CheckUpdates(user);
            });


            main.log("Modul Updates: Update Überprüfung abgeschlossen");
        }

        /// <summary>
        /// Überprüft die Updates für einen Spieler
        /// </summary>
        /// <param name="user">Spieler</param>
        private void CheckUpdates(User user)
        {
            List<Ship> listshipps = game.getShips();
            List<Station> liststations = game.getStations();
            List<Update> updates = getUpdates(user);


            foreach (Ship ship in listshipps)
            {
                if (ship.Uid == user)
                {
                    ship.power = ship.Type.power;
                    ship.power2 = ship.Type.power2;
                    ship.power3 = ship.Type.power3;
                    ship.power4 = ship.Type.power4;

                    ship.resistend1 = ship.Type.resistend1;
                    ship.resistend2 = ship.Type.resistend2;
                    ship.resistend3 = ship.Type.resistend3;
                    ship.resistend4 = ship.Type.resistend4;

                    ship.speed = ship.Type.speed;

                    foreach (Update update in updates)
                    {
                        if (update.shiptype == ship.Type)
                        {
                            ship.power += update.strength;
                            ship.power2 += update.strength2;
                            ship.power3 += update.strength3;
                            ship.power4 += update.strength4;

                            ship.resistend1 += update.resistend1;
                            ship.resistend2 += update.resistend2;
                            ship.resistend3 += update.resistend3;
                            ship.resistend4 += update.resistend4;

                            ship.speed += update.speed;

                        }

                    }

                }
            }

            foreach (Station station in liststations)
            {
                if (station.Uid == user)
                {
                    station.power = station.Type.power;
                    station.power2 = station.Type.power2;
                    station.power3 = station.Type.power3;
                    station.power4 = station.Type.power4;

                    station.resistend1 = station.Type.resistend1;
                    station.resistend2 = station.Type.resistend2;
                    station.resistend3 = station.Type.resistend3;
                    station.resistend4 = station.Type.resistend4;

                    foreach (Update update in updates)
                    {
                        if (update.stattype == station.Type)
                        {
                            station.power += update.strength;
                            station.power2 += update.strength2;
                            station.power3 += update.strength3;
                            station.power4 += update.strength4;

                            station.resistend1 += update.resistend1;
                            station.resistend2 += update.resistend2;
                            station.resistend3 += update.resistend3;
                            station.resistend4 += update.resistend4;

                        }

                    }

       
                }
            }



            // Überprüfung ob die Datenbankeinträge passen.

            List<Tech> listtech = main.modulmanager.tech.getTechs(user);


            List<Update> shouldHave = new List<Update>();

            foreach (Tech tech in listtech)
            {
                foreach (Update up in tech.update)
                {
                    shouldHave.Add(up);
                }
            }

            foreach (Update up in shouldHave)
            {
                if (!updates.Contains(up))
                {
                    updatelist[user].Add(up);
                }


            }

        }



    }
}
