using ProgramDesigner.Common.Enums;
using ProgramDesigner.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Tests.TestHelpers
{
    public static class ComputerScienceScenarioBuilder
    {
        public const int RootId = 1;

        public static List<ProgramItem> BuildFlatItems()
        {
            Group root = CreateGroup(1, "Computer Science", GroupRuleType.InOrder, null, null, 0);
            Group foundations = CreateGroup(2, "Foundations", GroupRuleType.InOrder, null, 1, 0);
            Group major = CreateGroup(3, "Major", GroupRuleType.Choice, 1, 1, 1);
            Step introToComputing = CreateStep(4, "Introduction to Computing", "Attend Session", 2, 0);
            Step mathForComputing = CreateStep(5, "Mathematics for Computing", "Pass Test", 2, 1);
            Group aiTrack = CreateGroup(6, "AI", GroupRuleType.InOrder, null, 3, 0);
            Group itTrack = CreateGroup(7, "IT", GroupRuleType.InOrder, null, 3, 1);
            Group programmingTrack = CreateGroup(8, "Programming", GroupRuleType.InOrder, null, 3, 2);
            Step finalCapstone = CreateStep(9, "Final Capstone", "Submit Work", 1, 2);
            Group electives = CreateGroup(10, "Electives", GroupRuleType.Choice, 2, 6, 1);
            Step machineLearningBasics = CreateStep(11, "Machine Learning Basics", "Attend Session", 6, 0);
            Step networksAndSecurity = CreateStep(12, "Networks & Security", "Attend Session", 7, 0);
            Step systemsAdministration = CreateStep(13, "Systems Administration", "Pass Test", 7, 1);
            Step algorithmsAndDataStructures = CreateStep(14, "Algorithms & Data Structures", "Pass Test", 8, 0);
            Step softwareEngineering = CreateStep(15, "Software Engineering", "Submit Work", 8, 1);
            Step computerVision = CreateStep(16, "Computer Vision", "Pass Test", 10, 0);
            Step naturalLanguageProcessing = CreateStep(17, "Natural Language Processing", "Pass Test", 10, 1);
            Step robotics = CreateStep(18, "Robotics", "Pass Test", 10, 2);
            Step aiCapstone = CreateStep(19, "AI Capstone", "Submit Work", 6, 2);

            major.PrerequisiteItemId = foundations.Id;
            finalCapstone.PrerequisiteItemId = major.Id;
            aiCapstone.PrerequisiteItemId = electives.Id;

            List<ProgramItem> flatItems = new List<ProgramItem>
        {
            root, foundations, major, introToComputing, mathForComputing,
            aiTrack, itTrack, programmingTrack, finalCapstone, electives,
            machineLearningBasics, networksAndSecurity, systemsAdministration,
            algorithmsAndDataStructures, softwareEngineering, computerVision,
            naturalLanguageProcessing, robotics, aiCapstone
        };

            return flatItems;
        }

        private static Step CreateStep(int id, string name, string stepType, int? parentGroupId, int order)
        {
            Step step = new Step();
            step.Id = id;
            step.Name = name;
            step.StepType = stepType;
            step.ParentGroupId = parentGroupId;
            step.Order = order;

            return step;
        }

        private static Group CreateGroup(int id, string name, GroupRuleType ruleType, int? choiceCount, int? parentGroupId, int order)
        {
            Group group = new Group();
            group.Id = id;
            group.Name = name;
            group.RuleType = ruleType;
            group.ChoiceCount = choiceCount;
            group.ParentGroupId = parentGroupId;
            group.Order = order;
            group.Children = new List<ProgramItem>();

            return group;
        }
    }
}
