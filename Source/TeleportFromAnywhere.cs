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
}