//using System.Collections.Generic;
//using System.IO;
//using UnityEditor;
//using UnityEngine;
//using System;
//using Object = UnityEngine.Object;
///// <summary>
///// 升级到2017之前的roleeditor，因为文件夹结构变更，已经无法使用，保留这个文件，方便后面查询，选择文件打包的方法
//
///// 角色打包编辑器(包含bone，parts，animation)
///// 
///// 创建角色bundle
///// 从选中目录或者文件中查找要处理的角色prefab
///// 创建基本骨骼bundle
///// 创建身体部位bundle
///// 
///// 关于SkinnedMeshRenderer的说明
///// 主要包含1 sharedMesh 2 bones 3 materials
///// 
///// 注意：角色打包没用到依赖打包，都是单独打包
///// 原因是
///// 依赖打包的条件是:bundleA bundleB中如果有共同依赖bundleC,那么bundleC必须打为依赖，并优先与AB加载，否则如果没有依赖包那么bundleA和bundleB就会加载到重复资源
///// 现在看角色换装编辑器的导出有2部分  1是go(包含skinnedMeshRenderer 里面有mesh，bonenames，但已经把materail=null了，默认编辑器模型上也不要绑材质，因为同一部位材质可能为多个)    2是材质
///// 不存在依赖打包重复加载资源的问题
///// 
///// todo 加入角色动作打包
///// </summary>
//class RoleEditor_lecacy{
//	
//	static bool bShowLog = true;
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
//	#endregion
//	
//	
//	#region 换装模型打包
//	//[MenuItem("NEditor/RoleEditor/CreateRoleAB")]
//	public static void CreateRoleAB(){
//		
//		//NDebug.i("step1---开始创建角色AB",bShowLog,"RoleEditor.CreateRoleAB");
//		
//		//如果输出文件夹不存在，创建输出文件夹
//		if (!Directory.Exists(ABOutputPath)){
//			
//			//NDebug.i("输出路径"+ABOutputPath+"不存在，创建输出路径",bShowLog,"RoleEditor.CreateRoleAB");
//			
//			Directory.CreateDirectory(ABOutputPath);
//		}
//		
//		//NDebug.i("step2---筛选目标文件",bShowLog,"RoleEditor.CreateRoleAB");
//		
//		//得到并筛选要处理的object
//		List<Object> objList = new List<Object>();
//		
//		foreach (Object obj in Selection.GetFiltered(typeof (Object), SelectionMode.DeepAssets)){
//			
//			if (!(obj is GameObject)) continue;			//忽略非GameObject
//			
//			if (obj.name.Contains("@")) continue;		//忽略与角色prefab在一起的角色动作prefab
//			
//			if (!AssetDatabase.GetAssetPath(obj).Contains("/RoleEditor/Res/")) continue;//忽略角色编辑器资源文件夹以外的资源，防止意外打错资源
//			
//			objList.Add(obj);
//			
//			//NDebug.i("筛选出目标文件:"+obj.name,bShowLog,"RoleEditor.CreateRoleAB");
//		}
//		
//		//NDebug.i("step3---生成骨骼与身体部位AB",bShowLog,"RoleEditor.CreateRoleAB");
//		
//		//生成骨骼ab和部位ab
//		for(int i = 0; i < objList.Count; i++){
//			
//			GameObject _go = (GameObject)objList[i];
//			
//			//NDebug.i("开始为角色:"+_go.name+"打包",bShowLog,"RoleEditor.CreateRoleAB");
//			
//			GenerateBoneAB(_go);
//			
//			GeneratePartAB(_go);
//
//			GeneratePartMatreialAB(_go);
//		}
//
//		//NDebug.i("step4---生成角色AB结束",bShowLog,"RoleEditor.CreateRoleAB");
//		
//		AssetDatabase.Refresh();//因为输出文件夹位于Asset中，为了方便显示，刷新下
//	}
//	
//	/// <summary>
//	/// 生成基本骨骼ab
//	/// </summary>
//	/// <param name="go">Go.</param>
//	static void GenerateBoneAB(GameObject go){
//		
//		string _goName = go.name.ToLower();
//		
//		//NDebug.i("开始为角色:"+go.name+"打骨骼包",bShowLog,"RoleEditor.GenerateBoneAB");
//		
//		GameObject goClone = Object.Instantiate(go) as GameObject;
//		
//		foreach (Animation anim in goClone.GetComponentsInChildren<Animation>()){
//			
//			anim.animateOnlyIfVisible = false;//即使物体在屏幕外，也播放动画
//			
//		}
//		
//		//删除包含SkinnedMeshRenderer组件的物体，实际就是角色部件，只留下骨骼相关
//		foreach (SkinnedMeshRenderer smr in goClone.GetComponentsInChildren<SkinnedMeshRenderer>()){
//
//			Object.DestroyImmediate(smr.gameObject);
//			
//		}
//		
//		//在Asset文件夹中创建名为basebone临时asset
//		Object characterBasePrefab = EditorHelper.GeneratePrefab(goClone, BoneAB_AssetName);
//		
//		Object.DestroyImmediate(goClone);
//		
//		string path = ABOutputPath + _goName +"_"+BoneAB_AssetName+ABSuffixName; //female_basebone.assetbundle     角色名+_basebone+ABSuffixName
//		
//		if(File.Exists(path)){
//			
//			//NDebug.i("文件"+path+"存在，删除之",bShowLog,"RoleEditor.GenerateBoneAB");
//			
//			File.Delete(path);
//		}
//		
//		//BuildPipeline.BuildAssetBundle(characterBasePrefab, null, path, BuildAssetBundleOptions.CollectDependencies);
//		
//		AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(characterBasePrefab));//删除临时用于打包的asset
//		
//	}
//
//	/// <summary>
//	/// 为(选中角色)所有材质单独打包
//	/// </summary>
//	/// <param name="go">Go.</param>
//	static void GeneratePartMatreialAB(GameObject go){
//
//		//NDebug.i("打包材质",bShowLog,"RoleEditor.GeneratePartAB");
//		
//		List<Material> materials = EditorHelper.CollectAll<Material>(GetMaterailsPath(go));//根据角色go找到角色go所在路径，进而找到目标角色材质文件夹中所有材质
//		
//		for(int i=0;i<materials.Count;i++){
//			
//			//Debug.Log("-----------matName:"+materials[i].name);
//			
//			string path = ABOutputPath + materials[i].name + ABSuffixName;//材质名一般格式为  角色名_部位名_模型名_材质名.mat  如果不包含这些就比较难区分是属于那个角色哪个部位
//			
//			if(File.Exists(path)){
//				
//				//NDebug.i("文件"+path+"存在，删除之",bShowLog,"RoleEditor.GeneratePartAB");
//				
//				File.Delete(path);
//			}
//			
//			//BuildPipeline.BuildAssetBundle(materials[i], null, path, BuildAssetBundleOptions.CollectDependencies);
//			
//		}
//	}
//
//	/// <summary>
//	/// 生成身体部位ab
//	/// 这种方法分离除了mesh，bonenames，materail分别打包
//	/// 然后加载时新连理SkinnedMeshRenderer，把上面3个赋值给SkinnedMeshRenderer
//	/// 这种操作没问题，但是操作不方便需要用把脚本StringHolder打包成ab用以保存bonenames
//	/// 暂时保留，以后可能会用到
//	/// </summary>
//	/// <param name="go">Go.</param>
//	static void GeneratePartAB(GameObject go){
//		
//		string _goName = go.name.ToLower();
//		
//		//NDebug.i("开始为角色:"+go.name+"打部位包",bShowLog,"RoleEditor.GeneratePartAB");
//		
//		//NDebug.i("开始遍历部位(SkinnedMeshRenderer)",bShowLog,"RoleEditor.GeneratePartAB");
//		
//		foreach (SkinnedMeshRenderer smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true)){
//			
//			//NDebug.i("开始为部位："+smr.name+"打包",bShowLog,"RoleEditor.GeneratePartAB");
//			
//			//用于集中meshs,materials,boneName的ScriptableObject,一并打包到assetBundle
//			List<Object> toinclude = new List<Object>();
//			
//			//NDebug.i("打包mesh",bShowLog,"RoleEditor.GeneratePartAB");
//			
//			//meshs---------------------------------------------------------
//			GameObject rendererClone = (GameObject)EditorUtility.InstantiatePrefab(smr.gameObject); //copy 一个角色prefab实例
//			
//			GameObject rendererParent = rendererClone.transform.parent.gameObject;//清理掉角色部位
//			
//			rendererClone.transform.parent = null;			//清理掉角色部位
//			
//			Object.DestroyImmediate(rendererParent);		//清理掉角色部位
//			
//			Object rendererPrefab = EditorHelper.GeneratePrefab(rendererClone, PartAB_MeshAssetName);
//			
//			Object.DestroyImmediate(rendererClone);
//			
//			toinclude.Add(rendererPrefab);
//			
//			//NDebug.i("打包骨骼名称",bShowLog,"RoleEditor.GeneratePartAB");
//			
//			//NAFIO INFO 
//			//bone信息可以考虑以.bytes形式保存到ab中，这样就可以避免使用StringHolder这个类
//			//bones---------------------------------------------------------
//			List<string> boneNames = new List<string>();
//			
//			foreach (Transform t in smr.bones){
//				
//				boneNames.Add(t.name);
//				
//			}
//			
//			string stringholderpath = "Assets/"+PartAB_BonenamesAssetName+".asset";//创建临时asset，用于存储stirng[]类型的骨骼名称
//
//			StringHolder holder = ScriptableObject.CreateInstance<StringHolder> ();//StringHolder类继承自ScriptableObject，可以被以asset形式打入bundle包，stringHolder中有string[] 存储boneNames
//			
//			holder.content = boneNames.ToArray();
//			
//			AssetDatabase.CreateAsset(holder, stringholderpath);
//			
//			toinclude.Add(AssetDatabase.LoadAssetAtPath(stringholderpath, typeof (StringHolder)));
//			
//			//NDebug.i("打包材质",bShowLog,"RoleEditor.GeneratePartAB");
//			
//			//NAFIO INFO 这里涉及到两个问题 
//			//1 没有合并材质，因为如果一个部位有两个材质，合并会出问题（细想下，只有角色穿套装合并材质才有意义，如果混搭，合并了套装的材质也无意义）
//			//2 材质可能有多个，读取的时候排除第一个meshs，和最后一个bones，其他都是材质
//			//materials---------------------------------------------------------
////			List<Material> materials = EditorHelpers.CollectAll<Material>(GetMaterailsPath(smr.gameObject));
////			
////			Debug.Log("================smrName:"+smr.name);
////			
////			for(int i=0;i<materials.Count;i++){
////				
////				if (materials[i].name.Contains(smr.name.ToLower())) {
////					
////					Debug.Log("-----------matName:"+materials[i].name);
////					
////					toinclude.Add(materials[i]);
////				}
////				
////			}
//			
//			// Save the assetbundle.
//			string bundleName = _goName + "_" + smr.name.ToLower();
//			
//			string path = ABOutputPath + bundleName + ABSuffixName;
//			
//			if(File.Exists(path)){
//				
//				//NDebug.i("文件"+path+"存在，删除之",bShowLog,"RoleEditor.GeneratePartAB");
//				
//				File.Delete(path);
//			}
//			
//			//BuildPipeline.BuildAssetBundle(null, toinclude.ToArray(), path, BuildAssetBundleOptions.CollectDependencies);
//			
//			//Debug.Log("Saved " + bundleName + " with " + (toinclude.Count - 2) + " materials");
//			
//			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(rendererPrefab));//清理临时mesh.asset
//			
//			AssetDatabase.DeleteAsset(stringholderpath);//已经打包结束，删除临时创建的bonenames.asset
//			
//		}
//		
//	}
//	
//	/// <summary>
//	/// 生成身体部位ab
//	/// 分两部分打包
//	/// 1 部位go，包含SkinnedMeshRenderer(里面包括mesh，bonenames，但不包括matrial，材质需要单独打)
//	/// 2 具体部位的matrial(一个部位可以有多个模型，每个模型可以有多个贴图，具体就是多个material，所以mat要单独打包)
//	/// 
//	/// 加载时不需要新建SkinnedMeshRenderer，直接把指定材质给加载出来的指定的SkinnedMeshRenderer，并且刷新骨骼节点就可以了
//	/// 
//	/// 实践证明使用单独打包的方式有问题
//	/// 单独打包时需要打包包含SkinnedMeshRenderer的Go，SkinnedMeshRenderer内部包含mesh和骨骼信息
//	/// 关键在于骨骼信息如何保存，如果不把骨骼Transform[] 一起打进包里的话SkinnedMeshRenderer的骨骼信息就丢失了
//	/// 如果把骨骼一起打进去，那么就是每个部位都包含整体骨骼信息，比较浪费，暂时废弃这种单独打包的方法(PcDemo的方法)
//	/// </summary>
//	/// <param name="go">Go.</param>
//	static void GeneratePartAB1(GameObject go){
//		
//		string _goName = go.name.ToLower();//角色名取小写
//		
//		//NDebug.i("开始为角色:"+go.name+"打部位包",bShowLog,"RoleEditor.GeneratePartAB");
//		
//		//NDebug.i("开始遍历部位(SkinnedMeshRenderer)",bShowLog,"RoleEditor.GeneratePartAB");
//		
//		foreach (SkinnedMeshRenderer smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true)){//筛选出角色go中的所有SkinnedMeshRenderer，其实就是身体部位，只有部位包含SkinnedMeshRenderer
//			
//			//NDebug.i("开始为部位："+smr.name+"打包",bShowLog,"RoleEditor.GeneratePartAB");
//			
//			//NDebug.i("打包除materail以外的部分",bShowLog,"RoleEditor.GeneratePartAB");
//			
//			GameObject rendererClone = (GameObject)EditorUtility.InstantiatePrefab(smr.gameObject); //copy 一个角色部位实例
//			
//			
//			//Transform[] tempBones = smr.bones;//临时记录下bones
//			
//			GameObject rendererParent = rendererClone.transform.parent.gameObject;//清理掉角色部位copy实例父节点
//			
//			rendererClone.transform.parent = null;			//清理掉角色部位copy实例与父节点连接
//			
//			
//			//-------------------START 记录下部位中包含的骨骼名称，并以相同名称的空Go存入SkinnedMeshRenderer.bones，这么做目的是避免多资源打包
//			//			GameObject g =null;
//			//			List<Transform> temp = new List<Transform>();
//			//			for(int i=0;i<tempBones.Length;i++){
//			//				g = new GameObject();
//			//				g.name = tempBones[i].name;
//			//				temp.Add(g.transform);
//			//			}
//			//			rendererClone.GetComponent<SkinnedMeshRenderer>().bones = temp.ToArray();
//			//-------------------END 记录下部位中包含的骨骼名称，并以相同名称的空Go存入SkinnedMeshRenderer.bones，这么做目的是避免多资源打包
//			
//			Object.DestroyImmediate(rendererParent);		//清理掉角色部位父节点(到这就从临时实例化的角色中分离出了一个完整的部位)
//			
//			Object rendererPrefab = EditorHelper.GeneratePrefab(rendererClone, PartAB_MeshAssetName);//为独立出来的角色部位在Asset路径上创建一个.prefab文件，用于后面打包ab
//			
//			
//			Object.DestroyImmediate(rendererClone);//清理掉角色部位copy实例
//			
//			string bundleName = _goName + "_" + smr.name.ToLower();//角色部位bundle名为角色名_部位名
//			
//			string path = ABOutputPath + bundleName + ABSuffixName;
//			
//			if(File.Exists(path)){
//				
//				//NDebug.i("文件"+path+"存在，删除之",bShowLog,"RoleEditor.GeneratePartAB");
//				
//				File.Delete(path);
//			}
//			
//			//BuildPipeline.BuildAssetBundle(rendererPrefab, null, path, BuildAssetBundleOptions.CollectDependencies);//单独打包
//			
//			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(rendererPrefab));//清理临时在Asset根目录创建的用于打包的.prefab文件
//			
//			
//			//			for(int j=0;j<temp.Count;j++){
//			//				GameObject.DestroyImmediate(temp[j].gameObject);
//			//			}
//
//		}
//	}
//
//	#endregion
//
//	#region 动作打包
//	/// <summary>
//	/// 打包动作(组)
//	/// 动作组bundle存放位置为date/res/role/
//	/// 动作组名称为 roleName_anim_animGroupName(比如 famele_anim_common),groupName就是动作所在文件夹名称
//	/// 动作组bundle中具体asset的名称就是.anim原始文件的名称，也就是动作名(walk,idle等)
//	/// 动作组简单调用流程
//	/// 首先比如role新增了一个weapon，那么就要加入这个角色这个weapon的动作组，首先调用程序需要加载
//	/// 动作组，那么就要求weapon gameDate中包含相关动作组table信息
//	/// 其次，进入某个场景，这个场景需要包含动作组信息
//	/// 
//	/// 仿war3的技能系统要求动作分类方式，任何动作都该有默认的特效挂载点，可以动态绑定特效和动作
//	/// 
//	/// 说明：目前的方式是一次性打所有动作组的包，以后要提供单独打一个动作组的功能
//	/// </summary>
//	//[MenuItem("NEditor/RoleEditor/CreateAnimAB")]
//	static void GenerateAnimAB(){
//
//		//角色编辑器资源目录地址
//		string roleResPath = EditorHelper.EDITOR_ROLE_RES_PATH;
//
//		//Debug.Log("roleResPath:"+roleResPath);
//		//角色编辑器资源目录地址下的所有子目录名(角色名，如Famele)
//		string[] roleNameFolderPaths = EditorHelper.GetSubFolderPaths(roleResPath);
//
//		//Debug.Log("LEN:"+roleNameFolderPaths.Length);
//
//		List<Object> tempList = new List<Object>();
//
//		for(int i=0;i<roleNameFolderPaths.Length;i++){
//			//Debug.Log("name:"+roleNameFolderPaths[i]);
//			//一个角色名目录下的子目录(资源类型目录，AnimationClip，Materials，texture)
//			string[] roleResTypeFolderPaths = EditorHelper.GetSubFolderPaths(roleNameFolderPaths[i]);
//			for(int j=0;j<roleResTypeFolderPaths.Length;j++){
//				//Debug.Log("btype:"+roleResTypeFolderPaths[j]+" 目录名:"+Path.GetFileName(roleResTypeFolderPaths[j]));
//				if(!EditorHelper.GetFileNameFromPath(roleResTypeFolderPaths[j]).Equals("AnimationClip"))continue;
//
//				//Debug.Log("type:"+roleResTypeFolderPaths[j]);
//
//				//角色动作组目录名(如common等)
//				string[] roleAnimGroupFolderPaths = EditorHelper.GetSubFolderPaths(roleResTypeFolderPaths[j]);
//				for(int k=0;k<roleAnimGroupFolderPaths.Length;k++){
//					//Debug.Log("animGroup:"+roleAnimGroupFolderPaths[k]);
//					//得到一个动作组目录中的所有文件
//					string[] animNamePaths =EditorHelper.GetSubFilesPaths(roleAnimGroupFolderPaths[k]);
//
//					tempList.Clear();
//
//					//已经得到动画文件和相应meta，准备打包
//					for(int m=0;m<animNamePaths.Length;m++){
//						//Debug.Log("name:"+animNamePaths[m]);
//
//						if(animNamePaths[m].Contains(".meta"))continue;
//
//						//string ss = "D:/SVN/NEditor/trunk/Assets/NEDITOR/RoleEditor/Res/Female/AnimationClip/common/walk.anim";
//						//string ss = "Assets/NEDITOR/RoleEditor/Res/Female/AnimationClip/common/walk.anim";
//
//						string _path = EditorHelper.ChangeToRelativePath(animNamePaths[m]);
//						//Debug.Log("_path:"+_path);
//						//Object o = AssetDatabase.LoadAssetAtPath<Object>(animNamePaths[m]));
//						Object o = AssetDatabase.LoadAssetAtPath<Object>(_path);
//						if(null == o)Debug.Log("o=null");
//						//TODO 注意这里类型限制填的是Object，以后有可能需要调整
//						tempList.Add(o);
//
//					}
//
//					//确定文件名famele_anim_animGroupName，资源生成位置
//					string bundleName = EditorHelper.GetFileNameFromPath(roleNameFolderPaths[i]).ToLower()+"_anim_" +Path.GetFileName(roleAnimGroupFolderPaths[k]).ToLower();
//					//确定生成路径
//					string path = ABOutputPath + bundleName + ABSuffixName;
//
//					EditorHelper.DeleteFileIfExists(path);
//
//					//Debug.Log("bName:"+bundleName+"  path:"+path +" len:"+tempList.Count);
//					//开始打包
//					//BuildPipeline.BuildAssetBundle(null, tempList.ToArray(), path, BuildAssetBundleOptions.CollectDependencies);
//
//				}
//			}
//		}
//
//		Debug.Log("anim 打包完毕");
//
//	}
//
//	#endregion
//
//	#region 分离anim
//	/// <summary>
//	/// 使用方法，选中RoleName@ActionName.FBX 中的anim文件，然后使用这个导出
//	/// 会把相应.anim文件自动导出到" /NEditor/RoleEditor/Res/RoleName/AnimatrionClip/"下
//	/// 
//	/// 注意事项，导出前RoleName@ActionName.FBX文件需要被设置为human或者Generic模式，选择Copy From Other Avata 并且要指定Avata
//	/// 否则导出的.anim文件可能缺少内容，比如没选择新动画而是选择了legecy模式，那么导出的.anim会看不到新版动画的烘焙选项
//	/// </summary>
//	//[MenuItem("NEditor/RoleEditor/AnimSeparate")]
//	static void AnimSeparate(){
//
//		Object[] SelectionAsset = Selection.GetFiltered(typeof(Object),SelectionMode.Unfiltered);
//
//		foreach(Object Asset in SelectionAsset){
//
//			string _assetPath = AssetDatabase.GetAssetPath(Asset);//Asset完整路径
//			string _dicPath = Path.GetDirectoryName(_assetPath);//Asset目录名
//			string _assetNameWithoutExt = Path.GetFileNameWithoutExtension(_assetPath);//Asset名(不包含后缀)
//			string _outputAnimPath = _dicPath+"/AnimationClip/";//输出AnimClip的目录
//
//			string _outputAnimName = Asset.name+".anim";
//			string _outputAnimFullPath = _outputAnimPath+_outputAnimName;
//
//			//输出目录不存在就创建
//			if(!Directory.Exists(_dicPath))Directory.CreateDirectory(_dicPath);
//
//			//如果输出目录已经存在当前要生成的文件就先删除
//			if(File.Exists(_outputAnimFullPath))File.Delete(_outputAnimFullPath);
//
//			AnimationClip newClip = new AnimationClip();
//
//			EditorUtility.CopySerialized(Asset,newClip);//将Asset clone 给newClip
//
//			AssetDatabase.CreateAsset(newClip,_outputAnimFullPath);
//
//			//NDebug.i("分离出动作文件:"+_outputAnimName,bShowLog,"RoleEditor.AnimSeparate");
//		}
//		AssetDatabase.Refresh();
//	}
//	#endregion
//
//
//	#region 辅助函数
//	
//	/// <summary>
//	/// 查找导出角色go的材质文件夹位置
//	/// </summary>
//	static string GetMaterailsPath(GameObject character){
//		
//		string root = AssetDatabase.GetAssetPath(character);
//		
//		string path = root.Substring(0, root.LastIndexOf('/') + 1)+"Materials";
//		
//		//NDebug.i("材质路径为:"+path,bShowLog,"RoleEditor.GetMaterailsPath");
//		
//		return path;
//		
//	}
//	
//	#endregion
//
//
//	//unity5的打包变换，貌似从u54就开始变换了
//	void TU5(){
//		//https://forum.unity3d.com/threads/dont-make-buildpipeline-buildassetbundle-obsolete.313995/
//		AssetBundleBuild[] build = new AssetBundleBuild[1];
//		build[0] = new AssetBundleBuild();
//		build[0].assetBundleName = "name";
//		build[0].assetNames = new string[1] { "Assets/object.prefab" };
//		BuildPipeline.BuildAssetBundles("Assets/", build, BuildAssetBundleOptions.CollectDependencies,BuildTarget.StandaloneWindows64);
//	}
//
//}
//
//
