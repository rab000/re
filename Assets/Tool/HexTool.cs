using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
//此类未经验证
public class HexTool {

	public static int StrHex2Int(string hexStr){
		byte[] bs = StrToToHexByte (hexStr);
		int num = Bytes2Int (bs);
		return num;
	}

	public static int Bytes2Int(byte[] bs)
	{
		int num=12345;
		byte[] bytes=BitConverter.GetBytes(num);//将int32转换为字节数组
		num=BitConverter.ToInt32(bytes,0);//将字节数组内容再转成int32类型
		return num;
	}

	public static string HexStringToString(string hs, Encoding encode)
	{
		string strTemp="";
		byte[] b=new byte[hs.Length/2];
		for (int i = 0; i < hs.Length / 2; i++)
		{
			strTemp = hs.Substring(i * 2, 2);
			b[i] = Convert.ToByte(strTemp, 16);
		}
		//按照指定编码将字节数组变为字符串
		return encode.GetString(b);
	}

	public static string ByteToHexStr(byte[] bytes)
	{
		string returnStr = "";
		if (bytes != null)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				returnStr += bytes[i].ToString("X2");
			}
		}
		return returnStr;
	}

	public static string StringToHexString(string s, Encoding encode)
	{
		byte[] b = encode.GetBytes(s);//按照指定编码将string编程字节数组
		string result = string.Empty;
		for (int i = 0; i < b.Length; i++)//逐字节变为16进制字符
		{
			result += Convert.ToString(b[i], 16);
		}
		return result;
	}

	public static byte[] StrToToHexByte(string hexString)
	{
		hexString = hexString.Replace(" ", "");
		if ((hexString.Length % 2) != 0)
			hexString += " ";
		byte[] returnBytes = new byte[hexString.Length / 2];
		for (int i = 0; i < returnBytes.Length; i++)
			returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
		return returnBytes;
	}
}
