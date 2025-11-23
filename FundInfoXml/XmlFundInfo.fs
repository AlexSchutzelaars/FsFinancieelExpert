namespace XmlFundLoader

open System
open System.Globalization
open System.Xml.Linq

module XmlFundLoader =

    /// Modelklassen
    type TimeSlice(date: DateTime, howMany: decimal, price: decimal) =
        member _.Date = date
        member _.HowMany = howMany
        member _.Price = price
        member _.Value = howMany * price
        override this.ToString() =
            sprintf "Date=%O; HowMany=%M; Price=%M; Value=%M" this.Date this.HowMany this.Price this.Value

    type Fund(provider: string, name: string, description: string, timeslice: TimeSlice) =
        member _.Provider = provider
        member _.Name = name
        member _.Description = description
        member _.TimeSlice = timeslice
        override this.ToString() =
            sprintf "%s | %s | %s | %s" provider name description (timeslice.ToString())

    /// Container/repository
    type FundRepository (funds: Fund list) =
        member _.Funds = funds
        member _.GetByProvider(provider: string) =
            funds |> List.filter (fun f -> String.Equals(f.Provider, provider, StringComparison.OrdinalIgnoreCase))
        member _.TotalValue() =
            funds |> List.sumBy (fun f -> f.TimeSlice.Value)
        override _.ToString() =
            sprintf "FundRepository with %d funds (total value: %M)" (List.length funds) (funds |> List.sumBy (fun f -> f.TimeSlice.Value))

    /// Helper: veilige elementleesfunctie
    let private getChildValue (parent: XElement) (name: string) : string option =
        if isNull parent then None
        else
            let e = parent.Element(XName.Get(name))
            if isNull e then None else Some e.Value

    /// Parsers met InvariantCulture
    let private parseDecimalInvariant (s: string) =
        Decimal.Parse(s, NumberStyles.AllowDecimalPoint ||| NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture)

    let private tryParseDecimalInvariant (sOpt: string option) =
        match sOpt with
        | Some s when not (String.IsNullOrWhiteSpace s) ->
            try Some (parseDecimalInvariant s) with _ -> None
        | _ -> None

    let private tryParseDateInvariant (sOpt: string option) =
        match sOpt with
        | Some s when not (String.IsNullOrWhiteSpace s) ->
            try Some (DateTime.Parse(s, CultureInfo.InvariantCulture)) with _ -> None
        | _ -> None

    /// Core: laad en parse XML naar FundRepository
    let LoadFromFile (path: string) : FundRepository =
        let xdoc = XDocument.Load(path)
        let root = xdoc.Root
        if isNull root then
            FundRepository([])
        else
            let fundElems = root.Elements(XName.Get("fund"))
            let parseFund (fe: XElement) =
                let provider = getChildValue fe "Provider" |> Option.defaultValue ""
                let name = getChildValue fe "Name" |> Option.defaultValue ""
                let description = getChildValue fe "Description" |> Option.defaultValue ""
                let tsElem = fe.Element(XName.Get("TimeSlice"))
                let date = tryParseDateInvariant (if isNull tsElem then None else getChildValue tsElem "Date") |> Option.defaultValue DateTime.MinValue
                let howMany = tryParseDecimalInvariant (if isNull tsElem then None else getChildValue tsElem "HowMany") |> Option.defaultValue 0M
                let price = tryParseDecimalInvariant (if isNull tsElem then None else getChildValue tsElem "Price") |> Option.defaultValue 0M
                let ts = TimeSlice(date, howMany, price)
                Fund(provider, name, description, ts)
            let funds = fundElems |> Seq.map parseFund |> Seq.toList
            FundRepository(funds)

    /// Voorbeeldgebruik (commentaar: zet pad naar jouw bestand)
    (*
    let repo = XmlFundLoader.LoadFromFile @"C:\pad\naar\FundInfoAlleBanken.xml"
    printfn "%O" repo
    repo.Funds |> List.iter (fun f -> printfn "%s -> value: %M" f.Name f.TimeSlice.Value)
    *)