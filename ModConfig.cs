using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeddingRings
{
    class ModConfig
    {
        public int MagneticRadius { get; set; } = 64;

        public float KnockbackModifier { get; set; } = 0.05f;

        public float WeaponPrecisionModifier { get; set; } = 0.05f;

        public float CritChanceModifier { get; set; } = 0.05f;

        public float CritPowerModifier { get; set; } = 0.05f;

        public float WeaponSpeedModifier { get; set; } = 0.05f;

        public float AttackIncreaseModifier { get; set; } = 0.05f;

        public int Resilience { get; set; } = 1;
    }
}
