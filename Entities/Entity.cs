using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;
using CWrapped;

namespace libMC.NET.Entities {
    /// <summary>
    /// Handles all Entities, including mobs and other players.
    /// </summary>
    public class Entity {
        public int Entity_ID, direction, Vehicle_ID, potion_color; // -- 0 = -z, 1 = -z, 2 = +z, 3 = +x
        public bool onFire, crouched, sprinting, eating, invisible, inBed, attached, leashed;
        public dynamic[] Metadata;
        public string Type, UUID, playerName;
        public Vector Location;
        public byte mobType;
        public sbyte pitch, headPitch, yaw, status, amplifier, animation, rotation, ambient, arrows, nametag; // -- mobType is optional, only for mobs.
        public short Velocity_X, Velocity_Y, Velocity_Z, heldItem, count, duration, air;
        public Item itemFrame;
        public float health;

        public double jumpStrength, movementSpeed;

        public Dictionary<int, Item> Inventory;

        public Entity(string type) {
            Type = type;
            Inventory = new Dictionary<int, Item>();
        }
        
        /// <summary>
        /// Places Metadata in its correct place as determined by the index, to keep the Metadata reader clean.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        void HandleMetadata(int index, dynamic data) {
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
                    rotation = (sbyte)data;
                    break;
                    

            }
        }
        
        public void HandleInventory(int slot, Item slotItem) {
            if (Inventory.ContainsKey(slot)) {
                Inventory.Remove(slot);
            }

            Inventory.Add(slot, slotItem);
        }

        public enum EntityId {
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
