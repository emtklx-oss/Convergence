using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadePlanet
{
    public static class GameManager
    {
        //Game controls the rooms

        //List of Object types, with a dictionary containing all objects of the Key type
        private static readonly Dictionary<ObjectType, Dictionary<int, WorldObject>> RoomObjects = new Dictionary<ObjectType, Dictionary<int, WorldObject>>();
        public static IReadOnlyDictionary<ObjectType, Dictionary<int, WorldObject>> AllObjects => RoomObjects;
        public static int _idCounter  = 0; // For assigning unique IDs to objects if needed in the future

        //Moving Objects. Used by enemies, if you want, you can use it for the player as well
        public static bool TryMove(WorldObject obj, int dX, int dY)
        {
            PointF targetPosition = new PointF(obj.Position.X + dX, obj.Position.Y + dY);
            RectangleF targetBounds = new RectangleF(targetPosition, obj.ObjSize);

            ObjectType[] solidTypes = { ObjectType.Wall, ObjectType.Friendly, ObjectType.Enemy }; // Types that block movement

            for (int i = 0; i < solidTypes.Length; i++)
            {
                ObjectType type = solidTypes[i]; // eg. we are checking only walls? this will = wall

                // If the category doesn't exist, skip it
                if (!RoomObjects.TryGetValue(type, out var categoryDict)) continue;

                var objects = categoryDict.Values; //get all objects of that type (eg. all walls)

                foreach (var otherObj in objects)
                {
                    if (otherObj.Id == obj.Id) continue;

                    // Broad-phase distance check: Skip collision math if too far away
                    if (Math.Abs(otherObj.Position.X - targetPosition.X) > 100 ||
                        Math.Abs(otherObj.Position.Y - targetPosition.Y) > 100)
                    {
                        continue; //Next object
                    }

                    // Narrow-phase precise collision check
                    if (targetBounds.IntersectsWith(otherObj.Bounds))
                    {
                        return false;
                    }
                }
            }

            // update the object's position
            obj.Position = targetPosition;
            return true;
        }
        //Spawning/adding object to room (for enemies, player, item, etc.)
        public static void SpawnObject(WorldObject obj)
        {
            obj.Id = _idCounter++;

            if (!RoomObjects.ContainsKey(obj.Type))
            {
                RoomObjects[obj.Type] = new Dictionary<int, WorldObject>();
            }

            // Add the object to its specific category using its ID as the key
            RoomObjects[obj.Type].Add(obj.Id, obj);
        }
        public static void DespawnObject(WorldObject obj)
        {
            // Ensure the category exists and contains the specific ID
            if (RoomObjects.TryGetValue(obj.Type, out var categoryDict))
            {
                categoryDict.Remove(obj.Id);
            }
        }
       
    }
}
