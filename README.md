# Nine Sols Teleport From Anywhere

Simply open the menu as usual, and the "Teleport" section will be available in the top left as if you were at the Pavilion root node.

In addition:

- The Teleport menu will let you teleport directly to your "current node", instead of assuming you must already be sitting at it.
- The Root Node menu will always show the [Pavilion]/[Last Node] button, even when the vanilla game normally hides this button (e.g. during the Prison sequence and just after Lady E).
- The Root Node menu will show the [Teleport] button on all nodes, not just the Pavilion node.

This also fixes or works around a handful of bugs caused by the vanilla game assuming you couldn't teleport away:

- Normally most boss arenas have a door that closes after you fight them, and opens after you retrieve the Seal from their vital sanctum. TFA forces that door to stay open in case you teleport out.
- Lady E's soulscape will no longer take away your Mystic Nymph, so that teleporting out of her soulscape won't leave you nymph-less.
- The "escort nymph" late in Lady E's soulscape will be reset to its initial checkpoint if you teleport out after doing one or more nymph escort segments.
- If you teleport out of Empyrean District Living Area's opera theater while the opera hologram is playing, the theater will be put back in its initial state so you can re-enter it.

I created this mod because my Archipelago Randomizer needs this functionality, and (unlike many parts of a randomizer) it makes sense as a separate quality-of-life mod for veteran players.

## Doesn't DebugMod already have a teleport feature?

The teleport feature in jakobhellermann's DebugMod is aimed at developers, and works well for them, while TFA is better suited for regular players. For example:
- DebugMod expects you to read its README to realize it even has a teleport feature, much less how to use it. TFA exposes the same menu and interface for teleportation that the vanilla game expects players to discover and use during normal gameplay.
- DebugMod lets you teleport *to* anywhere, including places you can't normally reach that potentially break the game. TFA only lets you teleport to root nodes you've unlocked before, just like the vanilla game.

## Building and Publishing

As [the Example Mod](https://github.com/nine-sols-modding/NineSols-ExampleMod) describes in more detail: use `dotnet publish` to generate a build in `thunderstore/build/`, test it by using r2modman's "Import local mod" feature, then upload it to https://thunderstore.io/c/nine-sols/create
