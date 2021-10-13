﻿using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace BetterLoading.Stage.SaveLoad {
    public class LoadMaps : LoadingStage {
        public static int NumMaps;

        private static int _currMapNum = -1;
        private static Map _currMap;

        private static bool _currMapInitialized;
        private static bool _currMapLoadedComponents;
        private static bool _currMapLoadedCompressed;

        private static bool _allMapsLoaded;

        public static void CountMaps() {
            if (!Scribe.EnterNode("maps"))
                return;

            try {
                NumMaps = Scribe.loader.curXmlParent.ChildNodes.Count;
                Log.Message($"[BetterLoading] This save file contains {NumMaps} map(s).");
            } finally {
                //Make sure we absolutely exit the node because if we don't we risk corrupting the save file.
                Scribe.ExitNode();
            }
        }

        public LoadMaps([NotNull] Harmony instance) : base(instance) {
        }

        public override string GetStageName() {
            return "正在加载地图";
        }

        public override string? GetCurrentStepName() {
            if (_currMapNum == -1)
                return "等待中...";

            var text = $"地图 {_currMapNum + 1} / {NumMaps}: ";

            if (_currMapLoadedCompressed)
                return text + "加载物体中";

            if (_currMapLoadedComponents)
                return text + "加载压缩地图数据中";

            if (_currMapInitialized)
                return text + "加载组件中";

            return text + "读取数据中";
        }

        public override int GetCurrentProgress() {
            if (_currMapNum == -1)
                return 0;

            var baseValue = _currMapNum * 4;

            if (_currMapLoadedCompressed)
                return baseValue + 3;

            if (_currMapLoadedComponents)
                return baseValue + 2;

            if (_currMapInitialized)
                return baseValue + 1;

            return baseValue;
        }

        public override int GetMaximumProgress() {
            return NumMaps * 4;
        }

        public override bool IsCompleted() {
            return _allMapsLoaded;
        }

        public override void BecomeInactive() {
            _allMapsLoaded = false;
            _currMapNum = -1;
        }

        public override void DoPatching(Harmony instance) {
            instance.Patch(AccessTools.Method(typeof(Map), nameof(Map.ExposeData)), new HarmonyMethod(typeof(LoadMaps), nameof(OnMapLoadStart)));
            instance.Patch(AccessTools.Method(typeof(Map), "ExposeComponents"), new HarmonyMethod(typeof(LoadMaps), nameof(OnMapComponentsLoadStart)));
            instance.Patch(AccessTools.Method(typeof(MapFileCompressor), nameof(MapFileCompressor.ExposeData)), new HarmonyMethod(typeof(LoadMaps), nameof(OnMapCompressedDataLoadStart)), new HarmonyMethod(typeof(LoadMaps), nameof(OnMapCompressedDataLoadEnd)));
            instance.Patch(AccessTools.Method(typeof(CameraDriver), nameof(CameraDriver.Expose)), postfix: new HarmonyMethod(typeof(LoadMaps), nameof(OnAllMapsLoaded)));
        }

        public static void OnMapLoadStart(Map __instance) {
            _currMap = __instance;
            _currMapNum++;

            _currMapInitialized = false;
            _currMapLoadedComponents = false;
            _currMapLoadedCompressed = false;
        }

        public static void OnMapComponentsLoadStart() {
            _currMapInitialized = true;
        }

        public static void OnMapCompressedDataLoadStart() {
            _currMapLoadedComponents = true;
        }

        public static void OnMapCompressedDataLoadEnd() {
            _currMapLoadedCompressed = true;
        }

        public static void OnAllMapsLoaded() {
            _allMapsLoaded = true;
        }
    }
}