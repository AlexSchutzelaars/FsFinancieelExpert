module ToekomstigeWaarde
open System.Windows.Forms
open System.Globalization
open System
open System.Drawing
open FsFinancieelRekenen



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
// berekenToekomstwaardeMetEulersGetal 1000 0.05 3.0 ==> 1163.83
let berekenToekomstwaardeMetEulersGetal (hoofdsom: float) (rentePerunage: float) (tijd: float) : float =
    hoofdsom * Math.Exp (rentePerunage * tijd)

// Bereken de toekomstige waarde met periodieke betalingen.
// Zelfde signatuur als de gelijknamige Excel functie (0 = Postnumerando = default)
// Alleen voor discrete rentebijschrijvingen (dus niet continu = e-macht).
// rentePerunage = rente per tijdeenheid (bijv. per maand, kwartaal, jaar)
// aantalTermijnen = totaal aantal tijdeenheden (bijv. maanden, kwartalen, jaren)
// bet = periodieke betaling (negatief voor stortingen, positief voor opnamen)
// groeifactor =  1 + rentePerunage
// Voorbeelden (spaarplan):
// TW(0.03;50;-6000; 0; 0) = € 676.781,20. Postnumerando
// TW(0.03;50;-6000; 0; 1) = € 697.084,64. Prenumerando
// TODO: rekenen met positieve bedragen (geldopnamen)
let TW (interestPerunage: float) (aantalTermijnen: float) (bet: float) (hw: float) (pXNumerando: RekenUtils.PXNumerando) : float =
    if interestPerunage = 0.0 then
        let resultaat = hw + bet * float aantalTermijnen
        resultaat
    else
        let twFactor = Math.Pow(1.0 + interestPerunage, aantalTermijnen)
        // Eenmalige investering (hw) wordt altijd aan het begin van de periode gedaan
        let mutable resultaat = hw * twFactor + bet * ((twFactor - 1.0) / interestPerunage)
        if (pXNumerando = RekenUtils.PXNumerando.Pre) then
            resultaat <- resultaat * (1.0 + interestPerunage)
        resultaat

