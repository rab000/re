using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using System.Text;
/// <summary>
/// 换装角色打包编辑器(包含bone，parts，mat，animation)
/// 
/// 关于SkinnedMeshRenderer的说明
/// 主要包含1 sharedMesh 2 bones 3 materials
/// 
/// 注意：角色打包没用到依赖打包，都是单独打包
/// 原因是
/// 依赖打包的条件是:bundleA bundleB中如果有共同依赖bundleC,那么bundleC必须打为依赖，并优先与AB加载，否则如果没有依赖包那么bundleA和bundleB就会加载到重复资源
/// 现在看角色换装编辑器的导出有2部分  1是go(包含skinnedMeshRenderer 里面有mesh，bonenames，但已经把materail=null了，默认编辑器模型上也不要绑材质，因为同一部位材质可能为多个)    2是材质
/// 不存在依赖打包重复加载资源的问题
/// </summary>
class RoleEditor{
	
	static bool bShowLog = true;


//	#region 打完整role包

	//一键打包
	//具体方式为，首先找到RoleEditor/Res/下所有角色名目录，依次为所有角色打包
	//对与1个具体的角色，首先打包骨骼，然后是各个部位，然后统一打包所有部位的材质，然后分离所有动画（这个要手动自行建立动画组文件夹），然后统一打包动画
//	[MenuItem("RoleEditor/BuildRoleBundle")]
//	public static void BuildRoleBundle()
//	{
//		Log.i ("RoleEditor","CreateAllRoleAB","step1---开始创建全部角色相关AB");
//
//		EditorHelper.CreateFolder (RoleEditorHelper.ABOutputPath);
//
//		string roleResPath = RoleEditorHelper.EDITOR_ROLE_ORIGNAL_RES_PATH;				//RoleEditor资源根目录
//
//		string[] _roleFolderURLs = EditorHelper.GetSubFolderPaths(roleResPath);	//根目录下一级子目录，以不同角色的角色名为文件夹的一级
//
//		Log.i ("RoleEditor","CreateAllRoleAB","step2---找到所有RoleEditor下Res下的一级目录(每个角色一个目录) 目录数量:"+_roleFolderURLs.Length);
//		//遍历全部角色文件夹
//		for (int i = 0; i < _roleFolderURLs.Length; i++)
//		{
//			//开始处理模型(bone,part)
//			string _modelFolderUrl = _roleFolderURLs[i] + "/model/";			//model所在文件夹路径
//
//			string _matFolderUrl = _roleFolderURLs[i] + "/mat/";			//mat所在文件夹路径
//
//			string _animFolderUrl = _roleFolderURLs[i] + "/anim/";				//anim所在文件夹路径
//
//			ProcessModel(_modelFolderUrl);
//
//			ProcessMat(_matFolderUrl);
//
//			ProcessAnim(_animFolderUrl);
//
//		}
//
//		Log.i ("RoleEditor","CreateAllRoleAB","step3---收集bundleBuild结束，开始打包",bShowLog);
//		//BuildPipeline.BuildAssetBundles (RoleEditorHelper.ABOutputPath, RoleBundleBuildList.ToArray(), BuildAssetBundleOptions.CollectDependencies, BuildTarget.StandaloneWindows);
//
//		//Log.i ("RoleEditor","CreateAllRoleAB","step4---生成角色AB结束,开始清理临时资源",bShowLog);
//		//CleanTempAsset();
//
//		Log.i ("RoleEditor","CreateAllRoleAB","step5---清理临时资源完毕",bShowLog);
//		AssetDatabase.Refresh();//因为输出文件夹位于Asset中，为了方便显示，刷新下
//
//		Log.i ("RoleEditor","CreateAllRoleAB","step6---刷新工程目录显示完毕，打包完成",bShowLog);
//
//	}

//	#endregion


	#region func

	//记录下当前打包性别，创建文件夹用
	public static string CurBuildSex;

