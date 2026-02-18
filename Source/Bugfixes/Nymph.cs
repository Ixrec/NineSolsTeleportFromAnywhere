using HarmonyLib;
using RCGFSM.Items;
using System.Collections.Generic;
using UnityEngine;
using static RCGFSM.Items.PickItemAction;

namespace TeleportFromAnywhere.Bugfixes;

[HarmonyPatch]
internal class Nymph {
    /*
     * Because Lady E's soulscape takes away the nymph at the start, just teleporting out of it would leave you nymph-less.
     *
     * This problem is not quite as simple as it sounds because other mods, such as the randomizer which depends on TFA, make
     * it legitimate to enter Lady E's soulscape without the nymph. So if we just gave the nymph to any player who teleported
     * out of soulscape, that would be a bug for randomizer players who didn't have the nymph when they entered.
     *
     * The simplest solution I know of that works for both vanilla and other mods is to prevent Lady E from taking away
     * your nymph in the first place.
     */

    public static string GetGOPath(GameObject go) {
        var transform = go.transform;
        List<string> pathParts = new List<string>();
        while (transform != null) {
            pathParts.Add(transform.name);
            transform = transform.parent;
        }
        pathParts.Reverse();
        return string.Join("/", pathParts);
    }

    private static string NymphAbilitySaveFlagId = "be31937c6691a44d88d3d70ac2f62cc9PlayerAbilityData";

    [HarmonyPrefix, HarmonyPatch(typeof(PickItemAction), "OnStateEnterImplement")]
    static bool PickItemAction_OnStateEnterImplement(PickItemAction __instance) {
        if (__instance.scheme == PickableScheme.RemoveItem && __instance.pickItemData.FinalSaveID == NymphAbilitySaveFlagId) {
            var goPath = GetGOPath(__instance.gameObject);
            if (goPath == "A2_Stage_Remake/Room/Prefab/FallingTeleportTrickBackgroundProvider/A7_HotSpring/溫泉場景Setting FSM Object/FSM Animator/LogicRoot/SectionA/[CutScene] Portal 第一次來到 A7/--[States]/FSM/[State] PlayCutSceneEnd/[Action] DisableButterfly") {

                // Since this block is guaranteed to run at the start of the soulscape, and we have to patch it anyway for the nymph removal issue,
                // we might as well re-use this block to fix the escort nymph issue as well:
                var escortNymphState = (GameFlagInt)SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict["abf251f7-ed0d-437e-8797-34946931068c_93653f615d5940a409fffdcecb72ec43GameFlagInt"];
                if (escortNymphState.CurrentValue > 0) {
                    Log.Info($"resetting the 'escort nymph' back to its initial position, since it was at position {escortNymphState.CurrentValue} which would make the soulscape impossible to finish");
                    escortNymphState.CurrentValue = 0;
                }

                Log.Info($"preventing Lady E's soulscape from disabling the nymph, so you can safely teleport out without losing it");
                return false; // prevent the base game code from disabling the nymph
            }
        }
        return true; // some other PickItemAction, just let it run normally
    }
}
