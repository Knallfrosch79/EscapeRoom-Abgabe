using System;
using System.Security.Principal;

namespace MyApp
{
    internal class Program
    {
        // Der Escape Room muss ggf noch angepasst werden um wirklich "Spaß" zu machen, hat aber die Grundlegenden funktionen
        // Ich habe den Umgang mit Methoden, Enums, Structs und Dictionaries gelernt. Der Umgang mit Arrays bereitet mir noch
        // Schwierigkeiten, da ich manchmal nicht auf anhieb weiß, wie man sie abruft bearbeitet etc, aber das kommt mit der Zeit. 
        //Ich habe auch noch nicht alle Funktionen so verinnerlicht wie ich es gerne hätte, konzentriere mich trotzdem erstmal
        //auf die anderen Projekte bzw. Abgaben, damit ich nicht am Anfang schon komplett frustriert werde, weil undurchsichtige
        //Fehler entstehen, die nicht wirklich etwas beitragen.

        // Struktur zur Darstellung einer Position (Zeile und Spalte)
        struct Position
        {
            public int Row;
            public int Column;
        }

        // Enum mit den Symbolen
        enum Symbols
        {
            None = '.',
            Leer = ' ',
            Player = 'O',
            Key = '§',
            CornerLeftUp = '╔',
            CornerLeftDown = '╚',
            CornerRightUp = '╗',
            CornerRightDown = '╝',
            Wall = '║',
            WallHorizontal = '═',
            SecretRoom = '=',
            Door = '│',
            VerticalDoor = '-',
            OpenDoorSide = '_',
            OpenDoorTop = '┐',
            OpenDoorBottom = '└',
            Box = '█'
        }

        Dictionary<string, Symbols> doorsDictionary = new Dictionary<string, Symbols>()
        {
        {"MainGate", Symbols.Door },
        {"LabyrinthGate", Symbols.Door },
        {"MysteryGate", Symbols.Door }
        };


        // Globale Variablen
        //Spielfeld und Player
        private static int[,] playingField;
        private static char playerSymbol = (char)Symbols.Player;
        private static Symbols CurrentSymbol = Symbols.None;
        private static Position CurrentPosition = new Position();


        //Game Variable, dass es läuft, wird in der While Gameschleife verwendet
        private static bool GameIsRunning = true;
        private static bool easterEggFound = false; // Flag für das Easter Egg (Raum 3)

        // Unsere drei Räume als eigene Spielfeld-Arrays
        private static int[,] hubField;
        private static int[,] room2Field;
        private static int[,] room3Field;

        // Definiere die Positionen der Türen im Hub (Raum 1)
        private static Position hubDoorToRoom2;  // Tür zum Kanalisationsraum (Raum 2)
        private static Position hubDoorToRoom3;  // Tür zum Easter-Egg-Raum (Raum 3)
        private static Position hubExitDoor;     // Hauptausgang, der nur mit Schlüssel begehbar ist
        private static int hubDoorWallSide = -1; //Wanseite an der die Exittür platziert wird
        private static int room2DoorWallSide = -1; //Wanseite an der die Tür platziert wird
        private static bool hubDoorPlaced = false; // Flag, um zu überprüfen, ob die Tür bereits platziert wurde
        private static bool room2DoorPlaced = false; // Flag, um zu überprüfen, ob die Tür bereits platziert wurde
        private static bool room3DoorPlaced = false; // Flag, um zu überprüfen, ob die Tür bereits platziert wurde

        // Definiere Rücktüren in den anderen Räumen, damit der Spieler beim Rückgang an der Tür erscheint
        private static Position room2ReturnDoor; // Rücktür in Raum 2 (führt zum Hub)
        private static Position room3ReturnDoor; // (Optional – wir machen hier bei Raum 3 die Rückkehr direkt via ASCII)


        //Door Variablen
        private static char door = (char)Symbols.Door;
        private static bool doorOpen = false;
        private static int wallSide = 0; // 0 = links, 1 = rechts, 2 = oben, 3 = unten (für die Türpositionierung in der Methode LoadHub)
        //private static int wallSide;

        //Key Variable
        private static Position keyPosition;
        private static bool gotKey = false;
        private static bool placedKey = false; // Flag, um zu überprüfen, ob der Schlüssel bereits platziert wurde  

        //Var. für den Numbergenerator
        private static Random rnd = new Random();

