namespace FsFinancieelExpert.Test

open System
open Xunit
open FsFinancieelRekenen

type ToekomstwaardeTests() =

    [<Fact>]
    /// Test de berekening van de toekomstige waarde met e^(r.t). In Excel is dit:
    /// =1000*EXP(0.05*10)

    member _.``Eindwaarde van 1000 euro na 10 jaar tegen 5% is correct`` () =
        let resultaat = TwRekenUtils.berekenToekomstigewaardeMetEulersGetal 1000.0 0.05 10.0
        Assert.Equal(1648.72, Math.Round(resultaat, 2))
