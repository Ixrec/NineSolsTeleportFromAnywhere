using HarmonyLib;

namespace TeleportFromAnywhere;

[HarmonyPatch]
internal class RootNodeMenu {
    /*
     * Make the Root Node menu show the [Pavilion]/[Last Node] and [Teleport] buttons even during the sequences where the
     * vanilla game disables teleport. This isn't strictly necessary since you can always leave the root node menu and use the
     * pause Teleport menu instead, but being consistent about this should prevent confusion, especially on the Prison node.
     */

    // in this mod, the tree is always chill
    [HarmonyPostfix, HarmonyPatch(typeof(TreeGotMadCondition), "isValid", MethodType.Getter)]
    public static void TreeGotMadCondition_get_isValid(TreeGotMadCondition __instance, ref bool __result) {
        //Log.Info($"TreeGotMadCondition isValid __result={__result}");
        if (__result == true) {
            Log.Info($"forcing TreeGotMadCondition to pass so the Root Node menus will continue to display [Pavilion]/[Last Node] options");
            __result = false;
        }
    }
    [HarmonyPostfix, HarmonyPatch(typeof(PlayerInPrisionCondition), "isValid", MethodType.Getter)]
    public static void PlayerInPrisionCondition_get_isValid(PlayerInPrisionCondition __instance, ref bool __result) {
        //Log.Info($"PlayerInPrisionCondition isValid __result={__result}");
        if (__result == true) {
            Log.Info($"forcing PlayerInPrisionCondition_get_isValid to return true so the Root Node menus will continue to display [Pavilion]/[Last Node] options");
            __result = false;
        }
    }
    [HarmonyPostfix, HarmonyPatch(typeof(TeleportPointMatchSavePanelCondition), "isValid", MethodType.Getter)]
    public static void TeleportPointMatchSavePanelCondition_get_isValid(TeleportPointMatchSavePanelCondition __instance, ref bool __result) {
        //Log.Info($"TeleportPointMatchSavePanelCondition isValid __result={__result} __instance={__instance.name} parent={__instance.transform.parent?.name}");
        if (__instance.transform.parent?.name == "Teleport 神遊" && __result == false) {
            //Log.Info($"forcing TeleportPointMatchSavePanelCondition on \"Teleport 神遊\" to pass so the non-FSP Root Node menus will also display the [Teleport] option");
            __result = true;
        }
    }
}
