#lang racket/gui

;;;; Eindewaarde-berekeningen (2 oktober 2025, 18:12)
;;;; MEE BEZIG een GUI hieromheen bouwen. Verwerking van Periodieke inleg nog toe te implementeren.
;;;  Scherm is nog lelijk. Ook voor Contante-waarde-berekeningen, enz.

(define RENTE-FREQUENTIE*
  (list
   '(continu 100000)
   '(dag 360)
   '(wekelijks 52)
   '(maand 12)
   '(kwartaal 3)
   '(halfjaar 2)
   '(jaar 1)))

(define (zoek-duur-rente-periode naam-periode)
  (first (rest (assoc naam-periode RENTE-FREQUENTIE*))))

(define (rente-frequentie-namen)
  (map (lambda (x) (symbol->string (first x))) RENTE-FREQUENTIE*))

;;; Berekent eindwaarde van startkapitaal (SK) met gebruik van de e-macht

(define (bereken-eindwaarde-SK-continu hoofdsom rente aantal-jaren)
  (* hoofdsom (exp (* (exact->inexact (/ rente 100)) aantal-jaren))))

;;; Berekent eindwaarde van startkapitaal (SK) op de gangbare manier
(define (bereken-eindwaarde-SK-discreet hoofdsom rente aantaljaren periode-naam)
  (let* [(rente-factor (zoek-duur-rente-periode periode-naam ))
         (rente-perunage (exact->inexact (/ rente 100)))]
    (* hoofdsom (expt (exact->inexact (+ 1 (/ rente-perunage rente-factor))) (* aantaljaren rente-factor)))))


(define (bereken-eindwaarde-SK hoofdbedrag rente-perc aantal-perioden periode-naam)
  ;;; Berekent eindwaarde van startkapitaal (SK). Bijvoorbeeld:
  ;;; (bereken-eindwaarde-SK 1000 5 2 'maand)
  (cond ((eq? periode-naam 'continu) (bereken-eindwaarde-SK-continu hoofdbedrag rente-perc aantal-perioden))
        (else (bereken-eindwaarde-SK-discreet hoofdbedrag rente-perc aantal-perioden periode-naam))))

;;; ********************************************
;;; De GUI
;;; ********************************************

(define (numerieke-waarde txtX)
  (string->number (send txtX get-value)))

;; Functie die het scherm opent voor de berekening van eindwaarde
(define (open-eindewaarde-frame)
  (define frame-eindewaarde (new frame% [label "Bereken eindwaarde"] [width 400] [height 250]))

  ;; Hoofdcontainer
  (define main-panel (new vertical-panel% [parent frame-eindewaarde] [alignment '(left top)] [spacing 10]))

  ;; Horizontaal panel
  (define hpanel (new horizontal-panel% [parent main-panel] [alignment '(left center)] [spacing 10]))

  (define txtInleg (new text-field% [parent hpanel] [label "Inleg initieel:"] [init-value "1000"] [min-width 150]))
  (define txtInlegPeriodiek (new text-field% [parent hpanel] [label "Inleg periodiek"] [init-value "10"] [min-width 150]))

  ;; Verticaal panel
  (define vpanel (new vertical-panel% [parent main-panel] [alignment '(left top)] [spacing 5]))
  (define txtRente (new text-field% [parent vpanel] [label "Rente"] [init-value "5"] [min-width 50]))
  (define lbox-Periodenamen
    (new list-box%
         [parent vpanel]
         [label "Kies een rente-interval  "]
         [choices (rente-frequentie-namen)]))

  (define txtJaren (new text-field% [parent vpanel] [label "Aantal jaren"] [init-value "2"] [min-width 50]))

  (define (roepaan-bereken-eindwaarde) (let* [(waarde-hoofdsom (numerieke-waarde txtInleg))
                                          (waarde-rente (numerieke-waarde txtRente))
                                          (waarde-jaren (numerieke-waarde txtJaren))
                                          (waarde-periode (string->symbol (send lbox-Periodenamen get-string-selection))) ]
                                       
                                     (number->string (bereken-eindwaarde-SK waarde-hoofdsom waarde-rente waarde-jaren waarde-periode))))
  
  (define btnBereken (new button% [parent vpanel] [label "Bereken eindwaarde"]
                          [callback (lambda (btn evt)
                                                      
                                      (send txtEindwaarde set-value (roepaan-bereken-eindwaarde))
                                      (displayln "Knop 'Bereken' werd ingedrukt!") )]))
  
  (define txtEindwaarde (new text-field% [parent vpanel] [label "Eindwaarde"] [init-value ""] [min-width 50]))

  ;;; Jaar als default
  (send lbox-Periodenamen set-selection 6)
  (send frame-eindewaarde show #t))

;; Hoofdscherm met knop
(define main-frame (new frame% [label "Financieel expert"] [width 300] [height 100]))
(define main-panel (new vertical-panel% [parent main-frame] [alignment '(center center)] [spacing 10]))
(new button% [parent main-panel] [label "Berekening eindwaarde"]
     [callback (lambda (btn evt) (open-eindewaarde-frame))])

(send main-frame show #t)