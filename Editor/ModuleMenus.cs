/// <summary>
/// Game Framework
/// 
/// 创建者：Hurley
/// 创建时间：2025-12-08
/// 功能描述：
/// </summary>

using UnityEditor;
using UnityEngine;

namespace Game.Module.Asset.NameSearcher.Editor
{
    /// <summary>
    /// 模块菜单
    /// </summary>
    static class ModuleMenus
    {
        const string MenuName_BuildAssetNameSearchTable = @"GooAsset/Module/构建资源名称索引文件";

        [MenuItem(MenuName_BuildAssetNameSearchTable)]
        static void OnBuildAssetNameSearchTable()
        {
            Debug.Log("开始构建资源名称索引文件");
        }
    }
}
