module NogTeBepalen
open System.Windows.Forms

let maaknogTeBepalenFormulier () =
    let form = new Form(Text = "Scherm drie: nog te bepalen", Width = 350, Height = 200)

    let textBox = new TextBox(Top = 40, Left = 10, Width = 200)
    let berekenButton = new Button(Text = "Bereken", Top = 80, Left = 10)
    let terugButton = new Button(Text = "Terug", Top = 80, Left = 120)

    berekenButton.Click.Add(fun _ ->
        MessageBox.Show($"Invoer: {textBox.Text}") |> ignore
    )

    terugButton.Click.Add(fun _ -> form.Close())

    form.Controls.Add(textBox)
    form.Controls.Add(berekenButton)
    form.Controls.Add(terugButton)
    form
