# FsFinancieelExpert

Voorbeelden van financiële berekeningen in een F# Windows Forms applicatie. De naam van de applicatie zal in de toekomst aangepast worden. NB: De applicatie is nog in een ALPHA-status, en zal/moet verder ontwikkeld worden. Nu kan schrijver dezes (Alex Schutzelaars) er niet voor instaan dat alle berekende waarden correct zijn.

## Beschrijving

Eenvoudige financiële berekeningen (bijvoorbeeld: spaarplannen) in een F# Windows Forms applicatie.

- Hoofdscherm, met links (via knoppen) naar:
- Effectenportefeuille: berekening van de waarden van beleggingsfondsen (aandelen/obligaties). De data komen uit een XML-bestand.
- Berekeningen van toekomstige waarde op basis van een startkapitaal (eventueel aangevuld met een periodieke inleg), rentepercentage en looptijd (aantal jaren).
- Berekeningen van contante waarde op basis van een doelwaarde in de toekomst, rentepercentage en looptijd (aantal jaren)

De rekenfuncties voor de laatste formules zijn respectievelijk gebaseerd (zelfde parameters, in dezelfde volgorde) op de Excel-functies TW en HW.
Zij moeten dan ook dezelfde resultaten (afgezien van wat extra decimalen/afronding) opleveren als die functies in Excel, natuurlijk bij gelijke invoer.

## Hoe te beginnen?

### Afhankelijkheden
- Zorg ervoor dat .NET 9.0 (of later) is geïnstalleerd op de PC waarop je het programma wilt bouwen (build van de applicatiecode in .NET, vanaf Windows 10.)
Ove hoe je dat doet, zie: https://dotnet.microsoft.com/en-us/download/dotnet/9.0

### Installatie
- Zet de code in een nieuwe map. (ZIP vanaf deze GitHub repository, en pak die map uit in bijvoorbeeld een map "C:/Projecten"s.)
- Build het programma in PowerShell (Windows-toets+R: type "powershell"), vanaf het hoogste niveau van de aangemaakte map). Dat doe je met:
    #### dotnet build --configuration Release
    of:
    #### dotnet publish -c Release -r win-x64 --self-contained true
De tweede optie maakt een grotere map aan met een standalone executable. Voordeel: je hebt geen .NET runtime meer nodig op de PC waarop je het programma wilt draaien.
- Zoek het uitvoerbare bestand: FsFinancieelExpert.exe in de map. Bijvoorbeeld:  C:\Projecten\FinancieelExpert2\bin\Release\net9.0-windows\FinancieelExpert2.exe
- Optioneel: Maak een snelkoppeling naar het uitvoerbare bestand op je bureaublad.

### Uitvoeren van het programma

- Draai FinancieelExpert2.exe vanaf PowerShell of de CMD. Bijvoorbeeld in: C:\Projecten\FinancieelExpert2\bin\Debug\net9.0-windows\FinancieelExpert2.exe


## Help

Hier is wat uitleg over de functionaliteit. (De applicatie is nog in ontwikkeling, dus de functies zijn nog wat rudimentair.)
### Hoofdscherm: selecteer een van de knoppen om naar het scherm met de genoemde functie te gaan.
Effectenportefeuille: laad een XML-bestand met gegevens over effecten (aandelen, obligaties, etc.).
Het bestand vooronderstelt een bepaalde structuur. Als je een fonds selecteert, wordt de waarde van dat fonds (aantal stukken MAAL koers) getoond.
Een voorbeeldbestand is aanwezig in de map "VoorbeeldBestanden". De gegevens worden getoond in een tabel.
(Nog te ontwikkelen: berekening van totalen en rendementen.)
(Als je een ander XML-bestand wilt gebruiken, zorg er dan voor dat het dezelfde structuur heeft als het voorbeeldbestand. Je kunt het voorbeeldbestand aanpassen met je eigen gegevens, of een nieuw bestand maken met dezelfde structuur. Zet het wel in een andere map (is netter),

Toekomstige waarde: voer de gegevens in (huidige waarde, rentepercentage, aantal jaren), en klik op "Bereken".
De toekomstige waarde wordt getoond. (Je kunt ook de knop "Voorbeeldgegevens" gebruiken om voorbeeldgegevens in te vullen.) 
### Je kunt kiezen of je periodiek wilt inleggen (en zo ja, hoeveel per periode).
Bovendien kun je kiezen:
### of de inleg aan het begin of aan het einde van de periode plaatsvindt (respectievelijk pre- en postnumerando).
### of de rente jaarlijks, halfjaarlijks, per kwartaal, maandelijks of dagelijks wordt berekend.


Contante waarde: voer de gegevens in (toekomstige waarde (= doelwaarde in de toekomst), rentepercentage, aantal jaren), en klik op "Bereken". De contante waarde wordt getoond. Je kunt ook de knop "Voorbeeldgegevens" gebruiken om voorbeeldgegevens in te vullen.  

Tenslotte is er een Documentatie-knop die deze readme.md opent (en als *rich text* toont) in de standaard webbrowser.
## Auteurs

Alex Schutzelaars (a.schutzelaars@outlook.com)

## Versiegeschiedenis
* 0.3
(Nog te ontwikkelen: in voorbereiding.)
   * Verbetering layout van schermen (scherm voor contante waarde is onvolledig)
    Effectenportefeuille: berekenen van totalen, en rendement (1-jarig, 5-jarig)
   * refactoring
   * verbeteren van de berekeningen (met uitbreiding voor continue rente-bijschrijving)
   * voor contante waarde: het scherm uitbreiden
   * toevoegen van een unit test project (om de financiële berekeningen - in diverse scenario's -  te testen))
   * Wellicht omzetten in een XAML-applicatie, en/of een webversie.

* 0.2
    * Diverse verbeteringen.
    * Zie [commit change]() or See [release history]()
* 0.1
    * Eerste versie

## Licentie/License

This project is licensed under the [Creative Commons Legal Code] License - see the LICENSE.md file for details

## Acknowledgments
Hulp van CoPilot.