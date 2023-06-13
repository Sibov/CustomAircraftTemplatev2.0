using Harmony;
using ModLoader.Classes;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VTOLVR.Multiplayer;
using static Rewired.UI.ControlMapper.ControlMapper;
using Debug = UnityEngine.Debug;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;

namespace CustomAircraftTemplateTejas
{
    //You should not need to edit any of this code!

    public class PilotSelectPatch : MonoBehaviour
    {


        public static void SetNVGShader()
        {
            GameObject helmetCamera = AircraftAPI.GetChildWithName(Main.aircraftCustom, "Camera (eye) Helmet", false);
            ScreenMaskedColorRamp smcr = helmetCamera.GetComponent<ScreenMaskedColorRamp>();
            Shader NVGShader = Shader.Find("Hidden/Grayscale Effect NVG");
            smcr.shader = NVGShader;


        }

        


    }


    [HarmonyPatch(typeof(BlackoutEffect), nameof(BlackoutEffect.LateUpdate))]
    public class BlackoutPatchPost
    {
        private static bool Prefix(BlackoutEffect __instance)
        {

            
            

            Traverse trav1 = Traverse.Create(__instance);
            float num = Mathf.Abs((float)trav1.Field("gAccum").GetValue()) * __instance.aFactor;
            trav1.Field("alpha").SetValue(Mathf.Lerp((float)trav1.Field("gAccum").GetValue(), num, 20f * Time.deltaTime));
            //Color32 alphaCol = __instance.quadRenderer.GetComponent<MeshRenderer>().material.GetColor("_Cutoff");
            //Debug.Log("BPP 1.0:" + alphaCol);
            float newAlpha =  (float)trav1.Field("alpha").GetValue();
            //Debug.Log("BPP 1.0:" + newAlpha);
            if (newAlpha < 0.001f)
            {
                __instance.quadRenderer.enabled = false;

                return false; }

            __instance.quadRenderer.enabled= true;
            if (newAlpha > 1.0f) { newAlpha= 1.0f; }
            if (newAlpha < -0.000001f) { newAlpha = 0.0f; }


            //Debug.Log("BPP 1.1:" + newAlpha);
            Color colorNew = new Color(0f, 0f, 0f, newAlpha);
            __instance.quadRenderer.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_Color", colorNew);
            if (!__instance.accelDied && __instance.useFlightInfo && __instance.flightInfo && __instance.flightInfo.maxInstantaneousG > __instance.instantaneousGDeath)
            {
                FlightLogger.Log("Died by instantaneous G-force (" + __instance.flightInfo.maxInstantaneousG.ToString("0.0") + ")");
                __instance.AccelDie();
            }
            return false;
        }

        }

    //    [HarmonyPatch(typeof(BlackoutEffect), nameof(BlackoutEffect.Start))]
    //public class BlackoutPatch
    //{
    //    private static bool Prefix(BlackoutEffect __instance)
    //    {
    //        Debug.Log("BProfile 1.0");
    //        Traverse trav1 = Traverse.Create(__instance);
    //        Debug.Log("BProfile 1.2");
    //        trav1.Field("images").SetValue(__instance.GetComponentsInChildren<Image>());
    //        Debug.Log("BProfile 1.1");
    //        if (__instance.quadRenderer)
    //        {
    //            Debug.Log("BProfile 1.3");
    //            Image[] array = (Image[])trav1.Field("images").GetValue();
    //            for (int i = 0; i < array.Length; i++)
    //            {
    //                Debug.Log("BProfile 1.4");
    //                array[i].gameObject.SetActive(false);
    //            }
    //           MaterialPropertyBlock mat = new MaterialPropertyBlock();
    //            Debug.Log("BProfile 1.5");
    //            trav1.Field("quadProps").SetValue(mat);
    //            Debug.Log("BProfile 1.6");
    //            trav1.Field("colorID").SetValue(Shader.PropertyToID("_TintColor"));
    //        }
    //        trav1.Field("nvg").SetValue(__instance.GetComponentInParent<NightVisionGoggles>());
    //        if (!__instance.flightInfo)
    //        {
    //            __instance.flightInfo = __instance.GetComponentInParent<FlightInfo>();
    //        }
    //        VehicleMaster componentInParent = __instance.GetComponentInParent<VehicleMaster>();
    //        if (componentInParent)
    //        {
    //            componentInParent.OnPilotDied += __instance.AccelDie;
    //        }

    //        return false;
    //    }
    //}


