namespace ProgramDesigner.Domain.Services
{
    public interface IPrerequisiteValidationService
    {
        Results.ValidationResult Validate(LearningProgram program);
    }
}