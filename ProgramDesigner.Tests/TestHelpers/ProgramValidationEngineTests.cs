using ProgramDesigner.BLL.DTOs.Responses;
using ProgramDesigner.BLL.Helpers;
using ProgramDesigner.Common.Enums;
using ProgramDesigner.DAL.Data.Models;
using ProgramDesigner.Tests.TestHelpers;
using Xunit;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Tests.TestHelpers
{
    public class ProgramValidationEngineTests
    {
        private readonly ProgramDesigner.BLL.Validators.ProgramValidationEngine.ProgramValidationEngine _engine;

        public ProgramValidationEngineTests()
        {
            _engine = new ProgramDesigner.BLL.Validators.ProgramValidationEngine.ProgramValidationEngine();
        }

        [Fact]
        public void FullComputerScienceScenario_ShouldValidateWithoutErrorsOrWarnings()
        {
            List<ProgramItem> flatItems = ComputerScienceScenarioBuilder.BuildFlatItems();
            ProgramTreeBuildResult tree = ProgramTreeBuilder.Build(flatItems, ComputerScienceScenarioBuilder.RootId);

            ValidateProgramResponseDto result = _engine.Validate(tree);

            Assert.True(result.IsValid);
            Assert.Empty(result.ImpossiblePrerequisites);
            Assert.Empty(result.ReachabilityWarnings);
        }

        [Fact]
        public void DirectPrerequisiteCycle_ShouldBeRejected()
        {
            Group root = new Group();
            root.Id = 1;
            root.Name = "Root";
            root.RuleType = GroupRuleType.InOrder;
            root.Children = new List<ProgramItem>();

            Step stepA = new Step();
            stepA.Id = 2;
            stepA.Name = "Step A";
            stepA.StepType = "Attend Session";
            stepA.ParentGroupId = 1;
            stepA.Order = 0;
            stepA.PrerequisiteItemId = 3;

            Step stepB = new Step();
            stepB.Id = 3;
            stepB.Name = "Step B";
            stepB.StepType = "Pass Test";
            stepB.ParentGroupId = 1;
            stepB.Order = 1;
            stepB.PrerequisiteItemId = 2;

            List<ProgramItem> flatItems = new List<ProgramItem> { root, stepA, stepB };
            ProgramTreeBuildResult tree = ProgramTreeBuilder.Build(flatItems, 1);

            ValidateProgramResponseDto result = _engine.Validate(tree);

            Assert.False(result.IsValid);
            Assert.NotEmpty(result.ImpossiblePrerequisites);
        }

        [Fact]
        public void PrerequisiteInsideChoiceGroup_ShouldGenerateReachabilityWarning()
        {
            Group root = new Group();
            root.Id = 1;
            root.Name = "Root";
            root.RuleType = GroupRuleType.InOrder;
            root.Children = new List<ProgramItem>();

            Group major = new Group();
            major.Id = 2;
            major.Name = "Major";
            major.RuleType = GroupRuleType.Choice;
            major.ChoiceCount = 1;
            major.ParentGroupId = 1;
            major.Order = 0;
            major.Children = new List<ProgramItem>();

            Group trackA = new Group();
            trackA.Id = 3;
            trackA.Name = "Track A";
            trackA.RuleType = GroupRuleType.InOrder;
            trackA.ParentGroupId = 2;
            trackA.Order = 0;
            trackA.Children = new List<ProgramItem>();

            Group trackB = new Group();
            trackB.Id = 4;
            trackB.Name = "Track B";
            trackB.RuleType = GroupRuleType.InOrder;
            trackB.ParentGroupId = 2;
            trackB.Order = 1;
            trackB.Children = new List<ProgramItem>();

            Step specialStep = new Step();
            specialStep.Id = 5;
            specialStep.Name = "Special Step";
            specialStep.StepType = "Pass Test";
            specialStep.ParentGroupId = 3;
            specialStep.Order = 0;

            Step bStep = new Step();
            bStep.Id = 6;
            bStep.Name = "B Step";
            bStep.StepType = "Pass Test";
            bStep.ParentGroupId = 4;
            bStep.Order = 0;

            Step finalStep = new Step();
            finalStep.Id = 7;
            finalStep.Name = "Final Step";
            finalStep.StepType = "Submit Work";
            finalStep.ParentGroupId = 1;
            finalStep.Order = 1;
            finalStep.PrerequisiteItemId = 5;

            List<ProgramItem> flatItems = new List<ProgramItem>
        {
            root, major, trackA, trackB, specialStep, bStep, finalStep
        };

            ProgramTreeBuildResult tree = ProgramTreeBuilder.Build(flatItems, 1);

            ValidateProgramResponseDto result = _engine.Validate(tree);

            Assert.True(result.IsValid);
            Assert.Empty(result.ImpossiblePrerequisites);
            Assert.Single(result.ReachabilityWarnings);
        }

        [Fact]
        public void PrerequisitePointingAtItself_ShouldBeRejected()
        {
            Group root = new Group();
            root.Id = 1;
            root.Name = "Root";
            root.RuleType = GroupRuleType.InOrder;
            root.Children = new List<ProgramItem>();

            Step stepA = new Step();
            stepA.Id = 2;
            stepA.Name = "Step A";
            stepA.StepType = "Attend Session";
            stepA.ParentGroupId = 1;
            stepA.Order = 0;
            stepA.PrerequisiteItemId = 2;

            List<ProgramItem> flatItems = new List<ProgramItem> { root, stepA };
            ProgramTreeBuildResult tree = ProgramTreeBuilder.Build(flatItems, 1);

            ValidateProgramResponseDto result = _engine.Validate(tree);

            Assert.False(result.IsValid);
            Assert.Single(result.ImpossiblePrerequisites);
            Assert.Contains("itself", result.ImpossiblePrerequisites[0].Description);
        }
    }
}
