﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BetterLoading.Stage.InitialLoad {
    public class StageApplyPatches : LoadingStage {
        private static List<ModContentPack> _modList;
        private static ModContentPack? _currentMod;
        private static int _currentModNum;

        private static bool _loadingPatches;
        private static int _numPatches = -1;
        private static int _currentPatch;

        private static StageApplyPatches inst;
        private static bool _hasFinished;

        public StageApplyPatches(Harmony instance) : base(instance) {
            _modList = LoadedModManager.RunningMods.ToList();
        }

        public override void BecomeInactive() {
            _currentMod = null;
            _currentModNum = 0;
            GlobalTimingData.TicksFinishedBuildingXmlTree = DateTime.UtcNow.Ticks;
            _hasFinished = true;
        }

        public override void BecomeActive() {
            inst = LoadingScreen.GetStageInstance<StageApplyPatches>();
        }

        public override string GetStageName() {
            return "正在应用补丁";
        }

        public override string? GetCurrentStepName() {
            if (_currentMod == null)
                return "<初始化中>";

            var result = _currentMod.Name + ": ";

            if (_numPatches < 0)
                result += "加载补丁中...";
            else
                result += $"{_currentPatch} / {_numPatches}";

            return result;
        }

        public override int GetCurrentProgress() {
            return _currentModNum;
        }

        public override int GetMaximumProgress() {
            if (_modList.Count == 0) return 1;

            return _modList.Count;
        }

        public override bool IsCompleted() {
            return GetCurrentProgress() == GetMaximumProgress() && _currentPatch >= _numPatches && !_loadingPatches;
        }

        public override void DoPatching(Harmony instance) {
            instance.Patch(AccessTools.Method(typeof(LoadedModManager), nameof(LoadedModManager.ApplyPatches)), new HarmonyMethod(typeof(StageApplyPatches), nameof(PreApplyPatches)));

            instance.Patch(AccessTools.PropertyGetter(typeof(ModContentPack), nameof(ModContentPack.Patches)), new HarmonyMethod(typeof(StageApplyPatches), nameof(PreLoadPatches)), new HarmonyMethod(typeof(StageApplyPatches), nameof(PostLoadPatches)));

            instance.Patch(AccessTools.Method(typeof(PatchOperation), nameof(PatchOperation.Apply)), postfix: new HarmonyMethod(typeof(StageApplyPatches), nameof(PostApplyPatch)));
        }

        public static void PreApplyPatches() {
            //Reset this in case the patch was triggered early
            _currentModNum = 0;
            //Some mods load their patches before this point - remove them (e.g. DocWorld)
        }

        public static void PreLoadPatches(ModContentPack __instance) {
            if (_hasFinished) return;

            _loadingPatches = true;
            _currentMod = __instance;
            _currentModNum = _modList.IndexOf(_currentMod) + 1;
            _numPatches = -1;
            _currentPatch = 0;
            BetterLoadingApi.DispatchChange(inst);
        }

        public static void PostLoadPatches(List<PatchOperation> ___patches) {
            if (_hasFinished) return;

            _numPatches = ___patches.Count;
            _currentPatch = 0;
            _loadingPatches = false;
            BetterLoadingApi.DispatchChange(inst);
        }

        public static void PostApplyPatch() {
            _currentPatch++;
            if (_currentPatch > _numPatches)
                _numPatches++;

            // BetterLoadingApi.DispatchChange(inst); //This is probably overkill
        }
    }
}