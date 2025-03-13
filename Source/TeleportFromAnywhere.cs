using BepInEx;
using HarmonyLib;
using NineSolsAPI;
using System.Collections.Generic;
using System.Linq;

namespace TeleportFromAnywhere;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[HarmonyPatch]
public class TeleportFromAnywhere : BaseUnityPlugin {
    private Harmony harmony = null!;

    private void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);

        // Load patches from any class annotated with @HarmonyPatch
        harmony = Harmony.CreateAndPatchAll(typeof(TeleportFromAnywhere).Assembly);

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void OnDestroy() {
        // Make sure to clean up resources here to support hot reloading

        harmony.UnpatchSelf();
    }

    /*
     * The flagship feature of this mod: Making the "Teleport" tab of the pause menu show up no matter where you are.
     * With the one exception of pre-node Prison to avoid softlocks/unreachable locations.
     */

    [HarmonyPostfix, HarmonyPatch(typeof(TabsUI), "PrepareValidTab")]
    public static void TabsUI_PrepareValidTab_Postfix(TabsUI __instance) {
        var items = AccessTools.FieldRefAccess<TabsUI, List<UITabsItem>>("items").Invoke(__instance);
        Log.Debug($"TabsUI_PrepareValidTab_Postfix {items} {string.Join("|", items.Select(i => i.gameObject.name))}");

        if (items[0].name != "TeleportPanel Tab") {
            // assume this is some other TabsUI instance (e.g. the subtabs for Inventory or Databsae) and leave it alone
            return;
        }

        if (!GameCore.IsAvailable() || !GameFlagManager.IsAvailable()) return;
        var inPrisonArea = SingletonBehaviour<GameCore>.Instance.CurrentSceneName == "A5_S2_Jail_Remake_Final";
        if (inPrisonArea) {
            var prisonNodeUnlocked = SingletonBehaviour<GameFlagManager>.Instance.GetTeleportPointWithPath("28a1908d9e21d4136b8c903e2b92b0afTeleportPointData").unlocked.CurrentValue;
            if (!prisonNodeUnlocked) {
                Log.Info($"TeleportFromAnywhere: Keeping Teleport menu hidden because we're in Prison and its node hasn't been activated yet."
                    + " If the player teleported out now, they'd never be able to get back in.");
                return;
            }
        }

        if (!items[0].IsAllValid) {
            Log.Debug($"TabsUI_PrepareValidTab_Postfix forcing TeleportTab into the list of 'valid' menu tabs");
            var validItems = AccessTools.FieldRefAccess<TabsUI, List<UITabsItem>>("validItems").Invoke(__instance);

            // This part is copied from the vanilla PrepareValidTab() impl
            validItems.Add(items[0]);
            items[0].gameObject.SetActive(true);
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(TeleportPointButton), "SubmitImplementation")]
    public static bool TeleportPointButton_SubmitImplementation(TeleportPointButton __instance) {
        Log.Info($"TeleportPointButton_SubmitImplementation called for button '{__instance?.name}'");

        if (!GameCore.IsAvailable() || !ApplicationUIGroupManager.IsAvailable()) return true;
        var savePointGO = SingletonBehaviour<GameCore>.Instance?.savePanelUiController?.CurrentSavePointGameObjectOnScene;
        if (savePointGO != null) {
            // This is a "normal" teleport, with Yi sitting down at the Pavilion root node
            return true; // let the vanilla implementation handle it
        }

        Log.Info($"Yi is not sitting at the Pavilion root node. Using mod impl of teleportation to avoid errors.");
        // The following is copy-pasted from the vanilla SubmitImplementation() impl, with the parts that can't work removed
        SingletonBehaviour<GameCore>.Instance.savePanelUiController.ToMenu = false;
        SingletonBehaviour<ApplicationUIGroupManager>.Instance.PopAll();
        SingletonBehaviour<GameCore>.Instance.savePanelUiController.ClearCurrentSavePoint();
        SingletonBehaviour<GameCore>.Instance.SetReviveSavePoint(__instance?.teleportPoint);
        SingletonBehaviour<GameCore>.Instance.TeleportToSavePoint(__instance?.teleportPoint);

        return false; // skip vanilla implementation
    }

    /*
     * Allow teleporting to your "current node", since this mod breaks the assumption that you must already be there.
     */

    // When you press a disabled UIControlButton, this IsNotAcquired is what's checked to prevent it from doing anything.
    // Thus, overriding this getter is how you forcibly re-enable a disabled UIControlButton.
    [HarmonyPostfix, HarmonyPatch(typeof(TeleportPointButton), "IsNotAcquired", MethodType.Getter)]
    public static void TeleportPointButton_get_IsNotAcquired(TeleportPointButton __instance, ref bool __result) {
        if (__result == true) {
            Log.Info($"TeleportPointButton_get_IsNotAcquired enabling '{__instance?.name}' button, so the player can teleport there despite it being their 'current node'");
            __result = false;
        }
    }

    // Also show the "[Z] Teleport" prompt on the current node, by preventing the TPB from swapping InstructionData with exitInstruction
    [HarmonyPrefix, HarmonyPatch(typeof(TeleportPointButton), "GetInstructionData")]
    public static bool TeleportPointButton_GetInstructionData(TeleportPointButton __instance, ref ButtonInstructionData __result) {
        // The vanilla impl is "return base.GetInstructionData() unless PlayerIsHere", we want just "base.GetInstructionData()", and
        // UIControlButton::GetInstructionData() just returns UIControlButton.InstructionData so that's the easiest thing to do.
        __result = AccessTools.FieldRefAccess<UIControlButton, ButtonInstructionData>("InstructionData").Invoke(__instance);
        return false;
    }

    /*
     * Finally, make the Root Node menu show the [Pavilion]/[Last Node] and [Teleport] buttons even during the sequences where the
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
            Log.Info($"forcing TeleportPointMatchSavePanelCondition on \"Teleport 神遊\" to pass so the non-FSP Root Node menus will also display the [Teleport] option");
            __result = true;
        }
    }
}