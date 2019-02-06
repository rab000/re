using UnityEditor;
using UnityEngine;
public class DetectChanges : AssetPostprocessor  
{

	static void OnAssinMaterialModel(Material mat,Renderer render){
		Debug.LogError ("-->mat:"+mat+" render:"+render);
	}

	static void OnPostprocessAllAssets (
		string[] importedAssets,
		string[] deletedAssets,
		string[] movedAssets,
		string[] movedFromAssetPaths) 
	{            
//		foreach (string str in importedAssets)
//		{
//			Debug.Log("Reimported Asset: " + str);
//			string[] splitStr = str.Split('/', '.');
//
//			string folder = splitStr[splitStr.Length-3];
//			string fileName = splitStr[splitStr.Length-2];
//			string extension = splitStr[splitStr.Length-1];
//			Debug.Log("File name: " + fileName);
//			Debug.Log("File type: " + extension);                
//		}
//
//		foreach (string str in deletedAssets)
//			Debug.Log("Deleted Asset: " + str);
//
//		for (int i=0;i<movedAssets.Length;i++)
//			Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);        
	}
}