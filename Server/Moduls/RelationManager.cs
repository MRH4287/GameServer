using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game.Game;

namespace Server.Moduls
{

    /// <summary>
    /// Eine Klasse, die die Beziehungen zwischen Spielern regelt
    /// </summary>
    [Serializable]
    class RelationManager : Modul
    {


        /// <summary>
        /// Der Wert, der Ausgegeben wird, wenn keine Beziehung in der Liste steht.
        /// </summary>
        RelationState defaultstate = RelationState.None;

        /// <summary>
        /// Das Konstrukt, dass verwendet wird um Beziehungen zu speichern
        /// </summary>
        /// 
        [Serializable]
        struct RelationHolder
        {
            public User userA;
            public User userB;
            public RelationState state;
        }

        /// <summary>
        /// Die Liste, in der gespeichert wird, welche Beziehung zwischen Spielern herrscht
        /// </summary>
        List<RelationHolder> list = new List<RelationHolder>();



        public RelationManager(Main main) : base(main)
        {
            
        }

        /// <summary>
        /// Liefert den Beziehungsstatus, der zwischen den beiden angegebenen Spielern besteht
        /// </summary>
        /// <param name="userA">Benutzer 1</param>
        /// <param name="userB">Benutzer 2</param>
        /// <returns>Beziehungsstatus</returns>
        public RelationState getRelation(User userA, User userB)
        {
            foreach (RelationHolder hold in list)
            {
                if (((userA == hold.userA) && (userB == hold.userB)) || ((userB == hold.userA) && (userA == hold.userB)))
                {
                    return hold.state;
                }

            }
            return defaultstate;
        }


        /// <summary>
        /// Setzt die Beziehungen zwischen zwei Spielern
        /// </summary>
        /// <param name="userA">Benutzer 1</param>
        /// <param name="userB">Benutzer 2</param>
        /// <param name="state">Beziehungsstatus</param>
        public void setRelation(User userA, User userB, RelationState state)
        {
            RelationHolder[] list2 = new RelationHolder[list.Count];
            this.list.CopyTo(list2);


            foreach (RelationHolder hold in list2)
            {
                if (((userA == hold.userA) && (userB == hold.userB)) || ((userB == hold.userA) && (userA == hold.userB)))
                {
                    list.Remove(hold);
                    RelationHolder holder = new RelationHolder();
                    holder.userA = userA;
                    holder.userB = userB;
                    holder.state = state;
                    list.Add(holder);
                    return;

                }

            }

            //Nicht in der Liste => neu Einfügen;
            RelationHolder holder2 = new RelationHolder();
            holder2.userA = userA;
            holder2.userB = userB;
            holder2.state = state;
            list.Add(holder2);


        }


    }
}
