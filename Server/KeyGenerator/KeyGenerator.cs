using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper
{
    /// <summary>
    /// Klasse zum erstellen und prüfen von CD-Keys
    /// </summary>
    class KeyGenerator
    {

        /// <summary>
        /// Passwort Schlüssel
        /// </summary>
        private int pw_key = 1234;
        /// <summary>
        /// Schlüssel
        /// </summary>
        private String key_list = "EAIGDBHFCP";
        /// <summary>
        /// Sicherungs-Level
        /// </summary>
        private int security_level = 3;


        /// <summary>
        /// Verwendeter Passwort Schlüssel
        /// </summary>
        public int Key
        {
            get
            {
                return pw_key;
            }
            set
            {
                if ((value < 1000) || (value > 9999))
                {
                    throw new ArgumentOutOfRangeException("Das Argument, darf nur zwischen 1000 und 9999 liegen!");
                }
                else
                {
                    pw_key = value;
                }
            }
        }


        /// <summary>
        /// Verwendeter Schlüssel
        /// </summary>
        public string KeyList
        {
            get
            {
                return key_list;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Das Argument key_list darf nicht null sein!");
                }

                if (value.Length != 10)
                {
                    throw new ArgumentException("Die Länge von key_list, muss genau 10 sein! Zudem darf kein Zeichen doppelt vorkommen");
                }

                key_list = value;

            }
        }


        /// <summary>
        /// Verwendeter Sicherheits-Schlüssel
        /// </summary>
        public int SecurityLevel
        {
            get
            {
                return security_level;
            }
            set
            {
                if ((value < 1) || (value > 5))
                {
                    throw new ArgumentOutOfRangeException("Das Argument security muss zwischen 1 und 5 liegen!");
                }

                this.security_level = value;
            }
        }


        /// <summary>
        /// Erstellt einen neuen Key-Generator
        /// </summary>
        /// <param name="pw_key">Der Wert, der bei überprüft wird, wenn ein Code generiert wurde. (1000-9999)</param>
        /// <param name="key_list">Eine Ersetzungstabelle (Länge = 10)</param>
        /// <param name="security">Sicherheits-Level (1-5)</param>
        public KeyGenerator(int pw_key, string key_list, int security)
        {
            this.pw_key = Key;
            this.key_list = KeyList;
            this.security_level = SecurityLevel;
        }


        /// <summary>
        /// Erstellt einen neuen Key-Generator
        /// </summary>
        /// <param name="pw_key">Der Wert, der bei überprüft wird, wenn ein Code generiert wurde. (1000-9999)</param>
        /// <param name="key_list">Eine Ersetzungstabelle (Länge = 10)</param>
        public KeyGenerator(int pw_key, string key_list)
            : this(pw_key, key_list, 3)
        {

        }

        /// <summary>
        /// Erstellt einen neuen Key-Generator
        /// </summary>
        /// <param name="pw_key">Der Wert, der bei überprüft wird, wenn ein Code generiert wurde. (1000-9999)</param>
        public KeyGenerator(int pw_key)
            : this(pw_key, "EAIGDBHFCP")
        {

        }



        /// <summary>
        /// Berechnet einen Code aus einer beliebigen Zeichenfolge
        /// </summary>
        /// <param name="indaten">Daten für die Berechnung</param>
        /// <returns>Berechneter Code</returns>
        private int rechne_key(String indaten)
        {

            int code = 0;
            int laenge = indaten.Length;
            int multipli = 1;
            char[] data = indaten.ToCharArray();



            for (int i = 0; i < laenge; i++)
            {
                code += ((int)data[i] * multipli);
                multipli += 2;

            }
            code += laenge;

            return code;
        }


        /// <summary>
        /// Überprüft einen Schlüssel auf Richtigkeit
        /// </summary>
        /// <param name="pw">Übergebener Key</param>
        /// <returns>Ist der Schlüssel Gültig?</returns>
        private Boolean pruefe_key(String pw)
        {
            try
            {

                String[] decodeTemp = pw.Split(new char[] { '-' });
                String part1 = "";
                String part2 = "";

                for (int i = 0; i < decodeTemp.Length - 1; i++)
                {

                    if (part1 == "")
                    {
                        part1 = decodeTemp[i];
                    }
                    else
                    {
                        part1 += "-" + decodeTemp[i];
                    }

                }


                part2 = decodeTemp[decodeTemp.Length - 1];

                int key = rechne_key(part1);

                String[] ar = part2.Split(new char[] { 'X' });


                if (ar.Length > 1)
                {

                    String code = ar[0] + ",";
                    String temp = ar[1];

                    for (int n = 0; n < temp.Length; n++)
                    {
                        Char temp2 = temp[n];

                        for (int i = 0; i < key_list.Length; i++)
                        {

                            if (temp2 == key_list[i])
                            {
                                code += (i).ToString();
                            }
                        }


                    }




                    float code2 = Convert.ToSingle(code);

                    Double tmp = key / code2;

                    Double abstand = pw_key - tmp;

                    if (abstand < 0) { abstand *= -1; }



                    if (abstand < (1 / (Math.Pow(10, security_level))))
                    {
                        return true;
                    }

                }
            }
            catch
            { }


            return false;
        }


        /// <summary>
        /// Überprüft ob ein Übergebener Schlüssel Valide ist
        /// </summary>
        /// <param name="key">Schlüssel der geprüft werden soll</param>
        /// <returns>Ist der Schlüssel valide</returns>
        public bool isValid(string key)
        {
            return pruefe_key(key);
        }


        /// <summary>
        /// Erstellt einen zufälligen Schlüssel
        /// </summary>
        /// <param name="length">Die Länge des Schlüssels</param>
        /// <param name="seed">Seed Wert</param>
        /// <returns>Zufälliger CD-Key</returns>
        private string generateRandomKey(int length = 6, int? seed = null)
        {
            string valueList = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; //abcdefghijklmnopqrstuvwxyz
            string key = "";

            try
            {
                Random rand;
                if (seed.HasValue)
                {
                    rand = new Random(seed.Value);
                }
                else
                {
                    rand = new Random();
                }

                for (int i = 0; i < length; i++)
                {
                    if (key != "")
                    {
                        key += "-";
                    }

                    for (int i2 = 0; i2 < 5; i2++)
                    {
                        int index = rand.Next(0, valueList.Length - 1);
                        key += valueList[index];

                    }
                }

            }
            catch
            { }

            return key;
        }


        /// <summary>
        /// Erstellt einen Schlüssel
        /// </summary>
        /// <param name="key">Der Beginn, des Schlüssels</param>
        /// <param name="depth">Aktuelle Tiefe</param>
        /// <returns>Gültiger CD-Key</returns>
        private string generateKey(string key = null, int depth = 0)
        {
            if (depth > 5)
            {
                return "";
            }



            if (key == null)
            {
                key = generateRandomKey();
            }
            int code = rechne_key(key);


            float correctionKey = ((float)code) / ((float)pw_key);
            string correctionKeyCode = "";
            String[] ar = correctionKey.ToString().Split(new char[] { ',' });
            correctionKeyCode = ar[0] + "X";

            if (ar.Length > 1)
            {
                int count = 0;
                foreach (char c in ar[1].ToCharArray())
                {
                    if (count++ < 5)
                    {
                        int index = int.Parse(c.ToString());
                        correctionKeyCode += key_list[index];
                    }
                }
            }
            string keyCode = key + "-" + correctionKeyCode;


            if (!pruefe_key(keyCode))
            {
                keyCode = generateKey(key + "-" + generateRandomKey(1, code), depth++);
            }


            return keyCode;

        }


        /// <summary>
        /// Liefert einen Zufälligen gültigen CD-Key
        /// </summary>
        /// <returns>Zufälliger Cd-Key</returns>
        public string getKey()
        {
            return generateKey();
        }

        /// <summary>
        /// Liefert einen Zufälligen gültigen CD-Key, der mit einer bestimmten Zeichenfolge beginnt
        /// </summary>
        /// <param name="begin">ZEichenfolge, mit der der Key beginnen soll</param>
        /// <returns>Zufälliger CD-Key</returns>
        public string getKey(string begin)
        {
            return generateKey(begin);
        }


    }
}
