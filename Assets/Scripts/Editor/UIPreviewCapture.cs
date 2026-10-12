using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OneHealth.EditorTools
{
    /// <summary>Renders all overlay canvases in the open scene to a PNG at a fixed resolution (for UI review).</summary>
    public static class UIPreviewCapture
    {
        public static string Capture(string path, int width = 1920, int height = 1080)
        {
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var camGo = new GameObject("~PreviewCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            var main = Camera.main;
            if (main != null)
            {
                // Mirror the scene camera so backgrounds and portraits render behind the UI.
                cam.CopyFrom(main);
                camGo.transform.SetPositionAndRotation(main.transform.position, main.transform.rotation);
            }
            cam.targetTexture = rt;
            cam.cullingMask = ~0;

            var changed = new List<Canvas>();
            foreach (var c in Object.FindObjectsByType<Canvas>())
            {
                if (!c.isRootCanvas || c.renderMode != RenderMode.ScreenSpaceOverlay || !c.gameObject.activeInHierarchy) continue;
                c.renderMode = RenderMode.ScreenSpaceCamera;
                c.worldCamera = cam;
                c.planeDistance = 1f;   // in front of world sprites; canvases sort by sortingOrder
                changed.Add(c);
            }
            Canvas.ForceUpdateCanvases();
            cam.Render();

            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;

            foreach (var c in changed) { c.renderMode = RenderMode.ScreenSpaceOverlay; c.worldCamera = null; }
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(camGo);
            rt.Release();
            Object.DestroyImmediate(rt);
            return path;
        }
    }
}
