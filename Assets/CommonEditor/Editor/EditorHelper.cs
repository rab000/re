using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;
/// <summary>
/// Editor自用常量
/// Editor常用方法，主要是处理路径相关方法
/// 同一处理管理所有AssetDatabase,Path,Directory相关功能调用
/// 
/// 好处是，写过的功能很容易忘记，经常发生路径相关处理二次使用时找不到之前使用位置的情况
/// 统一处理，统一标识，提高相关功能查找效率,一旦业务用到路径相关功能直接找EditorHelper
/// EditorHelper 对应各个编辑器业务来说是 Unity路径相关功能的总接口
/// 
/// </summary>
public static class EditorHelper{

	/// <summary>
	/// 生成bundle文件后缀名
	/// </summary>
	/// <value>The name of the suffix.</value>
	public static string ABSuffixName{
		get {return ".n"; }	
	}

	#region 编辑器路径
	/// <summary>
	/// Asset根目录绝对路径
	/// </summary>
	/// <value>The UR l_ ASSE.</value>
	public static string EDITOR_ASSETS_PATH{
		get{
			return Application.dataPath;
		}
	}

	/// <summary>
	/// 切换到相对路径
	/// 比如绝对路径为D:/SVN/NEditor/trunk/Assets/NEDITOR/RoleEditor/Res/Female/AnimationClip/common/walk.anim
	/// 转换为相对路径为Assets/NEDITOR/RoleEditor/Res/Female/AnimationClip/common/walk.anim
	/// 
	/// AssetDatabase.LoadAssetAtPath要使用相对路径
	/// 
	/// </summary>
	/// <returns>The relative path.</returns>
	/// <param name="absolutePath">Absolute path.</param>
	public static string ChangeToRelativePath(string absolutePath){
		string s = "Assets"+absolutePath.Substring(EDITOR_ASSETS_PATH.Length);
		return s;
	}
	
	/// <summary>
	/// 切换到绝对路径
	/// </summary>
	/// <returns>The absolute path.</returns>
	/// <param name="relativePath">Relative path.</param>
	public static string ChangeToAbsolutePath(string relativePath){
		string s = EDITOR_ASSETS_PATH+relativePath.Substring("Assets".Length);
		return s;
	}




	#endregion

	#region 编辑器输出路径

	/// <summary>
	/// 输出ab根目录
	/// </summary>
	/// <value>The output root path.</value>
	public static string OUTPUT_ROOT_PATH{
		get{
			return EditorHelper.GetParentFolderPath(Application.dataPath)+"/data";
		}
	}

	/// <summary>
	/// AB资源输出目录
	/// </summary>
	/// <value>The output res path.</value>
	public static string OUTPUT_RES_PATH{
		get{
			return OUTPUT_ROOT_PATH+"/res";
		}
	}



	#endregion


	#region Folder
	public static bool BeFolderExist(string folderPath){
		return Directory.Exists (folderPath);
	}

	/// <summary>
	/// Creates the folder.
	/// </summary>
	/// <returns>The folder.</returns>
	/// <param name="folderPath">Folder path.</param>
	/// <param name="bRebuild">If set to <c>true</c> 目录存在则先删除再重建.</param>
	public static void CreateFolder(string folderPath){
		if (!Directory.Exists(folderPath)){
			Log.i ("EditorHelper","CreateFolder","文件夹"+folderPath+"不存在，创建文件夹");
			Directory.CreateDirectory(folderPath);
		}
	}

	/// <summary>
	/// 删除文件夹
	/// 注意:
	/// 
	/// Directory.Delete第一个参数结尾不能是/否则异常提示找不到文件夹
	/// 
	/// Directory.Delete第二个参数
	/// =true只会删除文件夹下的所有文件及文件夹
	/// =false只有文件夹为null时才能删除自己,如果文件夹不为null，还设置为false，那么报错io异常
	/// 
	/// Directory.GetDirectories说明
	/// 如果传入参数如"Asset/FolderA"  那么返回的string[] 第一个就是"Asset/FolderA"
	/// 如果传入参数如"Asset/FolderA/" 那么返回的string[] 只有子folder
	/// 
	/// delSubFolder是否删除文件夹(目录结构),如果是false，只删除文件保留文件夹结构
	public static void DeleteFolder(string folderPath,bool delSubFolder=true){
		
		if (!Directory.Exists(folderPath))return;

		if (folderPath.EndsWith ("/"))
		{
			Log.e("EditorHelper","DeleteFolder","删除文件夹路径以/结尾，错误，终止删除");
			return;
		}

		Log.e ("EditorHelper","DeleteFolder","待删除文件夹"+folderPath+"存在，删除文件夹及子内容");
		Directory.Delete(folderPath,true);//先把所有文件删除(文件夹结构还没删除)

	}

