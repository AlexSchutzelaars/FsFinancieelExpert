#lang racket/gui

;;;; Eindewaarde-berekeningen (4 oktober 2025, 18:32)
;;;  Versies:
;;;  4 oktober 2025, 18:22: periodieke inleg toegevoegd aan berekening.
;;;  6 oktober 2025 16:32: pre- en postnumerando checkbox toegevoegd aan GUI, plus bugfix.
;;;  12 oktober 2025 10:55: paar kleine refactorings en initialisatie met rekenvoorbeeeld periodieke inleg
;;;  Scherm is nog lelijk. Ook voor Contante-waarde-berekeningen, enz.

(define (monetize getal)
  (~r getal #:precision '(= 2)))

(define INTEREST-FREQUENTIE*
  (list
   '(continu 100000)
   '(dag 360)
   '(wekelijks 52)
   '(maand 12)
   '(kwartaal 3)
   '(halfjaar 2)
   '(jaar 1)))

(define (zoek-duur-interest-interval naam-periode)
  (first (rest (assoc naam-periode INTEREST-FREQUENTIE*))))

(define (interest-frequentie-namen)
  (map (lambda (x) (symbol->string (first x))) INTEREST-FREQUENTIE*))

;;; Berekent eindwaarde van startkapitaal (SK) met gebruik van de e-macht

(define (bereken-eindwaarde-SK-continu hoofdsom interest-perunage aantal-jaren)
  (* hoofdsom (exp (* interest-perunage aantal-jaren))))

(define (bereken-eindwaarde-SK-discreet hoofdsom interest-perunage aantaljaren periode-naam)
  (let* [(n-tijdeenheden (zoek-duur-interest-interval periode-naam ))
         (grondtal (+ 1 (/ interest-perunage n-tijdeenheden)))
         (macht (* aantaljaren n-tijdeenheden))]
    (* hoofdsom (expt grondtal macht))))

(define (bereken-eindwaarde-SK hoofdbedrag interest aantal-termijnen periode-naam)
  ;;; rente-perunage: tussen 0 en 1.
  ;;; Berekent eindwaarde van startkapitaal (SK). Bijvoorbeeld:
  ;;; (bereken-eindwaarde-SK 1000 5 2 'maand)
  (let ((interest-perunage  (exact->inexact (/ interest 100))))
    (cond ((eq? periode-naam 'continu) (bereken-eindwaarde-SK-continu hoofdbedrag interest-perunage aantal-termijnen))
          (else (bereken-eindwaarde-SK-discreet hoofdbedrag interest-perunage aantal-termijnen periode-naam)))))

;;; Formule voor de som van een spaar-plan met betalingen (inleg) aan het begin van elke periode (pre-numerando).
;;; Formule: S(1 + i)(1 - (1 + i)^n)/-i
;;; Example 7.59
;;; (spaarplan-prenum 750 4.5 10) ==> 9630.88 (Basic mathematics for economists, p. 254)
(define (spaarplan-prenum inleg-per-periode interest aantal-termijnen)
  (let* [(interest-perunage (exact->inexact (/ interest 100)))]
    (* inleg-per-periode (/ (* (+ 1 interest-perunage) (- 1 (expt (+ 1 interest-perunage) aantal-termijnen))) (* -1 interest-perunage)))))

;;; Formule voor de som van een spaar-plan met betalingen (inleg) aan het einde van elke periode (post-numerando)
;;; TO DO: Werkt nu alleen voor tijdeenheid "jaar"!
;;; (spaar-plan-prenum 750 4.5 10) ==> 9630.88 (Basic mathematics for economists, p. 258).
;;; Formule: S(1 - (1 + i)^n)/-i
;;; Example 7.64
;;; (spaarplan-postnum 250 5 10) ==> 3144.47
(define (spaarplan-postnum inleg-per-periode interest aantal-termijnen)
  (let* [(interest-perun (exact->inexact (/ interest 100)))]
    (/ (* inleg-per-periode (- 1 (expt (+ 1 interest-perun) aantal-termijnen))) (* -1 interest-perun))))

(define (spaarplan-tests-1)
  (spaarplan-prenum 750 4.5 10)
  (spaarplan-postnum 250 5 10) )

;;; zelfde signatuur als TW in Excel, alleen laatste parameter toegevoegd
;;; hw = hoofdsom; betaling = inleg per periode
(define (toekomstigewaarde interest aantal-termijnen betaling hw type periode-naam)
  (let* [(eindwaarde-hw (bereken-eindwaarde-SK hw interest aantal-termijnen periode-naam))
         (eindwaarde-spaarplan
          (if (= type 0)
              ;;; TO DO: zorg ervoor dat het interest-interval correct verwerkt wordt.
              (spaarplan-postnum betaling interest aantal-termijnen)
              (spaarplan-prenum betaling interest aantal-termijnen)))]
    (+ eindwaarde-hw eindwaarde-spaarplan)))
    
;;; ********************************************
;;; De GUI
;;; ********************************************

(define (numerieke-waarde txtX)
  (let ((waarde (string->number (send txtX get-value))))
    (if (not waarde) 0
        waarde)))

;; Functie die het scherm opent voor de berekening van eindwaarde
(define (open-eindewaarde-frame)
  (define frame-eindewaarde (new frame% [label "Bereken eindwaarde van hoofdsom/periodieke inleg"] [width 400] [height 250]))

  ;; Hoofdcontainer
  (define main-panel (new vertical-panel% [parent frame-eindewaarde] [alignment '(left top)] [spacing 10]))

  ;; Horizontaal panel
  (define hpanel (new horizontal-panel% [parent main-panel] [alignment '(left center)] [spacing 10]))

  (define txtInleg (new text-field% [parent hpanel] [label "Inleg initieel:"] [init-value "0"] [min-width 150]))
  (define txtInlegPeriodiek (new text-field% [parent hpanel] [label "Inleg periodiek"] [min-width 150]))

  ;; Verticaal panel
  (define vpanel (new vertical-panel% [parent main-panel] [alignment '(left top)] [spacing 5]))
  (define txtInterest (new text-field% [parent vpanel] [label "Interest"] [min-width 50]))
  (define lbox-Periodenamen
    (new list-box%
         [parent vpanel]
         [label "Kies een interest-tijdinterval  "]
         [choices (interest-frequentie-namen)]))

  (define txtJaren (new text-field% [parent vpanel] [label "Aantal termijnen"] [min-width 50]))



  ;; Checkbox
  (define chkPostnumerando (new check-box%
                                [parent vpanel]
                                [value #t]
                                [label "Postnumerando"]))

  ;;; Levensverzekeringswiskunde en pensioencalculaties, p.10 ==> 25155.59 euro
  (send txtInlegPeriodiek set-value "0")
  (send txtInlegPeriodiek set-value "2000")
  (send txtInterest set-value "5")
  (send txtJaren set-value "10")
  (send chkPostnumerando set-value #t)
  
  ;;; Nu alleen postumerando = 0!
  (define (roepaan-bereken-eindwaarde) (let* [(w-hoofdsom (numerieke-waarde txtInleg))
                                              (w-inleg-periodiek (numerieke-waarde txtInlegPeriodiek))
                                              (w-interest (numerieke-waarde txtInterest))
                                              (w-aantal-termijnen (numerieke-waarde txtJaren))
                                              (w-periode (string->symbol (send lbox-Periodenamen get-string-selection)))
                                              (achteraf (send chkPostnumerando get-value))
                                              (w-prenum-postnum (if achteraf 0 1))]
                                         (monetize (toekomstigewaarde w-interest w-aantal-termijnen w-inleg-periodiek w-hoofdsom w-prenum-postnum w-periode))))
  ;;; (monetize (bereken-eindwaarde-SK waarde-hoofdsom waarde-rente waarde-aantal-termijnen waarde-periode))))

  ;; Horizontaal panel 2
  (define hpanel2 (new horizontal-panel% [parent main-panel] [alignment '(left center)] [spacing 10]))
  
  (define btnBereken (new button% [parent hpanel2] [label "Bereken eindwaarde"]
                          [callback (lambda (btn evt)                                                   
                                      (send txtEindwaarde set-value (roepaan-bereken-eindwaarde))
                                      (displayln "Knop 'Bereken' werd ingedrukt!") )]))

  
  (define txtEindwaarde (new text-field% [parent hpanel2] [label ""] [init-value ""] [min-width 50]))

  ;;; Jaar als default
  (send lbox-Periodenamen set-selection 6)
  (send frame-eindewaarde show #t))

;; Hoofdscherm met knop
(define main-frame (new frame% [label "Financieel expert"] [width 300] [height 100]))
(define main-panel (new vertical-panel% [parent main-frame] [alignment '(center center)] [spacing 10]))
(new button% [parent main-panel] [label "Berekening eindwaarde"]
     [callback (lambda (btn evt) (open-eindewaarde-frame))])

(send main-frame show #t)