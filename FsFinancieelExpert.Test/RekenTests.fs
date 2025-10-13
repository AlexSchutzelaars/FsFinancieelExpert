namespace FsFinancieelExpert.Test

open System
open Xunit
open FsFinancieelRekenen

type ToekomstwaardeTests() =

    [<Fact>]
    /// Test de berekening van de toekomstige waarde met e^(r.t). In Excel is dit:
    /// =1000*EXP(0.05*10)

    member _.``Eindwaarde van 1000 euro na 10 jaar tegen 5% continu is correct`` () =
        let resultaat = TwRekenUtils.BerekenToekomstigewaardeMetEulersGetal 1000.0 0.05 10.0
        Assert.Equal(1648.72, Math.Round(resultaat, 2))

    [<Fact>]
        /// Test de berekening van de toekomstige waarde met periodieke betalingen achteraf zonder
        /// initiële hoofdsom.
        /// In Excel is dit :   
        /// TW(0.03;50;-6000; 0; 0) = € 676.781,20. Postnumerando
        /// TODO: waarde van 6000 negatief maken in de test?
    member _.``Eindwaarde na 50 jaar van periodieke inleg 6000 euro achteraf, zonder hoofdsom tegen 3% jaarlijks`` () =
        let resultaat = TwRekenUtils.TW 0.03 50 6000 0.0 Common.PXNumerando.Post
        Assert.Equal(676781.20, Math.Round(resultaat, 2))