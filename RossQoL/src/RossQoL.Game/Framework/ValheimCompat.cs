using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace RossQoL.Game.Framework
{
    /// <summary>
    /// Checks the Valheim members each feature reaches for by NAME rather than
    /// by compiler-checked reference.
    ///
    /// Those fail differently. A publicized member read is bound at compile
    /// time: if Valheim removes it, the plugin fails to load with the member's
    /// name in the message. A Harmony patch whose target has been renamed
    /// fails QUIETLY -- the patch is skipped and the feature simply stops
    /// working. This exists so the log names the feature and the member first.
    /// </summary>
    internal static class ValheimCompat
    {
        public static List<string> FindMissing(IEnumerable<CompatMember> members)
        {
            var missing = new List<string>();
            if (members == null) return missing;

            var list = members as IReadOnlyList<CompatMember> ?? new List<CompatMember>(members);
            for (int i = 0; i < list.Count; i++)
            {
                var m = list[i];
                var type = AccessTools.TypeByName(m.Type);
                if (type == null)
                {
                    missing.Add($"{m.Type} (whole type) -- {m.Why}");
                    continue;
                }

                if (!HasMember(type, m.Member))
                    missing.Add($"{m.Type}.{m.Member} -- {m.Why}");
            }

            return missing;
        }

        /// <summary>
        /// Plain reflection rather than AccessTools: AccessTools writes a
        /// warning to the console every time a lookup misses, and this check
        /// misses on purpose -- it asks "field? property? event? method?" in
        /// turn, so every method it confirms cost two warnings. Reflection
        /// returns null quietly.
        ///
        /// Walks the base types itself, which is the part AccessTools was
        /// doing for us: private members are not inherited by GetField.
        /// </summary>
        private static bool HasMember(Type type, string name)
        {
            for (var t = type; t != null; t = t.BaseType)
            {
                if (t.GetField(name, AccessTools.all) != null) return true;
                if (t.GetProperty(name, AccessTools.all) != null) return true;
                if (t.GetEvent(name, AccessTools.all) != null) return true;

                try
                {
                    if (t.GetMethod(name, AccessTools.all) != null) return true;
                }
                catch (AmbiguousMatchException)
                {
                    // Overloaded: it exists, which is all this check asks.
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Harmony Prepare helper. A Prepare that just returns
        /// `AccessTools.Method(...) != null` skips its patch in total silence.
        /// </summary>
        public static bool RequireMethod(Type type, string method, string featureName)
        {
            if (HasMethod(type, method)) return true;

            RossQoLPlugin.Log.LogError(
                $"{type.Name}.{method} not found -- skipping that patch; {featureName} will not work. "
                + "See the compatibility check above.");
            return false;
        }

        /// <summary>
        /// Does this type still have a method by that name, overloads and all?
        /// Plain reflection rather than AccessTools: AccessTools logs on a
        /// miss, and asking it for an overloaded name -- Player.HaveRequirements,
        /// say -- throws rather than answering.
        /// </summary>
        private static bool HasMethod(Type type, string name)
        {
            for (var t = type; t != null; t = t.BaseType)
            {
                var methods = t.GetMethods(AccessTools.all);
                for (int i = 0; i < methods.Length; i++)
                    if (methods[i].Name == name) return true;
            }

            return false;
        }
    }

    internal static class EnumText
    {
        private static class Cache<T> where T : struct, Enum
        {
            public static readonly Dictionary<T, string> Names = Build();

            private static Dictionary<T, string> Build()
            {
                var values = (T[])Enum.GetValues(typeof(T));
                var names = Enum.GetNames(typeof(T));
                var map = new Dictionary<T, string>(values.Length);
                for (int i = 0; i < values.Length && i < names.Length; i++)
                    map[values[i]] = names[i];
                return map;
            }
        }

        public static string Name<T>(T value) where T : struct, Enum =>
            Cache<T>.Names.TryGetValue(value, out var name) ? name : string.Empty;
    }

    internal static class RpcSend
    {
        public static void ToOwner(ZNetView nview, string methodName, int arg0)
        {
            var rpc = ZRoutedRpc.instance;
            var zdo = nview.GetZDO();
            var data = Create(rpc, zdo.GetOwner(), zdo.m_uid, methodName);
            data.m_parameters.Write(arg0);
            Dispatch(rpc, data);
        }

        public static void ToOwner(ZNetView nview, string methodName, int arg0, bool arg1)
        {
            var rpc = ZRoutedRpc.instance;
            var zdo = nview.GetZDO();
            var data = Create(rpc, zdo.GetOwner(), zdo.m_uid, methodName);
            data.m_parameters.Write(arg0);
            data.m_parameters.Write(arg1);
            Dispatch(rpc, data);
        }

        public static void ToOwner(ZNetView nview, string methodName, string arg0, bool arg1)
        {
            var rpc = ZRoutedRpc.instance;
            var zdo = nview.GetZDO();
            var data = Create(rpc, zdo.GetOwner(), zdo.m_uid, methodName);
            data.m_parameters.Write(arg0 ?? "");
            data.m_parameters.Write(arg1);
            Dispatch(rpc, data);
        }

        private static ZRoutedRpc.RoutedRPCData Create(ZRoutedRpc rpc, long targetPeerId, ZDOID targetZdo, string methodName) =>
            new ZRoutedRpc.RoutedRPCData
            {
                m_msgID = rpc.m_id + rpc.m_rpcMsgID++,
                m_senderPeerID = rpc.m_id,
                m_targetPeerID = targetPeerId,
                m_targetZDO = targetZdo,
                m_methodHash = methodName.GetStableHashCode(),
            };

        private static void Dispatch(ZRoutedRpc rpc, ZRoutedRpc.RoutedRPCData data)
        {
            data.m_parameters.SetPos(0);
            if (data.m_targetPeerID == rpc.m_id || data.m_targetPeerID == 0L) rpc.HandleRoutedRPC(data);
            if (data.m_targetPeerID != rpc.m_id) rpc.RouteRPC(data);
        }
    }
}
