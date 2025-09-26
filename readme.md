# FinancieelExpert2

Voorbeelden van financiële berekeningen in een F# Windows Forms applicatie. De naam van de applicatie zal in de toekmst aangepast worden. NB: De applicatie is nog in een ALPHA-status.

## Beschrijving

Voorbeelden van financiële berekeningen in een F# Windows Forms applicatie.

- Hoofdscherm, met links (via knoppen) naar:
- Effectenportefeuille berekening (data uit XML-bestand)
- Berekeningen van toekomstige waarde
- Berekeningen van contante waarde

## Hoe te beginnen?

### Afhankelijkheden
* Zorg ervoor dat .NET 9.0 (of later) is geïnstalleerd op je PC. (Vanaf Windows 10.)
Voor hoe je dat doet, zie: https://dotnet.microsoft.com/en-us/download/dotnet/9.0

### Installatie
* Zet de code in een map. (ZIP vanaf deze GitHub repository, en pak die map uit in bijvoorbeeld een map C:/Projecten.)
* Build het programma in PowerShell (Windows toets+R: type "powershell"), vanaf het hoogste niveau van de aangemaakte map). Dat doe je met:
** dotnet build --configuration Release
** dotnet publish -c Release -r win-x64 --self-contained true (optioneel, voor een standalone executable). Deze optie maakt een grotere map aan, maar je hebt geen .NET runtime meer nodig op de PC waarop je het programma wilt draaien.
* Zoek het uitvoerbare bestand: FinancieelExpert2.exe in de map. Bijvoorbeeld:  C:\Projecten\FinancieelExpert2\bin\Release\net9.0-windows\FinancieelExpert2.exe
* Optioneel: Maak een snelkoppeling naar het bestand op je bureaublad.

### Uitvoeren van het programma

* Draai FinancieelExpert2.exe vanaf PowerShell of de CMD. Bijvoorbeeld in: C:\Projecten\FinancieelExpert2\bin\Debug\net9.0-windows\FinancieelExpert2.exe


## Help

Geen opmerkingen.

## Auteurs

Alex Schutzelaars

## Versiegeschiedenis
* 0.3
(Nog te ontwikkelen: in voorbereiding.)
   * Verbetering layout van schermen (scherm voor contante waarde is onvolledig)
    Effectenportefeuille: berekenen van totalen, en rendement (1-jarig, 5-jarig)
   * refactoring
   * verbeteren van de berekeningen
   * toevoegen van een unit test project.
   * Wellicht omzetten in een XAML-applicatie, en/of een webversie.

* 0.2
    * Diverse verbeteringen.
    * Zie [commit change]() or See [release history]()
* 0.1
    * Eerste versie

## Licentie/License

This project is licensed under the [NAME HERE] License - see the LICENSE.md file for details

## Acknowledgments
Hulp van CoPilot.