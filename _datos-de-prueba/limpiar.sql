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
    v_prefijo   CONSTANT VARCHAR2(10) := '[QA]';
    v_asientos  NUMBER;
    v_renglones NUMBER;
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

    DELETE FROM EJECUCION.T_ASIENTOS_DET d
     WHERE EXISTS (SELECT 1 FROM EJECUCION.T_ASIENTOS a
                    WHERE a.id_asiento = d.id_asiento AND a.leyenda LIKE v_prefijo || '%');

    DELETE FROM EJECUCION.T_ASIENTOS WHERE leyenda LIKE v_prefijo || '%';

    COMMIT;

    DBMS_OUTPUT.PUT_LINE('Listo. Quedan ' ||
        (SELECT COUNT(*) FROM EJECUCION.T_ASIENTOS) || ' asientos y ' ||
        (SELECT COUNT(*) FROM EJECUCION.T_ASIENTOS_DET) || ' renglones en total.');

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('ERROR: ' || SQLERRM);
        RAISE;
END;
/

exit
