using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using UnityEngine.UI;
using TMPro;



namespace CustomAircraftTemplateTejas
{
    //Elements should be included in here that are non standard and might be custom to the aircraft 
    [ExecuteInEditMode]
    public class CameraRenderWithMaterial : MonoBehaviour
    {
        public Material material;
       // public RenderTexture source;
        //public RenderTexture destination;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, material);
        }
    }
    public class CustomElements : MonoBehaviour
    {

        public static void RegisterCustomWeapons(String resourcePath, GameObject prefab)
        {
            VTNetworking.VTNetworkManager.RegisterOverrideResource(resourcePath, prefab);
        }


       

        

        public static void SetUpGauges()
        {
            Battery componentInChildren = Main.aircraftCustom.GetComponentInChildren<Battery>(true);
            FlightInfo componentInChildren2 = Main.aircraftCustom.GetComponentInChildren<FlightInfo>(true);
            GameObject childWithName3 = AircraftAPI.GetChildWithName(Main.aircraftCustom, "ClimbGauge", false);
            DashVertGauge dashVertGauge = childWithName3.AddComponent<DashVertGauge>();
            dashVertGauge.battery = componentInChildren;
            dashVertGauge.dialHand = AircraftAPI.GetChildWithName(childWithName3, "dialHand", false).transform;
            dashVertGauge.axis = new Vector3(0f, 1f, 0f);
            dashVertGauge.arcAngle = 360f;
            dashVertGauge.maxValue = 5f;
            dashVertGauge.lerpRate = 8f;
            dashVertGauge.loop = true;
            dashVertGauge.gizmoRadius = 0.02f;
            dashVertGauge.gizmoHeight = 0.005f;
            dashVertGauge.doCalibration = true;
            dashVertGauge.calibrationSpeed = 1f;
            dashVertGauge.info = componentInChildren2;
            dashVertGauge.measures = Main.aircraftCustom.GetComponent<MeasurementManager>();
        }
        

    }
}



