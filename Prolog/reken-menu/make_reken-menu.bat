@echo off
REM Definieer de basisdirectory en bestandsnaam (zonder extensie)
set DIR=C:\Users\aschu\Documents\Prolog\reken-menu
set NAME=reken-menu

REM Pad naar SWI-Prolog
set SWI="C:\Program Files\swipl\bin\swipl.exe"

REM Compileer naar een standalone executable
%SWI% --goal=main --stand_alone=true -o %DIR%\%NAME%.exe -c %DIR%\%NAME%.pl
