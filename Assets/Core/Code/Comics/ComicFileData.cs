using BeauUtil;
using BeauUtil.Debugger;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SupplyChain.Comics {
    /**
     * Compressing a comic sequence into a single file
     * 
     * File sections:
     * Textures (int count, ComicTextureData[]) // 4 + 16 * count bytes (count is max 2^16)
     * Colors (int count, Color32[]) // 4 + 4 * count bytes (count is max 2^8)
     * Layers (int count, ComicLayerData[]) // 4 + 20 * count bytes (count is max 2^16)
     * Panels (int count, ComicPanelData[]) // 4 + 8 * count bytes (count is max 2^16)
     * Pages (int count, ComicPageData[]) // 4 + 12 * count bytes (count is max 2^16)
     * Binary (byte[]) // bytes (separate file)
     * 
     * Binary might be much MUCH larger than the metadata
     * Consider splitting into its own file? Or caching it in browser
     * 
     * Metadata in one file (tiny, 5-10kb)
     * Texture blob in another (large, 10-20mb)
     * Hopefully LZ compression will help eat up the empty space on the R8 and BC4 textures
     * 
     * Allocate fixed block of memory for texture blob data? ~24mb
     * Determine how many textures of each format need to be loaded at a time
     * Allocate textures up-front for each slot
     * Overwrite unused slots as necessary to display current page
     * Try to avoid overwriting textures that will be needed for future pages
     * 
     * Allocate fixed block of memory for metadata file (16kb is plenty, load file to upper portion, write runtime data to lower)
     **/

    [Flags]
    public enum ComicFileFlags : ushort {
        None = 0
    }

    [StructLayout(LayoutKind.Sequential)] // 32 bytes
    public struct ComicFileHeader {
        public long LastEditTime;
        public ulong FileHash;
        public ushort Version;
        public ComicFileFlags Flags;
        public ushort TextureCount;
        public ushort LayerCount;
        public ushort PanelCount;
        public ushort PageCount;
        public byte ColorCount;
        public uint BinaryFileSize;
    }

    [StructLayout(LayoutKind.Sequential)] // 16 bytes
    public struct ComicTextureData {
        public ushort Width;
        public ushort Height;
        public ComicTextureFlags Flags;
        public byte BlockSize; // width+height of region blocks
        public OffsetLengthU32 Data;
    }

    [Flags]
    public enum ComicTextureFlags : byte {
        ChannelsR = 0x01, // R channel only
        ChannelsRGB = 0x02, // RGB channels
        __Unused2 = 0x04, // Unused - RGBA is implicit by absense of ChannelsR and ChannelsRGB
        DataIsBC = 0x08, // Is BC data
        DataIsPNG = 0x10, // Is PNG data
        DataIsJPEG = 0x20, // Is JPEG data
        __Unused = 0x40, // unused flag
        DataIsLZCompressed = 0x80, // Data is LZ-Compressed
    }

    public enum ComicTextureFormat : byte {
        R8,
        BC4,
        RGB24,
        BC1,
        RGBA32,
        BC3
    }

    [StructLayout(LayoutKind.Sequential)] // 8 bytes
    public struct ComicPanelData {
        public StringHash32 IdHash;
        public OffsetLengthU16 Layers;
    }

    [Flags]
    public enum ComicPageFlags : byte {
        None = 0,
    }

    [StructLayout(LayoutKind.Sequential)] // 12 bytes
    public struct ComicPageData {
        public ComicPageFlags Flags;
        public byte BackgroundColorIndexA;
        public ushort TotalWidth;
        public ushort TotalHeight;
        public ushort UsedTextureMask;
        public OffsetLengthU16 Panels;
    }

    public enum ComicLayerFlags : byte {
        UseIntensityShader = 0x01
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ComicLayerData { // 20 bytes
        public ushort TextureIndex;
        public byte TextureRegionBlockX;
        public byte TextureRegionBlockY;
        public ushort TextureRegionSizeX;
        public ushort TextureRegionSizeY;
        public ushort OffsetX;
        public ushort OffsetY;
        public ushort SizeX;
        public ushort SizeY;
        public ushort SiblingIndex;
        public ComicLayerFlags Flags;
        public byte TintIndex;
    }

    static public partial class ComicFileUtility {
        static public ComicTextureFormat ConvertToComicTextureFormat(ComicTextureFlags flags) {
            bool isCompressed = (flags & (ComicTextureFlags.DataIsJPEG | ComicTextureFlags.DataIsBC)) != 0;
            if ((flags & ComicTextureFlags.ChannelsR) != 0) {
                return isCompressed ? ComicTextureFormat.BC4 : ComicTextureFormat.R8;
            }
            if ((flags & ComicTextureFlags.ChannelsRGB) != 0) {
                return isCompressed ? ComicTextureFormat.BC1 : ComicTextureFormat.RGB24;
            }
            return isCompressed ? ComicTextureFormat.BC3 : ComicTextureFormat.RGBA32;
        }

        static public TextureFormat ConvertToTextureFormat(ComicTextureFormat format) {
            Assert.True(format >= ComicTextureFormat.R8 && format <= ComicTextureFormat.BC3, "Unknown format {0}", format);
            switch(format) {
                case ComicTextureFormat.R8:
                    return TextureFormat.R8;
                case ComicTextureFormat.BC4:
                    return TextureFormat.BC4;
                case ComicTextureFormat.RGB24:
                    return TextureFormat.RGB24;
                case ComicTextureFormat.BC1:
                    return TextureFormat.DXT1;
                case ComicTextureFormat.RGBA32:
                    return TextureFormat.RGBA32;
                default:
                    return TextureFormat.DXT5;
            }
        }
    }
}