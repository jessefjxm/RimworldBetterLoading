﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BetterLoading.Stage.InitialLoad {
    public class StageReadXML : LoadingStage {
        private static ModContentPack? _currentPack;
        private static int _currentPackIdx = 1;
        private int _numPacks = -1;

        private static StageReadXML? inst;

        public StageReadXML(Harmony instance) : base(instance) {
        }

        public override string GetStageName() {
            return "正在读取模组内容文件";
        }

        public override string? GetCurrentStepName() {
            return _currentPack?.Name;
        }

        public override int GetCurrentProgress() {
            return _currentPackIdx;
        }

        public override int GetMaximumProgress() {
            return _numPacks;
        }

        public override void DoPatching(Harmony instance) {
            instance.Patch(AccessTools.Method(typeof(LoadedModManager), nameof(LoadedModManager.LoadModXML)), new HarmonyMethod(typeof(Utils), nameof(Utils.HarmonyPatchCancelMethod)), new HarmonyMethod(typeof(StageReadXML), nameof(AlternativeLoadModXml)));
            instance.Patch(AccessTools.Method(typeof(ModContentPack), nameof(ModContentPack.LoadDefs)), postfix: new HarmonyMethod(typeof(StageReadXML), nameof(OnLoadDefsComplete)));
        }

        public override void BecomeActive() {
            _numPacks = LoadedModManager.RunningMods.Count();
            inst = LoadingScreen.GetStageInstance<StageReadXML>();
        }

        public static void AlternativeLoadModXml(ref List<LoadableXmlAsset> __result) {
            __result = LoadedModManager.RunningModsListForReading.AsParallel().SelectMany(m => {
                DeepProfiler.Start("正在加载 " + m);
                try {
                    return m.LoadDefs();
                } catch (Exception e) {
                    Log.Error("[BetterLoading] [Enhanced XML Load] Could not load defs for mod " + m.PackageIdPlayerFacing + ": " + e);
                    return new List<LoadableXmlAsset>();
                } finally {
                    DeepProfiler.End();
                }
            }).ToList();

            Log.Message($"[BetterLoading] [Enhanced XML Load] Loaded {__result.Count} loadable assets.");
            _currentPackIdx = inst._numPacks + 1;
        }

        public static void OnLoadDefsComplete(ModContentPack __instance) {
            _currentPack = __instance;
            _currentPackIdx++;
            BetterLoadingApi.DispatchChange(inst);
        }
    }
}