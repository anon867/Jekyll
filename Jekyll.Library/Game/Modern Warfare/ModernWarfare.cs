using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    /// <summary>
    /// Call of Duty: Modern Warfare
    /// Aliases: IW8, Kronos, Odin
    /// </summary>
    public partial class ModernWarfare : IGame
    {
        /// <summary>
        /// Gets the name of Modern Warfare.
        /// </summary>
        public string Name => "Modern Warfare";

        /// <summary>
        /// Gets the process names of Modern Warfare.
        /// </summary>
        public string[] ProcessNames => new string[]
        {
            "ModernWarfare"
        };

        /// <summary>
        /// Gets or sets the process index of Modern Warfare.
        /// </summary>
        public int ProcessIndex { get; set; }

        /// <summary>
        /// Gets or sets the base address of Modern Warfare.
        /// </summary>
        public long BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the DBAssetPools address of Modern Warfare.
        /// </summary>
        public long DBAssetPools { get; set; }

        /// <summary>
        /// Gets or sets the DBAssetPoolSizes address of Modern Warfare.
        /// Not used for this title, instead, it is stored in DBAssetPool.
        /// </summary>
        public long DBAssetPoolSizes { get; set; }

        /// <summary>
        /// Gets or sets the list of XAsset Pools of Modern Warfare.
        /// </summary>
        public List<IXAssetPool> XAssetPools { get; set; }

        /// <summary>
        /// XAsset Types of Modern Warfare.
        /// </summary>
        private enum XAssetType : int
        {
            physicslibrary,
            physicssfxeventasset,
            physicsvfxeventasset,
            physicsasset,
            physicsfxpipeline,
            physicsfxshape,
            physicsdebugdata,
            xanim,
            xmodelsurfs,
            xmodel = 9,
            mayhem,
            material,
            computeshader,
            libshader,
            vertexshader,
            hullshader,
            domainshader,
            pixelshader,
            techset,
            image = 19,
            soundglobals = 21,
            soundbank,
            soundbanktransient,
            col_map,
            com_map,
            glass_map,
            aipaths,
            navmesh,
            tacgraph,
            map_ents = 29,
            fx_map,
            gfx_map = 32,
            gfx_map_trzone = 32,
            iesprofile,
            lightdef = 34,
            gradingclut,
            ui_map,
            fogspline,
            animclass,
            playeranim,
            localize,
            attachment,
            weapon = 43,
            impactfx,
            surfacefx,
            aitype,
            mptype,
            character,
            xmodelalias,
            rawfile = 51,
            scriptfile = 52,
            scriptdebugdata,
            stringtable = 54,
            leaderboarddef,
            virtualleaderboarddef,
            ddl = 57,
            tracer,
            vehicle,
            addon_map_ents,
            netconststrings,
            luafile = 62,
            scriptable,
            equipsndtable,
            vectorfield,
            particlesimanimation,
            streaminginfo,
            laser,
            ttf = 69,
            suit,
            suitanimpackage,
            camera,
            hudoutline,
            spaceshiptarget,
            rumble,
            rumblegraph,
            animpkg,
            sfxpkg,
            vfxpkg,
            footstepvfx,
            behaviortree,
            aianimset,
            aiasm,
            proceduralbones,
            dynamicbones,
            reticle,
            xanimcurve,
            coverselector,
            enemyselector,
            clientcharacter,
            clothasset,
            cinematicmotion,
            locdmgtable,
            bulletpenetration,
            scriptbundle,
            blendspace2d,
            xcam,
            camo,
            xcompositemodel,
            xmodeldetailcollision,
            streamkey,
            streamtreeoverride,
            keyvaluepairs,
            stterrain,
            nativescriptpatch,
            carryobject,
            soundbanklist,
            decalvolumematerial,
            decalvolumemask,
            fx_map_trzone,
            dlogschema,
            edgelist,
            defaultdummy,
            dummy
        }

        /// <summary>
        /// Structure of a Modern Warfare XAsset Pool.
        /// </summary>
        public struct DBAssetPool
        {
            public long Entries { get; set; }
            public long FreeHead { get; set; }
            public uint PoolSize { get; set; }
            public uint ElementSize { get; set; }
        }

        /// <summary>
        /// Validates and sets the DBAssetPools address of Modern Warfare.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if address is valid, otherwise false.</returns>
        public bool InitializeGame(JekyllInstance instance)
        {
            BaseAddress = instance.Reader.GetBaseAddress();

            var scanDBAssetPools = instance.Reader.FindBytes(
                new byte?[] { 0x48, 0x8D, 0x04, 0x40, 0x4C, 0x8D, 0x8E, null, null, null, null, 0x4D, 0x8D, 0x0C, 0xC1, 0x8D, 0x42, 0xFF },
                BaseAddress,
                BaseAddress + instance.Reader.GetModuleMemorySize(),
                true);

            if (scanDBAssetPools.Length > 0)
            {
                DBAssetPools = instance.Reader.ReadInt32(scanDBAssetPools[0] + 0x7);

                // In Modern Warfare, axis_guide_createfx will always be the first entry in the XModel XAsset Pool.
                if (GetFirstXModel(instance) == "axis_guide_createfx")
                {
                    List<Dictionary<string, object>> pools = new List<Dictionary<string, object>>();

                    foreach (int index in Enum.GetValues(typeof(XAssetType)))
                    {
                        DBAssetPool pool = instance.Reader.ReadStruct<DBAssetPool>(instance.Game.BaseAddress + instance.Game.DBAssetPools + (index * Marshal.SizeOf<DBAssetPool>()));

                        pools.Add(new Dictionary<string, object>() {
                            { "Name", Enum.GetName(typeof(XAssetType), index) },
                            { "ElementSize", pool.ElementSize },
                        });
                    }

                    string path = Path.Combine(instance.ExportPath, "DBAssetPools.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (StreamWriter file = File.CreateText(path))
                    {
                        file.Write(JsonConvert.SerializeObject(pools, Formatting.Indented));
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the first entry in the XModel XAsset Pool of Modern Warfare.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>Name of the XModel.</returns>
        public string GetFirstXModel(JekyllInstance instance)
        {
            long address = BaseAddress + DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * (int)XAssetType.xmodel);
            DBAssetPool pool = instance.Reader.ReadStruct<DBAssetPool>(address);
            long name = instance.Reader.ReadInt64(pool.Entries);

            return instance.Reader.ReadNullTerminatedString(name);
        }
        /// <summary>
        /// Creates a shallow copy of the Modern Warfare IGame object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
