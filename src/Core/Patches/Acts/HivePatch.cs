using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using EXLagavulin.Core.Models.Encounters;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;

namespace EXLagavulin.Patches.Acts
{
    [HarmonyPatch(typeof(Hive), nameof(Hive.GenerateAllEncounters))]
    public static class HivePatch
    {
        static void Postfix(ref IEnumerable<EncounterModel> __result)
        {
            GD.Print(
                $"Contains Encounter: " +
                ModelDb.Contains(typeof(LagavulinMatriarchAct2Boss))
            );

            GD.Print(
                $"Encounter Id: " +
                ModelDb.Encounter<LagavulinMatriarchAct2Boss>().Id
            );

            __result = __result
                .Concat(new[] { ModelDb.Encounter<LagavulinMatriarchAct2Boss>() })
                .Distinct();
        }
    }
}