#lang racket/gui

;;; Eindewaarde-berekeningen (4 oktober 2025, 18:32)
;;;  Versies:
;;;  4 oktober 2025, 18:22: periodieke inleg toegevoegd aan berekening.
;;;  6 oktober 2025 16:32: pre- en postnumerando checkbox toegevoegd aan GUI, plus bugfix.
;;;  18 oktober 2025: EURO-teken vóór de berekende waarde.
;;;  30 oktober 2025: berekeningen vereenvoudigd (rente-perunage centraal)
;;;  08 november 2025: paar voorbeelden toegevoegd
;;;  10 november 2025: 2 tests toegevoegd
;;;  12 november 2025: meerdere modules + tests toegevoegd
;;;  14 november 2025: 4 test-functies gerefactored
;;;  15 november 2025: en knop voor de rekentests toegevoegd.

;;;  Scherm is nog lelijk. Ook voor Contante-waarde-berekeningen, enz.

(require "finance-utils.rkt")

;;; *** TESTS ****

(define (rapporteer-spaarplan-tests test-description waarde-berekend waarde-verwacht)
  (let*  [(message-correct (format "CORRECT~n~a~n[~a / ~a]" test-description waarde-berekend waarde-verwacht))
          (message
            (if (equal? waarde-berekend waarde-verwacht)
                message-correct
                (string-append "NIET " message-correct "!")))]
    (message-box "EINDWAARDE" message)))


(define (spaarplan-tests-prenum-1)
  (let* [(test-description (format "Periodieke inleg 1000 per jaar~nmet startkapitaal 10.000 gedurende 10 jaar met 5 % interest [PRENUMERANDO] (Wim de Groot)"))
         (waarde-berekend (monetize (tw 0.050 10 1000 10000 1 'jaar)))
         (waarde-verwacht "€ 29495.73")]
    (rapporteer-spaarplan-tests test-description waarde-berekend waarde-verwacht)))

(define (spaarplan-tests-prenum-2)
  (let* [(test-description (format "Periodieke inleg 1000 per jaar zonder startkapitaal gedurende 10 jaar met 5 % interest [PRENUMERANDO] (Wim de Groot)"))
         (waarde-berekend (monetize (tw 0.050 10 1000 0 1 'jaar)))
         (waarde-verwacht "€ 13206.79")]
    (rapporteer-spaarplan-tests test-description waarde-berekend waarde-verwacht)))

(define (spaarplan-tests-prenum-3)
  ;;; periodieke betaling 750 per jaar zonder startkapitaal gedurende 10 jaar met 4.5 % interest
  ;;; Example 7.59 ==> 9630.88 (Basic mathematics for economists, p. 254)
  (let [(test-description (format "Periodieke inleg 750 per jaar zonder startkapitaal gedurende 10 jaar~nmet 4.5 % interest PRENUMERANDO (Wim de Groot Example 7.59)"))
        (waarde-berekend (monetize (tw 0.045 10 750 0 1 'jaar)))
        (waarde-verwacht "€ 9630.88")]  
    (rapporteer-spaarplan-tests test-description waarde-berekend waarde-verwacht)))

(define (spaarplan-tests-postnum-1)
  ;;; Example 7.64 (Basic mathematics for economists) ==> 3144.47
  (let [(test-description (format "Periodieke inleg 250 per jaar zonder startkapitaal gedurende 10 jaar met 5 % interest (POSTNUMERANDO)"))
        (waarde-berekend (monetize (tw 0.050 10 250 0 0 'jaar)))
        (waarde-verwacht "€ 3144.47")]  
    (rapporteer-spaarplan-tests test-description waarde-berekend waarde-verwacht)))

(define (spaarplan-tests)
  (spaarplan-tests-prenum-1)
  (spaarplan-tests-prenum-2)
  (spaarplan-tests-prenum-3)
  (spaarplan-tests-postnum-1))

;;; ********************************************
;;; De GUI
;;; ********************************************

(define (numerieke-waarde txtX)
  (let ((waarde (string->number (send txtX get-value))))
    (if (not waarde) 0
        waarde)))

;; Functie die het scherm opent voor de berekening van eindwaarde
(define (open-eindewaarde-dialog)
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

  (define txtJaren (new text-field% [parent vpanel] [label "Aantal termijnen"] [init-value "2"] [min-width 50]))

  ;; Checkbox
  (define chkPostnumerando? (new check-box%
                                 [parent vpanel]
                                 [value #t]
                                 [label "Postnumerando"]))


  ;;; NB: postnumerando = 0!
  (define (roepaan-bereken-eindwaarde) (let* [(w-hoofdsom (numerieke-waarde txtInleg))
                                              (w-inleg-periodiek (numerieke-waarde txtInlegPeriodiek))
                                              (w-rente-perunage (exact->inexact (/ (numerieke-waarde txtRente) 100)))
                                              (w-aantal-termijnen (numerieke-waarde txtJaren))
                                              (w-periode (string->symbol (send lbox-Periodenamen get-string-selection)))
                                              (w-prenum-postnum (if (send chkPostnumerando? get-value) 0 1))]
                                         (monetize (tw w-rente-perunage w-aantal-termijnen w-inleg-periodiek w-hoofdsom w-prenum-postnum w-periode))))

  ;; Horizontaal panel 2
  (define hpanel2 (new horizontal-panel% [parent main-panel] [alignment '(left center)] [spacing 10]))
  
  (define btnBereken (new button% [parent hpanel2] [label "Bereken eindwaarde"]
                          [callback (lambda (btn evt)                                                   
                                      (send txtEindwaarde set-value (roepaan-bereken-eindwaarde)))]))
  
  (define txtEindwaarde (new text-field% [parent hpanel2] [label ""] [init-value ""] [min-width 50]))

  ;;; Jaar als default
  (send lbox-Periodenamen set-selection 6)
  (send frame-eindewaarde show #t))

;; Hoofdscherm met 2 knoppen
(define main-frame (new frame% [label "Financieel expert"] [width 300] [height 100]))
(define main-panel (new vertical-panel% [parent main-frame] [alignment '(center center)] [spacing 10]))
(new button% [parent main-panel] [label "Berekening eindwaarde"]
     [callback (lambda (btn evt) (open-eindewaarde-dialog))])
(new button% [parent main-panel] [label "Spaarplan-tests"]
     [callback (lambda (btn evt) (spaarplan-tests))])

(send main-frame show #t)

