namespace FsFinancieelRekenen

module RekenUtils =

        // Definieer het type voor resp. post- en prenumerando.
        // (in Excel is 0 = Postnumerando, 1 = Prenumerando)
    [<RequireQualifiedAccess>]
    type PXNumerando =
        | Post = 0
        | Pre = 1
