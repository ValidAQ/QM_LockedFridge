#if MCM_PRESENT
using ModConfigMenu;
using ModConfigMenu.Contracts;
using ModConfigMenu.Implementations;
using ModConfigMenu.Objects;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QM_LockedFridge
{
    /// <summary>
    /// Handles optional registration with Mod Configuration Menu (MCM).
    /// This file is only compiled when MCM.dll is found in the workshop folder at build time
    /// (i.e. the MCM_PRESENT symbol is defined in QM_LockedFridge.csproj).
    /// </summary>
    internal static class McmIntegration
    {
        /// <summary>
        /// Registers this mod with MCM if the MCM assembly is loaded at runtime.
        /// Safe to call even when MCM is not installed — the runtime check prevents any crash.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void RegisterIfPresent()
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name == "MCM")
                {
                    Register();
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Register()
        {
            var configValues = new List<IConfigValue>
            {
                new ConfigValue(
                    key: "RoutePerishablesToCryo",
                    value: Plugin.Config.RoutePerishablesToCryo,
                    header: "Cryochamber",
                    defaultValue: true,
                    tooltip: "When sorting a cargo tab, perishable items are auto-sorted into the cryochamber. Per-tab exclusions live in the tab's Configure (gear) screen.",
                    label: "Auto-sort perishables to cryochamber"),
            };

            ModConfigMenuAPI.RegisterModConfig(
                modName: "QM_LockedFridge",
                configData: configValues,
                OnConfigSaved: (Dictionary<string, object> currentConfig, out string feedback) =>
                {
                    feedback = null;
                    try
                    {
                        if (currentConfig.TryGetValue("RoutePerishablesToCryo", out object val))
                            Plugin.Config.RoutePerishablesToCryo = Convert.ToBoolean(val);
                        Plugin.Config.Save(Plugin.ConfigDirectories.ConfigPath);
                    }
                    catch (System.Exception ex)
                    {
                        Plugin.Logger.LogError("Failed to save MCM config.");
                        Plugin.Logger.LogException(ex);
                        feedback = "Locked Fridge: failed to save settings (see log).";
                        return false;
                    }
                    return true;
                });

            Plugin.Logger.Log("Registered with Mod Configuration Menu.");
        }
    }
}
#endif
