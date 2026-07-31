using ProgramDesigner.Domain;
using ProgramDesigner.Domain.ValueObjects;
using ProgramDesigner.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;    
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Infrastructure.SeedDataProvider
{
    public static class ComputerScienceSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            bool alreadySeeded = await context.LearningPrograms.AnyAsync();
            if (alreadySeeded)
            {
                return;
            }

            Group foundations = Group.Create("Foundations", GroupRule.InOrder());
            Step introToComputing = Step.Create("Introduction to Computing", "Attend Session");
            Step mathForComputing = Step.Create("Mathematics for Computing", "Pass Test");
            foundations.AddChild(introToComputing);
            foundations.AddChild(mathForComputing);

            Group electives = Group.Create("Electives", GroupRule.Choice(2));
            Step computerVision = Step.Create("Computer Vision", "Pass Test");
            Step naturalLanguageProcessing = Step.Create("Natural Language Processing", "Pass Test");
            Step robotics = Step.Create("Robotics", "Pass Test");
            electives.AddChild(computerVision);
            electives.AddChild(naturalLanguageProcessing);
            electives.AddChild(robotics);

            Step aiCapstone = Step.Create("AI Capstone", "Submit Work");
            aiCapstone.AddPrerequisite(electives);

            Group aiTrack = Group.Create("AI", GroupRule.InOrder());
            Step machineLearningBasics = Step.Create("Machine Learning Basics", "Attend Session");
            aiTrack.AddChild(machineLearningBasics);
            aiTrack.AddChild(electives);
            aiTrack.AddChild(aiCapstone);

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

            Group major = Group.Create("Major", GroupRule.Choice(1));
            major.AddChild(aiTrack);
            major.AddChild(itTrack);
            major.AddChild(programmingTrack);
            major.AddPrerequisite(foundations);

            Step finalCapstone = Step.Create("Final Capstone", "Submit Work");
            finalCapstone.AddPrerequisite(major);

            Group root = Group.Create("Computer Science", GroupRule.InOrder());
            root.AddChild(foundations);
            root.AddChild(major);
            root.AddChild(finalCapstone);

            LearningProgram program = LearningProgram.Create("Computer Science", root);

            await context.LearningPrograms.AddAsync(program);
            await context.SaveChangesAsync();
        }
    }
}
