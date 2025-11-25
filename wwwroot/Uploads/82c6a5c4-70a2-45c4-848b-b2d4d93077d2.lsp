(defun c:GETSURFPOLY (/ *error* polyligne block mtext_area)
    (vl-load-com)

    (defun get_polyline ( / flag ename ent)
        (setq flag t)
        (while flag
            (if (setq ename(car(entsel "Sélectionner la polyligne")))
                (progn
                    (setq ent (entget ename))
                    (if (wcmatch (cdr (assoc 0 ent)) "LINE,*POLYLINE,*LWPOLYLINE,*REGION")
                        (setq flag nil)
                    )
                )
            )
        )
        ename
    )

    (defun get_block ( / flag ename ent)
        (setq flag t)
        (while flag
            (if (setq ename(car(entsel "Sélectionner le bloc")))
                (progn
                    (setq ent (entget ename))
                    (if (and (wcmatch (cdr (assoc 0 ent)) "INSERT")
                            (wcmatch (cdr (assoc 2 ent)) "Bloc renseignement piece"))
                        (setq flag nil)
                    )
                )
            )
        )
        ename
    )

    (defun get_mtext_attribut ( block / mtext sub_entity name ent)
        (setq mtext nil)
        (setq sub_entity block)
        (while  (= nil mtext)
            (setq sub_entity (entnext sub_entity))
            (setq ent (entget sub_entity))
            (setq name (cdr (assoc 2 ent)))
            (if (= name "SURF-PIECE")
                (setq mtext sub_entity)
            )
        )
        mtext
    )

    (defun LM:quickfield:acdoc nil
        (eval (list 'defun 'LM:quickfield:acdoc 'nil (vla-get-activedocument (vlax-get-acad-object))))
        (LM:quickfield:acdoc)
    )

    (defun set_mtext_field ( polyligne mtxt / txt-obj utility polyligne_id vla_polyline text)
        (setq txt-obj (vlax-ename->vla-object mtxt))
        (if (vlax-write-enabled-p txt-obj)
            (progn
                (setq vla_polyline (vlax-ename->vla-object polyligne))
                (setq utility (vla-get-utility (LM:quickfield:acdoc)))
                (setq polyligne_id 
                    (if (vlax-method-applicable-p utility 'getobjectidstring)
                        (vlax-invoke utility 'getobjectidstring vla_polyline acfalse)
                        (itoa (vla-get-objectid vla_polyline))
                    ))
                (setq text (strcat
                    "%<\\AcObjProp.16.2 Object(%<\\_ObjId " polyligne_id ">%).Area \\f \"%lu2%pr2%ps[,]\">%"))
                (vla-put-textstring txt-obj "")
                (vla-put-textstring txt-obj text)
            )
        )
    )
    
    (defun *error* ( msg )
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
        )
        (princ)
    )

    (setq polyligne (get_polyline))
    (setq block (get_block))
    (setq mtext_area (get_mtext_attribut block))
    (set_mtext_field polyligne mtext_area)
    (command "_updatefield" block "")
)