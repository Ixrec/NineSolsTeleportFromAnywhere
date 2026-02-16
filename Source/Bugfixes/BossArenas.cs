using HarmonyLib;

namespace TeleportFromAnywhere.Bugfixes;

[HarmonyPatch]
internal class BossArenas {
    // Ji's arena has a door that locks behind you and doesn't immediately open after the fight. In vanilla it opens after you
    // retrieve the seal from the vital sanctum, but TFA can bypass that and leave you trapped outside the closed door.
    // So we want to force that door to stay open.
    [HarmonyPostfix, HarmonyPatch(typeof(GeneralState), "OnStateEnter")]
    private static void GeneralState_OnStateEnter(GeneralState __instance) {
        if (
            __instance.name == "[State] Closed" &&
            __instance.transform.parent?.parent?.parent?.name == "[Mech]BossDoorx6_FSM Variant 大柱子"
        ) {
            Log.Info($"GeneralState_OnStateEnter forcing the Ji arena door open");
            var openState = __instance.transform.parent.Find("[State] Opened");
            AccessTools.Method(typeof(GeneralState), "OnStateEnter", []).Invoke(openState?.GetComponent<GeneralState>(), []);
        }
    }
}
