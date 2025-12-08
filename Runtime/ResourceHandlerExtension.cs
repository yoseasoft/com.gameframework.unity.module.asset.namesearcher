/// <summary>
/// Game Framework
/// 
/// 创建者：Hurley
/// 创建时间：2025-12-08
/// 功能描述：
/// </summary>

using System;

using UnityObject = UnityEngine.Object;

namespace Game.Module.Asset.NameSearcher
{
    /// <summary>
    /// 资源管理句柄的扩展接口
    /// </summary>
    public static class ResourceHandlerExtension
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="name">资源名字</param>
        /// <param name="type">资源类型</param>
        public static UnityObject LoadAssetByName(this GameEngine.ResourceHandler self, string name, Type type)
        {
            string url = ConvertAssetNameToUrl(name);
            return self.LoadAsset(url, type);
        }

        static string ConvertAssetNameToUrl(string name)
        {
            return name;
        }
    }
}
