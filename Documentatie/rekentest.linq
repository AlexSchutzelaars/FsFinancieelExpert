<Query Kind="FSharpProgram" />

open System
open System.Globalization


// Renteberekening met e^(r * t)
// berekenToekomstwaarde 1000 5 3.0 ==> 1163.83
let berekenToekomstwaardeMetEulersGetal (hoofdsom: float) (rente: float) (tijd: float) : float =
    let rentePerunage = float(rente/100.0)
    hoofdsom * Math.Exp (rentePerunage * tijd)

// Bijv. valutaSymbool = "€"
let waardeInValutaEuro bedrag =
    // sprintf "%s.2f" valutaSymbool waarde
    // CultureInfo voor Nederland (euro)
    let nlCulture = CultureInfo("nl-NL")

// Format als valuta
    let geformatteerdBedrag = (float bedrag).ToString("C", nlCulture)
    geformatteerdBedrag
    
// Test
let hoofdsom = 1000.0      // Beginbedrag in euro
let rente = 5           // Jaarlijkse rente (5%)
let tijd = 3.0             // Aantal jaren

let toekomstwaarde = berekenToekomstwaardeMetEulersGetal hoofdsom rente tijd
let hoofdsominEuro = waardeInValutaEuro(hoofdsom)
let toekomstwaardeinEuro = waardeInValutaEuro(toekomstwaarde)

printfn "Na %.1f jaar is de waarde van hoofdsom €%.2f %s" tijd hoofdsom toekomstwaardeinEuro

