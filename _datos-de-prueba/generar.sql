-- =====================================================================================
-- Datos de prueba del modulo contable — generacion
--
-- Inserta asientos sinteticos en EJECUCION.T_ASIENTOS y EJECUCION.T_ASIENTOS_DET, y en
-- ninguna otra tabla. No toca el plan de cuentas, la matriz contable, las vigencias, las
-- jurisdicciones ni ningun otro esquema.
--
-- Todos los asientos llevan el prefijo [QA] en la leyenda: eso es lo unico que hace falta
-- para reconocerlos y para que limpiar.sql los borre.
--
-- El generador es determinista a proposito: no usa DBMS_RANDOM, sino aritmetica sobre el
-- indice del bucle. Dos corridas con los mismos parametros producen exactamente los mismos
-- asientos, que es lo que uno quiere de un juego de datos de prueba.
--
-- Las cuentas no estan escritas a mano: se toman del plan real por naturaleza. El script
-- sirve para cualquier municipio, no solo para Morteros.
--
--   sqlplus EJECUCION/... @generar.sql
--
-- Para ver que haria sin escribir nada, poner v_simular := TRUE.
-- =====================================================================================

set serveroutput on size unlimited
set feedback off
set linesize 140

DECLARE
    -- ---- Parametros -----------------------------------------------------------------

    -- TRUE = calcula e informa, y despues deshace todo. No deja nada en la base.
    v_simular      BOOLEAN := FALSE;

    v_prefijo      CONSTANT VARCHAR2(10) := '[QA]';

    -- Ejercicios a poblar y cuantos asientos en cada uno.
    v_anio_1       CONSTANT NUMBER := 2025;
    v_cantidad_1   CONSTANT NUMBER := 150;

    v_anio_2       CONSTANT NUMBER := 2026;
    v_cantidad_2   CONSTANT NUMBER := 250;

    -- ---- Tipos ----------------------------------------------------------------------

    TYPE t_ids IS TABLE OF NUMBER INDEX BY PLS_INTEGER;

    v_egresos      t_ids;
    v_ingresos     t_ids;
    v_bancos       t_ids;
    v_pasivo       t_ids;

    v_total_insertados NUMBER := 0;
    v_total_renglones  NUMBER := 0;

    -- ---- Carga del plan de cuentas --------------------------------------------------

    PROCEDURE cargar_cuentas(p_naturaleza IN NUMBER, p_codigo_like IN VARCHAR2, p_destino IN OUT t_ids) IS
        v_i PLS_INTEGER := 0;
    BEGIN
        p_destino.DELETE;

        FOR c IN (SELECT id_cta_contable
                    FROM EJECUCION.T_CUENTAS_CONTABLES
                   WHERE imputable = 'S'
                     AND activa = 'A'
                     AND (p_naturaleza IS NULL OR id_naturaleza = p_naturaleza)
                     AND (p_codigo_like IS NULL OR codigo LIKE p_codigo_like)
                   ORDER BY codigo)
        LOOP
            v_i := v_i + 1;
            p_destino(v_i) := c.id_cta_contable;
        END LOOP;
    END;

    -- Elige de forma ciclica y determinista dentro del conjunto.
    FUNCTION elegir(p_conjunto IN t_ids, p_indice IN NUMBER) RETURN NUMBER IS
    BEGIN
        IF p_conjunto.COUNT = 0 THEN
            RETURN NULL;
        END IF;

        RETURN p_conjunto(MOD(p_indice, p_conjunto.COUNT) + 1);
    END;

    -- ---- Generacion de un ejercicio -------------------------------------------------

    PROCEDURE generar_ejercicio(p_anio IN NUMBER, p_cantidad IN NUMBER) IS
        v_id_vigencia     NUMBER;
        v_id_jurisdiccion NUMBER;
        v_numero          NUMBER;
        v_id_asiento      NUMBER;
        v_fecha           DATE;
        v_mes             NUMBER;
        v_dia             NUMBER;
        v_importe         NUMBER;
        v_importe2        NUMBER;
        v_patron          NUMBER;
        v_tipo            NUMBER;
        v_mes_tope        NUMBER;
        v_leyenda         VARCHAR2(400);
        v_generados       NUMBER := 0;

        PROCEDURE renglon(p_id_asiento NUMBER, p_cuenta NUMBER, p_debe NUMBER, p_haber NUMBER, p_fecha DATE) IS
        BEGIN
            INSERT INTO EJECUCION.T_ASIENTOS_DET
                (id_asiento_det, id_asiento, id_cta_contable, fec_contable,
                 debe, haber, anulado, id_recurso_d_h, user_ing, fec_ing)
            VALUES
                (EJECUCION.SQ_T_ASIENTOS_DETALLES.NEXTVAL, p_id_asiento, p_cuenta, p_fecha,
                 p_debe, p_haber, 'N', CASE WHEN p_debe IS NULL THEN 3 ELSE 4 END, 'QA', SYSDATE);

            v_total_renglones := v_total_renglones + 1;
        END;
    BEGIN
        SELECT id_vigencia INTO v_id_vigencia
          FROM PRESUPUESTO.T_VIGENCIAS WHERE ejercicio = p_anio;

        -- La jurisdiccion sale de una unidad administrativa de esa vigencia, igual que lo
        -- hace el alta del modulo. No se inventa ni se escribe a mano.
        SELECT MIN(id_jurisdiccion) INTO v_id_jurisdiccion
          FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA WHERE id_vigencia = v_id_vigencia;

        IF v_id_jurisdiccion IS NULL THEN
            DBMS_OUTPUT.PUT_LINE('  ' || p_anio || ': sin unidades administrativas, se saltea.');
            RETURN;
        END IF;

        SELECT NVL(MAX(numero), 0) INTO v_numero
          FROM EJECUCION.T_ASIENTOS WHERE id_jurisdiccion = v_id_jurisdiccion;

        -- No se generan asientos con fecha futura.
        v_mes_tope := CASE WHEN p_anio = EXTRACT(YEAR FROM SYSDATE)
                           THEN EXTRACT(MONTH FROM SYSDATE)
                           ELSE 12 END;

        FOR i IN 1 .. p_cantidad LOOP
            v_mes  := MOD(i * 7, v_mes_tope) + 1;
            v_dia  := MOD(i * 13, 28) + 1;

            IF p_anio = EXTRACT(YEAR FROM SYSDATE)
               AND v_mes = EXTRACT(MONTH FROM SYSDATE)
               AND v_dia > EXTRACT(DAY FROM SYSDATE) THEN
                v_dia := EXTRACT(DAY FROM SYSDATE);
            END IF;

            v_fecha   := TO_DATE(LPAD(v_dia, 2, '0') || '/' || LPAD(v_mes, 2, '0') || '/' || p_anio, 'DD/MM/YYYY');
            v_importe := 1000 + MOD(i * 997, 900000) + MOD(i * 37, 100) / 100;
            v_patron  := MOD(i, 3);
            v_tipo    := CASE WHEN MOD(i, 5) = 0 THEN 2 ELSE 1 END;

            v_numero     := v_numero + 1;
            v_id_asiento := EJECUCION.SQ_T_ASIENTOS.NEXTVAL;

            v_leyenda := v_prefijo || ' ' ||
                CASE v_patron
                    WHEN 0 THEN 'DEVENGAMIENTO DE GASTO '
                    WHEN 1 THEN 'PAGO A PROVEEDOR '
                    ELSE 'RECAUDACION '
                END || TO_CHAR(p_anio) || '-' || LPAD(i, 5, '0');

            INSERT INTO EJECUCION.T_ASIENTOS
                (id_asiento, numero, anio, id_vigencia, id_jurisdiccion, observaciones,
                 fec_alta, user_alta, id_tipo_asiento, fec_contable, leyenda)
            VALUES
                (v_id_asiento, v_numero, p_anio, v_id_vigencia, v_id_jurisdiccion,
                 v_prefijo || ' dato de prueba', SYSDATE, 'QA', v_tipo, v_fecha, v_leyenda);

            IF v_patron = 0 THEN
                -- Devengamiento: el gasto se reconoce contra la deuda con el proveedor.
                -- Uno de cada siete reparte el gasto en dos cuentas, para que el juego de
                -- datos tenga asientos de mas de dos renglones.
                IF MOD(i, 7) = 0 THEN
                    v_importe2 := ROUND(v_importe / 3, 2);
                    renglon(v_id_asiento, elegir(v_egresos, i),     v_importe - v_importe2, NULL, v_fecha);
                    renglon(v_id_asiento, elegir(v_egresos, i + 1), v_importe2,             NULL, v_fecha);
                ELSE
                    renglon(v_id_asiento, elegir(v_egresos, i), v_importe, NULL, v_fecha);
                END IF;

                renglon(v_id_asiento, elegir(v_pasivo, i), NULL, v_importe, v_fecha);

            ELSIF v_patron = 1 THEN
                -- Pago: cancela deuda y sale plata del banco.
                renglon(v_id_asiento, elegir(v_pasivo, i), v_importe, NULL, v_fecha);
                renglon(v_id_asiento, elegir(v_bancos, i), NULL, v_importe, v_fecha);

            ELSE
                -- Recaudacion: entra plata al banco contra una cuenta de ingresos.
                renglon(v_id_asiento, elegir(v_bancos, i),  v_importe, NULL, v_fecha);
                renglon(v_id_asiento, elegir(v_ingresos, i), NULL, v_importe, v_fecha);
            END IF;

            v_generados := v_generados + 1;
        END LOOP;

        v_total_insertados := v_total_insertados + v_generados;

        DBMS_OUTPUT.PUT_LINE('  ' || p_anio || ': ' || v_generados || ' asientos' ||
                             ' | vigencia ' || v_id_vigencia ||
                             ' | jurisdiccion ' || v_id_jurisdiccion ||
                             ' | numeros ' || (v_numero - v_generados + 1) || ' a ' || v_numero);
    END;

    -- ---- Control previo -------------------------------------------------------------

    FUNCTION ya_hay_datos_de_prueba RETURN BOOLEAN IS
        v_cant NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_cant
          FROM EJECUCION.T_ASIENTOS WHERE leyenda LIKE v_prefijo || '%';

        RETURN v_cant > 0;
    END;

