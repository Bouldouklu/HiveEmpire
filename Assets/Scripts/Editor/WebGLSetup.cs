using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class WebGLSetup : EditorWindow
{
    [MenuItem("Tools/WebGL/Apply Full WebGL Setup")]
    public static void ApplyWebGLSetup()
    {
        // Bake all lights
        var allLights = GameObject.FindObjectsOfType<Light>();
        int bakedCount = 0;

        foreach (var light in allLights)
        {
            if (light.type == LightType.Directional || light.type == LightType.Point || light.type == LightType.Spot)
            {
                light.lightmapBakeType = LightmapBakeType.Baked;
                bakedCount++;
            }
        }

        // Make static geometry static
        var renderers = GameObject.FindObjectsOfType<Renderer>();
        int staticCount = 0;

        foreach (var r in renderers)
        {
            var go = r.gameObject;

            if (go.tag != "Bee") // don't make bees static
            {
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.ContributeGI);
                staticCount++;
            }
        }

        // Setup Bee units to use Light Probes and no shadows
        var bees = GameObject.FindGameObjectsWithTag("Bee");
        int beeCount = bees.Length;

        foreach (var bee in bees)
        {
            var r = bee.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                r.receiveShadows = false;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
                r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
            }
        }

        // Save scene
        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();

        Debug.Log(
            "✅ WebGL Setup Applied\n" +
            $"• Lights set to Baked: {bakedCount}\n" +
            $"• Static Lightmap Objects: {staticCount}\n" +
            $"• Bee Objects Updated: {beeCount}\n\n" +
            "⚠️ Now go to: Project Settings → Player → WebGL\n" +
            "• Compression: Brotli\n" +
            "• Decompression Fallback: Enabled\n" +
            "• Texture Compression: ASTC (If supported) or High Quality\n" +
            "• Color Space: Linear\n"
        );
    }
}
