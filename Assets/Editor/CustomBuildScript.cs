using System;
using UMA;
using UnityEditor;
using UnityEngine;

public class CustomBuildScript
{
    // This function can be called to initiate a build
    [MenuItem("Build/Build with UMA for macOS")]
    public static void BuildGameWithUMAForMacOS()
    {
        // Call the UMA-specific function to generate addressables before building
        GenerateUMAAddressables();

        // Define the build options for macOS (Intel 64-bit + Apple Silicon)
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Login.unity", "Assets/Scenes/RoomSelection.unity", "Assets/Scenes/JoinARoom.unity", "Assets/Scenes/CreateRoom.unity", "Assets/Scenes/Lobby.unity", "Assets/Scenes/PokerRoom/PokerRoom.unity" },  // Add your scenes here
            locationPathName = "Builds/MyGame_Mac.app",  // Output path (macOS app bundle)
            target = BuildTarget.StandaloneOSX,  // Target macOS
            options = BuildOptions.None
        };

        // Set the architecture for both Intel 64-bit and Apple Silicon
        PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, 2);  // Universal architecture (Intel 64-bit + Apple Silicon)

        // Run the build process
        BuildPipeline.BuildPlayer(buildOptions);

        // Call the UMA-specific function for post-build material update
        UMAPostBuildMaterialUpdate();
    }

    // Call this right before you build your bundles!
    public static void GenerateUMAAddressables()
    {
        // Clear the index, rebuild the type arrays, and then query the project for the indexed types
        // Add everything to the index except the text assets
        Debug.Log("Rebuilding asset index.");
        UMAAssetIndexer assetIndex = UMAAssetIndexer.Instance;
        try
        {
            // Ensure everything is clean and tidy
            assetIndex.PrepareBuild();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        Debug.Log($"Generating UMA addressable labels.");
        // Generate all UMA addressable labels by recipe. Every recipe gets a unique label
        // to demand load the necessary bundles
        UMAAddressablesSupport.Instance.GenerateAddressables(new SingleGroupGenerator
        {
            ClearMaterials = true
        });

        // Ensure the global library has references to every item not addressable
        Debug.Log($"Adding UMA resource references.");
        assetIndex.AddReferences();
    }

    // Call this right after your bundles have completed building!
    /// <summary>
    /// This will reset the materials on the assets by looking up the materials in the library.
    /// This needs to happen after the bundles are built.
    /// </summary>
    public static void UMAPostBuildMaterialUpdate()
    {
        Debug.Log($"PostProcessBuild - Adding UMA resource references");
        try
        {
            UMAAssetIndexer.Instance.PostBuildMaterialFixup();
        }
        catch (Exception ex)
        {
            Debug.Log($"PostProcessBuild - Adding UMA resource references failed with exception {ex.Message}");
        }
    }
}
