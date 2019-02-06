//using System.Collections.Generic;
//using System.IO;
//using UnityEditor;
//using UnityEngine;
//using System;
//using Object = UnityEngine.Object;
///// <summary>
///// 换装角色打包编辑器(包含bone，parts，mat，animation)
///// 
///// 使用说明
///// 1 把资源按指定目录结构放到RoleEditor/Res/下，Female/Anim/下要手动建立分组文件夹
///// 具体结构如下
///// Female/							模型角色名
///// ------Anim/						原始动作fbx目录
///// -----------AnimGroup/			动作组目录
/////	--------------------anim.fbx	具体动作文件
///// ------Materials/				材质目录
///// --------------------xx.mat		具体mat文件
///// ------Model/					角色模型目录
///// --------------------xx.fbx		具体换装角色模型			
///// ------Texture					tex目录
///// 
///// 2 使用NEditor->RoleEditor->BuildRoleBundle
///// 2017升级unity后不再使用选中资源再打包的方式，脚本自动查找相应文件夹，自动创建所有换装角色bundle
///// 
///// 关于SkinnedMeshRenderer的说明
///// 主要包含1 sharedMesh 2 bones 3 materials
///// 
///// 注意：角色打包没用到依赖打包，都是单独打包
///// 原因是
///// 依赖打包的条件是:bundleA bundleB中如果有共同依赖bundleC,那么bundleC必须打为依赖，并优先与AB加载，否则如果没有依赖包那么bundleA和bundleB就会加载到重复资源
///// 现在看角色换装编辑器的导出有2部分  1是go(包含skinnedMeshRenderer 里面有mesh，bonenames，但已经把materail=null了，默认编辑器模型上也不要绑材质，因为同一部位材质可能为多个)    2是材质
///// 不存在依赖打包重复加载资源的问题
///// </summary>
//class RoleEditor2{
//	
//	static bool BeShowLog = true;
//	
//	#region 可变变量
//	
//	/// <summary>
//	/// 输出文件夹位置
//	/// 路径不放在Asset文件夹内部，因为会生成多余的meta文件，不方便直接提取
//	/// </summary>
//	/// <value>The output path.</value>
//	static string ABOutputPath{
//		
//		get{ return Path.GetDirectoryName(Application.dataPath)+"/data/res/role/";}
//		
//		//get { return "Assets" + Path.DirectorySeparatorChar + "Output" + Path.DirectorySeparatorChar + "Role" + Path.DirectorySeparatorChar; }
//		
//	}
//	
//	/// <summary>
//	/// 生成bundle文件后缀名
//	/// </summary>
//	/// <value>The name of the suffix.</value>
//	static string ABSuffixName{
//		
//		get {return ".n"; }	
//	}
//	
//	/// <summary>
//	/// 角色公用骨骼AB 中唯一asset，mainasset的名称
//	/// 这个可以直接WWW.assetBundle.mainAsset来获取
//	/// 也可以和PartAB一样使用asset name进行资源获取(如果是这种就要在读取asset时用BoneAB_AssetName这个名称读取)
//	/// </summary>
//	/// <value>The name of the bone A b_ asset.</value>
//	static string BoneAB_AssetName{
//		
//		get{return "basebone";}
//	}
//	
//	/// <summary>
//	/// 角色部位AB中mesh asset的名称
//	/// 但加载角色部位的mesh时，需要WWW.assetBundle.LoadAsync("meshName", typeof(GameObject));
//	/// </summary>
//	/// <value>The name of the mesh asset.</value>
//	static string PartAB_MeshAssetName{
//		
//		get{return "mesh";}
//		
//	}
//	
//	/// <summary>
//	/// 角色部位AB中boneNames asset的名称
//	/// 但加载角色部位的bonenames时，需要WWW.assetBundle.LoadAsync("bonenames", typeof(StringHolder));
//	/// </summary>
//	/// <value>The name of the mesh asset.</value>
//	static string PartAB_BonenamesAssetName{
//		
//		get{return "bonenames";}
//	}
//	
//	/// <summary>
//	/// 角色编辑器资源文件夹关键字
//	/// 用于忽略角色编辑器资源文件夹以外的资源，防止意外打错资源
//	/// </summary>
//	/// <value>The res folder.</value>
//	static string ResFolder{
//		
//		get{return "/RoleEditor/Res/";}
//		
//	}
//
//	/// <summary>
//	/// 用来收集所有角色相关的bundleBuild
//	/// </summary>
//	static List<AssetBundleBuild> RoleBundleBuildList;
//
//	#endregion
//
//
//	#region 临时变量，用于保存需要最后清理的资源
//	//这部分存储的是临时生成的prefab文件，这些文件在生成bundle完毕后会被统一清理掉
//	static List<Object> boneTempPrefabList;				//骨骼临时
//	static List<Object> partTempPrefabList;				//部位临时
//	static List<string> partTempStringholderpathList;	//部位骨骼信息临时
//	#endregion
//
//
//	#region 老的打换装角色包的方法，直接打包
//
//
//
//
//
//
//	#endregion
//
//
//	#region 新版打包换装角色的方法，只分离换装模型，不直接打bundle
//	/// <summary>
//	/// 拆分原始角色，生成用于打包换装bundle的临时资源文件夹，为打换装bundle做准备
//	/// </summary>
//	//[MenuItem("NEditor/RoleEditor/TSplitRole")]
//	public static void GenerateRoleTempAsset(){
//
//		//RoleBundleBuildList = new List<AssetBundleBuild>();
//		Log.i ("RoleEditor","GenerateRoleTempAsset","step1---开始创建全部角色相关AB");
//		//如果输出文件夹不存在，创建输出文件夹
//		if (!Directory.Exists(ABOutputPath)){
//			Log.i ("RoleEditor","GenerateRoleTempAsset","输出路径"+ABOutputPath+"不存在，创建输出路径");
//			Directory.CreateDirectory(ABOutputPath);
//		}
//
//		//换装角色原始资源文件夹
//		string roleResPath = EditorHelper.EDITOR_ROLE_RES_PATH;					
//
//		//根目录下一级子目录，以不同角色的角色名为文件夹的一级
//		string[] _roleFolderURLs = EditorHelper.GetSubFolderPaths(roleResPath);	
//
//
//		Log.i ("RoleEditor","GenerateRoleTempAsset","step2---找到所有RoleEditor下Res下的一级目录(每个角色一个目录) 目录数量:"+_roleFolderURLs.Length);
//
//		//遍历全部角色文件夹
//		for (int i = 0; i < _roleFolderURLs.Length; i++){
//			//NINFO 首先检测，TempAsset中是否有当前要处理的角色的目录，有得话就什么都不做，没有就进行下一步拆分
//			string _roleFolderName = EditorHelper.GetFileNameFromPath(_roleFolderURLs[i]);
//			string _roleTempAssetFolderPath = EditorHelper.EDITOR_ROLE_ASSET_PATH + "/" + _roleFolderName;
//
//			Log.i ("RoleEditor","GenerateRoleTempAsset","遍历角色原始资源文件夹i="+i+" 角色（文件夹）名称:"+_roleFolderName+" 生成临时角色Asset目录Path:"+_roleTempAssetFolderPath,BeShowLog);
//			if (Directory.Exists (_roleTempAssetFolderPath)) {
//				Log.i("RoleEditor","GenerateRoleTempAsset","角色:"+_roleFolderName +" 临时Asset资源文件夹存在，不拆分当前角色，继续处理下一角色",BeShowLog);
//				continue;
//			}
//			else{
//				Log.i("RoleEditor","GenerateRoleTempAsset","角色:"+_roleFolderName +" 临时Asset资源文件夹不存在，开始拆分",BeShowLog);
//			}
//				
//
//			//开始处理模型(bone,part)
//			string _modelFolderUrl = _roleFolderURLs[i] + "/Model/";			//model所在文件夹路径
//			string _matFolderUrl = _roleFolderURLs[i] + "/Materials/";			//mat所在文件夹路径
//			string _animFolderUrl = _roleFolderURLs[i] + "/Anim/";				//anim所在文件夹路径
//			string[] _modelURLs = EditorHelper.GetSubFilesPaths(_modelFolderUrl);
//
//
//			Log.i ("RoleEditor","GenerateRoleTempAsset","开始打包骨骼部位，当前角色文件夹 _folderURLS["+i+"]->"+_roleFolderURLs[i]);
//			GameObject _modelGo = null;
//			for (int j = 0; j < _modelURLs.Length; j++){//注意一个Model文件夹里目前只允许出现1个model，这个model包括所有换装模型
//				if (!_modelURLs[j].EndsWith(".meta")){
//					Log.i ("RoleEditor","CreateAllRoleAB","开始遍历模型 模型地址 _modelURLs["+j+"]->"+_modelURLs[j]);
//					Object _modelObj = AssetDatabase.LoadAssetAtPath<Object>(EditorHelper.ChangeToRelativePath(_modelURLs[j]));
//					_modelGo = GameObject.Instantiate(_modelObj) as GameObject;
//					_modelGo.name = _modelObj.name;
//					GenerateBoneAB1(_modelGo);
//					GeneratePartAB1(_modelGo);
//					GameObject.DestroyImmediate(_modelGo);//清理掉临时
//				}
//			}
//
//
//			Log.i ("RoleEditor", "GenerateRoleTempAsset", "开始copy材质,因为每个mat只对应一个tex所以不单独copy tex了",BeShowLog);
//			string targetMatFolderPath = EditorHelper.EDITOR_ROLE_ASSET_PATH +"/"+_roleFolderName+ "/Materials/";
//			//Debug.LogError ("=====>tartgetMat:"+targetMatFolderPath);
//			EditorHelper.CreateFolder(targetMatFolderPath);//输出目录不存在就创建
//
//			//找到所有mat
//			string[] matURLs = EditorHelper.FindAllFileURLs(_matFolderUrl);
//			//Debug.LogError ("=====>matNum:"+matURLs.Length);
//			for(int j=0;j<matURLs.Length;j++){
//				string oneMatName = EditorHelper.GetFileNameFromPath(matURLs [j]);//这里不需要后缀
//				string _orignalPath = EditorHelper.ChangeToRelativePath( _matFolderUrl + oneMatName);
//				string _targetPath = EditorHelper.ChangeToRelativePath(targetMatFolderPath + oneMatName);
//				Log.i ("RoleEditor", "GenerateRoleTempAsset","===>移动mat从:"+_orignalPath+" 到"+_targetPath,BeShowLog);
//				EditorHelper.CopyAsset(_orignalPath,_targetPath);
//			}
//
//			//动画分离到指定文件夹xxx，分离动画流程需要手动，暂时没法自动进行
//			string[] _animGroupURLs = EditorHelper.GetSubFolderPaths(_animFolderUrl);
//			Log.i ("RoleEditor","GenerateRoleTempAsset","准备分离动作 找到animGroup数量:"+_animFolderUrl.Length);
//
//			for (int j = 0; j < _animGroupURLs.Length; j++) {
//				string[] _animURLs = EditorHelper.GetSubFilesPaths(_animGroupURLs[j]);
//				for (int k = 0; k < _animURLs.Length; k++)
//				{
//                    string s = _animURLs[k];
//                    if(s.EndsWith(".DS_Store")){
//                        Log.i("RoleEditor", "GenerateRoleTempAsset", "DS_Store文件存在,名字为:"+s);
//                        continue;
//                    }
//					Log.i ("RoleEditor","GenerateRoleTempAsset","开始分离动作,当前动作组:"+_animGroupURLs[j]+" 当前分离动作"+_animURLs[k]);
//					AnimSeparate1(_animURLs[k]);
//				}
//			}
//
//
//			//把分离出的动画文件信息存入对应bundleBuild
//			//string _roleName = Path.GetFileName(_roleFolderURLs[i]);//角色文件夹名 Female，生成动画bundle名时要用
//			string _tempAnimClipFolderURL = EditorHelper.EDITOR_ROLE_ASSET_PATH+"/"+_roleFolderName+"/AnimationClip/";
//			string[] _tempAnimClipGroupURLs = EditorHelper.GetSubFolderPaths(_tempAnimClipFolderURL);
//			//TODO 下一步，这里_animGroupURLs不是分离好的anim的文件夹，需要重新指定
//			Log.i ("RoleEditor","GenerateRoleTempAsset","开始打包已分离好的动画片段组，当前角色文件夹_folderURLS["+i+"]->"+_roleFolderURLs[i]+" 当前动画组数量:"+_tempAnimClipGroupURLs.Length);
//			for (int j = 0; j < _tempAnimClipGroupURLs.Length; j++){
//
//				string[] _animURLs = EditorHelper.GetSubFilesPaths(_tempAnimClipGroupURLs[j]);//获取动画组内的动画绝对地址
//				for(int k=0;k<_animURLs.Length;k++){
//					_animURLs [k] = EditorHelper.ChangeToRelativePath( _animURLs[k]); //打bundle需要相对路径
//				}
//
//				string _animGroupName = EditorHelper.GetFileNameFromPath(_tempAnimClipGroupURLs[j]);	//animGroup eg:common
//				Log.i ("RoleEditor","GenerateRoleTempAsset","开始打包动画片段，当前动画组 _animGroupURLs["+j+"]->"+_animGroupURLs[j]);
//				AssetBundleBuild ab = new AssetBundleBuild();
//				ab.assetBundleName = _roleFolderName + "_anim" +"_" + _animGroupName + ABSuffixName;
//				ab.assetNames = _animURLs;
//				//RoleBundleBuildList.Add(ab);
//			}
//
//		}
//
//		//NINFO role不再单独自己打包，跟ui，scene一起打包
//		//Log.i ("RoleEditor","GenerateRoleTempAsset","step3---收集bundleBuild结束，开始打包",BeShowLog);
//		//BuildPipeline.BuildAssetBundles (ABOutputPath, RoleBundleBuildList.ToArray(), BuildAssetBundleOptions.CollectDependencies, EditorUserBuildSettings.activeBuildTarget);
//
//		//Log.i ("RoleEditor","GenerateRoleTempAsset","step4---生成角色AB结束,开始清理临时资源",BeShowLog);
//		//CleanTempAsset();
//
//		Log.i ("RoleEditor","GenerateRoleTempAsset","step3---全部角色拆分完毕",BeShowLog);
//		AssetDatabase.Refresh();//因为输出文件夹位于Asset中，为了方便显示，刷新下
//
//		//Log.i ("RoleEditor","GenerateRoleTempAsset","step6---刷新工程目录显示完毕，打包完成",BeShowLog);
//	}
//
//
//	/// <summary>
//	/// 生成基本骨骼ab
//	/// </summary>
//	/// <param name="go">Go.</param>
//	static void GenerateBoneAB1(GameObject go)
//	{
//		boneTempPrefabList = new List<Object>();
//		string _goName = go.name.ToLower();
//		Log.i ("RoleEditor","GenerateBoneAB","开始为角色:"+go.name+"打骨骼包",BeShowLog);
//		//NDebug.i(,bShowLog,"RoleEditor.GenerateBoneAB");
//
//		GameObject _roleGoClone = Object.Instantiate(go) as GameObject;
//
//		foreach (Animation anim in _roleGoClone.GetComponentsInChildren<Animation>()){
//			anim.animateOnlyIfVisible = false;//即使物体在屏幕外，也播放动画
//		}
//
//		//删除包含SkinnedMeshRenderer组件的物体，实际就是角色部件，只留下骨骼相关
//		foreach (SkinnedMeshRenderer smr in _roleGoClone.GetComponentsInChildren<SkinnedMeshRenderer>()){
//			Object.DestroyImmediate(smr.gameObject);
//		}
//
//		string _tempBoneFolderPath = EditorHelper.ChangeToRelativePath (EditorHelper.EDITOR_ROLE_ASSET_PATH) + "/"+go.name.ToLower()+"/";
//		//在Asset文件夹中创建名为basebone临时asset
//		Object boneTempPrefab = EditorHelper.GeneratePrefab(_roleGoClone, BoneAB_AssetName,_tempBoneFolderPath);
//		boneTempPrefabList.Add(boneTempPrefab);
//
//		Object.DestroyImmediate(_roleGoClone);
//
//		string path = ABOutputPath + _goName +"_"+BoneAB_AssetName+ABSuffixName; //female_basebone.assetbundle     角色名+_basebone+ABSuffixName
//
//		EditorHelper.DeleteFileIfExists(path);	
//
//		AssetBundleBuild ab = new AssetBundleBuild();
//		ab.assetBundleName = _goName +"_"+BoneAB_AssetName+ABSuffixName;
//		string[] assetUrls = new string[1];
//		assetUrls[0] = _tempBoneFolderPath + BoneAB_AssetName + ".prefab";
//		ab.assetNames = assetUrls;
//		//RoleBundleBuildList.Add(ab);
//
//	}
//
//
//	static void GeneratePartAB1(GameObject go){
//
//		partTempPrefabList = new List<Object>();
//		partTempStringholderpathList = new List<string> ();
//
//		string _goName = go.name.ToLower();
//
//		Log.i ("RoleEditor","GeneratePartAB","准备为角色:"+go.name+"打部位包",BeShowLog);
//
//		foreach (SkinnedMeshRenderer smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true)){
//
//			Log.i ("RoleEditor","GeneratePartAB","开始为部位："+smr.name+"打包",BeShowLog);
//			//用于集中meshs,materials,boneName的ScriptableObject,一并打包到assetBundle
//			List<string> toincludeStr = new List<string> ();
//
//			Log.i ("RoleEditor","GeneratePartAB","打包mesh",BeShowLog);
//			//meshs---------------------------------------------------------
//			GameObject rendererClone = GameObject.Instantiate(smr.gameObject);
//			rendererClone.name = smr.gameObject.name;
//
//			//这里说明下part打包，单个part包里包括mesh和boneNames两部分资源，命名也使用mesh和boneNames
//			//打包时因为需要保持临时asset资源，这些资源还不能重名，为了保持输出bundle载入时能使用统一的asset名
//			//这里新建一些临时文件夹，打包完再清理掉
//			string tempPartFolderPath =  EditorHelper.ChangeToRelativePath( EditorHelper.EDITOR_ROLE_ASSET_PATH)+"/" +_goName+"/"  + _goName + "_" + smr.name.ToLower()+"/";
//
//			Object PartPrefab = EditorHelper.GeneratePrefab(rendererClone, PartAB_MeshAssetName,tempPartFolderPath);
//			partTempPrefabList.Add(PartPrefab);//存起来，打包完成后统一清理掉本地的临时asset资源
//
//			Object.DestroyImmediate(rendererClone);
//
//			toincludeStr.Add(tempPartFolderPath + PartAB_MeshAssetName + ".prefab");
//
//			Log.i ("RoleEditor","GeneratePartAB","打包骨骼名称",BeShowLog);
//
//			//NAFIO INFO 
//			//bone信息可以考虑以.bytes形式保存到ab中，这样就可以避免使用StringHolder这个类
//			//bones---------------------------------------------------------
//
//			List<string> boneNames = new List<string>();
//			foreach (Transform t in smr.bones){
//				boneNames.Add(t.name);
//			}
//
//			string stringholderpath = tempPartFolderPath + PartAB_BonenamesAssetName+".asset";//创建临时asset，用于存储stirng[]类型的骨骼名称
//			//Debug.LogError("===============>stringHolderPath:"+stringholderpath);
//			StringHolder holder = ScriptableObject.CreateInstance<StringHolder> ();//StringHolder类继承自ScriptableObject，可以被以asset形式打入bundle包，stringHolder中有string[] 存储boneNames
//
//			holder.content = boneNames.ToArray();
//
//			AssetDatabase.CreateAsset(holder, stringholderpath);
//			partTempStringholderpathList.Add(stringholderpath);
//
//
//			//toinclude.Add(AssetDatabase.LoadAssetAtPath(stringholderpath, typeof (StringHolder)));
//			toincludeStr.Add(stringholderpath);
//
//
//
//
//			//NDebug.i("打包材质",bShowLog,"RoleEditor.GeneratePartAB");
//
//			//NAFIO INFO 这里涉及到两个问题 
//			//1 没有合并材质，因为如果一个部位有两个材质，合并会出问题（细想下，只有角色穿套装合并材质才有意义，如果混搭，合并了套装的材质也无意义）
//			//2 材质可能有多个，读取的时候排除第一个meshs，和最后一个bones，其他都是材质
//			//materials---------------------------------------------------------
//			//			List<Material> materials = EditorHelpers.CollectAll<Material>(GetMaterailsPath(smr.gameObject));
//			//			
//			//			Debug.Log("================smrName:"+smr.name);
//			//			
//			//			for(int i=0;i<materials.Count;i++){
//			//				
//			//				if (materials[i].name.Contains(smr.name.ToLower())) {
//			//					
//			//					Debug.Log("-----------matName:"+materials[i].name);
//			//					
//			//					toinclude.Add(materials[i]);
//			//				}
//			//				
//			//			}
//
//			// Save the assetbundle.
//			string bundleName = _goName + "_" + smr.name.ToLower();
//			string path = ABOutputPath + bundleName + ABSuffixName;
//			EditorHelper.DeleteFileIfExists(path);	
//			AssetBundleBuild ab = new AssetBundleBuild();
//			ab.assetBundleName = bundleName + ABSuffixName;
//			ab.assetNames = toincludeStr.ToArray();//这里注意下assetName是个数组，这个partbundle中包括多个资源
//			//RoleBundleBuildList.Add(ab);
//
//		}
//
//	}
//
//	/// <summary>
//	/// 从fbx中分离anim
//	/// </summary>
//	/// <param name="animURL">anim.fpx的地址，包含animationClip的fbx文件</param>
//	static void AnimSeparate1(string animURL){
//
//		if (animURL.EndsWith (".meta"))return;
//
//		string reletiveAnimURL = EditorHelper.ChangeToRelativePath(animURL);						//获取相对Asset目录的anim的路径
//
//		Object Asset = AssetDatabase.LoadAssetAtPath<Object>(reletiveAnimURL);						//载入原始anim座位obj存在
//
//		string _assetPath = AssetDatabase.GetAssetPath(Asset);										//anim.fbx完整路径 			eg:Assets/NEditor/RoleEditor/Res/Female/Anim/common/Female@attack.fbx
//		string _assetFolderPath = EditorHelper.GetParentFolderPath(_assetPath);						//anim.fbx所在目录 			eg:Assets/NEditor/RoleEditor/Res/Female/Anim/common
//		string _animGroupName = EditorHelper.GetFileNameFromPath(_assetFolderPath);					//animGroupName 			eg:common
//		string _assetNameWithoutExt = EditorHelper.GetFileNameFromPath(_assetPath,true);			//anim.fbx名(不包含后缀)  		eg:Female@attack
//		string _roleName = _assetNameWithoutExt.Substring (0, _assetNameWithoutExt.IndexOf ('@')); 	//anim角色名					eg:Female
//		string _animName = _assetNameWithoutExt.Substring(_assetNameWithoutExt.IndexOf ('@')+1);	//anim名						eg:attack
//
//		//输出路径，临时目录/角色名/AnimationClip/AnimGroupName/
//		string _outputAnimFolderPath = EditorHelper.EDITOR_ROLE_ASSET_PATH+"/"+_roleName+"/AnimationClip/"+_animGroupName;//输出AnimClip的目录
//		string _outputAnimName = _animName+".anim";
//		string _outputAnimFullPath = _outputAnimFolderPath+"/"+_outputAnimName;
//
//		//		Debug.Log("anim _assetPath:"+_assetPath);
//		//		Debug.Log("anim _assetFolderPath:"+_assetFolderPath);
//		//		Debug.Log("anim _animGroupName:"+_animGroupName);
//		//		Debug.Log("anim _roleName:"+_roleName);
//		//		Debug.Log("anim _animName:"+_animName);
//		//		Debug.Log("anim _assetNameWithoutExt:"+_assetNameWithoutExt);
//		//		Debug.Log("anim _outputAnimPath:"+_outputAnimPath);
//		//		Debug.Log("anim _outputAnimFullPath:"+_outputAnimFullPath);
//
//		//输出目录不存在就创建
//		EditorHelper.CreateFolder(_outputAnimFolderPath);
//
//		//如果输出目录已经存在当前要生成的文件就先删除
//		EditorHelper.DeleteFileIfExists(_outputAnimFullPath);
//
//		var _objs = AssetDatabase.LoadAllAssetsAtPath (reletiveAnimURL);//这里会获取fbx中的每个object
//		var originalClip = System.Array.Find<Object> (_objs, item =>item is AnimationClip);
//		var copyClip = Object.Instantiate (originalClip);
//		AssetDatabase.CreateAsset (copyClip, EditorHelper.ChangeToRelativePath(_outputAnimFullPath));
//		Log.i ("RoleEditor","CleanTempAsset","分离出动作文件:"+_outputAnimFullPath,BeShowLog);
//	}
//
//	/// <summary>
//	/// 清理掉临时asset资源
//	/// </summary>
//	static void CleanTempAsset1()
//	{
//
//		Log.i ("RoleEditor","CleanTempAsset","开始清理TempAsset",BeShowLog);
//
//		int i = 0;
//
//		//清临时bone Asset
//		for (i = 0; i < boneTempPrefabList.Count; i++) {
//			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(boneTempPrefabList[i]));
//		}
//		boneTempPrefabList.Clear();
//
//		//清临时part Asset
//		for (i = 0; i < partTempPrefabList.Count; i++) {
//			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(partTempPrefabList[i]));
//		}
//		partTempPrefabList.Clear ();
//
//		for (i = 0; i < partTempStringholderpathList.Count; i++) {
//			AssetDatabase.DeleteAsset (partTempStringholderpathList [i]);
//		}
//		partTempStringholderpathList.Clear ();
//
//		//清理临时文件夹
//		EditorHelper.DeleteFolder(EditorHelper.EDITOR_ROLE_ASSET_PATH);
//		EditorHelper.CreateFolder (EditorHelper.EDITOR_ROLE_ASSET_PATH);
//	}
//
//	#endregion
//
//
//
//}
//
//
