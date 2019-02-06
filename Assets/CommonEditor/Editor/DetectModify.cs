using UnityEngine;
using UnityEditor;

public class DetectModify : AssetModificationProcessor {

	static void OnWillCreateAsset(string assetName)
	{
		//Debug.Log("OnWillCreateAsset is being called with the following asset: " + assetName + ".");
	}

	static void OnWillDeleteAsset(string assetName,RemoveAssetOptions opt)
	{
		//Debug.Log("OnWillDeleteAsset is being called with the following asset: " + assetName + ".");
	}

	private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
	{
		//Debug.Log("Source path: " + sourcePath + ". Destination path: " + destinationPath + ".");
		AssetMoveResult assetMoveResult = AssetMoveResult.DidMove;

		// Perform operations on the asset and set the value of 'assetMoveResult' accordingly.

		return assetMoveResult;
	}

	static string[] OnWillSaveAssets(string[] paths)
	{
//		Debug.Log("OnWillSaveAssets");
//		foreach (string path in paths)
//			Debug.Log(path);
		return paths;
	}



}
