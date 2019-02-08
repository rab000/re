using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
/// <summary>
/// 用于打包资源修改检测
/// 
/// 对于换装角色打包，需要对原始资源进行拆分，如果每次都拆分,
/// 被拆分出来的资源都会被重新设置bundleName，并重新打bundle
/// 所以要手动检测下资源是否有变化，有变化的资源再做拆分
/// 
/// 检测方法说明
/// 对于unity来说，分内置资源(mat等)和外部资源(fbx等)
/// 内置资源资源变化看资源本身(md5)是否发生变化
/// 外部资源检测变化要看.meta(md5)文件是否发生变化
/// </summary>
public class DetectRepeatRes {


	public static string MD5CheckDicPath{
		get{ 
		
			return EditorHelper.GetParentFolderPath(Application.dataPath)+"/md5/md5.n";
		}
	}

	//记录资源相对路径和md5
	public static Dictionary<string,string> PathMD5Dic = new Dictionary<string, string>();



	public static void LoadCheckDic()
	{
		bool b = EditorHelper.BeFileExist (MD5CheckDicPath);

		if (!b)
			return;
		
		var bs = FileHelper.ReadBytesFromFile (MD5CheckDicPath);

		IoBuffer ib = new IoBuffer (102400);

		ib.PutBytes (bs);
	
		int num = ib.GetInt ();

		for (int i = 0; i < num; i++) 
		{
			string relePath = ib.GetString ();
			string md5 = ib.GetString ();
			PathMD5Dic.Add (relePath,md5);
		}

	}


	public static void SaveCheckDic()
	{
		EditorHelper.DeleteFileIfExists (MD5CheckDicPath);

		int num = PathMD5Dic.Count;

		if (num == 0)
			return;

		IoBuffer ib = new IoBuffer (102400);

		ib.PutInt (num);

		foreach (var p in PathMD5Dic) 
		{
			ib.PutString (p.Key);
			ib.PutString (p.Value);
		}

		byte[] bs = ib.ToArray ();

		FileHelper.WriteBytes2File (MD5CheckDicPath,bs);

	}	

	public static void Add2MD5Dic()
	{
		 
	}

	public static void DelFromMD5Dic()
	{

	}

	//检查资源变更
	public static bool BeFileChanged(string filePath)
	{
		//string path = "D:/workspace/gitspace/re/Assets/RoleEditor/Res/man/mat/hanxin.mat";

		string tFilePath = filePath;

		bool bNative = BeUnityNativeRes (filePath);

		if(bNative)tFilePath = tFilePath+".meta";

		var relePath = EditorHelper.ChangeToRelativePath (tFilePath);

		var bs = File.ReadAllText (tFilePath);

		var hash = MD5.Md5Sum (bs);

		bool b = PathMD5Dic.ContainsKey (relePath);

		if (b && hash.Equals (PathMD5Dic [relePath]))
		{
			//md5没发生变化
			return false;
		}

		return true;
	}

	public static bool BeUnityNativeRes(string filePath)
	{
		string relePath = EditorHelper.ChangeToRelativePath (filePath);
		string strID = AssetDatabase.AssetPathToGUID (relePath);
		int id = EditorHelper.GetInstanceIDFromGUID (strID);
		bool b = AssetDatabase.IsNativeAsset (id);
		return b;
	}



//	public static void TT(string folderPath)
//	{
//		string path = "D:/workspace/gitspace/re/Assets/RoleEditor/Res/man/mat/hanxin.mat";
//		//string path = "D:/workspace/gitspace/re/Assets/RoleEditor/Res/man/model/hx.FBX.meta";
//		var bs = File.ReadAllText (path);
//		var hash = MD5.Md5Sum (bs);
//		Debug.LogError ("---------->Hash:"+hash);
//
//		string resPath = "Assets/RoleEditor/Res/man/mat/hanxin.mat";
//		string ids = AssetDatabase.AssetPathToGUID (resPath);
//
//		int id = EditorHelper.GetInstanceIDFromGUID (ids);
//		bool bF = AssetDatabase.IsForeignAsset (id);
//		bool bN = AssetDatabase.IsNativeAsset (id);
//
//		string resPath2 = "Assets/RoleEditor/Res/man/model/hx.FBX";
//		string id2str = AssetDatabase.AssetPathToGUID (resPath2);
//		int id2 = EditorHelper.GetInstanceIDFromGUID (id2str);
//		bool bF2 = AssetDatabase.IsForeignAsset (id2);
//		bool bN2 = AssetDatabase.IsNativeAsset (id2);
//
//		Debug.LogError ("ids:"+ids+" id:"+id+" bf:"+bF+" bn:"+bN+" ------------ids2:"+id2str+" id2:"+id2+" bf2:"+bF2+" bn2:"+bN2);
//
//	}






}
