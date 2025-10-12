module HuidigeWaarde
open System.Windows.Forms
open System
open System.Globalization
open System.Drawing
open FsFinancieelRekenen

// Definieer het type voor resp. post- en prenumerando (in Excel is 0 = Postnumerando, 1 = Prenumerando)

// type PXNumerando = Post | Pre

// Bereken de huidige waarde met periodieke betalingen voor prenumerando.
// TW is het te bereiken doelbedrag (nadat alle betalingen zijn gedaan).
// "bet" is de periodieke betaling (negatief getal).
// Een opname zou een positief getal zijn.
let HWPrenumerando (rente: float) (aantalTermijnen: int) (bet: float) (tw: float): float =
    let mutable resultaat = 0.0
    if rente = 0.0 then
        resultaat <- tw - bet * (float aantalTermijnen)
    else
        let cwFactor = Math.Pow(1.0 + rente, -aantalTermijnen)
        let hwDoelbedrag = tw*cwFactor
        resultaat <- hwDoelbedrag - bet * (1.0 + rente) * ((1.0 - cwFactor) / rente)
    resultaat

// Bereken de huidige waarde met periodieke betalingen voor postnumerando.
// TW is het te bereiken doelbedrag (nadat alle betalingen zijn gedaan).
// AantalTermijnen is hier het aantal termijnen inclusief de laatste termijn (dus bijv. 10 jaar = 10 termijnen).
// De laatste termijn wordt niet meer verdisconteerd, de eerste termijn wordt 1 periode verdisconteerd.
// "bet" is de periodieke betaling (negatief getal). Een opname zou een positief getal zijn.
let HWPostnumerando (rente: float) (aantalTermijnen: int) (bet: float) (tw: float): float =
    let aantalTermijnen2 = aantalTermijnen - 1
    if rente = 0.0 then
        let resultaat = tw + bet * (float aantalTermijnen2)
        resultaat
    else
        let cwFactor = Math.Pow(1.0 + rente, -aantalTermijnen2)
        let hwDoelbedrag = tw*cwFactor
        let resultaat = hwDoelbedrag - bet * (1.0 + ((1.0 - cwFactor) / rente))
        resultaat

// Bereken de huidige waarde met periodieke betalingen.
// Zelfde signatuur als de gelijknamige Excel functie (0 = Postnumerando = default)
let HW (rente: float) (aantalTermijnen: int) (bet: float) (tw: float) (pXNumerando: Common.PXNumerando) : float =
    if pXNumerando = Common.PXNumerando.Pre then
        let resultaat = HWPrenumerando rente aantalTermijnen bet tw
        resultaat
    else
        let resultaat = HWPostnumerando rente aantalTermijnen bet tw
        resultaat

let maakHwFormulier () =
    let form = new Form(Text = "Huidige waarde", Width = 600, Height = 400)

    let lblToekomstigeWaarde = new Label(Text = "Toekomstige waarde", Top = 20, Left = 10, Width = 200)
    let txtToekomstigeWaarde = new TextBox(Top = 40, Left = 10, Width = 200)
    let lblBedragPerTermijn = new Label(Text = "Inleg/opname per termijn (negatief/positief)", Top = 60, Left = 10, Width = 300)
    let txtBedragPerTermijn = new TextBox(Top = 80, Left = 10, Width = 100)
    let lblRentePercentage = new Label(Text = "Rente (%)", Top = 100, Left = 10, Width = 100)
    let txtRentePercentage = new TextBox(Top = 120, Left = 10, Width = 100)

    let chkPostnumerando = new CheckBox()
    chkPostnumerando.Text <- "Postnumerando"
    chkPostnumerando.Top <- 100
    chkPostnumerando.Left <- 150
    chkPostnumerando.Width <- 200
    chkPostnumerando.Checked <- true

    let lblAantalTermijnen = new Label(Text = "Aantal termijnen (jaren)", Top = 140, Left = 10, Width = 200)
    let txtAantalTermijnen = new TextBox(Top = 160, Left = 10, Width = 100)
    let lblHuidigeWaarde = new Label(Text = "Huidige waarde", Top = 180, Left = 10, Width = 200)
    let txtHuidigeWaarde = new TextBox(Top = 200, Left = 10, Width = 200)
    txtHuidigeWaarde.ReadOnly <- true 
    let btnBereken = new Button(Text = "Bereken", Top = 240, Left = 10)
    let btnTerug = new Button(Text = "Naar hoofdscherm", Top = 240, Left = 120, Width = 200)
    let btnVoorbeeldHw = new Button(Text = "Voorbeeld-data", Top = 240, Left = 420, Width = 100, BackColor = Color.LightGreen )
    
    // Event handlers

    btnBereken.Click.Add(fun _ ->
        let rente = 
            match Double.TryParse(txtRentePercentage.Text) with
            | (true, value) -> (value / 100.0)
            | _ -> 0.0

        let pxNumerando =
            if chkPostnumerando.Checked then Common.PXNumerando.Post
            else Common.PXNumerando.Pre

        let aantalTermijnen = 
            match Int32.TryParse(txtAantalTermijnen.Text) with
            | (true, value) -> value
            | _ -> 0

        let toekomstigeWaarde = 
            match Double.TryParse(txtToekomstigeWaarde.Text) with
            | (true, value) -> value
            | _ -> 0.0

        let betaling = 
            match Double.TryParse(txtBedragPerTermijn.Text) with
            | (true, value) -> value
            | _ -> 0.0
        let hw = HW rente aantalTermijnen betaling toekomstigeWaarde pxNumerando
        txtHuidigeWaarde.Text <- hw.ToString("F2", CultureInfo.InvariantCulture)
    )

    btnTerug.Click.Add(fun _ -> form.Close())
    btnVoorbeeldHw.Click.Add(fun _ ->
    // Zie Levensverzekeringskunde en pensioencalculaties (Academic Service), 2013, hoofdstuk 1.8, p.18
    // Vul voorbeelddata in, nl. TW = 0, inleg per termijn = -2.500,
    // rente = 6%, aantal termijnen = 50
    // Verwachte HW is dan (afgerond: 41.769): Postnumerando en Prenumerando
        txtToekomstigeWaarde.Text <- "0"
        txtBedragPerTermijn.Text <- "-2500"
        txtRentePercentage.Text <- "6"
        txtAantalTermijnen.Text <- "50"
    )

    form.Controls.Add(lblToekomstigeWaarde)
    form.Controls.Add(txtToekomstigeWaarde)
    form.Controls.Add(lblBedragPerTermijn)
    form.Controls.Add(txtBedragPerTermijn)
    form.Controls.Add(lblRentePercentage)
    form.Controls.Add(txtRentePercentage)
    form.Controls.Add(chkPostnumerando)
    form.Controls.Add(lblAantalTermijnen)
    form.Controls.Add(txtAantalTermijnen)
    form.Controls.Add(lblHuidigeWaarde)
    form.Controls.Add(txtHuidigeWaarde)      
    form.Controls.Add(btnBereken)
    form.Controls.Add(btnTerug)
    form.Controls.Add(btnVoorbeeldHw)
    form