    [HarmonyPatch(typeof(VTResources), nameof(VTResources.LoadExternalVehicle))]
    public class OkuPatch
    {
        private static bool Prefix()
        {
            var t = Traverse.Create(typeof(VTResources));
            t.Field<bool>("canLoadExternalVehicles").Value = true;

            return true;
        }
    }









    [HarmonyPatch(typeof(PlayerSpawn), nameof(PlayerSpawn.PlayerSpawnRoutine))]
    public class PlayerSpawnSelectedVehicleinScenario
    {
        private static bool Prefix()
        {
            if (PilotSaveManager.currentVehicle.vehicleName != AircraftInfo.vehicleName) { return true; }
            VTScenario.current.vehicle = PilotSaveManager.currentVehicle;
            Main.aircraftCustom = FlightSceneManager.instance.playerActor.gameObject;
             return true;
                        
        }
        
    }
    [HarmonyPatch(typeof(MultiplayerSpawn), nameof(MultiplayerSpawn.PlayerSpawnRoutine))]
    public class MPlayerSpawnSelectedVehicleinScenario
    {
        private static bool Prefix()
        {
            if (PilotSaveManager.currentVehicle.vehicleName != AircraftInfo.vehicleName) { return true; }
           // VTScenario.current.vehicle = PilotSaveManager.currentVehicle;
            Main.aircraftCustom = FlightSceneManager.instance.playerActor.gameObject;
            return true;

        }

    }

    [HarmonyPatch(typeof(PilotSelectUI), nameof(PilotSelectUI.StartSelectedPilotButton))]
    public class StartPilot
    {
        private static bool Prefix(PilotSelectUI __instance)
        {
            //Debug.Log("Tejas StartPilot Patch 1.0");
            var traverse = Traverse.Create(__instance);
            traverse.Field("vehicles").SetValue(PilotSaveManager.GetVehicleList()); 
                return true;
        }
    }

    [HarmonyPatch(typeof(PlayerSpawn), nameof(PlayerSpawn.OnPreSpawnUnit))]
    public class OnPreSpawnSelectedVehicleinScenario
    {
        private static bool Prefix()
        {
            //if (PilotSaveManager.currentVehicle.vehicleName != AircraftInfo.vehicleName) { return true; }
            VTScenario.current.vehicle = PilotSaveManager.currentVehicle;
            return true;
        }
    }
    [HarmonyPatch(typeof(LoadoutConfigurator), nameof(LoadoutConfigurator.Initialize))]
    public class LoadInAircraftsWeapons
    {
       

        public static void Prefix(LoadoutConfigurator __instance)
        {
            if (PilotSaveManager.currentVehicle.vehicleName != AircraftInfo.vehicleName) { return; }
            __instance.availableEquipStrings.Clear();
            //Debug.Log("Tejas LIAWPF  1.0");
            //Debug.Log("Tejas LIAWPF  1.2");
            foreach (string item2 in PilotSaveManager.currentVehicle.GetEquipNamesList())
            {
                //Debug.Log("Tejas LIAWPF  1.3" + item2);
                //if (!(VTScenario.currentScenarioInfo.gameVersion > new GameVersion(1, 3, 0, 30, GameVersion.ReleaseTypes.Testing)) || VTScenario.currentScenarioInfo.allowedEquips.Contains(item2))
                {
                    //Debug.Log("Tejas LIAWPF  1.4" + item2);
                    __instance.availableEquipStrings.Add(item2);
                   
                }
            }
            //Debug.Log("Tejas LIAWPF  1.5");
            
        }

    }
    [HarmonyPatch(typeof(AuxilliaryPowerUnit), "SetPower")]
    public class Tejas_AuxilliaryPowerUnit_post
    {
        public static void Postfix(AuxilliaryPowerUnit __instance)
        {

            //Debug.Log("Tejas  1.27");
            CustomElements.SetUpGauges();
            PilotSelectPatch.SetNVGShader();
           

            //Debug.Log("Tejas  1.28");

        }
    }

    [HarmonyPatch(typeof(VehicleConfigSceneSetup), nameof(VehicleConfigSceneSetup.Start))]
    public class VehicleConfigSceneSetupSettingforAllWeapons
    {
        private static bool Prefix()
        {
            if (PilotSaveManager.currentVehicle.vehicleName != AircraftInfo.vehicleName) { return true; }
            PilotSaveManager.currentCampaign.isStandaloneScenarios = false ;
            return true;
        }
    }


