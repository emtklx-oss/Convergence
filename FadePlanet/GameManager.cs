using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FadePlanet
{
    

    public static class GameManager
    {
        public static Player CurPlayer { get; private set; }
        public static void SetPlayer(Player plr) { CurPlayer = plr;  }
        #region Object Management

        private static readonly Dictionary<ObjectType, Dictionary<int, WorldObject>> RoomObjects = new Dictionary<ObjectType, Dictionary<int, WorldObject>>();
        public static IReadOnlyDictionary<ObjectType, Dictionary<int, WorldObject>> AllObjects => RoomObjects;
        public static int _idCounter = 0;

        public static void SpawnObject(WorldObject obj)
        {
            obj.Id = _idCounter++;

            if (!RoomObjects.ContainsKey(obj.Type))
            {
                RoomObjects[obj.Type] = new Dictionary<int, WorldObject>();
            }

            RoomObjects[obj.Type].Add(obj.Id, obj);
        }

        public static void DespawnObject(WorldObject obj)
        {
            if (RoomObjects.TryGetValue(obj.Type, out var categoryDict))
            {
                categoryDict.Remove(obj.Id);
            }
        }

        //Helper for spawning enemies into rooms
        private static Enemy SpawnEnemy(EnemyType type, Point pos)
        {
            return new Enemy(pos, new Size((int)(32 * 3.0f), (int)(32 * 3.0f)), type);
        }
        #endregion

        #region Object Updates & Rendering


        // Retrieves all objects of a specific type from GameManager.
        // Returns a list of objects for direct modification or querying.
        public static List<WorldObject> GetObjectsByType(ObjectType type)
        {
            if (RoomObjects.TryGetValue(type, out var objectDict))
            {
                return objectDict.Values.ToList();
            }
            return new List<WorldObject>();
        }

        //Updates all objects of a specific type from GameManager with the provided action.
        public static void UpdateObjectType(ObjectType type, Action<WorldObject> updateAction)
        {
            if (RoomObjects.TryGetValue(type, out var objectDict))
            {
                foreach (WorldObject obj in objectDict.Values.ToList())
                {
                    updateAction(obj);
                }
            }
        }


        // Draws all objects of a specific type from GameManager.
        // Optionally filters objects using a predicate.
        public static void DrawObjectType(Graphics g, ObjectType type, Func<WorldObject, bool> filter = null)
        {
            if (RoomObjects.TryGetValue(type, out var objectDict))
            {
                foreach (WorldObject obj in objectDict.Values)
                {
                    if (filter == null || filter(obj))
                    {
                        obj.Draw(g);
                    }
                }
            }
        }

        #endregion

        #region Rooms
        public static void LoadRoom_One()
        {
            //Load background, sound, etc.

            //Objects
            SetPlayer( new Player(new Point(528, 250), new Size(224, 224)) );

            new OldMan(new Point(800, 250), new Size(250, 250));


        }
        public static void LoadRoom_Two()
        {
            //Air
        }
        public static void LoadRoom_Three()
        {
            //Water
        }
        public static void LoadRoom_Four()
        {
            //Earth
        }
        public static void LoadRoom_Five()
        {
            //Fire
        }
        public static void LoadRoom_Six()
        {
            //Boss
        }
        #endregion
    }
}