	/// <summary>
	/// 获取当前路径的所有子目录的绝对path
	/// </summary>
	/// <returns>The sub folder path.</returns>
	/// <param name="path">Path.</param>
	public static string[] GetSubFolderPaths(string path){
		return Directory.GetDirectories(path);
	}

	/// <summary>
	/// 获取当前路径的所有文件的绝对path
	/// </summary>
	/// <returns>The sub folder path.</returns>
	/// <param name="path">Path.</param>
	public static string[] GetSubFilesPaths(string path){
		return Directory.GetFiles(path);
	}

	#endregion

	#region File

	public static bool BeFileExist(string filePath)
	{
		return File.Exists (filePath);
	}

	/// <summary>
	/// Creates the file.
	/// </summary>
	/// <param name="path">Path.</param>
	public static void DeleteFileIfExists(string path){
		if(!File.Exists(path))return;
		Log.i ("EditorHelper","DeleteFileIfExists","待删除文件"+path+"存在，删除之");
		File.Delete(path);
	}

	public static void BeFileSame(string file1Path,string file2Path)
	{
		//计算第一个文件的哈希值
		var hash = System.Security.Cryptography.HashAlgorithm.Create();
		var stream_1 = new System.IO.FileStream(file1Path, System.IO.FileMode.Open);
		byte[] hashByte_1 = hash.ComputeHash(stream_1);
		stream_1.Close();
		//计算第二个文件的哈希值
		var stream_2 = new System.IO.FileStream(file2Path, System.IO.FileMode.Open);
		byte[] hashByte_2 = hash.ComputeHash(stream_2);
		stream_2.Close();

		//比较两个哈希值
		if (BitConverter.ToString (hashByte_1) == BitConverter.ToString (hashByte_2))
			Debug.LogError("两个文件相同");
		else
			Debug.LogError("两个文件不同");

	}

	public static int GetInstanceIDFromGUID(string guid)
	{
		System.Reflection.MethodInfo method = typeof( AssetDatabase).GetMethod("GetInstanceIDFromGUID"
			, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
		int result =(int) method.Invoke(null,new object[]{ (object)guid});
		return result;
	}
	#endregion

	#region Path相关
	/// <summary>
	/// 得到当前路径(可以是文件或文件夹路径)上一级目录的路径
	/// </summary>
	/// <returns>The parent folder path.</returns>
	/// <param name="path">Path.</param>
	public static string GetParentFolderPath(string path){
		return Path.GetDirectoryName(path);
	}


	/// <summary>
	/// 得到路径中最后位置的名称(可以是文件名或者是文件夹名,最后一个/后面的内容如果是文件则包括后缀名)
	/// </summary>
	/// <returns>The file name from path.</returns>
	/// <param name="path">Path.</param>
	/// <param name="beWithoutExtension">是否包括扩展名</c> be without extension.</param>
	public static string GetFileNameFromPath(string path,bool beWithoutExtension=false){

		if (beWithoutExtension)
		{
			return Path.GetFileNameWithoutExtension (path);
		}
		return  Path.GetFileName(path);
	}

	#endregion

	#region BundleName相关

	/// <summary>
	/// bundle扩展名
	/// </summary>
	/// <value>The BUNDL e EX t NAM.</value>
	public static string BUNDLE_EXT_NAME{
		get{
			return "n";
		}
	}

	/// <summary>
	/// 设置bundle打包标签
	/// </summary>
	/// <param name="path">Path.Asset相对路径（需要后缀名）</param>
	/// <param name="bundleName">Bundle name.bundle名</param>
	/// <param name="bundleVariant">Bundle variant.后缀</param>
	public static void SetAssetBundleName(string path,string bundleName,string bundleVariant = "n"){
		//Debug.LogError ("SetAssetBundleName--->path:"+path);
		if (path.EndsWith (".meta"))
			return;

		AssetImporter asset = AssetImporter.GetAtPath (path);
		//asset.assetBundleName = bundleName;
		//asset.assetBundleVariant = bundleVariant;
		//asset.GetHashCode ();
		//asset.assetTimeStamp();
		asset.SetAssetBundleNameAndVariant(bundleName,bundleVariant);
		asset.SaveAndReimport();

	}

	public static string GetAssetBundleName(string path,bool HasVarant=true){
		AssetImporter asset = AssetImporter.GetAtPath (path);
		string name = asset.assetBundleName;
		if (HasVarant) {
			name = name +"."+asset.assetBundleVariant;
		}
		return name;
	}

	#endregion


	#region AssetDatabase相关
	/// <summary>
	/// 获取nityEngine.Object绝对路径
	/// </summary>
	/// <returns>The object absolute path.</returns>
	/// <param name="obj">Object.</param>
	public static string GetUnityObjAbsolutePath(UnityEngine.Object obj){
		return AssetDatabase.GetAssetPath(obj);
	}

	/// <summary>
	/// 载入Asset(路径是相对路径Asset/Neditro......)
	/// </summary>
	/// <returns>The asset at path.</returns>
	/// <param name="path">Path.</param>
	/// <param name="type">Type.</param>
	public static Object LoadAssetAtPath(string path,Type type){
		return AssetDatabase.LoadAssetAtPath(path,type);
	}

	/// <summary>
	/// 注意如果copy文件夹不存在，则copy不生效
	/// 需要相对路径,带文件名后缀
	/// eg:
	/// 移动mat
	/// 从:Assets/ArtRes/Role/Res\hg/Materials/hgmat.mat 
	/// 到Assets/ArtRes/Role/TempAsset/hg/Materials/hgmat.mat
	/// </summary>
	/// <param name="path">Path.</param>
	/// <param name="newpath">Newpath.需要相对路径</param>
	public static void CopyAsset(string path,string newpath){
		AssetDatabase.CopyAsset(path,newpath);
	}


	#endregion



	#region other 这个以后要放到通用工具类中
	/// <summary>
	/// 收集指定目录类型为T的资源到List<T>
	/// </summary>
	/// <returns>The all.</returns>
	/// <param name="path">Path.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static List<T> CollectAll<T>(string folderPath) where T : Object{
		
		List<T> l = new List<T>();
		
		string[] files = Directory.GetFiles(folderPath);
		// 注意之类可以考虑给Directory.GetFiles加过滤参数
		//string[] files = Directory.GetFiles(Application.dataPath + "/AAAA/", "*", SearchOption.AllDirectories);

		foreach (string file in files){
			
			// NAFIO INFO Unity自有文件
			if (file.Contains(".meta")) continue;
			
			// 注意这里AssetDatabase.LoadAssetAtPath要求的路径是Assets/xxxx/
			// 可以用这个强制规定下路径，避免找不到资源
			string tempFilePath = file.Substring(file.IndexOf("Assets"));
			
			// 注意这里加了一句避免有错误的反斜杠
			//tempFilePath = tempFilePath.Replace(@"\", "/");
			
			T asset = (T) AssetDatabase.LoadAssetAtPath(file, typeof(T));
			
			if (asset == null) throw new Exception("Asset is not " + typeof(T) + ": " + file);
			
			l.Add(asset);
		}
		return l;
	}

