-- =====================================================================================
-- Datos de prueba del modulo contable — limpieza
--
-- Borra unicamente los asientos cuya leyenda arranca con el prefijo [QA], que son los que
-- genero generar.sql. No toca ninguna otra fila ni ninguna otra tabla.
--
--   sqlplus EJECUCION/... @limpiar.sql
-- =====================================================================================

set serveroutput on size unlimited
set feedback off
set linesize 140

DECLARE
    v_prefijo    CONSTANT VARCHAR2(10) := '[QA]';
    v_asientos   NUMBER;
    v_renglones  NUMBER;
    v_reversas   NUMBER;
    v_quedan_a   NUMBER;
    v_quedan_r   NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_asientos
      FROM EJECUCION.T_ASIENTOS WHERE leyenda LIKE v_prefijo || '%';

    SELECT COUNT(*) INTO v_renglones
      FROM EJECUCION.T_ASIENTOS_DET d
     WHERE EXISTS (SELECT 1 FROM EJECUCION.T_ASIENTOS a
                    WHERE a.id_asiento = d.id_asiento AND a.leyenda LIKE v_prefijo || '%');

    DBMS_OUTPUT.PUT_LINE('A borrar: ' || v_asientos || ' asientos y ' || v_renglones || ' renglones.');

    IF v_asientos = 0 THEN
        DBMS_OUTPUT.PUT_LINE('No hay nada con el prefijo ' || v_prefijo || '.');
        RETURN;
    END IF;

    -- Los asientos que REVERSAN a uno de prueba tambien son de prueba, aunque su leyenda no lleve
    -- el prefijo: el modulo la arma como "REVERSA ASIENTO n". Existen solo porque existia el [QA]
    -- que reversan, y ademas T_ASIENTOS tiene una FK a si misma (FK_ASIENTO_REL) que impide borrar
    -- el original mientras la reversa lo apunte. Sin esto la limpieza fallaba entera con ORA-02292.
    SELECT COUNT(*) INTO v_reversas
      FROM EJECUCION.T_ASIENTOS r
     WHERE EXISTS (SELECT 1 FROM EJECUCION.T_ASIENTOS a
                    WHERE a.id_asiento = r.id_asiento_rel AND a.leyenda LIKE v_prefijo || '%');

    IF v_reversas > 0 THEN
        DBMS_OUTPUT.PUT_LINE('Ademas ' || v_reversas || ' asiento(s) que reversan a uno de prueba.');
    END IF;

    -- Renglones: primero los de las reversas, despues los de los [QA].
    DELETE FROM EJECUCION.T_ASIENTOS_DET d
     WHERE EXISTS (SELECT 1 FROM EJECUCION.T_ASIENTOS r
                    WHERE r.id_asiento = d.id_asiento
                      AND EXISTS (SELECT 1 FROM EJECUCION.T_ASIENTOS a
                                   WHERE a.id_asiento = r.id_asiento_rel
                                     AND a.leyenda LIKE v_prefijo || '%'));

    DELETE FROM EJECUCION.T_ASIENTOS_DET d
     WHERE EXISTS (SELECT 1 FROM EJECUCION.T_ASIENTOS a
                    WHERE a.id_asiento = d.id_asiento AND a.leyenda LIKE v_prefijo || '%');

    -- Cabeceras: las reversas antes que sus originales, o la FK lo impide.
    DELETE FROM EJECUCION.T_ASIENTOS r
     WHERE EXISTS (SELECT 1 FROM EJECUCION.T_ASIENTOS a
                    WHERE a.id_asiento = r.id_asiento_rel AND a.leyenda LIKE v_prefijo || '%');

    DELETE FROM EJECUCION.T_ASIENTOS WHERE leyenda LIKE v_prefijo || '%';

    COMMIT;

    -- Con SELECT ... INTO y no con subconsultas dentro del PUT_LINE: PL/SQL no admite un SELECT
    -- escalar dentro de una expresion, y asi estaba escrito. El script fallaba entero con
    -- PLS-00103 y nunca llegaba a borrar nada.
    SELECT COUNT(*) INTO v_quedan_a FROM EJECUCION.T_ASIENTOS;
    SELECT COUNT(*) INTO v_quedan_r FROM EJECUCION.T_ASIENTOS_DET;

    DBMS_OUTPUT.PUT_LINE('Listo. Quedan ' || v_quedan_a || ' asientos y ' ||
                         v_quedan_r || ' renglones en total.');

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('ERROR: ' || SQLERRM);
        RAISE;
END;
/

exit
