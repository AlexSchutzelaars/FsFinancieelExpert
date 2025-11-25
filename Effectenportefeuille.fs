module EffectenPortefeuille
open System.IO
open System.Windows.Forms
open System.Linq
open System
open FundReader

type Measurement = {MeasurementDate: string; HowMany: double; Price: double}
type FundInfo = {Provider: string; Name: string; Description: string; Measurements: List<Measurement>}

// Maak het formulier voor de effectenportefeuille
let maakEffectenPortefeuilleFormulier () =
    let frmEffectenportefeuille = new Form(Text = "Bekijk uw effectenportefeuille", Width = 700, Height = 400)
 
    // Maak een TextBox om het pad te tonen
    let startY_ButtonXmlPath = 20

    let btnXmlBestand = new Button(Text = "Kies XML-bestand voor effectenportefeuille", Left = 20, Top = startY_ButtonXmlPath, Width = 250)

    let startY_TxtBoxXmlPath = startY_ButtonXmlPath + 40

    let txtXmlBestand = new TextBox(ReadOnly = true, Height = 20, Top = startY_TxtBoxXmlPath, Left = 20, Width = 400)

    let startY_DateTime = startY_TxtBoxXmlPath + 40
    let dateTimePicker: DateTimePicker = new DateTimePicker(Left = 20, Top = startY_DateTime, Height = 20, Width = 200)

    dateTimePicker.MinDate = new DateTime(2019, 1, 1) |> ignore

    let startY_LBoxFunds = startY_DateTime + 40
    let lboxFunds = new ListBox(Left = 20, Top = startY_LBoxFunds, Height = 50, Width = 200)
    let btnTotalAllFunds = new Button(Text = "Totaal van portefeuille", Left = 250, Top = startY_LBoxFunds, Width = 200)
    let startY_LabelCalculatedValue = startY_LBoxFunds + lboxFunds.Height + 20
    let startY_TextCalculatedValue = startY_LabelCalculatedValue + 20
    let lblCalculatedForFunds = new Label(Text = "Marktwaarde en peildatum voor fonds", Left = 20, Top = startY_LabelCalculatedValue, Height = 20, Width = 400)
    let txtCalculatedForFunds = new TextBox(Left = 20, Top = startY_TextCalculatedValue, Height = 20, Width = 90, ReadOnly = true)
    let txtPeildatumForFund = new TextBox(Left = 150, Top = startY_TextCalculatedValue, Height = 20, Width = 90, ReadOnly = true)
    let btnTerug = new Button(Text = "Terug naar financieel hoofdmenu", Left = 20, Top = startY_TextCalculatedValue + 40,Width = 200)

    // Event handler voor de bestand-selectieknop
    btnXmlBestand.Click.Add(fun _ ->
        use dialog = new OpenFileDialog()
        dialog.Filter <- "XML bestanden (*.xml)|*.xml|Alle bestanden (*.*)|*.*"
        dialog.Title <- "Selecteer een XML-bestand"
        if dialog.ShowDialog() = DialogResult.OK then
            txtXmlBestand.Text <- dialog.FileName
    )

    btnXmlBestand.Click.Add(fun _ -> 
        lboxFunds.Items.Clear()
        let xmlFileName = txtXmlBestand.Text
        if File.Exists xmlFileName then
            let repo = XmlFundLoader.LoadFromFileSumSlices(xmlFileName)

            // Iterate directly over the sequence/list of funds instead of using an index.
            for fundInfo in repo.Funds do
                let fundName = fundInfo.Name
                lboxFunds.Items.Add(fundName) |> ignore
        )

        // Event handler voor selectie in de ListBox
        // Wanneer een fonds wordt geselecteerd, bereken en toon de marktwaarde op de gekozen datum

    lboxFunds.SelectedIndexChanged.Add(fun _ -> 
                                        let selectedDate = dateTimePicker.Value;
                                        let xmlFileName = txtXmlBestand.Text
                                        let repo = XmlFundLoader.LoadFromFileSumSlices(xmlFileName)
                                        let funds = repo.Funds
                                        let selectedFund = funds[lboxFunds.SelectedIndex]
                                        let fundData = selectedFund.TimeSlices
                                        let fondsinfoOpDatum = fundData.ToList().OrderBy(fun m -> m.Date)
                                        let minimalDate = (new DateTime(2000,1,1)).ToString("yyyy-MM-dd")
                                        let mutable measurement = {MeasurementDate = minimalDate; HowMany = 0.0; Price = 0.0}
                                        for ms in fondsinfoOpDatum do
                                          if ms.Date <= selectedDate then
                                            measurement <- {MeasurementDate = ms.Date.ToString(); 
                                                        HowMany = Convert.ToDouble(ms.HowMany); Price = Convert.ToDouble(ms.Price)}
  
                                        if measurement.MeasurementDate = minimalDate then
                                        // geen meting gevonden
                                          lblCalculatedForFunds.Text <- "Geen meting gevonden voor dit fonds op of vóór de gekozen datum"
                                          txtPeildatumForFund.Text <- ""
                                          txtCalculatedForFunds.Text <- ""
                                        else
                                          lblCalculatedForFunds.Text <- "Marktwaarde en peildatum voor fonds"
                                          let calculatedValue = (measurement.HowMany*measurement.Price)
                                          txtCalculatedForFunds.Text <- calculatedValue.ToString(".00")
                                          txtPeildatumForFund.Text <- measurement.MeasurementDate )
    
    // Event handler voor de knop om de totale marktwaarde van alle fondsen te berekenen
    btnTotalAllFunds.Click.Add(fun _ -> 
        let selectedDate = dateTimePicker.Value
        let minimalDate = (new DateTime(2000,1,1)).ToString("yyyy-MM-dd")
                                       
        let xmlFileName = txtXmlBestand.Text
        let repo = XmlFundLoader.LoadFromFileSumSlices(xmlFileName)
        let funds = repo.Funds
        let mutable totalValue = 0.0
        for fund in funds do
            let fondsinfoOpDatum = fund.TimeSlices.OrderBy(fun m -> m.Date)
            let mutable measurement = {MeasurementDate = minimalDate; HowMany = 0.0; Price = 0.0}
            // Zoek de meest recente meting op of vóór de geselecteerde datum
            for ms in fondsinfoOpDatum do
               if ms.Date <= selectedDate then
                  measurement <- {MeasurementDate = ms.Date.ToString(); 
                                HowMany = Convert.ToDouble(ms.HowMany); 
                                Price = Convert.ToDouble(ms.Price)}

            if measurement.MeasurementDate = minimalDate then
                // geen meting gevonden voor dit fonds op of vóór de geselecteerde datum
                lblCalculatedForFunds.Text <- "Geen metingen (op of VOOR de gekozen datum) gevonden"
                txtCalculatedForFunds.Text <- ""
                txtPeildatumForFund.Text <- ""
            else
                lblCalculatedForFunds.Text <- "Marktwaarde en peildatum voor alle fondsen in de portefeuille"
                let calculatedValue = measurement.HowMany*measurement.Price
                totalValue <- totalValue + calculatedValue
                txtCalculatedForFunds.Text <- totalValue.ToString(".00")
                txtPeildatumForFund.Text <- measurement.MeasurementDate
        // MessageBox.Show(sprintf "Totale marktwaarde van de effectenportefeuille op %s is %.2f" theDate totalValue) |> ignore
    )

    // Event handler voor de terug-knop [terug naar hoofdscherm]
    btnTerug.Click.Add(fun _ -> frmEffectenportefeuille.Close())

    frmEffectenportefeuille.Controls.Add dateTimePicker
    frmEffectenportefeuille.Controls.Add btnXmlBestand
    frmEffectenportefeuille.Controls.Add txtXmlBestand
    frmEffectenportefeuille.Controls.Add lboxFunds
    frmEffectenportefeuille.Controls.Add btnTotalAllFunds

    frmEffectenportefeuille.Controls.Add lblCalculatedForFunds
    frmEffectenportefeuille.Controls.Add txtCalculatedForFunds
    frmEffectenportefeuille.Controls.Add txtPeildatumForFund
    frmEffectenportefeuille.Controls.Add(btnTerug)
    frmEffectenportefeuille
