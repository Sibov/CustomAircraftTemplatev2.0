using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomAircraftTemplateTejas
{
    
    static class FileLoader
    {
        //PUBLIC LOADING METHODS
        //Thanks Baan/GentleLeviathan!!!!
        public static AssetBundle GetAssetBundleAsGameObject(string path, string name)
        {
            //Debug.Log("AssetBundleLoader: Attempting to load AssetBundle...");
            AssetBundle bundle = null;
            try
            {
                bundle = AssetBundle.LoadFromFile(path);
                //Debug.Log("AssetBundleLoader: Success.");
                return (AssetBundle)bundle;
            }
            catch (Exception e)
            {
                //Debug.Log("AssetBundleLoader: Couldn't load AssetBundle from path: '" + path + "'. Exception details: e: " + e.Message);
                return null;
            }

        }
        public static GameObject GetPrefabAsGameObject(AssetBundle bundleLoad, string name)
        {
            //Debug.Log("AssetBundleLoader: Attempting to retrieve: '" + name + "' as type: 'GameObject'" + " from " + bundleLoad);
            try
            {
                var temp = bundleLoad.LoadAsset(name, typeof(GameObject));
                //Debug.Log("AssetBundleLoader: Success.");
                return (GameObject)temp;
            }
            catch (Exception e)
            {
                //Debug.Log("AssetBundleLoader: Couldn't retrieve GameObject from AssetBundle.");
                return null;
            }

        }

        public static Shader GetShader(AssetBundle bundleLoad, string name)
        {
            //Debug.Log("AssetBundleLoader: Attempting to retrieve: '" + name + "' as type: 'Shader'" + " from " + bundleLoad);
            try
            {
                var temp = bundleLoad.LoadAsset(name, typeof(Shader));
                //Debug.Log("AssetBundleLoader: Success.");
                return (Shader)temp;
            }
            catch (Exception e)
            {
                //Debug.Log("AssetBundleLoader: Couldn't retrieve Shader from AssetBundle.");
                return null;
            }

        }


        public static Texture GetTexture(AssetBundle bundleLoad, string name)
        {
            //Debug.Log("AssetBundleLoader: Attempting to retrieve: '" + name + "' as type: 'Texture'" + " from " + bundleLoad);
            try
            {
                var temp = bundleLoad.LoadAsset(name, typeof(Texture));
                //Debug.Log("AssetBundleLoader: Success.");
                return (Texture)temp;
            }
            catch (Exception e)
            {
                //Debug.Log("AssetBundleLoader: Couldn't retrieve Texture from AssetBundle.");
                return null;
            }

        }

        public static ScriptableObject GetPrefabAsScriptableObject(AssetBundle bundleLoad, string name)
        {
            //Debug.Log("AssetBundleLoader: Attempting to retrieve: '" + name + "' as type: 'Scriptable'" + " from " + bundleLoad);
            try
            {
                var temp = bundleLoad.LoadAsset(name, typeof(ScriptableObject));
                //Debug.Log("AssetBundleLoader: Success.");
                return (ScriptableObject)temp;
            }
            catch (Exception e)
            {
                //Debug.Log("AssetBundleLoader: Couldn't retrieve GameObject from AssetBundle.");
                return null;
            }

        }

        public static Campaign GetPrefabAsCampaigns(AssetBundle bundleLoad, string name)
        {
            Campaign result;
            try
            {
                UnityEngine.Object @object = bundleLoad.LoadAsset(name, typeof(Campaign));
                result = (Campaign)@object;
            }
            catch (Exception ex)
            {
                result = null;
            }
            return result;
        }
        public static BuiltInCampaigns GetPrefabAsBICampaigns(AssetBundle bundleLoad, string name)
        {
            BuiltInCampaigns result;
            try
            {
                UnityEngine.Object @object = bundleLoad.LoadAsset(name, typeof(BuiltInCampaigns));
                result = (BuiltInCampaigns)@object;
            }
            catch (Exception ex)
            {
                result = null;
            }
            return result;
        }
        public static PlayerVehicle GetPrefabAsPlayerVehicle(AssetBundle bundleLoad, string name)
        {
            PlayerVehicle result;
            try
            {
                UnityEngine.Object @object = bundleLoad.LoadAsset(name, typeof(PlayerVehicle));
                result = (PlayerVehicle)@object;
            }
            catch (Exception ex)
            {
                result = null;
            }
            return result;
        }

    }


}
