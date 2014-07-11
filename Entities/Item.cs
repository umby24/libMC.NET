using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CWrapped;
using libMC.NET.Network;

namespace libMC.NET.Entities {
    public class Item {
        public int itemID;
        public byte itemCount;
        public short itemDamage;
        public byte[] nbtData;

        public string FriendlyName() {
            // -- Return the friendly name for the item we represent

            return ((Block.blockitemid)itemID).ToString();
        }

        public static Item ItemFromSlot(SlotData Item) {
            var newItem = new Item();
            newItem.itemID = Item.ID;
            newItem.itemCount = Item.ItemCount;
            newItem.itemDamage = Item.ItemDamage;
            newItem.nbtData = Item.NbtData;

            return newItem;
        }
    }
}
