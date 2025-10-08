namespace CsFinancieelExpert
{
    internal class Program
    {
        static void Main()
        {
            // Testprogrammatje voor berekenen van de toekomstige waarde
            // Zie ook: https://www.accountingtools.com/articles/what-is-the-formula-for-the-future-value-of-an-annuity-due.html?rq=future%20value
            // =TW(5%/12; 120; -50; -1000; 0)

            double principal = 1000;       // Beginwaarde
            double monthlyDeposit = 50;    // Maandelijkse inleg
            double annualRate = 0.05;      // Jaarlijkse rente
            int months = 120;              // Looptijd in maanden

            double monthlyRate = annualRate / 12;

            // Groei van de hoofdsom
            double futureValuePrincipal = principal * Math.Pow(1 + monthlyRate, months);

            // Groei van de maandelijkse inleg
            double futureValueMonthlyDeposit = 0;
            for (int i = 1; i <= months; i++)
            {
                futureValueMonthlyDeposit += monthlyDeposit * Math.Pow(1 + monthlyRate, months - i);
            }

            double totalFutureValue = futureValuePrincipal + futureValueMonthlyDeposit;

            // I.v.m. het weergeven van het EURO-teken.
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine($"Hoofdsom groeit naar: € {futureValuePrincipal:F2}");
            Console.WriteLine($"Inleg groeit naar: € {futureValueMonthlyDeposit:F2}");
            Console.WriteLine($"Totale toekomstige waarde: € {totalFutureValue:F2}");
        }
    }
}
