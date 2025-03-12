# Nine Sols Teleport From Anywhere

Simply open the menu as usual, and the "Teleport" section will be available in the top left as if you were at the Pavilion root node.

In addition:

- The Teleport menu will let you teleport directly to your "current node", instead of assuming you must already be sitting at it.

I created this mod because my Archipelago Randomizer needs this functionality, and (unlike many parts of a randomizer) it makes sense as a separate quality-of-life mod for veteran players.

## Doesn't DebugMod already have a teleport feature?

The teleport feature in jakobhellermann's DebugMod is aimed at developers, and works well for them, while TFA is better suited for regular players. For example:
- DebugMod expects you to read its README to realize it even has a teleport feature, much less how to use it. TFA exposes the same menu and interface for teleportation that the vanilla game expects players to discover and use during normal gameplay.
- DebugMod lets you teleport *to* anywhere, including places you can't normally reach that potentially break the game. TFA only lets you teleport to root nodes you've unlocked before, just like the vanilla game.

## Building and Publishing

As [the Example Mod](https://github.com/nine-sols-modding/NineSols-ExampleMod) describes in more detail: use `dotnet publish` to generate a build in `thunderstore/build/`, test it by using r2modman's "Import local mod" feature, then upload it to https://thunderstore.io/c/nine-sols/create