    [HarmonyPatch(typeof(VTResources), nameof(VTResources.LoadCustomCampaignAtPath))]
    class tejas_CSIPatch_LoadVTEditorCustomCampaigns
    {
        static bool Prefix(VTResources __instance, string filePath, ref bool skipUnmodified)
        {


            skipUnmodified = false;
            return true;
        }
    }



    

    [HarmonyPatch(typeof(CampaignSelectorUI), nameof(CampaignSelectorUI.ViewCampaign))]
    public class CampaignScreenCreatorIfNoCampaigns
    {
        private static bool Prefix(CampaignSelectorUI __instance)
        {
            
            Traverse trav1 = Traverse.Create(__instance);
            int cIDX = (int) trav1.Field("campaignIdx").GetValue();
            //Debug.Log("cIDX = " + cIDX);
            if (cIDX == -1) 
            { trav1.Field("campaignIdx").SetValue(0);

                 __instance.StartCoroutine(CSIPatchUtils.BetterSetupCampaignScreenRoutine(__instance));
                return false;
            }

           
            return true;
        }
    }



    public static class CSIPatchUtils
    {
        public static Campaign pubCampaign;

        /** Executes more iterations of the campaign select loop to add other planes' campaigns to our aircraft.
*	>> It isn't exactly "better" yet, just gives us more flexibility to control it. Possible todo: make better
*/
        public static IEnumerator BetterSetupCampaignScreenRoutine(CampaignSelectorUI instance)
        {
            //Debug.unityLogger.logEnabled = Main.logging;
            //Debug.Log("Tejas BSCSCR Patch 1.0");
            var traverse = Traverse.Create(instance);

            instance.loadingCampaignScreenObj.SetActive(true);
            //Debug.Log("Tejas BSCSCR Patch 1.1");
            bool wasInputEnabled = !ControllerEventHandler.eventsPaused;
            ControllerEventHandler.PauseEvents();
            VTScenarioEditor.returnToEditor = false;
            VTMapManager.nextLaunchMode = VTMapManager.MapLaunchModes.Scenario;
            PlayerVehicleSetup.godMode = false;
            //Debug.Log("Tejas BSCSCR Patch 1.2");
            instance.campaignDisplayObject.SetActive(true);
            instance.scenarioDisplayObject.SetActive(false);
            var _campaignsParent = (Transform)traverse.Field("campaignsParent").GetValue();
            //Debug.Log("Tejas BSCSCR Patch 1.3");
            if (_campaignsParent)
            {
                //Debug.Log("Tejas BSCSCR Patch 1.4");
                Object.Destroy(_campaignsParent.gameObject);
            }
            _campaignsParent = new GameObject("campaigns").transform;
            _campaignsParent.parent = instance.campaignTemplate.transform.parent;
            _campaignsParent.localPosition = instance.campaignTemplate.transform.localPosition;
            _campaignsParent.localRotation = Quaternion.identity;
            _campaignsParent.localScale = Vector3.one;
            //Debug.Log("Tejas BSCSCR Patch 1.5");
            traverse.Field("campaignsParent").SetValue(_campaignsParent);
            var _campaignWidth = ((RectTransform)instance.campaignTemplate.transform).rect.width;
            traverse.Field("campaignWidth").SetValue(_campaignWidth);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var _campaigns = new List<Campaign>();
            //Debug.Log("Tejas BSCSCR Patch 1.6" + PilotSaveManager.currentVehicle.vehicleName);
            

            /* --- BEGIN EDIT --- */
            /* THIS IS WHERE THE MAGIC HAPPENS, STEP BACK AND BEHOLD */
            var _aircraft = new List<string> { PilotSaveManager.currentVehicle.vehicleName };
            //Debug.Log("Tejas BSCSCR Patch 1.6.0.1" + _aircraft[0]);

            {
                //Debug.Log("Tejas BSCSCR Patch 1.6.1");
                _aircraft.Add("F/A-26B");


            }
            //Debug.Log("Tejas BSCSCR Patch 1.7");
            foreach (var vName in _aircraft)
            {
                //Debug.Log("Tejas BSCSCR Patch 1.8: " + vName);
                foreach (var vtcampaignInfo in VTResources.GetBuiltInCampaigns()
                             .Where(vtcampaignInfo => vtcampaignInfo.vehicle == vName && !vtcampaignInfo.hideFromMenu))
                {
                    //Debug.Log("Tejas BSCSCR Patch 1.9: " + vtcampaignInfo.campaignName);

                    Campaign c = vtcampaignInfo.ToIngameCampaignAsync(instance, out var bdcoroutine);
                    yield return bdcoroutine;
                    //Debug.Log("Tejas BSCSCR Patch 1.10: ");

                    _campaigns.Add(c);
                }
            }
            /* ---- END EDIT ---- */
            sw.Stop();
            //Debug.Log("Tejas [CSIPatch] Time loading BuiltInCampaigns: " + sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            //Debug.Log("Tejas BSCSCR Patch 1.11");

            foreach (var vName in _aircraft)
            {
                //Debug.Log("Tejas BSCSCR Patch 1.11.1: " + vName);
                if (PilotSaveManager.GetVehicle(vName).campaigns != null)
                {
                    foreach (var campaign in PilotSaveManager.GetVehicle(vName).campaigns)
                    {
                        //Debug.Log("Tejas BSCSCR Patch 1.12: " + campaign + ", " + vName);

                        if (campaign.isSteamworksStandalone)
                        {
                            //Debug.Log("Tejas BSCSCR Patch 1.13");
                            _campaigns.Add(campaign);
                            if (!SteamClient.IsValid) continue;
                            pubCampaign = campaign;
                            traverse.Field("swStandaloneCampaign").SetValue(campaign);
                            //Debug.Log("Tejas BSCSCR Patch 1.14");

                            campaign.campaignName = (string)traverse.Field("campaign_ws").GetValue();
                            campaign.description = (string)traverse.Field("campaign_ws_description").GetValue();
                            //Debug.Log("Tejas BSCSCR Patch 1.15");

                        }
                        else if (campaign.readyToPlay)
                        {
                            //Debug.Log("Tejas BSCSCR Patch 1.16");

                            _campaigns.Add(campaign);
                            if (!campaign.isCustomScenarios || !campaign.isStandaloneScenarios ||
                                campaign.isSteamworksStandalone) continue;
                            //Debug.Log("Tejas BSCSCR Patch 1.17");

                            campaign.campaignName = (string)traverse.Field("campaign_customScenarios").GetValue();
                            campaign.description = (string)traverse.Field("campaign_customScenarios_description").GetValue();
                            //Debug.Log("Tejas BSCSCR Patch 1.18");

                        }
                    }
                }
            }
            sw.Stop();
            //Debug.Log("Tejas [CSIPatch] Time loading vehicle campaigns: " + sw.ElapsedMilliseconds.ToString());
            sw.Reset();
            sw.Start();
            VTResources.GetCustomCampaigns();
            sw.Stop();
            //Debug.Log("Tejas [CSIPatch] Time loading custom campaigns list: " + sw.ElapsedMilliseconds.ToString());
            sw.Reset();
            sw.Start();

            /* --- BEGIN EDIT --- */
            /* MORE MAGIC IS HAPPENING, STEP BACK AND BEHOLD */
            foreach (var vName in _aircraft)
            {
                //Debug.Log("Tejas BSCSCR Patch 1.19: " + vName);

                foreach (var vtcampaignInfo2 in VTResources.GetCustomCampaigns()
                             .Where(vtcampaignInfo2 => vtcampaignInfo2.vehicle == vName && !vtcampaignInfo2.multiplayer))
                {
                    //Debug.Log("Tejas BSCSCR Patch 1.20: " + vtcampaignInfo2.campaignName);

                    Campaign c = vtcampaignInfo2.ToIngameCampaignAsync(instance, out var bdcoroutine2);
                    yield return bdcoroutine2;
                    //Debug.Log("Tejas BSCSCR Patch 1.21");
                    _campaigns.Add(c);
                    c = null;
                }
            }


            /* ---- END EDIT ---- */

            sw.Stop();
            //Debug.Log("Tejas [CSIPatch] Time converting custom campaigns ToIngameCampaigns: " + sw.ElapsedMilliseconds);
            sw.Reset();
            int num;
            //Debug.Log("Tejas BSCSCR Patch 1.22");
            for (var i = 0; i < _campaigns.Count; i = num + 1)
            {
                //Debug.Log("Tejas BSCSCR Patch 1.23");
                var gameObject = Object.Instantiate<GameObject>(instance.campaignTemplate, _campaignsParent);
                gameObject.transform.localPosition += _campaignWidth * (float)i * Vector3.right;
                //Debug.Log("Tejas BSCSCR Patch 1.24");
                var component = gameObject.GetComponent<CampaignInfoUI>();
                component.campaignImage.texture = instance.noImage;
                //Debug.Log("Tejas BSCSCR Patch 1.25");
                component.UpdateDisplay(_campaigns[i], PilotSaveManager.currentVehicle.vehicleName);
                gameObject.SetActive(true);
                //Debug.Log("Tejas BSCSCR Patch 1.26");
                yield return null;
                num = i;
            }
            var _campaignIdx = (int)traverse.Field("campaignIdx").GetValue();
            //Debug.Log("Tejas BSCSCR Patch 1.27");
            _campaignIdx = Mathf.Clamp(_campaignIdx, 0, _campaigns.Count - 1);
            traverse.Field("campaignIdx").SetValue(_campaignIdx);
            instance.campaignTemplate.SetActive(false);
            //Debug.Log("Tejas BSCSCR Patch 1.28");

            traverse.Field("campaigns").SetValue(_campaigns);

            var setupList = typeof(CampaignSelectorUI).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (setupList == null)
            {
                //Debug.Log("Tejas BSCSCR Patch 1.28.1");
            }
            var setupCampaignList = typeof(CampaignSelectorUI).GetMethod("SetupCampaignList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //Debug.Log("Tejas BSCSCR Patch 1.29");
            //instance.SetupCampaignList(); // private method invoke required
            setupCampaignList.Invoke(instance, null);
            //Debug.Log("Tejas BSCSCR Patch 1.30");
            instance.loadingCampaignScreenObj.SetActive(false);
            //Debug.Log("Tejas BSCSCR Patch 1.31");
            //if (wasInputEnabled)
            {
                //Debug.Log("Tejas BSCSCR Patch 1.31.1");

                ControllerEventHandler.UnpauseEvents();
            }
            //Debug.Log("Tejas BSCSCR Patch 1.32");
        }
    }

