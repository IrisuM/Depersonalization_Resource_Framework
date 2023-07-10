using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using PluginConfig = DepersonalizationResourceFramework.PluginConst.PluginConfig;

namespace DepersonalizationResourceFramework
{
    [BepInPlugin(PluginConfig.PLUGIN_GUID, PluginConfig.PLUGIN_NAME, PluginConfig.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Plugin s_Instance;

        private ConfigEntry<bool> is_output_config;
        private ConfigEntry<bool> is_replace_config;
        public bool is_output { get { return is_output_config.Value; } }
        public bool is_replace { get { return is_replace_config.Value; } }
        private void Awake()
        {
            // Plugin startup logic
            Log = Logger;
            s_Instance = this;
            Logger.LogInfo($"Plugin {PluginConfig.PLUGIN_GUID} is loaded!");
            Logger.LogInfo(PluginConfig.WELCOME_WORLD);

            is_output_config = Config.Bind(PluginConfig.CONFIG_SECTION, PluginConfig.CONFIG_IS_OUTPUT, false, new ConfigDescription(PluginConfig.CONFIG_IS_OUTPUT_DESCRIPTION));
            is_replace_config = Config.Bind(PluginConfig.CONFIG_SECTION, PluginConfig.CONFIG_IS_INPUT, true, new ConfigDescription(PluginConfig.CONFIG_IS_INTPUT_DESCRIPTION));

            Harmony harmony = new Harmony(PluginConfig.PLUGIN_GUID);
            harmony.PatchAll(typeof(UnityEngine_Resources_Patch));
            harmony.PatchAll(typeof(WorkshopHelper_EndUgcLoad_Patch));

            
            if(AccessTools.Method(typeof(RoleModel), "Awake") != null)
            {
                harmony.Patch(AccessTools.Method(typeof(RoleModel), "Awake"), new HarmonyMethod(typeof(RoleModel_Patch), "RoleModel_Awake_Postfix"));
            }
            
        }
    }
}