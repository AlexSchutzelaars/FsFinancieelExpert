open System
open System.Windows.Forms
open Documentatie
open ToekomstigeWaardeScherm
open EffectenPortefeuille
open HuidigeWaarde

[<STAThread>]
[<EntryPoint>]
do
    let form = new Form(Text = "FinancieelExpert in F# v0.2.2", Width = 400, Height = 300)

    let schermPanel = new Panel(Top = 10, Left = 10, Width = 360, Height = 170)
    form.Controls.Add(schermPanel)

    let toonHoofdscherm () =
        schermPanel.Controls.Clear()

        let btnEffectenPortefeuille = new Button(Text = "Effectenportefeuille", Top = 10, Left = 10, Width = 200)
        btnEffectenPortefeuille.Click.Add(fun _ -> (maakEffectenPortefeuilleFormulier().ShowDialog() |> ignore))

        let btnToekomstigeWaarde = new Button(Text = "Toekomstige waarde", Top = 50, Left = 10, Width = 200)
        btnToekomstigeWaarde.Click.Add(fun _ -> (maakToekomstigeWaardeFormulier().ShowDialog() |> ignore))

        let btnHuidigeWaarde = new Button(Text = "Huidige waarde (HW)", Top = 90, Left = 10, Width = 200)
        btnHuidigeWaarde.Click.Add(fun _ -> (maakHwFormulier().ShowDialog() |> ignore))

        let btnDocumentatie = new Button(Text = "Documentatie", Top = 130, Left = 10, Width = 200)
        btnDocumentatie.Click.Add(fun _ -> (maakDocumentatieFormulier().ShowDialog() |> ignore))

        schermPanel.Controls.Add(btnEffectenPortefeuille)
        schermPanel.Controls.Add(btnToekomstigeWaarde)

        schermPanel.Controls.Add(btnHuidigeWaarde)
        schermPanel.Controls.Add(btnDocumentatie)

    toonHoofdscherm()
    Application.Run(form)