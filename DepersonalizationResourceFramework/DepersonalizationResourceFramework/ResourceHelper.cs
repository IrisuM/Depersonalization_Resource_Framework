using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using PluginPathConfig = DepersonalizationResourceFramework.PluginConst.PluginPathConfig;

namespace DepersonalizationResourceFramework
{
    public static class ResourceHelper
    {
        static List<LOAD_CONFIG> load_sort = new List<LOAD_CONFIG>();
        public struct LOAD_CONFIG
        {
            public string name;
            public string description;
            public string path;
            public int index;
        }
        public static string GetOutputDir()
        {
            return Application.streamingAssetsPath + "/" + PluginPathConfig.FrameworkPath + "/" + PluginPathConfig.OutputPath;
        }
        public static string GetInputDir()
        {
            return Application.streamingAssetsPath + "/" + PluginPathConfig.FrameworkPath + "/" + PluginPathConfig.IntputPath;
        }
        public static string JsonRegex(JsonData jsonData)
        {
            Regex reg = new Regex(@"(?i)\\[uU]([0-9a-f]{4})");
            return reg.Replace(jsonData.ToJson(), delegate (Match m) { return ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString(); });
        }
        public static Dictionary<string, LOAD_CONFIG> SetLoadDir(Dictionary<string, LOAD_CONFIG> now_config)
        {
            Dictionary<string, LOAD_CONFIG> result = new Dictionary<string, LOAD_CONFIG>();
            //添加已有配置
            foreach (KeyValuePair<string, LOAD_CONFIG> config in now_config)
            {
                if (!result.ContainsKey(config.Key) && Directory.Exists(config.Value.path))
                {
                    result.Add(config.Key, config.Value);
                }
            }
            int index = -1;
            //更新新增配置
            if (SteamRuntimeManager.HasInstance)
            {
                foreach (var e in SteamRuntimeManager.Instance.SteamUGC_AllItems)
                {
                    Plugin.Log.LogDebug(e.SaveFolderPath);
                    if (e.LocalData.IsActive &&
                        e.SaveFolderPath != null &&
                        Directory.Exists(e.SaveFolderPath + "/" + PluginPathConfig.FrameworkPath + "/" + PluginPathConfig.IntputPath) &&
                        !result.ContainsKey(e.Name.GetHashCode().ToString()))
                    {
                        result.Add(e.Name.GetHashCode().ToString(), new LOAD_CONFIG { name = e.Name, description = e.Des, path = e.SaveFolderPath + "/" + PluginPathConfig.FrameworkPath + "/" + PluginPathConfig.IntputPath, index = index });
                        index--;
                    }
                    if(!e.LocalData.IsActive&&result.ContainsKey(e.Name.GetHashCode().ToString()))
                    {
                        result.Remove(e.Name.GetHashCode().ToString());
                    }
                }
            }
            //添加本地配置
            if (!result.ContainsKey(PluginPathConfig.ConfigLocalName.GetHashCode().ToString()))
            {
                result.Add(PluginPathConfig.ConfigLocalName.GetHashCode().ToString(), new LOAD_CONFIG { name = PluginPathConfig.ConfigLocalName, path = GetInputDir(), description = "", index = index });
                index--;
            }
            //更新序号
            load_sort = new List<LOAD_CONFIG>(from pair in result.OrderBy(pair => pair.Value.index) select pair.Value);
            for (int i = 0; i < load_sort.Count; i++)
            {
                string key = load_sort[i].name.GetHashCode().ToString();
                LOAD_CONFIG e = result[key];
                e.index = i;
                result[key] = e;
            }
            return result;
        }

        public static void ReloadPath()
        {
            //读取资源替换列表
            string config_path = GetInputDir() + "/" + PluginPathConfig.ConfigFileName;
            string config_str = "";
            if (!File.Exists(config_path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(config_path));
                File.Create(config_path).Close();
            }
            else
            {
                config_str = File.ReadAllText(GetInputDir() + "/" + PluginPathConfig.ConfigFileName);
            }
            JsonData data = JsonMapper.ToObject(config_str);
            Dictionary<string, LOAD_CONFIG> configs = new Dictionary<string, LOAD_CONFIG>();
            foreach (string key in (data as IDictionary).Keys)
            {
                if ((data[key] as IDictionary).Contains(PluginPathConfig.ConfigItemPath) &&
                    (data[key] as IDictionary).Contains(PluginPathConfig.ConfigItemDescription) &&
                    (data[key] as IDictionary).Contains(PluginPathConfig.ConfigItemName) &&
                    (data[key] as IDictionary).Contains(PluginPathConfig.ConfigItemOrder))
                {
                    configs[key] = new LOAD_CONFIG { name = (string)data[key][PluginPathConfig.ConfigItemName], description = (string)data[key][PluginPathConfig.ConfigItemDescription], path = (string)data[key][PluginPathConfig.ConfigItemPath], index = (int)data[key][PluginPathConfig.ConfigItemOrder] };
                }
            }
            JsonData data_new = new JsonData();
            configs = SetLoadDir(configs);
            foreach (KeyValuePair<string, LOAD_CONFIG> config in configs)
            {
                data_new[config.Key] = new JsonData();
                data_new[config.Key][PluginPathConfig.ConfigItemName] = config.Value.name;
                data_new[config.Key][PluginPathConfig.ConfigItemDescription] = config.Value.description;
                data_new[config.Key][PluginPathConfig.ConfigItemPath] = config.Value.path;
                data_new[config.Key][PluginPathConfig.ConfigItemOrder] = config.Value.index;
            }
            File.WriteAllText(GetInputDir() + "/" + PluginPathConfig.ConfigFileName, JsonRegex(data_new));
            Plugin.Log.LogMessage("加载资源替换列表完成");
        }

        public static bool CanReplacePNG(string path, out string replace_path)
        {
            replace_path = "";
            for (int i = 0; i < load_sort.Count; i++)
            {
                if (File.Exists(load_sort[i].path + "/" + path + ".png"))
                {
                    replace_path = load_sort[i].path + "/" + path + ".png";
                    Plugin.Log.LogMessage(string.Format("load {0} replace to {1}", path, replace_path));
                    return true;
                }
            }
            return false;
        }
    }
}
