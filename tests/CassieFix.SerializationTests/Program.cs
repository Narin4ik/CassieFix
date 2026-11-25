namespace CassieFix.SerializationTests
{
    using System;
    using System.Reflection;
    using Cassie;
    using CassieFix.Patches;
    using HarmonyLib;
    using Mirror;

    internal static class Program
    {
        private const string TestHarmonyId = "com.narin.cassiefix.serialization-tests";

        private static int Main()
        {
            try
            {
                CheckSerializerPatch();
                CheckPluginLifecycle();

                Console.WriteLine("All CASSIE serialization checks passed.");
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                return 1;
            }
        }

        private static void CheckSerializerPatch()
        {
            var harmony = new Harmony(TestHarmonyId);

            try
            {
                MethodInfo target = AccessTools.Method(typeof(CassieTtsPayload), nameof(CassieTtsPayload.WriteSelf));
                MethodInfo transpiler = AccessTools.Method(
                    typeof(CassiePayloadWriterPatch),
                    nameof(CassiePayloadWriterPatch.Transpiler));

                harmony.Patch(target, transpiler: new HarmonyMethod(transpiler));

                CheckContent(string.Empty, string.Empty);
                CheckContent("A", "A".PadRight(1024));
                CheckContent(new string('A', 1023), new string('A', 1023) + " ");
                CheckContent(new string('A', 1024), new string('A', 1024));
                CheckContent(new string('A', 1025), new string('A', 1025));
                CheckCustomSubtitles();
            }
            finally
            {
                harmony.UnpatchAll(TestHarmonyId);
            }
        }

        private static void CheckPluginLifecycle()
        {
            var plugin = new CassieFixPlugin();
            plugin.OnEnabled();

            try
            {
                CheckContent("SCP 1 7 3", "SCP 1 7 3".PadRight(1024));
            }
            finally
            {
                plugin.OnDisabled();
            }

            CheckContent("SCP 1 7 3", "SCP 1 7 3");
        }

        private static void CheckContent(string original, string expectedOnWire)
        {
            var payload = new CassieTtsPayload(original, false, false);
            CassieTtsPayload copy = RoundTrip(payload);

            Assert(payload.Content == original, "The server payload was modified.");
            Assert(copy.Content == expectedOnWire, $"Unexpected serialized content length for {original.Length} characters.");
            Assert(copy.SubtitleSource == payload.SubtitleSource, "Subtitle mode changed.");
            Assert(copy.PlayBackground == payload.PlayBackground, "Background playback flag changed.");
        }

        private static void CheckCustomSubtitles()
        {
            var payload = new CassieTtsPayload("SCP 1 7 3", "Visible subtitle", true);
            CassieTtsPayload copy = RoundTrip(payload);
            FieldInfo subtitleField = typeof(CassieTtsPayload).GetField(
                "_customSubtitle",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert(copy.Content == "SCP 1 7 3".PadRight(1024), "Custom subtitle payload was not padded.");
            Assert(copy.SubtitleSource == payload.SubtitleSource, "Custom subtitle mode changed.");
            Assert(copy.PlayBackground == payload.PlayBackground, "Background playback flag changed.");
            Assert((string)subtitleField.GetValue(copy) == "Visible subtitle", "Custom subtitle text changed.");
        }

        private static CassieTtsPayload RoundTrip(CassieTtsPayload payload)
        {
            var writer = new NetworkWriter();
            payload.WriteSelf(writer);

            var reader = new NetworkReader(writer.ToArraySegment());
            return new CassieTtsPayload(reader);
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
