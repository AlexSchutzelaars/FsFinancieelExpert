module EffectenPortefeuille
open System.IO
open System.Xml
open System.Windows.Forms
open System.Linq
open System.Xml.Linq
open System

type Measurement = {MeasurementDate: string; HowMany: double; Price: double}
type FundInfo = {Provider: string; Name: string; Description: string; Measurements: List<Measurement>}

let getProvider(doc: XmlDocument)  =
   let providerNodes = doc.SelectSingleNode("//provider/name") |> Seq.cast<XmlNode>
   let provider = providerNodes.ToList().[0].Value.ToString()
   provider

let getFundNodes(doc: XmlDocument)  =
   let fundNodes1 = doc.SelectNodes("//provider/funds/fund") |> Seq.cast<XmlNode>
   let fundNodes2 = fundNodes1.ToList()
   fundNodes2

let getDataForFund(fundProviderInXml: string, fundNode: XmlNode) =
   let results = XDocument.Parse(fundNode.OuterXml).Descendants().ToList()
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
   
   let mutable fundInfo = {Provider = fundProviderInXml; Name = fundName; Description = description;
                            Measurements = measurements}
   fundInfo

let getDataByFundName(fileName: string, nameOfFund: string) =

   (* https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmldocument?view=net-5.0 *)

   let doc = new XmlDocument()
   doc.Load fileName
   let providerName = getProvider doc
   let funds = getFundNodes doc
   let mutable fundInfo = {Provider = providerName; Name = "Dummy"; Description = "Dummy"; Measurements = []}
   let mutable found = false
   let mutable index = 0
   while not found do
       fundInfo <- getDataForFund(providerName, funds.[index])
       index <- index + 1
       if fundInfo.Name = nameOfFund then
            found <- true
   fundInfo

let maakEffectenPortefeuilleFormulier () =
    let frmEffectenportefeuille = new Form(Text = "Bekijk je effectenportefeuille", Width = 700, Height = 400)

    let clientsize = new System.Drawing.Size(700, 430)
    let fundsForm = new Form(MaximizeBox = false, Text = "Fonds-info", ClientSize = clientsize, StartPosition = FormStartPosition.CenterScreen)

    // Maak een TextBox om het pad te tonen
    let startY_ButtonXmlPath = 20

    let btnXmlBestand = new Button(Text = "Kies XML-bestand voor effectenportefeuille", Left = 20, Top = startY_ButtonXmlPath, Width = 250)

    let startY_TxtBoxXmlPath = startY_ButtonXmlPath + 40

    let txtXmlBestand = new TextBox(ReadOnly = true, Height = 20, Top = startY_TxtBoxXmlPath, Left = 20, Width = 400)

    let startY_DateTime = startY_TxtBoxXmlPath + 40
    let dateTimePicker: DateTimePicker = new DateTimePicker(Left = 20, Top = startY_DateTime, Height = 20, Width = 200)

    dateTimePicker.MinDate = new DateTime(2019, 1, 1) |> ignore

    let startY_LBoxFunds = startY_DateTime + 40
    let lboxFunds = new ListBox(Left = 20, Top = startY_LBoxFunds, Height = 50, Width = 300)
    let startY_LabelCalculatedValue = startY_LBoxFunds + lboxFunds.Height + 20
    let startY_TextCalculatedValue = startY_LabelCalculatedValue + 20
    let lblCalculatedForFund = new Label(Text = "Marktwaarde en peildatum voor fonds", Left = 20, Top = startY_LabelCalculatedValue, Height = 20, Width = 250)
    let txtCalculatedForFund = new TextBox(Left = 20, Top = startY_TextCalculatedValue, Height = 20, Width = 90, Visible = true)
    let txtPeildatumForFund = new TextBox(Left = 150, Top = startY_TextCalculatedValue, Height = 20, Width = 90, Visible = true)
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
        txtCalculatedForFund.Visible <- false
        let xmlFileName = txtXmlBestand.Text
        if File.Exists xmlFileName then
            let doc = new XmlDocument()
            doc.Load xmlFileName
            let provider = getProvider doc
            let funds = getFundNodes doc
            for index = 0 to funds.Count - 1 do
                let fundInfo = getDataForFund(provider, funds.[index])
                let fundName = fundInfo.Name
                lboxFunds.Items.Add(fundName) |> ignore
    )

    let xmlFileName = txtXmlBestand.Text

    if File.Exists xmlFileName then
        let doc = new XmlDocument()
        doc.Load xmlFileName
        let provider = getProvider doc
        let funds = getFundNodes doc
        for index = 0 to funds.Count - 1 do
            let fundData = getDataForFund(provider, funds.[index])
            let fundName = fundData.Name
            lboxFunds.Items.Add(fundName) |> ignore

    lboxFunds.SelectedIndexChanged.Add(fun _ -> 
                                        let theDate = dateTimePicker.Value.ToString("yyyy-MM-dd");
                                        let nameOfFund = lboxFunds.SelectedItem.ToString()
                                        let fundData = getDataByFundName(txtXmlBestand.Text, nameOfFund)
                                        // let fundData2 = getDataForFund(provider, funds.[index])

                                        let fondsinfoOpDatum = fundData.Measurements.ToList().OrderByDescending(fun m -> m.MeasurementDate)
                                        let mutable measurement = {MeasurementDate = "2019-01-01"; HowMany = 0.0; Price = 0.0}
                                        for m in fondsinfoOpDatum do
                                          if m.MeasurementDate <= theDate then
                                            measurement <- m
  
                                        if measurement.MeasurementDate = "2019-01-01" then
                                        // geen meting gevonden
                                          txtCalculatedForFund.Visible <- false
                                        else
                                          let calculatedValue = (measurement.HowMany*measurement.Price)
                                          txtCalculatedForFund.Visible <- true
                                          txtCalculatedForFund.Text <- calculatedValue.ToString(".00")
                                          txtPeildatumForFund.Visible <- true
                                          txtPeildatumForFund.Text <- measurement.MeasurementDate )
                                          
    btnTerug.Click.Add(fun _ -> frmEffectenportefeuille.Close())

    fundsForm.Controls.Add dateTimePicker
    fundsForm.Controls.Add btnXmlBestand
    fundsForm.Controls.Add txtXmlBestand
    fundsForm.Controls.Add lboxFunds
    fundsForm.Controls.Add txtCalculatedForFund

    frmEffectenportefeuille.Controls.Add dateTimePicker
    frmEffectenportefeuille.Controls.Add btnXmlBestand
    frmEffectenportefeuille.Controls.Add txtXmlBestand
    frmEffectenportefeuille.Controls.Add lboxFunds
    frmEffectenportefeuille.Controls.Add lblCalculatedForFund
    frmEffectenportefeuille.Controls.Add txtCalculatedForFund
    frmEffectenportefeuille.Controls.Add txtPeildatumForFund
    frmEffectenportefeuille.Controls.Add(btnTerug)
    frmEffectenportefeuille
