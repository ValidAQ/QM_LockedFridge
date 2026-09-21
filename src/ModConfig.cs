using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace QM_LockedFridge
{
    /// <summary>
    /// Holds this mod's user-editable settings.
    /// Serialized to / deserialized from JSON; new fields added in later mod
    /// versions are automatically written back with their default values.
    /// </summary>
    public class ModConfig
    {
        /// <summary>
        /// Global default: when sorting a regular cargo tab, perishable items
        /// are auto-sorted into the cryochamber. Toggleable in MCM.
        /// </summary>
        public bool RoutePerishablesToCryo { get; set; } = true;

        /// <summary>
        /// Per-tab overrides, keyed by the cargo tab's index in
        /// MagnumCargo.Tabs (index is stable: the 7 tabs and their paired
        /// ShipCargo storages are fixed-size, index-aligned lists serialized
        /// positionally into the save; vanilla has no add/remove/reorder path,
        /// and renaming a tab does not affect indices). Absent key = fall back
        /// to <see cref="RoutePerishablesToCryo"/>. NOTE: this config is shared
        /// across saves, so "tab 3" means the 4th tab in every save.
        /// </summary>
        public Dictionary<string, bool> TabRouting { get; set; } = new Dictionary<string, bool>();

        /// <summary>
        /// Effective routing decision for one cargo tab: the global default
        /// gated by the per-tab override (absent key inherits the default).
        /// </summary>
        public bool RoutingEnabledForTab(int tabIndex)
        {
            if (!RoutePerishablesToCryo)
                return false;
            if (TabRouting != null && TabRouting.TryGetValue(tabIndex.ToString(), out bool value))
                return value;
            return true;
        }

        /// <summary>Writes the explicit per-tab override (the Configure-screen toggle).</summary>
        public void SetTabRouting(int tabIndex, bool value)
        {
            TabRouting = TabRouting ?? new Dictionary<string, bool>();
            TabRouting[tabIndex.ToString()] = value;
        }

        public void Save(string configPath)
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(
                this, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        /// <summary>
        /// Loads the config from <paramref name="configPath"/>, or creates a default one if it does not exist.
        /// Also re-saves the file when new fields have been added since the last run.
        /// </summary>
        public static ModConfig LoadConfig(string configPath)
        {
            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };

            if (File.Exists(configPath))
            {
                try
                {
                    string sourceJson = File.ReadAllText(configPath);
                    var config = JsonConvert.DeserializeObject<ModConfig>(sourceJson, settings);

                    // Re-save to pick up any new fields added in this mod version.
                    string upgraded = JsonConvert.SerializeObject(config, settings);
                    if (upgraded != sourceJson)
                    {
                        Plugin.Logger.Log("Config updated with new default fields.");
                        File.WriteAllText(configPath, upgraded);
                    }

                    return config;
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError("Error reading config - using defaults.");
                    Plugin.Logger.LogException(ex);
                    return new ModConfig();
                }
            }
            else
            {
                var config = new ModConfig();
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, settings));
                return config;
            }
        }
    }
}
