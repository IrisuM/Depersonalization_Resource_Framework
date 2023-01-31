using ES3Types;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.IO;
using UnityEngine.UIElements;
using UnityEngine.XR;
using File = System.IO.File;
using Object = UnityEngine.Object;
using ResourceName = DepersonalizationResourceFramework.PluginConst.ResourceTypeName;

namespace DepersonalizationResourceFramework
{
    internal static class ReplaceTool
    {
        public static Texture2D LoadTexture(byte[] bytes)
        {
            return LoadTexture(0, 0, bytes, TextureFormat.RGBA64);
        }
        public static Texture2D LoadTexture(int w, int h, byte[] bytes, TextureFormat format)
        {
            Texture2D texture = new Texture2D(w, h, format, false);
            texture.filterMode = FilterMode.Point;
            texture.LoadImage(bytes);
            return texture;
        }
        static Texture2D DuplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height, TextureFormat.RGBA64, false);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
        static Texture2D DuplicateTexture(Sprite source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.texture.width,
                        source.texture.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source.texture, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D((int)source.textureRect.width, (int)source.textureRect.height, TextureFormat.RGBA64, false);
            readableText.ReadPixels(new Rect(source.textureRect.x, source.texture.height - source.textureRect.y - source.textureRect.height, source.textureRect.width, source.textureRect.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
        static void WriteFile(string path, byte[] bytes)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            System.IO.File.WriteAllBytes(path, bytes);
        }
        public static Sprite Sprite_Replace(string path, Sprite sprite)
        {
            if (Plugin.s_Instance.is_output && sprite != null && sprite.texture != null)
            {
                Texture2D texture = DuplicateTexture(sprite);
                byte[] bytes = texture.EncodeToPNG();
                WriteFile(ResourceHelper.GetOutputDir() + "/" + path + ".png", bytes);
            }

            string replace_path = "";
            //替换文件
            if (Plugin.s_Instance.is_replace && ResourceHelper.CanReplacePNG(path, out replace_path))
            {
                byte[] bytes = File.ReadAllBytes(replace_path);
                Texture2D texture = LoadTexture(bytes);
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                sprite = Sprite.Create(texture, rect, rect.center);
            }
            return sprite;
        }
        public static Texture2D Texture2D_Replace(string path, Texture2D texture)
        {
            //输出文件
            if (Plugin.s_Instance.is_output && texture != null)
            {
                Texture2D out_texture = DuplicateTexture(texture);
                byte[] bytes = out_texture.EncodeToPNG();
                WriteFile(ResourceHelper.GetOutputDir() + "/" + path + ".png", bytes);
            }

            string replace_path = "";
            //替换文件
            if (Plugin.s_Instance.is_replace && ResourceHelper.CanReplacePNG(path, out replace_path))
            {
                byte[] bytes = File.ReadAllBytes(replace_path);
                texture = LoadTexture(bytes);
            }
            return texture;
        }

        //游戏定制化支持模型图片导出，方便替换
        public static GameObject SpriteAnimation_Replace(string path, GameObject model)
        {
            //输出文件
            if (Plugin.s_Instance.is_output && model != null)
            {
                RoleModel role = model.GetComponent<RoleModel>();
                foreach (HeadIconData headIconData in role.HeadIcons)
                {
                    if (headIconData == null) continue;
                    headIconData.Icon = Sprite_Replace(path + "/" + headIconData.Icon.name, headIconData.Icon);
                }
                foreach (SpriteAnimationData spriteAnimationData in role.SpriteAnim.AnimationList)
                {
                    string animation_name = spriteAnimationData.Key;
                    foreach (SpriteConfigData spriteConfigData in spriteAnimationData.SpriteDatas)
                    {
                        if (spriteConfigData == null) continue;
                        Texture2D texture = Texture2D_Replace(path + "/" + model.gameObject.name.Replace("(Clone)", "") + "/" + animation_name + "/" + spriteConfigData.Sprite.name, spriteConfigData.Sprite.texture);
                        if (texture != spriteConfigData.Sprite.texture)
                        {
                            spriteConfigData.Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0), 64);
                        }
                    }
                    for (int i = 0; i < spriteAnimationData.Sprites.Count; i++)
                    {
                        Sprite sprite = spriteAnimationData.Sprites[i];
                        if (sprite == null) continue;
                        Texture2D texture = Texture2D_Replace(path + "/" + model.gameObject.name.Replace("(Clone)", "") + "/" + animation_name + "/" + sprite.name, sprite.texture);
                        if (texture != sprite.texture)
                        {
                            spriteAnimationData.Sprites[i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0), 64);
                        }
                    }
                }

            }

            return model;
        }
    }
    internal class UIRoot_Initialize_Patch
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIRoot), "Initialize")]
        static void UIRoot_Initialize_Postfix()
        {
            ResourceHelper.ReloadPath();
        }
    }
    internal class ES3_Load_Patch
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ES3), "Load", new Type[] { typeof(string), typeof(ES3Settings) })]
        static object ES3_Load_Postfix(ref object __result, string key, ES3Settings settings)
        {
            Plugin.Log.LogMessage(string.Format("key:{0}", key));
            return __result;
        }
    }
    internal class RoleModel_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RoleModel), "Awake")]
        static void RoleModel_Awake_Postfix(ref RoleModel __instance)
        {
            ReplaceTool.SpriteAnimation_Replace("Charator/Prefabs", __instance.gameObject);
        }
    }
    internal class UnityEngine_Resources_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Resources), "Load", new Type[] { typeof(string), typeof(Type) })]
        static void UnityEngine_Resources_Load_Postfix(ref Object __result, string path, Type systemTypeInstance)
        {
            //官方有时候资源名写错之类的，所以可能造成加载失败，直接pass掉
            /*if (__result == null)
            {
                Plugin.Log.LogError(string.Format("资源加载错误:{0}", path));
                return;
            }*/
            try
            {
                //分类输出和加载资源
                switch (systemTypeInstance.Name)
                {
                    case ResourceName.SPRITE_NAME:
                        {
                            __result = ReplaceTool.Sprite_Replace(path, __result as Sprite);
                        }
                        break;
                    case ResourceName.TEXTURE2D_NAME:
                        {
                            __result = ReplaceTool.Texture2D_Replace(path, __result as Texture2D);
                        }
                        break;
                    case ResourceName.OBJECT_NAME:
                    case ResourceName.GAMEOBJECT_NAME:
                        {
                            bool is_replace = false;
                            //定制化启用，要定制化就定制到底咯
                            //定制化支持游戏部分模型
                            /*if (path.StartsWith("Charator/Prefabs", StringComparison.OrdinalIgnoreCase))
                            {
                                __result = SpriteAnimation_Replace(path, __result as GameObject);
                                is_replace = true;
                            }*/

                            if (!is_replace)
                            {
                                Plugin.Log.LogMessage(string.Format("path:{0} type:{1}", path, systemTypeInstance.FullName));
                            }
                        }
                        break;
                    default:
                        Plugin.Log.LogMessage(string.Format("path:{0} type:{1}", path, systemTypeInstance.FullName));
                        break;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(string.Format("资源加载错误:{0} {1}", path, e.Message));
            }

        }
    }
}
