using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace BetterLoading.Stage.SaveLoad {
    public class FinalizeScribeLoad : LoadingStage {
        private static bool _complete;

        public FinalizeScribeLoad([NotNull] Harmony instance) : base(instance) {
        }

        public override string GetStageName() {
            return "正在完成存档读取并进行清理...";
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

        public override bool IsCompleted() {
            return _complete;
        }

        public override void BecomeInactive() {
            _complete = false;
        }

        public override void DoPatching(Harmony instance) {
            instance.Patch(AccessTools.Method(typeof(ScribeLoader), nameof(ScribeLoader.FinalizeLoading)), postfix: new HarmonyMethod(typeof(FinalizeScribeLoad), nameof(OnResolvingComplete)));
        }

        public static void OnResolvingComplete() {
            _complete = true;
        }
    }
}