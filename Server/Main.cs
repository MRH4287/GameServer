using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MysqlConnect;
using Network;
using Game;
using Game.Game;
using System.IO;
using Communication;

using Server.Moduls;


namespace Server
{
    [Serializable]
    class Main : IMysql
    {
        // Network:
        [NonSerialized()]
        TCP network;

        [NonSerialized()]
        private volatile System.Collections.Generic.Dictionary<int, Command> response = new Dictionary<int, Command>();
        [NonSerialized()]
        private Translator tr = new Translator();

        [NonSerialized()]
        public Dictionary<User, System.Net.Sockets.TcpClient> userClientList = new Dictionary<User, System.Net.Sockets.TcpClient>();

        int sessionKey = (new Random()).Next(1000,9999);

        private Dictionary<string, User> userSessions = new Dictionary<string,User>();

        Encryption.KeyGenerator sessionKeyGenerator;


        // Config
        [NonSerialized()]
        public Data.Config config = new Data.Config("Config/config.xml");

        // Game
        public GameData game;
        public ModulManager modulmanager;

        //Rundenzahl:
        /// <summary>
        /// In welcher Runde, befinden wir uns
        /// </summary>
        private int round = 0;
        /// <summary>
        /// Gibt an, in welcher Runde sich das Spiel befindet
        /// </summary>
        public int Round
        {
            get
            {
                return round;
            }
        }



        #region Threading System

        /// <summary>
        /// Varibale, die sagt, ob das System den Server beenden soll.
        /// </summary>
        private volatile bool close = false;

        /// <summary>
        /// Solange diese Funktion true liefert, wird der Server ausgeführt.
        /// </summary>
        /// <returns>true wenn Server läuft</returns>
        public bool run()
        {
            return !close;
        }

        /// <summary>
        /// Startet den Server
        /// </summary>
        public void start()
        {
            main();
        }
        #endregion

        public Main()
        {
            sessionKeyGenerator = new Encryption.KeyGenerator(sessionKey);
        }


        public void stop()
        {
            log("System Abschaltung Initiert....");
            close = true;
        }


        public void main()
        {
            checkConfig();

            //   try
            //  {
            int serverPort = int.Parse(config["TCP/port"]);
            network = new TCP(serverPort, "GamePW");
            network.OnTextRecieved += new TCP.TextRecievedEvent(TCP_TextRecieved);
            network.OnClientConnected += new TCP.ClientConnectedEvent(TCP_ClientConnected);
            network.OnError += new TCP.TCPErrorEvent(TCP_Error);
            network.OnClientDisconnected += new TCP.ClientDisconnectedEvent(TCP_ClientDisconnected);


            log("Lade Spiel Daten ... ");

            string filepath = config["Game/GameDataPath"] + "GameData.dat";
            if (!File.Exists(filepath))
            {
                Console.WriteLine("Game Data Datei nicht gefunden!");
                close = true;
                return;
            }

            game = new GameData(filepath);
            log("Spiel Dateien erfolgreich geparsed");


            modulmanager = new ModulManager(this);
            log("Modul Manager erfolgreich gestartet");

            printStatistics();

            //  log("Server bereit für Anfragen ...");


            // TestUser
            User user = new User(1, "test", "4", game.getRace(1));
            user.password = "test";
            user.encryptPassword();

            addUser(user);


            log("Lade Map-Daten bitte warten...");

            modulmanager.map.load(config["Game/Map/File"]);



            //   }
            //  catch (Exception ex)
            //  {
            //      log("Fehler: " + ex.Message);
            //  close = true;
            //  }
        }



        #region IMysql Member

        public void MYSQL_Error(Exception error)
        {
            Console.WriteLine("MysqlError: " + error.Message);
        }

        #endregion

        #region TCP_Server Member

        public void TCP_TextRecieved(string input, byte[] byteInput, System.Net.Sockets.TcpClient client)
        {
            Translator tr = new Translator();
            Command com = tr.getCommand(byteInput);

            string arguments = "";
            foreach (Object ob in com.Arguments)
            {
                arguments += ob.ToString() + ", ";
            }

            log("Command Recieved: " + com.command + " (" + arguments + ")");



            if (com.command == "response")
            {
                int UID = (int)com.Arguments[0];
                response.Add(UID, com);
            }
            else
            {
                Command response = handleRequest(com, client);

                network.TCP_SendByteStream(tr.writeCommand(response), client);
            }

        }

