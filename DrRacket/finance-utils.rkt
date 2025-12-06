#lang racket

;;; Functies voor berekening van eindwarde (TW)
(provide monetize)

;;; (provide zoek-duur-rente-periode)
(provide rente-frequentie-namen)
;;; (provide bereken-eindwaarde-SK-continu)
;;; (provide bereken-eindwaarde-SK-discreet)
;;; (provide bereken-eindwaarde-SK)
(provide tw)


(define (monetize waarde)
  (string-append "€" " " (~r waarde #:precision '(= 2))))

(define *RENTE-FREQUENTIE*
  (list
   '(continu 100000)
   '(dag 360)
   '(wekelijks 52)
   '(maand 12)
   '(kwartaal 3)
   '(halfjaar 2)
   '(jaar 1)))

(define (zoek-duur-rente-periode naam-periode)
  (first (rest (assoc naam-periode *RENTE-FREQUENTIE*))))

(define (rente-frequentie-namen)
  (map (lambda (x) (symbol->string (first x))) *RENTE-FREQUENTIE*))

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

;;; zelfde signatuur als TW in Excel, alleen laatste parameter toegevoegd
;;; hw = hoofdsom; betaling = inleg per periode. NB: postief hier, negatief in Excel.
;;; Voorbeelden met PRENUMERANDO (Wim de Groot 2013, p. 102-103)
;;; periodieke betaling 1000 met startkapitaal 10.000
;;; (tw 0.05 10 1000 10000 1) ==> 29.496
;;; geen periodieke betalingen
;;; (tw 0.05 3 0 10000 1) ==> 11.576
;;; enkel betalingen van 1000 per jaar
;;; (tw 0.05 10 1000 0 1) ==> 13.207

(define (tw rente-perunage aantal-termijnen betaling hw type periode-naam)
  (let* [(eindwaarde-hw (bereken-eindwaarde-SK hw rente-perunage aantal-termijnen periode-naam))
         (eindwaarde-spaarplan
          (if (= type 0)
              (spaarplan-postnum betaling rente-perunage aantal-termijnen)
              (spaarplan-prenum betaling rente-perunage aantal-termijnen)))]
    (+ eindwaarde-hw eindwaarde-spaarplan)))


