/// <summary>
/// Game Framework
/// 
/// 创建者：Hurley
/// 创建时间：2025-12-08
/// 功能描述：
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;

using UnityObject = UnityEngine.Object;

namespace Game.Module.Asset.NameSearcher
{
    /// <summary>
    /// 资源管理句柄的扩展接口
    /// </summary>
    public static class ResourceHandlerExtension
    {
        public const string AssetNameSearcherFile = @"AssetNameSearchList.json";

        /// <summary>
        /// 记录资源文件名和路径的映射关系的管理容器
        /// </summary>
        private static IDictionary<string, string> _assetPathMappingDict = new Dictionary<string, string>();

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="name">资源名字k</param>
        /// <param name="type">资源类型</param>
        public static UnityObject LoadAssetByName(this GameEngine.ResourceHandler self, string name, Type type)
        {
            string url = ConvertAssetNameToUrl(name);
            return self.LoadAsset(url, type);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="name">资源名字</param>
        /// <param name="completed">加载完成回调</param>
        public static GooAsset.Asset LoadAssetAsyncByName<T>(this GameEngine.ResourceHandler self, string name, Action<UnityObject> completed) where T : UnityObject
        {
            string url = ConvertAssetNameToUrl(name);
            return self.LoadAssetAsync<T>(url, completed);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="name">资源名字</param>
        public static async UniTask<T> LoadAssetAsyncByName<T>(this GameEngine.ResourceHandler self, string name) where T : UnityObject
        {
            string url = ConvertAssetNameToUrl(name);
            return await self.LoadAssetAsync<T>(url);
        }

        /// <summary>
        /// 通过名称和类型异步加载资源
        /// </summary>
        /// <param name="name">资源名字</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static async UniTask<UnityObject> LoadAssetAsyncByName(this GameEngine.ResourceHandler self, string name, Type type)
        {
            string url = ConvertAssetNameToUrl(name);
            return await self.LoadAssetAsync(url, type);
        }

        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="name">资源名字</param>
        /// <param name="isAdditive">是否使用叠加方式加载</param>
        public static GooAsset.Scene LoadSceneByName(this GameEngine.ResourceHandler self, string name, bool isAdditive = false)
        {
            string url = ConvertAssetNameToUrl(name);
            return self.LoadScene(url, isAdditive);
        }

        /// <summary>
        /// 异步加载场景(回调)
        /// </summary>
        /// <param name="name">资源地址(名字或路径)</param>
        /// <param name="isAdditive">是否使用叠加方式加载</param>
        /// <param name="completed">加载完成回调</param>
        public static GooAsset.Scene LoadSceneAsyncByName(this GameEngine.ResourceHandler self, string name, bool isAdditive, System.Action<GooAsset.Scene> completed)
        {
            string url = ConvertAssetNameToUrl(name);
            return self.LoadSceneAsync(url, isAdditive, completed);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="url">资源地址(名字或路径)</param>
        /// <param name="isAdditive">是否使用叠加方式加载</param>
        public static async UniTask<GooAsset.Scene> LoadSceneAsyncByName(this GameEngine.ResourceHandler self, string name, bool isAdditive = false)
        {
            string url = ConvertAssetNameToUrl(name);
            return await self.LoadSceneAsync(url, isAdditive);
        }

        /// <summary>
        /// 同步加载原始流式文件(直接读取persistentDataPath中的文件, 然后可根据文件保存路径(RawFile.savePath)读取文件, 使用同步加载前需已保证文件更新)
        /// <param name="name">文件原打包路径('%ORIGINAL_RESOURCE_PATH%/......', 若为Assets外部文件则为:'Assets文件夹同级目录/...'或'Assets文件夹同级文件')</param>
        /// </summary>
        public static GooAsset.RawFile LoadRawFileByName(this GameEngine.ResourceHandler self, string name)
        {
            string url = ConvertAssetNameToUrl(name);
            return self.LoadRawFile(url);
        }

        /// <summary>
        /// 异步加载原始流式文件(将所需的文件下载到persistentDataPath中, 完成后可根据文件保存路径(RawFile.savePath)读取文件)
        /// /// <param name="name">文件原打包路径('%ORIGINAL_RESOURCE_PATH%/......', 若为Assets外部文件则为:'Assets文件夹同级目录/...'或'Assets文件夹同级文件')</param>
        /// </summary>
        public static GooAsset.RawFile LoadRawFileAsyncByName(this GameEngine.ResourceHandler self, string name, System.Action<GooAsset.RawFile> completed)
        {
            string url = ConvertAssetNameToUrl(name);
            return self.LoadRawFileAsync(url, completed);
        }

        /// <summary>
        /// 异步加载原始流式文件(将所需的文件下载到persistentDataPath中, 完成后可根据文件保存路径(RawFile.savePath)读取文件)
        /// /// <param name="name">文件原打包路径('%ORIGINAL_RESOURCE_PATH%/......', 若为Assets外部文件则为:'Assets文件夹同级目录/...'或'Assets文件夹同级文件')</param>
        /// </summary>
        public static async UniTask<GooAsset.RawFile> LoadRawFileAsyncByName(this GameEngine.ResourceHandler self, string name)
        {
            string url = ConvertAssetNameToUrl(name);
            return await self.LoadRawFileAsync(url);
        }

        /// <summary>
        /// 使用前先预加载映射配置InitLoadAssetPathMapping
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static string ConvertAssetNameToUrl(string name)
        {
            return GetAssetUrlByFileName(name);
        }

        #region 读取映射文件

        /// <summary>
        /// 加载资源映射json文件
        /// </summary>
        public static async UniTask InitLoadAssetPathMapping(this GameEngine.ResourceHandler self)
        {
            if (_assetPathMappingDict.Count == 0)
            {
                string jsonPath = GooAsset.AssetPath.TranslateToLocalDataPath(AssetNameSearcherFile);
                string jsonContent = await ReadFileTextAsync(jsonPath);
                _assetPathMappingDict = LitJson.JsonMapper.ToObject<Dictionary<string, string>>(jsonContent);
            }
        }

        /// <summary>
        /// 通过文件名获取文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static string GetAssetUrlByFileName(string fileName)
        {
            if (_assetPathMappingDict.TryGetValue(fileName, out string fullPath))
            {
                return fullPath;
            }
            return string.Empty;
        }

        static async UniTask<string> ReadFileTextAsync(string filePath)
        {
            string text = await File.ReadAllTextAsync(filePath);
            if (GooAsset.Configure.Secret.ManifestFileEncryptEnabled)
            {
                return GooAsset.Utility.Cryptography.Decrypt(text);
            }

            return text;
        }

        #endregion
    }
}
