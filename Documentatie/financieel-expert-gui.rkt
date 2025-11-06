#lang racket/gui

;;; Eindewaarde-berekeningen (4 oktober 2025, 18:32)
;;;  Versies:
;;;  4 oktober 2025, 18:22: periodieke inleg toegevoegd aan berekening.
;;;  6 oktober 2025 16:32: pre- en postnumerando checkbox toegevoegd aan GUI, plus bugfix.
;;;  18 oktober 2025: EURO-teken vóór de berekende waarde.
;;;  30 oktober: berekeningen vereenvoudigd (rente-perunage dctraal)
;;;  Scherm is nog lelijk. Ook voor Contante-waarde-berekeningen, enz.

(define (monetize waarde)
  (string-append "€" " " (~r waarde #:precision '(= 2))))

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

(define (bereken-eindwaarde-SK-continu hoofdsom rente-perunage aantal-jaren)
  (* hoofdsom (exp (* rente-perunage aantal-jaren))))

;;; Berekent eindwaarde van startkapitaal (SK) op de gangbare manier
;; rente-perunage: tussen 0 en 1.
(define (bereken-eindwaarde-SK-discreet hoofdsom rente-perunage aantaljaren periode-naam)
  (let* [(rente-factor (zoek-duur-rente-periode periode-naam ))]
    (* hoofdsom (expt (+ 1 (/ rente-perunage rente-factor)) (* aantaljaren rente-factor)))))


(define (bereken-eindwaarde-SK hoofdbedrag rente-perunage aantal-termijnen periode-naam)
  ;; rente-perunage: tussen 0 en 1.
  ;;; Berekent eindwaarde van startkapitaal (SK). Bijvoorbeeld:
  ;;; (bereken-eindwaarde-SK 1000 0.05 2 'maand)
    (cond ((eq? periode-naam 'continu) (bereken-eindwaarde-SK-continu hoofdbedrag rente-perunage aantal-termijnen))
          (else (bereken-eindwaarde-SK-discreet hoofdbedrag rente-perunage aantal-termijnen periode-naam))))

;;; Formule voor de som van een spaar-plan met betalingen (inleg) aan het begin van elke periode (pre-numerando).
;;; Formule: S(1 + i)(1 - (1 + i)^n)/-i
;;; Example 7.59
;;; (spaarplan-prenum 750 0.045 10) ==> 9630.88 (Basic mathematics for economists, p. 254)
(define (spaarplan-prenum inleg-per-periode rente-perunage aantal-termijnen)
  (* inleg-per-periode (/ (* (+ 1 rente-perunage) (- 1 (expt (+ 1 rente-perunage) aantal-termijnen))) (* -1 rente-perunage))))

;;; Formule voor de som van een spaar-plan met betalingen (inleg) aan het einde van elke periode (post-numerando)
;;; Basic mathematics for economists, p. 258.
;;; Formule: S(1 - (1 + i)^n)/-i
;;; Example 7.64
;;; (spaarplan-postnum 250 0.05 10) ==> 3144.47
(define (spaarplan-postnum inleg-per-periode rente-perunage aantal-termijnen)
  (/ (* inleg-per-periode (- 1 (expt (+ 1 rente-perunage) aantal-termijnen))) (* -1 rente-perunage)))

(define (spaarplan-tests-1)
  (spaarplan-prenum  750 0.045 10)
  (spaarplan-postnum 250 0.050 10) )

;;; zelfde signatuur als TW in Excel, alleen laatste parameter toegevoegd
;;; hw = hoofdsom; betaling = inleg per periode
(define (tw rente-perunage aantal-termijnen betaling hw type periode-naam)
  (let* [(eindwaarde-hw (bereken-eindwaarde-SK hw rente-perunage aantal-termijnen periode-naam))
         (eindwaarde-spaarplan
          (if (= type 0)
              (spaarplan-postnum betaling rente-perunage aantal-termijnen)
              (spaarplan-prenum betaling rente-perunage aantal-termijnen)))]
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


  ;;; Nu alleen posntumerando = 0!
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

;; Hoofdscherm met knop
(define main-frame (new frame% [label "Financieel expert"] [width 300] [height 100]))
(define main-panel (new vertical-panel% [parent main-frame] [alignment '(center center)] [spacing 10]))
(new button% [parent main-panel] [label "Berekening eindwaarde"]
     [callback (lambda (btn evt) (open-eindewaarde-frame))])

(send main-frame show #t)