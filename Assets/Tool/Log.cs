using UnityEngine;
//using System.Collections;

/// <summary>
/// Nafio 统一控制log
/// 避免发布版本有Debug输出影响效率
/// 进一步要在真机做控制台
/// </summary>
public class Log{
	#if UNITY_ANDROID
	public static bool beOpen = false;
	#else
	public static bool beOpen = true;
	#endif

	public static void i(string s)
	{
		if(!beOpen)return;
		Debug.Log(s);
	}

	/// <summary>
	/// 用于单独模块的测试，方便各个模块log开关
	/// </summary>
	/// <param name="className">Class name.</param>
	/// <param name="funcName">Func name.</param>
	/// <param name="content">Content.</param>
	/// <param name="beShow">If set to <c>true</c> be show.</param>
	public static void i(string className,string funcName,string content,bool beShow=true)
	{
		if(!beOpen)return;
		if (!beShow)return;
		Debug.Log("[i]-["+className+"->"+funcName+"]["+content+"]");
	}

	public static void w(string s)
	{
		if(!beOpen)return;
		Debug.LogWarning(s);
	}

	public static void w(string className,string funcName,string content,bool beShow=true)
	{
		if(!beOpen)return;
		if (!beShow)return;
		Debug.LogWarning("[w]-["+className+"->"+funcName+"]["+content+"]");
	}

	public static void e(string s)
	{
		if(!beOpen)return;
		Debug.LogError(s);
	}

	public static void e(string className,string funcName,string content,bool beShow=true)
	{
		if(!beOpen)return;
		if (!beShow)return;
		Debug.LogError("[e]-["+className+"->"+funcName+"]["+content+"]");
	}
}
