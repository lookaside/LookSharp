using System;
using System.Linq;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Rendering;
using SharpDX;
using LookSharp.Plugins;


namespace LookSharp
{
    abstract class Helper
    {
        public static Vector3 extend(Vector3 position, Vector3 target, float distance, int towards) // towards/away from target
        {
            return position + Vector3.Normalize(towards * (target - position)) * distance;
        }

        public static Vector3 RotateAroundPoint(Vector3 rotated, Vector3 around, float angle)
        {
            double sin = Math.Sin((double)angle);
            double cos = Math.Cos((double)angle);

            double x = ((rotated.X - around.X) * cos) - ((rotated.Y - around.Y) * sin) + around.X;
            double y = ((rotated.X - around.X) * sin) + ((rotated.Y - around.Y) * cos) + around.Y;

            return new Vector3((float)x, (float)y, rotated.Z);
        }

        public static bool IsValidTarget(AIHeroClient target)
        {
            return target.IsValid && target.IsEnemy && !target.IsDead && !target.IsInvulnerable && !target.IsZombie;
        }

        public static bool HasSmite(AIHeroClient target)
        {
            return target.Spellbook.Spells.Any(spell => spell.Name.ToLower().Contains("smite"));
        }
    }
}
