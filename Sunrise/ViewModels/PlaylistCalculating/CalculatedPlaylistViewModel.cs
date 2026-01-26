using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Model;
using Sunrise.Views;

namespace Sunrise.ViewModels;

public class CalculatedPlaylistViewModel : ObservableObject
{
    private string _name;
    private PlaylistTermRuleViewModel _selectedTermRule;
    private PlaylistSortingRuleViewModel _selectedSortingRule;
    private int _maxTracks;
    private PlaylistCalculatedData? _calculatedData;

    public CalculatedPlaylistViewModel() { } // For designer

    public CalculatedPlaylistViewModel(Player player)
    {
        AddTermCommand = new RelayCommand(OnAddTerm);
        DeleteTermCommand = new RelayCommand(OnDeleteTerm, CanDeleteTerm);
        AddSortingCommand = new RelayCommand(OnAddSorting);
        DeleteSortingCommand = new RelayCommand(OnDeleteSorting, CanDeleteSorting);
        OkCommand = new RelayCommand(OnOk);
        CancelCommand = new RelayCommand(OnCancel);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public IRelayCommand AddTermCommand { get; }

    public IRelayCommand DeleteTermCommand { get; }

    public ObservableCollection<PlaylistTermRuleViewModel> TermRules { get; } = [];

    public PlaylistTermRuleViewModel SelectedTermRule
    {
        get => _selectedTermRule;
        set => SetProperty(ref _selectedTermRule, value);
    }

    public IRelayCommand AddSortingCommand { get; }

    public IRelayCommand DeleteSortingCommand { get; }

    public ObservableCollection<PlaylistSortingRuleViewModel> SortingRules { get; } = [];

    public PlaylistSortingRuleViewModel SelectedSortingRule
    {
        get => _selectedSortingRule;
        set => SetProperty(ref _selectedSortingRule, value);
    }

    public int MaxTracks
    {
        get => _maxTracks;
        set => SetProperty(ref _maxTracks, value);
    }

    public IRelayCommand OkCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public ICalculatedPlaylistView View { get; set; }

    private void OnAddTerm()
    {
        var termRuleViewModel = new PlaylistTermRuleViewModel();
        TermRules.Add(termRuleViewModel);
        SelectedTermRule = termRuleViewModel;
    }

    private bool CanDeleteTerm() => _selectedTermRule is not null;

    private void OnDeleteTerm()
    {
        var termRuleViewModel = _selectedTermRule;

        if (termRuleViewModel is not null)
            TermRules.Remove(termRuleViewModel);
    }

    private void OnAddSorting()
    {
        var sortingRuleViewModel = new PlaylistSortingRuleViewModel();
        SortingRules.Add(sortingRuleViewModel);
        SelectedSortingRule = sortingRuleViewModel;
    }

    private bool CanDeleteSorting() => _selectedSortingRule is not null;

    private void OnDeleteSorting()
    {
        var sortingRuleViewModel = _selectedSortingRule;

        if (sortingRuleViewModel is not null)
            SortingRules.Remove(sortingRuleViewModel);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SelectedTermRule))
            DeleteTermCommand.NotifyCanExecuteChanged();
        else if (e.PropertyName == nameof(SelectedSortingRule))
            DeleteSortingCommand.NotifyCanExecuteChanged();
    }

    private void OnOk()
    {
        if (TermRules.Count == 0)
            return;

        _calculatedData = new()
        {
            TermRules = [],
            SortingRules = [],
            MaxTracks = MaxTracks,
        };

        foreach (var termRuleViewModel in TermRules)
        {
            var rule = new PlaylistTermRule()
            {
                Parameter = termRuleViewModel.Parameter,
                Operator = termRuleViewModel.Operator,
                Value = termRuleViewModel.Value,
            };

            _calculatedData.TermRules.Add(rule);
        }

        foreach (var sortingRuleViewModel in SortingRules)
        {
            var rule = new PlaylistSortingRule()
            {
                Parameter = sortingRuleViewModel.Parameter,
                Sorting = sortingRuleViewModel.Sorting,
            };

            _calculatedData.SortingRules.Add(rule);
        }

        View?.Close();
    }

    private void OnCancel() => View?.Close();

    public static async Task<(string name, PlaylistCalculatedData? calculatedData)> ShowAsync(Window owner, Player player, CancellationToken token)
    {
        var viewModel = new CalculatedPlaylistViewModel(player);
        var dialog = new CalculatedPlaylistWindow() { DataContext = viewModel };
        viewModel.View = dialog;
        await dialog.ShowDialog(owner);
        return (viewModel._name, viewModel._calculatedData);
    }

}
