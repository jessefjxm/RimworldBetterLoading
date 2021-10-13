﻿using HarmonyLib;
using JetBrains.Annotations;

namespace BetterLoading.Stage.SaveLoad {
    public class FinalizeGameState : LoadingStage {
        public FinalizeGameState([NotNull] Harmony instance) : base(instance) {
        }

        public override string GetStageName() {
            return "正在设置最终游戏控制器";
        }

        public override string? GetCurrentStepName() {
            return null;
        }

        public override int GetCurrentProgress() {
            return 0;
        }

        public override int GetMaximumProgress() {
            return 1;
        }

        public override void DoPatching(Harmony instance) {
        }
    }
}