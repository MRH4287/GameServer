using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game.Game;

namespace Server.Moduls
{
    /// <summary>
    /// Technologie Manager
    /// </summary>
    /// 
    [Serializable]
    class TechSystem : Modul
    {
     

        /// <summary>
        /// Liste die Angibt, welche TEchnologien ein Spieler besitzt
        /// </summary>
        private Dictionary<User, List<Tech>> techlist = new Dictionary<User, List<Tech>>();

        /// <summary>
        /// Lieste die angibt, welche Forschungen ein Spieler betreibt
        /// </summary>
        private Dictionary<User, List<Forschung>> researchlist = new Dictionary<User, List<Forschung>>();


        /// <summary>
        /// Management Klasse für Technologie
        /// </summary>
        /// <param name="main">Main</param>
        public TechSystem(Main main) : base(main)
        {
            
        }


        /// <summary>
        /// Liefert alle erforschten Technologien eines Spielers zurück 
        /// </summary>
        /// <param name="user">Spieler</param>
        /// <returns>Alle erforschten Technologien</returns>
        public List<Tech> getTechs(User user)
        {

            if ((techlist.ContainsKey(user)) && (techlist[user] != null))
            {
                return techlist[user];
            }
            return new List<Tech>();

        }

        /// <summary>
        /// Liefert alle Forschungen eines Spielers zurück
        /// </summary>
        /// <param name="user">Spieler</param>
        /// <returns>Liste aller Forschungen</returns>
        public List<Forschung> getResearch(User user)
        {
            if (researchlist.ContainsKey(user))
            {
                return researchlist[user];
            }
            else
            {
                return new List<Forschung>();
            }
        }


        /// <summary>
        /// Liefert alle Forschungen  zurück
        /// </summary>
        /// <returns>Liste aller Forschungen</returns>
        public List<Forschung> getResearch()
        {
            List<Forschung> list = new List<Forschung>();

            foreach (KeyValuePair<User, List<Forschung>> el in researchlist)
            {
                foreach (Forschung research in el.Value)
                {
                    list.Add(research);
                }


            }
            return list;
        }



        /// <summary>
        /// Überprüft ob ein Spieler eine Technologie besitzt
        /// </summary>
        /// <param name="user">Spieler</param>
        /// <param name="tech">Zu Überprüfende Technologie</param>
        /// <returns>Besitzt ein Spieler eine Technologie</returns>
        public bool haveTech(User user, Tech tech)
        {
            return getTechs(user).Contains(tech);

        }


        /// <summary>
        /// Fügt einem Spieler eine Technologie hinzu
        /// </summary>
        /// <param name="user">Spieler</param>
        /// <param name="tech">Technologie</param>
        public void addTech(User user, Tech tech)
        {

            lock (techlist)
            {

                List<Tech> have = getTechs(user);

                if (!have.Contains(tech))
                {
                    techlist[user].Add(tech);

                }

            }

        }


        /// <summary>
        /// Entfert eine Technologie von einem Spieler
        /// </summary>
        /// <param name="user">Spieler</param>
        /// <param name="tech">Technologie</param>
        public void removeTech(User user, Tech tech)
        {
            lock (techlist)
            {

                List<Tech> have = getTechs(user);

                if (have.Contains(tech))
                {
                    techlist[user].Remove(tech);
                }

            }
        }


        /// <summary>
        /// Fügt einen Spieler der Liste hinzu
        /// </summary>
        /// <param name="user">Spieler</param>
        public void addUser(User user)
        {
            techlist.Add(user, new List<Tech>());
        }


        /// <summary>
        /// Funktion, die überprüft ob Forschungen vollendet sind.
        /// </summary>
        public void checkTech()
        {

            List<User> userlist = game.getUsers();

            System.Threading.Tasks.Parallel.ForEach(userlist, user =>
            {
                fkt_checkTech(user);
            });


            main.log("Modul Technologie: Forschungs-Überpüfung abgeschlossen");

        }


        /// <summary>
        /// Subroutine für checkTech
        /// </summary>
        /// <param name="user">Benutzer für den die Überprüfung durchgeführt werden soll</param>
        private void fkt_checkTech(User user)
        {
            if (researchlist.ContainsKey(user))
            {
                List<Forschung> list = researchlist[user];

                foreach (Forschung forschung in list)
                {
                    int finish = forschung.Started + forschung.Tech.time;
                    if (finish <= main.Round)
                    {
                        // Forschung fertig:

                        addTech(user, forschung.Tech);
                        researchlist[user].Remove(forschung);


                    }
                }

            }

        }

    }
}
