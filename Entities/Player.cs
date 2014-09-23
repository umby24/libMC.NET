using System.Collections.Generic;
using libMC.NET.Common;

namespace libMC.NET.Entities {
    /// <summary>
    /// This class holds all information for the minecraft client. This includes location, inventory, direction, and names.
    /// </summary>
    public class Player {
        public int EntityId; // -- The client's Entity ID on the server.
        public string PlayerName;
        public byte GameMode, Animation;
        public sbyte SelectedSlot;
        public float PlayerHealth, FoodSaturation, ExpBar, FlyingSpeed, WalkingSpeed;
        public short PlayerHunger, Level, TotalExp;
        public bool InBed, OnGround;

        #region Location Information
        public DoubleVector Location = new DoubleVector();
        public float[] Look = new float[2];
        #endregion

        public Dictionary<int, Item> Inventory = new Dictionary<int,Item>();

        public void SetInventory(Item newItem, short slotId) {
            //TODO: Maybe change this to Player.Inventory.SetItem(Item, SlotID)
            if (Inventory.ContainsKey(slotId)) {
                Inventory.Remove(slotId);
            }
            Inventory.Add(slotId, newItem);
        }
        //TODO: -- Make events for health updated, game mode updated, inventory updated, ect.

    }
}
