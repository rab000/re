using UnityEngine;
using System.Collections;
using System.IO;
using System;
/// <summary>
/// Nafio 文件操作
/// </summary>
public class FileHelper{

	/// <summary>
	/// 从指定路径目录查找所有符合过滤条件的文件名
	/// </summary>
	/// <returns>The file name.</returns>
	/// <param name="DirPath">目录地址</param>
	/// <param name="FilterStr">过滤字符串比如*.jpg查找jpg图片  *查找所有</param>
//	public static string[] FindFilesFromTargetDir(string DirPath,string FilterStr){//这个函数莫名奇妙不能用了，原因是拖入并覆盖了NDebug和RoleEditor两个类，sb UNITY
//
//		return Directory.GetFiles(DirPath,FilterStr,SearchOption.AllDirectories);
//	}
	
	#if UNITY_ANDROID
	public static byte[] ReadBytesFromFile(string url)
	{
		WWW www = new WWW(url);
		//N_TODO 没用异步和协程,释放问题？
		while(!www.isDone){}
		return www.bytes;
	}
	public static string ReadBytesFromFile(string url,int _c)
	{
		WWW www = new WWW(url);
		//N_TODO 没用异步和协程,释放问题？
		while(!www.isDone){}
		return www.text;
	}
	#else
	public static byte[] ReadBytesFromFile(string url)
	{
		if(!File.Exists(url)){
			Debug.Log(string.Format("找不到:{0} 读取失败！！",url));
			
			return null;
		}
		
		FileStream fs = new FileStream(url,FileMode.Open);
		//N_INFO 如果读大文件会出问题,不读图片就ok
		int len = (int)fs.Length;
		byte[] bytes = new byte[len];
		fs.Read(bytes,0,len);
		fs.Close();
		return bytes;
		
	}
	#endif
	
	public static void WriteBytes2File(string url,byte[] bytes){
		if(File.Exists(url))File.Delete(url);
		
		FileStream fs = new FileStream(url,FileMode.Create);
		fs.Write(bytes,0,bytes.Length);
		fs.Close();
	}

}