using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using System.IO;

public class dgUtil
{

	public static string FormatTime(float seconds){
		return (seconds >= 3600 ? (Mathf.FloorToInt (seconds / 3600).ToString () 
			+ ":" ) : "")
			+ ((seconds % 3600) >= 600 ? "": "0")
			+ Mathf.FloorToInt((seconds % 3600) / 60)
			+ ":"
			+ ((seconds % 60) >= 10 ? "": "0")
			+ Mathf.FloorToInt(seconds % 60);
	}
    public static string FormatNum(float value, float decimals = 1)
    {
        float factor = Mathf.Pow(10, decimals);
        return (Mathf.Round(value * factor) / factor).ToString();
    }

	private static GameObject InstantiateLocalOnParent(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool world_local, object[] data = null)
    {
		GameObject go = (GameObject)GameObject.Instantiate(prefab, position, rotation);
		go.transform.SetParent(parent);
		if (world_local) 
		{
			go.transform.localPosition = position;
			go.transform.localRotation = rotation;
		} 
		else 
		{
			go.transform.position = position;
			go.transform.rotation = rotation;
		}
        go.name = prefab.name;
		if (data != null) {
			if (data.Length > 0) {
				go.SendMessage ("SetData", data [0].ToString(), SendMessageOptions.DontRequireReceiver);
			}
		}
        return go;
    }

	public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, bool world_local = true, Transform parent = null, bool network_local = true, object[] data = null)
	{
		if(network_local)
		{
			return InstantiateLocalOnParent (prefab, position, rotation, parent, world_local, data);
		}
		else
		{
			if (false)//contectedremotely 
			{
				return InstantiateLocalOnParent (prefab, position, rotation, parent, world_local, data);
			} 
			else 
			{
				return InstantiateLocalOnParent (prefab, position, rotation, parent, world_local, data);
			}
		}
	}

    public static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int k = Mathf.FloorToInt(Random.value * list.Count); ;
            T value = list[k];
            list[k] = list[i];
            list[i] = value;
        }
    }
    // PAST HERE IS THE ADMIN SECTION
#if DEVELOPMENT_BUILD || UNITY_EDITOR
	//dev
	public static List<string> testData = new List<string> ();
	public static int testCount = 0;
	/*
	public static void SaveTestData(List<string> saveData){
		StreamWriter outStream = System.IO.File.CreateText(Application.dataPath +"TestData.csv");
		foreach (string str in saveData) {
			outStream.WriteLine (str);
		}
		outStream.Close();
	}*/

#else
	//prod
#endif
}
