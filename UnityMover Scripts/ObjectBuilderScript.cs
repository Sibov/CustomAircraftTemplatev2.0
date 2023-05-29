using UnityEngine;

using System.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;
using System.IO.Compression;


[System.Serializable]
public struct SerializableVector3custom
{
    /// <summary>
    /// x component
    /// </summary>
    public float x;

    /// <summary>
    /// y component
    /// </summary>
    public float y;

    /// <summary>
    /// z component
    /// </summary>
    public float z;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rX"></param>
    /// <param name="rY"></param>
    /// <param name="rZ"></param>
    public SerializableVector3custom(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    /// <summary>
    /// Returns a string representation of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}]", x, y, z);
    }
    public Vector3 toVector3
    {
        get
        {
            return new Vector3(x, y, z);
        }
        set
        {

        }
    }
    /// <summary>
    /// Automatic conversion from SerializableVector3 to Vector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Vector3(SerializableVector3custom rValue)
    {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    /// <summary>
    /// Automatic conversion from Vector3 to SerializableVector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableVector3custom(Vector3 rValue)
    {
        return new SerializableVector3custom(rValue.x, rValue.y, rValue.z);
    }


}
[Serializable]
public class customTransform
{
    public String name;

    public SerializableVector3custom positions;

    public SerializableVector3custom rotations;

    public SerializableVector3custom scales;
    [NonSerialized]
    public bool skip = false;
}


[AddComponentMenu("Tiled Map/Tiled Map Component")]
public class ObjectBuilderScript : MonoBehaviour
{
    public GameObject gs;
    public Vector3 spawnPoint;
    public string paths;
    public bool rendsetSave;
    public bool rendsetLoad;
    public void save()
    {
        {
            gs = gameObject;
            if (gs == null)
                return;


            Transform[] transList = gs.GetComponentsInChildren<Transform>(true);


            Debug.Log(transList.Length);
            int customnum = 0;
            foreach (var tran in transList)
            {
                if (tran.gameObject.tag == "custom")
                {
                    customnum++;
                }
            }

            using (BinaryWriter writer = new BinaryWriter(File.Open(paths, FileMode.Create)))
            {
                writer.Write(customnum);

                foreach (var tran in transList)
                {
                    if (tran.gameObject.tag == "custom")
                    {
                         writer.Write(tran.name.Length);

                        string strInput = tran.name;
                        string finname = tran.name;
                        if (!tran.name.Contains("MFD") && !tran.name.Contains("aFighter") && !tran.name.Contains("RPMGauge") && !tran.name.Contains("HP"))
                              finname = strInput.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ' });
                       
                        Debug.Log(finname);
                        if (rendsetSave)
                        {
                            if (!tran.gameObject.active)
                                writer.Write("disable");
                     else
                        if (tran.gameObject.GetComponent<Renderer>() != null)
                        {
                               
                               
                                if (tran.gameObject.GetComponent<Renderer>().enabled)
                                    writer.Write("render");
                                else
                                    writer.Write("dontrender");
                        }else
                        writer.Write("norender");
                        }

                        writer.Write(finname);
                        if(tran.parent!=gs.transform)
                        {
                            string strInputp = tran.parent.name;
                            string finnamep = tran.parent.name;
                            if (!strInputp.Contains("MFD")&&!strInputp.Contains("aFighter")&&!strInputp.Contains("RPMGauge") && !tran.name.Contains("HP"))
                                 finnamep = strInputp.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ' });
                            writer.Write(finnamep);
                        }
                        else
                            writer.Write("noparent");
                        writer.Write(tran.localPosition.x);
                        writer.Write(tran.localPosition.y);
                        writer.Write(tran.localPosition.z);

                        writer.Write(tran.localEulerAngles.x);
                        writer.Write(tran.localEulerAngles.y);
                        writer.Write(tran.localEulerAngles.z);

                        writer.Write(tran.localScale.x);
                        writer.Write(tran.localScale.y);
                        writer.Write(tran.localScale.z);
                    }
                }

            }
        }
    }
    public void load( )
    {
        gs =  gameObject;
        Debug.Log("load1"); 
        if (gs == null)
            return;
        List<customTransform> listCustomTraans = new List<customTransform>();
        using (BinaryReader reader = new BinaryReader(File.Open(paths, FileMode.Open)))
        {
           
          
            int total = reader.ReadInt32();
            Debug.Log("lod"+ total);

            for (int i = 0; i < total; i++)
            {
               int totalString = reader.ReadInt32();

                string render = "";
                
                if(rendsetLoad) render = reader.ReadString();
                string name = reader.ReadString();
                string pname = reader.ReadString();
                float x = reader.ReadSingle() * 100;
                float y = reader.ReadSingle() * 100;
                float z = reader.ReadSingle() * 100;

                float rx = reader.ReadSingle();
                float ry = reader.ReadSingle();
                float rz = reader.ReadSingle();

                float sx = reader.ReadSingle();
                float sy = reader.ReadSingle();
                float sz = reader.ReadSingle();

                customTransform trans = new customTransform();
                trans.name = name;
                float fc = 100.0f;
                //if (!rendsetLoad)
                   //fc = 1.0f;
                trans.positions = new Vector3(x/ fc, y / fc, z / fc);
                trans.rotations = new Vector3(rx, ry, rz);
                 trans.scales = new Vector3(sx, sy, sz);

                foreach (var ts in gs.GetComponentsInChildren<Transform>(true))
                {
                  
                    if (ts.name == (name))
                        {
                        if (ts.parent.name == (pname) || pname == "noparent")
                        {
                            if (render == "disable")
                                ts.gameObject.active = false;
                            if (rendsetLoad)
                                if (ts.gameObject.GetComponent<Renderer>() != null)
                                {
                                    if (render == "render")
                                        ts.gameObject.GetComponent<Renderer>().enabled = true;

                                    if (render == "dontrender")
                                        ts.gameObject.GetComponent<Renderer>().enabled = false;

                                }

                            ts.localPosition = trans.positions;
                            ts.localEulerAngles = trans.rotations;
                            ts.localScale = trans.scales;

                            ts.gameObject.tag = "custom";
                        }
                    }
                }
            }
        }
    }

}