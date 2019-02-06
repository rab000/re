using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Nafio 数据缓冲区
/// 暂时不能异步读大文件(貌似目标不是读大文件用的，网络和数据存储用)
/// </summary>
public class IoBuffer{
	
	byte[] buffer;
	int putIndex = 0;
	int getIndex = 0;


	public IoBuffer(int num = 1024){
		buffer = new byte[num];
	}
	
	public void PutBool(bool b){
		if(b)buffer[putIndex] = (byte)1;
		else buffer[putIndex] = (byte)0;
		putIndex++;

	}
	public void PutByte(byte b){
		buffer[putIndex] = b;
		putIndex++;
	}
	
	public void PutShort(short value){
		byte[] bytes = flip(BitConverter.GetBytes(value));
		Array.Copy(bytes, 0, buffer, putIndex, bytes.Length);
		putIndex = putIndex + bytes.Length;
	}

	public void PutInt(int value){
		byte[] bytes = flip(BitConverter.GetBytes(value));
		Array.Copy(bytes, 0, buffer, putIndex, bytes.Length);
		putIndex = putIndex + bytes.Length;
	}
	public void PutFloat(float value){
		byte[] bytes = flip(BitConverter.GetBytes(value));
		Array.Copy(bytes, 0, buffer, putIndex, bytes.Length);
		putIndex = putIndex + bytes.Length;
	}
	
	public void PutString(string value){
	
		byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(value);
		bytes = flip(bytes);
		PutInt(bytes.Length);

		Array.Copy(bytes, 0, buffer, putIndex, bytes.Length);
		putIndex = putIndex + bytes.Length;

	}


	public void PutBytes(byte[] bytes){
		Array.Copy(bytes, 0, buffer, putIndex, bytes.Length);
		putIndex = putIndex + bytes.Length;
	}



	public bool GetBool(){
		byte value = buffer[getIndex];
		if(value == 0){
			getIndex++;
			return false;
		}
		else {
			if(value!=1)Debug.Log("GetBool error");
			getIndex++;
			return true;
		}

	}

	public byte GetByte(){
		byte value = buffer[getIndex];
		getIndex++;
		return value;
	}

	public short GetShort()
	{
		return BitConverter.ToInt16(Get(2), 0);
	}

	public int GetInt(){
		return BitConverter.ToInt32(Get(4), 0);
	}
	
	public float GetFloat(){
		return BitConverter.ToSingle(Get(4), 0);
	}

	public string GetString(){
		int size = GetInt();
		return System.Text.UTF8Encoding.UTF8.GetString(Get(size));
		//return BitConverter.ToSingle(Get(4), 0);
	}

	byte[] flip(byte[] bytes){
		if (BitConverter.IsLittleEndian){
			Array.Reverse(bytes);
		}
		return bytes;
	}



	byte[] Get(int len){
		byte[] bytes = new byte[len];
		Array.Copy(buffer, getIndex, bytes, 0, len);
		if (BitConverter.IsLittleEndian){
			Array.Reverse(bytes);
		}
		getIndex += len;
		return bytes;
	}

	public void Clear(){
		putIndex = 0;
		getIndex = 0;
		Array.Clear(buffer,0,buffer.Length);
		//buffer = null;
	}

	public byte[] ToArray(){
		byte[] bytes = new byte[putIndex];
		Array.Copy(buffer, 0, bytes, 0, bytes.Length);
		return bytes;
	}
}
