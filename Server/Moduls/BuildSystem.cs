using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game.Game;

namespace Server.Moduls
{
    [Serializable]
    class BuildSystem : Modul
    {

        public BuildSystem(Main main) : base(main) { }

        public void handleBuilds()
        {


            main.log("Modul BuildSystem: Baulistenüberprüfung abgeschlossen");
        }




      



    }
}
