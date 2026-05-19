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
        public static void LoadRoom()
        {
            CurPlayer = new Player(new Point(528, 248), new Size(224, 224));
        }
        public static Player CurPlayer { get; private set; }
        public static void SetPlayer(Player plr) { CurPlayer = plr;  }
        #region Object Management

        private static readonly Dictionary<ObjectType, Dictionary<int, WorldObject>> RoomObjects = new Dictionary<ObjectType, Dictionary<int, WorldObject>>();
        public static IReadOnlyDictionary<ObjectType, Dictionary<int, WorldObject>> AllObjects => RoomObjects;
        public static int _idCounter = 0;

        public static bool TryMove(WorldObject obj, int dX, int dY)
        {
            PointF targetPosition = new PointF(obj.Position.X + dX, obj.Position.Y + dY);
            RectangleF targetBounds = new RectangleF(targetPosition, obj.ObjSize);

            ObjectType[] solidTypes = { ObjectType.Wall, ObjectType.Friendly, ObjectType.Enemy };

            for (int i = 0; i < solidTypes.Length; i++)
            {
                ObjectType type = solidTypes[i];

                if (!RoomObjects.TryGetValue(type, out var categoryDict)) continue;

                var objects = categoryDict.Values;

                foreach (var otherObj in objects)
                {
                    if (otherObj.Id == obj.Id) continue;

                    if (Math.Abs(otherObj.Position.X - targetPosition.X) > 100 ||
                        Math.Abs(otherObj.Position.Y - targetPosition.Y) > 100)
                    {
                        continue;
                    }

                    if (targetBounds.IntersectsWith(otherObj.Hitbox))
                    {
                        if (otherObj.Type == ObjectType.Friendly || otherObj.Type == ObjectType.Enemy)
                        {
                            // Handle interactions here (e.g., combat, dialogue)
                        }

                        return false;
                    }
                }
            }

            obj.Position = targetPosition;
            return true;
        }

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
    }
}