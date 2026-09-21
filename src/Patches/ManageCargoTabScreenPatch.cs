using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QM_LockedFridge
{
    /// <summary>
    /// Adds a per-tab toggle ("Auto-sort perishables to cryochamber") to the
    /// cargo tab's Configure screen (the gear button in the hold header).
    ///
    /// The vanilla screen assembles its toggles dynamically from a pooled
    /// QToggle widget inside Configure (AddAllToggle / AddIncludeToSortToggle /
    /// AddWeaponClassToggle / AddToggle), parented to _content and registered
    /// in _otherToggles + _navigables for navigation and pool cleanup. This
    /// postfix mirrors exactly that pattern, writing into the mod's own
    /// config (per-tab override, keyed by tab index) on change.
    ///
    /// Placement rationale (user request): a header checkbox next to the gear
    /// was considered and rejected — the caption strip is 19px slots already
    /// shared with the vanilla buttons and the Sort All Tabs mod; the
    /// Configure screen has no such space competition and matches the vanilla
    /// per-tab UX.
    /// </summary>
    [HarmonyPatch(typeof(ManageCargoTabScreen), "Configure")]
    internal static class ManageCargoTabScreenPatch
    {
        private static void Postfix(ManageCargoTabScreen __instance, MagnumCargoTab cargoTab)
        {
            try
            {
                if (cargoTab == null || Plugin.Config == null)
                    return;

                MagnumCargo magnumCargo = Plugin.State?.Get<MagnumCargo>();
                if (magnumCargo == null)
                    return;

                // The Configure screen also opens for the cryochamber and
                // recycler tabs; the routing toggle is meaningless there.
                if (cargoTab == magnumCargo.FridgeTab || cargoTab == magnumCargo.RecyclingTab)
                    return;

                int tabIndex = magnumCargo.Tabs.IndexOf(cargoTab);
                if (tabIndex < 0)
                    return;

                Traverse screen = Traverse.Create(__instance);
                Pool togglePool = screen.Field<Pool>("_togglePool").Value;
                Transform content = screen.Field<Transform>("_content").Value;
                List<QToggle> otherToggles = screen.Field<List<QToggle>>("_otherToggles").Value;
                List<Navigable> navigables = screen.Field<List<Navigable>>("_navigables").Value;

                if (togglePool == null || content == null || otherToggles == null || navigables == null)
                {
                    Plugin.Logger.LogError("ManageCargoTabScreen internals not found — toggle not added.");
                    return;
                }

                QToggle toggle = togglePool.Take().GetComponent<QToggle>();
                toggle.name = "ToggleRoutePerishables";
                toggle.Initialize(LocalizationKeys.RoutePerishables, Plugin.Config.RoutingEnabledForTab(tabIndex));
                toggle.transform.SetParent(content, false);
                toggle.transform.SetAsLastSibling();
                RectTransform rectTransform = toggle.transform as RectTransform;
                rectTransform.sizeDelta = new Vector2(200f, rectTransform.sizeDelta.y);

                navigables.Add(toggle.Navigable);
                otherToggles.Add(toggle);

                toggle.OnToggleChanged += delegate (bool value)
                {
                    Plugin.Config.SetTabRouting(tabIndex, value);
                    Plugin.Config.Save(Plugin.ConfigDirectories.ConfigPath);
                    screen.Field<GameObject>("_changesWarning").Value?.SetActive(true);
                };

                // Rebuild keyboard/gamepad navigation so the new toggle is reachable.
                __instance.RefreshNavigationTransitions();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError("Failed to add perishable-routing toggle to ManageCargoTabScreen.");
                Plugin.Logger.LogException(ex);
            }
        }
    }

    internal static class LocalizationKeys
    {
        public const string RoutePerishables = "mod.qm_lockedfridge.route_perishables";
    }
}
