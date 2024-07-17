using LeptonViewerWPF;
using System.Windows.Input;

public class StartStreamingCommand : ICommand
{
    MainViewModel _viewModel;

    public StartStreamingCommand(MainViewModel viewModel)
    {
        _viewModel = viewModel;
    }
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        if (_viewModel.LeptonClass == null)
            return;

        _viewModel.LeptonClass.StartStreaming();
    }
}

public class StopStreamingCommand : ICommand
{
    MainViewModel _viewModel;

    public StopStreamingCommand(MainViewModel viewModel)
    {
        _viewModel = viewModel;
    }
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        if (_viewModel.LeptonClass == null)
            return;

        _viewModel.LeptonClass.StopStreaming();
    }
}

public class NormalizeCommand : ICommand
{
    MainViewModel _viewModel;

    public NormalizeCommand(MainViewModel viewModel)
    {
        _viewModel = viewModel;
    }
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        if (_viewModel.LeptonClass == null)
            return;

        _viewModel.LeptonClass.Normalize();
    }
}