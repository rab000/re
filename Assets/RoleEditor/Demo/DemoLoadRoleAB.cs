using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
/// <summary>
/// 导入遗留问题
/// 
/// 正常5个资源asset，3个材质，一个stringbuilder，一个mesh
/// 
/// 
/// 
/// 
/// </summary>
public class DemoLoadRoleAB : MonoBehaviour {
	
	//角色部位样式		female_hair-1，female_hair-2等
	//角色部位具体材质	female_hair-1_brown等
	
	bool bShowLog = true;
	
	string PC_PATH_START_STR = "file:///";//PC上使用WWW要加这个路径（使用File.Exists时如果加了file:///这个前缀那么是检测不到文件存在的，但用www在PC要有这个，没有返回的www.assetbundle就为null）
	
	//need equipInfoTable
	
	public string GetBonePath(string roleName){
		
		return DataFolderPath+roleName+"_basebone.n";//这里_basebone这个名称是画蛇添足
	}
	
	public string GetPartPath(string roleName,int partID,string equipID){
		
		string resName = roleName+"_"+GetPartNameByPartID(partID)+"-"+GetModleNameByEquipID(equipID)+".n";
		
		return DataFolderPath + resName;
	}

	public string GetMatPath(string roleName){

		return DataFolderPath + roleName+"_top-1_blue.n";
	}

	string DataFolderPath{
		
		get{
			#if UNITY_ANDROID
			return Path.GetDirectoryName(Application.dataPath)+"/data/res/role/";
			#else
			return PC_PATH_START_STR+Path.GetDirectoryName(Application.dataPath)+"/data/res/role/";
			#endif
		}
	}
	
	string GetPartNameByPartID(int partID){
		
		switch(partID){
		case 0:
			return "top";
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
			Debug.LogError("nothing");
			break;
		}
		
		
		return "";
	}
	
	string GetModleNameByEquipID(string equipID){
		
		return "1";
	}
	
	string GetMaterailByEquipID(string equipID){
		
		return "female_top-1_blue";
	}
	

	
	void OnGUI () {
		
		if(GUILayout.Button("导入角色骨骼",GUILayout.Width(100))){
			
			StartCoroutine(StartloadBone("female"));
			
		}
		
		if(GUILayout.Button("导入角色部位",GUILayout.Width(100))){
			
			string partPath = GetPartPath("female",0,"1");
			
			StartCoroutine(StartloadPart("female",partPath,GetMaterailByEquipID("1")));
			
		}
		
		if(GUILayout.Button("角色换装",GUILayout.Width(100))){
			
		}
		
	}
	
	
	/// <summary>
	/// 加载骨骼
	/// </summary>
	/// <returns>The bone.</returns>
	/// <param name="path">Path.</param>
	IEnumerator StartloadBone(string roleName){
		
		string roleBonePath = GetBonePath(roleName);
		
		Debug.Log("开始加载骨骼，地址为:"+roleBonePath+"  DemoLoadRoleAB.StartloadBone");
		
		WWW www = new WWW(roleBonePath);
		
		yield return www;
		
		if(www.error!=null)Debug.Log("error:"+www.error+"DemoLoadRoleAB.StartloadBone");
		
		Object obj = www.assetBundle.LoadAsset("basebone");
		
		GameObject go = GameObject.Instantiate(obj) as GameObject;
		
		go.name = roleName;
		
	}
	
	/// <summary>
	/// 加载角色部位模型
	/// </summary>
	/// <returns>The part.</returns>
	/// <param name="path">Path.</param>
	IEnumerator StartloadPart(string roleName,string path,string materailName){
		
		Debug.Log("开始加载部位，地址为:"+path+"DemoLoadRoleAB.StartloadPart");
		
		WWW www = new WWW(path);
		
		yield return www;
		
		if(www.error!=null)Debug.Log("error:"+www.error+"DemoLoadRoleAB.StartloadBone");
		
		
				Object[] objs = www.assetBundle.LoadAllAssets();
		
				for(int i = 0; i < objs.Length; i++){
		
					Debug.Log("------>"+i+"  name:"+objs[i].name+"  type:"+objs[i].GetType());
				}
		
		
		//这里特别注意mesh,bonenames都是跟editor协议好的，但materail name是跟具体equipID挂钩的
		AssetBundleRequest mesh_assetBundleRequest = www.assetBundle.LoadAssetAsync("mesh",typeof(GameObject));
		
		AssetBundleRequest bonenames_assetBundleRequest = www.assetBundle.LoadAssetAsync("bonenames",typeof(StringHolder));
		

		
		
		yield return mesh_assetBundleRequest;
		
		Debug.Log("mesh return!!");
		
		yield return bonenames_assetBundleRequest;
		
		Debug.Log("bonenames return!!");
		

		//这里不用mesh合并，理由是
		//1 如果合并了mesh，那么每次换装都要重新合并
		//2 如果合并了mesh, 那么一个part的skinRender只能有一个materail，否则materail可能会出错
		
		//这里要考虑下是每次合并mesh好，还是不合并，合并的好处就是合并完后效率更高
		
		//List<CombineInstance> combineInstances = new List<CombineInstance>();
		//List<Material> materials = new List<Material>();
		
		List<Transform> bones = new List<Transform>();//用于存储当前part有关的骨骼transform
		
		GameObject roleBaseBoneGo = GameObject.Find(roleName);//基本骨骼Go
		
		GameObject partGo = new GameObject();//为part新增go
		
		partGo.name = "part";
		
		Transform[] transforms = roleBaseBoneGo.GetComponentsInChildren<Transform>();
		
		SkinnedMeshRenderer role_smr = partGo.AddComponent<SkinnedMeshRenderer>();//为形状part go添加SkinnedMeshRenderer
		
		GameObject meshGO = GameObject.Instantiate(mesh_assetBundleRequest.asset) as GameObject;//实例化meshGO

		//TODO 这句打包有问题，
		role_smr.sharedMesh = meshGO.GetComponent<SkinnedMeshRenderer>().sharedMesh;//mesh   把从ab中读取的mesh赋值给新建立的partGo.sharedMesh


		
		
		//---------------------------------------------------------------------重新绑定骨骼
		StringHolder bonenames = (StringHolder)bonenames_assetBundleRequest.asset;
		
		foreach (string bone in bonenames.content){    
			
			foreach (Transform transform in transforms)
			{
				if (transform.name != bone) continue;
				
				bones.Add(transform);
				
				break;
			}
		}
		
		role_smr.bones = bones.ToArray();
		//---------------------------------------------------------------------加载材质
		

		www = new WWW(GetMatPath(roleName));

		yield return www;

		AssetBundleRequest materail_assetBundleRequest = www.assetBundle.LoadAssetAsync(materailName,typeof(Material));//这里material是单独打包可以用mainAsset来取

		yield return materail_assetBundleRequest;

		role_smr.sharedMaterial = (Material)materail_assetBundleRequest.asset;
		
		role_smr.material = (Material)materail_assetBundleRequest.asset;//material   把从ab中读取的material赋值给新建立的partGo.material

		//---------------------------------------------------------------------

		partGo.transform.parent = roleBaseBoneGo.transform;//把partGo绑定到基本骨骼Go上
		
		Object.Destroy(meshGO);
		
		
		
	}

