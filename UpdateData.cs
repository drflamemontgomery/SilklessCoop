using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace SilklessCoop
{

    [DataContract]
    struct GeoEvent
    {
        [DataMember]
        public int amount;
    }

    [DataContract]
    struct SpriteData
    {
        [DataMember]
        public int id;
        [DataMember]
        public float scaleX;
        [DataMember]
        public float scaleY;
    }

    [DataContract]
    struct CompassData
    {
        [DataMember]
        public bool active;
        [DataMember]
        public float x;
        [DataMember]
        public float y;
    }

    [DataContract]
    struct UpdateData
    {
        [DataMember]
        public float x;
        [DataMember]
        public float y;
        [DataMember]
        public float z;

        [DataMember]
        public float vx;
        [DataMember]
        public float vy;

        [DataMember]
        public SpriteData sprite;
        [DataMember]
        public CompassData? compass;

        [DataMember]
        public GeoEvent? geoEvent;
        [DataMember]
        public GeoEvent? shardEvent;

        [DataMember]
        public int? damage;

        [DataMember]
        public bool? deathEvent;
    }
}