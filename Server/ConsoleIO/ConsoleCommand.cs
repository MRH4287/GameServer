using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.ConsoleIO
{
    /// <summary>
    /// Klasse für die Interaktion durch die Konsole
    /// </summary>
    class ConsoleCommand
    {
        /// <summary>
        /// Refferenz auf den Spiel Server
        /// </summary>
        private Main system;

        /// <summary>
        /// Alle verfügbaren Konsolen Kommandos
        /// </summary>
        private enum Command
        {
            stop, exit, info, help, save, load, addUser, getUser, mapRecreate
        }

        /// <summary>
        /// Liste mit allen HilfeTexten für die Funktionen
        /// </summary>
        private Dictionary<Command, string> helptext = new Dictionary<Command, string>();


        /// <summary>
        /// Instanziert den Listener für neue Konsolen Abfragen
        /// </summary>
        /// <param name="server">Server Instanz</param>
        public ConsoleCommand(Main server)
        {
            this.system = server;
            initHelpText();

            Task helpTask = new Task(new Action(inputFunktion));
            helpTask.Start();

        }

        /// <summary>
        /// Initiert die Hilfe Texte
        /// </summary>
        private void initHelpText()
        {
            this.helptext.Clear();

            helptext.Add(Command.exit, "Beendet den Server");
            helptext.Add(Command.stop, "Alias für exit");
            helptext.Add(Command.info, "Zeigt Statistiken an");
            helptext.Add(Command.load, "Läd einen Spielstand (Unstable)");
            helptext.Add(Command.save, "Speichert einen Spielstand (Unstable)");
            helptext.Add(Command.addUser, "Fügt einen neuen Test-Benutzer hinzu");
            helptext.Add(Command.getUser, "Zeigt alle Benutzer an");
            helptext.Add(Command.help, "Zeigt diese Hilfe Seite an");
            helptext.Add(Command.mapRecreate, "Generiert eine neue Karte");

        }


        private void inputFunktion()
        {
            string input = Console.ReadLine();

            string[] array = input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                Command kommando = (Command)Enum.Parse(typeof(Command), array[0]);

                switch (kommando)
                {
                    case Command.stop:
                    case Command.exit:
                        system.stop();
                        break;

                    case Command.info:
                        system.printStatistics();
                        break;

                    case Command.help:

                        int sizename = 0;
                        int sizehelp = 0;

                        foreach (KeyValuePair<Command, string> command in helptext)
                        {
                            if (command.Key.ToString().Length > sizename)
                            {
                                sizename = command.Key.ToString().Length;
                            }

                            if (command.Value.ToString().Length > sizehelp)
                            {
                                sizehelp = command.Value.ToString().Length;
                            }

                        }

                        StringBuilder namelines = new StringBuilder();
                        for (int i = 0; i < sizename + 1; i++)
                        {
                            namelines.Append("-");
                        }

                        StringBuilder namehelp = new StringBuilder();
                        for (int i = 0; i < sizehelp; i++)
                        {
                            namehelp.Append("-");
                        }

                        Console.WriteLine(namelines.ToString() + "-" + namehelp.ToString());

                        Console.WriteLine(getHelpText("Befehle", sizename) + " | Wirkung");
                        Console.WriteLine(namelines.ToString() + "+" + namehelp.ToString());

                        foreach (KeyValuePair<Command, string> command in helptext)
                        {
                            Console.WriteLine(getHelpText(command.Key.ToString(), sizename) + " | " + command.Value);
                        }


                        break;

                    case Command.save:

                        try
                        {
                            system.save("./save/" + array[1] + ".sav");

                        }
                        catch (Exception ex)
                        {
                            system.log("Fehler: " + ex.Message);
                        }

                        break;


                    case Command.load:

                        try
                        {
                            Program.load("./save/" + array[1] + ".sav");

                        }
                        catch (Exception ex)
                        {
                            system.log("Fehler: " + ex.Message);
                        }

                        break;

                    case Command.addUser:

                        Game.Game.User user = new Game.Game.User(1, array[1], "1", new Game.Game.Race(1, "1"));
                        user.password = array[2];
                        user.encryptPassword();

                        system.addUser(user);

                        break;

                    case Command.getUser:

                        Console.WriteLine();
                        Console.WriteLine(" ------ Benutzerliste: --------");

                        foreach (Game.Game.User gU_user in system.game.getUsers())
                        {
                            Console.WriteLine(gU_user.username);
                        }
                        Console.WriteLine();


                        break;

                    case Command.mapRecreate:

                        system.log("Erstelle neue Map Datei ...");
                        system.modulmanager.map.reCreateMap();


                        break;


                }
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Der Befehl konnte nicht gefunden werden!");
            }
            catch
            {
                Console.WriteLine("Der Befehl konnte nicht bearbeitet werden!");
            }


            inputFunktion();
        }


        private string getHelpText(string text, int length)
        {
            StringBuilder builder = new StringBuilder();
            int toAppend = length - text.Length;
            builder.Append(text);
            for (int i = 0; i < toAppend; i++)
            {
                builder.Append(" ");
            }

            return builder.ToString();
        }

    }
}
