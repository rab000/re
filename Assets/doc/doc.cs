using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doc : MonoBehaviour {

	//	规划2019.2.5
	//
	//	角色编辑器使用两种打包导出机制
	//	男女换装模型，其他简单模型(非换装)(暂未实现)
	//
	//
	//	换装模型原始资源文件夹说明
	//	anim  			存放动作组
	//	bone 			存放裸模，用于提取各个
	//	mat   			材质
	//	model			存放部位模型
	//	modelA
	//	face
	//	head
	//	top
	//	hand
	//	down
	//	shoes
	//	modelB
	//	modelC
	//	textures    	贴图
	//
	//
	//
	//	特殊测试记录
	//	mat修改后，计算md5，发现没有变化，必须要要对着材质ctrl+s一次计算出的md5才有变化
	//	ctrl+s做了mat的保持工作，走了DetectModify.OnWillSaveAssets
	//	即使对着mat右键，reinport也不会走上面的保存，只能ctrl+s
	//
	//	模型和材质发生变化时，模型的变化记录在meta中，而材质的变化记录在.mat文件本身(meta文件没发生变化)


	//	注意事项
	//	Asset4Build文件夹和外部data都被删除，如果md5文件夹还存在的话，资源也不会重新打包，需要手动删除md5文件夹


}