        public void TCP_ClientConnected(System.Net.Sockets.TcpClient client)
        {
            log("Neuer Client Connected");


        }

        public void TCP_Error(Exception error)
        {
            Console.WriteLine("Network Error: " + error.Message);
        }


        public void TCP_ClientDisconnected(System.Net.Sockets.TcpClient client)
        {

            if (userClientList.ContainsValue(client))
            {
                User disconnected = null;

                foreach (KeyValuePair<User, System.Net.Sockets.TcpClient> element in userClientList)
                {
                    if (element.Value == client)
                    {
                        log("Benutzer " + element.Key.username + " Disconnected");
                        disconnected = element.Key;
                    }
                }

                if (disconnected != null)
                {
                    userClientList.Remove(disconnected);
                }

            }


        }

        #endregion

        #region Game

        public void addUser(User user)
        {
            game.addUser(user);
            modulmanager.res.addUser(user);
            modulmanager.tech.addUser(user);
        }


        private enum dataManipulationMode
        {
            SELECT, UPDATE, DELETE, ADD, NONE, TEST, LOGIN
        }

        private enum dataManipulationSource
        {
            USER, SHIP, STATION, PLANET, MAP, RACE, PLANETTYPE,
            TECH, SHIPTYPE, STATIONTYPE, SKILL, UPDATE, FORSCHUNG
        }

        private struct Request
        {
            public dataManipulationMode dataMode;
            public dataManipulationSource source;
            public string condition;
            public string conditionValue;
            public bool contains;
            public List<Object> Arguments;

        }

        private Request commandData(Command request)
        {
            string command = request.command;

            try
            {
                Request requestData = new Request();
                requestData.condition = "";
                requestData.conditionValue = "";

                command = command.ToLower();
                string[] data = command.Split(new char[] { ' ' });

                // SQL Mäsige System Eingaben:
                dataManipulationMode dataMode = (dataManipulationMode)Enum.Parse(typeof(dataManipulationMode), data[0].ToUpper());
                requestData.dataMode = dataMode;



                dataManipulationSource dataSource = (dataManipulationSource)Enum.Parse(typeof(dataManipulationSource), data[1].ToUpper());
                requestData.source = dataSource;

                if (data.Length > 2)
                {

                    if (data[2] == "where")
                    {
                        if (data[3] == "contains")
                        {
                            requestData.contains = true;
                            if (data.Length > 4)
                                requestData.condition = data[4];
                        }
                        else
                        {
                            requestData.contains = false;
                            string[] conditionData = data[3].Split(new char[] { '=' });
                            requestData.condition = conditionData[0];
                            if (conditionData.Length > 1)
                                requestData.conditionValue = conditionData[1];
                        }

                    }
                }

                requestData.Arguments = request.Arguments;

                return requestData;

            }
            catch (Exception ex)
            {
                log("Fehler: " + ex.Message + " wegen:");
                log("Fehlerhafte Anweisung erhalten: " + command);
            }

            Request d = new Request();
            d.dataMode = dataManipulationMode.NONE;
            return d;

        }

