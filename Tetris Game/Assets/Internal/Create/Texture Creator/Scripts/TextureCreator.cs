using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Internal.Create
{
    public class TextureCreator : MonoBehaviour
    {

        [SerializeField] private RenderTextureFormat textureFormat;
        [SerializeField] private FilterMode filterMode;

        void Start()
        {
            Render();
        }

        private void Render()
        {
            Camera cam = GetComponent<Camera>();
            cam.CreateRenderTexture(out RenderTexture renderTexture, textureFormat, filterMode);
            cam.Render();

            renderTexture.WriteToFile("Assets/Image.png");
        }

        
    }

    public static class Extensions
    {
        public static RenderTexture CreateRenderTexture(this Camera cam, out RenderTexture renderTexture, RenderTextureFormat textureFormat, FilterMode filterMode)
        {
            renderTexture = new RenderTexture(Screen.width, Screen.height, 0, textureFormat, 0);
            renderTexture.filterMode = filterMode;
            cam.targetTexture = renderTexture;
            return renderTexture;
        }
        
        public static void WriteToFile(this RenderTexture renderTexture, string path)
        {
            WriteToFile(renderTexture.ToTexture2D(), path);
        }

        public static void WriteToFile(this Texture2D texture2D, string path)
        {
            byte[] bytes = texture2D.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }

        public static Texture2D ToTexture2D(this RenderTexture renderTexture)
        {
            Texture2D tex = new(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            return tex;
        }
    }
}