        // Hauptprogramm
        static void Main(string[] args)
        {


            // Spielbegrüßung
            Console.SetCursorPosition(0, 1);
            String[] textZeilen = new string[]
            {
            " Willkommen Spieler ",
            " Du öffnest deine Augen und findest dich in einem Raum wieder, der einem Gefängnis gleicht. ",
            " Du siehst Fesseln an den Wänden und knochige Skellette die schon ewig hier zu sein scheinen. ",
            " Ich muss hier raus denkst du dir! Es muss doch irgendwo einen Schlüssel geben! "
            };
            TextBox(textZeilen);
            //Textbox(textZeilen);
            EnterText();

            // Regelerklärung
            Console.SetCursorPosition(0, 1);
            textZeilen = new string[]
            {
                " Spielablauf: ",
            " - Bestimme die Größe des Spielfelds. ",
            " - Wähle deinen Spielercharakter aus. ",
            " - Nutze WASD oder die Pfeiltasten zur Bewegung. ",
            " - Drücke E, um einen Schlüssel aufzunehmen. "
            };
            TextBox(textZeilen);
            EnterText();

            // Abfrage der Spielfeldgröße
            int x = 0, y = 0;
            Console.SetCursorPosition(0, 1);
            textZeilen = new string[]
            {
            " Wie groß soll der Escaperoom sein? ", " " , " Gib die Breite ein: "
            };
            TextBox(textZeilen);
            Console.WriteLine();
            do
            {
                string input = Console.ReadLine();
                if (!int.TryParse(input, out x))
                {
                    textZeilen = new string[]
                    {
                    " Ungültige Eingabe! Bitte gib eine ganze Zahl ein. "
                    };
                    TextBox(textZeilen);
                    continue;
                }
                if (x < 5 || x > 90)
                {
                    textZeilen = new string[]
                    {
                    " Die Breite muss zwischen 5 und 90 liegen. Bitte versuche es erneut. "
                    };
                    TextBox(textZeilen);
                }
            } while (x < 5 || x > 90);

            Console.Clear();
            Console.SetCursorPosition(0, 1);
            textZeilen = new string[]
                    {
                    $" Dein persönlicher Escape Room ist {x} Felder breit. ", " " , " Gib die Höhe ein: "
                    };
            TextBox(textZeilen);
            do
            {
                string input = Console.ReadLine();
                if (!int.TryParse(input, out y))
                {
                    textZeilen = new string[]
                    {
                    " Ungültige Eingabe! Bitte gib eine ganze Zahl ein. "
                    };
                    TextBox(textZeilen);
                    continue;
                }
                if (y < 5 || y > 50)
                {
                    textZeilen = new string[]
                    {
                    " Die Breite muss zwischen 5 und 50 liegen. Bitte versuche es erneut. "
                    };
                    TextBox(textZeilen);
                }
            } while (y < 5 || y > 50);

            Console.Clear();
            Console.SetCursorPosition(0, 1);
            textZeilen = new string[]
                    {
                    $" Dein persönlicher Escape Room wird erstellt und ist {x} breit und {y} hoch. "
                    };
            TextBox(textZeilen);
            EnterText();

            // Spielercharakter auswählen
            playerSymbol = SelectCharacter();
            Console.Clear();
            Console.SetCursorPosition(0, 1);
            textZeilen = new string[]
                    {
                    $" Du hast '{playerSymbol}' als deinen Spielercharakter gewählt. "
                    };
            TextBox(textZeilen);
            EnterText();

            // Spielfeld erstellen
            int width = x + 2;    // x als Breite die +2 dient dazu, den Rand zu berücksichtigen
            int height = y + 2;   // y als Höhe
            LoadHub(width, height); // direkt nach Spielfeldgrößenwahl
            playingField = CreatePlayingfield(width, height);

            if (width >= 50 && height >= 40)
            {
                // Großes Spielfeld: 6 Hindernisse
                PlaceObjectSimple();
                PlaceObjectSimple();
                PlaceObjectSimple();
                PlaceObjectSimple();
                PlaceObjectSimple();
                PlaceObjectSimple();
                PlaceObjectSimple();
                PlaceObjectSimple();
                PlaceObjectSimple();
                PlaceObjectSimple();
                PlaceObjectSimple();
                PlaceObjectSimple();
            }
            else if (width >= 30 && height >= 20)
            {
                // Mittleres Spielfeld: 4 Hindernisse
                PlaceObjectSimple();
                PlaceObjectSimple();
                PlaceObjectSimple();
                PlaceObjectSimple();
            }
            else if (width == 5 || height == 5)
            {
                PlaceObjectSimple();
            }
            else
            {
                // Kleineres Spielfeld: nur 2 Hindernis
                PlaceObjectSimple();
                PlaceObjectSimple();
            }

            // Initialisiere die Spielerposition (nutzt den Randomgenerator)
            CurrentPosition.Row = GenerateRandomNumber(1, height - 2);
            CurrentPosition.Column = GenerateRandomNumber(1, width - 2);

            LoadHub(width, height);

            // Spielfeld ausgeben und Eingaben verarbeiten
            //Console.SetBufferSize(200, 200);
            //Console.OutputEncoding = System.Text.Encoding.UTF8;

            //Spielschleife
            while (GameIsRunning)
            {
                // Zeichne das aktuelle Spielfeld (dies hängt davon ab, ob gerade hubField, room2Field oder room3Field aktiv ist)
                PrintField(playingField, width, height);

                // Verarbeite Benutzereingaben (Bewegung, Schlüssel aufsammeln etc.)
                CheckInput();

                // Überprüfe, in welchem Raum wir uns befinden und ob der Spieler an eine Tür gerückt ist:
                // --- Im Hub (Raum 1):
                // == Spieler im HUB ==
                if (playingField == hubField)
                {
                    // Tür zu Raum 2
                    if (CurrentPosition.Row == hubDoorToRoom2.Row && CurrentPosition.Column == hubDoorToRoom2.Column)
                    {
                        LoadRoom2(width, height);
                    }
                    // Tür zu Raum 3 (Easter Egg)
                    else if (CurrentPosition.Row == hubDoorToRoom3.Row && CurrentPosition.Column == hubDoorToRoom3.Column)
                    {
                        LoadRoom3(width, height);
                    }
                    // Hauptausgang – nur wenn Schlüssel vorhanden
                    else if (CurrentPosition.Row == hubExitDoor.Row && CurrentPosition.Column == hubExitDoor.Column)
                    {
                        if (gotKey)
                        {
                            // Sieg / Spielende
                            Console.Clear();
                            Console.WriteLine("Du hast erfolgreich das Verlies verlassen!");
                            GameIsRunning = false;
                        }
                        else
                        {
                            Console.SetCursorPosition(0, height + 2);
                            Console.WriteLine("Die Tür ist verschlossen. Du brauchst den Schlüssel.");
                        }
                    }
                }
                // == Spieler in Raum 2 (Kanalisation) ==
                else if (playingField == room2Field)
                {
                    if (CurrentPosition.Row == room2ReturnDoor.Row && CurrentPosition.Column == room2ReturnDoor.Column)
                    {
                        LoadHub(width, height, "fromRoom2");
                    }
                }
            }

            Console.Clear();
            if (playingField == hubField && CurrentPosition.Row == hubExitDoor.Row && CurrentPosition.Column == hubExitDoor.Column && gotKey)
            {
                Console.Clear();
                string[] text = new string[]
                {
                " Herzlichen Glückwunsch Gefangener!!! ",
                " Du hast das Verlies überlebt und brichst nun in ein Leben voller Abenteuer auf! "
                };
                TextBox(text);
                VictoryBeep();
                EnterText();
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Spiel wurde beendet.");
            }

        }

