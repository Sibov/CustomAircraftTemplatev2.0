using Harmony;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Reflection;
using System.IO;
using Valve.Newtonsoft.Json;
using Harmony;
using TMPro;
using Rewired.Platforms;
using Rewired.Utils;
using Rewired.Utils.Interfaces;

namespace CustomAircraftTemplate
{

    public static class MirageSpecific
    {

        public static void SetupArmingText()
        {
            
            Debug.unityLogger.logEnabled = Main.logging;
            Debug.Log("Setup Arming Text");

        }
    }

    //Fixes Radar so it can see ground units, it applies an RCS to all units
    [HarmonyPatch(typeof(Radar), "ProcessUnit")]
    public static class PatchRadarProcessingForGroundAttack
    {
        public static RaycastHit raycastHit;

        public static bool Prefix(Radar __instance, Actor a, float dotThresh, bool hasMapGen)
        {
            Debug.unityLogger.logEnabled = Main.logging;

            if (!a || !a.gameObject.activeSelf || a.name == "Enemy Infantry MANPADS" || a.name == "Enemy Infantry" || a.name == "Allied Infantry MANPADS" || a.name == "Allied Infantry")
            {
                return false;
            }
            if (a.finalCombatRole == Actor.Roles.Air || a.role == Actor.Roles.GroundArmor || a.role == Actor.Roles.Ground)
            {
                Debug.Log("Radar found: " + a.actorName);
                if (!__instance.detectAircraft)
                {
                    return false;

                }

            }
            else if (a.role == Actor.Roles.Missile)
            {
                if (!__instance.detectMissiles)
                {
                    return false;
                }
            }
            else
            {
                if (a.finalCombatRole != Actor.Roles.Ship)
                {
                    return false;
                }
                if (!__instance.detectShips)
                {
                    return false;
                }
            }
            if (!a.alive)
            {
                return false;
            }
            Vector3 position = a.position;
            float sqrMagnitude = (position - __instance.rotationTransform.position).sqrMagnitude;



            if (sqrMagnitude >= 150000 && !Radar.ADV_RADAR)
            {
                return false;
            }
            Vector3 vector = __instance.rotationTransform.InverseTransformPoint(position);
            vector.y = 0f;
            if (Vector3.Dot(vector.normalized, Vector3.forward) < dotThresh)
            {
                return false;
            }
            Quaternion localRotation = __instance.rotationTransform.localRotation;
            float y = VectorUtils.SignedAngle(__instance.rotationTransform.parent.forward, Vector3.ProjectOnPlane(position - __instance.rotationTransform.position, __instance.rotationTransform.parent.up), __instance.rotationTransform.right);
            __instance.rotationTransform.localRotation = Quaternion.Euler(0f, y, 0f);
            if (Vector3.Dot((position - __instance.radarTransform.position).normalized, __instance.radarTransform.forward) > 0.32)
            {
                Traverse traverseT1 = Traverse.Create(__instance);
                bool myChunkColliderEnabledPatched = (bool)traverseT1.Field("myChunkColliderEnabled").GetValue();


                bool flag = !hasMapGen || VTMapGenerator.fetch.IsChunkColliderEnabled(a.position);
                //RaycastHit raycastHit;
                if (myChunkColliderEnabledPatched && Physics.Linecast(__instance.radarTransform.position, position, out raycastHit, 1) && (raycastHit.point - position).sqrMagnitude > 10000f)
                {
                    __instance.rotationTransform.localRotation = localRotation;
                    return false;
                }
                if (flag && Physics.Linecast(position, __instance.radarTransform.position, out raycastHit, 1) && (raycastHit.point - __instance.radarTransform.position).sqrMagnitude > 10000f)
                {
                    Hitbox component = raycastHit.collider.GetComponent<Hitbox>();
                    if (!component || component.actor != a)
                    {
                        __instance.rotationTransform.localRotation = localRotation;
                        return false;
                    }


                }
                if (hasMapGen && (!myChunkColliderEnabledPatched || !flag))
                {
                    __instance.StartCoroutine(__instance.HeightmapOccludeCheck(a));
                    __instance.rotationTransform.localRotation = localRotation;
                    return false;
                }
                Radar.SendRadarDetectEvent(a, __instance.myActor, __instance.radarSymbol, __instance.detectionPersistanceTime, __instance.rotationTransform.position, __instance.transmissionStrength);
                if (Radar.ADV_RADAR)
                {
                    float radarSignalStrength = Radar.GetRadarSignalStrength(__instance.radarTransform.position, a);
                    float num = __instance.transmissionStrength * radarSignalStrength / sqrMagnitude;
                    if (num < 1f / __instance.receiverSensitivity)
                    {

                        __instance.rotationTransform.localRotation = localRotation;
                        return false;
                    }


                }
                __instance.DetectActor(a);
            }
            __instance.rotationTransform.localRotation = localRotation;
            return false;

        }
    }

    // This Patch is for Ground Radar for Mirage - Remove if no Ground Radar Needed
    [HarmonyPatch(typeof(Actor), "Awake")]
    public class UnitSpawnRCSPatch
    {
        public static void Postfix(Actor __instance)
        {
            Debug.unityLogger.logEnabled = Main.logging;
            Debug.Log("RCS Patch: " + __instance.name);
            Debug.Log("RCS Patch: 1.1 ");



            Debug.Log("RCS Patch: 1.3 ");


            Debug.Log("No RCS");
            GameObject UnitGO = __instance.gameObject;
            Debug.Log("No RCS 1");
            RadarCrossSection RCSforUnit = UnitGO.GetComponent<RadarCrossSection>();
            if (RCSforUnit == null)
            {
                RadarCrossSection UnitRCS = UnitGO.AddComponent<RadarCrossSection>();
                Debug.Log("No RCS 2");
                UnitRCS.weaponManager = UnitGO.GetComponent<WeaponManager>();

                Debug.Log("No RCS 3");
                UnitRCS.size = 7f;
                Debug.Log("No RCS 4");
                UnitRCS.overrideMultiplier = 1f;
                Debug.Log("No RCS 5");

                List<RadarCrossSection.RadarReturn> UnitRCSReturns = new List<RadarCrossSection.RadarReturn>();
                Debug.Log("No RCS 6");
                UnitRCSReturns = Main.aircraftMirage.GetComponent<RadarCrossSection>().returns;
                UnitRCS.enabled = true;
            }

            return;
        }

    }

}




