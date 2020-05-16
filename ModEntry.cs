using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using Microsoft.Xna.Framework;

namespace WeddingRings
{
    public class ModEntry : Mod, IAssetEditor
    {
        private ModConfig config;

        const int weddingRing = 801;

        private bool isEquipped;

        private int magneticRadius;

        private float knockbackModifier;

        private float weaponPrecisionModifier;

        private float critChanceModifier;

        private float critPowerModifier;

        private float weaponSpeedModifier;

        private float attackIncreaseModifier;

        private int resilience;

        private bool isSpouseEquipped;

        public override void Entry(IModHelper helper)
        {
            Configure();

            helper.Events.GameLoop.UpdateTicked          += GameLoop_UpdateTicked;
            helper.Events.Player.Warped                  += Player_Warped;
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Data/ObjectInformation"))
            {
                return true;
            }

            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/ObjectInformation"))
            {
                IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;

                string[] fields = data[weddingRing].Split('/');
                fields[5] = "Glows, attracts items, and increases knockback, precision, critical strike chance and power, weapon and attack speeds, and defense.";
                data[weddingRing] = string.Join("/", fields);
            }
        }

        private void Configure()
        {
            config = Helper.ReadConfig<ModConfig>();

            magneticRadius          = config.MagneticRadius;
            knockbackModifier       = config.KnockbackModifier;
            weaponPrecisionModifier = config.WeaponPrecisionModifier;
            critChanceModifier      = config.CritChanceModifier;
            critPowerModifier       = config.CritPowerModifier;
            weaponSpeedModifier     = config.WeaponSpeedModifier;
            attackIncreaseModifier  = config.AttackIncreaseModifier;
            resilience              = config.Resilience;
    }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree) { return; }

            if (Game1.player.isWearingRing(weddingRing) && (isEquipped == false))
            {
                WeddingRingEquipped();
            }
            else if ((!Game1.player.isWearingRing(weddingRing)) && (isEquipped == true))
            {
                WeddingRingUnEquipped();
            }

            if (! isEquipped) { return; }

            LightMove(Game1.currentLocation);

            SpouseBonus();
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            LightOff(e.OldLocation);
            LightOn(e.NewLocation);
        }

        private void WeddingRingEquipped()
        {
            isEquipped = true;

            LightOn(Game1.currentLocation);

            BuffStats();
        }

        private void WeddingRingUnEquipped()
        {
            isEquipped = false;

            LightOff(Game1.currentLocation);

            NerfStats();
        }

        private void LightOn(GameLocation location)
        {
            location.sharedLights[(1337 + (int)Game1.player.uniqueMultiplayerID)] = new LightSource(
                1,
                new Vector2(Game1.player.Position.X + 21f, Game1.player.Position.Y + 64f),
                10f,
                new Color(0, 80, 0),
                1337 + (int)Game1.player.UniqueMultiplayerID,
                LightSource.LightContext.None,
                Game1.player.UniqueMultiplayerID
            );
        }

        private void LightOff(GameLocation location)
        {
            location.removeLightSource(1337 + (int)Game1.player.uniqueMultiplayerID);
        }

        private void LightMove(GameLocation location)
        {
            location.repositionLightSource(
                1337 + (int)Game1.player.UniqueMultiplayerID,
                new Vector2(Game1.player.Position.X + 21f, Game1.player.Position.Y)
            );

            if (!location.isOutdoors && !(location is MineShaft))
            {
                LightSource i = location.getLightSource(1337 + (int)Game1.player.UniqueMultiplayerID);
                if (i != null)
                {
                    i.radius.Value = 3f;
                }
            }
        }

        private void BuffStats()
        {
            Game1.player.magneticRadius.Value    += magneticRadius;
            Game1.player.knockbackModifier       += knockbackModifier;
            Game1.player.weaponPrecisionModifier += weaponPrecisionModifier;
            Game1.player.critChanceModifier      += critChanceModifier;
            Game1.player.critPowerModifier       += critPowerModifier;
            Game1.player.weaponSpeedModifier     += weaponSpeedModifier;
            Game1.player.attackIncreaseModifier  += attackIncreaseModifier;
            Game1.player.resilience              += resilience;

            LogStats();
        }

        private void NerfStats()
        {
            Game1.player.magneticRadius.Value    -= magneticRadius;
            Game1.player.knockbackModifier       -= knockbackModifier;
            Game1.player.weaponPrecisionModifier -= weaponPrecisionModifier;
            Game1.player.critChanceModifier      -= critChanceModifier;
            Game1.player.critPowerModifier       -= critPowerModifier;
            Game1.player.weaponSpeedModifier     -= weaponSpeedModifier;
            Game1.player.attackIncreaseModifier  -= attackIncreaseModifier;
            Game1.player.resilience              -= resilience;
            
            LogStats();
        }
        
        private void LogStats()
        {
            Monitor.Log($"Wedding Ring IS {(!isEquipped ? "NOT " : "")}equipped.\n" +
                $"MR: {Game1.player.magneticRadius.Value}\n" +
                $"KB: {Game1.player.knockbackModifier}\n" +
                $"PM: {Game1.player.weaponPrecisionModifier}\n" +
                $"CC: {Game1.player.critChanceModifier}\n" +
                $"CP: {Game1.player.critPowerModifier}\n" +
                $"WS: {Game1.player.weaponSpeedModifier}\n" +
                $"AI: {Game1.player.attackIncreaseModifier}\n" +
                $"RS: {Game1.player.resilience}\n",
            LogLevel.Debug);
        }

        private void SpouseBonus()
        {
            Farmer spouse;
            long? spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
            bool foundSpouse = Game1.otherFarmers.TryGetValue(spouseID.Value, out spouse);

            if (!foundSpouse)
            {
                if (isSpouseEquipped)
                {
                    SpouseUnEquipped();
                }

                return;
            }

            if ((spouse.isWearingRing(weddingRing)) && (isSpouseEquipped == false))
            {
                SpouseEquipped();
            }
            else if ((!spouse.isWearingRing(weddingRing)) && (isSpouseEquipped == true))
            {
                SpouseUnEquipped();
            }


        }

        private void SpouseEquipped()
        {
            isSpouseEquipped = true;

            LogSpouse();
            
            BuffStats();
        }

        private void SpouseUnEquipped()
        {
            isSpouseEquipped = false;

            LogSpouse();
         
            NerfStats();
        }

        private void LogSpouse()
        {
            Monitor.Log($"Spouse's Wedding Ring IS {(! isSpouseEquipped ? "NOT " : "")}equipped.", LogLevel.Debug);
        }
    }
}
