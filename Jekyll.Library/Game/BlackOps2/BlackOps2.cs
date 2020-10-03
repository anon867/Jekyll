﻿using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Black Ops II (T6)
    /// </summary>
    public partial class BlackOps2 : IGame
    {
        /// <summary>
        /// Gets the name of Black Ops II.
        /// </summary>
        public string Name => "Black Ops II";

        /// <summary>
        /// Gets the process names of Black Ops II.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "t6sp",
            "t6mp",
            "t6zm"
        };

        /// <summary>
        /// Gets or sets the process index of Black Ops II.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of Black Ops II.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pools address of Black Ops II.
        /// </summary>
        public long XAssetPoolsAddress { get; set; }

        /// <summary>
        /// Gets or sets the XAsset Pool Sizes address of Black Ops II.
        /// </summary>
        public long XAssetPoolSizesAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of Black Ops II.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Pools of Black Ops II.
        /// </summary>
        private enum XAssetPool : int
        {
            xmodelpieces,
            physpreset,
            physconstraints,
            destructibledef,
            xanim,
            xmodel,
            material,
            techset,
            image,
            sound,
            sound_patch,
            col_map_sp,
            col_map_mp,
            com_map,
            game_map_sp,
            game_map_mp,
            map_ents,
            gfx_map,
            lightdef,
            ui_map,
            font,
            fonticon,
            menufile,
            menu,
            localize,
            weapon,
            weapondef,
            weaponvariant,
            weaponfull,
            attachment,
            attachmentunique,
            weaponcamo,
            snddriverglobals,
            fx,
            impactfx,
            aitype,
            mptype,
            mpbody,
            mphead,
            character,
            xmodelalias,
            rawfile,
            stringtable,
            leaderboarddef,
            xGlobals,
            ddl,
            glasses,
            emblemset,
            scriptparsetree,
            keyvaluepairs,
            vehicle,
            memoryblock,
            addon_map_ents,
            tracer,
            skinnedverts,
            qdb,
            slug,
            footsteptable,
            footstepfxtable,
            zbarrier,
            _string,
            assetlist,
            report,
            depend
        }

        /// <summary>
        /// Structure of an Black Ops II XAsset Pool.
        /// </summary>
        public struct XAssetPoolData
        {
            public int PoolEntries { get; set; }
        }

        /// <summary>
        /// Structure of an Black Ops II XAsset Pool Size.
        /// </summary>
        public struct XAssetPoolSizesData
        {
            public int PoolSize { get; set; }
        }

        /// <summary>
        /// Validates and sets the XAsset Pools and XAsset Pool Sizes addresses of Black Ops II.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if addresses are valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();
            long moduleSize = instance.Reader.GetModuleMemorySize();

            long[] scanXAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0x56, 0x51, 0xFF, 0xD2, 0x8B, 0xF0, 0x83, 0xC4, 0x04, 0x85, 0xF6 },
                BaseAddress,
                BaseAddress + moduleSize,
                true);

            if (scanXAssetPools.Length > 0)
            {
                XAssetPoolsAddress = instance.Reader.ReadInt32(scanXAssetPools[0] - 0xB);
                XAssetPoolSizesAddress = instance.Reader.ReadInt32(scanXAssetPools[0] + 0x3B);

                // In Black Ops II, defaultvehicle will always be the first entry in the XModel XAsset Pool.
                if (instance.Reader.ReadNullTerminatedString(instance.Reader.ReadInt32(instance.Reader.ReadInt32(XAssetPoolsAddress + (Marshal.SizeOf<XAssetPoolData>() * (int)XAssetPool.xmodel)) + 4)) == "defaultvehicle")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a shallow copy of the Black Ops II IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}