        private static int GenerateRandomNumber(int min, int max)
        {
            //erzeugt eine Zufallszahl im Bereich von Min und Max, sollte hierbei width und heigth sein
            return rnd.Next(min, max);
        }

        // Methode, die den Benutzer zur Eingabe auffordert und danach die Konsole leert, wurde am Anfang des Erstellungsprozesses gemacht, mit Keyinfo wäre es eleganter
        private static void EnterText()
        {
            Console.Write("\n Drücke 'Enter' um fortzufahren.");
            Console.ForegroundColor = ConsoleColor.Black; // Schriftfarbe wird geändert, sodass Eingaben nicht sichtbar sind
            Console.ReadLine();
            Console.ResetColor();
            Console.Clear();
        }

        static void TextBox(string[] textInput)
        {
            //int maxTextLength = 0;
            //foreach (string line in textInput)
            //{
            //    if (line.Length > maxTextLength)
            //        maxTextLength = line.Length;
            //}

            int maxTextLength = 0;
            for (int i = 0; i < textInput.Length; i++)
            {
                if (textInput[i].Length > maxTextLength)
                {
                    maxTextLength = textInput[i].Length;
                }
            }

            int height = textInput.Length + 4; // +2 für Rand, +2 für leere Zeilen
            int length = maxTextLength + 4;    // +2 für Rand, +2 für Leerzeichen

            string[,] boxArray = new string[height, length];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (i == 0 || j == 0 || i == height - 1 || j == length - 1)
                    {
                        boxArray[i, j] = ((char)Symbols.Box).ToString();
                    }
                    else
                    {
                        boxArray[i, j] = ((char)Symbols.Leer).ToString();
                    }
                }
            }

            // Text einfügen – mit Leerzeichen rechts & links
            for (int i = 0; i < textInput.Length; i++)
            {
                string line = textInput[i];
                int row = i + 2; // startet nach der oberen Leerzeile

                boxArray[row, 1] = " "; // Leerzeichen links
                for (int j = 0; j < line.Length; j++)
                {
                    boxArray[row, j + 2] = line[j].ToString();
                }
                boxArray[row, line.Length + 2] = " "; // Leerzeichen rechts
            }

