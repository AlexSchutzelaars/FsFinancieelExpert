open System
open System.Windows.Forms
open HuidigeWaarde
open ToekomstigeWaarde
open NogTeBepalen

[<STAThread>]
do
    let form = new Form(Text = "FinancieelExpert2", Width = 400, Height = 300)

    let schermPanel = new Panel(Top = 80, Left = 10, Width = 360, Height = 150)
    form.Controls.Add(schermPanel)

    let toonHoofdscherm () =
        schermPanel.Controls.Clear()

        let knop1 = new Button(Text = "Toekomstige waarde", Top = 10, Left = 10, Width = 200)
        knop1.Click.Add(fun _ -> (maakToekomstigeWaardeFormulier().ShowDialog() |> ignore))

        let knop2 = new Button(Text = "Hudige waarde (HW)", Top = 50, Left = 10, Width = 200)
        knop2.Click.Add(fun _ -> (maakHwFormulier().ShowDialog() |> ignore))

        let knop3 = new Button(Text = "Nog te bepalen", Top = 90, Left = 10, Width = 200)
        knop3.Click.Add(fun _ -> (maaknogTeBepalenFormulier().ShowDialog() |> ignore))

        schermPanel.Controls.Add(knop1)
        schermPanel.Controls.Add(knop2)
        schermPanel.Controls.Add(knop3)

    toonHoofdscherm()
    Application.Run(form)