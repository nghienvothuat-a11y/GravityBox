#if DEVELOPMENT_BUILD && !UNITY_EDITOR
// Generated. Explicit calls preserve scenarios under player stripping.
using System;
using System.Collections;
namespace GravityBox.Venom.ChapterProof
{
    internal static class COgheChapterProofScenarios
    {
        internal const string SourceManifest = "COgheBoss30ScreenTests:ad4a528a146ea5ed8ea3c6acd65ff9a3f25a9001132d74f13479b8d505a39f9e;COgheCampaign40RouteTests:4683e84c9d9e3caa787980e1aeaacc2744222dc6a521fbaefd085f7c357842a4;COgheCampaign40CooperationTests:f52515732c7bb40d829441ff566b3dd2e041cbfe8be02d470487b1ece5ac94e4;COgheCampaign40BossTests:9c0a529386b31dc5838b1cd2ca6609c68f0ffca41c6364c7cfd2b8e9fd05973c";
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
