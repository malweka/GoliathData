namespace Goliath.Data.CodeGenerator
{
    /// <summary>   Interface for action runner. </summary>
    public interface IActionRunner
    {
        /// <summary>   Gets the name of the action. </summary>
        ///
        /// <value> The name of the action. </value>
        string ActionName { get; }

        /// <summary>   Executes the command. </summary>
        ///
        /// <param name="opts">             Options for controlling the operation. </param>
        /// <param name="codeGenRunner">    The code generate runner. </param>
        void Execute(AppOptionInfo opts, CodeGenRunner codeGenRunner);
    }
}