using libMC.NET.Network;

namespace libMC.NET.Entities {
    public class Item {
        public int ItemId;
        public byte ItemCount;
        public short ItemDamage;
        public byte[] NbtData;

        public string FriendlyName() {
            // -- Return the friendly name for the item we represent

            return ((Block.Blockitemid)ItemId).ToString();
        }

        public static Item ItemFromSlot(SlotData item) {
            var newItem = new Item
            {
                ItemId = item.Id,
                ItemCount = item.ItemCount,
                ItemDamage = item.ItemDamage,
                NbtData = item.NbtData
            };

            return newItem;
        }
    }
}
