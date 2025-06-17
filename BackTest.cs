
using System.Drawing;

public class BackTest {

    private readonly MomentumSignal MomentumSignal;

    public BackTest(MomentumSignal signal) {
        this.MomentumSignal = signal;
    }

    public decimal TradeWith(decimal startingCash, decimal salesTaxRate) {
        
        var cash = startingCash;
        var units = 0;
        for (var i = 0; i < this.MomentumSignal.Series.Count; i++) {
            var today = this.MomentumSignal.Series[i];
            var price = today.Day.Average;

            // Buy
            if (today.Signal == Signal.Buy) {
                var unitsBought = (int)(cash / price);
                var remainingCash = cash % price;
                units += unitsBought;
                cash = remainingCash;
            }

            // Sell
            if (today.Signal == Signal.Sell) {
                var salePrice = units * price;
                var salesTax = salePrice * salesTaxRate;
                cash += salePrice - salesTax;
                units = 0;
            }

        }

        var finalSalesPrice = this.MomentumSignal.Series.Last().Day.Average;
        var finalSale = units * finalSalesPrice;
        cash += finalSale;
        return cash;

    }

}