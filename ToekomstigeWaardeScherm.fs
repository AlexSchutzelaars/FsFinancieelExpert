module ToekomstigeWaardeScherm
open System.Windows.Forms
open System.Globalization
open System
open System.Drawing
open FsFinancieelRekenen

let maakToekomstigeWaardeFormulier () =
    let form = new Form(Text = "Berekening van toekomstige waarde (TW)", Width = 640, Height = 360)

    // Label en tekstveld voor Initiële - en periodieke Inleg en Rente
    let lblInlegInitieel = new Label(Text = "Initiële inleg", Top = 10, Left = 30, Width = 100)
    let txtInlegInitieel = new TextBox(Top = 30, Left = 30, Width = 100)

    let lblInlegPeriodiek = new Label(Text = "Inleg (periodiek)", Top = 10, Left = 150, Width = 100)
    let txtInlegPeriodiek = new TextBox(Top = 30, Left = 150, Width = 100)

    let lblInterest = new Label(Text = "Interest", Top = 10, Left = 270, Width = 100)
    let txtInterest = new TextBox(Top = 30, Left = 270, Width = 100)

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

    let lblInlegfrequentie = new Label(Text = "Inleg-frequentie", Top = 110, Left = 30, Width = 200)
    let listBoxInlegfrequentie = new ListBox(Top = 130, Left = 30, Width = 200, Height = 80)

    listBoxInlegfrequentie.DisplayMember <- "Naam"
    listBoxInlegfrequentie.ValueMember <- "Waarde" // optioneel, maar handig

    let lblResultaat = new Label(Text = "Berekende waarde (TW)", Top = 220, Left = 30, Width = 200)
    let txtResult = new TextBox(Top = 240, Left = 30, Width = 200, ReadOnly = true)

    let btnBereken = new Button(Text = "Bereken", Top = 280, Left = 30, Width = 100)
    let btnTerug = new Button(Text = "Naar financieel hoofdmenu", Top = 280, Left = 200, Width = 200)
    let btnVoorbeeldTw = new Button(Text = "Voorbeeld-data (Spaarplan)", Top = 280, Left = 420, Width = 200, BackColor = Color.LightGreen )

    // Event handler voor knop Berekenen
    btnBereken.Click.Add(fun _ ->
        let successInitieel, inlegInitieel = Double.TryParse(txtInlegInitieel.Text)
        let successInlegPeriodiek, _ = Double.TryParse(txtInlegPeriodiek.Text)
        let interestConversie = txtInterest.Text.Replace(".", ",")
        let successRente, interest = Double.TryParse(interestConversie)
        let selectedIndex = listBoxInlegfrequentie.SelectedIndex

        if (successInitieel || successInlegPeriodiek) && successRente && selectedIndex >= 0 then
            let geselecteerd = listBoxInlegfrequentie.SelectedItem :?> Common.FinMutatieFrequentieItem
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

            let pXNumerando =   if chkPostnumerando.Checked then Common.PXNumerando.Post
                                else Common.PXNumerando.Pre
            let mutable resultaat = 0.0
            let mutable aantalTijdeenheden = 0.0
            let mutable interestPerunage = 0.0

            if frequentieFactor = Common.FinMutatieFrequentie.Continu then
                interestPerunage <- interest / 100.0
                aantalTijdeenheden <- float(aantaljaren) + float (aantalmaanden) / 12.0
                let mutable inlegPeriodiekToekomstwaarde = 0.0
                // formule voor de toekomstige waarde van periodieke inleg bij continue rente:
                
                let restantJaarFractie = float aantalTijdeenheden - float aantaljaren
                for tijdPunt in 1 .. aantaljaren do
                    // Elke inlegPeriodiek wordt ingelegd op tijdstip 'tijdPunt', en groeit dan nog

                    let waarde = TwRekenUtils.BerekenToekomstigewaardeMetEulersGetal inlegPeriodiek interestPerunage (float(aantaljaren - tijdPunt))

                    inlegPeriodiekToekomstwaarde <- inlegPeriodiekToekomstwaarde + waarde
                
                if restantJaarFractie > 0.0 then
                    let waarde = TwRekenUtils.BerekenToekomstigewaardeMetEulersGetal inlegPeriodiek interestPerunage (restantJaarFractie)
                    inlegPeriodiekToekomstwaarde <- inlegPeriodiekToekomstwaarde + waarde
                
                let resultaatInlegInitieel = TwRekenUtils.BerekenToekomstigewaardeMetEulersGetal inlegInitieel interestPerunage aantalTijdeenheden
                resultaat <- resultaatInlegInitieel + inlegPeriodiekToekomstwaarde
             else
                interestPerunage <- (interest / 100.0) / float (Common.mapFrequentieNaarGetal frequentieFactor)
                aantalTijdeenheden <- float(aantaljaren * Common.mapFrequentieNaarGetal frequentieFactor)
                if aantalmaanden > 0 then
                    let extraTijdeenheden = (float aantalmaanden / 12.0)
                    aantalTijdeenheden <- aantalTijdeenheden + float extraTijdeenheden
                resultaat <- TwRekenUtils.TW (interestPerunage) aantalTijdeenheden (inlegPeriodiek) (inlegInitieel) pXNumerando
            
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
    // Voorbeeld TW(0.03;50;6000; 0; 0) = € 676.781,20. Postnumerando
    // Voorbeeld TW(0.03;50;6000; 0; 1) = € 697.084,64. Prenumerando
    // Zie Basisboek wiskunde en financiële berekeningen, 2020, hoofdstuk 4.9, p.161
    btnVoorbeeldTw.Click.Add(fun _ ->
        txtInlegInitieel.Text <- "0"
        txtInlegPeriodiek.Text <- "-6000"
        txtInterest.Text <- "3"
        txtJaren.Text <- "50"
        txtMaanden.Text <- "0"
        listBoxInlegfrequentie.SelectedIndex <- int(Common.FinMutatieFrequentie.Jaarlijks) - 1
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

    form.Controls.Add(lblInlegfrequentie)
    form.Controls.Add(listBoxInlegfrequentie)
    form.Controls.Add(chkPostnumerando)
    form.Controls.Add(lblResultaat)
    form.Controls.Add(txtResult)
    form.Controls.Add(btnBereken)
    form.Controls.Add(btnTerug)
    form.Controls.Add(btnVoorbeeldTw)

    listBoxInlegfrequentie.DataSource <- Common.financieMutatieFrequentieItems
    listBoxInlegfrequentie.SelectedIndex <- int(Common.FinMutatieFrequentie.Jaarlijks) - 1
    form
