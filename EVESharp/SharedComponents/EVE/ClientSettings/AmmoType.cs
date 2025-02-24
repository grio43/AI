using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SharedComponents.EVE.ClientSettings
{
    [Serializable]
    public class AmmoType
    {
        #region Fields

        private string _name = string.Empty;

        #endregion Fields

        /**
         * This needs some kind of callback mechanism so that E# (the launcher) can have a client (DirectEVE asking eve) lookup the TypeID and get back an InvType
         *
        public string Name
        {
            get
            {
                if (!String.IsNullOrEmpty(_name))
                    return _name;

                var ret = String.Empty;
                if (!ESCache.Instance.DirectEve.DoesInvTypeExistInTypeStorage(TypeId))
                    return ret;

                var invType = ESCache.Instance.DirectEve.GetInvType(TypeId);

                if (invType == null)
                    return ret;

                var typeName = invType.TypeName;
                _name = typeName;
                return typeName;
            }
        }
        **/

        #region Methods

        public AmmoType Clone()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                if (GetType().IsSerializable)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Binder = new CustomizedBinder();
                    formatter.Serialize(stream, this);
                    stream.Position = 0;
                    return (AmmoType)formatter.Deserialize(stream);
                }
                return null;
            }
        }

        #endregion Methods

        #region Classes

        [Serializable]
        private sealed class CustomizedBinder : SerializationBinder
        {
            #region Methods

            public override Type BindToType(string assemblyName, string typeName)
            {
                if (typeName.Contains(nameof(AmmoType)))
                    return typeof(AmmoType);
                if (typeName.Contains(nameof(ClientSettings.DamageType)))
                    return typeof(DamageType);
                return null;
            }

            #endregion Methods
        }

        #endregion Classes

        #region Constructors

        public AmmoType()
        {
        }

        public AmmoType(XElement ammo)
        {
            try
            {
                TypeId = (int)ammo.Attribute("typeId");
                DamageType = (DamageType)Enum.Parse(typeof(DamageType), (string)ammo.Attribute("damageType"));
                Range = (int)ammo.Attribute("range");
                Quantity = (int)ammo.Attribute("quantity");
                Description = (string)ammo.Attribute("description");
                Default = (bool?)ammo.Attribute("default") ?? false;
                OverrideTargetTypeId = (int?)ammo.Attribute("OverrideTargetTypeId") ?? 0;
                OverrideTargetGroupId = (int?)ammo.Attribute("OverrideTargetGroupId") ?? 0;
                OverrideTargetName = (string)ammo.Attribute("OverrideTargetName") ?? "NPCNameNeedsToBePartOfAmmoDefintionIfYouWantToUseThis";
                OnlyUseAsOverrideAmmo = (bool?)ammo.Attribute("OnlyUseAsOverrideAmmo") ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("AmmoType exception: " + ex);
            }
        }

        #endregion Constructors

        #region Properties

        public DamageType DamageType { get; set; }
        public string Description { get; set; }
        public int OverrideTargetTypeId { get; set; }
        public int OverrideTargetGroupId { get; set; }

        public string OverrideTargetName { get; set; }
        public bool OnlyUseAsOverrideAmmo { get; set; }
        public int Quantity { get; set; }
        public int Range { get; set; }
        public int TypeId { get; set; }

        public int EMDamage { get; set; } = 0;
        public int KineticDamage { get; set; } = 0;
        public int ThermalDamage { get; set; } = 0;
        public int ExplosiveDamage { get; set; } = 0;

        public bool Default { get; set; } = false;


        public int DamageForThisDamageType(DamageType thisDamageType)
        {
            if (thisDamageType == DamageType.EM)
            {
                return EMDamage;
            }

            if (thisDamageType == DamageType.Kinetic)
            {
                return KineticDamage;
            }

            if (thisDamageType == DamageType.Thermal)
            {
                return ThermalDamage;
            }

            if (thisDamageType == DamageType.Explosive)
            {
                return ExplosiveDamage;
            }

            return 0;
        }

        public bool IsValidAmmo(int TypeIdOfKilltarget = 0, int GroupIdOfKilltarget = 0)
        {
            if (OnlyUseAsOverrideAmmo)
            {
                if (OverrideTargetTypeId != 0)
                {
                    if (TypeIdOfKilltarget == OverrideTargetTypeId)
                    {
                        return true;
                    }
                }

                if (OverrideTargetGroupId != 0)
                {
                    if (GroupIdOfKilltarget == OverrideTargetGroupId)
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        #endregion Properties
    }

    [Serializable]
    [XmlRoot(ElementName = "ammoType")]
    public class AmmoTypeOldFromXML
    {
        #region Properties

        [XmlAttribute(AttributeName = "damageType")]
        public DamageType DamageType { get; set; }

        [XmlAttribute(AttributeName = "quantity")]
        public int Quantity { get; set; }

        [XmlAttribute(AttributeName = "range")]
        public int Range { get; set; }

        [XmlAttribute(AttributeName = "typeId")]
        public int TypeId { get; set; }


        #endregion Properties
    }
}