	//打包换装模型
	[MenuItem("RoleEditor/BuildChangeEquipBundle")]
	public static void BuildChangeEquipBundle()
	{
		
		EditorHelper.CreateFolder (RoleEditorHelper.ABOutputPath);

		DetectRepeatRes.LoadCheckDic ();//step1
		CheckDelRes();//检测是有dic中存在，但工程中不存在的资源

		string manFolderPath = RoleEditorHelper.EDITOR_ROLE_ORIGNAL_RES_PATH + "/man";

		string womenFolderPath = RoleEditorHelper.EDITOR_ROLE_ORIGNAL_RES_PATH + "/women";

		CurBuildSex = "man";

		ProcessChangeEquip (manFolderPath);

		CurBuildSex = "women";

		ProcessChangeEquip (womenFolderPath);

		AssetDatabase.Refresh();

		BuildPipeline.BuildAssetBundles (RoleEditorHelper.ABOutputPath,BuildAssetBundleOptions.None,BuildTarget.StandaloneWindows64);


		DetectRepeatRes.SaveCheckDic ();
	}


	//处理一组换装资源，传入man或women资源文件夹路径
	public static void ProcessChangeEquip(string folderPath)
	{
		//开始处理模型(bone,part)
		string _boneFolderUrl = folderPath + "/bone/";			//bone所在文件夹路径

		string _modelFolderUrl = folderPath + "/model/";		//model所在文件夹路径

		string _matFolderUrl = folderPath + "/mat/";			//mat所在文件夹路径

		string _animFolderUrl = folderPath+ "/anim/";			//anim所在文件夹路径

		ProcessBone(_boneFolderUrl);

		ProcessModel(_modelFolderUrl);

		ProcessMat(_matFolderUrl);

		ProcessAnim(_animFolderUrl);

	}

	#endregion



	#region 分布处理资源

	static void ProcessBone(string _boneFolderUrl)
	{
		Log.i ("RoleEditor","ProcessModel","开始打包骨骼，当前角色文件夹 _boneFolderUrl:"+_boneFolderUrl);
		string[] _modelURLs = EditorHelper.GetSubFilesPaths(_boneFolderUrl);
		GameObject _modelGo = null;
		for (int j = 0; j < _modelURLs.Length; j++)//理论上可以有多个bone，这里资源文件夹里只放一个
		{
			if (!_modelURLs[j].EndsWith(".meta")){


				bool b = CheckNeedUpdate (_modelURLs[j]);//step2
				if (!b)//资源没变化，不需要再分拆一次骨骼
					continue;

				Log.i ("RoleEditor","ProcessBone","开始骨骼地址 _modelURLs["+j+"]->"+_modelURLs[j]);
				Object _modelObj = AssetDatabase.LoadAssetAtPath<Object>(EditorHelper.ChangeToRelativePath(_modelURLs[j]));
				_modelGo = GameObject.Instantiate(_modelObj) as GameObject;
				_modelGo.name = _modelObj.name;
				GenerateBoneAB(_modelGo);
				GameObject.DestroyImmediate(_modelGo);//清理掉临时
			}
		}
	}

	static void ProcessModel(string _modelFolderUrl)
	{
		Log.i ("RoleEditor","ProcessModel","开始打包部位，当前角色文件夹 _modelFolderUrl:"+_modelFolderUrl);
		string[] _modelURLs = EditorHelper.GetSubFilesPaths(_modelFolderUrl);
		GameObject _modelGo = null;
		for (int j = 0; j < _modelURLs.Length; j++){//注意一个Model文件夹里目前只允许出现1个model，这个model包括所有换装模型
			if (!_modelURLs[j].EndsWith(".meta")){

				bool b = CheckNeedUpdate (_modelURLs[j]);//step2
				if (!b)//资源没变化，不需要再分拆一次模型
					continue;

				Log.i ("RoleEditor","CreateAllRoleAB","开始遍历模型 模型地址 _modelURLs["+j+"]->"+_modelURLs[j]);
				Object _modelObj = AssetDatabase.LoadAssetAtPath<Object>(EditorHelper.ChangeToRelativePath(_modelURLs[j]));
				_modelGo = GameObject.Instantiate(_modelObj) as GameObject;
				_modelGo.name = _modelObj.name;
				//GenerateBoneAB(_modelGo);
				GeneratePartAB(_modelGo);
				GameObject.DestroyImmediate(_modelGo);//清理掉临时
			}
		}
	}