        private Command handleRequest(Command request, System.Net.Sockets.TcpClient client)
        {
            string command = request.command;
            Command result = new Command();
            int UID = (int)request.Arguments[0];
            result.command = "response";
            result.Arguments.Add(UID);

            Request info = commandData(request);


            switch (info.dataMode)
            {
                case dataManipulationMode.SELECT:

                    switch (info.source)
                    {


                        case dataManipulationSource.PLANET:

                            List<Planet> planets = game.getPlanets();

                            if (info.contains)
                            {
                                Planet PCP = (Planet)info.Arguments[1];

                                result.Arguments.Add(planets.Contains(PCP));


                            }
                            else
                            {
                                switch (info.condition)
                                {
                                    case "uid":

                                        int SPId = int.Parse(info.conditionValue);

                                        foreach (Planet planet_data in planets)
                                        {
                                            if (planet_data.UID.id == SPId)
                                            {
                                                result.Arguments.Add(planet_data);
                                            }
                                        }
                                        break;

                                    case "user":

                                        User SPUs = (User)info.Arguments[1];
                                        foreach (Planet planet_data in planets)
                                        {
                                            if (planet_data.UID == SPUs)
                                            {
                                                result.Arguments.Add(planet_data);
                                            }
                                        }
                                        break;

                                    case "type":

                                        PlanetClass SPPc = (PlanetClass)info.Arguments[1];
                                        foreach (Planet planet_data in planets)
                                        {
                                            if (planet_data.type == SPPc)
                                            {
                                                result.Arguments.Add(planet_data);
                                            }
                                        }

                                        break;

                                    case "typeid":

                                        int SPPCId = int.Parse(info.conditionValue);

                                        foreach (Planet planet_data in planets)
                                        {
                                            if (planet_data.type.Id == SPPCId)
                                            {
                                                result.Arguments.Add(planet_data);
                                            }
                                        }

                                        break;

                                    case "":

                                        foreach (Planet planet_data2 in planets)
                                        {
                                            result.Arguments.Add(planet_data2);
                                        }

                                        break;

                                }
                            }

                            break;

                        case dataManipulationSource.PLANETTYPE:

                            List<PlanetClass> SPCtypes = game.getPlanetTypes();



                            switch (info.condition)
                            {
                                case "id":

                                    int SPId = int.Parse(info.conditionValue);

                                    foreach (PlanetClass planet_data in SPCtypes)
                                    {
                                        if (planet_data.Id == SPId)
                                        {
                                            result.Arguments.Add(planet_data);
                                        }
                                    }

                                    break;

                                case "":

                                    foreach (PlanetClass planet_data2 in SPCtypes)
                                    {
                                        result.Arguments.Add(planet_data2);
                                    }

                                    break;

                            }



                            break;

                        case dataManipulationSource.SHIP:

                            List<Ship> SSHs = game.getShips();

                            switch (info.condition)
                            {
                                case "uid":

                                    int SPId = int.Parse(info.conditionValue);

                                    foreach (Ship data in SSHs)
                                    {
                                        if (data.Uid.id == SPId)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }
                                    break;

                                case "user":

                                    User SPUs = (User)info.Arguments[1];
                                    foreach (Ship data in SSHs)
                                    {
                                        if (data.Uid == SPUs)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }
                                    break;

                                case "type":

                                    ShipClass SPPc = (ShipClass)info.Arguments[1];
                                    foreach (Ship data in SSHs)
                                    {
                                        if (data.Type == SPPc)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }

                                    break;

                                case "typeid":

                                    int SPPCId = int.Parse(info.conditionValue);

                                    foreach (Ship data in SSHs)
                                    {
                                        if (data.Type.Id == SPPCId)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }

                                    break;

                                case "fleet":

                                    int SPPFl = int.Parse(info.conditionValue);

                                    foreach (Ship data in SSHs)
                                    {
                                        if (data.Fleet == SPPFl)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }


                                    break;

                                case "":

                                    foreach (Ship data in SSHs)
                                    {
                                        result.Arguments.Add(data);
                                    }

                                    break;

                            }



                            break;

                        case dataManipulationSource.SHIPTYPE:

                            List<ShipClass> SSTys = game.getShipTypes();

                            switch (info.condition)
                            {
                                case "id":

                                    int SPId = int.Parse(info.conditionValue);

                                    foreach (ShipClass data in SSTys)
                                    {
                                        if (data.Id == SPId)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }
                                    break;


                                case "race":

                                    Race SPRs = (Race)info.Arguments[1];
                                    foreach (ShipClass data in SSTys)
                                    {
                                        if (data.race.Contains(SPRs))
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }

                                    break;

                                case "raceid":

                                    int SPRsID = int.Parse(info.conditionValue);

                                    foreach (ShipClass data in SSTys)
                                    {
                                        foreach (Race raceData in data.race)
                                        {
                                            if (raceData.id == SPRsID)
                                            {
                                                result.Arguments.Add(data);
                                            }
                                        }
                                    }

                                    break;



                                case "":

                                    foreach (ShipClass data in SSTys)
                                    {
                                        result.Arguments.Add(data);
                                    }

                                    break;

                            }


                            break;

                        case dataManipulationSource.STATION:

                            List<Station> SSSs = game.getStations();

                            switch (info.condition)
                            {
                                case "uid":

                                    int SPId = int.Parse(info.conditionValue);

                                    foreach (Station data in SSSs)
                                    {
                                        if (data.Uid.id == SPId)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }
                                    break;

                                case "user":

                                    User SPUs = (User)info.Arguments[1];
                                    foreach (Station data in SSSs)
                                    {
                                        if (data.Uid == SPUs)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }
                                    break;

                                case "type":

                                    StationClass SPPc = (StationClass)info.Arguments[1];
                                    foreach (Station data in SSSs)
                                    {
                                        if (data.Type == SPPc)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }

                                    break;

                                case "typeid":

                                    int SPPCId = int.Parse(info.conditionValue);

                                    foreach (Station data in SSSs)
                                    {
                                        if (data.Type.Id == SPPCId)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }

                                    break;

                                case "":

                                    foreach (Station data in SSSs)
                                    {
                                        result.Arguments.Add(data);
                                    }

                                    break;

                            }


                            break;

                        case dataManipulationSource.STATIONTYPE:
                            List<StationClass> SSSTys = game.getStationTypes();

                            switch (info.condition)
                            {
                                case "id":

                                    int SPId = int.Parse(info.conditionValue);

                                    foreach (StationClass data in SSSTys)
                                    {
                                        if (data.Id == SPId)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }
                                    break;


                                case "race":

                                    Race SPRs = (Race)info.Arguments[1];
                                    foreach (StationClass data in SSSTys)
                                    {
                                        if (data.race.Contains(SPRs))
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }

                                    break;

                                case "raceid":

                                    int SPRsID = int.Parse(info.conditionValue);

                                    foreach (StationClass data in SSSTys)
                                    {
                                        foreach (Race raceData in data.race)
                                        {
                                            if (raceData.id == SPRsID)
                                            {
                                                result.Arguments.Add(data);
                                            }
                                        }
                                    }

                                    break;



                                case "":

                                    foreach (StationClass data in SSSTys)
                                    {
                                        result.Arguments.Add(data);
                                    }

                                    break;

                            }

                            break;

                        case dataManipulationSource.RACE:

                            List<Race> SRtypes = game.getRaces();



                            switch (info.condition)
                            {
                                case "id":

                                    int SPId = int.Parse(info.conditionValue);

                                    foreach (Race planet_data in SRtypes)
                                    {
                                        if (planet_data.id == SPId)
                                        {
                                            result.Arguments.Add(planet_data);
                                        }
                                    }

                                    break;

                                case "":

                                    foreach (Race planet_data in SRtypes)
                                    {
                                        result.Arguments.Add(planet_data);
                                    }

                                    break;

                            }


                            break;


                        case dataManipulationSource.TECH:

                            List<Tech> STTypes = game.getTechs();

                            switch (info.condition)
                            {
                                case "id":

                                    int SPId = int.Parse(info.conditionValue);

                                    foreach (Tech data in STTypes)
                                    {
                                        if (data.Id == SPId)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }
                                    break;


                                case "race":

                                    Race SPRs = (Race)info.Arguments[1];
                                    foreach (Tech data in STTypes)
                                    {
                                        if (data.race.Contains(SPRs))
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }

                                    break;

                                case "raceid":

                                    int SPRsID = int.Parse(info.conditionValue);

                                    foreach (Tech data in STTypes)
                                    {
                                        foreach (Race raceData in data.race)
                                        {
                                            if (raceData.id == SPRsID)
                                            {
                                                result.Arguments.Add(data);
                                            }
                                        }
                                    }

                                    break;



                                case "":

                                    foreach (Tech data in STTypes)
                                    {
                                        result.Arguments.Add(data);
                                    }

                                    break;

                            }

                            break;

                        case dataManipulationSource.UPDATE:
                            List<Update> SUp = game.getUpdates();

                            switch (info.condition)
                            {
                                case "id":

                                    int SPId = int.Parse(info.conditionValue);

                                    foreach (Update data in SUp)
                                    {
                                        if (data.Id == SPId)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }
                                    break;


                                case "shiptype":

                                    ShipClass SPRs = (ShipClass)info.Arguments[1];
                                    foreach (Update data in SUp)
                                    {
                                        if (data.shiptype == SPRs)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }

                                    break;

                                case "shiptypeid":

                                    int SPRsID = int.Parse(info.conditionValue);

                                    foreach (Update data in SUp)
                                    {
                                        if (data.shiptype.Id == SPRsID)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }

                                    break;



                                case "":

                                    foreach (Update data in SUp)
                                    {
                                        result.Arguments.Add(data);
                                    }

                                    break;

                            }


                            break;

                        case dataManipulationSource.USER:

                            List<User> SUse = game.getUsers();

                            switch (info.condition)
                            {
                                case "id":

                                    int SPId = int.Parse(info.conditionValue);

                                    foreach (User data in SUse)
                                    {
                                        if (data.id == SPId)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }
                                    break;


                                case "username":

                                    string SPUs = info.conditionValue;

                                    foreach (User data in SUse)
                                    {
                                        if (data.username.ToLower() == SPUs)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }
                                    break;

                                case "race":

                                    Race SPRs = (Race)info.Arguments[1];
                                    foreach (User data in SUse)
                                    {
                                        if (data.race == SPRs)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }

                                    break;

                                case "raceid":

                                    int SPRsID = int.Parse(info.conditionValue);

                                    foreach (User data in SUse)
                                    {
                                        if (data.race.id == SPRsID)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }

                                    break;



                                case "":

                                    foreach (User data in SUse)
                                    {
                                        result.Arguments.Add(data);
                                    }

                                    break;

                            }

                            break;

                        case dataManipulationSource.FORSCHUNG:

                            List<Forschung> SFs = this.modulmanager.tech.getResearch();

                            switch (info.condition)
                            {
                                case "uid":

                                    int SPId = int.Parse(info.conditionValue);

                                    foreach (Forschung data in SFs)
                                    {
                                        if (data.User.id == SPId)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }
                                    break;

                                case "user":

                                    User SPUs = (User)info.Arguments[1];
                                    foreach (Forschung data in SFs)
                                    {
                                        if (data.User == SPUs)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }
                                    break;
                            }


                            break;

                        case dataManipulationSource.MAP:

                            // Dieser Wert dürfte keine Bedingungen benötigen:

                            result.Arguments.Add(modulmanager.map.getCurrentMap());

                            break;

                        case dataManipulationSource.SKILL:

                            List<Skill> SSk = game.getSkills();



                            switch (info.condition)
                            {
                                case "id":

                                    int SPId = int.Parse(info.conditionValue);

                                    foreach (Skill data in SSk)
                                    {
                                        if (data.Id == SPId)
                                        {
                                            result.Arguments.Add(data);
                                        }
                                    }

                                    break;

                                case "":

                                    foreach (Skill data in SSk)
                                    {
                                        result.Arguments.Add(data);
                                    }

                                    break;

                            }



                            break;

                    }



                    break;

                case dataManipulationMode.UPDATE:

                    switch (info.source)
                    {
                        case dataManipulationSource.MAP:
                            try
                            {
                                Map map = (Map)info.Arguments[1];

                                modulmanager.map.setMap(map);
                                log("Map-Datei erfolgreich übernommen");


                            }
                            catch
                            {
                                log("Fehler beim Empfangen der Map-Daten");

                            }
                            break;

                    }



                    break;

                case dataManipulationMode.DELETE:
                    break;

                case dataManipulationMode.ADD:
                    break;

                case dataManipulationMode.TEST:
                    result.Arguments.Add("OK");

                    break;

                case dataManipulationMode.NONE:
                    break;

                case dataManipulationMode.LOGIN:
                    string username = (string)request.Arguments[1];
                    string password = (string)request.Arguments[2];  

                    User user = login(username, password);
                    if (user != null)
                    {
                        userClientList.Add(user, client);
                        userSessions.Add(sessionKeyGenerator.getKey(), user);
                        
                    }

                    result.Arguments.Add(user);


                    break;

                default:

                    result.Arguments.Add(new ArgumentException("Befehl nicht erkannt"));

                    break;
            }


            return result;
        }


