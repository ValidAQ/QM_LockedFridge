using HarmonyLib;
using MGSC;
using System.Collections.Generic;

namespace QM_LockedFridge
{
    /// <summary>
    /// Patches the ship cargo sort button to provide specific cryochamber-aware behaviours.
    ///
    /// <list type="bullet">
    /// <item><description>
    /// Fridge tab active -> sort items in-place within the fridge; nothing leaves.
    /// </description></item>
    /// <item><description>
    /// Regular tab active -> perishable items are routed into the fridge before
    /// normal tab distribution runs for everything else.
    /// </description></item>
    /// </list>
    /// If the fridge is full, overflow falls back to regular cargo.
    /// </summary>
    [HarmonyPatch(typeof(ScreenWithShipCargo), "SortArsenalButtonOnClick")]
    internal static class FreezerSortPatch
    {
        // Returns false to replace the original method entirely.
        private static bool Prefix(ScreenWithShipCargo __instance)
        {
            ItemStorage activeShipCargo = __instance._activeShipCargo;
            MagnumCargo magnumCargo = __instance._magnumCargo;
            SpaceTime spaceTime = __instance._spaceTime;
            MagnumProgression magnumProg = __instance._magnumSpaceship;

            // ── Branch 1: Fridge tab is active ──────────────────────────────────
            // Items that are already frozen should stay in the fridge.
            // Just re-pack the grid and we're done.
            if (activeShipCargo == magnumCargo.FridgeStorage)
            {
                activeShipCargo.SortWithExpandByTypeAndName(spaceTime);
                __instance.RefreshView();
                return false;
            }

            // ── Branch 2: Regular cargo tab ──────────────────────────────────────

            // Snapshot all items and clear the tab (mirrors original behaviour).
            var items = new List<BasePickupItem>(activeShipCargo.Items);
            activeShipCargo.RemoveAllItems();

            // Reproduce the original tab-filter logic:
            //   - If this tab has IncludeToSort=true  - distribute freely (tabFilter=true)
            //   - If IncludeToSort=false              - items stay on this specific tab
            bool tabFilter = true;
            ItemStorage specificStorage = null;
            int tabIndex = magnumCargo.ShipCargo.IndexOf(activeShipCargo);
            if (tabIndex != -1)
            {
                MagnumCargoTab activeTab = magnumCargo.Tabs[tabIndex];
                tabFilter = activeTab.IncludeToSort;
                if (!tabFilter)
                    specificStorage = activeShipCargo;
            }

            bool hasFridge = magnumProg.HasStoreFridge;

            foreach (BasePickupItem item in items)
            {
                // Route perishables to the fridge when it is available.

                // Get ItemClass from the primary record (ItemRecord) of this item.
                var itemRecord = Data.Items.GetSimpleRecord<ItemRecord>(item.Id);
                int itemClassValue = (int)itemRecord.ItemClass;

                // Data.ItemExpire.GetRecord returns null for non-perishable items,
                // but QuasiPact items may also have expiration records while not
                // being normally perishable. Not sure why.
                bool isQuasiItem = itemClassValue == (int)ItemClass.QuasiPact;

                bool filterCondition = hasFridge && Data.ItemExpire.GetRecord(item.Id) != null && !isQuasiItem;

                if (filterCondition)
                {
                    // PutItemWithFallback already handles a full fridge by
                    // spilling to ShipCargo[0], so no extra overflow logic is needed.
                    MagnumCargoSystem.AddCargo(
                        magnumCargo, spaceTime, item,
                        specificStorage: magnumCargo.FridgeStorage,
                        splittedItem: false,
                        tabFilter: false);
                }
                else
                {
                    // Non-perishable items: use the original distribution logic.
                    MagnumCargoSystem.AddCargo(
                        magnumCargo, spaceTime, item,
                        specificStorage: specificStorage,
                        splittedItem: false,
                        tabFilter: tabFilter);
                }
            }

            // Sort the active (non-fridge) tab after redistribution.
            activeShipCargo.SortWithExpandByTypeAndName(spaceTime);
            __instance.RefreshView();
            return false;
        }
    }
}
