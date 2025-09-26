module Documentatie

open System.Windows.Forms
open System
open Markdig
open System.IO
open System.Reflection


let maakDocumentatieFormulier () =
    // Met dank aan CoPlot op https://stackoverflow.com/questions/6590554/displaying-markdown-in-a-windows-forms-application
    // https://github.com/xoofx/markdig
    // dotnet add package Markdig

    // Load Markdown file
    let exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    let solutionDir = Directory.GetParent(exeDir).Parent.Parent.FullName
    let readmePath = Path.Combine(solutionDir, "README.md")

    let markdownText = System.IO.File.ReadAllText(readmePath)

    // Convert Markdown to HTML
    let html = Markdown.ToHtml(markdownText)

    // Create WebBrowser control
    let browser = new WebBrowser(Dock = DockStyle.Bottom)
    browser.DocumentText <- html
    let terugButton = new Button(Text = "Terug naar hoofdscherm", Top = 20, Left = 10, Width = 200)
    let form = new Form(Text = "Documentatie", Top = 10, Width = 500, Height = 300)
    
    // Event handlers

    terugButton.Click.Add(fun _ -> form.Close())
    form.Controls.Add(browser)
    form.Controls.Add(terugButton)
    form