    [HarmonyPatch(typeof(VTResources), nameof(VTResources.LoadCustomCampaignAtPath))]
    class Tejas_CSIPatch_LoadVTEditorCustomCampaigns
    {
        static bool Prefix(VTResources __instance, string filePath, ref bool skipUnmodified)
        {

          
            skipUnmodified = false;
            return true;
        }
    }

        [HarmonyPatch(typeof(CampaignSelectorUI), "FinallyOpenCampaignSelector")]
    public static class Tejas_CSIPatch_FinallyOpenCampaignSelector
    {

        static CampaignSelectorUI instance;
        private static string String1;

        /** Replica of StartWorkshopCampaignRoutine. */
        static IEnumerator CSI_FinallyOpenCampaignSelectorRoutine(Action onOpenedSelector)
        {
            //Debug.unityLogger.logEnabled = Main.logging;
            //Debug.Log("Tejas CSI Patch 1.1");
            var traverse = Traverse.Create(instance);
            instance.campaignDisplayObject.SetActive(true);
            instance.missionBriefingDisplayObject.SetActive(false);
            //Debug.Log("Tejas CSI Patch 1.2");
            yield return instance.StartCoroutine(CSIPatchUtils.BetterSetupCampaignScreenRoutine(instance));
            //Debug.Log("Tejas CSI Patch 1.2.1.1");
            if (onOpenedSelector != null)
            {
                //Debug.Log("Tejas CSI Patch 1.3");
                onOpenedSelector();
                yield break;
            }
            //Debug.Log("Tejas CSI Patch 1.2.1.2");
            try
            {
                //Debug.Log("Tejas CSI Patch 1.2.1.2.1");
                String1 = PilotSaveManager.currentCampaign.campaignName;
                //Debug.Log("Tejas CSI Patch 1.2.1.2.2");
            }
            
            catch (NullReferenceException)
            {
                //Debug.Log("Tejas CSI Patch 1.2.1.2.4");
            

                    
                var _campaignslist = (List<Campaign>)traverse.Field("campaigns").GetValue();
                PilotSaveManager.currentCampaign = _campaignslist[0];
                }

            //Debug.Log("Tejas CSI Patch 1.2.1.3");
            if (String1 == null) { yield break; }
            //Debug.Log("Tejas CSI Patch 1.2.1" + PilotSaveManager.currentCampaign.campaignName);
            if (!PilotSaveManager.currentCampaign) yield break;
            //Debug.Log("Tejas CSI Patch 1.4");
            var lastCSave = PilotSaveManager.current.lastVehicleSave.GetCampaignSave(PilotSaveManager.currentCampaign.campaignID);
            var _campaigns = (List<Campaign>)traverse.Field("campaigns").GetValue();
            var num = _campaigns.FindIndex(x => x.campaignID == PilotSaveManager.currentCampaign.campaignID);
            //Debug.Log("Tejas CSI Patch 1.5");
            if (num >= 0)
            {
                //Debug.Log("Tejas CSI Patch 1.6");

                PilotSaveManager.currentCampaign = _campaigns[num];
                var _campaignIdx = traverse.Field("campaignIdx");
                //this.campaignIdx = num;
                _campaignIdx.SetValue(num);
                //instance.ViewCampaign(num); // private method invoke
                var viewCampaign = instance.GetType().GetMethod("ViewCampaign", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                //Debug.Log("Tejas CSI Patch 1.7");
                viewCampaign.Invoke(instance, new System.Object[] { num });

                instance.SelectCampaign();
                if (PilotSaveManager.currentScenario == null) yield break;
                //Debug.Log("Tejas CSI Patch 1.8");
                var _viewingCampaign = (Campaign)traverse.Field("viewingCampaign").GetValue();
                while (_viewingCampaign == null)
                {
                    //Debug.Log("Tejas CSI Patch 1.9");
                    _viewingCampaign = (Campaign)traverse.Field("viewingCampaign").GetValue();
                    yield return null;
                }
                var num2 = _campaigns[(int)_campaignIdx.GetValue()].missions
                    .FindIndex(x => PilotSaveManager.currentScenario.scenarioID == x.scenarioID);
                if (num2 >= 0)
                {
                    //Debug.Log("Tejas CSI Patch 1.10" + num2);
                    //this.missionIdx = num2;
                    traverse.Field("missionIdx").SetValue(num2);
                    //Debug.Log("Tejas CSI Patch 1.10.1");
                    instance.MissionsButton();
                    //Debug.Log("Tejas CSI Patch 1.10.2");
                }
                else
                {
                    //Debug.Log("Tejas CSI Patch 1.11");
                    var numa = _campaigns[(int)_campaignIdx.GetValue()];
                    //Debug.Log("Tejas CSI Patch 1.11.0");
                    num2 = numa.trainingMissions
                        .FindIndex(x => PilotSaveManager.currentScenario.scenarioID == x.scenarioID);
                    //Debug.Log("Tejas CSI Patch 1.11.1" + num2) ;
                    if (num2 < 0) yield break;
                    //this.trainingIdx = num2;
                    //Debug.Log("Tejas CSI Patch 1.12");
                    traverse.Field("trainingIdx").SetValue(num2);
                    instance.TrainingButton();
                }
            }

            else if ((bool)traverse.Field("openedWorkshopCampaign").GetValue()) //CampaignSelectorUI.openedWorkshopCampaign
            {
                //Debug.Log("Tejas CSI Patch 1.13");
                var _loadedWorkshopCampaigns = (List<Campaign>)traverse.Field("loadedWorkshopCampaigns").GetValue();
                using (var enumerator = _loadedWorkshopCampaigns.GetEnumerator())
                {
                    //Debug.Log("Tejas CSI Patch 1.14");
                    while (enumerator.MoveNext())
                    {
                        //Debug.Log("Tejas CSI Patch 1.15");
                        var _workshopCampaignID = (string)traverse.Field("workshopCampaignID").GetValue(); //static
                        if (enumerator.Current.campaignID != _workshopCampaignID) continue;
                        instance.StartWorkshopCampaign(_workshopCampaignID);

                        //Debug.Log("Tejas CSI Patch 1.16");
                        var _viewingCampaign = (Campaign)traverse.Field("viewingCampaign").GetValue();
                        while (_viewingCampaign == null)
                        {
                            //Debug.Log("Tejas CSI Patch 1.17");
                            _viewingCampaign = (Campaign)traverse.Field("viewingCampaign").GetValue();
                            yield return null;
                        }
                        if (lastCSave == null)
                        {
                            break;
                        }
                        if (lastCSave.lastScenarioWasTraining)
                        {
                            //Debug.Log("Tejas CSI Patch 1.18");
                            //this.trainingIdx = lastCSave.lastScenarioIdx;
                            traverse.Field("trainingIdx").SetValue(lastCSave.lastScenarioIdx);
                            instance.TrainingButton();
                            break;
                        }
                        //Debug.Log("Tejas CSI Patch 1.19");
                        //this.missionIdx = lastCSave.lastScenarioIdx;
                        traverse.Field("missionIdx").SetValue(lastCSave.lastScenarioIdx);
                        instance.MissionsButton();
                        break;
                    }
                }
            }

           //Debug.Log("Tejas CSI Patch 1.14");
            yield break;
            yield break;
        }

        static void Postfix(CampaignSelectorUI __instance, Action onOpenedSelector)
        {
            //Debug.unityLogger.logEnabled = Main.logging;
            //Debug.Log("Tejas CSI Patch 1.20: cv = " + PilotSaveManager.currentVehicle.vehicleName + ", ModVN = " + AircraftInfo.vehicleName);

            if (PilotSaveManager.currentVehicle.vehicleName != AircraftInfo.vehicleName) { return; }
            //Debug.Log("Tejas CSI Patch 1.20");
            instance = __instance;
            //Debug.Log("Tejas CSI Patch 1.21");
            instance.StartCoroutine(CSI_FinallyOpenCampaignSelectorRoutine(onOpenedSelector));
            //Debug.Log("Tejas CSI Patch 1.22");
        }
    }

