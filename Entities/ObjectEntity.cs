namespace libMC.NET.Entities {
    public class ObjectEntity {
        public int ObjectId;
        public string ObjectFriendlyName;
        public sbyte Type;
        #region Optional Variables
        // -- These are specific to each individual type of object.
        string _style; // -- Minecart style, I.E. Chest, Furnace, ect.
        int _orientation; // -- 0-3: south, west, north, east.
        int _blockType; // BlockID | (Metadata << 0xC)
        int _entityId; // Fishing float, Splash potion, Projectiles.

        public short SpeedX;
        public short SpeedY;
        public short SpeedZ;
        #endregion

        public ObjectEntity(sbyte type) {
            Type = type;
        }

        public void GetFriendlyName(int metadataId) {

            switch (Type) {
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
                    _entityId = metadataId;
                    break;
                case 61:
                    ObjectFriendlyName = "Arrow (projectile)";
                    _entityId = metadataId;
                    break;
                case 62:
                    ObjectFriendlyName = "Snowball (projectile)";
                    _entityId = metadataId;
                    break;
                case 63:
                    ObjectFriendlyName = "Fireball (Ghast projectile)";
                    _entityId = metadataId;
                    break;
                case 64:
                    ObjectFriendlyName = "FireCharge (Blaze projectile)";
                    _entityId = metadataId;
                    break;
                case 65:
                    ObjectFriendlyName = "Thrown Enderpearl";
                    _entityId = metadataId;
                    break;
                case 66:
                    ObjectFriendlyName = "Wither Skull (projectile)";
                    _entityId = metadataId;
                    break;
                case 70:
                    ObjectFriendlyName = "Falling Objects";
                    _blockType = metadataId;
                    break;
                case 71:
                    ObjectFriendlyName = "Item frames";
                    _orientation = metadataId;
                    break;
                case 72:
                    ObjectFriendlyName = "Eye of Ender";
                    _entityId = metadataId;
                    break;
                case 73:
                    ObjectFriendlyName = "Thrown Potion";
                    _entityId = metadataId;
                    break;
                case 74:
                    ObjectFriendlyName = "Falling Dragon Egg";
                    _entityId = metadataId;
                    break;
                case 75:
                    ObjectFriendlyName = "Thrown Exp Bottle";
                    _entityId = metadataId;
                    break;
                case 90:
                    ObjectFriendlyName = "Fishing Float";
                    _entityId = metadataId;
                    break;
            }
        }
    }
}