	/// <summary>
	/// 复制材质到打包目录
	/// </summary>
	/// <param name="_modelFolderUrl">Model folder URL.</param>
	static void ProcessMat(string _matFolderUrl)
	{
		Log.i ("RoleEditor","ProcessMat","开始copy材质，当前材质文件夹 _matFolderUrl->"+_matFolderUrl);
		//先将材质copy到用于打包的asset文件夹，然后再设定bundleName
		string[] matURLs = EditorHelper.FindAllFileURLs(_matFolderUrl);
		//创建目标mat文件夹，如果文件夹不存在则AssetDatabase.CopyAsset不会生效
		string folderPath = RoleEditorHelper.EDITOR_TEMP_ASSET_PATH + "/" + CurBuildSex + "/mat/";
		EditorHelper.CreateFolder(folderPath);
		for (int i = 0; i < matURLs.Length; i++) 
		{

			bool b = CheckNeedUpdate (matURLs[i]);//step2
			if (!b)//资源没变化，不需要移动一次mat
				continue;
			

			string orignalPath = matURLs[i];
			string tempRelePath = orignalPath.Substring (RoleEditorHelper.EDITOR_ROLE_ORIGNAL_RES_PATH.Length);
			string newPath = RoleEditorHelper.EDITOR_TEMP_ASSET_PATH + tempRelePath; 
			string orignalRelePath = EditorHelper.ChangeToRelativePath (orignalPath);
			string newRelePath = EditorHelper.ChangeToRelativePath (newPath);
			//Debug.LogError ("-----copy:--op:"+orignalRelePath+" --np:"+newRelePath);
			EditorHelper.CopyAsset(orignalRelePath,newRelePath);
		}


		Log.i ("RoleEditor","ProcessMat","开始打包材质，当前材质文件夹 folderPath->"+folderPath);
		GeneratePartMatAB (folderPath);
	}

	static void ProcessAnim(string _animFolderUrl)
	{
		//动画分离到指定文件夹xxx，分离动画流程需要手动，暂时没法自动进行
		string[] _animGroupURLs = EditorHelper.GetSubFolderPaths(_animFolderUrl);
		Log.i ("RoleEditor","CreateAllRoleAB","准备分离动作 找到animGroup数量:"+_animFolderUrl.Length);
		
		for (int j = 0; j < _animGroupURLs.Length; j++) 
		{
			string[] _animURLs = EditorHelper.GetSubFilesPaths(_animGroupURLs[j]);
			for (int k = 0; k < _animURLs.Length; k++)
			{

				bool b = CheckNeedUpdate (_animURLs[k]);//step2
				if (!b)//资源没变化，不需要再分离动画
					continue;

				Log.i ("RoleEditor","CreateAllRoleAB","开始分离动作,当前动作组:"+_animGroupURLs[j]+" 当前分离动作"+_animURLs[k]);
				AnimSeparate(_animURLs[k]);
			}
		}


		//把分离出的动画文件 设置bundleName
		string _roleName = CurBuildSex;//角色文件夹名 Female，生成动画bundle名时要用
		string _tempAnimClipFolderURL = RoleEditorHelper.EDITOR_TEMP_ASSET_PATH+"/"+CurBuildSex+"/anim/";
		string[] _tempAnimClipGroupURLs = EditorHelper.GetSubFolderPaths(_tempAnimClipFolderURL);

		//Log.i ("RoleEditor","CreateAllRoleAB","开始打包已分离好的动画片段组，当前角色文件夹_folderURLS["+i+"]->"+_roleFolderURLs[i]+" 当前动画组数量:"+_tempAnimClipGroupURLs.Length);
		for (int j = 0; j < _tempAnimClipGroupURLs.Length; j++){
						
			string[] _animURLs = EditorHelper.GetSubFilesPaths(_tempAnimClipGroupURLs[j]);//获取动画组内的动画绝对地址
			string _animGroupName = Path.GetFileName(_tempAnimClipGroupURLs[j]);	//animGroup eg:common
			for(int k=0;k<_animURLs.Length;k++)
			{
				_animURLs [k] = EditorHelper.ChangeToRelativePath( _animURLs[k]); //打bundle需要相对路径

				string bundleName = CurBuildSex +"/anim/"+ _animGroupName;
				string relePath =  _animURLs [k];
				//Debug.LogError ("====>>>>>>>>>>relePath:"+relePath+" bundleName:"+bundleName);
				EditorHelper.SetAssetBundleName (relePath,bundleName);

			}

			Log.i ("RoleEditor","CreateAllRoleAB","开始打包动画片段，当前动画组 _animGroupURLs["+j+"]->"+_animGroupURLs[j]);

		}
	}


	#endregion

	
	#region 换装模型打包

