using System.Collections.ObjectModel;
using System.Reflection.Metadata;

namespace Kalkulator;

public partial class MainPage : ContentPage
{
    public CalculationChain calculation { get; private set; }
    private ObservableCollection<HistoryContainer> Histories { get; set; } = new ObservableCollection<HistoryContainer>();
    public MainPage()
    {
        calculation = new CalculationChain(ChngeText);
        InitializeComponent();
        //historyList.ItemsSource = Histories;
    }

    void ChngeText(string text, string equals)
    {
        result.Text = text;
    }


    private void Display(Button sender, int number)
    {

    }

    public void NewHistorySelected(HistoryContainer historyContainer)
    {

    }

    public void OnHistory(object sender, EventArgs e) {
        Window secondWindow = new Window(new HistoryPage(this, Histories));
        Application.Current.OpenWindow(secondWindow);
    }

    private void Click_0(object sender, EventArgs e) => calculation.InputNumber(0);
    private void Click_1(object sender, EventArgs e) => calculation.InputNumber(1);
    private void Click_2(object sender, EventArgs e) => calculation.InputNumber(2);
    private void Click_3(object sender, EventArgs e) => calculation.InputNumber(3);
    private void Click_4(object sender, EventArgs e) => calculation.InputNumber(4);
    private void Click_5(object sender, EventArgs e) => calculation.InputNumber(5);
    private void Click_6(object sender, EventArgs e) => calculation.InputNumber(6);
    private void Click_7(object sender, EventArgs e) => calculation.InputNumber(7);
    private void Click_8(object sender, EventArgs e) => calculation.InputNumber(8);
    private void Click_9(object sender, EventArgs e) => calculation.InputNumber(9);

    private void Click_Add(object sender, EventArgs e) => calculation.InputOperation(CalculationChain.CalculationBlock.OperationBlock.CalcOperation.ADD);
    private void Click_Sub(object sender, EventArgs e) => calculation.InputOperation(CalculationChain.CalculationBlock.OperationBlock.CalcOperation.SUB);
    private void Click_Mlt(object sender, EventArgs e) => calculation.InputOperation(CalculationChain.CalculationBlock.OperationBlock.CalcOperation.MLT);
    private void Click_Div(object sender, EventArgs e) => calculation.InputOperation(CalculationChain.CalculationBlock.OperationBlock.CalcOperation.DIV);
    private void Click_Eqls(object sender, EventArgs e) => calculation.InputEquals();
    private void Click_Back(object sender, EventArgs e) => calculation.TurnBack();

    

}



