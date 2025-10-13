namespace FsFinancieelRekenen

open System

module Common =
    let afronden (waarde: float) (decimalen: int) : float =
        Math.Round(waarde, decimalen)

        // Definieer het type voor resp. post- en prenumerando.
        // (in Excel is 0 = Postnumerando, 1 = Prenumerando)
    [<RequireQualifiedAccess>]
    type PXNumerando =
        | Post = 0
        | Pre = 1

            // Definieer de frequentie als een enum
    type FinMutatieFrequentie =
        | Continu = 1
        | Dagelijks = 2
        | Maandelijks = 3
        | PerKwartaal = 4
        | Halfjaarlijks = 5
        | Jaarlijks = 6

    type FinMutatieFrequentieItem = {
        Naam: string
        Waarde: FinMutatieFrequentie
    }

    let financieMutatieFrequentieItems = 
        [|
            { Naam = "Continu (e^[r.t])"; Waarde = FinMutatieFrequentie.Continu }
            { Naam = "Dagelijks"; Waarde = FinMutatieFrequentie.Dagelijks }
            { Naam = "Maandelijks"; Waarde = FinMutatieFrequentie.Maandelijks }
            { Naam = "Per kwartaal"; Waarde = FinMutatieFrequentie.PerKwartaal }
            { Naam = "Halfjaarlijks"; Waarde = FinMutatieFrequentie.Halfjaarlijks }
            { Naam = "Jaarlijks"; Waarde = FinMutatieFrequentie.Jaarlijks }
        |]

    let mapFrequentieNaarGetal (freq: FinMutatieFrequentie) =
    // Map de frequentie enum naar een deel- en vermenigvuldigingsfactor voor berekeningen
    // Continu is een speciale case, die apart wordt behandeld
    // De functie ervan is dat de rente gedeeld wordt door deze factor, en het aantal jaren
    // ermee vermenigvuldigd.
         if freq =  FinMutatieFrequentie.Continu then 1
         elif freq =  FinMutatieFrequentie.Dagelijks then 365
         elif freq =  FinMutatieFrequentie.Maandelijks then 12
         elif freq =  FinMutatieFrequentie.PerKwartaal then 4
         elif freq =  FinMutatieFrequentie.Halfjaarlijks then 6
         elif freq =  FinMutatieFrequentie.Jaarlijks then 1
         else 1

module TwRekenUtils =

         // Renteberekening met e^(r * t)
// erekenToekomstwaardeMetEulersGetal 1000 0.05 3.0 ==> 1163.83
    let BerekenToekomstigewaardeMetEulersGetal (hoofdsom: float) (rentePerunage: float) (tijd: float) : float =
        hoofdsom * Math.Exp (rentePerunage * tijd)

    // Bereken de toekomstige waarde met periodieke betalingen.
    // Zelfde signatuur als de gelijknamige Excel functie (0 = Postnumerando = default)
    // Alleen voor discrete rentebijschrijvingen (dus niet continu = e-macht).
    // rentePerunage = rente per tijdeenheid (bijv. per maand, kwartaal, jaar)
    // aantalTermijnen = totaal aantal tijdeenheden (bijv. maanden, kwartalen, jaren)
    // bet = periodieke betaling (negatief voor stortingen, positief voor opnamen)
    // groeifactor =  1 + rentePerunage
    // Voorbeelden (spaarplan) in Excel:
    // TW(0.03;50;-6000; 0; 0) = € 676.781,20. Postnumerando
    // TW(0.03;50;-6000; 0; 1) = € 697.084,64. Prenumerando
    // TODO: rekenen met positieve bedragen (geldopnamen)
    let TW (interestPerunage: float) (aantalTermijnen: float) (bet: float) (hw: float)
                                    (pXNumerando: Common.PXNumerando) : float =
        if interestPerunage = 0.0 then
            let resultaat = hw + bet * float aantalTermijnen
            resultaat
        else
            let twFactor = Math.Pow(1.0 + interestPerunage, aantalTermijnen)
            // Eenmalige investering (hw) wordt altijd aan het begin van de periode gedaan
            let mutable resultaat = hw * twFactor + bet * ((twFactor - 1.0) / interestPerunage)
            if (pXNumerando = Common.PXNumerando.Pre) then
                resultaat <- resultaat * (1.0 + interestPerunage)
            resultaat