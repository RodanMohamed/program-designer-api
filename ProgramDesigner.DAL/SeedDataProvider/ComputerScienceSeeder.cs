//using ProgramDesigner.Common.Enums;
//using ProgramDesigner.DAL.Data.Context;
//using ProgramDesigner.DAL.Data.Models;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ProgramDesigner.DAL.SeedDataProvider
//{
//    public static class ComputerScienceSeeder
//    {
//        public static async Task SeedAsync(ApplicationDbContext context)
//        {
//            bool alreadySeeded = await context.LearningPrograms.AnyAsync();

//            if (alreadySeeded)
//            {
//                return;
//            }

//            // Foundations [in order]
//            Step introToComputing = CreateStep("Introduction to Computing", "Attend Session", 0);
//            Step mathForComputing = CreateStep("Mathematics for Computing", "Pass Test", 1);

//            Group foundations = CreateGroup("Foundations", GroupRuleType.InOrder, null, 0);
//            foundations.Children.Add(introToComputing);
//            foundations.Children.Add(mathForComputing);

//            // AI track [in order]
//            Step machineLearningBasics = CreateStep("Machine Learning Basics", "Attend Session", 0);

//            Step computerVision = CreateStep("Computer Vision", "Pass Test", 0);
//            Step naturalLanguageProcessing = CreateStep("Natural Language Processing", "Pass Test", 1);
//            Step robotics = CreateStep("Robotics", "Pass Test", 2);

//            Group electives = CreateGroup("Electives", GroupRuleType.Choice, 2, 1);
//            electives.Children.Add(computerVision);
//            electives.Children.Add(naturalLanguageProcessing);
//            electives.Children.Add(robotics);

//            Step aiCapstone = CreateStep("AI Capstone", "Submit Work", 2);
//            aiCapstone.PrerequisiteItem = electives;

//            Group aiTrack = CreateGroup("AI", GroupRuleType.InOrder, null, 0);
//            aiTrack.Children.Add(machineLearningBasics);
//            aiTrack.Children.Add(electives);
//            aiTrack.Children.Add(aiCapstone);

//            // IT track [in order]
//            Step networksAndSecurity = CreateStep("Networks & Security", "Attend Session", 0);
//            Step systemsAdministration = CreateStep("Systems Administration", "Pass Test", 1);

//            Group itTrack = CreateGroup("IT", GroupRuleType.InOrder, null, 1);
//            itTrack.Children.Add(networksAndSecurity);
//            itTrack.Children.Add(systemsAdministration);

//            // Programming track [in order]
//            Step algorithmsAndDataStructures = CreateStep("Algorithms & Data Structures", "Pass Test", 0);
//            Step softwareEngineering = CreateStep("Software Engineering", "Submit Work", 1);

//            Group programmingTrack = CreateGroup("Programming", GroupRuleType.InOrder, null, 2);
//            programmingTrack.Children.Add(algorithmsAndDataStructures);
//            programmingTrack.Children.Add(softwareEngineering);

//            // Major [choice - pick 1 of 3] PREREQUISITE: Foundations
//            Group major = CreateGroup("Major", GroupRuleType.Choice, 1, 1);
//            major.Children.Add(aiTrack);
//            major.Children.Add(itTrack);
//            major.Children.Add(programmingTrack);
//            major.PrerequisiteItem = foundations;

//            // Final Capstone PREREQUISITE: Major
//            Step finalCapstone = CreateStep("Final Capstone", "Submit Work", 2);
//            finalCapstone.PrerequisiteItem = major;

//            // Root: Computer Science [in order]
//            Group root = CreateGroup("Computer Science", GroupRuleType.InOrder, null, 0);
//            root.Children.Add(foundations);
//            root.Children.Add(major);
//            root.Children.Add(finalCapstone);

//            LearningProgram program = new LearningProgram();
//            program.Name = "Computer Science";
//            program.RootGroup = root;

//            await context.LearningPrograms.AddAsync(program);
//            await context.SaveChangesAsync();
//        }

//        private static Step CreateStep(string name, string stepType, int order)
//        {
//            Step step = new Step();
//            step.Name = name;
//            step.StepType = stepType;
//            step.Order = order;

//            return step;
//        }

//        private static Group CreateGroup(string name, GroupRuleType ruleType, int? choiceCount, int order)
//        {
//            Group group = new Group();
//            group.Name = name;
//            group.RuleType = ruleType;
//            group.ChoiceCount = choiceCount;
//            group.Order = order;
//            group.Children = new List<ProgramItem>();

//            return group;
//        }
//    }
//}
