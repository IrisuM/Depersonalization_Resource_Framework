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
    internal class UIRoot_Initialize_Patch
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIRoot), "Initialize")]
        static void UIRoot_Initialize_Postfix()
        {
            ResourceHelper.ReloadPath();
        }
    }
    internal class UnityEngine_Resources_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Resources), "Load", new Type[] { typeof(string), typeof(Type) })]
        static void UnityEngine_Resources_Load_Postfix(ref Object __result, string path, Type systemTypeInstance)
        {
            if (__result == null)
            {
                Plugin.Log.LogError(string.Format("资源加载错误:{0}", path));
                return;
            }
            try
            {
                switch (systemTypeInstance.Name)
                {
                    case ResourceName.SPRITE_NAME:
                        {
                            if ((__result as Sprite) != null)
                            {
                                __result = Sprite_Replace(path, __result as Sprite);
                            }
                            else
                            {
                                Plugin.Log.LogError(string.Format("资源加载Sprite类型错误:{0}", path));
                            }
                        }
                        break;
                    case ResourceName.TEXTURE2D_NAME:
                        {
                            if ((__result as Texture2D) != null)
                            {
                                __result = Texture2D_Replace(path, __result as Texture2D);
                            }
                            else
                            {
                                Plugin.Log.LogError(string.Format("资源加载Texture2D类型错误:{0}", path));
                            }
                            //__result = Texture2D_Replace(path, __result as Texture2D);
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
        static Texture2D LoadTexture(int w, int h, byte[] bytes, TextureFormat format)
        {
            Texture2D texture = new Texture2D(w, h, format, false);
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
            Texture2D readableText = new Texture2D(source.width, source.height,source.format,false);
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
            Texture2D readableText = new Texture2D((int)source.textureRect.width, (int)source.textureRect.height,source.texture.format,false);
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
        static Sprite Sprite_Replace(string path, Sprite sprite)
        {
            if (sprite.texture == null)
            {
                Plugin.Log.LogError(string.Format("Sprite资源加载错误:{0}", path));
                return sprite;
            }
            if (Plugin.s_Instance.is_output)
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
                Texture2D texture = LoadTexture(sprite.texture.width, sprite.texture.height, bytes,sprite.texture.format);
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), sprite.pivot, sprite.pixelsPerUnit, 0, SpriteMeshType.Tight, sprite.border, false);
            }
            return sprite;
        }
        static Texture2D Texture2D_Replace(string path, Texture2D texture)
        {
            //输出文件
            if (Plugin.s_Instance.is_output)
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
                texture = LoadTexture(texture.width, texture.height, bytes,texture.format);
            }
            return texture;
        }
    }
}
