using System.Windows.Input;

namespace X3UR.Infrastructure.Commands;
public class RelayCommand : ICommand {
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
    public bool CanExecute(object _) => true;
    public void Execute(object _) => _execute();
    public event EventHandler CanExecuteChanged;
}