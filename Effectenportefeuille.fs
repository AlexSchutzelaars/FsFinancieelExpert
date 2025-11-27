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
        let repo = XmlFundLoader.LoadFundsChooseLatestAsOfDate(xmlFileName, dateTimePicker.Value)   
        for fundInfo in repo.Funds do
                let fundName = fundInfo.Name
                lboxFunds.Items.Add(fundName) |> ignore
        )
       
    // Event handler voor selectie in de ListBox
    // Wanneer een fonds wordt geselecteerd, bereken en toon de marktwaarde op de laatst bekende datum <= pieldatum
    lboxFunds.SelectedIndexChanged.Add(fun _ -> 
                                        let selectedDate = dateTimePicker.Value;
                                        let xmlFileName = txtXmlBestand.Text
                                        let repo = XmlFundLoader.LoadFundsChooseLatestAsOfDate(xmlFileName, selectedDate)
                                        let selectedFund = repo.Funds[lboxFunds.SelectedIndex]
                                        let (latestDate, calculatedValue, _) = XmlFundLoader.StatisticsForFundAsOfGivenDate repo selectedFund.Name selectedDate
                                        lblCalculatedForFunds.Text <- "Marktwaarde en peildatum voor fonds " + selectedFund.Name
                                        txtCalculatedForFunds.Text <- calculatedValue.ToString(".00")
                                        txtPeildatumForFund.Text <- latestDate.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture)
                                       )

    // Wanneer ingedrukt, bereken de totale marktwaarde van alle fondsen op de gekozen datum
    // TODO: berekenen van laatste beschikbare datum per fonds <= gekozen datum
    
    btnTotalAllFunds.Click.Add(fun _ -> 
        let selectedDate = dateTimePicker.Value                             
        let xmlFileName = txtXmlBestand.Text
        let repo = XmlFundLoader.LoadFundsChooseLatestAsOfDate(xmlFileName, selectedDate)

        let funds = repo.Funds
        let mutable totalValue = 0.0
        lblCalculatedForFunds.Text <- "Marktwaarde voor alle fondsen in de portefeuille per peildatum"
        txtPeildatumForFund.Text <- selectedDate.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture)
        for fund in funds do
            let (_, calculatedValue, _) = XmlFundLoader.StatisticsForFundAsOfGivenDate repo fund.Name selectedDate
            totalValue <- totalValue + float calculatedValue
        txtCalculatedForFunds.Text <- totalValue.ToString(".00")
     
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
