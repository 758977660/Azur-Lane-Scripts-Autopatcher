﻿using System.IO;
using System.Text.RegularExpressions;

namespace Azurlane
{
    internal static class Write
    {
        internal static void Cooldown(string lua, int value)
        {
            if (lua.Contains("weapon_property"))
                Rewrite(lua, "reload_max = .*", $"reload_max = {value.ToString()},");
        }

        internal static void Damage(string lua, int value)
        {
            if (lua.Contains("weapon_property"))
                Rewrite(lua, "damage = .*", $"damage = {value.ToString()},");
        }

        internal static void GodMode(string lua)
        {
            if (lua.Contains("enemy_data_statistics"))
            {
                Rewrite(lua, @"equipment_list = \{([^\}]+)\}", "equipment_list = {}");
                Rewrite(lua, "speed = .*", "speed = 12,");
                Rewrite(lua, "speed_growth = .*", "speed_growth = 12,");
            }
            if (lua.Contains("aircraft_template"))
            {
                Rewrite(lua, "accuracy = .*", "accuracy = 1,");
                Rewrite(lua, "attack_power = .*", "attack_power = 1,");
                Rewrite(lua, "AP_growth = .*", "AP_growth = 1,");
                Rewrite(lua, "crash_DMG = .*", "crash_DMG = 1,");
            }
        }

        internal static void Rewrite(string path, string pattern, string replacement) => File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), pattern, replacement));

        internal static void WeakEnemy(string lua, int value)
        {
            if (lua.Contains("enemy_data_statistics"))
            {
                Rewrite(lua, "durability = .*", $"durability = {value.ToString()},");
                Rewrite(lua, "durability_growth = .*", $"durability_growth = {value.ToString()},");
            }
        }
    }
}