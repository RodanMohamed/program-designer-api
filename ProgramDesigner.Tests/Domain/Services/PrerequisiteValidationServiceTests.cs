using ProgramDesigner.Domain;
using ProgramDesigner.Domain.Exceptions;
using ProgramDesigner.Domain.Results;
using ProgramDesigner.Domain.Services;
using ProgramDesigner.Domain.ValueObjects;
using ProgramDesigner.Tests.Domain.TestHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.TestsDomain.Services
{
    public class PrerequisiteValidationServiceTests
    {
        private readonly PrerequisiteValidationService _service = new();

        [Fact]
        public void FullComputerScienceScenario_ShouldValidateWithoutErrorsOrWarnings()
        {
            ComputerScienceScenario scenario = ComputerScienceScenario.Build();

            ValidationResult result = _service.Validate(scenario.Program);

            Assert.True(result.IsValid);
            Assert.Empty(result.ImpossiblePrerequisites);
            Assert.Empty(result.ReachabilityWarnings);
        }

        [Fact]
        public void DirectPrerequisiteCycle_ShouldBeRejected()
        {
            Group root = Group.Create("Root", GroupRule.InOrder());
            Step stepA = Step.Create("Step A", "Attend Session");
            Step stepB = Step.Create("Step B", "Pass Test");
            root.AddChild(stepA);
            root.AddChild(stepB);
            stepA.AddPrerequisite(stepB);
            stepB.AddPrerequisite(stepA);

            LearningProgram program = LearningProgram.Create("Root Program", root);

            ValidationResult result = _service.Validate(program);

            Assert.False(result.IsValid);
            Assert.NotEmpty(result.ImpossiblePrerequisites);
        }

        [Fact]
        public void PrerequisiteInsideChoiceGroup_ShouldGenerateReachabilityWarning()
        {
            Group root = Group.Create("Root", GroupRule.InOrder());
            Group major = Group.Create("Major", GroupRule.Choice(1));
            Group trackA = Group.Create("Track A", GroupRule.InOrder());
            Group trackB = Group.Create("Track B", GroupRule.InOrder());
            Step specialStep = Step.Create("Special Step", "Pass Test");
            Step bStep = Step.Create("B Step", "Pass Test");
            Step finalStep = Step.Create("Final Step", "Submit Work");

            trackA.AddChild(specialStep);
            trackB.AddChild(bStep);
            major.AddChild(trackA);
            major.AddChild(trackB);
            root.AddChild(major);
            root.AddChild(finalStep);
            finalStep.AddPrerequisite(specialStep);

            LearningProgram program = LearningProgram.Create("Root Program", root);

            ValidationResult result = _service.Validate(program);

            Assert.True(result.IsValid);
            Assert.Empty(result.ImpossiblePrerequisites);
            Assert.Single(result.ReachabilityWarnings);
        }

        [Fact]
        public void AddingSelfAsPrerequisite_ShouldBeRejectedImmediately()
        {
            // In the new Domain model this is rejected at the moment it would happen,
            // not just flagged later during Validate() — an aggregate can never even be
            // built with a self-referencing prerequisite.
            Step step = Step.Create("Step A", "Attend Session");

            DomainException exception = Assert.Throws<DomainException>(() => step.AddPrerequisite(step));
            Assert.Contains("itself", exception.Message);
        }

        [Fact]
        public void PrerequisiteOnDescendant_ShouldBeRejected()
        {
            Group root = Group.Create("Root", GroupRule.InOrder());
            Group parent = Group.Create("Parent", GroupRule.InOrder());
            Step child = Step.Create("Child", "Attend Session");
            parent.AddChild(child);
            root.AddChild(parent);
            parent.AddPrerequisite(child); // parent depends on something inside itself

            LearningProgram program = LearningProgram.Create("Root Program", root);

            ValidationResult result = _service.Validate(program);

            Assert.False(result.IsValid);
            Assert.Single(result.ImpossiblePrerequisites);
            Assert.Contains("nested inside", result.ImpossiblePrerequisites[0].Description);
        }

        [Fact]
        public void MultiplePrerequisites_OneImpossibleOneValid_ShouldReportOnlyTheImpossibleOne()
        {
            // Covers the new "more than one prerequisite" requirement: each one is
            // checked independently, so a valid one never hides a broken one.
            Group root = Group.Create("Root", GroupRule.InOrder());
            Step stepA = Step.Create("Step A", "Attend Session");
            Step stepB = Step.Create("Step B", "Pass Test");
            Step stepC = Step.Create("Step C", "Submit Work");
            root.AddChild(stepA);
            root.AddChild(stepB);
            root.AddChild(stepC);

            stepC.AddPrerequisite(stepA); // valid: A comes before C
            stepA.AddPrerequisite(stepB); // impossible: B comes after A

            LearningProgram program = LearningProgram.Create("Root Program", root);

            ValidationResult result = _service.Validate(program);

            Assert.False(result.IsValid);
            Assert.Single(result.ImpossiblePrerequisites);
            Assert.Equal("Step A", result.ImpossiblePrerequisites[0].Item.Name);
        }
    }
}
