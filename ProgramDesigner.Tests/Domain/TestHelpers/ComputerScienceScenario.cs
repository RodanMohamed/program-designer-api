using ProgramDesigner.Domain;
using ProgramDesigner.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ProgramDesigner.Tests.Domain.TestHelpers
{
    public sealed class ComputerScienceScenario
    {
        public LearningProgram Program { get; }
        public Group Root { get; }
        public Group Foundations { get; }
        public Group Major { get; }
        public Step IntroToComputing { get; }
        public Step MathForComputing { get; }
        public Group AiTrack { get; }
        public Group ItTrack { get; }
        public Group ProgrammingTrack { get; }
        public Step FinalCapstone { get; }
        public Group Electives { get; }
        public Step MachineLearningBasics { get; }
        public Step NetworksAndSecurity { get; }
        public Step SystemsAdministration { get; }
        public Step AlgorithmsAndDataStructures { get; }
        public Step SoftwareEngineering { get; }
        public Step ComputerVision { get; }
        public Step NaturalLanguageProcessing { get; }
        public Step Robotics { get; }
        public Step AiCapstone { get; }

        private ComputerScienceScenario(
            LearningProgram program, Group root, Group foundations, Group major,
            Step introToComputing, Step mathForComputing, Group aiTrack, Group itTrack,
            Group programmingTrack, Step finalCapstone, Group electives,
            Step machineLearningBasics, Step networksAndSecurity, Step systemsAdministration,
            Step algorithmsAndDataStructures, Step softwareEngineering, Step computerVision,
            Step naturalLanguageProcessing, Step robotics, Step aiCapstone)
        {
            Program = program; Root = root; Foundations = foundations; Major = major;
            IntroToComputing = introToComputing; MathForComputing = mathForComputing;
            AiTrack = aiTrack; ItTrack = itTrack; ProgrammingTrack = programmingTrack;
            FinalCapstone = finalCapstone; Electives = electives;
            MachineLearningBasics = machineLearningBasics; NetworksAndSecurity = networksAndSecurity;
            SystemsAdministration = systemsAdministration; AlgorithmsAndDataStructures = algorithmsAndDataStructures;
            SoftwareEngineering = softwareEngineering; ComputerVision = computerVision;
            NaturalLanguageProcessing = naturalLanguageProcessing; Robotics = robotics; AiCapstone = aiCapstone;
        }

        public static ComputerScienceScenario Build()
        {
            Group root = Group.Create("Computer Science", GroupRule.InOrder());

            Group foundations = Group.Create("Foundations", GroupRule.InOrder());
            Step introToComputing = Step.Create("Introduction to Computing", "Attend Session");
            Step mathForComputing = Step.Create("Mathematics for Computing", "Pass Test");
            foundations.AddChild(introToComputing);
            foundations.AddChild(mathForComputing);

            Group major = Group.Create("Major", GroupRule.Choice(1));

            Group aiTrack = Group.Create("AI", GroupRule.InOrder());
            Step machineLearningBasics = Step.Create("Machine Learning Basics", "Attend Session");
            Group electives = Group.Create("Electives", GroupRule.Choice(2));
            Step computerVision = Step.Create("Computer Vision", "Pass Test");
            Step naturalLanguageProcessing = Step.Create("Natural Language Processing", "Pass Test");
            Step robotics = Step.Create("Robotics", "Pass Test");
            electives.AddChild(computerVision);
            electives.AddChild(naturalLanguageProcessing);
            electives.AddChild(robotics);
            Step aiCapstone = Step.Create("AI Capstone", "Submit Work");
            aiTrack.AddChild(machineLearningBasics);
            aiTrack.AddChild(electives);
            aiTrack.AddChild(aiCapstone);
            aiCapstone.AddPrerequisite(electives);

            Group itTrack = Group.Create("IT", GroupRule.InOrder());
            Step networksAndSecurity = Step.Create("Networks & Security", "Attend Session");
            Step systemsAdministration = Step.Create("Systems Administration", "Pass Test");
            itTrack.AddChild(networksAndSecurity);
            itTrack.AddChild(systemsAdministration);

            Group programmingTrack = Group.Create("Programming", GroupRule.InOrder());
            Step algorithmsAndDataStructures = Step.Create("Algorithms & Data Structures", "Pass Test");
            Step softwareEngineering = Step.Create("Software Engineering", "Submit Work");
            programmingTrack.AddChild(algorithmsAndDataStructures);
            programmingTrack.AddChild(softwareEngineering);

            major.AddChild(aiTrack);
            major.AddChild(itTrack);
            major.AddChild(programmingTrack);
            major.AddPrerequisite(foundations);

            Step finalCapstone = Step.Create("Final Capstone", "Submit Work");
            finalCapstone.AddPrerequisite(major);

            root.AddChild(foundations);
            root.AddChild(major);
            root.AddChild(finalCapstone);

            LearningProgram program = LearningProgram.Create("Computer Science", root);

            // Ids only matter for the simulation tests (Choices/CompletedItemIds
            // reference items by Id). Numbering matches the challenge document.
            root.AssignId(1); foundations.AssignId(2); major.AssignId(3);
            introToComputing.AssignId(4); mathForComputing.AssignId(5);
            aiTrack.AssignId(6); itTrack.AssignId(7); programmingTrack.AssignId(8);
            finalCapstone.AssignId(9); electives.AssignId(10);
            machineLearningBasics.AssignId(11); networksAndSecurity.AssignId(12);
            systemsAdministration.AssignId(13); algorithmsAndDataStructures.AssignId(14);
            softwareEngineering.AssignId(15); computerVision.AssignId(16);
            naturalLanguageProcessing.AssignId(17); robotics.AssignId(18); aiCapstone.AssignId(19);

            return new ComputerScienceScenario(
                program, root, foundations, major, introToComputing, mathForComputing,
                aiTrack, itTrack, programmingTrack, finalCapstone, electives,
                machineLearningBasics, networksAndSecurity, systemsAdministration,
                algorithmsAndDataStructures, softwareEngineering, computerVision,
                naturalLanguageProcessing, robotics, aiCapstone);
        }
    }
}