	/// <summary>
	/// 查找目录下所有文件
	/// 输出目录eg:Assets/NEditor/RoleEditor/Res/Female/Materials\female_top-2_orange.mat
	/// </summary>
	/// <returns>The all files.</returns>
	/// <param name="folderPath">文件夹目录，eg:Assets/NEditor/RoleEditor/Res/Female/Materials</param>
	public static string[] FindAllFileURLs(string folderPath) {
		
		string[] fileURLs = Directory.GetFiles(folderPath,"*",SearchOption.AllDirectories);//注意这里要排除meta文件

		List<string> filsList = new List<string>();
		for (int i = 0; i < fileURLs.Length; i++) 
		{
			//NDebug.i("查找目录"+folderPath+"下文件"+i+"->"+fileURLs[i],true,"EditorHelper.FindAllFiles");

			//避免找到无用的.meta文件
			if (fileURLs [i].Contains (".meta"))continue;

			filsList.Add (fileURLs[i]);

		}
		return filsList.ToArray();
	}
		

	/// <summary>
	/// 创建一个名为name内容为go的prefab
	/// </summary>
	/// <returns>The prefab.</returns>
	/// <param name="go">Go.</param>
	/// <param name="name">Name.</param>
	public static UnityEngine.Object GeneratePrefab(GameObject go, string name,string folderPath = "Assets/"){

		if(!Directory.Exists (folderPath))
		{
			Directory.CreateDirectory (folderPath);
		}

		UnityEngine.Object tempPrefab = EditorUtility.CreateEmptyPrefab(folderPath + name + ".prefab");//TODO 这个创建位置以后要处理下
		
		tempPrefab = EditorUtility.ReplacePrefab(go, tempPrefab);

		return tempPrefab;
		
	}

	/// <summary>
	/// 这个方法是假的，用来记录下如何对比类型
	/// </summary>
	/// <returns><c>true</c>, if same type was been, <c>false</c> otherwise.</returns>
	/// <param name="obj">Object.</param>
	/// <param name="type">Type.</param>
	public static bool BeSameType(Object obj,Type type){
		//obj.GetType() == typeof(UnityEditor.DefaultAsset)
		return false;
	}

	#endregion
}
