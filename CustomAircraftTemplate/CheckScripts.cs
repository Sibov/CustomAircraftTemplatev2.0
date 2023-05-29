using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomAircraftTemplate
{

    [HarmonyPatch(typeof(LoadoutConfigurator), "AttachImmediate")]
    public class AttachImmediateStartPatch
    {
        public static void Postfix(AttachImmediate __instance, string weaponName, int hpIdx)
        {
            Traverse traverse = Traverse.Create(__instance);


            Main.aircraftMirage = GameObject.Instantiate(Main.aircraftPrefab);
            object LC = traverse.Field("config").GetValue();

            Traverse traverse2 = Traverse.Create(LC);
            traverse2.Field("wm").SetValue(Main.aircraftMirage.GetComponent<WeaponManager>());

            return;

        }
    }
}