using System;
using System.Collections.Generic;
using libMC.NET.Common;

namespace libMC.NET.Entities {
    /// <summary>
    /// Handles all Entities, including mobs and other players.
    /// </summary>
    public class Entity {
        public int Entity_ID, Direction, VehicleId, PotionColor; // -- 0 = -z, 1 = -z, 2 = +z, 3 = +x
        public bool OnFire, Crouched, Sprinting, Eating, Invisible, InBed, Attached, Leashed;
        public dynamic[] Metadata;
        public string Type, Uuid, PlayerName;
        public Vector Location;
        public byte MobType;
        public sbyte Pitch, HeadPitch, Yaw, Status, Amplifier, Animation, Rotation, Ambient, Arrows, Nametag; // -- mobType is optional, only for mobs.
        public short VelocityX, VelocityY, VelocityZ, HeldItem, Count, Duration, Air;
        public Item ItemFrame;
        public float Health;

        public double JumpStrength, MovementSpeed;

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
                    var bitmask = (byte)data;
                    OnFire = Convert.ToBoolean(bitmask & 0x01);
                    Crouched = Convert.ToBoolean(bitmask & 0x02);
                    Sprinting = Convert.ToBoolean(bitmask & 0x08);
                    Eating = Convert.ToBoolean(bitmask & 0x10);
                    Invisible = Convert.ToBoolean(bitmask & 0x20);
                    break;

                case 1:
                    Air = (short)data;
                    break;
                case 2:
                    ItemFrame = (Item)data;
                    break;
                case 3:
                    Rotation = (sbyte)data;
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
