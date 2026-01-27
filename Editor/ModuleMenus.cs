/// <summary>
/// Game Framework
/// 
/// 创建者：Hurley
/// 创建时间：2025-12-08
/// 功能描述：
/// </summary>

using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEngine;

using GooAsset;
using GooAsset.Editor.Build;

using GameFramework.Asset.NameSearcher;

namespace GameFramework.Editor.Asset.NameSearcher
{
    [Serializable]
    internal class ManifestRoot
    {
        public List<ManifestBundleInfo> manifestBundleInfoList; // 资源包信息数组
    }

    /// <summary>
    /// 模块菜单
    /// </summary>
    static class ModuleMenus
    {
        const string MenuName_BuildAssetNameSearchTable = @"GooAsset/Module/构建资源名称索引文件";
        const string MenuName_CopyAssetNameSearchTableToStreamingAssetPath = @"GooAsset/Module/复制资源名称索引文件到StreamingAsset目录";

        #region 生成资源名称索引文件

        [MenuItem(MenuName_BuildAssetNameSearchTable, priority = 1000)]
        static void OnBuildAssetNameSearchTable()
        {
            GetAllAssetJson(BuildUtils.PlatformBuildPath);
        }

        [MenuItem(MenuName_CopyAssetNameSearchTableToStreamingAssetPath, priority = 1001)]
        static void OnCopyAssetMappingToStreamingAssetPath()
        {
            CopyFileToStreamingAssetsPath(ResourceHandlerExtension.AssetNameSearcherFile);
        }

        /// <summary>
        /// 将json配置生成映射表
        /// </summary>
        /// <param name="path"></param>
        static void GetAllAssetJson(string path)
        {
            //1. 遍历所有资源配置，获取配置名
            string configNames = string.Empty;
            List<string> namelist = new List<string>();
            foreach (ManifestConfig manifestConfig in BuildUtils.GetAllManifestConfigs())
            {
                namelist.Add(manifestConfig.name);
            }

            Dictionary<string, string> pathsDict = new Dictionary<string, string>();
            // 2. 获取 JSON 文件所在目录
            string jsonFolderPath = path;

            // 3. 遍历目录下所有 .json 文件
            string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json", SearchOption.AllDirectories);

            // 4. 逐个处理 JSON 文件
            foreach (string filePath in jsonFiles)
            {
                bool isHad = false;
                foreach (string configName in namelist)
                {
                    if (filePath.Contains(configName))
                    {
                        isHad = true;
                        break;
                    }
                }
                if (!isHad)
                {
                    Debug.Log($"--无--处理文件：{filePath}");
                    continue;

                }
                Debug.Log($"---处理文件：{filePath}");
                try
                {
                    string jsonContent = File.ReadAllText(filePath); // 读取文件内容
                    ManifestRoot root = JsonUtility.FromJson<ManifestRoot>(jsonContent); // 反序列化

                    if (root != null && root.manifestBundleInfoList != null)
                    {
                        foreach (var bundle in root.manifestBundleInfoList)
                        {
                            foreach (string assetPath in bundle.a)
                            {
                                string assetName = Path.GetFileName(assetPath);
                                // Debug.Log(resourceName + "-----File------" + resourcePath);
                                // 将键值对写入目标字典
                                if (!pathsDict.ContainsKey(assetName))
                                {
                                    pathsDict[assetName] = assetPath;
                                }
                                else
                                {
                                    Debug.LogError("不允许文件名重复，文件名：" + assetName);
                                }
                            }

                            //Debug.Log($"\n资源名：{bundle.n}");
                            //Debug.Log($"是否原始文件：{bundle.IsRawFile}");
                            //Debug.Log($"资源路径：{string.Join(", ", bundle.a)}");
                            //Debug.Log($"资源大小：{bundle.s} 字节");
                            //Debug.Log($"资源哈希：{bundle.h}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"反序列化失败，文件结构不匹配：{filePath}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"处理文件 {filePath} 时出错：{e.Message}");
                }
            }

            SaveDictToJson(pathsDict, ResourceHandlerExtension.AssetNameSearcherFile);
        }

        /// <summary>
        /// 将 Dictionary<string, string> 保存为 JSON 文件
        /// </summary>
        /// <param name="dict">要保存的字典</param>
        /// <param name="fileName">文件名</param>
        public static void SaveDictToJson(Dictionary<string, string> dict, string fileName)
        {

            // 1. 序列化字典为 JSON 字符串
            string json = LitJson.JsonMapper.ToJson(dict);

            // 2. 确定文件保存路径
            string savePath = Path.Combine(BuildUtils.PlatformBuildPath, fileName);

            try
            {
                // 3. 写入 JSON 字符串到文件
                File.WriteAllText(savePath, json);
                Debug.Log($"JSON 文件保存成功！路径：{savePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JSON 文件保存失败：{e.Message}");
            }
        }

        /// <summary>
        /// 拷贝fromFileName文件到StreamingAssetsPath目录下
        /// </summary>
        /// <param name="fromFileName"></param>
        /// <param name="destFileName"></param>
        static void CopyFileToStreamingAssetsPath(string fromFileName, string destFileName = null)
        {
            // 不传入目标文件名时使用源文件名
            destFileName ??= fromFileName;

            string srcFilePath = BuildUtils.TranslateToBuildPath(fromFileName);
            string destFilePath = AssetPath.CombinePath(BuildUtils.BuildLocalDataPath, destFileName);
            if (!File.Exists(srcFilePath))
            {
                Debug.LogWarning($"所需首包文件{srcFilePath}不存在, 请检查原因(例:是否打成游戏安装包前没有构建资源包？)");
                return;
            }

            string destFolderPath = Path.GetDirectoryName(destFilePath);
            if (!Directory.Exists(destFolderPath))
            {
                Directory.CreateDirectory(destFolderPath);
            }

            File.Copy(srcFilePath, destFilePath, true);
        }

        #endregion
    }
}
