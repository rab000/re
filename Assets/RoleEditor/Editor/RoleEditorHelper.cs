using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleEditorHelper{



	/// <summary>
	/// 角色公用骨骼AB 中唯一asset，mainasset的名称
	/// 这个可以直接WWW.assetBundle.mainAsset来获取
	/// 也可以和PartAB一样使用asset name进行资源获取(如果是这种就要在读取asset时用BoneAB_AssetName这个名称读取)
	/// </summary>
	/// <value>The name of the bone A b_ asset.</value>
	public static string BoneAB_AssetName{

		get{return "bone";}
	}

	/// <summary>
	/// 角色部位AB中mesh asset的名称
	/// 但加载角色部位的mesh时，需要WWW.assetBundle.LoadAsync("meshName", typeof(GameObject));
	/// </summary>
	/// <value>The name of the mesh asset.</value>
	public static string PartAB_MeshAssetName{

		get{return "mesh";}

	}

	/// <summary>
	/// 角色部位AB中boneNames asset的名称
	/// 但加载角色部位的bonenames时，需要WWW.assetBundle.LoadAsync("bonenames", typeof(StringHolder));
	/// </summary>
	/// <value>The name of the mesh asset.</value>
	public static string PartAB_BonenamesAssetName{

		get{return "bonenames";}
	}

	/// <summary>
	/// 输出文件夹位置
	/// 路径不放在Asset文件夹内部，因为会生成多余的meta文件，不方便直接提取
	/// </summary>
	/// <value>The output path.</value>
	public static string ABOutputPath{

		get{ return EditorHelper.OUTPUT_ROOT_PATH+"/res/re/";}

	}

	/// <summary>
	/// 原始资源生成的待打包asset资源（根）路径
	/// </summary>
	/// <value>The EDITO r TEM p ASSE t PAT.</value>
	public static string EDITOR_TEMP_ASSET_PATH{
		get{
			return EditorHelper.EDITOR_ASSETS_PATH+"/Asset4Build";
		}
	} 

	//原始换装资源路径
	public static string EDITOR_ROLE_ORIGNAL_RES_PATH{
		get{
			return EditorHelper.EDITOR_ASSETS_PATH+"/RoleEditor/Res";
		}
	}


}
