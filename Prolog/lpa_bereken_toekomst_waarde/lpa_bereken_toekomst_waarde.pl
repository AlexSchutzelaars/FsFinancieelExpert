% lpa_bereken_toekomst_waarde.pl

replace( From, To, Input, Output ) :-
   replace( From, To ) <~ Input ~> Output.

replace( From, _ ) :-
   find( From, 2, Found ),
   Found = ``,
   !.

replace( From, To ) :-
   write( To ) ,
   replace( From, To ).

ontleed_getal(GetalAsString, Getal) :-
    number_string(Getal, GetalAsString) ,
    !.

ontleed_getal(GetalAsString, Getal) :-
    replace(`,`, `.`, GetalAsString, GetalAsString2) ,
    number_string(Getal, GetalAsString2) .

bepaal_frequentie_per_jaar(`jaarlijks`, 1) :-
   ! .

bepaal_frequentie_per_jaar(`maandelijks`, 12) :-
   ! .

bepaal_frequentie_per_jaar(`dagelijks`, 360) :-
     ! .

bepaal_frequentie_per_jaar(`continu`, 10000) :-
   ! .

laatste_parameters(Hoofdsom, Rente, InlegPeriodiekRente, Looptijd, Interval) :-
  tw_parameters(Hoofdsom, Rente, InlegPeriodiekRente, Looptijd, Interval ) , 
  ! .

laatste_parameters(Hoofdsom, Rente, InlegPeriodiekRente, Looptijd, Interval) :-
  Hoofdsom is 1000 ,
  Rente is 5 ,
  InlegPeriodiekRente is 0 ,
  Interval is `jaarlijks` ,
  assert(tw_parameters(Hoofdsom, Rente, InlegPeriodiekRente, Looptijd, Interval)).

% test the listbox by getting the current answer

% 90 voor Interval, 100 voor pxNumerando
haal_listbox_waarde_op(Id_lbox, (Name,Value) ) :-
   wlstsel( (fred,Id_lbox), Item ),
   wlstget( (fred,Id_lbox), Item, Name, Value ).

vul_intervallijst :-
   IntervalListbox is 90 ,
   wlstadd( (fred,IntervalListbox ), 0, `continu`,      1 ),
   wlstadd( (fred,IntervalListbox ), 1, `dagelijks`,    2 ),
   wlstadd( (fred,IntervalListbox ), 2, `maandelijks`,  3 ),
   wlstadd( (fred,IntervalListbox ), 3, `jaarlijks`,    4 ) ,
   % Kies 'jaarlijks' als default
   wlstsel( (fred,IntervalListbox ), 3 ).

vul_pxnumerandolijst :-
   wlstadd( (fred,100), 0, `vooraf`,      1 ),
   wlstadd( (fred,100), 1, `achteraf`,    2 ),
   % Kies `achteraf` als default
   wlstsel( (fred,100), 1 ).

number_dialog( HoofdsomStr, InlegStr, RenteStr, LooptijdStr, Interval, Pxnumerando) :-
   DS = [ws_popup,ws_caption] ,
   BS = [ws_child,ws_visible, bs_defpushbutton] ,
   % BS2 = [ws_child,ws_visible, bs_checkbox] ,
   ES = [ws_child,ws_border,ws_visible,es_multiline] ,
   SS = [ws_child,ws_visible,ss_left] ,
   LS = [ws_child,ws_visible,ws_ex_clientedge,ws_tabstop,ws_vscroll,lbs_sort],
   Left1 is 10 ,
   Left2 is Left1 + 90 ,
   Left3 is Left2 + 190 ,
   TopHoofdsom is 10 ,
   TopInleg is TopHoofdsom + 50 ,
   TopRente is TopInleg + 50 ,
   TopLooptijd is TopRente + 50 ,
   TopRenteinterval is TopLooptijd + 50 ,

   wdcreate( fred, `Voer de parameters op voor de TW-berekening`,  90, 90, 800, 300, DS ),

   wccreate( (fred,  10), static, `Hoofdsom`,        Left1,   TopHoofdsom,  80,  30, SS ) ,
   wccreate( (fred,  11), edit,   ``,                Left2,   TopHoofdsom,  80,  30, ES ) ,
   
   wccreate( (fred,  80), button, `Bereken`,         Left3,   TopHoofdsom,  80,  30, BS ) ,

   wccreate( (fred,  20), static, `Inleg`,           Left1,   TopInleg,  80,  30, SS ) ,
   wccreate( (fred,  21), edit,   ``,                Left2,   TopInleg,  80,  30, ES ) ,

   wccreate( (fred,  30), static, `Rente`,           Left1,   TopRente,  80,  30, SS ) ,
   wccreate( (fred,  31), edit,   ``,                Left2,   TopRente,  80,  30, ES ) ,

   wccreate( (fred,  40), static, `Looptijd`,        Left1,   TopLooptijd ,  80,  30, SS ) ,
   wccreate( (fred,  41), edit,   ``,                Left2,   TopLooptijd ,  80,  30, ES ) ,
   wccreate( (fred,  90), listbox, `Rente-interval`, Left1, TopRenteinterval , 120, 120, LS ) ,
   wccreate( (fred, 100), listbox, `Achteraf`,       Left3, TopRenteinterval ,  80,  80, LS ) ,

   vul_intervallijst ,
   vul_pxnumerandolijst ,
   wtext( (fred, 11), `1000`) ,
   wtext( (fred, 21), `0`) ,
   wtext( (fred, 31), `5.5`) ,
   wtext( (fred, 41), `3`) ,
   size_dialog( fred, 0 ) ,
   call_dialog( fred, _ ) ,

   % Haal ingevoerde waarden op
   wtext( (fred, 11), HoofdsomStr) ,

   wtext( (fred, 21), InlegStr) ,
   wtext( (fred, 31), RenteStr ) ,
   wtext( (fred, 41), LooptijdStr) ,
   haal_listbox_waarde_op(90, (Interval, _)) ,
   haal_listbox_waarde_op(100, (Pxnumerando, _) ) .

