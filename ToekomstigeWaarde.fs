module ToekomstigeWaarde
open System.Windows.Forms
open System.Globalization
open System
open System.Drawing

// Definieer het type voor resp. post- en prenumerando
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
// Map de frequentie enum naar een deel- en vermenigvuldiginsfactor voor berekeningen
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

// Renteberekening met e^(r * t)
// berekenToekomstwaarde 1000 0.05 3.0 ==> 1163.83
let berekenToekomstwaardeMetEulersGetal (hoofdsom: float) (rentePerunage: float) (tijd: float) : float =
    hoofdsom * Math.Exp (rentePerunage * tijd)

// Bereken de toekomstige waarde met periodieke betalingen.
// Zelfde signatuur als de gelijknamige Excel functie (0 = Postnumerando = default)
// Alleen voor discrete rentebijschrijvingen (dus niet continu = e-macht)
// groeifactor =  1 + rentePerunage
// Voorbeelden:
// TW(0.03;50;6000; 0; 0) = € 676.781,20. Postnumerando
// TW(0.03;50;6000; 0; 1) = € 697.084,64. Prenumerando
// TODO: rekenen met negatieve betalingen (inleg) en positieve (opnames)
let TW (rentePerunage: float) (aantalTermijnen: int) (bet: float) (hw: float) (pXNumerando: PXNumerando) : float =
    let mutable resultaat: float = 0.0
    if rentePerunage = 0.0 then
        resultaat <- -1.0*(hw + bet * float aantalTermijnen)
    else
        let twFactor = Math.Pow(1.0 + rentePerunage, aantalTermijnen)
        // Eenmalige investering (hw) wordt altijd aan het begin van de periode gedaan
        resultaat <- -1.0*(hw * twFactor + bet * ((twFactor - 1.0) / rentePerunage))
        if (pXNumerando = PXNumerando.Pre) then
            resultaat <- resultaat * (1.0 + rentePerunage)
    resultaat

