namespace FadePlanet {
 public class InventoryComponent 
 {
   private const int hotbarLength = 4;
   public WorldObject[] Hotbar { get; private set; } = new WorldObject[hotbarLength];
   
   public InventoryComponent(int hotbarLength) {}
   
   public void AddObject(WorldObject obj) {
   
}
public void RemoveObject(WorldObject obj = null, int slotIndex = null) 
 {
    if (obj != null && slotIndex != null) {}
    if (obj) 
{//find obj in Hotbar }
    else if (slotIndex) 
{ //find object at index in Hotbar }

 }



 }
}