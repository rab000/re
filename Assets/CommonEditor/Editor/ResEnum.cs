using UnityEngine;
using System.Text;
/// <summary>
/// 资源管理常量
/// </summary>
public class ResEnum{

	#region 常用委托
	public delegate void DelegateLoadassetCallback(int resID,UnityEngine.Object obj,UnityEngine.Object[] objs);//单独打包时4参数传null，多资源时3参数传null
	public delegate void DeleCallback();
	#endregion

	#region 资源管理器版本
	public static int ResVer = 1;//资源版本
	#endregion

	#region 资源路径

	//资源(配表)路径 =ApkRootURL(CacheRootURL,RomateRootURL)+RES_URL(TABLE_URL)+SCENE_URL(ROLE_URL,UI_URL)
	public static string URL0_ApkRootURL;
	public static string URL0_CacheRootURL_FOR_WWWREAD;//本地缓存根目录
	public static string URL0_CacheRootURL;//本地缓存根目录
	public static string URL0_RomateRootURL;//远程地址

	//资源的类型决定资源的位置
	public static string URL1_RES = "/res";//资源根路径
	public static string URL1_TABLE = "/table";//配表根路径
	public static string URL2_SCENE ="/scene";//场景配表专用路径
	public static string URL2_MAP ="/map";//共有
	public static string URL2_ROLE ="/role";//角色资源路径
	public static string URL2_UI ="/ui";//ui资源路径
	public static string URL2_EFFECT ="/effect";//特效资源路径
	public static string URL2_SOUND ="/sound";//声音资源路径
	public static string URL2_SYSTEM ="/system";//系统配表路径
	public static void InitResConfig(){
		#if UNITY_ANDROID
		ApkRootURL = Application.streamingAssetsPath;
		//CacheRootURL = Application.persistentDataPath+"/cache";
		CacheRootURL = Application.persistentDataPath;
		CacheRootURL_FOR_WWWREAD = Application.persistentDataPath;//TODO 具体是否需要更改看上android后的状况
		RomateRootURL = "127.0.0.1";
		#else
		//pc上要加这个奇怪的开头
		URL0_ApkRootURL = "file:///"+Application.streamingAssetsPath;
		//CacheRootURL = "file:///"+Application.persistentDataPath+"/cache";
		URL0_CacheRootURL_FOR_WWWREAD = "file:///"+Application.persistentDataPath;
		URL0_CacheRootURL = Application.persistentDataPath;
		URL0_RomateRootURL = "127.0.0.1";
		#endif
		Log.i("读取资源版本Ver:"+ResVer);
	}
	#endregion


	
	//TODO 这里ID写什么值还需要处理，暂时没用到这些内容，当时资源管理器里加了这些特殊资源的判断，现在方案是这些资源是直接用文件名导入的
	/// <summary>
	/// 基础配表资源id
	/// 这些配表都是记录资源索引的，却没人记录它们，姑且都在这记录了,一共10个表
	/// apkVer,apkRes,apkUpdate,apkDynamic,
	/// cacheVer,cacheRes,cacheDynamic,
	/// romateVer,romateUpdate,romateDynamic
	/// </summary>

	public static int[] TableResID ={1,2,3,4,5,6,7,8,9,10};
	public static RES_POS[] TableResPos ={RES_POS.Location,RES_POS.Location,RES_POS.Location,RES_POS.Location,RES_POS.Cache,RES_POS.Cache,RES_POS.Cache,RES_POS.Romate,RES_POS.Romate,RES_POS.Romate};
	

	#region 资源类型常量,资源id解析,通过资源类型获取资源id
	//主类型限制16个，子类型限制128*4=512个
	
	/// <summary>
	/// 资源类型与ID说明
	/// 使用int来标识资源resID
	/// int 32位划分如下
	/// 低16位(1-16位)是具体类型资源的排序ID
	/// 高15位(17-31位)是资源类型(最高位时符号位，不使用)
	/// 17-19(3位,最多8种)是资源主类型(Prefab,Bytes等)
	/// 19-22(4位,最多16种)是资源游戏内按用途划分(Scene,Role,UI,movie,sound等)
	/// 23-31(8位,最多128种)留给具体类型资源做子类型扩展
	/// </summary>
	public const byte RES_TYPE0_PREFAB = 0;//资源
	public const byte RES_TYPE0_BYTES = 1;//配表统一用二进制
	public const byte RES_TYPE0_TEXTURE = 2;//暂时没用到
	