	/// <summary>
	/// 生成基本骨骼ab
	/// </summary>
	/// <param name="go">Go.</param>
	static void GenerateBoneAB(GameObject go)
	{
		string _goName = go.name.ToLower();

		Log.i ("RoleEditor","GenerateBoneAB","开始为角色:"+go.name+"打骨骼包",bShowLog);

		GameObject _roleGoClone = Object.Instantiate(go) as GameObject;

		foreach (Animation anim in _roleGoClone.GetComponentsInChildren<Animation>()){
			anim.animateOnlyIfVisible = false;//即使物体在屏幕外，也播放动画
		}

		//删除包含SkinnedMeshRenderer组件的物体，实际就是角色部件，只留下骨骼相关
		foreach (SkinnedMeshRenderer smr in _roleGoClone.GetComponentsInChildren<SkinnedMeshRenderer>()){
			Object.DestroyImmediate(smr.gameObject);
		}

		// Assets/Asset4Build/hx/bone/
		string boneAssetFolderPath = EditorHelper.ChangeToRelativePath (RoleEditorHelper.EDITOR_TEMP_ASSET_PATH) + "/"+CurBuildSex+"/bone/";
		//在Asset文件夹中创建名为bone的asset
		Object bonePrefab = EditorHelper.GeneratePrefab(_roleGoClone, RoleEditorHelper.BoneAB_AssetName,boneAssetFolderPath);

		Object.DestroyImmediate(_roleGoClone);

		string bundleName = CurBuildSex +"/bone/"+ go.name.ToLower() + "_bone";
		string relePath =  boneAssetFolderPath+"bone.prefab";
		//Debug.LogError ("---------------relePath:"+relePath+" bundleName:"+bundleName);
		EditorHelper.SetAssetBundleName (relePath,bundleName);

	}

