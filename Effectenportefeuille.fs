module EffectenPortefeuille
open System.IO
open System.Xml
open System.Windows.Forms
open System.Linq
open System.Xml.Linq
open System
open FundReader

type Measurement = {MeasurementDate: string; HowMany: double; Price: double}
type FundInfo = {Provider: string; Name: string; Description: string; Measurements: List<Measurement>}

let getFundNodes(fileName: string)  =
   let doc = new XmlDocument()
   doc.Load fileName
   let fundNodes1 = doc.SelectNodes("//funds/fund") |> Seq.cast<XmlNode>
   let fundNodes2 = fundNodes1.ToList()
   fundNodes2

let getDataForFund(fundNode: XmlNode) =
   let results = XDocument.Parse(fundNode.OuterXml).Descendants().ToList()
   let providerName = results.Where(fun n -> n.Name = XName.Get "Provider").ToList().[0].Value
   let fundName = results.Where(fun n -> n.Name = XName.Get "Name").ToList().[0].Value
   let description = results.Where(fun d -> d.Name = XName.Get "Description").ToList().[0].Value

   let timeslices = results.Where(fun t -> t.Name = XName.Get "TimeSlice").ToList()
   let mutable measurements: List<Measurement> = []

   for timeslice in timeslices do
      let timesliceResults = XElement.Parse(timeslice.ToString()).Descendants().ToList()
      // let testje = timesliceResults.Where(fun node -> node.Name = XName.Get "Date" && node.Value = "2021-04-13").ToList()
      let measurementDate = timesliceResults.Where(fun n -> n.Name = XName.Get "Date").ToList().[0].Value
      let howMany = timesliceResults.Where(fun n -> n.Name = XName.Get "HowMany").ToList().[0].Value
      let price = timesliceResults.Where(fun n -> n.Name = XName.Get "Price").ToList().[0].Value
      let measurement: Measurement = {MeasurementDate = measurementDate; HowMany = double(howMany); Price = double(price)}
      measurements <- measurement::measurements
   
   let mutable fundInfo = {Provider = providerName; Name = fundName; Description = description;
                            Measurements = measurements}
   fundInfo

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
    let lblCalculatedForFunds = new Label(Text = "Marktwaarde en peildatum voor fonds", Left = 20, Top = startY_LabelCalculatedValue, Height = 20, Width = 250)
    let txtCalculatedForFunds = new TextBox(Left = 20, Top = startY_TextCalculatedValue, Height = 20, Width = 90, Visible = true, Enabled = false)
    let txtPeildatumForFund = new TextBox(Left = 150, Top = startY_TextCalculatedValue, Height = 20, Width = 90, Visible = true, Enabled = false)
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
        txtCalculatedForFunds.Visible <- false
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
        // TODO: gebruik de reeds aanwezige functies in XmlFundInfo.fs
    lboxFunds.SelectedIndexChanged.Add(fun _ -> 
                                        let theDate = dateTimePicker.Value.ToString("yyyy-MM-dd");
                                        let xmlFileName = txtXmlBestand.Text
                                        let funds = getFundNodes(xmlFileName)
                                        let selectedFund = funds[lboxFunds.SelectedIndex]
                                        let fundData = getDataForFund(selectedFund)
                                        let fondsinfoOpDatum = fundData.Measurements.ToList().OrderByDescending(fun m -> m.MeasurementDate)
                                        let mutable measurement = {MeasurementDate = "2019-01-01"; HowMany = 0.0; Price = 0.0}
                                        for m in fondsinfoOpDatum do
                                          if m.MeasurementDate <= theDate then
                                            measurement <- m
  
                                        if measurement.MeasurementDate = "2019-01-01" then
                                        // geen meting gevonden
                                          txtCalculatedForFunds.Visible <- false
                                        else
                                          lblCalculatedForFunds.Text <- "Marktwaarde en peildatum voor fonds"
                                          let calculatedValue = (measurement.HowMany*measurement.Price)
                                          txtCalculatedForFunds.Visible <- true
                                          txtCalculatedForFunds.Text <- calculatedValue.ToString(".00")
                                          txtPeildatumForFund.Visible <- true
                                          txtPeildatumForFund.Text <- measurement.MeasurementDate )
    
    btnTotalAllFunds.Click.Add(fun _ -> 
        let theDate = dateTimePicker.Value.ToString("yyyy-MM-dd");
        let xmlFileName = txtXmlBestand.Text
        let funds = getFundNodes(xmlFileName)
        let mutable totalValue = 0.0
        for fundNode in funds do
            let fundData = getDataForFund(fundNode)
            let fondsinfoOpDatum = fundData.Measurements.ToList().OrderByDescending(fun m -> m.MeasurementDate)
            let mutable measurement = {MeasurementDate = "2019-01-01"; HowMany = 0.0; Price = 0.0}
            for m in fondsinfoOpDatum do
                if m.MeasurementDate <= theDate then
                    measurement <- m
            if measurement.MeasurementDate <> "2019-01-01" then
                lblCalculatedForFunds.Text <- "Marktwaarde en peildatum voor alle fondsen in de portefeuille"
                let calculatedValue = (measurement.HowMany*measurement.Price)
                totalValue <- totalValue + calculatedValue
                txtCalculatedForFunds.Text <- totalValue.ToString(".00")
                txtPeildatumForFund.Text <- theDate
        // MessageBox.Show(sprintf "Totale marktwaarde van de effectenportefeuille op %s is %.2f" theDate totalValue) |> ignore
    )

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