	public const byte RES_TYPE1_NULL = 0;//保留
	public const byte RES_TYPE1_SCENE = 1;//配表独有
	public const byte RES_TYPE1_MAP = 2;//共有
	public const byte RES_TYPE1_ROLE = 3;//共有
	public const byte RES_TYPE1_UI = 4;//不知道有没有配表
	public const byte RES_TYPE1_SOUND = 5;//资源独有
	public const byte RES_TYPE1_EFFECT = 6;//不知道有没有配表
	public const byte RES_TYPE1_SYSTEM_TABLE = 7;//跟资源总表在一个文件夹
	
	public const byte RES_TYPE2_NULL = 0;//需要空这个参数，比如系统配表就不需要填充
	//SCENE相关放一个文件夹里就可以，比如TERRAIN,LIGHT等,所以这个类型不用来找路径，用来计算resid用
	public const byte RES_TYPE2_MAP_TERRAIN = 0;//资源独有
	public const byte RES_TYPE2_MAP_LIGHT = 1;//资源独有
	public const byte RES_TYPE2_MAP_OBJ = 2;//资源独有
	public const byte RES_TYPE2_MAP_INFO = 3;//配表独有
	public const byte RES_TYPE2_MAP_BLOCK = 4;//配表独有
	public const byte RES_TYPE2_MAP_EVENT = 5;//配表独有
	
	//BODY同理跟SCENE一样，不用来找路径，用来计算resid用
	public const byte RES_TYPE2_ROLE_BONE = 0;
	public const byte RES_TYPE2_ROLE_EYES = 1;
	public const byte RES_TYPE2_ROLE_FACE = 2;
	public const byte RES_TYPE2_ROLE_HEAD = 3;
	public const byte RES_TYPE2_ROLE_TOP = 4;
	public const byte RES_TYPE2_ROLE_HANDS = 5;
	public const byte RES_TYPE2_ROLE_PANTS = 6;
	public const byte RES_TYPE2_ROLE_SHOES = 7;

	//public const byte RES_TYPE2_ROLE_BONE_MAT = 8;
	public const byte RES_TYPE2_ROLE_EYES_MAT = 8;
	public const byte RES_TYPE2_ROLE_FACE_MAT = 9;
	public const byte RES_TYPE2_ROLE_HEAD_MAT = 10;
	public const byte RES_TYPE2_ROLE_TOP_MAT = 11;
	public const byte RES_TYPE2_ROLE_HANDS_MAT = 12;
	public const byte RES_TYPE2_ROLE_PANTS_MAT = 13;
	public const byte RES_TYPE2_ROLE_SHOES_MAT = 14;