	/// <summary>
	/// 这个用来测试加载多资源时如果加载出全部assets，就释放assetbundle，那么
	/// 加载出的mat和texture之间的依赖关系是否还在，事实证明依赖关系还在
	/// </summary>
	/// <returns>The part1.</returns>
	/// <param name="roleName">Role name.</param>
	/// <param name="path">Path.</param>
	/// <param name="materailName">Materail name.</param>
	IEnumerator StartloadPart1(string roleName,string path,string materailName){
		
		Debug.Log("开始加载部位，地址为:"+path+" DemoLoadRoleAB.StartloadPart");
		
		WWW www = new WWW(path);
		
		yield return www;
		
		if(www.error!=null)Debug.Log("error:"+www.error+"DemoLoadRoleAB.StartloadBone");
		
		
		Object[] objs = www.assetBundle.LoadAllAssets();


		www.assetBundle.Unload(false);
		www.Dispose();
		www = null;



		int i=0;

		GameObject meshGo = null;

		StringHolder sh = null;

		Material mat = null;

		for(i = 0; i < objs.Length; i++){
		
			Debug.Log("------>"+i+"  name:"+objs[i].name+"  type:"+objs[i].GetType());

			Object obj = objs[i];

			if(obj.name.Equals("mesh"))meshGo = (GameObject)objs[i];
			if(obj.name.Equals("bonenames"))sh = (StringHolder)objs[i];

			if(obj.name.Equals(materailName)){


				Debug.Log("找到mesh--index:"+i+" meshName:"+objs[i].name+" type:"+obj.GetType());


				if(obj.GetType().Equals(typeof(Material))){//注意这里写法，写了多次才对

					Debug.Log("succ");

					mat = objs[i] as Material;
				}else{
					Debug.Log("faile:  typeObje:"+obj.GetType()+"    matType:"+typeof(Material));
				}


			}
		}

		
		List<Transform> bones = new List<Transform>();//用于存储当前part有关的骨骼transform
		
		GameObject roleBaseBoneGo = GameObject.Find(roleName);//基本骨骼Go
		
		GameObject partGo = new GameObject();//为part新增go
		
		partGo.name = "part";
		
		Transform[] transforms = roleBaseBoneGo.GetComponentsInChildren<Transform>();
		
		SkinnedMeshRenderer role_smr = partGo.AddComponent<SkinnedMeshRenderer>();//为形状part go添加SkinnedMeshRenderer
		
		GameObject meshGO = GameObject.Instantiate(meshGo) as GameObject;//实例化meshGO
		
		role_smr.sharedMesh = meshGO.GetComponent<SkinnedMeshRenderer>().sharedMesh;//mesh   把从ab中读取的mesh赋值给新建立的partGo.sharedMesh,不知道为什么非要这句
		
		role_smr.material = mat;//material   把从ab中读取的material赋值给新建立的partGo.material
		
		
		//---------------------------------------------------------------------重新绑定骨骼
		StringHolder bonenames = sh;
		
		foreach (string bone in bonenames.content){    
			
			foreach (Transform transform in transforms)
			{
				if (transform.name != bone) continue;
				
				bones.Add(transform);
				
				break;
			}
		}
		
		role_smr.bones = bones.ToArray();
		//---------------------------------------------------------------------
		
		
		
		partGo.transform.parent = roleBaseBoneGo.transform;//把partGo绑定到基本骨骼Go上
		
		Object.Destroy(meshGO);
		
		
		
	}

}
