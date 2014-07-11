using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Entities {
    public class ObjectEntity {
        public int ObjectID;
        public string ObjectFriendlyName;
        public sbyte type;
        #region Optional Variables
        // -- These are specific to each individual type of object.
        string style; // -- Minecart style, I.E. Chest, Furnace, ect.
        int orientation; // -- 0-3: south, west, north, east.
        int blockType; // BlockID | (Metadata << 0xC)
        int EntityID; // Fishing float, Splash potion, Projectiles.

        public short Speed_X;
        public short Speed_Y;
        public short Speed_Z;
        #endregion

        public ObjectEntity(sbyte Type) {
            type = Type;
        }

        public void GetFriendlyName(int metadataID) {

            switch (type) {
                case 0:
                    return;
                case 1:
                    ObjectFriendlyName = "Boat";
                    break;
                case 2:
                    ObjectFriendlyName = "Item Stack";
                    break;
                case 10:
                    ObjectFriendlyName = "Minecart";
                    //if (metadataID == 0)
                    break;
                case 50:
                    ObjectFriendlyName = "Activated TNT";
                    break;
                case 51:
                    ObjectFriendlyName = "EnderCrystal";
                    EntityID = metadataID;
                    break;
                case 61:
                    ObjectFriendlyName = "Arrow (projectile)";
                    EntityID = metadataID;
                    break;
                case 62:
                    ObjectFriendlyName = "Snowball (projectile)";
                    EntityID = metadataID;
                    break;
                case 63:
                    ObjectFriendlyName = "Fireball (Ghast projectile)";
                    EntityID = metadataID;
                    break;
                case 64:
                    ObjectFriendlyName = "FireCharge (Blaze projectile)";
                    EntityID = metadataID;
                    break;
                case 65:
                    ObjectFriendlyName = "Thrown Enderpearl";
                    EntityID = metadataID;
                    break;
                case 66:
                    ObjectFriendlyName = "Wither Skull (projectile)";
                    EntityID = metadataID;
                    break;
                case 70:
                    ObjectFriendlyName = "Falling Objects";
                    blockType = metadataID;
                    break;
                case 71:
                    ObjectFriendlyName = "Item frames";
                    orientation = metadataID;
                    break;
                case 72:
                    ObjectFriendlyName = "Eye of Ender";
                    EntityID = metadataID;
                    break;
                case 73:
                    ObjectFriendlyName = "Thrown Potion";
                    EntityID = metadataID;
                    break;
                case 74:
                    ObjectFriendlyName = "Falling Dragon Egg";
                    EntityID = metadataID;
                    break;
                case 75:
                    ObjectFriendlyName = "Thrown Exp Bottle";
                    EntityID = metadataID;
                    break;
                case 90:
                    ObjectFriendlyName = "Fishing Float";
                    EntityID = metadataID;
                    break;
            }
        }
    }
}