    [HarmonyPatch(typeof(CampaignSelectorUI), "StartWorkshopCampaign")]
    class Tejas_CSIPatch_StartWorkshopCampaign
    {
        static CampaignSelectorUI instance;

        /** Replica of StartWorkshopCampaignRoutine. */
        static IEnumerator CSI_StartWorkshopCampaignRoutine(string campaignID)
        {
            
            //Debug.unityLogger.logEnabled = Main.logging;
            //Debug.Log("Tejas CSI Patch 2.1");
            var traverse = Traverse.Create(instance);

            //CampaignSelectorUI.openedFromWorkshop = true;
            traverse.Field("openedFromWorkshop").SetValue(true);
            //CampaignSelectorUI.openedWorkshopCampaign = true;
            traverse.Field("openedWorkshopCampaign").SetValue(true);
            //CampaignSelectorUI.workshopCampaignID = campaignID;
            traverse.Field("workshopCampaignID").SetValue(campaignID);
            //Debug.Log("Tejas CSI Patch 2.2");
            var campaign = VTResources.GetSteamWorkshopCampaign(campaignID);
            if (campaign == null)
            {
                //Debug.Log("Tejas CSI Patch 2.3");
                //Debug.Log("Tejas Missing campaign in CSI_StartWorkshopCampaignRoutine");
            }
            var vehicle = campaign.vehicle;
            PilotSaveManager.current.lastVehicleUsed = vehicle;
            PilotSaveManager.currentVehicle = VTResources.GetPlayerVehicle(vehicle);

            //Debug.Log("Tejas CSI Patch 2.4");
            yield return instance.StartCoroutine(CSIPatchUtils.BetterSetupCampaignScreenRoutine(instance));

            //instance.campaignIdx = 0;
            //instance.campaignIdx = 0;
            traverse.Field("campaignIdx").SetValue(0);
            //Debug.Log("Tejas CSI Patch 2.5");
            var _loadedWorkshopCampaigns = (List<Campaign>)traverse.Field("loadedWorkshopCampaigns").GetValue();

            var campaign2 = _loadedWorkshopCampaigns.FirstOrDefault(campaign3 => campaign3.campaignID == campaignID);
            //Debug.Log("Tejas CSI Patch 2.6");
            if (campaign2 == null)
            {
                //Debug.Log("Tejas CSI Patch 2.7");
                campaign2 = campaign.ToIngameCampaign();
                _loadedWorkshopCampaigns.Add(campaign2);
            }
            traverse.Field("loadedWorkshopCampaigns").SetValue(_loadedWorkshopCampaigns); //just to be safe

            if (campaign2 == null) yield break;
            //Debug.Log("Tejas CSI Patch 2.8");
            instance.campaignDisplayObject.SetActive(true);
            PilotSaveManager.currentCampaign = campaign2;
            //Debug.Log("Tejas CSI Patch 2.9");
            instance.SetupCampaignScenarios(campaign2, true);
        }