	static void GeneratePartAB(GameObject go){

		string _goName = go.name.ToLower();

		Log.i ("RoleEditor","GeneratePartAB","准备为角色:"+go.name+"打部位包",bShowLog);

		foreach (SkinnedMeshRenderer smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
		{

			Log.i ("RoleEditor","GeneratePartAB","开始为部位："+smr.name+"打包",bShowLog);

			Log.i ("RoleEditor","GeneratePartAB","打包mesh",bShowLog);
			//meshs---------------------------------------------------------
			GameObject rendererClone = GameObject.Instantiate(smr.gameObject);
			rendererClone.name = smr.gameObject.name;

			//这里说明下part打包，单个part包里包括mesh和boneNames两部分资源，命名也使用mesh和boneNames
			//打包时因为需要保持临时asset资源，这些资源还不能重名，为了保持输出bundle载入时能使用统一的asset名
			//这里新建一些临时文件夹，打包完再清理掉
			string tempPartFolderPath =  EditorHelper.ChangeToRelativePath( RoleEditorHelper.EDITOR_TEMP_ASSET_PATH)+"/" +CurBuildSex+"/model/" + _goName+"/"+smr.name.ToLower()+"/";

			Object PartPrefab = EditorHelper.GeneratePrefab(rendererClone, RoleEditorHelper.PartAB_MeshAssetName,tempPartFolderPath);

			Object.DestroyImmediate(rendererClone);

			Log.i ("RoleEditor","GeneratePartAB","打包骨骼名称",bShowLog);

			//NAFIO INFO 
			//bone信息可以考虑以.bytes形式保存到ab中，这样就可以避免使用StringHolder这个类
			//bones---------------------------------------------------------

			List<string> boneNames = new List<string>();
			foreach (Transform t in smr.bones){
				boneNames.Add(t.name);
			}

			string stringholderpath = tempPartFolderPath + RoleEditorHelper.PartAB_BonenamesAssetName+".asset";//创建临时asset，用于存储stirng[]类型的骨骼名称

			StringHolder holder = ScriptableObject.CreateInstance<StringHolder> ();//StringHolder类继承自ScriptableObject，可以被以asset形式打入bundle包，stringHolder中有string[] 存储boneNames

			holder.content = boneNames.ToArray();

			AssetDatabase.CreateAsset(holder, stringholderpath);


			//NDebug.i("打包材质",bShowLog,"RoleEditor.GeneratePartAB");

			//NAFIO INFO 这里涉及到两个问题 
			//1 没有合并材质，因为如果一个部位有两个材质，合并会出问题（细想下，只有角色穿套装合并材质才有意义，如果混搭，合并了套装的材质也无意义）
			//2 材质可能有多个，读取的时候排除第一个meshs，和最后一个bones，其他都是材质
			//materials---------------------------------------------------------
			//			List<Material> materials = EditorHelpers.CollectAll<Material>(GetMaterailsPath(smr.gameObject));
			//			
			//			Debug.Log("================smrName:"+smr.name);
			//			
			//			for(int i=0;i<materials.Count;i++){
			//				
			//				if (materials[i].name.Contains(smr.name.ToLower())) {
			//					
			//					Debug.Log("-----------matName:"+materials[i].name);
			//					
			//					toinclude.Add(materials[i]);
			//				}
			//				
			//			}


			string partAssetFolderPath = EditorHelper.ChangeToRelativePath (RoleEditorHelper.EDITOR_TEMP_ASSET_PATH) + "/"+CurBuildSex+"/model/"+_goName+"/"+smr.name.ToLower();
			string bundleName = CurBuildSex +"/model/"+ _goName + "/"+smr.name.ToLower();
			string relePath =  partAssetFolderPath+"/mesh.prefab";
			//Debug.LogError ("relePath:"+relePath+" bundleName:"+bundleName);
			EditorHelper.SetAssetBundleName (relePath,bundleName);

			string atrriPath = partAssetFolderPath+"/bonenames.asset"; 
			string atrriBundleName = bundleName;
			EditorHelper.SetAssetBundleName (atrriPath,atrriBundleName);
		}

	}


	static void GeneratePartMatAB(string matFolderUrl)
	{
		string[] matURLs = EditorHelper.FindAllFileURLs(matFolderUrl);
		Log.i ("RoleEditor","GeneratePartMatAB","打包材质,材质数量:"+matURLs.Length,bShowLog);
		string matName = "";
		for(int i=0;i<matURLs.Length;i++)
		{
			matName = EditorHelper.GetFileNameFromPath (matURLs [i], false);

			string bundleName = CurBuildSex + "/mat/" + matName;
			string relePath =  EditorHelper.ChangeToRelativePath(matURLs[i]);

			//Debug.LogError ("------>bname:"+bundleName+" relePath:"+relePath);
			EditorHelper.SetAssetBundleName (relePath,bundleName);

		}
	}

	/// <summary>
	/// 从fbx中分离anim
	/// </summary>
	/// <param name="animURL">anim.fpx的地址，包含animationClip的fbx文件</param>
	static void AnimSeparate(string animURL){

		if (animURL.EndsWith (".meta"))return;

		string reletiveAnimURL = EditorHelper.ChangeToRelativePath(animURL);						//获取相对Asset目录的anim的路径

		Object Asset = AssetDatabase.LoadAssetAtPath<Object>(reletiveAnimURL);						//载入原始anim座位obj存在

		string _assetPath = AssetDatabase.GetAssetPath(Asset);										//anim.fbx完整路径 			eg:Assets/NEditor/RoleEditor/Res/Female/Anim/common/Female@attack.fbx
		string _assetFolderPath = EditorHelper.GetParentFolderPath(_assetPath);						//anim.fbx所在目录 			eg:Assets/NEditor/RoleEditor/Res/Female/Anim/common
		string _animGroupName = EditorHelper.GetFileNameFromPath(_assetFolderPath);					//animGroupName 			eg:common
		string _assetNameWithoutExt = EditorHelper.GetFileNameFromPath(_assetPath,true);			//anim.fbx名(不包含后缀)  		eg:Female@attack
		string _roleName = _assetNameWithoutExt.Substring (0, _assetNameWithoutExt.IndexOf ('@')); 	//anim角色名					eg:Female
		string _animName = _assetNameWithoutExt.Substring(_assetNameWithoutExt.IndexOf ('@')+1);	//anim名						eg:attack

		//输出路径，临时目录/角色名/AnimationClip/AnimGroupName/
		string _outputAnimFolderPath = RoleEditorHelper.EDITOR_TEMP_ASSET_PATH+"/"+CurBuildSex+"/anim/"+_animGroupName;//输出AnimClip的目录
		string _outputAnimName = _animName+".anim";
		string _outputAnimFullPath = _outputAnimFolderPath+"/"+_outputAnimName;

		//		Debug.Log("anim _assetPath:"+_assetPath);
		//		Debug.Log("anim _assetFolderPath:"+_assetFolderPath);
		//		Debug.Log("anim _animGroupName:"+_animGroupName);
		//		Debug.Log("anim _roleName:"+_roleName);
		//		Debug.Log("anim _animName:"+_animName);
		//		Debug.Log("anim _assetNameWithoutExt:"+_assetNameWithoutExt);
		//		Debug.Log("anim _outputAnimPath:"+_outputAnimPath);
		//		Debug.Log("anim _outputAnimFullPath:"+_outputAnimFullPath);

		//输出目录不存在就创建
		EditorHelper.CreateFolder(_outputAnimFolderPath);

		//如果输出目录已经存在当前要生成的文件就先删除
		EditorHelper.DeleteFileIfExists(_outputAnimFullPath);

		var _objs = AssetDatabase.LoadAllAssetsAtPath (reletiveAnimURL);//这里会获取fbx中的每个object
		var originalClip = System.Array.Find<Object> (_objs, item =>item is AnimationClip);
		var copyClip = Object.Instantiate (originalClip);
		AssetDatabase.CreateAsset (copyClip, EditorHelper.ChangeToRelativePath(_outputAnimFullPath));
		Log.i ("RoleEditor","CleanTempAsset","分离出动作文件:"+_outputAnimFullPath,bShowLog);
	}

	#endregion

	#region 预处理原始资源(变更检测)

	//处理每个要预处理的资源时都检查下,检测当前资源是否需要重新处理 ,返回true需要重新处理
	public static bool CheckNeedUpdate(string fileAbsPath)
	{
		string realModifyPath = DetectRepeatRes.GetRealModifyURL (fileAbsPath);

		bool b = DetectRepeatRes.GetMD5Dic().ContainsKey (realModifyPath);

		if (b) 
		{
			//存在
			bool bChanged = DetectRepeatRes.BeFileChanged(fileAbsPath);
			if (bChanged) 
			{
				string md5 = DetectRepeatRes.GetMD5(realModifyPath);
				DetectRepeatRes.ModifyMD5Dic(realModifyPath,md5);
				return true;
			}
		} 
		else
		{
			//不存在
			string md5 = DetectRepeatRes.GetMD5(realModifyPath);
			DetectRepeatRes.Add2MD5Dic(realModifyPath,md5);
			return true;
		}

		return false;

	}

	//检查是否有资源被删除，如果原始资源被删除，那么要清空待打包asset和打出来的bundle资源
	public static void CheckDelRes()
	{
		var dic = DetectRepeatRes.GetMD5Dic ();

		foreach (var p in dic) 
		{
			bool b = EditorHelper.BeFileExist(p.Key);

			if (!b) 
			{
				//从记录表中剔除
				DetectRepeatRes.DelFromMD5Dic (p.Key);

				DelAssetAndBundle(p.Key);

			}

		}

	}

	//这段代码比较特殊
	//用来删除过期的asset，bundle文件
	//检测asset目录是否有这个资源的生成文件，有则删除
	//检测bundle目录是否有这个资源的生成文件,有则删除
	private static void DelAssetAndBundle(string fileAbsPath)
	{
		//问题，怎么判断文件类型，anim，model，mat，bone
		string resType = null;

		if (fileAbsPath.Contains ("anim"))
			resType = "anim"; 

		if (fileAbsPath.Contains ("bone"))
			resType = "bone"; 

		if (fileAbsPath.Contains ("model"))
			resType = "model"; 

		if (fileAbsPath.Contains ("mat"))
			resType = "mat";

		string relePath = fileAbsPath.Substring(RoleEditorHelper.EDITOR_ROLE_ORIGNAL_RES_PATH.Length);

		string delPath = null;

		switch (resType) 
		{
		case "anim":
			string pPath = EditorHelper.GetParentFolderPath (relePath);
			delPath = RoleEditorHelper.EDITOR_TEMP_ASSET_PATH + pPath;
			break;
		case "model":

			//fbx的文件夹相对路径
			string parentPath = EditorHelper.GetParentFolderPath (relePath);
			//没后缀的名称
			string fileNameWithEx = EditorHelper.GetFileNameFromPath (fileAbsPath, true);
			//Asset中存放模型的文件夹路径
			delPath = RoleEditorHelper.EDITOR_TEMP_ASSET_PATH + parentPath + "/" + fileNameWithEx;

			EditorHelper.DeleteFolder (delPath,true);

			break;
		case "bone":
		case "mat":
			delPath = RoleEditorHelper.EDITOR_TEMP_ASSET_PATH + relePath;
			//EditorHelper.DeleteFileIfExists ();
			break;
		}

		Debug.Log ("RoleEditor.DelAssetAndBundle relePath:" + relePath+" delPath:"+delPath);

	} 


	#endregion
}


