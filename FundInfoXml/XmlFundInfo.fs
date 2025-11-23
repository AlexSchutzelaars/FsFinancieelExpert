namespace FundReader

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

    type Fund(provider: string, name: string, description: string, timeSlices: TimeSlice list) =
        member _.Provider = provider
        member _.Name = name
        member _.Description = description
        member _.TimeSlices = timeSlices
        /// Kies meest recente TimeSlice op datum; als geen TimeSlice aanwezig: DateTime.MinValue en 0
        member _.MostRecentTimeSlice =
            match timeSlices with
            | [] -> TimeSlice(DateTime.MinValue, 0M, 0M)
            | _ -> timeSlices |> List.maxBy (fun ts -> ts.Date)
        /// Som van alle values (useful als meerdere posities)
        member _.TotalValue = timeSlices |> List.sumBy (fun ts -> ts.Value)
        override this.ToString() =
            sprintf "%s | %s | %s | slices=%d | total=%M" provider name description (List.length timeSlices) (this.TotalValue)

    /// Container/repository
    type FundRepository (funds: Fund list) =
        member _.Funds = funds
        member _.GetByProvider(provider: string) =
            funds |> List.filter (fun f -> String.Equals(f.Provider, provider, StringComparison.OrdinalIgnoreCase))
        member _.TotalValue() =
            funds |> List.sumBy (fun f -> f.TotalValue)
        override _.ToString() =
            sprintf "FundRepository with %d funds (total value: %M)" (List.length funds) (funds |> List.sumBy (fun f -> f.TotalValue))

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

    /// Parse één TimeSlice XElement naar TimeSlice option (None als onbruikbaar)
    let private parseTimeSlice (tsElem: XElement) : TimeSlice option =
        let dateOpt = tryParseDateInvariant (getChildValue tsElem "Date")
        let howManyOpt = tryParseDecimalInvariant (getChildValue tsElem "HowMany")
        let priceOpt = tryParseDecimalInvariant (getChildValue tsElem "Price")
        match dateOpt, howManyOpt, priceOpt with
        | Some d, Some h, Some p -> Some (TimeSlice(d, h, p))
        | _ -> None

    /// Core parser: retourneert Fund list en biedt 2 helper loaders:
    ///  - LoadFromFileChooseMostRecent: per fund enkel de meest recente timeslice gebruikt (in Fund.MostRecentTimeSlice)
    ///  - LoadFromFileSumSlices: per fund alle timeslices ingelezen en opgeteld in Fund.TotalValue
    let private loadFundsGeneric (path: string) : Fund list =
        let xdoc = XDocument.Load(path)
        let root = xdoc.Root
        if isNull root then []
        else
            let fundElems = root.Elements(XName.Get("fund"))
            let parseFund (fe: XElement) =
                let provider = getChildValue fe "Provider" |> Option.defaultValue ""
                let name = getChildValue fe "Name" |> Option.defaultValue ""
                let description = getChildValue fe "Description" |> Option.defaultValue ""
                // alle TimeSlice child elementen
                let tsElems = fe.Elements(XName.Get("TimeSlice")) |> Seq.toList
                let parsedTs = tsElems |> List.choose parseTimeSlice
                Fund(provider, name, description, parsedTs)
            fundElems |> Seq.map parseFund |> Seq.toList

    /// Public loaders
    let LoadFromFile (path: string) : FundRepository =
        FundRepository(loadFundsGeneric path)

    /// Convenience: identiek aan LoadFromFile, maar naam die expliciet laat zien dat meerdere slices ondersteund worden
    let LoadFromFileSumSlices = LoadFromFile

    /// Convenience: maak een repository waarin elke fund slechts één timeslice heeft: de meest recente (voor situaties waar je enkel de laatste snapshot wilt)
    let LoadFromFileChooseMostRecent (path: string) : FundRepository =
        let funds = loadFundsGeneric path
                    |> List.map (fun f ->
                        let most = f.MostRecentTimeSlice
                        Fund(f.Provider, f.Name, f.Description, [most]))
        FundRepository(funds)

    /// Berekent de totale waarde voor een fonds op basis van de laatst beschikbare datum <= asOf.
    /// - zoekt alle TimeSlices voor alle providers met Date <= asOf
    /// - vindt de maximale datum (latestDate)
    /// - telt alleen de TimeSlices op met laatste Date <= asOf
    /// Voorbeeld:
    /// ```fsharp
    /// let repo = LoadFromFileSumSlices @"C:\Pad\Naar\FundInfoAlleBanken.xml"
    /// let peildatum = DateTime(2025, 8, 31)
    /// let totaal = TotalValueForFundAsOfDate repo "ASN AandelenFonds" peildatum
    /// printfn "Totale waarde voor ASN AandelenFonds (laatste datum ≤ %O) = %M" peildatum totaal
    let TotalValueForFundAsOfDate (repo: FundRepository) (fundName: string) (asOf: DateTime) : decimal =
            // verzamel alle TimeSlices voor het fonds (case-insensitive naam) met Date <= asOf
        let relevantSlices =
                repo.Funds
                |> List.filter (fun f -> String.Equals(f.Name, fundName, StringComparison.OrdinalIgnoreCase))
                |> List.collect (fun f -> f.TimeSlices)
                |> List.filter (fun ts -> ts.Date <= asOf)

        match relevantSlices with
            | [] -> 0M
            | _ ->
                let latestDate = relevantSlices |> List.maxBy (fun ts -> ts.Date) |> fun ts -> ts.Date
                relevantSlices
                |> List.filter (fun ts -> ts.Date = latestDate)
                |> List.sumBy (fun ts -> ts.Value)