        static bool Prefix(CampaignSelectorUI __instance, string campaignID)
        {
            if (PilotSaveManager.currentVehicle.vehicleName == AircraftInfo.vehicleName) { return true; }
            //Debug.Log("Tejas CSI Patch 2.0.1");
            {
                //Debug.Log("Tejas CSI Patch 2.0.2");
                instance = __instance;
                instance.StartCoroutine(CSI_StartWorkshopCampaignRoutine(campaignID));
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CampaignSelectorUI), "StartWorkshopMission")]
    class Tejas_CSIPatch_StartWorkshopMission
    {
        static CampaignSelectorUI instance;

        /** Replica of StartWorkshopCampaignRoutine. */
        static IEnumerator CSI_StartWorkshopMissionRoutine(string scenarioID)
        {
            //Debug.unityLogger.logEnabled = Main.logging;
            //Debug.Log("Tejas CSI Patch 3.0.1");
            var traverse = Traverse.Create(instance);

            //CampaignSelectorUI.openedFromWorkshop = true;
            traverse.Field("openedFromWorkshop").SetValue(true);
            //CampaignSelectorUI.openedWorkshopCampaign = false;
            traverse.Field("openedWorkshopCampaign").SetValue(false);
            //Debug.Log("Tejas CSI Patch 3.0.2");

            //Debug.Log("Tejas [CSIPatch] ========== A steam workshop standalone mission should be loaded at this time ===========");
            var scenario = VTResources.GetSteamWorkshopStandaloneScenario(scenarioID);
            var vehicle = scenario.vehicle;
            //Debug.Log("Tejas CSI Patch 3.0.3 vehicle: " + vehicle.vehicleName + ", sce:" + scenario.name );
            PilotSaveManager.currentVehicle = vehicle;
            PilotSaveManager.current.lastVehicleUsed = vehicle.vehicleName;
            yield return instance.StartCoroutine(CSIPatchUtils.BetterSetupCampaignScreenRoutine(instance));
            //Debug.Log("Tejas CSI Patch 3.1");

            var campaign = (Campaign)traverse.Field("swStandaloneCampaign").GetValue<Campaign>();
            
            //Debug.Log("Tejas CSI Patch 3.1.1");

            if (campaign.missions != null)
            {
                //Debug.Log("Tejas CSI Patch 3.2");
                campaign.missions.Clear();
            }
            else
            {
                //Debug.Log("Tejas CSI Patch 3.3");
                campaign.missions = new List<CampaignScenario>(1);
            }
            //Debug.Log("Tejas CSI Patch 3.4");
            var _loadedWorkshopSingleScenarios = (List<CampaignScenario>)traverse.Field("loadedWorkshopSingleScenarios").GetValue();

            var campaignScenario = _loadedWorkshopSingleScenarios.FirstOrDefault(campaignScenario2 => campaignScenario2.scenarioID == scenarioID) ??
                                   scenario.ToIngameScenario(null);
            //Debug.Log("Tejas CSI Patch 3.5");
            campaign.missions.Add(campaignScenario);
            PilotSaveManager.currentCampaign = campaign;
            instance.SetupCampaignScenarios(campaign, false);
            //Debug.Log("Tejas CSI Patch 3.6");
            foreach (var mission in campaign.missions)
            {
                //Debug.Log("Tejas CSI Patch 3.6.1");
                if (mission.scenarioID != scenarioID) continue;
                //instance.StartMission(campaign.missions[mission]);
                var _startMission = instance.GetType().GetMethod("StartMission", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[]  { typeof(CampaignScenario)},null);
                //Debug.Log("Tejas CSI Patch 3.6.2");
                _startMission.Invoke(instance, new object[] { mission });
                yield break;
            }
        }

