module HuidigeWaarde
open System.Windows.Forms
open System
open System.Globalization
open System.Drawing

// Definieer het type voor resp. post- en prenumerando (in Excel is 0 = Postnumerando, 1 = Prenumerando)

type PXNumerando = Post | Pre

// Bereken de huidige waarde met periodieke betalingen voor prenumerando.
// TW is het te bereiken doelbedrag (nadat alle betalingen zijn gedaan).
let HWPrenumerando (rente: float) (aantalTermijnen: int) (bet: float) (tw: float): float =
    let mutable resultaat = 0.0
    if rente = 0.0 then
        resultaat <- tw + bet * (float aantalTermijnen)
    else
        let cwFactor = Math.Pow(1.0 + rente, -aantalTermijnen)
        let hwDoelbedrag = tw*cwFactor
        resultaat <- hwDoelbedrag + bet * (1.0 + rente) * ((1.0 - cwFactor) / rente)
    resultaat

let HWPostnumerando (rente: float) (aantalTermijnen: int) (bet: float) (tw: float): float =
    let aantalTermijnen2 = aantalTermijnen - 1
    let mutable resultaat = 0.0
    if rente = 0.0 then
        resultaat <- tw + bet * (float aantalTermijnen2)
    else
        let cwFactor = Math.Pow(1.0 + rente, -aantalTermijnen2)
        let hwDoelbedrag = tw*cwFactor
        resultaat <- hwDoelbedrag + bet + bet * ((1.0 - cwFactor) / rente)
    resultaat

// Bereken de huidige waarde met periodieke betalingen.
// Zelfde signatuur als de gelijknamige Excel functie (0 = Postnumerando = default)
let HW (rente: float) (aantalTermijnen: int) (bet: float) (tw: float) (pXNumerando: PXNumerando) : float =
    let mutable resultaat: float = 0.0
    if pXNumerando = PXNumerando.Pre then
        resultaat <- HWPrenumerando rente aantalTermijnen bet tw
    else
        resultaat <- HWPostnumerando rente aantalTermijnen bet tw
    resultaat

let maakHwFormulier () =
    let form = new Form(Text = "Huidige waarde", Width = 600, Height = 400)

    let lblToekomstigeWaarde = new Label(Text = "Toekomstige waarde", Top = 20, Left = 10, Width = 200)
    let txtBoxToekomstigeWaarde = new TextBox(Top = 40, Left = 10, Width = 200)
    let lblInlegPerTermijn = new Label(Text = "Inleg (positief getal!)", Top = 60, Left = 10, Width = 200)
    let txtBoxInlegPerTermijn = new TextBox(Top = 80, Left = 10, Width = 100)
    let lblRente = new Label(Text = "Rente (%)", Top = 100, Left = 10, Width = 100)
    let txtBoxRente = new TextBox(Top = 120, Left = 10, Width = 100)

    let chkPostnumerando = new CheckBox()
    chkPostnumerando.Text <- "Postnumerando"
    chkPostnumerando.Top <- 100
    chkPostnumerando.Left <- 150
    chkPostnumerando.Width <- 200
    chkPostnumerando.Checked <- true

    let lblAantalTermijnen = new Label(Text = "Aantal termijnen (jaren)", Top = 140, Left = 10, Width = 200)
    let txtBoxAantalTermijnen = new TextBox(Top = 160, Left = 10, Width = 100)
    let lblHuidigeWaarde = new Label(Text = "Huidige waarde", Top = 180, Left = 10, Width = 200)
    let txtBoxHuidigeWaarde = new TextBox(Top = 200, Left = 10, Width = 200)
    txtBoxHuidigeWaarde.ReadOnly <- true 
    let berekenButton = new Button(Text = "Bereken", Top = 240, Left = 10)
    let terugButton = new Button(Text = "Terug naar hoofdscherm", Top = 240, Left = 120, Width = 200)
    let btnVoorbeeldHw = new Button(Text = "Voorbeeld-data", Top = 240, Left = 420, Width = 100, BackColor = Color.LightGreen )
    
    // Event handlers

    berekenButton.Click.Add(fun _ ->
        let rente = 
            match Double.TryParse(txtBoxRente.Text) with
            | (true, value) -> (value / 100.0)
            | _ -> 0.0

        let pxNumerando = if chkPostnumerando.Checked then PXNumerando.Post else PXNumerando.Pre

        let aantalTermijnen = 
            match Int32.TryParse(txtBoxAantalTermijnen.Text) with
            | (true, value) -> value
            | _ -> 0

        let toekomstigeWaarde = 
            match Double.TryParse(txtBoxToekomstigeWaarde.Text) with
            | (true, value) -> value
            | _ -> 0.0

        let betaling = 
            match Double.TryParse(txtBoxInlegPerTermijn.Text) with
            | (true, value) -> value
            | _ -> 0.0
        let hw = HW rente aantalTermijnen betaling toekomstigeWaarde pxNumerando
        txtBoxHuidigeWaarde.Text <- hw.ToString("F2", CultureInfo.InvariantCulture)
    )

    terugButton.Click.Add(fun _ -> form.Close())
    btnVoorbeeldHw.Click.Add(fun _ ->
    // Vul voorbeelddata in, nl. TW = 10.000, inleg per termijn = 2.500, rente = 6%, aantal termijnen = 50
    // Zie Leveringsverzekeringskunde en pensioencalculaties (Academic Service), 2013, hoofdstuk 1.8, p.18
    // Verwachte HW is dan - agerond - 41.769 (Postnumerando en Prenumerando)
        txtBoxToekomstigeWaarde.Text <- "10000"
        txtBoxInlegPerTermijn.Text <- "2500"
        txtBoxRente.Text <- "6"
        txtBoxAantalTermijnen.Text <- "50"
    )

    form.Controls.Add(lblToekomstigeWaarde)
    form.Controls.Add(txtBoxToekomstigeWaarde)
    form.Controls.Add(lblInlegPerTermijn)
    form.Controls.Add(txtBoxInlegPerTermijn)
    form.Controls.Add(lblRente)
    form.Controls.Add(txtBoxRente)
    form.Controls.Add(chkPostnumerando)
    form.Controls.Add(lblAantalTermijnen)
    form.Controls.Add(txtBoxAantalTermijnen)
    form.Controls.Add(lblHuidigeWaarde)
    form.Controls.Add(txtBoxHuidigeWaarde)      
    form.Controls.Add(berekenButton)
    form.Controls.Add(terugButton)
    form.Controls.Add(btnVoorbeeldHw)
    form