        private User login(string username, string password)
        {
            List<User> user = game.getUsers();

            foreach (User u in user)
            {
                string md5 = User.GetMD5Hash(password);
                if ((u.username == username) && (u.password == md5))
                {

                    u.Lastlogin = GameData.getTimestamp();

                    log("Benutzer " + u.username + " erfolgreich eingeloggt");

                    return u;
                }
            }

            return null;

        }


        public void save(string filename)
        {
            Translator tr = new Translator();
            ClassContainer container = new ClassContainer();
            container.type = ClassType.Main;
            container.objekt = this;
            List<ClassContainer> list = new List<ClassContainer>();
            list.Add(container);


            tr.writeData(list, filename);


        }

        public void prepareLoad()
        {
            network.TCP_Close();
        }


        public void loaded()
        {
            config = new Data.Config("Config/config.xml");
            response = new Dictionary<int, Command>();
            userClientList = new Dictionary<User, System.Net.Sockets.TcpClient>();
            tr = new Translator();

            checkConfig();


            int serverPort = int.Parse(config["TCP/port"]);
            network = new TCP(serverPort, "GamePW");
            network.OnTextRecieved += new TCP.TextRecievedEvent(TCP_TextRecieved);
            network.OnClientConnected += new TCP.ClientConnectedEvent(TCP_ClientConnected);
            network.OnError += new TCP.TCPErrorEvent(TCP_Error);
            network.OnClientDisconnected += new TCP.ClientDisconnectedEvent(TCP_ClientDisconnected);



            printStatistics();

            log("Server bereit für Anfragen ...");
        }