BEGIN
    DBMS_OUTPUT.PUT_LINE('===========================================================');
    DBMS_OUTPUT.PUT_LINE(CASE WHEN v_simular THEN 'SIMULACION (no escribe nada)' ELSE 'GENERACION DE DATOS DE PRUEBA' END);
    DBMS_OUTPUT.PUT_LINE('===========================================================');

    IF ya_hay_datos_de_prueba THEN
        DBMS_OUTPUT.PUT_LINE('Ya hay asientos con el prefijo ' || v_prefijo || '.');
        DBMS_OUTPUT.PUT_LINE('Corre limpiar.sql antes de volver a generar.');
        RETURN;
    END IF;

    cargar_cuentas(5,    NULL,          v_egresos);
    cargar_cuentas(4,    NULL,          v_ingresos);
    cargar_cuentas(NULL, '01.01.01%',   v_bancos);
    cargar_cuentas(NULL, '02.01%',      v_pasivo);

    DBMS_OUTPUT.PUT_LINE('Cuentas tomadas del plan real:');
    DBMS_OUTPUT.PUT_LINE('  egresos   ' || v_egresos.COUNT);
    DBMS_OUTPUT.PUT_LINE('  ingresos  ' || v_ingresos.COUNT);
    DBMS_OUTPUT.PUT_LINE('  bancos    ' || v_bancos.COUNT);
    DBMS_OUTPUT.PUT_LINE('  pasivo    ' || v_pasivo.COUNT);

    IF v_egresos.COUNT = 0 OR v_ingresos.COUNT = 0 OR v_bancos.COUNT = 0 OR v_pasivo.COUNT = 0 THEN
        DBMS_OUTPUT.PUT_LINE('Falta alguno de los grupos de cuentas. No se genera nada.');
        RETURN;
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    generar_ejercicio(v_anio_1, v_cantidad_1);
    generar_ejercicio(v_anio_2, v_cantidad_2);

    DBMS_OUTPUT.PUT_LINE('');
    DBMS_OUTPUT.PUT_LINE('Total: ' || v_total_insertados || ' asientos, ' || v_total_renglones || ' renglones.');

    IF v_simular THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('SIMULACION: se deshizo todo. La base quedo como estaba.');
    ELSE
        COMMIT;
        DBMS_OUTPUT.PUT_LINE('Confirmado.');
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('ERROR: ' || SQLERRM);
        DBMS_OUTPUT.PUT_LINE('Se deshizo todo. La base quedo como estaba.');
        RAISE;
END;
/

exit
