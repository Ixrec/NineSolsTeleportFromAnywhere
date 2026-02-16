using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleportFromAnywhere.Bugfixes;

[HarmonyPatch]
internal class OperaTheater {
    /*
     * The opera theater in EDLA closes its doors and starts playing the opera hologram when Yi walks into it.
     * Normally the doors open and the opera stops playing only after Yi has completed Fuxi's vital sanctum.
     * 
     * Since TFA makes it possible to teleport out of the theater while the opera is still playing, the opera
     * can get "stuck on", preventing you from getting back inside. The opera hologram also comes with a darkening
     * effect on nearby rooms, which makes these rooms annoying to navigate if you're outside with the opera stuck on.
     * 
     * So we try to detect this corner case and "turn off the opera".
     */

    [HarmonyPrefix, HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Awake))]
    private static void GameLevel_Awake(GameLevel __instance) {
        var levelName = __instance.name;
        // if we're not in EDLA, don't waste any more time in this patch
        if (levelName != "A9_S2")
            return;

        // if we're coming out of Fuxi's vital sanctum, don't mess with anything; this means we're going through the opera sequence as intended
        if (SingletonBehaviour<GameCore>.Instance.PreviousScene == "VR_Memory_伏羲")
            return;

        var flagDict = SingletonBehaviour<SaveManager>.Instance.allFlags.FlagDict;
        var operaStartedFlag = (ScriptableDataBool)flagDict["1e9feef9-1dc2-4f83-9554-d32780aebfd0_348738fb5673845a6aa8b023a95252cfScriptableDataBool"];
        var fuxiSanctumDoneFlag = (ScriptableDataBool)flagDict["708d79555a8b54472988d75ac5ba8823ScriptableDataBool"];

        if (operaStartedFlag.CurrentValue && !fuxiSanctumDoneFlag.CurrentValue) {
            Log.Info($"OperaTheater GameLevel_Awake detected the EDLA theater is stuck playing the opera and darkening nearby rooms. " +
                $"Turning off the opera so you can get back inside, and see where you're going in nearby rooms.");
            operaStartedFlag.CurrentValue = false;
        }
    }
}