let maakToekomstigeWaardeFormulier () =
    let form = new Form(Text = "Berekening van toekomstige waarde (TW)", Width = 640, Height = 360)

    // Label en tekstveld voor Initiële - en periodieke Inleg en Rente
    let lblInlegInitieel = new Label(Text = "Initiële inleg", Top = 10, Left = 30, Width = 100)
    let txtInlegInitieel = new TextBox(Top = 30, Left = 30, Width = 100)

    let lblInlegPeriodiek = new Label(Text = "Inleg (periodiek)", Top = 10, Left = 150, Width = 100)
    let txtInlegPeriodiek = new TextBox(Top = 30, Left = 150, Width = 100)

    let lblInterest = new Label(Text = "Interest", Top = 10, Left = 270, Width = 100)
    let txtInterest = new TextBox(Top = 30, Left = 270, Width = 100)

        // Label voor Jaren

    let lblJaren = new Label(Text = "Jaren", Top = 60, Left = 30, Width = 100)
    let txtJaren = new TextBox(Top = 80, Left = 30, Width = 100)
    txtJaren.Text <- "1"

    let lblMaanden = new Label(Text = "Maanden", Top = 60, Left = 150, Width = 100)
    let txtMaanden = new TextBox(Top = 80, Left = 150, Width = 100)
    txtMaanden.Text <- ""

    let chkPostnumerando = new CheckBox()
    chkPostnumerando.Text <- "Postnumerando"
    chkPostnumerando.Top <- 60
    chkPostnumerando.Left <- 270
    chkPostnumerando.Width <- 200
    chkPostnumerando.Checked <- true

    // Label en ListBox voor Frequentie
    let lblInlegFrequentie = new Label(Text = "Inleg-frequentie", Top = 110, Left = 30, Width = 200)

    let listBoxInlegfrequentie = new ListBox()
    listBoxInlegfrequentie.Top <- 130
    listBoxInlegfrequentie.Left <- 30
    listBoxInlegfrequentie.Width <- 200
    listBoxInlegfrequentie.Height <- 80

    listBoxInlegfrequentie.DisplayMember <- "Naam"
    listBoxInlegfrequentie.ValueMember <- "Waarde" // optioneel, maar handig

    let lblResultaat = new Label(Text = "Berekende waarde (TW)", Top = 220, Left = 30, Width = 200)
    let txtResult = new TextBox(Top = 240, Left = 30, Width = 200, ReadOnly = true)

    let btnBereken = new Button(Text = "Bereken", Top = 280, Left = 30, Width = 100)
    let btnTerug = new Button(Text = "Terug naar financieel hoofdmenu", Top = 280, Left = 200, Width = 200)
    let btnVoorbeeldTw = new Button(Text = "Voorbeeld-data (Spaarplan)", Top = 280, Left = 420, Width = 200, BackColor = Color.LightGreen )

    // Event handler voor knop Berekenen (click)
    btnBereken.Click.Add(fun _ ->
        let successInitieel, inlegInitieel = Double.TryParse(txtInlegInitieel.Text)
        let successInlegPeriodiek, _ = Double.TryParse(txtInlegPeriodiek.Text)
        let interestConversie = txtInterest.Text.Replace(".", ",")
        let successRente, interest = Double.TryParse(interestConversie)
        let selectedIndex = listBoxInlegfrequentie.SelectedIndex

        if (successInitieel || successInlegPeriodiek) && successRente && selectedIndex >= 0 then
            let geselecteerd = listBoxInlegfrequentie.SelectedItem :?> FinMutatieFrequentieItem
            let frequentieFactor = geselecteerd.Waarde

            let aantaljaren = 
                match Int32.TryParse(txtJaren.Text) with
                | (true, value) -> value
                | _ -> 0

            let aantalmaanden = 
                match Int32.TryParse(txtMaanden.Text) with
                | (true, value) -> value
                | _ -> 0

            let inlegPeriodiek = 
                match Double.TryParse(txtInlegPeriodiek.Text) with
                | (true, value) -> value
                | _ -> 0.0

            // Postnumerando (0) of Prenumerando (1)

            let pXNumerando = if chkPostnumerando.Checked then RekenUtils.PXNumerando.Post else RekenUtils.PXNumerando.Pre
            let mutable resultaat = 0.0
            let mutable aantalTijdeenheden = 0.0
            let mutable interestPerunage = 0.0

            if frequentieFactor = FinMutatieFrequentie.Continu then
                interestPerunage <- interest / 100.0
                aantalTijdeenheden <- float(aantaljaren) + float (aantalmaanden) / 12.0
                let mutable inlegPeriodiekToekomstwaarde = 0.0
                // formule voor de toekomstige waarde van periodieke inleg bij continue rente:
                
                let restantJaarFractie = float aantalTijdeenheden - float aantaljaren
                for tijdPunt in 1 .. aantaljaren do
                    // Elke inlegPeriodiek wordt ingelegd op tijdstip 'tijdPunt', en groeit dan nog

                    let waarde = berekenToekomstwaardeMetEulersGetal inlegPeriodiek interestPerunage (float(aantaljaren - tijdPunt))

                    inlegPeriodiekToekomstwaarde <- inlegPeriodiekToekomstwaarde + waarde
                
                if restantJaarFractie > 0.0 then
                    let waarde = berekenToekomstwaardeMetEulersGetal inlegPeriodiek interestPerunage (restantJaarFractie)
                    inlegPeriodiekToekomstwaarde <- inlegPeriodiekToekomstwaarde + waarde
                
                let resultaatInlegInitieel = berekenToekomstwaardeMetEulersGetal inlegInitieel interestPerunage aantalTijdeenheden
                resultaat <- resultaatInlegInitieel + inlegPeriodiekToekomstwaarde
             else
                interestPerunage <- (interest / 100.0) / float (mapFrequentieNaarGetal frequentieFactor)
                aantalTijdeenheden <- float(aantaljaren * mapFrequentieNaarGetal frequentieFactor)
                if aantalmaanden > 0 then
                    let extraTijdeenheden = (float aantalmaanden / 12.0)
                    aantalTijdeenheden <- aantalTijdeenheden + float extraTijdeenheden
                resultaat <- TW (interestPerunage) aantalTijdeenheden (-inlegPeriodiek) (-inlegInitieel) pXNumerando
            
        // Gebruik de huidige systeemcultuur
            let systeemCultuur = CultureInfo.CurrentCulture.Clone() :?> CultureInfo
            // Stel de gewenste scheidingstekens in voor valuta
            systeemCultuur.NumberFormat.CurrencyGroupSeparator <- "."
            systeemCultuur.NumberFormat.CurrencyDecimalSeparator <- ","
            systeemCultuur.NumberFormat.CurrencySymbol <- "€"
            let formattedResultaat = resultaat.ToString("C", systeemCultuur)
            txtResult.Text <- formattedResultaat
        else
            MessageBox.Show("Voer geldige getallen in en kies de frequentie voor de rentebijschrijving.") |> ignore
    )

    btnTerug.Click.Add(fun _ -> form.Close())
    // Voorbeeld TW(0.03;50;6000; 0; 0) = € € 676.781,20. Postnumerando
    // Voorbeeld TW(0.03;50;6000; 0; 1) = € 697.084,64. Prenumerando
    // Zie Basisboek wiskunde en financiële berekeningen, 2020, hoofdstuk 4.9, p.161
    btnVoorbeeldTw.Click.Add(fun _ ->
        txtInlegInitieel.Text <- "0"
        txtInlegPeriodiek.Text <- "-6000"
        txtInterest.Text <- "3"
        txtJaren.Text <- "50"
        txtMaanden.Text <- "0"
        listBoxInlegfrequentie.SelectedIndex <- int(FinMutatieFrequentie.Jaarlijks) - 1
        chkPostnumerando.Checked <- true
    )

    // Voeg alle elementen toe aan het formulier
    form.Controls.Add(lblInlegInitieel)
    form.Controls.Add(txtInlegInitieel)

    form.Controls.Add(lblInlegPeriodiek)
    form.Controls.Add(txtInlegPeriodiek)

    form.Controls.Add(lblInterest)
    form.Controls.Add(txtInterest)

    form.Controls.Add(lblJaren)
    form.Controls.Add(txtJaren)

    form.Controls.Add(lblMaanden)
    form.Controls.Add(txtMaanden)

    form.Controls.Add(lblInlegFrequentie)
    form.Controls.Add(listBoxInlegfrequentie)
    form.Controls.Add(chkPostnumerando)
    form.Controls.Add(lblResultaat)
    form.Controls.Add(txtResult)
    form.Controls.Add(btnBereken)
    form.Controls.Add(btnTerug)
    form.Controls.Add(btnVoorbeeldTw)

    listBoxInlegfrequentie.DataSource <- financieMutatieFrequentieItems
    listBoxInlegfrequentie.SelectedIndex <- int(FinMutatieFrequentie.Jaarlijks) - 1
    form
