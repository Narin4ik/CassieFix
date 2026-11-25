namespace CassieFix
{
    using System;
    using Cassie;
    using CassieFix.Patches;
    using Exiled.API.Features;
    using HarmonyLib;

    /// <summary>
    /// Applies the server-side CASSIE payload fix.
    /// </summary>
    public sealed class CassieFixPlugin : Plugin<Config>
    {
        private const string HarmonyId = "com.narin.cassiefix";

        private Harmony? harmony;

        /// <inheritdoc />
        public override string Name => "CassieFix";

        /// <inheritdoc />
        public override string Author => "Narin";

        /// <inheritdoc />
        public override Version Version => new Version(1, 0, 0);

        /// <inheritdoc />
        public override Version RequiredExiledVersion => new Version(9, 15, 3);

        /// <inheritdoc />
        public override void OnEnabled()
        {
            var target = AccessTools.Method(typeof(CassieTtsPayload), nameof(CassieTtsPayload.WriteSelf));
            var transpiler = AccessTools.Method(typeof(CassiePayloadWriterPatch), nameof(CassiePayloadWriterPatch.Transpiler));

            if (target == null || transpiler == null)
            {
                throw new MissingMethodException("CassieFix could not find the CASSIE serializer or its patch.");
            }

            harmony = new Harmony(HarmonyId);

            try
            {
                harmony.Patch(target, transpiler: new HarmonyMethod(transpiler));
            }
            catch (Exception exception)
            {
                harmony.UnpatchAll(HarmonyId);
                harmony = null;
                Log.Error($"CassieFix could not patch CASSIE payload serialization: {exception}");
                throw;
            }

            base.OnEnabled();
        }

        /// <inheritdoc />
        public override void OnDisabled()
        {
            harmony?.UnpatchAll(HarmonyId);
            harmony = null;

            base.OnDisabled();
        }
    }
}