        #endregion

        #region Sonstiges





        public void log(string message)
        {
            Console.WriteLine(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + " - " + message);

        }

        private void checkConfig()
        {

            string curFile = @"Config\config.xml";
            if (!File.Exists(curFile))
            {
                Console.WriteLine("Config Datei nicht gefunden!");
                close = true;

            }
            else
            {
                config.parse(curFile);

            }


        }

        /// <summary>
        /// Schreib die Statistik in die Konsole
        /// </summary>
        public void printStatistics()
        {
            //Statistik:
            Console.WriteLine();
            Console.WriteLine("Statistik:");
            int stat_shipp_class = game.getShipTypes().Count;
            int stat_stat_class = game.getStationTypes().Count;
            int stat_planet_class = game.getPlanetTypes().Count;
            int stat_shipp = game.getShips().Count;
            int stat_stats = game.getStations().Count;
            int stat_users = game.getUsers().Count;
            int stat_updates = game.getUpdates().Count;
            int stat_tech = game.getTechs().Count;
            int stat_skills = game.getSkills().Count;
            int stat_race = game.getRaces().Count;
            int stat_research = modulmanager.tech.getResearch().Count;

            Console.WriteLine(stat_stat_class + " Stations Klassen geladen");
            Console.WriteLine(stat_shipp_class + " Shiffs Klassen geladen");
            Console.WriteLine(stat_planet_class + " Planeten Klassen geladen");
            Console.WriteLine(stat_shipp + " Schiffe geladen");
            Console.WriteLine(stat_stats + " Stationen geladen");
            Console.WriteLine(stat_users + " Benutzer geladen");
            Console.WriteLine(stat_updates + " Updates geladen");
            Console.WriteLine(stat_tech + " Technologien geladen");
            Console.WriteLine(stat_skills + " Fähigkeiten geladen");
            Console.WriteLine(stat_race + " Rassen geladen");
            Console.WriteLine(stat_research + " Forschungen geladen");
            Console.WriteLine();


        }



        #endregion

        #region Communication

        public Command request(Command request, System.Net.Sockets.TcpClient client)
        {

            int UID = (new Random()).Next(int.MaxValue);

            Command send = new Command();
            send.command = request.command;
            send.Arguments.Add(UID);

            foreach (Object ob in request.Arguments)
            {
                send.Arguments.Add(ob);
            }

            byte[] tmp = tr.writeCommand(send);

            network.TCP_SendByteStream(tmp, client);

            Command result = null;


            int count = 0;
            int count2 = 0;



            while (!response.ContainsKey(UID))
            {
                count++;
                System.Threading.Thread.Sleep(100);

                if (count >= 5)
                {
                    count2++;
                    network.TCP_SendByteStream(tr.writeCommand(send), client);
                    count = 0;
                }
                if (count2 >= 3)
                {
                    break;
                }

            }
            if (response.ContainsKey(UID))
            {
                result = response[UID];
                response.Remove(UID);
            }



            return result;

        }

        #endregion

    }
}
