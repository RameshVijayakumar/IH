namespace Paycor.Import.Validator
{
    public interface IValidator<in TInput>
    {
        bool Validate(TInput input, out string errorMessage);
    }
}