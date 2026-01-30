using System.IO; // provides file writing utilities for saving pngs
using UnityEditor; 
using UnityEngine; 

public class FlatEarthProjectorWindow : EditorWindow // creates a custom editor window tool
{
    private Texture2D sourceEquirect; // the input texture in equirectangular projection (lon-lat rectangle)
    private int outputSize = 2048; // width and height of output square texture in pixels
    private float longitudeOffsetDegrees = 0f; // rotates the disk around its center to choose which longitude points "up"
    private bool transparentOutsideDisk = true; // if true, pixels outside disk become alpha 0 instead of black
    private string outputAssetPath = "Assets/Textures/earth_flat_azimuthal.png"; // where the png will be written

    [MenuItem("Tools/Flat Earth/Project Equirectangular To Disk")] // adds menu item to open this window
    public static void OpenWindow() // called by the menu item above
    {
        GetWindow<FlatEarthProjectorWindow>("Flat Earth Projector"); // creates/focuses the window with a title
    }

    private void OnGUI() // draws the ui every time the window repaints
    {
        EditorGUILayout.LabelField("input", EditorStyles.boldLabel); // section label in the window
        sourceEquirect = (Texture2D)EditorGUILayout.ObjectField("source (equirect)", sourceEquirect, typeof(Texture2D), false); // pick input texture asset

        EditorGUILayout.Space(8f); // add a small gap for readability

        EditorGUILayout.LabelField("output", EditorStyles.boldLabel); // section label in the window
        outputSize = EditorGUILayout.IntField("output size", outputSize); // user chooses output resolution
        longitudeOffsetDegrees = EditorGUILayout.Slider("longitude offset", longitudeOffsetDegrees, -180f, 180f); // user rotates map orientation
        transparentOutsideDisk = EditorGUILayout.Toggle("transparent outside", transparentOutsideDisk); // user chooses outside-disk alpha behavior
        outputAssetPath = EditorGUILayout.TextField("save path", outputAssetPath); // user can change where png is saved

        EditorGUILayout.Space(12f); // add a larger gap before the action button

        using (new EditorGUI.DisabledScope(sourceEquirect == null)) // disables the button until a texture is assigned
        {
            if (GUILayout.Button("generate flat-earth disk texture")) // main action button
            {
                GenerateAndSave(); // run the projection and save the png
            }
        }

        EditorGUILayout.HelpBox( // shows a helpful note at the bottom
            "required: enable 'read/write' on the source texture. recommended: clamp wrap mode, no compression.", // guidance text
            MessageType.Info // info styling
        ); // end help box call
    }

    private void GenerateAndSave() // performs reprojection and writes png to disk
    {
        if (sourceEquirect == null) return; // safety: nothing to do if input is missing

        outputSize = Mathf.Clamp(outputSize, 256, 8192); // clamp to reasonable sizes so memory usage doesn't explode
        float half = outputSize * 0.5f; // precompute half-size for mapping pixels to centered coordinates
        float radius = half - 1f; // disk radius in pixels, subtracting 1 avoids sampling right on the edge
        float lonOffsetRad = longitudeOffsetDegrees * Mathf.Deg2Rad; // convert longitude rotation from degrees to radians

        Texture2D output = new Texture2D(outputSize, outputSize, TextureFormat.RGBA32, false, false); // create output texture with alpha channel
        output.name = "earth_flat_azimuthal_generated"; // name for debugging and clarity in memory

        for (int y = 0; y < outputSize; y++) // loop over each row of output pixels
        {
            for (int x = 0; x < outputSize; x++) // loop over each column of output pixels
            {
                float dx = (x + 0.5f) - half; // pixel-center x relative to texture center
                float dy = (y + 0.5f) - half; // pixel-center y relative to texture center
                float r = Mathf.Sqrt(dx * dx + dy * dy); // radial distance from center of disk in pixels

                if (r > radius) // if pixel is outside the circular disk
                {
                    Color outside = transparentOutsideDisk ? new Color(0f, 0f, 0f, 0f) : Color.black; // choose transparent or black background
                    output.SetPixel(x, y, outside); // write background pixel
                    continue; // skip projection math for outside pixels
                }

                float rho = r / radius; // normalize radius to 0..1 where 1 is the disk edge
                float c = rho * Mathf.PI; // angular distance from the north pole in radians (0 center -> pi at rim)

                float latRad = (Mathf.PI * 0.5f) - c; // latitude in radians: north pole at center, decreasing toward rim
                float theta = Mathf.Atan2(dx, dy); // angle around the disk; dy is "up" so longitude 0 points upward
                float lonRad = theta + lonOffsetRad; // add user rotation so you can choose which longitude is at the top

                float u = (lonRad / (2f * Mathf.PI)) + 0.5f; // map longitude -pi..pi to u 0..1
                u = u - Mathf.Floor(u); // wrap u into 0..1 so it repeats cleanly across the 180 seam

                float v = (latRad / Mathf.PI) + 0.5f; // map latitude -pi/2..pi/2 into v 0..1
                v = Mathf.Clamp01(v); // clamp v since latitude should not wrap beyond poles

                Color sample = sourceEquirect.GetPixelBilinear(u, v); // sample input texture using bilinear filtering for smooth output
                output.SetPixel(x, y, sample); // write the sampled color into the disk texture
            }
        }

        output.Apply(false, false); // upload pixels to gpu copy without generating mipmaps and keep it readable

        byte[] pngBytes = output.EncodeToPNG(); // encode texture pixels to png file bytes
        if (pngBytes == null || pngBytes.Length == 0) // sanity check in case encoding fails
        {
            Debug.LogError("png encoding failed."); // log error to console
            return; // abort save
        }

        string fullPath = ToAbsoluteProjectPath(outputAssetPath); // convert assets-relative path to absolute filesystem path
        string directory = Path.GetDirectoryName(fullPath); // extract directory portion from the full path
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory); // create folders if they don't exist yet

        File.WriteAllBytes(fullPath, pngBytes); // write the png bytes to disk at the chosen path
        AssetDatabase.Refresh(); // make unity import the newly written png as an asset

        Debug.Log($"generated flat-earth texture: {outputAssetPath}"); // log success with the asset path
    }

    private static string ToAbsoluteProjectPath(string assetPath) // converts "Assets/..." to an absolute path on disk
    {
        assetPath = assetPath.Replace('\\', '/'); // normalize slashes for cross-platform consistency
        if (!assetPath.StartsWith("Assets/")) assetPath = "Assets/" + assetPath.TrimStart('/'); // force assets-relative format if user omitted it
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..")); // compute project root folder from assets folder
        return Path.Combine(projectRoot, assetPath); // join project root with assets-relative path to produce absolute path
    }
}
