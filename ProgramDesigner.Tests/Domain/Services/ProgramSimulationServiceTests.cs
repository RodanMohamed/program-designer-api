using ProgramDesigner.Domain;
using ProgramDesigner.Domain.Enums;
using ProgramDesigner.Domain.Exceptions;
using ProgramDesigner.Domain.Requests;
using ProgramDesigner.Tests.Domain.TestHelpers;
using ProgramDesigner.Domain.Results;
using ProgramDesigner.Domain.Services;
using ProgramDesigner.Domain.ValueObjects;

namespace ProgramDesigner.Tests.Domain.Services
{
    public class ProgramSimulationServiceTests
    {
        private readonly ProgramSimulationService _service = new();

        [Fact]
        public void FreshStart_NothingCompletedNoChoices_OnlyFirstStepsAreUnlocked()
        {
            ComputerScienceScenario scenario = ComputerScienceScenario.Build();
            SimulationInput input = new(new Dictionary<int, IReadOnlyList<int>>(), new HashSet<int>());

            SimulationResult result = _service.Simulate(scenario.Program, input);

            Assert.Equal(ProgramItemStatus.Unlocked, FindStatus(result, scenario.IntroToComputing));
            Assert.Equal(ProgramItemStatus.Blocked, FindStatus(result, scenario.MathForComputing));
            Assert.Equal(ProgramItemStatus.Blocked, FindStatus(result, scenario.Major));
            Assert.Equal(ProgramItemStatus.Blocked, FindStatus(result, scenario.FinalCapstone));
        }

        [Fact]
        public void FoundationsCompleted_ChoiceNotYetDecided_MajorUnlocksButNoBranchIsExcluded()
        {
            ComputerScienceScenario scenario = ComputerScienceScenario.Build();
            SimulationInput input = new(
                new Dictionary<int, IReadOnlyList<int>>(),
                new HashSet<int> { scenario.IntroToComputing.Id, scenario.MathForComputing.Id });

            SimulationResult result = _service.Simulate(scenario.Program, input);

            Assert.Equal(ProgramItemStatus.Unlocked, FindStatus(result, scenario.Major));
            Assert.NotEqual(ProgramItemStatus.Excluded, FindStatus(result, scenario.AiTrack));
            Assert.NotEqual(ProgramItemStatus.Excluded, FindStatus(result, scenario.ItTrack));
            Assert.NotEqual(ProgramItemStatus.Excluded, FindStatus(result, scenario.ProgrammingTrack));
        }

        [Fact]
        public void FoundationsCompletedAndAiChosen_OtherTracksAreExcludedAndFirstAiStepUnlocks()
        {
            ComputerScienceScenario scenario = ComputerScienceScenario.Build();
            SimulationInput input = new(
                new Dictionary<int, IReadOnlyList<int>>
                {
                    { scenario.Major.Id, new List<int> { scenario.AiTrack.Id } }
                },
                new HashSet<int> { scenario.IntroToComputing.Id, scenario.MathForComputing.Id });

            SimulationResult result = _service.Simulate(scenario.Program, input);

            Assert.Equal(ProgramItemStatus.Excluded, FindStatus(result, scenario.ItTrack));
            Assert.Equal(ProgramItemStatus.Excluded, FindStatus(result, scenario.ProgrammingTrack));
            Assert.Equal(ProgramItemStatus.Unlocked, FindStatus(result, scenario.MachineLearningBasics));
            Assert.Equal(ProgramItemStatus.Blocked, FindStatus(result, scenario.Electives));
        }

        [Fact]
        public void EntireAiTrackCompleted_MajorAndFinalCapstoneBecomeReachable()
        {
            ComputerScienceScenario scenario = ComputerScienceScenario.Build();
            SimulationInput input = new(
                new Dictionary<int, IReadOnlyList<int>>
                {
                    { scenario.Major.Id, new List<int> { scenario.AiTrack.Id } },
                    { scenario.Electives.Id, new List<int> { scenario.ComputerVision.Id, scenario.NaturalLanguageProcessing.Id } }
                },
                new HashSet<int>
                {
                    scenario.IntroToComputing.Id, scenario.MathForComputing.Id,
                    scenario.MachineLearningBasics.Id, scenario.ComputerVision.Id,
                    scenario.NaturalLanguageProcessing.Id, scenario.AiCapstone.Id
                });

            SimulationResult result = _service.Simulate(scenario.Program, input);

            Assert.Equal(ProgramItemStatus.Complete, FindStatus(result, scenario.Major));
            Assert.Equal(ProgramItemStatus.Unlocked, FindStatus(result, scenario.FinalCapstone));
        }

        [Fact]
        public void MultiplePrerequisites_ReasonListsEveryIncompleteOne()
        {
            // Covers the new "more than one prerequisite" requirement (AND semantics).
            Group root = Group.Create("Root", GroupRule.InOrder());
            Step stepA = Step.Create("Step A", "Attend Session");
            Step stepB = Step.Create("Step B", "Pass Test");
            Step stepC = Step.Create("Step C", "Submit Work");
            root.AddChild(stepA);
            root.AddChild(stepB);
            root.AddChild(stepC);
            stepC.AddPrerequisite(stepA);
            stepC.AddPrerequisite(stepB);
            stepA.AssignId(101);
            stepB.AssignId(102);
            stepC.AssignId(103);

            LearningProgram program = LearningProgram.Create("Root Program", root);
            SimulationInput input = new(new Dictionary<int, IReadOnlyList<int>>(), new HashSet<int>());

            SimulationResult result = _service.Simulate(program, input);

            ProgramItemState stateC = FindState(result, stepC);
            Assert.Equal(ProgramItemStatus.Blocked, stateC.Status);
            Assert.Equal("Waiting on: Step A, Step B", stateC.Reason);
        }

        private static ProgramItemStatus FindStatus(SimulationResult result, ProgramItem item) =>
            FindState(result, item).Status;

        private static ProgramItemState FindState(SimulationResult result, ProgramItem item)
        {
            foreach (ProgramItemState state in result.Items)
            {
                if (ReferenceEquals(state.Item, item))
                {
                    return state;
                }
            }

            throw new InvalidOperationException($"Item '{item.Name}' was not found in the simulation result.");
        }
    }
}
