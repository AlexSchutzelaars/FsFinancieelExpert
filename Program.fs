open System
open System.Windows.Forms
open HuidigeWaarde
open ToekomstigeWaarde
open EffectenPortefeuille

[<STAThread>]
[<EntryPoint>]
do
    let form = new Form(Text = "FinancieelExpert v2", Width = 400, Height = 300)

    let schermPanel = new Panel(Top = 80, Left = 10, Width = 360, Height = 150)
    form.Controls.Add(schermPanel)

    let toonHoofdscherm () =
        schermPanel.Controls.Clear()

        let btnToekomstigeWaarde = new Button(Text = "Toekomstige waarde", Top = 10, Left = 10, Width = 200)
        btnToekomstigeWaarde.Click.Add(fun _ -> (maakToekomstigeWaardeFormulier().ShowDialog() |> ignore))

        let btnEffectenPortefeuille = new Button(Text = "Effectenportefeuille", Top = 50, Left = 10, Width = 200)
        btnEffectenPortefeuille.Click.Add(fun _ -> (maakEffectenPortefeuilleFormulier().ShowDialog() |> ignore))

        let btnHuidigeWaarde = new Button(Text = "Huidige waarde (HW)", Top = 90, Left = 10, Width = 200)
        btnHuidigeWaarde.Click.Add(fun _ -> (maakHwFormulier().ShowDialog() |> ignore))

        schermPanel.Controls.Add(btnToekomstigeWaarde)
        schermPanel.Controls.Add(btnEffectenPortefeuille)
        schermPanel.Controls.Add(btnHuidigeWaarde)

    toonHoofdscherm()
    Application.Run(form)