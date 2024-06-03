using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using WorldGenTest.Content;

namespace WorldGenTest
{
    public class GameConsole
    {
        private string command;
        private List<string> commandHistory;
        private int currentCommandIndex;
        public bool isVisible;
        private World world;
        private SpriteFont font;

        public GameConsole(World world, SpriteFont font)
        {
            this.world = world;
            this.font = font;

            command = "";
            isVisible = false;
            commandHistory = new List<string>();
            currentCommandIndex = -1;
        }

        public void Update(InputManager inputManager)
        {
            if (inputManager.IsKeySinglePress(Keys.Tab))
            {
                command = "";
                isVisible = !isVisible;
            }
            if (isVisible)
            {
                HandleConsoleCommands(inputManager);
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            if (isVisible)
            {
                int consoleHeight = 55 + commandHistory.Count * 15;
                spriteBatch.DrawRectangle(new Rectangle(0, 0, graphics.PreferredBackBufferWidth, consoleHeight), new Color(0, 0, 0, 125), 1f);
                spriteBatch.DrawString(font, "[Console Prompt]", new Vector2(10, 15), Color.Red);

                for (int i = 0; i < commandHistory.Count; i++)
                {
                    Color commandColor = Color.SpringGreen;
                    if (commandHistory[i].StartsWith("[!]"))
                    {
                        commandColor = Color.Red;
                    }
                    if (commandHistory[i].StartsWith("[?]"))
                    {
                        commandColor = Color.LightSkyBlue;
                    }
                    if (commandHistory[i].StartsWith(" "))
                    {
                        commandColor = Color.LightGray;
                    }
                    spriteBatch.DrawString(font, commandHistory[i], new Vector2(10, 30 + (i + 1) * 15), commandColor);
                }

                spriteBatch.DrawString(font, ">> " + command, new Vector2(10, 30), Color.Yellow);
            }
        }

        private void HandleConsoleCommands(InputManager inputManager)
        {
            if (inputManager.IsKeySinglePress(Keys.Enter))
            {
                if (!string.IsNullOrWhiteSpace(command))
                {
                    ExecuteCommand(command);
                    commandHistory.Insert(0, command);
                }
                command = "";
                currentCommandIndex = -1;
            }
            if (inputManager.IsKeySinglePress(Keys.Back))
            {
                if (command.Length > 0)
                {
                    command = command.Remove(command.Length - 1, 1);
                }
            }
            else
            {
                command += inputManager.GetPressedKeys();
            }

            if (inputManager.IsKeySinglePress(Keys.Down))
            {
                if (currentCommandIndex < commandHistory.Count - 1)
                {
                    currentCommandIndex++;
                    if (!command.StartsWith(" ") && !command.StartsWith("[?]") && !command.StartsWith("[!]"))
                    {
                        command = commandHistory[currentCommandIndex];
                    }
                }
            }
            else if (inputManager.IsKeySinglePress(Keys.Up))
            {
                if (currentCommandIndex >= 0)
                {
                    currentCommandIndex--;
                    if (currentCommandIndex >= 0)
                    {
                        if (!command.StartsWith(" ") && !command.StartsWith("[?]") && !command.StartsWith("[!]"))
                        {
                            command = commandHistory[currentCommandIndex];
                        }
                    }
                    else
                    {
                        command = "";
                    }
                }
            }
        }

        private void ExecuteCommand(string command)
        {
            if (command == "HELP")
            {
                commandHistory.Insert(0, "[?] TILE [ID] - Changes tile brush");
                commandHistory.Insert(0, "[?] DEVMODE - Turns developer mode on or off");
                commandHistory.Insert(0, "[?] CLEARCHAT - Clears console prompt");
                commandHistory.Insert(0, "[?] CLEARWORLD - Clears sandbox");
                commandHistory.Insert(0, "[?] TILELIST - Shows off the list of all tiles in game");
            }
            else if (command == "CLEARCHAT")
            {
                commandHistory.Clear();
                commandHistory.Insert(0, " Console prompt cleared");
            }
            else if (command == "CLEARWORLD")
            {
                for (int i = 0; i < world.size; i++)
                {
                    for (int j = 0; j < world.size; j++)
                    {
                        world.SetTileID(i, j, 1);
                    }
                }
                commandHistory.Insert(0, " Sandbox cleared");
            }
            else if (command == "TILELIST")
            {
                for (int i = Tile.Type.Count - 1; i >= 0; i--)
                {
                    commandHistory.Insert(0, " ID: " + i + " --- " + Tile.GetTileName(i));
                }
                commandHistory.Insert(0, " We have " + Tile.Type.Count.ToString() + " tiles in total");
            }
            else if (command == "DEVMODE")
            {
                if (Main.devMode)
                {
                    commandHistory.Insert(0, " Turned developer mode off");
                    Main.devMode = false;
                }
                else
                {
                    commandHistory.Insert(0, " Turned developer mode on");
                    Main.devMode = true;
                }
            }
            else if (command.StartsWith("TILE"))
            {
                string[] commandParts = command.Split(' ');

                if (commandParts.Length >= 2)
                {
                    if (int.TryParse(commandParts[1], out int tileID))
                    {
                        if (tileID < 4)
                        {
                            Main.chosenTile = tileID;
                            commandHistory.Insert(0, " Changed tile brush to " + Tile.GetTileName(tileID));
                        }
                        else
                        {
                            commandHistory.Insert(0, "[!] Invalid tile ID");
                        }
                    }
                }
            }
            else
            {
                commandHistory.Insert(0, "[!] Unknown command");
            }
        }
    }
}
