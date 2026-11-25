namespace CassieFix.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using Cassie;
    using CassieFix.Services;
    using HarmonyLib;
    using Mirror;

    /// <summary>
    /// Changes only the text written to clients. Server playback and subtitle data remain untouched.
    /// </summary>
    public static class CassiePayloadWriterPatch
    {
        /// <summary>
        /// Inserts padding immediately before the CASSIE content is serialized.
        /// </summary>
        /// <param name="instructions">Original serializer instructions.</param>
        /// <returns>Serializer instructions with the padding call.</returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo contentField = AccessTools.Field(typeof(CassieTtsPayload), nameof(CassieTtsPayload.Content));
            MethodInfo writeString = AccessTools.Method(
                typeof(NetworkWriterExtensions),
                nameof(NetworkWriterExtensions.WriteString),
                new[] { typeof(NetworkWriter), typeof(string) });
            MethodInfo pad = AccessTools.Method(typeof(MessagePadding), nameof(MessagePadding.ForClient));

            if (contentField == null || writeString == null || pad == null)
            {
                throw new MissingMemberException("CassieFix could not resolve the CASSIE serializer members.");
            }

            var result = new List<CodeInstruction>(instructions);
            int matchCount = 0;

            for (int index = 0; index < result.Count - 1; index++)
            {
                if (result[index].opcode != OpCodes.Ldfld || !Equals(result[index].operand, contentField))
                {
                    continue;
                }

                if (result[index + 1].opcode != OpCodes.Call || !Equals(result[index + 1].operand, writeString))
                {
                    continue;
                }

                result.Insert(index + 1, new CodeInstruction(OpCodes.Call, pad));
                matchCount++;
                index++;
            }

            if (matchCount != 1)
            {
                throw new InvalidOperationException(
                    $"CassieFix expected one CASSIE content write, but found {matchCount}. The game version may have changed.");
            }

            return result;
        }
    }
}
