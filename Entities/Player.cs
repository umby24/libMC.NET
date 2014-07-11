using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;

namespace libMC.NET.Entities {
    /// <summary>
    /// This class holds all information for the minecraft client. This includes location, inventory, direction, and names.
    /// </summary>
    public class Player {
        public int Entity_ID; // -- The client's Entity ID on the server.
        public string playerName;
        public byte gameMode, Animation;
        public sbyte selectedSlot;
        public float playerHealth, foodSaturation, ExpBar, flyingSpeed, WalkingSpeed;
        public short playerHunger, level, totalExp;
        public bool inBed, onGround;

        #region Location Information
        public DoubleVector location = new DoubleVector();
        public float[] look = new float[2];
        #endregion

        public Dictionary<int, Item> Inventory = new Dictionary<int,Item>();

        public void SetInventory(Item newItem, short slotID) {
            //TODO: Maybe change this to Player.Inventory.SetItem(Item, SlotID)
            if (Inventory.ContainsKey(slotID)) {
                Inventory.Remove(slotID);
            }
            Inventory.Add(slotID, newItem);
        }
        //TODO: -- Make events for health updated, game mode updated, inventory updated, ect.

    }
}
