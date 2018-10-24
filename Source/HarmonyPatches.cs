using Harmony;
using System;
using System.Reflection;
using Verse;

namespace ChangeEquipmentStats
{
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = HarmonyInstance.Create("com.ChangeEquipmentStats.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message(
                "ChangeEquipmentStats Harmony Patches:" + Environment.NewLine +
                "  Prefix:" + Environment.NewLine +
                "    GameComponentUtility.StartedNewGame" + Environment.NewLine +
                "    GameComponentUtility.LoadedGame");
        }
    }

    [HarmonyPatch(typeof(GameComponentUtility), "StartedNewGame")]
    static class Patch_GameComponentUtility_StartedNewGame
    {
        static void Postfix()
        {
            Controller.Settings.Init();
        }
    }

    [HarmonyPatch(typeof(GameComponentUtility), "LoadedGame")]
    static class Patch_GameComponentUtility_LoadedGame
    {
        static void Postfix()
        {
            Controller.Settings.Init();
        }
    }
}