% TODO: refactoren !!!

% Postnumerando
bereken_EW_HS_PreOfPost(`achteraf`, Hoofdsom, RentePerunage, Looptijd, Freq, Resultaat) :-
     Resultaat is Hoofdsom * (1 + RentePerunage) ^ (Looptijd * Freq) .

% Prenumerando
bereken_EW_HS_PreOfPost(`vooraf`, Hoofdsom, RentePerunage, Looptijd, Freq, Resultaat) :-
     ResultaatPost = Hoofdsom * (1 + RentePerunage) ^ (Looptijd * Freq) ,
     Resultaat is (1 + RentePerunage) * ResultaatPost .

bereken_EW_periodieke_inleg(`achteraf`, Hoofdsom, RentePerunage, Looptijd, Freq, Resultaat) :-
    Resultaat is Hoofdsom * ((1 + RentePerunage) ^ (Looptijd * Freq) - 1) / RentePerunage.

bereken_EW_periodieke_inleg(`vooraf`, Hoofdsom, RentePerunage, Looptijd, Freq, Resultaat) :-
    Resultaat is Hoofdsom * (1 + RentePerunage) * ((1 + RentePerunage) ^ (Looptijd * Freq) - 1) / RentePerunage.

% Getest met bedrag van 3000 euro jaarlijks ingelegd ACHTERAF gedurende 30 jaar met een interestvergoeding
% van 4 % per jaar ==> 168254.81 euro (p. 11 Financiële Rekenkunde).

bereken_EW_Spaarplan(Pxnumerando, Hoofdsom, InlegPeriodiek, RentePerunage, Looptijd, Freq, EW) :-
   bereken_EW_HS_PreOfPost(Pxnumerando, Hoofdsom, RentePerunage, Looptijd, Freq, EW_Hoofdsom) ,
   bereken_EW_periodieke_inleg(Pxnumerando, InlegPeriodiek, RentePerunage, Looptijd, Freq, EW_Inleg) ,
   EW is EW_Hoofdsom + EW_Inleg .

run :-
  number_dialog(HoofdsomStr, InlegStr, RenteStr, LooptijdStr, Interval, Pxnumerando) ,
  ontleed_getal(HoofdsomStr, Hoofdsom) ,
  ontleed_getal(InlegStr, InlegPeriodiek) ,
  ontleed_getal(RenteStr, RentePerc) ,
  ontleed_getal(LooptijdStr, Looptijd) ,

  bepaal_frequentie_per_jaar(Interval, Freq) ,
  RentePerunage is (RentePerc / 100) / Freq ,

  bereken_EW_Spaarplan(Pxnumerando, Hoofdsom, InlegPeriodiek, RentePerunage, Looptijd, Freq, Eindwaarde) ,
       
  (       write('De eindwaarde voor uw spaarplan met:'), 
          write('~M~J- hoofdsom ') , write(Hoofdsom) ,
		write('~M~J- inleg ') , write(InlegPeriodiek) ,
		write('~M~J- interest-percentage ') , write(RentePerc) ,
		write('~M~J- looptijd ') , write(Looptijd) ,
		write('~M~J- interval "') , write(Interval) , write('"') ,
		write('~M~J~M~J[De bijschrijving gebeurt: ') , write(Pxnumerando) , write(']') ,
          write('~M~Jbedraagt: '), write( Eindwaarde ), write('.'), nl) ~> Message ,
  msgbox( `De berekende eindwaarde is `, Message, 48, D ). 

% main hook used when running this program as a stand-alone application

app_main_hook :-
  (  write( `Berekening eindwaarde voor uw spaarplan~M~J` ) ,
     write( `------------------------------------------~M~J~M~J` ) ,
     write( `Copyright AAJH Schutzelaars~M~J20 december 2025~M~J` ) ,
     write( `` )
  ) ~> AboutString,
  bdsbox( AboutString, 1 ),
  pause( 4000 ),
  bdsbox( ``, -1 ),
  abort.

% abort hook used when running this program as a stand-alone application

app_abort_hook :-
  (  run
  -> repeat,
     wait( 0 ),
     /* Yes en No knoppen */
     msgbox( `Opnieuw berekenen`, `Nog een keer?`, 36, Yes ),
     (  Yes = 6 -> abort
     ;  ( msgbox( `Tot ziens`, `Het rekenprogramma is klaar`, 48, _ ), halt )
     )
  ;  halt( 1 )
  ).


