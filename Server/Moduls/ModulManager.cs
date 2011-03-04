using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Moduls
{
    /// <summary>
    /// Modul Manager
    /// Diese Klasse Managed die vom Spiel benötigten Module
    /// </summary>
    [Serializable]
    class ModulManager
    {
        Main main;

        // Module
        public UpdateSystem update;
        public TechSystem tech;
        public Resources res;
        public BuildSystem build;
        public FlottenManagement fleet;
        public RelationManager relation;
        public NodeTrackingSystem nodetracking;
        public MapHandler map;

 

        public ModulManager(Main main)
        {
            this.main = main;
            update = new UpdateSystem(main);
            tech = new TechSystem(main);
            res = new Resources(main);
            build = new BuildSystem(main);
            fleet = new FlottenManagement(main);
            relation = new RelationManager(main);
            nodetracking = new NodeTrackingSystem(main);
            map = new MapHandler(main);

            //Test.NodeTrackingTest test = new Test.NodeTrackingTest();

            HandleModuls();
        }


        public void HandleModuls()
        {
         


            //   build.handleBuilds();
            update.HandleUpdates();
            tech.checkTech();
            res.checkRes();
            fleet.HandleMovement();


        }


    }
}
