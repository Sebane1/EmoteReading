using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using RoleplayingVoice;
using System;
using System.Linq;

namespace ArtemisRoleplayingKit {
    /// <summary>
    /// Implementation Based On Findings From PatMe
    /// </summary>
    public class EmoteReaderHooks : IDisposable {
        public Action<IGameObject, ushort> OnEmote;

        public delegate void OnEmoteFuncDelegate(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2);
        private readonly Hook<OnEmoteFuncDelegate> hookEmote;

        public bool IsValid = false;
        private IClientState _clientState;
        private IObjectTable _objectTable;

        public EmoteReaderHooks(IGameInteropProvider interopProvider, IClientState clientState, IObjectTable objectTable) {
            _clientState = clientState;
            _objectTable = objectTable;
            try {
                var emoteFuncPtr = "E8 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ?? 4C 89 74 24";
                hookEmote = interopProvider.HookFromSignature<OnEmoteFuncDelegate>(emoteFuncPtr, OnEmoteDetour);

                hookEmote.Enable();

                IsValid = true;
            } catch (Exception ex) {
                Plugin.PluginLog.Error(ex, "Something went wrong!");
            }
        }

        public void Dispose() {
            hookEmote?.Dispose();
            IsValid = false;
        }

        void OnEmoteDetour(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2) {
            // unk - some field of event framework singleton? doesn't matter here anyway
            // PluginLog.Log($"Emote >> unk:{unk:X}, instigatorAddr:{instigatorAddr:X}, emoteId:{emoteId}, targetId:{targetId:X}, unk2:{unk2:X}");
            try {
                if (_clientState.LocalPlayer != null) {
                    var instigatorOb = _objectTable.FirstOrDefault(x => (ulong)x.Address == instigatorAddr);
                    if (instigatorOb != null) {
                        OnEmote?.Invoke(instigatorOb, emoteId);
                    }
                }
            } catch {

            }

            hookEmote.Original(unk, instigatorAddr, emoteId, targetId, unk2);
        }
    }
}
