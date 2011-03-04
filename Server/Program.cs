using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game;


namespace Server
{
    class Program
    {
        static Main system = new Main();


        static void Main(string[] args)
        {
            log("Server started.");

            Server.ConsoleIO.ConsoleCommand consoleIO = new ConsoleIO.ConsoleCommand(system);

            System.Action oAction = new System.Action(run);
            System.Threading.Tasks.Task oTask = new System.Threading.Tasks.Task(oAction);

            oTask.Start();

           while (system.run())
            {
                System.Threading.Thread.Sleep(1000);
            }
            

            log("Server closed");
        }

        static void run()
        {
            system.start();
        }

 

       public static void load(string filename)
        {
            system.prepareLoad();

            Communication.Translator tr = new Communication.Translator();
            List<Communication.ClassContainer> list = tr.readData(filename);
            Communication.ClassContainer container = list[0];
            system = (Main)container.objekt;
            system.loaded();

        }



         static void log(string message)
        {
            Console.WriteLine(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + " - " + message);

        }

    }
}
