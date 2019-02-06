using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class MD5
{

	public static string Md5Sum (string strToEncrypt)
	{
		byte[] bs = UTF8Encoding.UTF8.GetBytes (strToEncrypt);
		System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5CryptoServiceProvider.Create ();

		byte[] hashBytes = md5.ComputeHash (bs);

		string hashString = "";
		for (int i = 0; i < hashBytes.Length; i++) {
			hashString += System.Convert.ToString (hashBytes [i], 16).PadLeft (2, '0');
		}
		return hashString.PadLeft (32, '0');
	}

}
