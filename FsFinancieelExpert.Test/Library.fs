namespace FsFinancieelExpert.Test

open Xunit
open System
//open ToekomstigeWaarde

module Tests =

    [<Fact>]
    let ``Eindwaarde van 1000 euro na 10 jaar tegen 5% is correct`` () =
        let resultaat = 10.0
        Assert.Equal(1628.89, Math.Round(resultaat, 2))

