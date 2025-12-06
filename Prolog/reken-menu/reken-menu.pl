% rekenmenu.pl
% Versie van 4 december 2025 (SWI-Prolog 10.0.0)
% Hoofdingang voor reken-menu -- een SWI-Prolog applicatie.
% Toont een menu en roept de juiste functies uit de modules aan.

:- use_module(kwadrateren).
:- use_module(eindwaarde).
:- use_module(vaarwel).

main :-
    format('Welkom bij SWI-Prolog App REKEN-MENU (v1.0 - 04 december 22:34)~n~n') ,
    menu_loop.

menu_loop :-
    format('Keuze-menu REKEN-MENU~n~n') ,
    format('1. Kwadrateer getal~n'),
    format('2. Bereken eindwaarde~n'),
    format('3. Exit~n'),
    format('Kies een optie: >>> '),
    readln([Keuze|_]) ,
    handle_choice(Keuze).

handle_choice(1) :-
    rekenvraag ,
    menu_loop.
handle_choice(2) :-
    bereken_eindwaarde,
    menu_loop.
handle_choice(3) :-
    zeg_tot_ziens,
    % format('Harde stop ...~n'),
    halt.
handle_choice(_) :-
    format('Ongeldige keuze, probeer het opnieuw.~n'),
    menu_loop.

