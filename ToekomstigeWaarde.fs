module ToekomstigeWaarde
open System.Windows.Forms
open System.Globalization
open System

// Definieer het type voor resp. post- en prenumerando
type PXNumerando =
    | Achteraf = 0
    | Vooraf = 1

// Definieer de frequentie als een enum
type Frequentie =
    | Dagelijks = 1
    | Maandelijks = 2
    | Halfjaarlijks = 3
    | Jaarlijks = 4

type FrequentieItem = {
    Naam: string
    Waarde: Frequentie
}

let frequentieItems = 
    [|
        { Naam = "Dagelijks"; Waarde = Frequentie.Dagelijks }
        { Naam = "Maandelijks"; Waarde = Frequentie.Maandelijks }
        { Naam = "Halfjaarlijks"; Waarde = Frequentie.Halfjaarlijks }
        { Naam = "Jaarlijks"; Waarde = Frequentie.Jaarlijks }
    |]

let mapFrequentieNaarRenteFactor (freq: Frequentie) =
     if freq =  Frequentie.Dagelijks then 365
     elif freq =  Frequentie.Maandelijks then 12
     elif freq =  Frequentie.Halfjaarlijks then 6
     elif freq =  Frequentie.Jaarlijks then 1
     else 1

    // Bereken de toekomstige waarde met periodieke betalingen.
    // Zelfde signatuur als de gelijknamige Excel functie (0 = Postnumeranco = default)
let TW (rente: float) (aantalTermijnen: int) (bet: float) (hw: float) (pXNumerando: PXNumerando) : float =
    let mutable resultaat: float = 0.0
    if rente = 0.0 then
        resultaat <- -hw - bet * float aantalTermijnen
    else
        let twFactor = Math.Pow(1.0 + rente, aantalTermijnen)
        // Eenmalige investering (hw) wordt altijd aan het begin van de periode gedaan
        resultaat <- -hw * twFactor - bet * (twFactor - 1.0) / rente
        if (pXNumerando = PXNumerando.Vooraf) then
            resultaat <- resultaat * (1.0 + rente)
    resultaat


// let terugButton = new Button(Text = "Terug", Top = 280, Left = 150, Width = 100)

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

    let checkbox = new CheckBox()
    checkbox.Text <- "Postnumerando"
    checkbox.Top <- 60
    checkbox.Left <- 150
    checkbox.Width <- 200
    checkbox.Checked <- true

    // Label en ListBox voor Frequentie
    let labelInlegFrequentie = new Label(Text = "Inleg-frequentie", Top = 110, Left = 30, Width = 200)

    let listBoxInlegfrequentie = new ListBox()
    listBoxInlegfrequentie.Top <- 130
    listBoxInlegfrequentie.Left <- 30
    listBoxInlegfrequentie.Width <- 200
    listBoxInlegfrequentie.Height <- 80

    listBoxInlegfrequentie.DisplayMember <- "Naam"
    listBoxInlegfrequentie.ValueMember <- "Waarde" // optioneel, maar handig

    //    MessageBox.Show (string listBox.Items.[i]) |> ignore

    let labelResultaat = new Label(Text = "Berekende waarde", Top = 220, Left = 30, Width = 200)
    let textResult = new TextBox(Top = 240, Left = 30, Width = 200, ReadOnly = true)

    let btnBereken = new Button(Text = "Bereken", Top = 280, Left = 30, Width = 100)
    let btnTerug = new Button(Text = "Terug naar hoofdscherm", Top = 280, Left = 270, Width = 200)

    btnTerug.Click.Add(fun _ -> form.Close())

    // Event handler voor knopklik
    btnBereken.Click.Add(fun _ ->
        let successInitieel, inlegInitieel = Double.TryParse(inputInlegInitieel.Text)
        let successInlegPeriodiek, _ = Double.TryParse(inputInlegPeriodiek.Text)
        let successRente, rente = Double.TryParse(inputRente.Text)
        let selectedIndex = listBoxInlegfrequentie.SelectedIndex

        if (successInitieel || successInlegPeriodiek) && successRente && selectedIndex >= 0 then
            let geselecteerd = listBoxInlegfrequentie.SelectedItem :?> FrequentieItem
            let frequentieFactor = geselecteerd.Waarde

            let aantaljaren = Convert.ToInt32(inputJaren.Text)
            let inlegPeriodiek = 
                match Double.TryParse(inputInlegPeriodiek.Text) with
                | (true, value) -> value
                | _ -> 0.0
            // Postnumerando (0) of Prenumerando (1)

            let pXNumerando = if checkbox.Checked then PXNumerando.Achteraf else PXNumerando.Vooraf
            let renteperunage = (rente / 100.0) / float (mapFrequentieNaarRenteFactor frequentieFactor)
            let resultaat = TW (renteperunage) (aantaljaren * mapFrequentieNaarRenteFactor frequentieFactor) (-inlegPeriodiek) (-inlegInitieel) pXNumerando
            
        // Gebruik de huidige systeemcultuur
            let systeemCultuur = CultureInfo.CurrentCulture.Clone() :?> CultureInfo
            // Stel de gewenste scheidingstekens in voor valuta
            systeemCultuur.NumberFormat.CurrencyGroupSeparator <- "."
            systeemCultuur.NumberFormat.CurrencyDecimalSeparator <- ","
            systeemCultuur.NumberFormat.CurrencySymbol <- "€"
            let formattedResultaat = resultaat.ToString("C", systeemCultuur)
            textResult.Text <- formattedResultaat
        else
            MessageBox.Show("Voer geldige getallen in en kies de frequentie voor de rentering.") |> ignore
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
    form.Controls.Add(checkbox)
    form.Controls.Add(labelResultaat)
    form.Controls.Add(textResult)
    form.Controls.Add(btnBereken)
    form.Controls.Add(btnTerug)

    listBoxInlegfrequentie.DataSource <- frequentieItems
    listBoxInlegfrequentie.SelectedIndex <- int(Frequentie.Jaarlijks) - 1
    form
