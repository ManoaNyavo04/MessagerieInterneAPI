CREATE OR REPLACE FUNCTION cast_valid_date(date_text VARCHAR)
RETURNS DATE
AS $$
DECLARE
    formatted_date_text VARCHAR;
    result_date DATE;
BEGIN
    formatted_date_text := REPLACE(date_text, '/', '-');
BEGIN
    result_date := TO_DATE(formatted_date_text, 'DD-MM-YYYY');
    EXCEPTION
        WHEN others THEN
            RETURN NULL;
        END;
    RETURN result_date;
END;
$$ LANGUAGE plpgsql;



CREATE OR REPLACE FUNCTION cast_valid_datetime(date_text VARCHAR)
RETURNS TIMESTAMP
AS $$
DECLARE
    result_date TIMESTAMP;
BEGIN
    BEGIN
        result_date := TO_TIMESTAMP(date_text, 'DD/MM/YYYY HH24:MI:SS');
    EXCEPTION
        WHEN others THEN
            RETURN NULL;
    END;
    RETURN result_date;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION cast_valid_datetime(date_text VARCHAR)
RETURNS TIMESTAMP
AS $$
DECLARE
    result_date TIMESTAMP;
BEGIN
    BEGIN
        -- Essai avec le format DD/MM/YYYY HH24:MI:SS
        result_date := TO_TIMESTAMP(date_text, 'DD/MM/YYYY HH24:MI:SS');
    EXCEPTION
        WHEN others THEN
            -- Si erreur, essai avec le format YYYY-MM-DD HH24:MI:SS
            BEGIN
                result_date := TO_TIMESTAMP(date_text, 'YYYY-MM-DD HH24:MI:SS');
            EXCEPTION
                WHEN others THEN
                    -- Si toujours une erreur, retourner NULL
                    RETURN NULL;
            END;
    END;
    RETURN result_date;
END;
$$ LANGUAGE plpgsql;



CREATE OR REPLACE FUNCTION is_numeric(num Text)
returns boolean
AS $$
BEGIN
    IF num ~ '^-?\d*\.?\d+$' THEN
        RETURN TRUE;
    ELSE
        RETURN FALSE;
    END IF;
END;
$$ language plpgsql;


CREATE OR REPLACE FUNCTION is_numeric_and_positive(num Text)
returns boolean
AS $$
BEGIN
    IF num ~ '^-?\d*\.?\d+$' THEN
        IF num::NUMERIC > 0 THEN
            RETURN TRUE;
        ELSE
            RETURN FALSE;
        END IF;
    ELSE
        RETURN FALSE;
    END IF;
END;
$$ language plpgsql;


------------------------------------------------------------------------------------------------------------------------------------------------------------

CREATE OR REPLACE FUNCTION insert_type_plat()
returns void as $$
begin

    insert into type_plat (name_type) 
        select
            distinct m1.type
        from meal_temp m1
        where not EXISTS (
            select 1 from type_plat tp 
            where tp. name_type = m1.type
        );

end;
$$ language plpgsql;


CREATE OR REPLACE FUNCTION insert_type_journee()
returns void as $$
begin

    insert into type_journee (name_type)
        select
            distinct m2.journee
        from meal_temp m2
        where not exists (
            select 1 from type_journee tj
            where tj.name_type = m2.journee
        );

end;
$$ language plpgsql;


create or REPLACE function insert_group()
returns void as $$
begin

    insert into plat (id_type_plat, id_type_journee, name_plat, date_plat)
        select 
            distinct tp.id_type_plat, tj.id_type_journee, m3.plat, (select cast_valid_date(m3.date))
        from meal_temp m3
        join type_plat tp on tp.name_type = m3.type
        join type_journee tj on tj.name_type = m3.journee
        where not exists (
            select 1 from plat p
            where p.id_type_plat = tp.id_type_plat
                AND p.id_type_journee = tj.id_type_journee
                AND p.name_plat = m3.plat
                AND p.date_plat = (select cast_valid_date(m3.date))
        );

END;
$$ LANGUAGE plpgsql;


create or REPLACE function insert_group_livraison()
returns void as $$
begin

    insert into livraison (id_user, id_plat, date_pointage)
        SELECT 
            DISTINCT u."Id", p.id_plat, cast_valid_datetime(lt.date_de_pointage)
        FROM livraison_temp lt
        JOIN public."Employe" e 
            ON CONCAT(TRIM(e."Nom"), ' ', TRIM(e."Prenom")) = TRIM(lt.nom_du_pax)
        JOIN public."User" u 
            ON u."EmployeId" = e."Id"
        JOIN (
            SELECT id_plat, TRIM(REPLACE(name_plat, E'\n', ' ')) AS name_plat, date_plat 
            FROM plat 
            WHERE date_plat = '2024-09-23'
        ) p 
            ON TRIM(REPLACE(p.name_plat, E'\n', ' ')) = TRIM(lt.plat) 
        and not exists (
            select 1 from livraison l
            where l.id_user = u."Id"
                AND l.id_plat = p.id_plat
                AND l.date_pointage =(select cast_valid_datetime(lt.date_de_pointage))
        );

END;
$$ LANGUAGE plpgsql;


-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- POB

CREATE OR REPLACE FUNCTION insert_rapprochement()
RETURNS void AS $$
BEGIN
    INSERT INTO rapprochement (date, purhase_nb, served_nb, ecart, facture_nb, pourcentage)
    SELECT 
        (pt.date)::date,
        (pt.plat_commande)::NUMERIC,
        (pt.plat_servi)::NUMERIC,
        (pt.ecart)::NUMERIC,
        (pt.plat_facturer)::NUMERIC,
        (pt.pourcentage)::NUMERIC
    FROM pob_temp pt
    WHERE NOT EXISTS (
        SELECT 1 FROM rapprochement r
        WHERE r.date = (pt.date)::date
          AND r.purhase_nb = (pt.plat_commande)::NUMERIC
          AND r.served_nb = (pt.plat_servi)::NUMERIC
          AND r.ecart = (pt.ecart)::NUMERIC
          AND r.facture_nb = (pt.plat_facturer)::NUMERIC
          AND r.pourcentage = (pt.pourcentage)::NUMERIC
    );
END;
$$ LANGUAGE plpgsql;

DELETE FROM rapprochement;
ALTER SEQUENCE rapprochement_id_rapprochement_seq RESTART WITH 1;



"C:\Program Files\PostgreSQL\12\bin\pg_dump.exe" -U postgres -d cantine_project -f "D:/Stage_Manoa/Projet/Cantine_project1/backup.sql"