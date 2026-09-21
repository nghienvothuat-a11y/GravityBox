#if DEVELOPMENT_BUILD && !UNITY_EDITOR
// Generated. Explicit calls preserve scenarios under player stripping.
using System;
using System.Collections;
namespace GravityBox.Venom.ChapterProof
{
    internal static class COgheChapterProofScenarios
    {
        internal const string SourceManifest = "COgheChapterScreenInput:474dfbf04bc16e980d8fb3a9bc16805ae28928a4cc44856e50ba9f5286460732;COgheBoss30ScreenTests:ad4a528a146ea5ed8ea3c6acd65ff9a3f25a9001132d74f13479b8d505a39f9e;COgheCampaign40RouteTests:2357d5e418b61d80942c8df4387e26f9b39f67b4b369847b78ed082be8f346ca;COgheCampaign40CooperationTests:676dce24c46c253a1b5bd84786fc603738606fd95171680265f9540943c8fc54;COgheCampaign40BossTests:55eb9a39bbbf57c3355eb74bc07f792df8fbb619acfd3f3f53a7c0c5e88d4c08";
        internal static IEnumerator Run(int level)
        {
            switch (level)
            {
                case 30:
                {
                    var scenario = new COgheBoss30ScreenReplay();
                    scenario.Before();
                    yield return scenario.Campaign30WholeSolutionUsesScreenCommands();
                    scenario.After();
                    break;
                }
                case 31:
                {
                    var scenario = new COgheCampaign40RouteReplay();
                    yield return scenario.Before();
                    yield return scenario.Level31RetractsExposesPinThenMeshesAndWins();
                    yield return scenario.After();
                    break;
                }
                case 32:
                {
                    var scenario = new COgheCampaign40RouteReplay();
                    yield return scenario.Before();
                    yield return scenario.Level32UsesAThenReturnsToMoveTheSameBridgeToBAndWins();
                    yield return scenario.After();
                    break;
                }
                case 33:
                {
                    var scenario = new COgheCampaign40RouteReplay();
                    yield return scenario.Before();
                    yield return scenario.Level33ChoosesAOpensEReturnsToSAndChoosesBToWin();
                    yield return scenario.After();
                    break;
                }
                case 34:
                {
                    var scenario = new COgheCampaign40RouteReplay();
                    yield return scenario.Before();
                    yield return scenario.Level34RetractsBParksXPassesGReturnsXRestoresBAndWins();
                    yield return scenario.After();
                    break;
                }
                case 35:
                {
                    var scenario = new COgheCampaign40CooperationReplay();
                    yield return scenario.Before();
                    yield return scenario.Level35ReturnsWhenAIsReleasedThenCatchesDReunitesPullsEAndExits();
                    yield return scenario.After();
                    break;
                }
                case 36:
                {
                    var scenario = new COgheCampaign40CooperationReplay();
                    yield return scenario.Before();
                    yield return scenario.Level36RevealsBChangesRolesAndCompletesWithRealWinchEffort();
                    yield return scenario.After();
                    break;
                }
                case 37:
                {
                    var scenario = new COgheCampaign40CooperationReplay();
                    yield return scenario.Before();
                    yield return scenario.Level37UsesOneGearAtBothStationsWhileAIsHeldThenReunitesAndExits();
                    yield return scenario.After();
                    break;
                }
                case 38:
                {
                    var scenario = new COgheCampaign40CooperationReplay();
                    yield return scenario.Before();
                    yield return scenario.Level38SplitsOpensDReunitesMovesHeavyQSplitsAgainAndExits();
                    yield return scenario.After();
                    break;
                }
                case 39:
                {
                    var scenario = new COgheCampaign40BossReplay();
                    scenario.Before();
                    yield return scenario.Campaign39TwoHeldOutputsReuniteAndEscape();
                    scenario.After();
                    break;
                }
                case 40:
                {
                    var scenario = new COgheCampaign40BossReplay();
                    scenario.Before();
                    yield return scenario.Campaign40PreparesBeforeCutsThenUnlocksAndPullsFinalCap();
                    scenario.After();
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(level));
            }
        }
    }
}
#endif