        static void Postfix(CampaignSelectorUI __instance, ref CampaignSelectorUI.WorkshopLaunchStatus __result, string scenarioID)
        {
            if (PilotSaveManager.currentVehicle.vehicleName != AircraftInfo.vehicleName) { return; }
            //Debug.Log("Tejas CSI Patch 4.1");
            instance = __instance;

            var steamWorkshopStandaloneScenario = VTResources.GetSteamWorkshopStandaloneScenario(scenarioID);
            //Debug.Log("Tejas CSI Patch 4.2");
            if (steamWorkshopStandaloneScenario == null)
            {
                //Debug.LogError("[CSIPatch] Tried to run workshop scenario but scenario was null!");
                __result = new CampaignSelectorUI.WorkshopLaunchStatus
                {

                    success = false,
                    message = VTLStaticStrings.err_scenarioNotFound
                };
                return;
            }
            if (GameStartup.version < steamWorkshopStandaloneScenario.gameVersion)
            {
                //Debug.Log("Tejas CSI Patch 4.3");
                var str = "[CSIPatch] Tried to run workshop scenario but version incompatible.  Game version: ";
                var str2 = GameStartup.version.ToString();
                var str3 = ", scenario game version: ";
                var gameVersion = steamWorkshopStandaloneScenario.gameVersion;
                //Debug.LogError(str + str2 + str3 + gameVersion);
                __result = new CampaignSelectorUI.WorkshopLaunchStatus
                {
                    success = false,
                    message = VTLStaticStrings.err_version
                };
                return;
            }
            instance.StartCoroutine(CSI_StartWorkshopMissionRoutine(scenarioID));
            //Debug.Log("Tejas CSI Patch 4.4");

            __result = new CampaignSelectorUI.WorkshopLaunchStatus
            {
                success = true
            };
        }
    }

}

