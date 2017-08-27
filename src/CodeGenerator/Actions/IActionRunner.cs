namespace Goliath.Data.CodeGenerator.Actions
{
    interface IActionRunner
    {
        string ActionName { get; }

        void Exetute(AppOptionInfo opts, CodeGenRunner codeGenRunner);
    }
}