using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace TeleportFromAnywhere.Bugfixes;

[HarmonyPatch]
internal class BossArenas {
    /*
     * Most Sol arenas have doors that lock behind you and don't immediately open after the fight.
     * In vanilla these are meant to ensure you retrieve the seal from the vital sanctum before leaving.
     * But you can use TFA to bypass them, leaving yourself trapped outside the closed doors with no way back in.
     * So we want to force these doors to stay open (at least on the side you're meant to enter through).
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

    [HarmonyPostfix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    private static void GeneralState_OnStateEnter(GeneralState __instance) {
        if (
            __instance.name == "[State] Closed" &&
            (__instance.transform.parent?.parent?.parent?.name.Contains("[Mech]BossDoorx6_FSM Variant") ?? false)
        ) {
            var path = GetGOPath(__instance.gameObject);
            Log.Info($"BossArena::GeneralState_OnStateEnter detected a closed Sol arena door: {path}");

            string? doorName = null;
            if (path == "A2_S5_ BossHorseman_GameLevel/Room/Simple Binding Tool/Boss_SpearHorse_Logic/[Mech]BossDoorx6_FSM Variant/--[States]/FSM/[State] Closed") {
                doorName = "Yingzhao arena front door";
            } else if (path == "A2_S5_ BossHorseman_GameLevel/Room/Simple Binding Tool/Boss_SpearHorse_Logic/[Mech]BossDoorx6_FSM Variant (1)/--[States]/FSM/[State] Closed") {
                doorName = "Yingzhao arena back door"; // should never matter, but might as well
            } else if (path == "A3_S5_BossGouMang_GameLevel/Room/[Mech]BossDoorx6_FSM Variant/--[States]/FSM/[State] Closed") {
                doorName = "Goumang arena back door"; // Goumang's arena is often entered from behind, so this one definitely matters
            } else if (path == "A3_S5_BossGouMang_GameLevel/Room/[Mech]BossDoorx6_FSM Variant (1)/--[States]/FSM/[State] Closed") {
                doorName = "Goumang arena front door";
            } else if (path == "A5_S5/Room/EventBinder/[Mech]BossDoorx6_FSM Variant/--[States]/FSM/[State] Closed") {
                doorName = "Jiequan arena door";
            } else if (path == "P2_R22_Savepoint_GameLevel/Room/Prefab/EventBinder (Boss Fight 相關)/[Mech]BossDoorx6_FSM Variant (1)/--[States]/FSM/[State] Closed") {
                doorName = "Fengs arena front door";
            } else if (path == "P2_R22_Savepoint_GameLevel/Room/Prefab/EventBinder (Boss Fight 相關)/[Mech]BossDoorx6_FSM Variant (2)/--[States]/FSM/[State] Closed") {
                doorName = "Fengs arena back door"; // should never matter, but might as well
            } else if (path == "A10S5/Room/Boss And Environment Binder/[Mech]BossDoorx6_FSM Variant 大柱子/--[States]/FSM/[State] Closed") {
                doorName = "Ji arena door";
            }

            if (doorName != null) {
                var openState = __instance.transform.parent.Find("[State] Opened");
                AccessTools.Method(typeof(GeneralState), "OnStateEnter", []).Invoke(openState?.GetComponent<GeneralState>(), []);
                Log.Info($"BossArena::GeneralState_OnStateEnter forced open the {doorName}");
            }
        }
    }
}