let maakToekomstigeWaardeFormulier () =
    let form = new Form(Text = "Berekening van toekomstige waarde (TW)", Width = 640, Height = 360)

    // Label en tekstveld voor Initiële - en periodieke Inleg en Rente
    let labelInlegInitieel = new Label(Text = "Initiële inleg", Top = 10, Left = 30, Width = 100)
    let inputInlegInitieel = new TextBox(Top = 30, Left = 30, Width = 100)

    let labelInlegPeriodiek = new Label(Text = "Inleg (periodiek)", Top = 10, Left = 150, Width = 100)
    let inputInlegPeriodiek = new TextBox(Top = 30, Left = 150, Width = 100)

    let labelRente = new Label(Text = "Rente", Top = 10, Left = 270, Width = 100)
    let inputRente = new TextBox(Top = 30, Left = 270, Width = 100)

        // Label voor Jaren

    let labelJaren = new Label(Text = "Jaren", Top = 60, Left = 30, Width = 100)
    let inputJaren = new TextBox(Top = 80, Left = 30, Width = 100)
    inputJaren.Text <- "1"

    let chkPostnumerando = new CheckBox()
    chkPostnumerando.Text <- "Postnumerando"
    chkPostnumerando.Top <- 60
    chkPostnumerando.Left <- 150
    chkPostnumerando.Width <- 200
    chkPostnumerando.Checked <- true

    // Label en ListBox voor Frequentie
    let labelInlegFrequentie = new Label(Text = "Inleg-frequentie", Top = 110, Left = 30, Width = 200)

    let listBoxInlegfrequentie = new ListBox()
    listBoxInlegfrequentie.Top <- 130
    listBoxInlegfrequentie.Left <- 30
    listBoxInlegfrequentie.Width <- 200
    listBoxInlegfrequentie.Height <- 80

    listBoxInlegfrequentie.DisplayMember <- "Naam"
    listBoxInlegfrequentie.ValueMember <- "Waarde" // optioneel, maar handig

    let labelResultaat = new Label(Text = "Berekende waarde (TW)", Top = 220, Left = 30, Width = 200)
    let textResult = new TextBox(Top = 240, Left = 30, Width = 200, ReadOnly = true)

    let btnBereken = new Button(Text = "Bereken", Top = 280, Left = 30, Width = 100)
    let btnTerug = new Button(Text = "Terug naar financieel hoofdmenu", Top = 280, Left = 200, Width = 200)
    let btnVoorbeeldTw = new Button(Text = "Voorbeeld-data", Top = 280, Left = 420, Width = 100, BackColor = Color.LightGreen )

    // Event handler voor knop Berekenen (click)
    btnBereken.Click.Add(fun _ ->
        let successInitieel, inlegInitieel = Double.TryParse(inputInlegInitieel.Text)
        let successInlegPeriodiek, _ = Double.TryParse(inputInlegPeriodiek.Text)
        let successRente, rente = Double.TryParse(inputRente.Text)
        let selectedIndex = listBoxInlegfrequentie.SelectedIndex

        if (successInitieel || successInlegPeriodiek) && successRente && selectedIndex >= 0 then
            let geselecteerd = listBoxInlegfrequentie.SelectedItem :?> FinMutatieFrequentieItem
            let frequentieFactor = geselecteerd.Waarde

            let aantaljaren = Convert.ToInt32(inputJaren.Text)
            let inlegPeriodiek = 
                match Double.TryParse(inputInlegPeriodiek.Text) with
                | (true, value) -> value
                | _ -> 0.0

            // Postnumerando (0) of Prenumerando (1)

            let pXNumerando = if chkPostnumerando.Checked then PXNumerando.Post else PXNumerando.Pre
            let mutable resultaat = 0.0
            let mutable aantalTijdeenheden = 0
            let mutable rentePerunage = 0.0

            if frequentieFactor = FinMutatieFrequentie.Continu then
            // TODO: bij berekening van periodieke inleg rekening houden met pre- of postnumerando
                rentePerunage <- rente / 100.0
                aantalTijdeenheden <- aantaljaren

                let mutable inlegPeriodiekToekomstwaarde = 0.0
                // formule voor de toekomstige waarde van periodieke inleg bij continue rente:

                for tijdPunt in 1 .. aantaljaren do
                    // Elke inlegPeriodiek wordt ingelegd op tijdstip 'tijdPunt', en groeit dan nog

                    let waarde = berekenToekomstwaardeMetEulersGetal inlegPeriodiek rentePerunage (float(aantaljaren - tijdPunt))
                    // let waarde = inlegPeriodiek * Math.Exp(rentePerunage * (float(aantaljaren - tijdPunt)))
                    inlegPeriodiekToekomstwaarde <- inlegPeriodiekToekomstwaarde + waarde
                    
                let resultaatInlegInitieel = berekenToekomstwaardeMetEulersGetal inlegInitieel rentePerunage aantalTijdeenheden
                resultaat <- resultaatInlegInitieel + inlegPeriodiekToekomstwaarde
             else
                rentePerunage <- (rente / 100.0) / float (mapFrequentieNaarGetal frequentieFactor)
                aantalTijdeenheden <- aantaljaren * mapFrequentieNaarGetal frequentieFactor
                resultaat <- TW (rentePerunage) aantalTijdeenheden (-inlegPeriodiek) (-inlegInitieel) pXNumerando
            
        // Gebruik de huidige systeemcultuur
            let systeemCultuur = CultureInfo.CurrentCulture.Clone() :?> CultureInfo
            // Stel de gewenste scheidingstekens in voor valuta
            systeemCultuur.NumberFormat.CurrencyGroupSeparator <- "."
            systeemCultuur.NumberFormat.CurrencyDecimalSeparator <- ","
            systeemCultuur.NumberFormat.CurrencySymbol <- "€"
            let formattedResultaat = resultaat.ToString("C", systeemCultuur)
            textResult.Text <- formattedResultaat
        else
            MessageBox.Show("Voer geldige getallen in en kies de frequentie voor de rentebijschrijving.") |> ignore
    )

    btnTerug.Click.Add(fun _ -> form.Close())
    // Voorbeeld TW(0.03;50;6000; 0; 0) = € € 676.781,20. Postnumerando
    // Voorbeeld TW(0.03;50;6000; 0; 1) = € 697.084,64. Prenumerando
    // Zie Basisboek wiskunde en financiële berekeningen, 2020, hoofdstuk 4.9, p.161
    btnVoorbeeldTw.Click.Add(fun _ ->
        inputInlegInitieel.Text <- "0"
        inputInlegPeriodiek.Text <- "6000"
        inputRente.Text <- "3"
        inputJaren.Text <- "50"
        listBoxInlegfrequentie.SelectedIndex <- int(FinMutatieFrequentie.Jaarlijks) - 1
        chkPostnumerando.Checked <- true
    )

    // Voeg alle elementen toe aan het formulier
    form.Controls.Add(labelInlegInitieel)
    form.Controls.Add(inputInlegInitieel)

    form.Controls.Add(labelInlegPeriodiek)
    form.Controls.Add(inputInlegPeriodiek)

    form.Controls.Add(labelRente)
    form.Controls.Add(inputRente)

    form.Controls.Add(labelJaren)
    form.Controls.Add(inputJaren)

    form.Controls.Add(labelInlegFrequentie)
    form.Controls.Add(listBoxInlegfrequentie)
    form.Controls.Add(chkPostnumerando)
    form.Controls.Add(labelResultaat)
    form.Controls.Add(textResult)
    form.Controls.Add(btnBereken)
    form.Controls.Add(btnTerug)
    form.Controls.Add(btnVoorbeeldTw)


    listBoxInlegfrequentie.DataSource <- financieMutatieFrequentieItems
    listBoxInlegfrequentie.SelectedIndex <- int(FinMutatieFrequentie.Jaarlijks) - 1
    form
