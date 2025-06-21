
public class Order {

    public DateTime Date { get; set; }
    public Signal Signal { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }

    public Order(MomentumDay momentumDay) {
        this.Date = momentumDay.Day.Date;
        this.Signal = momentumDay.Signal;
        this.BuyPrice = momentumDay.Day.Average;
        this.SellPrice = momentumDay.Day.Average;
    }

    public Order(SwingDay swingDay) {
        this.Date = swingDay.Day.Date;
        this.Signal = swingDay.Signal;
        this.BuyPrice = swingDay.Day.Average;
        this.SellPrice = swingDay.Day.Average;
    }

    public Order() { }
    
}