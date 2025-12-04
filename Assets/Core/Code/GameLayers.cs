using System;


static public class LayerMasks {
    
    // Layer 0: Default
    public const int Default_Index = 0;
    public const int Default_Mask = 1;
    // Layer 1: TransparentFX
    public const int TransparentFX_Index = 1;
    public const int TransparentFX_Mask = 2;
    // Layer 2: Ignore Raycast
    public const int IgnoreRaycast_Index = 2;
    public const int IgnoreRaycast_Mask = 4;
    // Layer 4: Water
    public const int Water_Index = 4;
    public const int Water_Mask = 16;
    // Layer 5: UI
    public const int UI_Index = 5;
    public const int UI_Mask = 32;
    // Layer 6: Grid Base
    public const int GridBase_Index = 6;
    public const int GridBase_Mask = 64;
    // Layer 7: Nodes
    public const int Nodes_Index = 7;
    public const int Nodes_Mask = 128;
    // Layer 8: Links
    public const int Links_Index = 8;
    public const int Links_Mask = 256;
    // Layer 9: FloorplanDrag
    public const int FloorplanDrag_Index = 9;
    public const int FloorplanDrag_Mask = 512;
    // Layer 10: FloorLinkNodes
    public const int FloorLinkNodes_Index = 10;
    public const int FloorLinkNodes_Mask = 1024;
    // Layer 11: FloorNodes
    public const int FloorNodes_Index = 11;
    public const int FloorNodes_Mask = 2048;
    // Layer 12: ChipFabDrag
    public const int ChipFabDrag_Index = 12;
    public const int ChipFabDrag_Mask = 4096;
    // Layer 13: ChipFabClickBox
    public const int ChipFabClickBox_Index = 13;
    public const int ChipFabClickBox_Mask = 8192;
    // Layer 14: ChipFabEtchProjectile
    public const int ChipFabEtchProjectile_Index = 14;
    public const int ChipFabEtchProjectile_Mask = 16384;
    // Layer 15: ChipFabSputterProjectile
    public const int ChipFabSputterProjectile_Index = 15;
    public const int ChipFabSputterProjectile_Mask = 32768;
    // Layer 16: ChipFabSemi
    public const int ChipFabSemi_Index = 16;
    public const int ChipFabSemi_Mask = 65536;
    // Layer 17: ChipFabOxide-Metal
    public const int ChipFabOxide_Metal_Index = 17;
    public const int ChipFabOxide_Metal_Mask = 131072;
    // Layer 18: ChipFabResist
    public const int ChipFabResist_Index = 18;
    public const int ChipFabResist_Mask = 262144;
    // Layer 27: ResearchSlot
    public const int ResearchSlot_Index = 27;
    public const int ResearchSlot_Mask = 134217728;
    // Layer 28: ResearchGem
    public const int ResearchGem_Index = 28;
    public const int ResearchGem_Mask = 268435456;
    // Layer 29: SupplyRoute
    public const int SupplyRoute_Index = 29;
    public const int SupplyRoute_Mask = 536870912;
    // Layer 30: SupplyRegion
    public const int SupplyRegion_Index = 30;
    public const int SupplyRegion_Mask = 1073741824;
    // Layer 31: SupplyNode
    public const int SupplyNode_Index = 31;
    public const int SupplyNode_Mask = -2147483648;
}
static public class SortingLayers {
    
    // Layer Background
    public const int Background = -1734242873;
    // Layer Default
    public const int Default = 0;
    // Layer Grid Base
    public const int GridBase = 1979389103;
    // Layer Grid Nodes
    public const int GridNodes = 1845702645;
    // Layer Grid Overlay
    public const int GridOverlay = 114194533;
    // Layer UI
    public const int UI = 1132335069;
    // Layer ChipFab Microgame
    public const int ChipFabMicrogame = 1897548539;
}
static public class UnityTags {
    
    // Tag Untagged
    public const string Untagged = "Untagged";
    // Tag Respawn
    public const string Respawn = "Respawn";
    // Tag Finish
    public const string Finish = "Finish";
    // Tag EditorOnly
    public const string EditorOnly = "EditorOnly";
    // Tag MainCamera
    public const string MainCamera = "MainCamera";
    // Tag Player
    public const string Player = "Player";
    // Tag GameController
    public const string GameController = "GameController";
}