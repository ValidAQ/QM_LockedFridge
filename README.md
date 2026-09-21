# Locked Fridge

Quasimorph mod that improves the ship's cryochamber cargo management.

Don't you hate it when you're tidying up your Magnum after a good haul, and all the spoilable items spill out of your cryochamber to other cargo holds?

This mod takes care of that in a few ways.

## Features

- **Perishable item auto-sort to cryochamber**: When you sort your ship cargo, perishable items are automatically routed into the cryochamber (if available) before normal sorting runs for other items.
- **Cryochamber sorting**: If the cryochamber tab is open when you click Sort, items are sorted in-place within the cryochamber without any being moved out.
- **Overflow handling**: If the cryochamber is full, overflow items fall back to regular cargo.

## Mod Compatibility

This mod patches the following game classes. Other mods that patch the same methods may conflict.

| Class                      | Method                     | Patch type           |
| -------------------------- | -------------------------- | -------------------- |
| `MGSC.ScreenWithShipCargo` | `SortArsenalButtonOnClick` | Prefix (replacement) |

### Unlimited Cargo Stacks compatibility

Compatible as of 1.0.1.
UCS raises ship-cargo stack limits to 9999 but intentionally keeps the Cryochamber at vanilla slot limits, so a single cargo item can hold far more than one fridge slot accepts.

The sorting logic therefore processes the active tab in slot-sized pieces: after the normal pass, routing continues while the fridge keeps accepting pieces (each iteration moves one slot's worth and UCS returns the remainder to the tab), until the fridge is full or nothing routable remains. Without UCS, the drain logic does nothing, as no stack ever exceeds a slot.

# Source Code
Source code is available on GitHub at https://github.com/validaq/QM_LockedFridge

## Changelog

### 1.0.1
* UCS compatibility: sorting with **Unlimited Cargo Stacks** moves everything the cryochamber can hold in one press instead of one stack per press.

### 1.0.0
* Initial release.
