using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace QM_LockedFridge
{
    /// <summary>
    /// Centralizes all path logic for this mod's config and persistence files.
    /// Config is stored under Quasimorph_ModConfigs/ (sibling of the game's AppData folder)
    /// to avoid being overwritten when Steam Cloud syncs the game's AppData folder.
    /// </summary>
    public class ConfigDirectories
    {
        /// <summary>The name of this mod's assembly (used as the mod's folder name).</summary>
        public string ModAssemblyName { get; private set; }

        /// <summary>The shared Quasimorph_ModConfigs/ root folder for all mods.</summary>
        public string AllModsConfigFolder { get; private set; }

        /// <summary>This mod's dedicated sub-folder inside AllModsConfigFolder.</summary>
        public string ModPersistenceFolder { get; private set; }

        /// <summary>Full path to this mod's config file.</summary>
        public string ConfigPath { get; private set; }

        public ConfigDirectories(string configFileName = "config.json")
        {
            ModAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            AllModsConfigFolder = Path.Combine(Application.persistentDataPath, "../Quasimorph_ModConfigs/");
            ModPersistenceFolder = Path.Combine(AllModsConfigFolder, ModAssemblyName);
            ConfigPath = Path.Combine(ModPersistenceFolder, configFileName);
        }
    }
}
