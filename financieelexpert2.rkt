#lang racket


;;;; Eindewaarde-berekeningen (25 september 2025)
;;;; TODO: een GUI hieromheen bouwen. Ook Contante-waarde-berekeningen, enz.

(define (calculate-continuous-interest rate time)
  (exp (* rate time)))

(define RENTE-FREQUENTIE*
  (list
   '(continu 10000)
   '(dag 360)
   '(maand 12)
   '(kwartaal 3)
   '(halfjaar 2)
   '(jaar 1)))

(define (get-lengte-rente-periode naam-periode)
  (first (rest (assoc naam-periode RENTE-FREQUENTIE*))))

(define (eindwaarde hoofdbedrag rente-perc aantal-perioden periode-naam)
  ;;; Berekent eindwaarde
  ;;; (eindwaarde 1000 5 2 'maand)
  (let* [  (aantal-perioden-exact (* aantal-perioden (get-lengte-rente-periode periode-naam )))
           (rente-perunage (exact->inexact (/ rente-perc 100)))
           (perunage-exact (/ rente-perunage aantal-perioden-exact))
           (factor (+ 1 (exact->inexact perunage-exact)))
           (berekend 
            (case periode-naam
              ('continu (* hoofdbedrag (calculate-continuous-interest rente-perunage aantal-perioden)))
              (else (* hoofdbedrag (expt factor aantal-perioden-exact)))))]
    berekend))

;;;  Essential mathematics for economic analysis (Knut Sydsaeter e.a.), 5th editiob, p. 379
(let* [(periode-naam 'continu)
       (hoofdbedrag 5000)
       (rente-perc 9)
       (lengte-periode 8)
       (berekend 
        (eindwaarde hoofdbedrag rente-perc lengte-periode periode-naam))]
  (displayln (string-append "Eindbedrag met inleg " (number->string hoofdbedrag) " (" (symbol->string periode-naam)  ") € " (number->string berekend))))


(let* [(periode-naam 'continu)
       (hoofdbedrag 1)
       (rente-perc 2)
       (lengte-periode 1)
       (berekend 
        (eindwaarde hoofdbedrag rente-perc lengte-periode periode-naam))]
  (displayln (string-append "Eindbedrag met inleg " (number->string hoofdbedrag) " (" (symbol->string periode-naam)  ") € " (number->string berekend))))

;; passim, p. 380

(exp 0.08)
(calculate-continuous-interest 0.08 1)
(* 5000 (calculate-continuous-interest 0.09 8))