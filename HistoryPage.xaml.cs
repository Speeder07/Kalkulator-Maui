using System.Collections.ObjectModel;

namespace Kalkulator;

public partial class HistoryPage : ContentPage
{
	private ObservableCollection<HistoryContainer> Histories { get; set; } = new ObservableCollection<HistoryContainer>();
    MainPage mainPage;
    public HistoryPage(MainPage mainPage, ObservableCollection<HistoryContainer> Histories)
	{
        this.mainPage = mainPage;
		this.Histories = Histories;
		InitializeComponent();
        historyList.ItemsSource = Histories;
        mainPage.calculation.OnEquals += OnEquals;
    }
    public void OnEquals(HistoryContainer container)
    {
        this.Histories.Add(container);
    }

    private void historyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        HistoryContainer selected = e.CurrentSelection.FirstOrDefault() as HistoryContainer;
        this.mainPage.calculation.LoadHistory(selected);
    }
}