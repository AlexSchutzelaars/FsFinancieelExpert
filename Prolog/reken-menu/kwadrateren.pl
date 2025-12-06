% kwadrateren.pl
:- module(kwadrateren, [rekenvraag/0]).

rekenvraag :-
   format('~nVoer een getal in: >> ') ,
   readln([Number|_]) ,
   Squared is Number * Number ,
   format('De uitkomst is: ~w~n~n', Squared) .
