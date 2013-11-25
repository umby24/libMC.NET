using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Classes {
    /// <summary>
    /// Handles all Entities, including mobs and other players.
    /// </summary>
    public class Entity {
        public int Entity_ID, direction, Vehicle_ID, potion_color; // -- 0 = -z, 1 = -z, 2 = +z, 3 = +x
        public bool onFire, crouched, sprinting, eating, invisible, inBed, attached, leashed;
        public dynamic[] Metadata;
        public string Type, UUID, playerName;
        public Vector Location;
        public byte mobType, pitch, headPitch, yaw, status, amplifier, animation, rotation, ambient, arrows, nametag; // -- mobType is optional, only for mobs.
        public short Velocity_X, Velocity_Y, Velocity_Z, heldItem, count, duration, air;
        public Item itemFrame;
        public float health;

        public double jumpStrength, movementSpeed;

        public Dictionary<int, Item> Inventory;

        public Entity(ref Minecraft mc, string type) {
            Type = type;
            Inventory = new Dictionary<int, Item>();
        }

        public void readEntityMetadata(ref Minecraft mc) {

            do {

                byte item = mc.nh.wSock.readByte();

                if (item == 127) break;

                int index = item & 0x1F;
                int type = item >> 5;

                switch (type) {
                    case 0:
                        handleMetadata(index, mc.nh.wSock.readByte());
                        break;
                    case 1:
                        handleMetadata(index, mc.nh.wSock.readShort());
                        break;
                    case 2:
                        handleMetadata(index, mc.nh.wSock.readInt());
                        break;
                    case 3:
                        handleMetadata(index, mc.nh.wSock.readFloat());
                        break;
                    case 4:
                        handleMetadata(index, mc.nh.wSock.readString());
                        break;
                    case 5:
                        Classes.Item temp = new Classes.Item();
                        temp.readSlot(ref mc);

                        handleMetadata(index, temp);
                        break;
                    case 6:
                        Vector v = new Vector();

                        v.x = mc.nh.wSock.readInt();
                        v.y = mc.nh.wSock.readInt();
                        v.z = mc.nh.wSock.readInt();

                        handleMetadata(index, v);
                        break;

                }
            } while (true);
           

        }
        
        /// <summary>
        /// Places metadata in its correct place as determined by the index, to keep the metadata reader clean.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        void handleMetadata(int index, dynamic data) {
            switch (index) { // -- Parsing this is pretty much impossible without making things 300x more complicated than need-be. As such, this is as much as im parsing :).
                case 0:
                    byte bitmask = (byte)data;
                    onFire = Convert.ToBoolean(bitmask & 0x01);
                    crouched = Convert.ToBoolean(bitmask & 0x02);
                    sprinting = Convert.ToBoolean(bitmask & 0x08);
                    eating = Convert.ToBoolean(bitmask & 0x10);
                    invisible = Convert.ToBoolean(bitmask & 0x20);
                    break;

                case 1:
                    air = (short)data;
                    break;
                case 2:
                    itemFrame = (Item)data;
                    break;
                case 3:
                    rotation = (byte)data;
                    break;
                    

            }
        }
        
        public void handleInventory(int slot, Item slotItem) {
            if (Inventory.ContainsKey(slot)) {
                Inventory.Remove(slot);
            }

            Inventory.Add(slot, slotItem);
        }

        public enum entityId {
            Creeper = 50,
            Skeleton,
            Spider,
            GiantZombie,
            Zombie,
            Slime,
            Ghast,
            ZombiePigman,
            Enderman,
            CaveSpider,
            Silverfish,
            Blaze,
            MagmaCube,
            EnderDragon,
            Wither,
            Bat,
            Witch,
            Pig = 90,
            Sheep,
            Cow,
            Chicken,
            Squid,
            Wolf,
            Mooshroom,
            Snowman,
            Ocelot,
            IronGolem,
            Villager = 120
        }
    }
}
