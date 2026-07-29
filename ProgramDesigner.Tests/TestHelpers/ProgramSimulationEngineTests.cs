using ProgramDesigner.BLL.DTOs.Requests;
using ProgramDesigner.BLL.DTOs.Responses;
using ProgramDesigner.BLL.Helpers;
using Xunit;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Tests.TestHelpers
{
    public class ProgramSimulationEngineTests
    {
        private readonly BLL.Simulators.ProgramSimulationEngine.ProgramSimulationEngine _engine;

        public ProgramSimulationEngineTests()
        {
            _engine = new ProgramDesigner.BLL.Simulators.ProgramSimulationEngine.ProgramSimulationEngine();
        }

        [Fact]
        public void FreshStart_NothingCompletedNoChoices_OnlyFirstStepsAreUnlocked()
        {
            var flatItems = ComputerScienceScenarioBuilder.BuildFlatItems();
            var tree = ProgramTreeBuilder.Build(flatItems, ComputerScienceScenarioBuilder.RootId);

            SimulateProgramDto request = new SimulateProgramDto();

            SimulateProgramResponseDto result = _engine.Simulate(tree, request);

            ProgramItemStateDto introToComputing = FindItem(result, 4);
            ProgramItemStateDto mathForComputing = FindItem(result, 5);
            ProgramItemStateDto major = FindItem(result, 3);
            ProgramItemStateDto finalCapstone = FindItem(result, 9);

            Assert.Equal("Unlocked", introToComputing.Status);
            Assert.Equal("Blocked", mathForComputing.Status);
            Assert.Equal("Blocked", major.Status);
            Assert.Equal("Blocked", finalCapstone.Status);
        }

        [Fact]
        public void FoundationsCompleted_ChoiceNotYetDecided_MajorUnlocksButNoBranchIsExcluded()
        {
            var flatItems = ComputerScienceScenarioBuilder.BuildFlatItems();
            var tree = ProgramTreeBuilder.Build(flatItems, ComputerScienceScenarioBuilder.RootId);

            SimulateProgramDto request = new SimulateProgramDto();
            request.CompletedItemIds = new List<int> { 4, 5 };

            SimulateProgramResponseDto result = _engine.Simulate(tree, request);

            ProgramItemStateDto major = FindItem(result, 3);
            ProgramItemStateDto aiTrack = FindItem(result, 6);
            ProgramItemStateDto itTrack = FindItem(result, 7);
            ProgramItemStateDto programmingTrack = FindItem(result, 8);

            Assert.Equal("Unlocked", major.Status);

            // No choice was recorded yet for group 3 ("Major"), so no branch should be Excluded.
            Assert.NotEqual("Excluded", aiTrack.Status);
            Assert.NotEqual("Excluded", itTrack.Status);
            Assert.NotEqual("Excluded", programmingTrack.Status);
        }

        [Fact]
        public void FoundationsCompletedAndAiChosen_OtherTracksAreExcludedAndFirstAiStepUnlocks()
        {
            var flatItems = ComputerScienceScenarioBuilder.BuildFlatItems();
            var tree = ProgramTreeBuilder.Build(flatItems, ComputerScienceScenarioBuilder.RootId);

            SimulateProgramDto request = new SimulateProgramDto();
            request.CompletedItemIds = new List<int> { 4, 5 };
            request.Choices = new Dictionary<int, List<int>> { { 3, new List<int> { 6 } } };

            SimulateProgramResponseDto result = _engine.Simulate(tree, request);

            ProgramItemStateDto itTrack = FindItem(result, 7);
            ProgramItemStateDto programmingTrack = FindItem(result, 8);
            ProgramItemStateDto machineLearningBasics = FindItem(result, 11);
            ProgramItemStateDto electives = FindItem(result, 10);

            Assert.Equal("Excluded", itTrack.Status);
            Assert.Equal("Excluded", programmingTrack.Status);
            Assert.Equal("Unlocked", machineLearningBasics.Status);
            Assert.Equal("Blocked", electives.Status);
        }

        [Fact]
        public void EntireAiTrackCompleted_MajorAndFinalCapstoneBecomeReachable()
        {
            var flatItems = ComputerScienceScenarioBuilder.BuildFlatItems();
            var tree = ProgramTreeBuilder.Build(flatItems, ComputerScienceScenarioBuilder.RootId);

            SimulateProgramDto request = new SimulateProgramDto();
            request.Choices = new Dictionary<int, List<int>> { { 3, new List<int> { 6 } }, { 10, new List<int> { 16, 17 } } };

            // Foundations (4,5) + full AI track: ML Basics (11), two chosen electives (16,17), AI Capstone (19)
            request.CompletedItemIds = new List<int> { 4, 5, 11, 16, 17, 19 };

            SimulateProgramResponseDto result = _engine.Simulate(tree, request);

            ProgramItemStateDto major = FindItem(result, 3);
            ProgramItemStateDto finalCapstone = FindItem(result, 9);

            Assert.Equal("Complete", major.Status);
            Assert.Equal("Unlocked", finalCapstone.Status);
        }

        private ProgramItemStateDto FindItem(SimulateProgramResponseDto response, int itemId)
        {
            foreach (ProgramItemStateDto item in response.Items)
            {
                if (item.ItemId == itemId)
                {
                    return item;
                }
            }

            throw new InvalidOperationException("Item with Id " + itemId + " was not found in the simulation result.");
        }
    }
}
