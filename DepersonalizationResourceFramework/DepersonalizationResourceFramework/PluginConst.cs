using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DepersonalizationResourceFramework
{
    public static class PluginConst
    {
        //插件配置
        public static class PluginConfig
        {
            public const string PLUGIN_GUID = "irisu.fyumi.depersonalizationresourceframework";
            public const string PLUGIN_NAME = "DepersonalizationResourceFramework";
            public const string PLUGIN_VERSION = "1.0.0";
            public const string WELCOME_WORLD = "\n\n\n" +
                "欢迎关注 安堂いなり(安堂稻荷) https://space.bilibili.com/392505232" +
                "\n" +
                "是擅长各类恐怖游戏的可爱机械狐狐" +
                "\n" +
                "《人格解体》直播视频：https://www.bilibili.com/video/BV1SD4y177ne/?share_source=copy_web&vd_source=e242b81f6701dd9662b9bfedc6729eba" +
                "\n\n\n";
            public const string CONFIG_SECTION = "资源替换框架";
            public const string CONFIG_IS_OUTPUT = "是否导出资源";
            public const string CONFIG_IS_OUTPUT_DESCRIPTION = "开启后游戏加载资源时会自动导出一份到本地";
            public const string CONFIG_IS_INPUT = "是否替换资源";
            public const string CONFIG_IS_INTPUT_DESCRIPTION = "开启后如果替换目录下存在资源，则会替换游戏内资源";
        }

        //需要解析的资源类名
        public static class ResourceTypeName
        {
            public const string SPRITE_NAME = "Sprite";
            public const string TEXTURE2D_NAME = "Texture2D";
        }
        
        public static class PluginPathConfig
        {
            public const string FrameworkPath = "ResourcesFramework";
            public const string OutputPath = "OutPut";
            public const string IntputPath = "Input";
            public const string ConfigFileName = "config.json";
            public const string ConfigItemDescription = "description";
            public const string ConfigItemPath = "path";
            public const string ConfigItemName = "name";
            public const string ConfigItemOrder = "index";
            public const string ConfigLocalName = "本地替换目录";
        }
    }
}
