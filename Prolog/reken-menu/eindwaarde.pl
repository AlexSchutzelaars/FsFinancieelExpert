% eindwaarde.pl
:- module(eindwaarde, [bereken_eindwaarde/0]).

% Interestperunage is nu afhankelijk van tijdinterval (jaar,
% maand, dagekijks (360) enz.)
% Versie van donderdag 4 december 2025

interest_interval_rekenfactor(dag,    360) .
interest_interval_rekenfactor(week,    52) .
interest_interval_rekenfactor(maand,   12) .
interest_interval_rekenfactor(kwartaal, 4) .
interest_interval_rekenfactor(jaar,     1) .


interest_pars(Interest, Looptijd, Interval , Perunage, AantalTijdeenheden) :-
   interest_interval_rekenfactor(Interval, Rekenfactor) ,
   Perunage is (Interest / 100) / Rekenfactor ,
   AantalTijdeenheden is Looptijd * Rekenfactor .

tw(_, _, _, onbekend, 0) .

tw(Bedrag, Interest, Looptijd, Interval, EW) :-
    interest_pars(Interest, Looptijd, Interval, Perunage, AantalTijdeenheden) ,
    EW is Bedrag*(1 + Perunage)^AantalTijdeenheden .

% Berekening was niet mogelijk.

rapporteer_tw(Bedrag, Looptijd, Interest, Interval, _) :-
   Interval = onbekend ,
   string_upper(Interval, IntervalHL) ,
   format('~nDe eindwaarde voor inleg ~w en interest ~w%, met interval ~s kan NIET berekend worden.~n', [Bedrag, Interest, IntervalHL]) ,
   format('(De looptijd is ~w jaar.)~n', Looptijd) .

% Print bedragen met 2 decimalen

rapporteer_tw(Bedrag, Looptijd, Interest, Interval, EW) :-
   string_upper(Interval, IntervalHL) ,

   format('~nDe eindwaarde voor inleg ~w en interest ~w%, met interval ~s is: EUR ~2f.~n', [Bedrag, Interest, IntervalHL, EW]) ,
   format('(De looptijd is ~w jaar.)~n', Looptijd) .

rapporteer_antwoord(Antwoord) :-
    (   Antwoord = 'dag' ->
        writeln('Je hebt gekozen voor dagelijkse berekening.')
    ;   Antwoord = 'week' ->
        writeln('Je hebt gekozen voor wekelijkse berekening.')
    ;   Antwoord = 'maand' ->
        writeln('Je hebt gekozen voor maandelijkse berekening.')
    ;   Antwoord = 'kwartaal' ->
        writeln('Je hebt gekozen voor kwartaalberekening.')
    ;   Antwoord = 'halfjaar' ->
        writeln('Je hebt gekozen voor halfjaar-berekening.')
    ;   Antwoord = 'jaar' ->
        writeln('Je hebt gekozen voor jaarlijkse berekening.')
    ;   Antwoord = 'onbekend' ->
        writeln('Je hebt geen geldige keuze gemaakt.')
    ) .

bereken_eindwaarde :-
    format('~nFinanciele berekeningen in SWI-Prolog voor einde-waarde (2 december 2025 17:46)~n~n') ,
    vragen(Vragen) ,
    toon_vragen(Vragen) ,
    write('Kies het nummer van je keuze: ') ,

    read_line_to_string(user_input, Input) ,
    (   number_string(Nummer, Input) ,
        nth1(Nummer, Vragen, Antw) ,
        length(Vragen, Length) ,
        Nummer =< Length ,
        Antwoord = Antw -> true
        ;   Antwoord = 'onbekend'
    ) ,

    rapporteer_antwoord(Antwoord) ,
    Bedrag = 1000, Interest is 5, Looptijd is 2 ,
    tw(Bedrag, Interest, Looptijd, Antwoord, EW) ,
    rapporteer_tw(Bedrag, Looptijd, Interest, Antwoord, EW) .

vragen([
    'dag',
    'week',
    'maand',
    'kwartaal',
    'halfjaar',
    'jaar'
]).

toon_vragen(Vragen) :-
    forall(nth1(I, Vragen, Vraag),
           format('~w. ~w~n', [I, Vraag])).


