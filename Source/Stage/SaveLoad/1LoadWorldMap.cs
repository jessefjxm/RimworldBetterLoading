﻿using HarmonyLib;
using JetBrains.Annotations;
using RimWorld.Planet;
using System.Linq;

namespace BetterLoading.Stage.SaveLoad {
    public class LoadWorldMap : LoadingStage {
        private static bool _hasLoadedWorldFromFile;
        private static bool _hasFinalizedWorldInit;
        private static bool _loadingGeneratorDataFromFile;

        private static int _numWorldgenSteps;

        public LoadWorldMap([NotNull] Harmony instance) : base(instance) {
        }

        public override string GetStageName() {
            return "正在初始化世界地图";
        }

        public override string? GetCurrentStepName() {
            if (!_hasLoadedWorldFromFile) {
                if (_numWorldgenSteps == 0)
                    return "初始化基础世界数据中";

                if (_loadingGeneratorDataFromFile)
                    return $"从存档加载 {_numWorldgenSteps} 世界特征中";

                return $"生成 {_numWorldgenSteps} 世界特征中";
            }

            return "结束世界初始化中";
        }

        public override int GetCurrentProgress() {
            return _hasLoadedWorldFromFile ? 1 : 0;
        }

        public override int GetMaximumProgress() {
            return 2;
        }

        public override bool IsCompleted() {
            return _hasFinalizedWorldInit;
        }

        public override void BecomeInactive() {
            _hasFinalizedWorldInit = false;
            _hasLoadedWorldFromFile = false;
        }

        public override void DoPatching(Harmony instance) {
            instance.Patch(AccessTools.Method(typeof(World), nameof(World.ExposeData)), postfix: new HarmonyMethod(typeof(LoadWorldMap), nameof(OnLoadWorldEnd)));
            instance.Patch(AccessTools.Method(typeof(WorldGenerator), nameof(WorldGenerator.GenerateFromScribe)), new HarmonyMethod(typeof(LoadWorldMap), nameof(OnStartLoadGeneratedData)));
            instance.Patch(AccessTools.Method(typeof(WorldGenerator), nameof(WorldGenerator.GenerateWithoutWorldData)), new HarmonyMethod(typeof(LoadWorldMap), nameof(OnStartGenerateFreshData)));
            instance.Patch(AccessTools.Method(typeof(World), nameof(World.FinalizeInit)), postfix: new HarmonyMethod(typeof(LoadWorldMap), nameof(OnFinalizeWorldInitEnd)));
        }

        public static void OnStartLoadGeneratedData() {
            _loadingGeneratorDataFromFile = true;
            _numWorldgenSteps = WorldGenerator.GenStepsInOrder.Count();
        }

        public static void OnStartGenerateFreshData() {
            _loadingGeneratorDataFromFile = false;
            _numWorldgenSteps = WorldGenerator.GenStepsInOrder.Count();
        }

        public static void OnLoadWorldEnd() {
            _hasLoadedWorldFromFile = true;
        }

        public static void OnFinalizeWorldInitEnd() {
            _hasFinalizedWorldInit = true;
        }
    }
}