	public const byte RES_TYPE2_ROLE_ANIMGROUP = 15;
	/// <summary>
	/// 通过资源ID获取资源位置
	/// </summary>
	/// <returns>The URL by res I.</returns>
	/// <param name="resid">资源id</param>
	/// <param name="bHasResName">是否包含文件名，false时就是目录地址，true就是文件地址<c>true</c> b has res name.</param>
//	public static string GetUrlByResID(int resid,bool bHasResName=true){
//		StringBuilder sb = new StringBuilder();
//		//获取加载位置
//		ResPosInfo resPosInfo = ResMgr.GetIns().verManager.GetResPosInfoFromLocationTable(resid);
//		if(null == resPosInfo){
//			resPosInfo = ResMgr.GetIns().verManager.GetResPosInfoFromDynamicTable(resid);//查询动态加载表信息
//			if(null == resPosInfo){
//				Debug.LogError("ResEnum.GetUrlByResID:资源加载位置表中不存在ID为:"+resid+"的资源");
//				return null;
//			}
//		}
//		RES_POS resPos = resPosInfo.resPos;
//		switch(resPos){
//		case RES_POS.Location:
//			sb.Append(URL0_ApkRootURL);
//			break;
//		case RES_POS.Cache:
//			sb.Append(URL0_CacheRootURL_FOR_WWWREAD);
//			break;
//		case RES_POS.Dynamic:
//			sb.Append(URL0_RomateRootURL);
//			break;
//		}
//
//		//bool isNameContainIDWithoutType = true;//文件名是否包含不含类型的id
//		int resType0 =  GetResType0(resid);
//		int resType1 =  GetResType1(resid);
//		int resType2 =  GetResType2(resid);
//		int resIDWithoutType = GetResIDWithoutType(resid);
//		string extenName = "";
//		Print("GetUrlByResID:resType0:"+resType0+" resType1:"+resType1+" resType2:"+resType2+" resIDWithoutName:"+resIDWithoutType);
//		//获取主类型0目录
//		switch(resType0){
//		case RES_TYPE0_PREFAB:
//			sb.Append(URL1_RES);
//			extenName = ".n";
//			break;
//		case RES_TYPE0_BYTES:
//			sb.Append(URL1_TABLE);
//			extenName = ".bytes";
//			break;
//		}
//
//
//		//获取主类型1目录
//		switch(resType1){
//		case RES_TYPE1_SCENE:
//			sb.Append(URL2_SCENE);
//			break;
//		case RES_TYPE1_MAP:
//			sb.Append(URL2_MAP);
//			break;
//		case RES_TYPE1_ROLE:
//			sb.Append(URL2_ROLE);
//			break;
//		case RES_TYPE1_UI:
//			sb.Append(URL2_UI);
//			break;
//		case RES_TYPE1_SOUND:
//			sb.Append(URL2_SOUND);
//			break;
//		case RES_TYPE1_EFFECT:
//			sb.Append(URL2_EFFECT);
//			break;
//		case RES_TYPE1_SYSTEM_TABLE:
//			sb.Append(URL2_SYSTEM);
//			//isNameContainIDWithoutType = false;
//			break;
//		}
//
//		string resName = ResMgr.GetIns().verManager.GetResInfo(resid).ResName;
//
//
//		if(bHasResName){
//			sb.Append("/");
//			sb.Append(resName);
//			//if(isNameContainIDWithoutType)sb.Append(resIDWithoutType);
//			sb.Append(extenName);
//		}
//
//		return sb.ToString();
//	}

/// <summary>
/// 使用类型和id拼接出资源id
/// </summary>
	public static int GetResIDByTypeAndID(int resId,int type0,int type1,int type2,int id){
		int _resid = 0;
		_resid = SetResType0(resId,type0);
		_resid = SetResType1(_resid,type1);
		_resid = SetResType2(_resid,type2);
		_resid = SetResIDWithoutType(_resid,id);
		return _resid;
	}
	public static int GetResType0(int resID){ return (resID>>28)&0xF; }
	public static int GetResType1(int resID){ return (resID>>24)&0xF; }
	public static int GetResType2(int resID){ return (resID>>16)&0xFF; }
	public static int GetResIDWithoutType(int resID){ return resID&0xFFFF; }
	public static int SetResType0(int resID,int resType0){  resID = resID & 0xFFFFF | (resType0 << 28);  return resID; }
	public static int SetResType1(int resID,int resType1){  resID = resID & 0x70FFFFFF | (resType1 << 24); return resID; }
	public static int SetResType2(int resID,int resType2){  resID = resID & 0x7F00FFFF | (resType2 << 16); return resID; }
	public static int SetResIDWithoutType(int resID,int resIDWithoutType){ resID = resID & 0x7FFF0000 | resIDWithoutType; return resID; }
	#endregion

	
	#region 资源存储位置Enum
	public enum RES_POS{
		Location = 0,
		Cache =1,
		Romate = 2,//只是一个标识，没什么实际意义，代表首次更新的内容
		Dynamic = 3,//远程加载，被标识为Romate的都是动态下载资源，不存在Romate类型，所以Update类型都在首次下载中被转变为Cache了
	}
	#endregion
	
	#region 资源载入状态常量
	public enum RES_LOAD_STATE{
		Wait = 0,//等待加载
		Loading = 1,//从apk，cache，romate到内存的过程
		//Jiexi,//从www->obj,bytes,text的过程,加载完毕就直接解析了，不需要单独的状态
		Loadover = 2,//加载完毕
		Interrapt = 3,//被打断
		Error = 4,//加载失败
		Unload = 5,//被卸载
		DelayUnload = 6,//等待延迟卸载
		Null = 7,//空状态
	}
	#endregion

	#region 资源释放类型
	public enum RES_UNLOAD_TYPE{
		RefCount=0,//引用为0释放
		Delay=1,//延迟释放
		Never=2,//永不释放
	}
	#endregion

	#region 资源打包类型
	public enum RES_PACK_TYPE{//资源打包形式不同，解析方式就不同
		single=0,//单独打包Assetbundle
		multi=1,//多资源打包Assetbundle
		depend=2,//依赖打包Assetbundle
	}
	#endregion

	#region 资源载入优先级
	//数值越大优先级越高
	public const byte Load_Priority_Normarl = 0;//非依赖资源
	public const byte Load_Priority_Dependent = Load_Priority_Normarl+1;//依赖资源
	#endregion


}
