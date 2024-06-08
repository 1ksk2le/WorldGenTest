using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WorldGenTest.Content
{
    public class World
    {
        public int tileSize { get; } = 32;
        public int sizeX { get; } = 600;
        public int sizeY { get; } = 600;
        public Tile[,] tiles { get; private set; }

        public World()
        {
            tiles = new Tile[(int)sizeX, (int)sizeY];
            GenerateWorldGrid();
        }

        private void GenerateWorldGrid()
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    var rectangle = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                    tiles[x, y] = Tile.Type[0].Clone(rectangle);
                }
            }
        }
        public void GenerateDungeon(int roomCount, int roomMinSize, int roomMaxSize, Vector2 startPos, int spacing, int corridorThickness)
        {
            Random rand = new Random();
            List<Room> rooms = new List<Room>();

            int gridSizeX = (int)Math.Sqrt(roomCount) + 1;
            int gridSizeY = (int)Math.Sqrt(roomCount) + 1;

            int roomIndex = 0;

            Room startRoom = new Room(10, 10, 30, 30, false);
            rooms.Add(startRoom);

            for (int x = 0; x < gridSizeX && roomIndex < roomCount; x++)
            {
                for (int y = 0; y < gridSizeY && roomIndex < roomCount; y++)
                {
                    int roomWidth = rand.Next(roomMinSize, roomMaxSize);
                    int roomHeight = roomWidth;
                    int roomX = (int)startPos.X + x * (roomMaxSize + spacing);
                    int roomY = (int)startPos.Y + y * (roomMaxSize + spacing);

                    Room newRoom = new Room(roomX, roomY, roomWidth, roomHeight, false);
                    bool intersects = rooms.Any(r => r.Intersects(newRoom));

                    if (!intersects)
                    {
                        rooms.Add(newRoom);
                        roomIndex++;
                    }
                }
            }

            Room finalRoom = new Room(10, 10 + (int)Math.Sqrt(roomCount) * (roomMaxSize + spacing), 30, 30, false);
            rooms.Add(finalRoom);

            LinkRoomsWithMST(rooms, corridorThickness);

            foreach (Room room in rooms)
            {
                CreateRoom(room);
            }

        }

        private void LinkRoomsWithMST(List<Room> rooms, int corridorThickness)
        {
            Random rand = new Random();
            List<Tuple<Room, Room>> potentialCorridors = new List<Tuple<Room, Room>>();
            HashSet<Room> connectedRooms = new HashSet<Room> { rooms[0] };
            Queue<Room> roomQueue = new Queue<Room>();
            roomQueue.Enqueue(rooms[0]);

            while (connectedRooms.Count < rooms.Count)
            {
                if (roomQueue.Count == 0)
                {
                    Room closestRoomA = null;
                    Room closestRoomB = null;
                    int closestDistance = int.MaxValue;

                    foreach (var connectedRoom in connectedRooms)
                    {
                        foreach (var room in rooms)
                        {
                            if (!connectedRooms.Contains(room))
                            {
                                int distance = GetDistance(connectedRoom, room);
                                if (distance < closestDistance)
                                {
                                    closestRoomA = connectedRoom;
                                    closestRoomB = room;
                                    closestDistance = distance;
                                }
                            }
                        }
                    }

                    if (closestRoomA != null && closestRoomB != null)
                    {
                        CreateStraightCorridor(closestRoomA, closestRoomB, corridorThickness);
                        connectedRooms.Add(closestRoomB);
                        roomQueue.Enqueue(closestRoomB);
                    }
                }

                while (roomQueue.Count > 0)
                {
                    Room currentRoom = roomQueue.Dequeue();

                    var adjacentRooms = rooms
                        .Where(r => !connectedRooms.Contains(r) && IsAdjacent(currentRoom, r))
                        .OrderBy(r => GetDistance(currentRoom, r))
                        .ToList();

                    if (adjacentRooms.Count > 0)
                    {
                        Room nextRoom = adjacentRooms.First();
                        CreateStraightCorridor(currentRoom, nextRoom, corridorThickness);
                        connectedRooms.Add(nextRoom);
                        roomQueue.Enqueue(nextRoom);
                    }
                }
            }

            foreach (var room in rooms)
            {
                var adjacentRooms = rooms.Where(r => r != room && IsAdjacent(room, r)).ToList();
                foreach (var adjRoom in adjacentRooms)
                {
                    if (rand.NextDouble() > 0.1)
                    {
                        CreateStraightCorridor(room, adjRoom, corridorThickness);
                    }
                }
            }
        }

        private int GetDistance(Room roomA, Room roomB)
        {
            int dx = (roomA.X + roomA.Width / 2) - (roomB.X + roomB.Width / 2);
            int dy = (roomA.Y + roomA.Height / 2) - (roomB.Y + roomB.Height / 2);
            return dx * dx + dy * dy; // Squared distance
        }

        private void CreateStraightCorridor(Room roomA, Room roomB, int corridorThickness)
        {
            int xStart = roomA.X + roomA.Width / 2;
            int xEnd = roomB.X + roomB.Width / 2;
            int yStart = roomA.Y + roomA.Height / 2;
            int yEnd = roomB.Y + roomB.Height / 2;

            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++)
            {
                for (int i = -corridorThickness / 2; i <= corridorThickness / 2; i++)
                {
                    SetTileID(x, yStart + i, 1);
                }
            }

            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++)
            {
                for (int i = -corridorThickness / 2; i <= corridorThickness / 2; i++)
                {
                    SetTileID(xEnd + i, y, 1);
                }
            }
        }

        private bool IsAdjacent(Room roomA, Room roomB)
        {
            int deltaX = Math.Abs(roomA.X - roomB.X);
            int deltaY = Math.Abs(roomA.Y - roomB.Y);

            return (deltaX <= roomA.Width + 1 && deltaY == 0) || (deltaY <= roomA.Height + 1 && deltaX == 0);
        }


        private void CreateRoom(Room room)
        {
            if (!room.IsBlank)
            {
                for (int x = room.X; x < room.Width + room.X; x++)
                {
                    for (int y = room.Y; y < room.Height + room.Y; y++)
                    {
                        SetTileID(x, y, 3);
                    }
                }
            }

        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, Dictionary<int, Texture2D> tileTextures)
        {
            int startX = (int)MathHelper.Clamp(camera.position.X / tileSize, 0, sizeX - 1);
            int startY = (int)MathHelper.Clamp(camera.position.Y / tileSize, 0, sizeY - 1);

            int endX = (int)MathHelper.Clamp((camera.position.X + spriteBatch.GraphicsDevice.Viewport.Width / camera.zoom) / tileSize + 1, 0, sizeX);
            int endY = (int)MathHelper.Clamp((camera.position.Y + spriteBatch.GraphicsDevice.Viewport.Height / camera.zoom) / tileSize + 1, 0, sizeY);

            Vector2 mouseWorldPosition = InputManager.Instance.mousePosition / camera.zoom + camera.position;
            Point mouseTile = new Point((int)(mouseWorldPosition.X / tileSize), (int)(mouseWorldPosition.Y / tileSize));

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    Texture2D texture = tileTextures[tiles[x, y].ID];
                    tiles[x, y].Draw(spriteBatch, texture);

                    if (x == mouseTile.X && y == mouseTile.Y)
                    {
                        spriteBatch.DrawRectangle(tiles[x, y].rectangle, Color.White * 0.2f, 0.99f);
                    }
                }
            }
        }

        public int GetTileID(int x, int y)
        {
            if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
            {
                return tiles[x, y].ID;
            }
            return 0;
        }

        public void SetTileID(int x, int y, int tileID)
        {
            if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
            {
                tiles[x, y].ID = tileID;
            }
        }

        public Tile GetTile(int x, int y)
        {
            if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
            {
                return tiles[x, y];
            }
            return null;
        }

        public void SetTile(int x, int y, Tile tile)
        {
            if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
            {
                tiles[x, y] = tile.Clone(new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize));
            }
        }

        public void SaveToFile(string fileName)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folderPath = Path.Combine(appDataPath, "WhirlingRealms");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int y = 0; y < sizeY; y++)
                {
                    int currentTileID = tiles[0, y].ID;
                    int count = 1;

                    for (int x = 1; x < sizeX; x++)
                    {
                        if (tiles[x, y].ID == currentTileID)
                        {
                            count++;
                        }
                        else
                        {
                            writer.Write($"{currentTileID}:{count} ");
                            currentTileID = tiles[x, y].ID;
                            count = 1;
                        }
                    }

                    writer.Write($"{currentTileID}:{count} ");
                    writer.WriteLine();
                }
            }
        }

        public void LoadFromFile(string fileName)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folderPath = Path.Combine(appDataPath, "WhirlingRealms");
            string filePath = Path.Combine(folderPath, fileName);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File {fileName} does not exist.");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);

                for (int y = 0; y < lines.Length; y++)
                {
                    string line = lines[y];
                    string[] tileData = line.Split(' ');

                    int x = 0;
                    foreach (string data in tileData)
                    {
                        string[] parts = data.Split(':');
                        if (parts.Length != 2)
                        {
                            Console.WriteLine($"Invalid format in line {y + 1}: {line}");
                            continue;
                        }

                        if (!int.TryParse(parts[0], out int tileID) || !int.TryParse(parts[1], out int count))
                        {
                            Console.WriteLine($"Invalid integer format in line {y + 1}: {line}");
                            continue;
                        }

                        for (int i = 0; i < count; i++)
                        {
                            if (x < sizeX && y < sizeY)
                            {
                                var rectangle = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                                tiles[x, y] = Tile.Type[tileID].Clone(rectangle);
                                x++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while loading the file: {ex.Message}");
            }
        }
    }

    public class Room
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsBlank { get; set; }

        public Room(int x, int y, int width, int height, bool isBlank)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsBlank = isBlank;
        }

        public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

        public bool Intersects(Room other)
        {
            return Bounds.Intersects(other.Bounds);
        }
    }
}
