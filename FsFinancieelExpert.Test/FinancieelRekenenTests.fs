namespace FsFinancieelExpert.Test

open System
open Xunit
open FsFinancieelRekenen

type ToekomstwaardeTests() =

    [<Fact>]
    /// Test de berekening van de toekomstige waarde met e^(r.t). In Excel is dit:
    /// =1000*EXP(0.05*10)

    member _.``Eindwaarde van 1000 euro na 10 jaar tegen 5% continue interest is correct`` () =
        let resultaat = TwRekenUtils.BerekenToekomstigewaardeMetEulersGetal 1000.0 0.05 10.0
        Assert.Equal(1648.72, Math.Round(resultaat, 2))

    [<Fact>]
        /// Test de berekening van de toekomstige waarde met periodieke betalingen
        /// achteraf, zonder initiële hoofdsom.
        /// In Excel is dit :   
        /// TW(0.03;50;-6000; 0.0; 0) = € 676.781,20. [0 = Postnumerando]

    member _.``Eindwaarde na 50 jaar van periodieke inleg 6000 euro achteraf, zonder hoofdsom tegen 3% jaarlijks`` () =
        let resultaat = TwRekenUtils.TW 0.03 50 -6000 0.0 Common.PXNumerando.Post
        Assert.Equal(676781.20, Math.Round(resultaat, 2))

    [<Fact>]
        /// Test de berekening van de toekomstige waarde met periodieke betalingen
        /// vooraf, zonder initiële hoofdsom.
        /// In Excel is dit :   
        /// TW(0.03;50;-6000; 0; 1) = € 697.084,64. [1 = Prenumerando]
    member _.``Eindwaarde na 50 jaar van periodieke inleg 6000 euro vooraf, zonder hoofdsom tegen 3% jaarlijks`` () =
        let resultaat = TwRekenUtils.TW 0.03 50 -6000 0.0 Common.PXNumerando.Pre
        Assert.Equal(697084.64, Math.Round(resultaat, 2))

    [<Fact>]
        /// Test de berekening van de toekomstige waarde met enkel een hoofdsom
        /// achteraf, zonder periodieke betalinggen.
        /// In Excel is dit :   
        /// TW(0.05;2;-1000; 0; 0) = € 1102.50. [0 = Postnumerando]
    member _.``Eindwaarde van 1000 euro na 2 jaar tegen 5% interest per periode is correct`` () =
        let resultaat = TwRekenUtils.TW 0.05 2 0.0 -1000.0 Common.PXNumerando.Post
        Assert.Equal(1102.50, Math.Round(resultaat, 2))