            // Ausgabe
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    Console.Write(boxArray[i, j]);
                }
                Console.WriteLine();

            }
        }

        private static void LoadHub(int width, int height, string entrySide = "")
        {
            if (hubField == null)
            {
                hubField = CreatePlayingfield(width, height);
            }
            playingField = hubField; // playingField frühzeitig setzen

            if (gotKey == false && placedKey == false)
            {
                PlaceKey();
            } // Schlüssel wird platziert, wenn er noch nicht vorhanden ist

            // Türen setzen
            if (!hubDoorPlaced)
                placeHubDoor(width, height);
            else
                DrawHubDoor();

            if (!room2DoorPlaced)
                placeRoom2Door(width, height);
            else
                DrawRoom2Door();

            if (!room3DoorPlaced)
                placeRoom3Door(width, height);
            else
                DrawRoom3Door();

            // Spieler-Position festlegen
            if (entrySide == "fromRoom2")
            {
                CurrentPosition = new Position { Row = hubDoorToRoom2.Row, Column = hubDoorToRoom2.Column};
                hubField[hubDoorToRoom2.Row, hubDoorToRoom2.Column] = (char)Symbols.OpenDoorSide; // Tür öffnen
            }
            else if (entrySide == "fromRoom3")
            {
                CurrentPosition = new Position { Row = hubDoorToRoom3.Row, Column = hubDoorToRoom3.Column - 1 };
            }
            else
            {
                CurrentPosition = new Position { Row = height / 2, Column = width / 2 };
            }

            // Überprüfe, ob die Spielerposition außerhalb der Spielfeldgrenzen liegt
            if (CurrentPosition.Row < 1) CurrentPosition.Row = 1; // Verhindere, dass der Spieler oberhalb des Spielfelds spawnt
            if (CurrentPosition.Row >= height - 1) CurrentPosition.Row = height - 2; // Verhindere, dass der Spieler unterhalb des Spielfelds spawnt
            if (CurrentPosition.Column < 1) CurrentPosition.Column = 1; // Verhindere, dass der Spieler links außerhalb des Spielfelds spawnt
            if (CurrentPosition.Column >= width - 1) CurrentPosition.Column = width - 2; // Verhindere, dass der Spieler rechts außerhalb des Spielfelds spawnt

            // Aktives Spielfeld setzen
            playingField = hubField;
        }

        // Erzeugt das Spielfeld-Array mit Rand und Innenbereich
        private static int[,] CreatePlayingfield(int width, int height)
        {
            int[,] field = new int[height, width];

            for (int i = 0; i < height; i++) // i durchläuft die Zeilen
            {
                for (int j = 0; j < width; j++) // j durchläuft die Spalten
                {
                    // Wenn obere oder untere Zeile, setze horizontalen Rand
                    if (i == 0 || i == height - 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        field[i, j] = (char)Symbols.WallHorizontal;
                        Console.ResetColor();
                    }
                    // Wenn linke oder rechte Spalte, setze vertikalen Rand
                    else if (j == 0 || j == width - 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        field[i, j] = (char)Symbols.Wall;
                        Console.ResetColor();
                    }
                    else
                    {
                        // Innenbereich: Standardwert (hier: das Zeichen für Symbols.None)
                        field[i, j] = (char)Symbols.None;
                    }
                }
            }
            return field;
        }

        // Gibt das Spielfeld, den Spielercursor und das "HUD" in der Konsole aus
        private static void PrintField(int[,] playingField, int width, int height)
        {
            if (playingField == hubField)
            {
                if (gotKey)
                {
                    hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.OpenDoorBottom;
                }
                else
                {
                    hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.VerticalDoor;
                }
            }
            Console.Clear();
            Console.WriteLine();
            for (int i = 0; i < height; i++) // Zeilen
            {
                for (int j = 0; j < width; j++) // Spalten
                {
                    // Setze die Hintergrundfarbe für den Rand (obere/untere oder linke/rechte Zellen)
                    if (i == 0 || i == height - 1 || j == 0 || j == width - 1)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }

                    // Wenn die aktuelle Zelle der Spielerposition entspricht, wird ein "Cursor" gezeichnet & der Wandforeground wird gezeichnet
                    if (CurrentPosition.Row == i && CurrentPosition.Column == j)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        DrawCursor('[', playerSymbol, ']');
                    }
                    //1-4 else if: Gibt die Ecken des Spielfeldes aus
                    //5-6 else if: Sorgt dafür, dass die oberen Zeilen komplett gefüllt sind, ist durch DrawCursor nötig, da immer Leerzeichen eingefügt werden
                    else if (i == 0 && j == 0)
                    {
                        DrawCursor(' ', (char)Symbols.CornerLeftUp, (char)Symbols.WallHorizontal);
                    }
                    else if (i == height - 1 && j == 0)
                    {
                        DrawCursor(' ', (char)Symbols.CornerLeftDown, (char)Symbols.WallHorizontal);
                    }
                    else if (i == 0 && j == width - 1)
                    {
                        DrawCursor((char)Symbols.WallHorizontal, (char)Symbols.CornerRightUp, ' ');
                    }
                    else if (i == height - 1 && j == width - 1)
                    {
                        DrawCursor((char)Symbols.WallHorizontal, (char)Symbols.CornerRightDown, ' ');
                    }
                    else if (i == 0) //j wird nicht benötigt, da es in der Spalte keine Verschiebung durch die DrawCursor Methode gibt
                    {
                        DrawCursor((char)Symbols.WallHorizontal, (char)playingField[i, j], (char)Symbols.WallHorizontal);
                    }
                    else if (i == height - 1)   //sorgt dafür, dass die unteren Zeilen komplett gefüllt sind, ist durch DrawCursor nötig, da immer Leerzeichen eingefügt werden
                    {
                        DrawCursor((char)Symbols.WallHorizontal, (char)playingField[i, j], (char)Symbols.WallHorizontal);
                    }
                    else
                    {
                        DrawCursor(' ', (char)playingField[i, j], ' ');
                    }
                    Console.ResetColor();
                }
                Console.WriteLine(); // Zeilenumbruch nach jeder Zeile
            }
            //Console.Write("y: " + CurrentPosition.Row);       //(Debug Tool) 
            //Console.Write("x: " + CurrentPosition.Column);    //(Debug Tool)  

            //Stellt ein simples inventar unter dem escape room dar
            Console.Write("Items:"); if (gotKey == true) { Console.Write("Schlüssel"); } else { Console.Write("_________"); }
            if (easterEggFound == true) { Console.Write("      Easter Egg"); }
            Console.WriteLine();
            Console.Write("      "); Console.WriteLine("_________");

            // Wenn der Spieler auf der Schlüsselposition steht und den Schlüssel noch nicht hat, wird die Aufforderung angezeigt.
            if (!gotKey && keyPosition.Row == CurrentPosition.Row && keyPosition.Column == CurrentPosition.Column)
            {
                Console.WriteLine("Drücke 'e' um den Schlüssel einzusammeln");
            }
        }

        // Erstellt Raum 2 also die Kanalisation
        private static void LoadRoom2(int width, int height)
        {
            // Erzeuge ein neues Spielfeld für Raum 2
            room2Field = CreatePlayingfield(width, height);
            playingField = room2Field;

            // Bestimme die Wandseite der Tür im Hauptraum
            if (room2DoorWallSide == 0) // Linke Wand im Hauptraum
            {
                room2ReturnDoor = new Position { Row = CurrentPosition.Row, Column = width - 1 }; // Rechte Wand in Raum 2
            }
            else if (room2DoorWallSide == 1) // Rechte Wand im Hauptraum
            {
                room2ReturnDoor = new Position { Row = CurrentPosition.Row, Column = 0 }; // Linke Wand in Raum 2
            }
            else if (room2DoorWallSide == 2) // Obere Wand im Hauptraum
            {
                room2ReturnDoor = new Position { Row = height - 1, Column = CurrentPosition.Column }; // Untere Wand in Raum 2
            }
            else if (room2DoorWallSide == 3) // Untere Wand im Hauptraum
            {
                room2ReturnDoor = new Position { Row = 0, Column = CurrentPosition.Column }; // Obere Wand in Raum 2
            }

            // Platziere die Tür im Raum 2
            room2Field[room2ReturnDoor.Row, room2ReturnDoor.Column] = (char)Symbols.OpenDoorSide;

            // Positioniere den Spieler direkt vor der Tür
            if (room2ReturnDoor.Column == 0) // Linke Wand in Raum 2
            {
                CurrentPosition = new Position { Row = room2ReturnDoor.Row, Column = room2ReturnDoor.Column + 1 };
            }
            else if (room2ReturnDoor.Column == width - 1) // Rechte Wand in Raum 2
            {
                CurrentPosition = new Position { Row = room2ReturnDoor.Row, Column = room2ReturnDoor.Column - 1 };
            }
            else if (room2ReturnDoor.Row == 0) // Obere Wand in Raum 2
            {
                CurrentPosition = new Position { Row = room2ReturnDoor.Row + 1, Column = room2ReturnDoor.Column };
            }
            else if (room2ReturnDoor.Row == height - 1) // Untere Wand in Raum 2
            {
                CurrentPosition = new Position { Row = room2ReturnDoor.Row - 1, Column = room2ReturnDoor.Column };
            }

            // Setze das aktive Spielfeld
            playingField = room2Field;
        }

        //ERstellt Raum 3 also das Easter Egg
        private static void LoadRoom3(int width, int height)
        {
            // Leere Bildschirm und zeige den Easter-Egg-ASCII-Art
            Console.Clear();
            string[] vulcanHand = new string[]
    {
        "⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
        "⠀⠀⠀⠀⠀⠀⠀⠀⣾⣿⣿⡀⠀⠀⠀⠀⢠⣶⣶⡄⠀⠀⠀",
        "⠀⠀⠀⠀⠀⣀⣀⠀⢹⣿⣿⡇⠀⠀⠀⠀⣾⣿⣿⠃⠀⠀⠀",
        "⠀⠀⠀⠀⢸⣿⣿⣇⠈⣿⣿⣧⠀⠀⠀⢠⣿⣿⡏⠀⣰⣶⡄",
        "⠀⠀⠀⠀⠘⣿⣿⣿⠀⢹⣿⣿⡀⠀⠀⣾⣿⣿⠃⢰⣿⣿⡇",
        "⠀⠀⠀⠀⠀⢹⣿⣿⡆⠘⣿⣿⡇⠀⢠⣿⣿⡏⠀⣾⣿⣿⠁",
        "⠀⠀⠀⠀⠀⠈⣿⣿⣿⠀⢹⣿⣧⣀⣾⣿⣿⣇⣸⣿⣿⣿⠀",
        "⠀⠀⠀⠀⠀⠀⢻⣿⣿⣷⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⠀",
        "⣴⣿⣿⣷⣤⡀⠈⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠀⠀",
        "⠻⣿⣿⣿⣿⣿⣷⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠀⠀",
        "⠀⠈⠙⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⠀⠀",
        "⠀⠀⠀⠀⠙⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⠀⠀⠀",
        "⠀⠀⠀⠀⠀⠀⠉⠛⠿⠿⠿⠿⠿⠿⠿⠿⠟⠋⠀⠀⠀⠀",
        "Live Long and Prosper!"
    };

            Console.Clear();
            Console.OutputEncoding = System.Text.Encoding.UTF8; // UTF-8 für Sonderzeichen
            foreach (string line in vulcanHand)
            {
                Console.WriteLine(line);
            }
            EasterEggBeep();
            easterEggFound = true;
            //Console.WriteLine("\nDrücke Enter, um zum Hub zurückzukehren...");
            Console.WriteLine();
            EnterText();
            playingField = hubField;
            // Kehre in den Hub zurück (der Spieler erscheint dann wieder neben der entsprechenden Tür)
            LoadHub(width, height, "fromRoom3");
        }


        //Setzt den Schlüssel auf eine zufällige Position und überprüft, dass der Schlüssel nicht auf dem Spieler spawnt
        private static void PlaceKey()
        {
            
            if (playingField == null)
            {
                throw new InvalidOperationException("Das Spielfeld (playingField) wurde nicht initialisiert.");
            }

            keyPosition = new Position();

            keyPosition.Row = GenerateRandomNumber(1, playingField.GetLength(0) - 2);
            keyPosition.Column = GenerateRandomNumber(1, playingField.GetLength(1) - 2);

            while (keyPosition.Row == CurrentPosition.Row || keyPosition.Column == CurrentPosition.Column)
            {
                keyPosition.Row = GenerateRandomNumber(1, playingField.GetLength(0) - 2);
                keyPosition.Column = GenerateRandomNumber(1, playingField.GetLength(1) - 2);
            }
            playingField[keyPosition.Row, keyPosition.Column] = (char)Symbols.Key;

            placedKey = true; // Schlüssel ist platziert
            Console.WriteLine($"Der Schlüssel wurde an Position ({keyPosition.Row}, {keyPosition.Column}) platziert.");
        }

        //erstellt ein Objekt, das auf dem Spielfeld platziert werden soll
        private static void PlaceObjectSimple()
        {
            // Objektgröße: 2 Zeilen x 3 Spalten
            int objectHeight = 2;
            int objectWidth = 3;
            bool placed = false;

            // Wir wollen den Rand plus einen Puffer (z.B. 1 weitere Zelle) vermeiden:
            int minRow = 2; // mind. 2 Zellen Abstand zum oberen Rand
            int maxRow = playingField.GetLength(0) - objectHeight - 2; // Abstand zum unteren Rand
            int minCol = 2; // mind. 2 Zellen Abstand zur linken Wand
            int maxCol = playingField.GetLength(1) - objectWidth - 2;  // Abstand zur rechten Wand

            // Versuche maximal 50 Mal, einen passenden Platz zu finden
            for (int attempt = 0; attempt < 50; attempt++)
            {
                int startRow = GenerateRandomNumber(minRow, maxRow + 1);
                int startCol = GenerateRandomNumber(minCol, maxCol + 1);

                bool canPlace = true;
                // Überprüfe jede Zelle, die vom Objekt belegt werden soll
                for (int r = 0; r < objectHeight; r++)
                {
                    for (int c = 0; c < objectWidth; c++)
                    {
                        int posRow = startRow + r;
                        int posCol = startCol + c;
                        char cell = (char)playingField[posRow, posCol];

                        // Ist die Zelle nicht frei (also Symbols.None), brechen wir ab
                        if (cell != (char)Symbols.None)
                        {
                            canPlace = false;
                            break;
                        }
                        // Falls die Zelle der Tür entspricht, nicht platzieren!
                        // Annahme: Die Tür ist durch Symbols.Door oder Symbols.VerticalDoor definiert.
                        if (cell == (char)Symbols.Door || cell == (char)Symbols.VerticalDoor)
                        {
                            canPlace = false;
                            break;
                        }
                    }
                    if (!canPlace)
                        break;
                }

                if (canPlace)
                {
                    // Platziere das Objekt mit dem Zeichen für Box (█)
                    for (int r = 0; r < objectHeight; r++)
                    {
                        for (int c = 0; c < objectWidth; c++)
                        {
                            playingField[startRow + r, startCol + c] = (char)Symbols.Box;
                        }
                    }
                    placed = true;
                    break;
                }
            }

            // Falls nach 50 Versuchen kein Platz gefunden wurde, versuche es erneut.
            if (!placed)
            {
                PlaceObjectSimple();
            }
        }

        // Liest die Benutzereingabe (WASD bzw. Pfeiltasten) und aktualisiert die Spielerposition
        private static void CheckInput()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            char input = keyInfo.KeyChar;

            // Wandlung von Pfeiltasten in WASD
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow: input = 'w'; break;
                case ConsoleKey.LeftArrow: input = 'a'; break;
                case ConsoleKey.DownArrow: input = 's'; break;
                case ConsoleKey.RightArrow: input = 'd'; break;
                case ConsoleKey.Escape:
                    GameIsRunning = false;
                    return;
            }

            // Neue Position berechnen
            Position newPos = new Position { Row = CurrentPosition.Row, Column = CurrentPosition.Column };
            switch (input)
            {
                case 'w': newPos.Row--; break;
                case 'a': newPos.Column--; break;
                case 's': newPos.Row++; break;
                case 'd': newPos.Column++; break;
                case 'e': // Schlüssel einsammeln
                    if (CurrentPosition.Row == keyPosition.Row && CurrentPosition.Column == keyPosition.Column)
                    {
                        gotKey = true;
                        playingField[keyPosition.Row, keyPosition.Column] = (char)Symbols.None;
                        KeyBeep();
                    }
                    return;
                default: return;
            }

            // Zelle prüfen
            char target = (char)playingField[newPos.Row, newPos.Column];
            bool isWalkable = target == (char)Symbols.None || target == (char)Symbols.Key || 
                              target == (char)Symbols.OpenDoorSide ||
                              target == (char)Symbols.OpenDoorBottom ||
                              target == (char)Symbols.OpenDoorTop ||
                              target == (char)Symbols.SecretRoom;

            // Ausgangstür nur, wenn man den Schlüssel hat
            if (target == (char)Symbols.VerticalDoor && gotKey)
                isWalkable = true;


            if (isWalkable)
                CurrentPosition = newPos;
        }


        // Zeichnet einen Cursor oder ein Feld: Gibt drei Zeichen hintereinander aus.
        private static void DrawCursor(char LeftChar, char Symbol, char Rightchar)
        {
            Console.Write(LeftChar);
            Console.Write(Symbol);
            Console.Write(Rightchar);
        }

        // Methode zur Auswahl des Spielercharakters mittels Cursor (Auswahl zwischen "O" und "X")
        private static char SelectCharacter()
        {
            string[] options = { "O", "X" };  // Die Auswahlmöglichkeiten
            int selectedIndex = 0;            // Startet bei Option 0 ("O")
            ConsoleKeyInfo keyInfo;

            do
            {
                Console.Clear();
                Console.WriteLine("Wähle deinen Spielercharakter mithilfe der Pfeiltasten aus: \n");

                // Ausgabe aller Optionen nebeneinander
                for (int i = 0; i < options.Length; i++)
                {
                    // Markiere die aktuell ausgewählte Option mit eckigen Klammern
                    if (i == selectedIndex)
                        DrawCursor('[', options[i][0], ']');
                    else
                        DrawCursor(' ', options[i][0], ' ');

                    // Füge einen Abstand zwischen den Optionen ein
                    Console.Write("   ");
                }
                Console.WriteLine();

                // Lese die Eingabe des Benutzers
                Console.WriteLine("\nBestätige deine Eingabe mit 'Enter'.");

                keyInfo = Console.ReadKey(true);

                // Bei linker Pfeiltaste wird der Index um 1 verringert (wenn er > 0 ist)
                if (keyInfo.Key == ConsoleKey.LeftArrow && selectedIndex > 0)
                    selectedIndex--; // Auswahl zurücksetzen auf "O" (Option 0)

                // Bei rechter Pfeiltaste wird der Index um 1 erhöht (wenn er < max index ist)
                else if (keyInfo.Key == ConsoleKey.RightArrow && selectedIndex < options.Length - 1)
                    selectedIndex++; // Auswahl auf "X" (Option 1) setzen

            } while (keyInfo.Key != ConsoleKey.Enter); // Schleife bis Enter gedrückt wird

            // Gibt den ausgewählten Spielercharakter zurück
            return options[selectedIndex][0];  // Rückgabe des ersten Zeichens ("O" oder "X")
        }

        private static int WallSelect(int min = 0, int max = 4)
        {
           // Entscheide zufällig, ob die Tür an einer Seiten- oder oberen/unteren Wand spawnt (4 wird bei rnd. nicht berücksichtig)
           wallSide = rnd.Next(0, 4); // 0 = links, 1 = rechts, 2 = oben, 3 = unten
            return (wallSide);
        }

        
        private static Position placeDoor(int wallSide, int _width, int _height)
        {
            Position doorType = new Position();
            bool doorPlaced = false;
            do
            {
                if (wallSide == 0) // Linke Wand
                {
                    doorType.Row = GenerateRandomNumber(1, playingField.GetLength(0) - 2);
                    doorType.Column = 0;
                }
                else if (wallSide == 1) // Rechte Wand
                {
                    doorType.Row = GenerateRandomNumber(1, playingField.GetLength(0) - 2);
                    doorType.Column = playingField.GetLength(1) - 1;
                }
                else if (wallSide == 2) // Obere Wand
                {
                    doorType.Row = 0;
                    doorType.Column = GenerateRandomNumber(1, playingField.GetLength(1) - 2);
                }
                else // Untere Wand
                {
                    doorType.Row = playingField.GetLength(0) - 1;
                    doorType.Column = GenerateRandomNumber(1, playingField.GetLength(1) - 2);
                }

                // Stelle sicher, dass die Tür nicht auf etwas anderem als einer Wand spawnt
                if (playingField[doorType.Row, doorType.Column] == (char)Symbols.Wall)
                {
                    doorPlaced = true;
                }
                else if (playingField[doorType.Row, doorType.Column] == (char)Symbols.WallHorizontal)
                {
                    doorPlaced = true;
                }

            } while (!doorPlaced);
            
            return (doorType);
        }

        static void placeHubDoor(int width, int height)
        {
            // Tür zum Ausgang (unten in der Mitte) 
            hubExitDoor = new Position();
            int exitWall = WallSelect();
            hubExitDoor = placeDoor(exitWall, width, height);
            hubDoorWallSide = exitWall;


            if (gotKey && (exitWall == 0 || exitWall == 1))
            {
                hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.VerticalDoor;
            }
            else if (gotKey && exitWall == 2)
            {
                hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.OpenDoorTop;
            }
            else if (gotKey && exitWall == 3)
            {
                hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.OpenDoorBottom;
            }
            else if (!gotKey && (exitWall == 0 || exitWall == 1))
            {
                hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.VerticalDoor;
            }
            else if (!gotKey && (exitWall == 2 || exitWall == 3))
            {
                hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.VerticalDoor;
            }
            hubDoorPlaced = true; // Tür ist platziert
        }

        static void DrawHubDoor()
        {
            if (hubDoorWallSide == 0 || hubDoorWallSide == 1) // links oder rechts
            {
                if (gotKey)
                {
                    hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.OpenDoorSide;
                }
                else
                {
                    hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.Door; 
                }
            }
            else if (hubDoorWallSide == 2) // oben
            {
                if (gotKey)
                {
                    hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.OpenDoorTop;
                }
                else
                {
                    hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.VerticalDoor;
                }
            }
            else if (hubDoorWallSide == 3) // unten
            {
                if (gotKey)
                {
                    hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.OpenDoorBottom;
                }
                else
                {
                    hubField[hubExitDoor.Row, hubExitDoor.Column] = (char)Symbols.VerticalDoor;
                }
            }
        }

        static void placeRoom2Door(int width, int height)
        {
            //  Tür zum Kanalisationsraum (Raum 2)
            hubDoorToRoom2 = new Position();
            int room2Wall = WallSelect();
            hubDoorToRoom2 = placeDoor(room2Wall, width, height);
            room2DoorWallSide = room2Wall;

            if (room2Wall == 0 || room2Wall == 1)
            {
                hubField[hubDoorToRoom2.Row, hubDoorToRoom2.Column] = (char)Symbols.OpenDoorSide;
            }
            else if (room2Wall == 2)
            {
                hubField[hubDoorToRoom2.Row, hubDoorToRoom2.Column] = (char)Symbols.OpenDoorTop;
            }
            else
            {
                hubField[hubDoorToRoom2.Row, hubDoorToRoom2.Column] = (char)Symbols.OpenDoorBottom;
            }
            room2DoorPlaced = true; // Tür ist platziert
        }

        static void DrawRoom2Door()
        {
            if (room2DoorWallSide == 0 || room2DoorWallSide == 1)
            {
                hubField[hubDoorToRoom2.Row, hubDoorToRoom2.Column] = (char)Symbols.OpenDoorSide;
            }
            else if (room2DoorWallSide == 2)
            {
                hubField[hubDoorToRoom2.Row, hubDoorToRoom2.Column] = (char)Symbols.OpenDoorTop;
            }
            else
            {
                hubField[hubDoorToRoom2.Row, hubDoorToRoom2.Column] = (char)Symbols.OpenDoorBottom;
            }
        }

        static void placeRoom3Door(int width, int height)
        {
            //  Tür zum Easter-Egg-Raum (Raum 3 – rechte Wand)
            hubDoorToRoom3 = new Position();
            int room3Wall = rnd.Next(2,4);
            hubDoorToRoom3 = placeDoor(room3Wall, width, height);
            
            hubField[hubDoorToRoom3.Row, hubDoorToRoom3.Column] = (char)Symbols.SecretRoom;
            room3DoorPlaced = true; // Tür ist platziert
        }

        static void DrawRoom3Door()
        {
            hubField[hubDoorToRoom3.Row, hubDoorToRoom3.Column] = (char)Symbols.SecretRoom;
        }

        static void KeyBeep()
        {
            Console.Beep(523, 200);  // C5
            Console.Beep(587, 200);  // D5
            Console.Beep(659, 200);  // E5
            Console.Beep(523, 200);  // C5
        }

        // Melodie von https://gist.github.com/ataylor32/24f429e147d8b2a758d7
        static void VictoryBeep()
        {
            Console.Beep(130, 100);
            Console.Beep(262, 100);
            Console.Beep(330, 100);
            Console.Beep(392, 100);
            Console.Beep(523, 100);
            Console.Beep(660, 100);
            Console.Beep(784, 300);
            Console.Beep(660, 300);
            Console.Beep(146, 100);
            Console.Beep(262, 100);
            Console.Beep(311, 100);
            Console.Beep(415, 100);
            Console.Beep(523, 100);
            Console.Beep(622, 100);
            Console.Beep(831, 300);
            Console.Beep(622, 300);
            Console.Beep(155, 100);
            Console.Beep(294, 100);
            Console.Beep(349, 100);
            Console.Beep(466, 100);
            Console.Beep(588, 100);
            Console.Beep(699, 100);
            Console.Beep(933, 300);
            Console.Beep(933, 100);
            Console.Beep(933, 100);
            Console.Beep(933, 100);
            Console.Beep(1047, 400);
        }

        static void EasterEggBeep()
        {

            Console.Beep(659, 300); // E5
            Console.Beep(784, 300); // G5
            Console.Beep(880, 300); // A5
            Console.Beep(659, 300); // E5
            Console.Beep(784, 300); // G5
            Console.Beep(880, 300); // A5
            Console.Beep(988, 300); // B5
            Console.Beep(1047, 600); // C6
        }

    }

}
