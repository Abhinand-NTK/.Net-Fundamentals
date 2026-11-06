namespace SharedLibrary
{
    // 'public' is required so other projects can see this class.
    // An 'internal' class is visible only inside this project.
    public class PriceCalculator
    {
        public decimal AddTax(decimal amount, decimal taxRate) => amount * (1 + taxRate);

        public decimal ApplyDiscount(decimal amount, decimal percent) => amount - amount * percent / 100;
    }
}
