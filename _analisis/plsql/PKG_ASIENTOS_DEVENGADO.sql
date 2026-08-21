PACKAGE BODY PKG_ASIENTOS_DEVENGADO IS

  FUNCTION OBTENER_HABER_ASIENTO(p_ID_DEVENGADO IN NUMBER DEFAULT NULL)
                 RETURN NUMBER IS
    V_ID_JURISDICCION NUMBER;
    V_ID_VIGENCIA     NUMBER;
    v_ID_TIPODOC      NUMBER;
    V_TIPO_FACTURA    VARCHAR2(1);
    V_NRO_DEVENGADO   VARCHAR2(50);
    V_FEC_CONTABLE    DATE;

    V_PARTIDA_DOBLE   VARCHAR(50);
    V_DEDUCCION       NUMBER;
    V_FECHA_CONFORMACION DATE;
    v_TIPO_SALDO       VARCHAR(1);
    v_COD_CTA_CONTABLE VARCHAR2(20);
    V_ID_CTA_CONTABLE NUMBER(10);

    --VARIABLES PARA LA CABECERA DEL ASIENTO
    v_ID_ASIENTO      NUMBER;
    v_NUMERO          NUMBER;
    v_ANIO            NUMBER;
    v_ID_TIPO_ASIENTO NUMBER;
    v_LEYENDA         VARCHAR(500);

    --VARIABLES PARA EL DETALLE DEL ASIENTO
    v_ID_ASIENTO_DET          NUMBER;
    v_DEBE                    NUMBER;
    v_HABER                   NUMBER;
    V_ID_CTA_CBLE_DESTINO_DEV NUMBER;

    CURSOR cur_T_ASIENTOS_ANULAR IS
      SELECT DISTINCT A.ID_ASIENTO
        FROM EJECUCION.T_ASIENTOS A
        JOIN EJECUCION.T_ASIENTOS_DET AD
          ON AD.ID_ASIENTO = A.ID_ASIENTO
       WHERE A.ID_DEVENGADO = P_ID_DEVENGADO
         AND AD.ANULADO = 'N';

    CURSOR cur_T_DETALLE IS
      SELECT UPPER(CC.NOMBRE) NOMBRE,
             SUM(NVL((ROUND(CANTIDAD * IMPORTE, 2)), 0)) AS IMPORTE,
             DECODE(v_TIPO_SALDO, 'H', 'D', 'H') AS DEBE_HABER,
             DECODE(DECODE(v_TIPO_SALDO, 'H', 'D', 'H'),
                    'D',
                    DD.ID_DEVENGADODET,
                    NULL) ID_DEVENGADODET,
             CC.ID_CTA_CONTABLE
        FROM T_DEVENGADO_DETALLE DD
        JOIN T_CUENTAS_CONTABLES CC
          ON CC.ID_CTA_CONTABLE = DD.ID_CUENTA_CONTABLE
       WHERE DD.ID_ESTADO <> 20
         AND DD.ID_DEVENGADO = p_ID_DEVENGADO
         AND (V_DEDUCCION <> 1 OR v_ID_TIPODOC <> 137)
       GROUP BY UPPER(CC.NOMBRE),
                DECODE(v_TIPO_SALDO, 'H', 'D', 'H'),
                DECODE(DECODE(v_TIPO_SALDO, 'H', 'D', 'H'),
                       'D',
                       DD.ID_DEVENGADODET,
                       NULL),
                CC.ID_CTA_CONTABLE

      UNION

      SELECT UPPER(CC.NOMBRE) NOMBRE,
             SUM(NVL((ROUND(CANTIDAD * IMPORTE, 2)), 0)) AS IMPORTE,
             DECODE(v_TIPO_SALDO, 'H', 'D', 'H') AS DEBE_HABER,
             DECODE(DECODE(v_TIPO_SALDO, 'H', 'D', 'H'),
                    'D',
                    DD.ID_DEVENGADODET,
                    NULL) ID_DEVENGADODET,
             CC.ID_CTA_CONTABLE
        FROM T_DEVENGADO_DETALLE DD
        JOIN T_CUENTAS_CONTABLES CC
          ON CC.ID_CTA_CONTABLE = DD.ID_CUENTA_CONTABLED
       WHERE DD.ID_ESTADO <> 20
         AND DD.ID_DEVENGADO = p_ID_DEVENGADO
       AND (V_DEDUCCION = 1 AND v_ID_TIPODOC = 137)
       GROUP BY UPPER(CC.NOMBRE),
                DECODE(v_TIPO_SALDO, 'H', 'D', 'H'),
                DECODE(DECODE(v_TIPO_SALDO, 'H', 'D', 'H'),
                       'D',
                       DD.ID_DEVENGADODET,
                       NULL),
                CC.ID_CTA_CONTABLE

      UNION

      SELECT UPPER(CC1.NOMBRE),
             (SELECT NVL(SUM(ROUND(CANTIDAD * IMPORTE, 2)), 0) AS IMPORTE
                FROM T_DEVENGADO_DETALLE DD
               WHERE DD.ID_ESTADO <> 20
                 AND DD.ID_DEVENGADO = p_ID_DEVENGADO),
             DECODE(v_TIPO_SALDO, 'H', 'H', 'D'),
             NULL,
             CC1.ID_CTA_CONTABLE
        FROM T_CUENTAS_CONTABLES CC1
       WHERE CC1.CODIGO = v_COD_CTA_CONTABLE
         AND CC1.Id_Jurisdiccion IN
             (SELECT J1.ID_JURISDICCION
                FROM PRESUPUESTO.T_JURISDICCIONES J1
                JOIN PRESUPUESTO.T_JURISDICCIONES J2
                  ON J2.NUMERO_JURISDICCION = J1.NUMERO_JURISDICCION
               WHERE J2.ID_JURISDICCION = V_ID_JURISDICCION)
         AND (V_DEDUCCION <> 1 OR v_ID_TIPODOC <> 137)
       GROUP BY UPPER(CC1.NOMBRE),
                DECODE(v_TIPO_SALDO, 'H', 'H', 'D'),
                CC1.ID_CTA_CONTABLE

      UNION

      SELECT UPPER(CC.NOMBRE) NOMBRE,
             SUM(NVL((ROUND(CANTIDAD * IMPORTE, 2)), 0)) AS IMPORTE,
             DECODE(v_TIPO_SALDO, 'H', 'H', 'D') AS DEBE_HABER,
             DECODE(DECODE(v_TIPO_SALDO, 'H', 'H', 'D'),
                    'D',
                    DD.ID_DEVENGADODET,
                    NULL) ID_DEVENGADODET,
             CC.ID_CTA_CONTABLE
        FROM T_DEVENGADO_DETALLE DD
        JOIN T_CUENTAS_CONTABLES CC
          ON CC.ID_CTA_CONTABLE = DD.ID_CUENTA_CONTABLE
       WHERE DD.ID_ESTADO <> 20
         AND DD.ID_DEVENGADO = p_ID_DEVENGADO
       AND (V_DEDUCCION = 1 AND v_ID_TIPODOC = 137)
       GROUP BY UPPER(CC.NOMBRE),
                DECODE(v_TIPO_SALDO, 'H', 'H', 'D'),
                DECODE(DECODE(v_TIPO_SALDO, 'H', 'H', 'D'),
                       'D',
                       DD.ID_DEVENGADODET,
                       NULL),
                CC.ID_CTA_CONTABLE

       ORDER BY 4;

  BEGIN

    SELECT D.ID_JURISDICCION,
           D.ID_TIPODOC,
           D.ID_VIGENCIA,
           D.TIPO_FACTURA,
           D.NRO_DEVENGADO,
           D.ID_CTA_CBLE_DESTINO,
           DECODE(D.OBSERVACION, 'DEDUCCIONES', 1, 0),
           d.fecha_conformacion
      INTO V_ID_JURISDICCION,
           v_ID_TIPODOC,
           V_ID_VIGENCIA,
           V_TIPO_FACTURA,
           V_NRO_DEVENGADO,
           V_ID_CTA_CBLE_DESTINO_DEV,
           V_DEDUCCION,
           V_FECHA_CONFORMACION
      FROM EJECUCION.T_DEVENGADO D
     WHERE D.ID_DEVENGADO = P_ID_DEVENGADO;

    SELECT V.VALOR
      INTO V_PARTIDA_DOBLE
      FROM T_VARIABLES_CONFIGURACION V
     WHERE V.ID_USUARIO = V_ID_JURISDICCION
       AND V.NOMBRE_VARIABLE = 'PARTIDA_DOBLE';

    IF V_PARTIDA_DOBLE = 'S' THEN

      IF to_char(sysdate(), 'YYYY') = substr(V_NRO_DEVENGADO, 0, 4) then
        V_FEC_CONTABLE := TRUNC(V_FECHA_CONFORMACION);
      ELSE
        SELECT last_day(TO_date('1/' || '12/' ||
                                substr(V_NRO_DEVENGADO, 0, 4),
                                'DD/MM/YYYY')) Ultimo_Dia_Anio
          INTO V_FEC_CONTABLE
          FROM DUAL;
      End IF;



      SELECT SQ_T_ASIENTOS.NEXTVAL --ID_ASIENTO
        INTO v_ID_ASIENTO
        FROM DUAL;

      SELECT EJECUCION.F_NUMERO_ASIENTO(v_ID_JURISDICCION) --NUMERO ASIENTO
        INTO v_NUMERO
        FROM DUAL;

      SELECT EJERCICIO --ANIO
        INTO v_ANIO
        FROM PRESUPUESTO.T_VIGENCIAS V
       WHERE V.ID_VIGENCIA = v_ID_VIGENCIA;

      SELECT CC.TIPO_SALDO, CC.COD_CTA_CONTABLE, CC.ID_TIPO_ASIENTO
        INTO v_TIPO_SALDO, v_COD_CTA_CONTABLE, v_ID_TIPO_ASIENTO
        FROM EJECUCION.T_CODIGOS_CONTABILIZACION CC
       WHERE CC.TABLA_ORIGEN = 'T_DEVENGADO_DETALLE'
         AND CC.NUMERO_JURISDICCION =
             (SELECT j.numero_jurisdiccion
                FROM PRESUPUESTO.T_JURISDICCIONES J
               WHERE J.ID_JURISDICCION = V_ID_JURISDICCION)
         AND CC.ID_TIPODOC = V_ID_TIPODOC
         AND (CC.TIPO_FACTURA = V_TIPO_FACTURA OR V_TIPO_FACTURA IS NULL)

       GROUP BY CC.TIPO_SALDO, CC.COD_CTA_CONTABLE, CC.ID_TIPO_ASIENTO;

      --INSERTAR CABECERA

      v_LEYENDA := 'GENERACION DE ASIENTO DESDE DEVENGADO AUTOMATICO';

      IF v_ID_TIPODOC = 179 THEN

        SELECT CC1.CODIGO
          INTO V_COD_CTA_CONTABLE
          FROM EJECUCION.T_CUENTAS_CONTABLES CC1
         WHERE CC1.ID_CTA_CONTABLE = V_ID_CTA_CBLE_DESTINO_DEV;

        IF v_TIPO_SALDO = 'H' THEN
          v_TIPO_SALDO := 'D';
        ELSE
          v_TIPO_SALDO := 'H';
        END IF;

      ELSE
        IF v_ID_TIPODOC = 177 THEN
          IF v_TIPO_SALDO = 'H' THEN
            v_TIPO_SALDO := 'D';
          ELSE
            v_TIPO_SALDO := 'H';
          END IF;
        end if;

      END IF;

      FOR ENT_DETALLE IN cur_T_DETALLE LOOP

        v_DEBE  := NULL;
        v_HABER := NULL;

        IF ENT_DETALLE.DEBE_HABER = 'H' THEN
          v_HABER := ENT_DETALLE.IMPORTE;
          IF v_HABER < 0 THEN
            v_HABER := v_HABER * -1;
          END IF;
        ELSE
          v_DEBE := ENT_DETALLE.IMPORTE;
          IF v_DEBE < 0 THEN
            v_DEBE := v_DEBE * -1;
          END IF;
        END IF;

        IF v_HABER > 0 THEN
          v_id_cta_contable := ENT_DETALLE.ID_CTA_CONTABLE;
        END IF;


      END LOOP;

      return v_id_cta_contable;

    END IF;

  EXCEPTION
    WHEN OTHERS THEN

      PKG_UTIL.BASE_ROLLBACK;

  END OBTENER_HABER_ASIENTO;

  PROCEDURE INSERTAR_ASIENTOS(p_ID_DEVENGADO IN NUMBER DEFAULT NULL,
                              P_ID_ASIENTO   OUT NUMBER,
                              P_MSG          IN OUT VARCHAR2) IS
    V_ID_JURISDICCION NUMBER;
    V_ID_VIGENCIA     NUMBER;
    v_ID_TIPODOC      NUMBER;
    V_TIPO_FACTURA    VARCHAR2(1);
    V_NRO_DEVENGADO   VARCHAR2(50);
    V_FEC_CONTABLE    DATE;
    V_PARTIDA_DOBLE   VARCHAR(50);
    V_DEDUCCION       NUMBER;
    V_FECHA_CONFORMACION DATE;
    v_TIPO_SALDO       VARCHAR(1);
    v_COD_CTA_CONTABLE VARCHAR2(20);

    --VARIABLES PARA LA CABECERA DEL ASIENTO
    v_ID_ASIENTO      NUMBER;
    v_NUMERO          NUMBER;
    v_ANIO            NUMBER;
    v_ID_TIPO_ASIENTO NUMBER;
    v_LEYENDA         VARCHAR(500);

    --VARIABLES PARA EL DETALLE DEL ASIENTO
    v_ID_ASIENTO_DET          NUMBER;
    v_DEBE                    NUMBER;
    v_HABER                   NUMBER;
    V_ID_CTA_CBLE_DESTINO_DEV NUMBER;

    CURSOR cur_T_ASIENTOS_ANULAR IS
      SELECT DISTINCT A.ID_ASIENTO
        FROM EJECUCION.T_ASIENTOS A
        JOIN EJECUCION.T_ASIENTOS_DET AD
          ON AD.ID_ASIENTO = A.ID_ASIENTO
       WHERE A.ID_DEVENGADO = P_ID_DEVENGADO
         AND AD.ANULADO = 'N';

    CURSOR cur_T_DETALLE IS
      SELECT UPPER(CC.NOMBRE) NOMBRE,
             SUM(NVL((ROUND(CANTIDAD * IMPORTE, 2)), 0)) AS IMPORTE,
             DECODE(v_TIPO_SALDO, 'H', 'D', 'H') AS DEBE_HABER,
             DECODE(DECODE(v_TIPO_SALDO, 'H', 'D', 'H'),
                    'D',
                    DD.ID_DEVENGADODET,
                    NULL) ID_DEVENGADODET,
             CC.ID_CTA_CONTABLE
        FROM T_DEVENGADO_DETALLE DD
        JOIN T_CUENTAS_CONTABLES CC
          ON CC.ID_CTA_CONTABLE = DD.ID_CUENTA_CONTABLE
       WHERE DD.ID_ESTADO <> 20
         AND DD.ID_DEVENGADO = p_ID_DEVENGADO
         AND (V_DEDUCCION <> 1 OR v_ID_TIPODOC <> 137)
       GROUP BY UPPER(CC.NOMBRE),
                DECODE(v_TIPO_SALDO, 'H', 'D', 'H'),
                DECODE(DECODE(v_TIPO_SALDO, 'H', 'D', 'H'),
                       'D',
                       DD.ID_DEVENGADODET,
                       NULL),
                CC.ID_CTA_CONTABLE

      UNION

      SELECT UPPER(CC.NOMBRE) NOMBRE,
             SUM(NVL((ROUND(CANTIDAD * IMPORTE, 2)), 0)) AS IMPORTE,
             DECODE(v_TIPO_SALDO, 'H', 'H', 'D') AS DEBE_HABER,
             DECODE(DECODE(v_TIPO_SALDO, 'H', 'D', 'H'),
                    'D',
                    DD.ID_DEVENGADODET,
                    NULL) ID_DEVENGADODET,
             CC.ID_CTA_CONTABLE
        FROM T_DEVENGADO_DETALLE DD
        JOIN T_CUENTAS_CONTABLES CC
          ON CC.ID_CTA_CONTABLE = DD.ID_CUENTA_CONTABLED
       WHERE DD.ID_ESTADO <> 20
         AND DD.ID_DEVENGADO = p_ID_DEVENGADO
       AND (V_DEDUCCION = 1 AND v_ID_TIPODOC = 137)
       GROUP BY UPPER(CC.NOMBRE),
                DECODE(v_TIPO_SALDO, 'H', 'H', 'D'),
                DECODE(DECODE(v_TIPO_SALDO, 'H', 'D', 'H'),
                       'D',
                       DD.ID_DEVENGADODET,
                       NULL),
                CC.ID_CTA_CONTABLE

      UNION

      SELECT UPPER(CC1.NOMBRE),
             (SELECT NVL(SUM(ROUND(CANTIDAD * IMPORTE, 2)), 0) AS IMPORTE
                FROM T_DEVENGADO_DETALLE DD
               WHERE DD.ID_ESTADO <> 20
                 AND DD.ID_DEVENGADO = p_ID_DEVENGADO),
             DECODE(v_TIPO_SALDO, 'H', 'H', 'D'),
             NULL,
             CC1.ID_CTA_CONTABLE
        FROM T_CUENTAS_CONTABLES CC1
       WHERE CC1.CODIGO = v_COD_CTA_CONTABLE
         AND CC1.Id_Jurisdiccion IN
             (SELECT J1.ID_JURISDICCION
                FROM PRESUPUESTO.T_JURISDICCIONES J1
                JOIN PRESUPUESTO.T_JURISDICCIONES J2
                  ON J2.NUMERO_JURISDICCION = J1.NUMERO_JURISDICCION
               WHERE J2.ID_JURISDICCION = V_ID_JURISDICCION)
         AND (V_DEDUCCION <> 1 OR v_ID_TIPODOC <> 137)
       GROUP BY UPPER(CC1.NOMBRE),
                DECODE(v_TIPO_SALDO, 'H', 'H', 'D'),
                CC1.ID_CTA_CONTABLE

      UNION

      SELECT UPPER(CC.NOMBRE) NOMBRE,
             SUM(NVL((ROUND(CANTIDAD * IMPORTE, 2)), 0)) AS IMPORTE,
             DECODE(v_TIPO_SALDO, 'H', 'D', 'H') AS DEBE_HABER,
             DECODE(DECODE(v_TIPO_SALDO, 'H', 'H', 'D'),
                    'D',
                    DD.ID_DEVENGADODET,
                    NULL) ID_DEVENGADODET,
             CC.ID_CTA_CONTABLE
        FROM T_DEVENGADO_DETALLE DD
        JOIN T_CUENTAS_CONTABLES CC
          ON CC.ID_CTA_CONTABLE = DD.ID_CUENTA_CONTABLE
       WHERE DD.ID_ESTADO <> 20
         AND DD.ID_DEVENGADO = p_ID_DEVENGADO
       AND (V_DEDUCCION = 1 AND v_ID_TIPODOC = 137)
       GROUP BY UPPER(CC.NOMBRE),
                DECODE(v_TIPO_SALDO, 'H', 'D', 'H'),
                DECODE(DECODE(v_TIPO_SALDO, 'H', 'H', 'D'),
                       'D',
                       DD.ID_DEVENGADODET,
                       NULL),
                CC.ID_CTA_CONTABLE

       ORDER BY 4;

  BEGIN

    SELECT D.ID_JURISDICCION,
           D.ID_TIPODOC,
           D.ID_VIGENCIA,
           D.TIPO_FACTURA,
           D.NRO_DEVENGADO,
           D.ID_CTA_CBLE_DESTINO,
           DECODE(D.OBSERVACION, 'DEDUCCIONES', 1, 0),
           d.fecha_conformacion
      INTO V_ID_JURISDICCION,
           v_ID_TIPODOC,
           V_ID_VIGENCIA,
           V_TIPO_FACTURA,
           V_NRO_DEVENGADO,
           V_ID_CTA_CBLE_DESTINO_DEV,
           V_DEDUCCION,
           V_FECHA_CONFORMACION
      FROM EJECUCION.T_DEVENGADO D
     WHERE D.ID_DEVENGADO = P_ID_DEVENGADO;

    SELECT V.VALOR
      INTO V_PARTIDA_DOBLE
      FROM T_VARIABLES_CONFIGURACION V
     WHERE V.ID_USUARIO = V_ID_JURISDICCION
       AND V.NOMBRE_VARIABLE = 'PARTIDA_DOBLE';

    IF V_PARTIDA_DOBLE = 'S' THEN

      IF to_char(sysdate(), 'YYYY') = substr(V_NRO_DEVENGADO, 0, 4) then
        V_FEC_CONTABLE := TRUNC(V_FECHA_CONFORMACION);
      ELSE
        SELECT last_day(TO_date('1/' || '12/' ||
                                substr(V_NRO_DEVENGADO, 0, 4),
                                'DD/MM/YYYY')) Ultimo_Dia_Anio
          INTO V_FEC_CONTABLE
          FROM DUAL;
      End IF;

      FOR ENT_ASIENTOS_ANULAR IN cur_T_ASIENTOS_ANULAR LOOP

        EJECUCION.PKG_ASIENTOS_DEVENGADO.INSERTAR_CONTAASIENTO(ENT_ASIENTOS_ANULAR.ID_ASIENTO);

      END LOOP;

      SELECT SQ_T_ASIENTOS.NEXTVAL --ID_ASIENTO
        INTO v_ID_ASIENTO
        FROM DUAL;

      SELECT EJECUCION.F_NUMERO_ASIENTO(v_ID_JURISDICCION) --NUMERO ASIENTO
        INTO v_NUMERO
        FROM DUAL;

      SELECT EJERCICIO --ANIO
        INTO v_ANIO
        FROM PRESUPUESTO.T_VIGENCIAS V
       WHERE V.ID_VIGENCIA = v_ID_VIGENCIA;

      SELECT CC.TIPO_SALDO, CC.COD_CTA_CONTABLE, CC.ID_TIPO_ASIENTO
        INTO v_TIPO_SALDO, v_COD_CTA_CONTABLE, v_ID_TIPO_ASIENTO
        FROM EJECUCION.T_CODIGOS_CONTABILIZACION CC
       WHERE CC.TABLA_ORIGEN = 'T_DEVENGADO_DETALLE'
         AND CC.NUMERO_JURISDICCION =
             (SELECT j.numero_jurisdiccion
                FROM PRESUPUESTO.T_JURISDICCIONES J
               WHERE J.ID_JURISDICCION = V_ID_JURISDICCION)
         AND CC.ID_TIPODOC = V_ID_TIPODOC
         AND (CC.TIPO_FACTURA = V_TIPO_FACTURA OR V_TIPO_FACTURA IS NULL)

       GROUP BY CC.TIPO_SALDO, CC.COD_CTA_CONTABLE, CC.ID_TIPO_ASIENTO;

      --INSERTAR CABECERA

      v_LEYENDA := 'GENERACION DE ASIENTO DESDE DEVENGADO AUTOMATICO';

      IF v_ID_TIPODOC = 179 THEN

        SELECT CC1.CODIGO
          INTO V_COD_CTA_CONTABLE
          FROM EJECUCION.T_CUENTAS_CONTABLES CC1
         WHERE CC1.ID_CTA_CONTABLE = V_ID_CTA_CBLE_DESTINO_DEV;

        IF v_TIPO_SALDO = 'H' THEN
          v_TIPO_SALDO := 'D';
        ELSE
          v_TIPO_SALDO := 'H';
        END IF;

      ELSE
        IF v_ID_TIPODOC = 177 THEN
          IF v_TIPO_SALDO = 'H' THEN
            v_TIPO_SALDO := 'D';
          ELSE
            v_TIPO_SALDO := 'H';
          END IF;
        end if;

        UPDATE T_DEVENGADO D
           SET D.ID_CTA_CBLE_DESTINO =
               (SELECT CC1.ID_CTA_CONTABLE
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC1
                 WHERE CC1.CODIGO = v_COD_CTA_CONTABLE
                   AND CC1.ID_JURISDICCION in
                       (SELECT J1.ID_JURISDICCION
                          FROM PRESUPUESTO.T_JURISDICCIONES J1
                          JOIN PRESUPUESTO.T_JURISDICCIONES J2
                            ON J2.NUMERO_JURISDICCION =
                               J1.NUMERO_JURISDICCION
                         WHERE J2.ID_JURISDICCION = V_ID_JURISDICCION))
         WHERE D.ID_DEVENGADO = P_ID_DEVENGADO;
      END IF;

      INSERT INTO EJECUCION.T_ASIENTOS
        (ID_ASIENTO,
         NUMERO,
         ANIO,
         ID_VIGENCIA,
         ID_JURISDICCION,
         FEC_ALTA,
         OBSERVACIONES,
         ID_DEVENGADO,
         ID_TIPO_ASIENTO,
         FEC_CONTABLE,
         LEYENDA)
      VALUES
        (v_ID_ASIENTO,
         v_NUMERO,
         v_ANIO,
         V_ID_VIGENCIA,
         V_ID_JURISDICCION,
         trunc(sysdate()),
         'ASIENTO CORRESPONDIENTE AL DEVENGADO: ' || v_NRO_DEVENGADO,
         p_ID_DEVENGADO,
         v_ID_TIPO_ASIENTO,
         V_FEC_CONTABLE,
         v_LEYENDA);
      --INSERTAR DETALLE

      FOR ENT_DETALLE IN cur_T_DETALLE LOOP
        SELECT EJECUCION.SQ_T_ASIENTOS_DETALLES.NEXTVAL --ID_ASIENTODET
          INTO v_ID_ASIENTO_DET
          FROM DUAL;

        v_DEBE  := NULL;
        v_HABER := NULL;

        IF ENT_DETALLE.DEBE_HABER = 'H' THEN
          v_HABER := ENT_DETALLE.IMPORTE;
          IF v_HABER < 0 THEN
            v_HABER := v_HABER * -1;
          END IF;
        ELSE
          v_DEBE := ENT_DETALLE.IMPORTE;
          IF v_DEBE < 0 THEN
            v_DEBE := v_DEBE * -1;
          END IF;
        END IF;

        INSERT INTO EJECUCION.T_ASIENTOS_DET
          (ID_ASIENTO_DET,
           ID_ASIENTO,
           ID_CTA_CONTABLE,
           FEC_CONTABLE,
           DEBE,
           HABER,
           ID_DEVENGADODET,
           FEC_ING,
           ANULADO)
        VALUES
          (v_ID_ASIENTO_DET,
           v_ID_ASIENTO,
           ENT_DETALLE.ID_CTA_CONTABLE,
           V_FEC_CONTABLE,
           v_DEBE,
           v_HABER,
           ENT_DETALLE.ID_DEVENGADODET,
           TRUNC(SYSDATE()),
           'N');

      END LOOP;

      P_ID_ASIENTO := v_ID_ASIENTO;

    END IF;

  EXCEPTION
    WHEN OTHERS THEN
      P_MSG := 'Error al INSERTAR. Verifique: ' || SQLERRM;
      PKG_UTIL.BASE_ROLLBACK;

  END INSERTAR_ASIENTOS;

  PROCEDURE INSERTAR_CONTAASIENTO(P_ID_ASIENTO IN NUMBER) IS
    V_ID_ASIENTO      NUMBER;
    V_NUMERO          NUMBER;
    V_FECHA           DATE;
    V_ANIO            NUMBER;
    V_ANIO_SYSDATE    NUMBER;
    V_FECHA_CHAR      VARCHAR2(10);
    V_ID_JURISDICCION NUMBER;
  BEGIN

    SELECT EJECUCION.SQ_T_ASIENTOS.NEXTVAL INTO V_ID_ASIENTO FROM DUAL;

    SELECT substr(D.NRO_DEVENGADO, 0, 4), D.ID_JURISDICCION
      INTO V_ANIO, V_ID_JURISDICCION
      FROM T_DEVENGADO D
      JOIN T_ASIENTOS A
        ON A.ID_DEVENGADO = D.ID_DEVENGADO
     WHERE A.ID_ASIENTO = P_ID_ASIENTO;

    V_FECHA        := TRUNC(SYSDATE());
    V_ANIO_SYSDATE := TO_CHAR(V_FECHA, 'YYYY');

    IF V_ANIO_SYSDATE <> V_ANIO THEN
      V_FECHA_CHAR := '31/12/' || TO_CHAR(V_ANIO);
      V_FECHA      := TO_DATE(V_FECHA_CHAR, 'DD/MM/YYYY');
    END IF;

    V_NUMERO := EJECUCION.F_NUMERO_ASIENTO(V_ID_JURISDICCION);

    INSERT INTO EJECUCION.T_ASIENTOS
      (ID_ASIENTO,
       ID_VIGENCIA,
       NUMERO,
       FEC_CONTABLE,
       ID_TIPO_ASIENTO,
       OBSERVACIONES,
       ANIO,
       ID_JURISDICCION,
       ID_ANTICIPO,
       FEC_ALTA,
       USER_ALTA,
       ID_ASIENTO_REL,
       LEYENDA,
       ID_DEVENGADO)

      SELECT V_ID_ASIENTO,
             A.ID_VIGENCIA,
             V_NUMERO,
             V_FECHA,
             A.ID_TIPO_ASIENTO,
             'CONTRAASIENTO - ' || A.OBSERVACIONES,
             A.ANIO,
             A.ID_JURISDICCION,
             A.ID_ANTICIPO,
             A.FEC_ALTA,
             A.USER_ALTA,
             P_ID_ASIENTO,
             A.LEYENDA,
             A.ID_DEVENGADO
        FROM T_ASIENTOS A
       WHERE A.ID_ASIENTO = P_ID_ASIENTO;

    INSERT INTO EJECUCION.T_ASIENTOS_DET
      (ID_ASIENTO_DET,
       ID_ASIENTO,
       ID_CTA_CONTABLE,
       DEBE,
       HABER,
       FEC_ING,
       USER_ING,
       FEC_CONTABLE,
       ANULADO,
       ID_DEVENGADODET)
      SELECT EJECUCION.SQ_T_ASIENTOS_DETALLES.NEXTVAL,
             V_ID_ASIENTO,
             AD.ID_CTA_CONTABLE,
             AD.HABER,
             AD.DEBE,
             AD.FEC_ING,
             AD.USER_ING,
             V_FECHA,
             'S',
             AD.ID_DEVENGADODET
        FROM T_ASIENTOS_DET AD
       WHERE AD.ID_ASIENTO = P_ID_ASIENTO;

    UPDATE T_ASIENTOS_DET AD
       SET AD.ANULADO = 'S'
     WHERE AD.ID_ASIENTO = P_ID_ASIENTO;

  END INSERTAR_CONTAASIENTO;

  PROCEDURE COPIAR_ASIENTOS_DET(P_ID_DEVENGADODET IN NUMBER,
                                P_ID_ASIENTO      IN NUMBER,
                                P_ID_ASIENTO_OLD  IN NUMBER,
                                P_MSGERR          OUT VARCHAR2) IS
    /*    \*Copia los asientos de los items que no se modificaron.
        Se llama desde un trigger cuando elimino o edito otro item*\
        V_ERRLOG          BOOLEAN := FALSE;
        V_ID_DEVENGADODET EJECUCION.T_DEVENGADO_DETALLE.ID_DEVENGADODET%TYPE := P_ID_DEVENGADODET;
        V_ID_ASIENTO      EJECUCION.T_ASIENTOS_DET.ID_ASIENTO%TYPE := P_ID_ASIENTO;
        V_ID_ASIENTO_OLD  EJECUCION.T_ASIENTOS_DET.ID_ASIENTO%TYPE := P_ID_ASIENTO_OLD;

        CURSOR C_ASIENTOS_DET IS
          SELECT *
            FROM EJECUCION.T_ASIENTOS_DET AD
           WHERE AD.ID_ASIENTO = P_ID_ASIENTO_OLD
             AND AD.ID_DEVENGADODET = V_ID_DEVENGADODET
             AND AD.ANULADO = 'N';
    */
  BEGIN
    /*    FOR R_ASIENTOS_DET IN C_ASIENTOS_DET LOOP
      BEGIN
        INSERT INTO EJECUCION.T_ASIENTOS_DET
          (ID_ASIENTO_DET,
           ID_ASIENTO,
           ID_CTA_CONTABLE,
           FEC_CONTABLE,
           DEBE,
           HABER,
           ID_DEVENGADODET,
           USER_ING,
           FEC_ING,
           ANULADO,
           ID_RECURSO_D_H)
        VALUES
          (EJECUCION.SEQ_ASIENTOS_DET.NEXTVAL,
           P_ID_ASIENTO,
           R_ASIENTOS_DET.ID_CTA_CONTABLE,
           R_ASIENTOS_DET.FEC_CONTABLE,
           R_ASIENTOS_DET.DEBE,
           R_ASIENTOS_DET.HABER,
           V_ID_DEVENGADODET,
           USER,
           SYSDATE,
           'N',
           R_ASIENTOS_DET.ID_RECURSO_D_H);
      EXCEPTION
        WHEN OTHERS THEN
          BEGIN
            P_MSGERR := SUBSTR(SQLERRM, 1, 200);
            V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO_DETALLE',
                                              TO_CHAR(V_ID_DEVENGADODET),
                                              'FALLA INSERT T_ASIENTOS_DET-COPIAR_ASIENTOS_DET' ||
                                              P_ID_ASIENTO || '-' ||
                                              P_ID_ASIENTO_OLD || '-' ||
                                              UPPER(RTRIM(P_MSGERR)));
          EXCEPTION
            WHEN OTHERS THEN
              NULL;
          END;
      END;
    END LOOP;*/
    P_MSGERR := '';
  END COPIAR_ASIENTOS_DET;

  FUNCTION FNC_INSERTAR_T_LOG_PD(P_TABLA         IN EJECUCION.T_LOG_PD.TABLA%TYPE,
                                 P_IDENTIFICADOR IN EJECUCION.T_LOG_PD.IDENTIFICADOR%TYPE,
                                 P_COMENTARIO    IN EJECUCION.T_LOG_PD.COMENTARIO%TYPE)
    RETURN BOOLEAN IS
    V_TABLA         EJECUCION.T_LOG_PD.TABLA%TYPE := P_TABLA;
    V_IDENTIFICADOR EJECUCION.T_LOG_PD.IDENTIFICADOR%TYPE := P_IDENTIFICADOR;
    V_COMENTARIO    EJECUCION.T_LOG_PD.COMENTARIO%TYPE := P_COMENTARIO;
  BEGIN
    INSERT INTO EJECUCION.T_LOG_PD LPD
      (LPD.ID_LOG_PD,
       LPD.TABLA,
       LPD.IDENTIFICADOR,
       LPD.COMENTARIO,
       LPD.FECHA,
       LPD.USUARIO)
    VALUES
      (EJECUCION.SQ_T_LOG_PD.NEXTVAL,
       V_TABLA,
       TO_CHAR(V_IDENTIFICADOR),
       V_COMENTARIO,
       SYSDATE,
       USER);
    RETURN FALSE;
  EXCEPTION
    WHEN OTHERS THEN
      RETURN TRUE;

  END FNC_INSERTAR_T_LOG_PD;

  --// PARA DESHABILITAR / HABILITAR LA GENERACION DE ASIENTOS DE DEVENGADO DE FORMA MAS RAPIDA  SE CREAN ESTOS PROCEDIMIENTOS
  --// CAMBIAMOS ENTRE UNO Y OTRO  POR ESO ESTAN DOS VECES . " INSERTAR_ASIENTOS_CAB "
  /*  PROCEDURE INSERTAR_ASIENTOS_CAB(P_ID_DEVENGADO    IN EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE,
                                  P_ID_OBJETO_GASTO IN PRESUPUESTO.T_OBJETOS_GASTO.ID_OBJETO_GASTO%TYPE DEFAULT NULL,
                                  P_ID_ASIENTO      OUT NUMBER,
                                  P_MSGERR          OUT VARCHAR2) IS

  BEGIN
    P_MSGERR := NULL ;
    P_ID_ASIENTO := 0;
  END INSERTAR_ASIENTOS_CAB;*/
  --
  PROCEDURE INSERTAR_ASIENTOS_CAB(P_ID_DEVENGADO IN EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE,
                                  P_ID_ASIENTO   OUT NUMBER,
                                  P_MSGERR       OUT VARCHAR2) IS
    /*
        V_ID_CTA_CONTABLE    NUMBER := 0;
        V_TIPO_SALDO         VARCHAR2(1) := NULL;
        V_ID_ASIENTO         NUMBER := 0;
        V_GENERA_ASIENTO     NUMBER(10) := 0;
        V_ID_TIPO_ASIENTO    NUMBER;
        V_LEYENDA            VARCHAR2(500);
        V_NUMERO             NUMBER(10);
        V_EJERCICIO          PRESUPUESTO.T_VIGENCIAS.EJERCICIO%TYPE;
        V_ID_VIGENCIA        EJECUCION.T_DEVENGADO.ID_VIGENCIA%TYPE;
        V_ID_TIPODOC         EJECUCION.T_DEVENGADO.ID_TIPODOC%TYPE;
        V_PARTIDA            PRESUPUESTO.T_OBJETOS_GASTO.ID_OBJETO_GASTO%TYPE;
        V_ID_UNIDAD_ADM      EJECUCION.T_DEVENGADO.ID_UNIDAD_ADM%TYPE;
        V_FECHA_CONFORMACION EJECUCION.T_DEVENGADO.FECHA_CONFORMACION%TYPE;
        V_ID_RECURSO_D_H     NUMBER(10);
        V_ERRLOG             BOOLEAN := FALSE;
        V_EXISTE             NUMBER := 0;
        V_FEC_CONTABLE       date;
        V_FECHA_CORTE        DATE := NULL;
        V_A?O                PRESUPUESTO.T_VIGENCIAS.EJERCICIO%TYPE;
        V_CANT               NUMBER;
        V_ID_DEVENGADO       EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE := P_ID_DEVENGADO;
        v_es_administracion_gral          number(10);
    */
  BEGIN
    /* PARTIDA DOBLE
    No debe generarse el asiento si se carga un devengado:
      1) Asociado a otro devengado, salvo para: NOTA DE CREDITO, NOTA DE CREDITO PAICOR,
                                                NOTA DE DEBITO Y CERTIFICACION DE SALDO.
      2) Contra las partidas 20 y 22. */
    --(1)--
    /*    BEGIN
          SELECT COUNT(1)
            INTO V_GENERA_ASIENTO
            FROM EJECUCION.T_DEVENGADO D
           WHERE (D.ID_DEVENGADO_REL IS NULL OR
                 (D.ID_DEVENGADO_REL IS NOT NULL AND
                 D.ID_TIPODOC IN (101, 145, 104, 166)))
             AND D.ID_DEVENGADO = V_ID_DEVENGADO;
        EXCEPTION
          WHEN NO_DATA_FOUND THEN
            V_GENERA_ASIENTO := 0;
        END;
        --(2)--
        BEGIN
          SELECT NVL(COUNT(1), 0)
            INTO V_GENERA_ASIENTO
            FROM EJECUCION.T_DEVENGADO_DETALLE DD
           WHERE DD.ID_DEVENGADO = P_ID_DEVENGADO
             AND DD.ID_ESTADO != 20
             HAVING COUNT(1) != (SELECT NVL(COUNT(1), 0)
                                FROM EJECUCION.T_DEVENGADO_DETALLE DD
                               WHERE DD.ID_DEVENGADO = V_ID_DEVENGADO
                                 and dd.id_estado <> 20
                                 AND DECODE(LENGTH(EJECUCION.F_NRO_OBJETO_GASTO(DD.ID_OBJETO_GASTO)),
                                            7,
                                            SUBSTR(EJECUCION.F_NRO_OBJETO_GASTO(DD.ID_OBJETO_GASTO),
                                                   1,
                                                   1),
                                            SUBSTR(EJECUCION.F_NRO_OBJETO_GASTO(DD.ID_OBJETO_GASTO),
                                                   1,
                                                   2)) IN (20, 22));



        EXCEPTION
          WHEN NO_DATA_FOUND THEN
            V_GENERA_ASIENTO := 0;
        END;
        ------------------------------------------------------------------------------
        IF V_GENERA_ASIENTO > 0 THEN
          --Busca los datos del Devengado
          BEGIN
            SELECT D.ID_VIGENCIA,
                   D.ID_TIPODOC,
                   D.ID_UNIDAD_ADM,
                   D.FECHA_CONFORMACION
              INTO V_ID_VIGENCIA,
                   V_ID_TIPODOC,
                   V_ID_UNIDAD_ADM,
                   V_FECHA_CONFORMACION
              FROM EJECUCION.T_DEVENGADO D
             WHERE D.ID_DEVENGADO = V_ID_DEVENGADO;

             select count(ua.id_unidad_adm) into v_es_administracion_gral
      from PRESUPUESTO.t_unidades_administrativa ua
     where ua.id_unidad_adm = V_ID_UNIDAD_ADM
       and ua.id_jurisdiccion in
           (select j.id_jurisdiccion from PRESUPUESTO.t_jurisdicciones j where j.id_grupo_sector = (select gs.id_grupo_sector
                                                                       from PRESUPUESTO.t_grupo_sectores gs
                                                                      where gs.id_vigencia = V_ID_VIGENCIA
                                                                        and gs.numero_grupo_sector =
                                                                            '1010101'));
          EXCEPTION
            WHEN OTHERS THEN
              BEGIN
                P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO',
                                                  V_ID_DEVENGADO,
                                                  'FALLA BUSCAR EJECUCION.T_DEVENGADO' ||
                                                  TO_CHAR(V_ID_DEVENGADO) || '-' ||
                                                  UPPER(P_MSGERR));

              EXCEPTION
                WHEN OTHERS THEN
                  NULL;
              END;
          END;
          --Busca el A?o en curso
          BEGIN
            SELECT EJECUCION.F_BUSCA_EJERCICIO(V_ID_VIGENCIA)
              INTO V_EJERCICIO
              FROM DUAL;
          EXCEPTION
            WHEN OTHERS THEN
              BEGIN
                P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO',
                                                  V_ID_DEVENGADO,
                                                  'FALLA BUSCAR EJECUCION.F_BUSCA_EJERCICIO ' ||
                                                  TO_CHAR(V_ID_VIGENCIA) || '-' ||
                                                  UPPER(P_MSGERR));
              EXCEPTION
                WHEN OTHERS THEN
                  NULL;
              END;
          END;
          --Busca el Tipo de Asiento y su Leyenda(Dependen del tipo de documento que sea el devengado)
       \*   BEGIN
            SELECT CC.ID_TIPO_ASIENTO, CC.LEYENDA
              INTO V_ID_TIPO_ASIENTO, V_LEYENDA
              FROM EJECUCION.T_CODIGOS_CONTABILIZACION CC
             WHERE CC.ID_TIPODOC =
                   DECODE(V_ID_TIPODOC, '167', '91', V_ID_TIPODOC)
               AND CC.TABLA_ORIGEN = 'T_DEVENGADO';
          EXCEPTION
            WHEN NO_DATA_FOUND THEN
              BEGIN
                P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO',
                                                  V_ID_DEVENGADO,
                                                  'FALLA BUSCAR EJECUCION.T_CODIGOS_CONTABILIZACION NO_DATA_FOUND ' ||
                                                  TO_CHAR(V_ID_TIPODOC) || '-' ||
                                                  UPPER(P_MSGERR));
              EXCEPTION
                WHEN OTHERS THEN
                  NULL;
              END;
            WHEN OTHERS THEN
              BEGIN
                P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO',
                                                  V_ID_DEVENGADO,
                                                  'FALLA BUSCAR EJECUCION.T_CODIGOS_CONTABILIZACION WHEN OTHERS ' ||
                                                  TO_CHAR(V_ID_TIPODOC) || '-' ||
                                                  UPPER(P_MSGERR));
              EXCEPTION
                WHEN OTHERS THEN
                  NULL;
              END;
          END;*\
          --Busca proximo numero de Asiento
     \*     BEGIN
            V_NUMERO := null; --EJECUCION.F_GENERA_NRO_ASIENTO(V_EJERCICIO); se comenta porque no se coloca al generar nro asiento
          EXCEPTION
            WHEN OTHERS THEN
              P_MSGERR := SUBSTR(SQLERRM, 1, 200);
              BEGIN
                V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO',
                                                  V_ID_DEVENGADO,
                                                  'FALLA BUSCAR EJECUCION.F_GENERA_NRO_ASIENTO ' ||
                                                  V_EJERCICIO || '-' ||
                                                  UPPER(P_MSGERR));
              EXCEPTION
                WHEN OTHERS THEN
                  NULL;
              END;
          END;*\
          \*Si no hay errores se continua con la insercion. Esta condicion esta puesta porque si falla
          lo anterior tambien va a fallar la insercion y se van a insertar archivos log de mas
          *\
          IF P_MSGERR IS NULL THEN
    if v_es_administracion_gral <>0 then
            SELECT EJECUCION.SEQ_ASIENTOS.NEXTVAL INTO V_ID_ASIENTO FROM DUAL;

            SELECT TO_NUMBER(TO_CHAR(SYSDATE, 'YYYY') - 1)
              INTO V_A?O
              FROM DUAL;
            -- DETERMINA FECHA CONTABLE Gali 14857
            IF V_A?O = V_EJERCICIO THEN
              \*Si al a?o sgte cargo un devengado con fecha del anterior, la fecha contable
              debe ser el 31/12 del a?o anterior*\
              V_FEC_CONTABLE := TO_DATE('31/12/' || V_EJERCICIO, 'DD/MM/YYYY');
            ELSE
              SELECT FCE.FECHA_CORTE
                INTO V_FECHA_CORTE
                FROM EJECUCION.T_FECHA_CORTE_ETAPAS FCE
               WHERE FCE.ID_ETAPA = 16
                 AND FCE.ID_UNIDAD_ADM = V_ID_UNIDAD_ADM;

              IF V_FECHA_CONFORMACION <= V_FECHA_CORTE THEN
                V_FEC_CONTABLE := V_FECHA_CORTE;
              ELSE
                V_FEC_CONTABLE := V_FECHA_CONFORMACION;
              END IF;
            END IF;
            BEGIN
              --Si es edicion especial, la fecha contable debe ser= a la original
              SELECT NVL(COUNT(1), 0)
                INTO V_CANT
                FROM EJECUCION.T_DEVENGADO
               WHERE ID_DEVENGADO = V_ID_DEVENGADO
                 AND ID_ESTADO IN (32, 34);

              IF V_CANT > 0 THEN
                SELECT FEC_CONTABLE
                  INTO V_FEC_CONTABLE
                  FROM EJECUCION.T_ASIENTOS A1
                 WHERE ID_DEVENGADO = V_ID_DEVENGADO
                   AND A1.ID_ASIENTO =
                       (SELECT MIN(A2.ID_ASIENTO)
                          FROM EJECUCION.T_ASIENTOS A2
                         WHERE A1.ID_DEVENGADO = A2.ID_DEVENGADO);
              END IF;
            EXCEPTION
              WHEN OTHERS THEN
                NULL;
            END;
            BEGIN
              \*----------INSERTA CABECERA DEL ASIENTO----------*\
              INSERT INTO EJECUCION.T_ASIENTOS
                (ID_ASIENTO,
                 NUMERO,
                 ANIO,
                 ID_VIGENCIA,
                 ID_UNIDAD_ADM,
                 FEC_ALTA,
                 USER_ALTA,
                 ID_DEVENGADO,
                 ID_TIPO_ASIENTO,
                 FEC_CONTABLE,
                 LEYENDA,
                 VISIBLE)
              VALUES
                (V_ID_ASIENTO,
                 V_NUMERO,
                 V_EJERCICIO,
                 V_ID_VIGENCIA,
                 V_ID_UNIDAD_ADM,
                 SYSDATE,
                 USER,
                 V_ID_DEVENGADO,
                 V_ID_TIPO_ASIENTO,
                 V_FEC_CONTABLE,
                 V_LEYENDA,
                 DECODE(V_ID_TIPODOC, '167', 'N', 'S'));
            EXCEPTION
              WHEN DUP_VAL_ON_INDEX THEN
                \*Si el numero de asiento ya existe, se busca el proximo y se intenta
                nuevamente la insercion de la cabecera*\
                BEGIN
                  V_NUMERO := null; --EJECUCION.F_GENERA_NRO_ASIENTO(V_EJERCICIO);, se comenta porque no se coloca al generar nro asiento
                EXCEPTION
                  WHEN OTHERS THEN
                    NULL;
                END;

                BEGIN
                  INSERT INTO EJECUCION.T_ASIENTOS
                    (ID_ASIENTO,
                     NUMERO,
                     ANIO,
                     ID_VIGENCIA,
                     ID_UNIDAD_ADM,
                     FEC_ALTA,
                     USER_ALTA,
                     ID_DEVENGADO,
                     ID_TIPO_ASIENTO,
                     FEC_CONTABLE,
                     LEYENDA,
                     VISIBLE)
                  VALUES
                    (V_ID_ASIENTO,
                     V_NUMERO,
                     V_EJERCICIO,
                     V_ID_VIGENCIA,
                     V_ID_UNIDAD_ADM,
                     SYSDATE,
                     USER,
                     V_ID_DEVENGADO,
                     V_ID_TIPO_ASIENTO,
                     V_FEC_CONTABLE,
                     V_LEYENDA,
                     DECODE(V_ID_TIPODOC, '167', 'N', 'S'));
                EXCEPTION
                  WHEN DUP_VAL_ON_INDEX THEN
                    \*Si se repite nuevamente el numero de asiento se loguea el error*\
                    BEGIN
                      P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                      V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO',
                                                        V_ID_DEVENGADO,
                                                        'FALLA INSERT EJECUCION.T_ASIENTOS - 2?INTENTO.ASIENTO: ' ||
                                                        TO_CHAR(V_ID_ASIENTO) || '-' ||
                                                        TO_CHAR(V_NUMERO) || '-' ||
                                                        TO_CHAR(V_EJERCICIO) || '-' ||
                                                        TO_CHAR(V_ID_VIGENCIA) || '-' ||
                                                        TO_CHAR(V_ID_UNIDAD_ADM) || '-' ||
                                                        TO_CHAR(SYSDATE,
                                                                'DD/MM/RRRR HH24:MI:SS') || '-' || USER || '-' ||
                                                        TO_CHAR(V_ID_DEVENGADO) || '-' ||
                                                        V_ID_TIPO_ASIENTO || '-' ||
                                                        V_FECHA_CONFORMACION || '-' ||
                                                        V_LEYENDA || '-' ||
                                                        UPPER(RTRIM(P_MSGERR)));
                    EXCEPTION
                      WHEN OTHERS THEN
                        NULL;
                    END;
                END;
              WHEN OTHERS THEN
                BEGIN
                  --Se loguea el error si falla la insercion de la cabecera de Asiento por otro problema
                  P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                  V_ERRLOG := EJECUCION.PKG_ASIENTOS_DEVENGADO.FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO',
                                                                                V_ID_DEVENGADO,
                                                                                'FALLA INSERT EJECUCION.T_ASIENTOS - WHEN OTHERS ' ||
                                                                                TO_CHAR(V_ID_ASIENTO) || '-' ||
                                                                                TO_CHAR(V_NUMERO) || '-' ||
                                                                                TO_CHAR(V_EJERCICIO) || '-' ||
                                                                                TO_CHAR(V_ID_VIGENCIA) || '-' ||
                                                                                TO_CHAR(V_ID_UNIDAD_ADM) || '-' ||
                                                                                TO_CHAR(SYSDATE,
                                                                                        'DD/MM/RRRR HH24:MI:SS') || '-' || USER || '-' ||
                                                                                TO_CHAR(V_ID_DEVENGADO) || '-' ||
                                                                                V_ID_TIPO_ASIENTO || '-' ||
                                                                                V_FECHA_CONFORMACION || '-' ||
                                                                                V_LEYENDA || '-' ||
                                                                                UPPER(RTRIM(P_MSGERR)));
                EXCEPTION
                  WHEN OTHERS THEN
                    NULL;
                END;
            END;
            END IF;
          END IF;
        END IF;
    */
    P_ID_ASIENTO := 0; --V_ID_ASIENTO;
  END INSERTAR_ASIENTOS_CAB;

  --// PARA DESHABILITAR / HABILITAR LA GENERACION DE ASIENTOS DE DEVENGADO DE FORMA MAS RAPIDA  SE CREAN ESTOS PROCEDIMIENTOS
  --// CAMBIAMOS ENTRE UNO Y OTRO  POR ESO ESTAN DOS VECES .
  /*  PROCEDURE INSERTAR_ASIENTOS_DET(P_ID_DEVENGADO    IN EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE,
                                  P_ID_DEVENGADODET IN EJECUCION.T_DEVENGADO_DETALLE.ID_DEVENGADODET%TYPE,
                                  P_ID_OBJETO_GASTO IN PRESUPUESTO.T_OBJETOS_GASTO.ID_OBJETO_GASTO%TYPE,
                                  P_IMPORTE         IN EJECUCION.T_DEVENGADO_DETALLE.IMPORTE%TYPE,
                                  P_CANTIDAD        IN EJECUCION.T_DEVENGADO_DETALLE.CANTIDAD%TYPE,
                                  P_ID_ASIENTO      IN NUMBER,
                                  P_MSGERR          OUT VARCHAR2) IS

   BEGIN

        P_MSGERR := NULL;
   END     INSERTAR_ASIENTOS_DET ;
  */
  --------------------

  PROCEDURE INSERTAR_ASIENTOS_DET(P_ID_DEVENGADO    IN EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE,
                                  P_ID_DEVENGADODET IN EJECUCION.T_DEVENGADO_DETALLE.ID_DEVENGADODET%TYPE,
                                  P_ID_OBJETO_GASTO IN PRESUPUESTO.T_OBJETOS_GASTO.ID_OBJETO_GASTO%TYPE,
                                  P_IMPORTE         IN EJECUCION.T_DEVENGADO_DETALLE.IMPORTE%TYPE,
                                  P_CANTIDAD        IN EJECUCION.T_DEVENGADO_DETALLE.CANTIDAD%TYPE,
                                  P_ID_ASIENTO      IN NUMBER,
                                  P_MSGERR          OUT VARCHAR2) IS
    /*
        V_ID_CTA_CONTABLE    NUMBER := 0;
        V_TIPO_SALDO         VARCHAR2(1) := NULL;
        V_GENERA_ASIENTO     NUMBER(10) := 0;
        V_ESTA_ANULADO       NUMBER(10) := 0;
        V_ID_TIPO_ASIENTO    NUMBER;
        V_LEYENDA            VARCHAR2(500);
        V_NUMERO             NUMBER(10);
        V_EJERCICIO          PRESUPUESTO.T_VIGENCIAS.EJERCICIO%TYPE;
        V_ID_VIGENCIA        EJECUCION.T_DEVENGADO.ID_VIGENCIA%TYPE;
        V_ID_TIPODOC         EJECUCION.T_DEVENGADO.ID_TIPODOC%TYPE;
        V_PARTIDA            PRESUPUESTO.T_OBJETOS_GASTO.ID_OBJETO_GASTO%TYPE;
        V_ID_UNIDAD_ADM      EJECUCION.T_DEVENGADO.ID_UNIDAD_ADM%TYPE;
        V_FECHA_CONFORMACION EJECUCION.T_DEVENGADO.FECHA_CONFORMACION%TYPE;
        V_ID_RECURSO_D_H     NUMBER(10);
        V_ERRLOG             BOOLEAN := FALSE;
        V_EXISTE             NUMBER := 0;
        V_FEC_CONTABLE       date;
        V_ID_DEVENGADO       EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE := P_ID_DEVENGADO;
        V_ID_DEVENGADODET    EJECUCION.T_DEVENGADO_DETALLE.ID_DEVENGADODET%TYPE := P_ID_DEVENGADODET;
        V_ID_OBJETO_GASTO    PRESUPUESTO.T_OBJETOS_GASTO.ID_OBJETO_GASTO%TYPE := P_ID_OBJETO_GASTO;
        V_ID_PROVEEDOR       EJECUCION.T_DEVENGADO.ID_PROVEEDOR%TYPE;
        V_IMPORTE            EJECUCION.T_DEVENGADO_DETALLE.IMPORTE%TYPE := P_IMPORTE;
        V_CANTIDAD           EJECUCION.T_DEVENGADO_DETALLE.CANTIDAD%TYPE := P_CANTIDAD;
        V_ID_ASIENTO         NUMBER;
        v_es_administracion_gral          number(10);
        V_TIPO_AUXILIAR      NUMBER;
    */
  BEGIN
    P_MSGERR := '';
    /*    SELECT NVL(P_ID_ASIENTO, 0) INTO V_ID_ASIENTO FROM DUAL;
        \* PARTIDA DOBLE
        No debe generarse el asiento si se carga un devengado:
          1) Asociado a otro devengado, salvo para: NOTA DE CREDITO, NOTA DE CREDITO PAICOR,
                                                    NOTA DE DEBITO Y CERTIFICACION DE SALDO.
          2) Contra las partidas 20 y 22. *\
        --(1)--
        SELECT COUNT(1)
          INTO V_GENERA_ASIENTO
          FROM EJECUCION.T_DEVENGADO D
         WHERE (D.ID_DEVENGADO_REL IS NULL OR
               (D.ID_DEVENGADO_REL IS NOT NULL AND
               D.ID_TIPODOC IN (101, 145, 104, 166)))
           AND D.ID_DEVENGADO = V_ID_DEVENGADO;
        --(2)--
        SELECT COUNT(1)
          INTO V_ESTA_ANULADO
          FROM EJECUCION.T_DEVENGADO_DETALLE DD
         WHERE DD.ID_DEVENGADODET = V_ID_DEVENGADODET
           AND DD.ID_ESTADO = 20;

        SELECT DECODE(LENGTH(EJECUCION.F_NRO_OBJETO_GASTO(V_ID_OBJETO_GASTO)),
                      7,
                      SUBSTR(EJECUCION.F_NRO_OBJETO_GASTO(V_ID_OBJETO_GASTO), 1, 1),
                      SUBSTR(EJECUCION.F_NRO_OBJETO_GASTO(V_ID_OBJETO_GASTO), 1, 2))
          INTO V_PARTIDA
          FROM DUAL;

        IF V_PARTIDA = 20 OR V_PARTIDA = 22 THEN
          V_GENERA_ASIENTO := 0;
        END IF;

        IF V_GENERA_ASIENTO > 0 THEN
          --Busca los datos del Devengado
          BEGIN
            SELECT D.ID_VIGENCIA,
                   D.ID_TIPODOC,
                   D.ID_UNIDAD_ADM,
                   D.FECHA_CONFORMACION,
                   D.ID_PROVEEDOR
              INTO V_ID_VIGENCIA,
                   V_ID_TIPODOC,
                   V_ID_UNIDAD_ADM,
                   V_FECHA_CONFORMACION,
                   V_ID_PROVEEDOR
              FROM EJECUCION.T_DEVENGADO D
             WHERE D.ID_DEVENGADO = V_ID_DEVENGADO;

                select count(ua.id_unidad_adm) into v_es_administracion_gral
      from PRESUPUESTO.t_unidades_administrativa ua
     where ua.id_unidad_adm = V_ID_UNIDAD_ADM
       and ua.id_jurisdiccion in
           (select j.id_jurisdiccion from PRESUPUESTO.t_jurisdicciones j where j.id_grupo_sector = (select gs.id_grupo_sector
                                                                       from PRESUPUESTO.t_grupo_sectores gs
                                                                      where gs.id_vigencia = V_ID_VIGENCIA
                                                                        and gs.numero_grupo_sector =
                                                                            '1010101'));
          EXCEPTION
            WHEN OTHERS THEN
              BEGIN
                P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO',
                                                  V_ID_DEVENGADO,
                                                  'FALLA BUSCAR EJECUCION.T_DEVENGADO' ||
                                                  TO_CHAR(V_ID_DEVENGADO) || '-' ||
                                                  UPPER(P_MSGERR));
              EXCEPTION
                WHEN OTHERS THEN
                  NULL;
              END;
          END;
          IF V_ID_TIPODOC in (101, 145, 104, 166, 147) THEN
            V_TIPO_AUXILIAR := 1;
          ELSE
            V_TIPO_AUXILIAR := 2;
          END IF;
          \*Si falla la insercion de la cabecera, no tiene sentido seguir con el detalle*\
          IF P_MSGERR IS NULL THEN
            IF V_ESTA_ANULADO = 0 THEN
              FOR I IN 1 .. 2 LOOP
                IF I = 1 THEN
                  \*El primer asiento se genera con los datos de EJECUCION.T_CODIGOS_CONTABILIZACION*\
                  BEGIN
                    SELECT CC.TIPO_SALDO, CC.ID_CTA_CONTABLE
                      INTO V_TIPO_SALDO, V_ID_CTA_CONTABLE
                      FROM EJECUCION.T_CODIGOS_CONTABILIZACION CC
                     WHERE CC.ID_TIPODOC =
                           DECODE(V_ID_TIPODOC, '167', '91', V_ID_TIPODOC)
                       AND CC.TABLA_ORIGEN = 'T_DEVENGADO';
                  EXCEPTION
                    WHEN NO_DATA_FOUND THEN
                      BEGIN
                        P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                        V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO_DETALLE',
                                                          TO_CHAR(V_ID_DEVENGADODET),
                                                          'FALLA BUSCAR EJECUCION.T_CODIGOS_CONTABILIZACION NO_DATA_FOUND ' ||
                                                          TO_CHAR(V_ID_TIPODOC) || '-' ||
                                                          UPPER(P_MSGERR));
                      EXCEPTION
                        WHEN OTHERS THEN
                          NULL;
                      END;
                    WHEN OTHERS THEN
                      BEGIN
                        P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                        V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO_DETALLE',
                                                          TO_CHAR(V_ID_DEVENGADODET),
                                                          'FALLA BUSCAR EJECUCION.T_CODIGOS_CONTABILIZACION OTHERS ' ||
                                                          TO_CHAR(V_ID_TIPODOC) || '-' ||
                                                          UPPER(P_MSGERR));
                      EXCEPTION
                        WHEN OTHERS THEN
                          NULL;
                      END;
                  END;
                ELSE
                  \*El segundo asiento se genera con los datos de EJECUCION.T_MATRIZ_CONTABLE*\
                  BEGIN
                    SELECT MC.ID_CTA_CONTABLE
                      INTO V_ID_CTA_CONTABLE
                      FROM EJECUCION.T_MATRIZ_CONTABLE MC
                     WHERE MC.ID_OBJETO_GASTO = V_ID_OBJETO_GASTO;
                  EXCEPTION
                    WHEN NO_DATA_FOUND THEN
                      BEGIN
                        P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                        V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO_DETALLE',
                                                          TO_CHAR(V_ID_DEVENGADODET),
                                                          'FALLA BUSCAR EJECUCION.T_MATRIZ_CONTABLE NO_DATA_FOUND ' ||
                                                          TO_CHAR(V_ID_OBJETO_GASTO) || '-' ||
                                                          UPPER(P_MSGERR));
                      EXCEPTION
                        WHEN OTHERS THEN
                          NULL;
                      END;
                    WHEN OTHERS THEN
                      BEGIN
                        P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                        V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO_DETALLE',
                                                          TO_CHAR(V_ID_DEVENGADODET),
                                                          'FALLA BUSCAR EJECUCION.T_MATRIZ_CONTABLE WHEN OTHERS ' ||
                                                          TO_CHAR(V_ID_OBJETO_GASTO) || '-' ||
                                                          UPPER(P_MSGERR));
                      EXCEPTION
                        WHEN OTHERS THEN
                          NULL;
                      END;
                  END;
                  \*El tipo de saldo del segundo asiento debe ser el opuesto al del primer asiento*\
                  SELECT DECODE(V_TIPO_SALDO, 'D', 'H', 'D')
                    INTO V_TIPO_SALDO
                    FROM DUAL;
                END IF;
                \*Busca el dato para completar el campo ID_RECURSO_D_H*\
                BEGIN
                  SELECT DECODE(V_TIPO_SALDO,
                                'H',
                                CTAC.ID_RECURSO_HABER,
                                CTAC.ID_RECURSO_DEBE)
                    INTO V_ID_RECURSO_D_H
                    FROM EJECUCION.T_CUENTAS_CONTABLES CTAC
                   WHERE CTAC.ID_CTA_CONTABLE = V_ID_CTA_CONTABLE;
                EXCEPTION
                  WHEN NO_DATA_FOUND THEN
                    BEGIN
                      P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                      V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO_DETALLE',
                                                        TO_CHAR(V_ID_DEVENGADODET),
                                                        'FALLA BUSQUEDA EJECUCION.T_CUENTAS_CONTABLES NO_DATA_FOUND ' ||
                                                        V_ID_CTA_CONTABLE || '-' ||
                                                        UPPER(P_MSGERR));
                    EXCEPTION
                      WHEN OTHERS THEN
                        NULL;
                    END;
                  WHEN OTHERS THEN
                    BEGIN
                      P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                      V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO_DETALLE',
                                                        TO_CHAR(V_ID_DEVENGADODET),
                                                        'FALLA BUSQUEDA EJECUCION.T_CUENTAS_CONTABLES OTHERS' ||
                                                        V_ID_CTA_CONTABLE || '-' ||
                                                        UPPER(P_MSGERR));
                    EXCEPTION
                      WHEN OTHERS THEN
                        NULL;
                    END;
                END;
                IF P_MSGERR IS NULL THEN
                  if v_es_administracion_gral <>0 then
                  \*----------INSERTA EL DETALLE DEL ASIENTO----------*\
                  BEGIN
                    INSERT INTO EJECUCION.T_ASIENTOS_DET
                      (ID_ASIENTO_DET,
                       ID_ASIENTO,
                       ID_CTA_CONTABLE,
                       FEC_CONTABLE,
                       DEBE,
                       HABER,
                       ID_DEVENGADODET,
                       USER_ING,
                       FEC_ING,
                       ANULADO,
                       ID_RECURSO_D_H,
                       ID_AUX_PROV,
                       ID_AUX_OG)
                    VALUES
                      (EJECUCION.SEQ_ASIENTOS_DET.NEXTVAL,
                       V_ID_ASIENTO,
                       V_ID_CTA_CONTABLE,
                       (SELECT FEC_CONTABLE
                          FROM EJECUCION.T_ASIENTOS
                         WHERE ID_ASIENTO = V_ID_ASIENTO),
                       DECODE(V_TIPO_SALDO,
                              'D',
                              ROUND(ABS(V_IMPORTE * V_CANTIDAD), 2),
                              NULL),
                       DECODE(V_TIPO_SALDO,
                              'H',
                              ROUND(ABS(V_IMPORTE * V_CANTIDAD), 2),
                              NULL),
                       V_ID_DEVENGADODET,
                       USER,

                       SYSDATE,
                       'N',
                       V_ID_RECURSO_D_H,
                       DECODE(V_TIPO_SALDO,
                              'D',
                              DECODE(V_TIPO_AUXILIAR, 1, V_ID_PROVEEDOR, NULL),
                              DECODE(V_TIPO_AUXILIAR, 2, V_ID_PROVEEDOR, NULL)),
                       DECODE(V_TIPO_SALDO,
                              'D',
                              DECODE(V_TIPO_AUXILIAR, 2, V_ID_OBJETO_GASTO, NULL),
                              DECODE(V_TIPO_AUXILIAR, 1, V_ID_OBJETO_GASTO, NULL)));
                  EXCEPTION
                    WHEN OTHERS THEN
                      BEGIN
                        UPDATE EJECUCION.T_ASIENTOS A
                           SET A.VISIBLE = 'N'
                         WHERE A.ID_ASIENTO = V_ID_ASIENTO;
                        P_MSGERR := SUBSTR(SQLERRM, 1, 200);
                        V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO_DETALLE',
                                                          TO_CHAR(V_ID_DEVENGADODET),
                                                          'FALLA INSERT EJECUCION.T_ASIENTOS_DET ' ||
                                                          V_ID_ASIENTO || '-' ||
                                                          V_ID_CTA_CONTABLE || '-' ||
                                                          V_TIPO_SALDO || '-' ||
                                                          TO_CHAR(ABS(V_IMPORTE *
                                                                      V_CANTIDAD)) ||
                                                          UPPER(P_MSGERR));
                      EXCEPTION
                        WHEN OTHERS THEN
                          NULL;
                      END;
                  END;
                  end if;
                ELSE
                  UPDATE EJECUCION.T_ASIENTOS A
                     SET A.VISIBLE = 'N'
                   WHERE A.ID_ASIENTO = V_ID_ASIENTO;
                END IF;
              END LOOP;
            END IF;
          END IF;
        END IF;
    */
  END INSERTAR_ASIENTOS_DET;

  --// PARA DESHABILITAR / HABILITAR LA GENERACION DE ASIENTOS DE DEVENGADO DE FORMA MAS RAPIDA  SE CREAN ESTOS PROCEDIMIENTOS
  --// CAMBIAMOS ENTRE UNO Y OTRO  POR ESO ESTAN DOS VECES
  /*
  PROCEDURE INSERTAR_CONTRA_ASIENTOS(P_ID_DEVENGADO    IN EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE DEFAULT NULL,
                                     P_ID_DEVENGADODET IN EJECUCION.T_DEVENGADO_DETALLE.ID_DEVENGADODET%TYPE DEFAULT NULL,
                                     P_ID_ASIENTO      OUT NUMBER) IS

  BEGIN


      P_ID_ASIENTO := 0 ;


  END INSERTAR_CONTRA_ASIENTOS;   */

  PROCEDURE INSERTAR_CONTRA_ASIENTOS(P_ID_DEVENGADO    IN EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE DEFAULT NULL,
                                     P_ID_DEVENGADODET IN EJECUCION.T_DEVENGADO_DETALLE.ID_DEVENGADODET%TYPE DEFAULT NULL,
                                     P_ID_ASIENTO      OUT NUMBER) IS
    /*
        V_PROX_ASIENTO    NUMBER := 0;
        V_ASIENTO_ACTUAL  NUMBER := 0;
        V_NUMERO          NUMBER(10) := 0;
        V_ID_VIGENCIA     EJECUCION.T_DEVENGADO.ID_VIGENCIA%TYPE := 0;
        V_MSGERR          VARCHAR2(500) := NULL;
        V_ERRLOG          BOOLEAN := FALSE;
        V_CANT_ASDET      NUMBER(10);
        V_EJERCICIO       PRESUPUESTO.T_VIGENCIAS.EJERCICIO%TYPE;
        V_ID_DEVENGADO    EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE := P_ID_DEVENGADO;
        V_ID_DEVENGADODET EJECUCION.T_DEVENGADO_DETALLE.ID_DEVENGADODET%TYPE := P_ID_DEVENGADODET;
        \*Trae los asientos NO anulados (para luego reversarlos)*\
        CURSOR C_ASIENTOS_DET IS
          SELECT AD.ID_ASIENTO_DET,
                 AD.ID_ASIENTO,
                 AD.ID_CTA_CONTABLE,
                 AD.FEC_CONTABLE,
                 AD.DEBE,
                 AD.HABER,
                 AD.ID_DEVENGADODET,
                 AD.ANULADO,
                 AD.ID_RECURSO_D_H,
                 AD.ID_AUX_PROV,
                 AD.ID_AUX_OG
            FROM EJECUCION.T_ASIENTOS_DET AD
           WHERE ID_ASIENTO = V_ASIENTO_ACTUAL
             AND AD.ANULADO = 'N';
    */
    V_ID_ASIENTO NUMBER := 0;
  BEGIN
    P_ID_ASIENTO := 0;

    SELECT DISTINCT A.ID_ASIENTO
      INTO V_ID_ASIENTO
      FROM T_ASIENTOS A
      JOIN T_ASIENTOS_DET AD
        ON A.ID_ASIENTO = AD.ID_ASIENTO
      JOIN T_DEVENGADO_DETALLE DD
        ON DD.ID_DEVENGADODET = AD.ID_DEVENGADODET
     WHERE DD.ID_DEVENGADO = P_ID_DEVENGADO
       AND A.ID_ASIENTO_REL IS NULL;

    INSERTAR_CONTAASIENTO(V_ID_ASIENTO);
    ------------------------------------------------------------------------------
    --Obtiene V_ASIENTO_ACTUAL
    /*    IF (V_ID_DEVENGADO IS NOT NULL AND V_ID_DEVENGADODET IS NOT NULL) OR
           (V_ID_DEVENGADO IS NOT NULL AND V_ID_DEVENGADODET IS NULL) THEN
          \*Se inserta un nuevo item en un devengado existente*\
          BEGIN
            --Busca el id_asiento que hay que reversar
            SELECT ID_ASIENTO, ID_VIGENCIA
              INTO V_ASIENTO_ACTUAL, V_ID_VIGENCIA
              FROM EJECUCION.T_ASIENTOS A
             WHERE A.ID_DEVENGADO = V_ID_DEVENGADO
               AND ID_ASIENTO =
                   (SELECT MAX(A2.ID_ASIENTO)
                      FROM EJECUCION.T_ASIENTOS A2
                     WHERE A2.ID_DEVENGADO = V_ID_DEVENGADO);
          EXCEPTION
            WHEN OTHERS THEN
              NULL;
          END;
        ELSE
          \*Se edita o anula un item del devengado*\
          --Busca el id_asiento que hay que reversar
          BEGIN
            SELECT A.ID_ASIENTO, A.ID_VIGENCIA
              INTO V_ASIENTO_ACTUAL, V_ID_VIGENCIA
              FROM EJECUCION.T_ASIENTOS A
             WHERE A.ID_ASIENTO = (SELECT ID_ASIENTO
                                     FROM EJECUCION.T_ASIENTOS_DET AD
                                    WHERE AD.ID_DEVENGADODET = V_ID_DEVENGADODET
                                      AND AD.ANULADO = 'N'
                                      AND ROWNUM = 1);
          EXCEPTION
            WHEN OTHERS THEN
              V_MSGERR := SUBSTR(SQLERRM, 1, 200);
              BEGIN
                V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO',
                                                  V_ID_DEVENGADODET,
                                                  'FALLA BUSCAR ASIENTO A REVERSAR INSERTAR_CONTRA_ASIENTOS' || '-' ||
                                                  V_ID_DEVENGADODET || '-' ||
                                                  UPPER(V_MSGERR));
              EXCEPTION
                WHEN OTHERS THEN
                  NULL;
              END;
          END;
        END IF;
        P_ID_ASIENTO := V_ASIENTO_ACTUAL;
        ------------------------------------------------------------------------------
        --Busca el A?o en curso
        BEGIN
          SELECT EJECUCION.F_BUSCA_EJERCICIO(V_ID_VIGENCIA)
            INTO V_EJERCICIO
            FROM DUAL;
        EXCEPTION
          WHEN OTHERS THEN
            BEGIN
              V_MSGERR := SUBSTR(SQLERRM, 1, 200);
              V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO',
                                                V_ID_DEVENGADO,
                                                'FALLA BUSCAR EJECUCION.F_BUSCA_EJERCICIO ' ||
                                                TO_CHAR(V_ID_VIGENCIA) || '-' ||
                                                UPPER(V_MSGERR));
            EXCEPTION
              WHEN OTHERS THEN
                NULL;
            END;
        END;
        -------------------------------------------------------------------------------
        --Continua si No hay errores y si hay asientos que reversar
        SELECT NVL(COUNT(1), 0)
          INTO V_CANT_ASDET
          FROM EJECUCION.T_ASIENTOS_DET AD
         WHERE ID_ASIENTO = V_ASIENTO_ACTUAL
           AND AD.ANULADO = 'N';

        IF V_MSGERR IS NULL AND V_CANT_ASDET > 0 THEN
          --Genera sgte. id de Asiento
          SELECT EJECUCION.SEQ_ASIENTOS.NEXTVAL INTO V_PROX_ASIENTO FROM DUAL;
          --Busca sgte. n? de Asiento
          BEGIN
            V_NUMERO := null; --EJECUCION.F_GENERA_NRO_ASIENTO(V_EJERCICIO);, se comenta porque no se coloca al generar nro asiento
          EXCEPTION
            WHEN OTHERS THEN
              V_MSGERR := SUBSTR(SQLERRM, 1, 200);
              BEGIN
                V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO_DETALLE',
                                                  V_ID_DEVENGADODET,
                                                  'FALLA BUSCAR EJECUCION.F_GENERA_NRO_ASIENTO AL GENERAR EL CONTRA ASIENTO' || '-' ||
                                                  V_EJERCICIO || '-' ||
                                                  UPPER(V_MSGERR));
              EXCEPTION
                WHEN OTHERS THEN
                  NULL;
              END;
          END;
        END IF;
        --Continua si No hay errores, si obtuvo el asiento actual y si hay asientos que reversar
        IF V_MSGERR IS NULL AND V_ASIENTO_ACTUAL > 0 AND V_CANT_ASDET > 0 THEN
          BEGIN
            INSERT INTO EJECUCION.T_ASIENTOS A1
              (A1.ID_ASIENTO,
               A1.NUMERO,
               A1.ANIO,
               A1.ID_VIGENCIA,
               A1.ID_UNIDAD_ADM,
               A1.OBSERVACIONES,
               A1.FEC_ALTA,
               A1.USER_ALTA,
               A1.ID_ASIENTO_REL,
               A1.ID_DEVENGADO,
               A1.ID_TIPO_ASIENTO,
               A1.FEC_CONTABLE,
               A1.LEYENDA)
              (SELECT V_PROX_ASIENTO,
                      V_NUMERO,
                      A.ANIO,
                      A.ID_VIGENCIA,
                      A.ID_UNIDAD_ADM,
                      A.OBSERVACIONES,
                      SYSDATE,
                      USER,
                      A.ID_ASIENTO,
                      A.ID_DEVENGADO,
                      A.ID_TIPO_ASIENTO,
                      A.FEC_CONTABLE,
                      'ANULA ' || A.LEYENDA --Gali 12862
                 FROM EJECUCION.T_ASIENTOS A
                WHERE A.ID_ASIENTO = V_ASIENTO_ACTUAL);
          EXCEPTION
            WHEN DUP_VAL_ON_INDEX THEN
              \*Si el numero de asiento ya existe, se busca el proximo y se intenta
              nuevamente la insercion de la cabecera*\
              BEGIN
                V_NUMERO := null; --EJECUCION.F_GENERA_NRO_ASIENTO(V_EJERCICIO);, se comenta porque no se coloca al generar nro asiento
              EXCEPTION
                WHEN OTHERS THEN
                  NULL;
                  V_MSGERR := NULL;
              END;
              BEGIN
                INSERT INTO EJECUCION.T_ASIENTOS A1
                  (A1.ID_ASIENTO,
                   A1.NUMERO,
                   A1.ANIO,
                   A1.ID_VIGENCIA,
                   A1.ID_UNIDAD_ADM,
                   A1.OBSERVACIONES,
                   A1.FEC_ALTA,
                   A1.USER_ALTA,
                   A1.ID_ASIENTO_REL,
                   A1.ID_DEVENGADO,
                   A1.ID_TIPO_ASIENTO,
                   A1.FEC_CONTABLE,
                   A1.LEYENDA)
                  (SELECT V_PROX_ASIENTO,
                          V_NUMERO,
                          A.ANIO,
                          A.ID_VIGENCIA,
                          A.ID_UNIDAD_ADM,
                          A.OBSERVACIONES,
                          SYSDATE,
                          USER,
                          A.ID_ASIENTO,
                          A.ID_DEVENGADO,
                          A.ID_TIPO_ASIENTO,
                          A.FEC_CONTABLE,
                          'ANULA ' || A.LEYENDA --Gali 12862
                     FROM EJECUCION.T_ASIENTOS A
                    WHERE A.ID_ASIENTO = V_ASIENTO_ACTUAL);
              EXCEPTION
                WHEN DUP_VAL_ON_INDEX THEN
                  \*Si se repite nuevamente el numero de asiento se loguea el error*\
                  BEGIN
                    V_MSGERR := SUBSTR(SQLERRM, 1, 200);
                    V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO_DETALLE',
                                                      V_ASIENTO_ACTUAL,
                                                      'FALLA INSERT EJECUCION.T_ASIENTOS NUMERO DE ASIENTO DUPLICADO AL GENERAR EL CONTRA ASIENTO');
                  EXCEPTION
                    WHEN OTHERS THEN
                      NULL;
                  END;
              END;
            WHEN OTHERS THEN
              BEGIN
                \*Si falla la insercion de la cabecera de Asiento por otro problema,
                se loguea el error*\
                V_MSGERR := SUBSTR(SQLERRM, 1, 200);
                V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO_DETALLE',
                                                  V_ASIENTO_ACTUAL,
                                                  'FALLA INSERT EJECUCION.T_ASIENTOS FALLO TAMBIEN EL 2?INTENTO AL GENERAR EL CONTRA ASIENTO' ||
                                                  V_MSGERR);
              EXCEPTION
                WHEN OTHERS THEN
                  NULL;
              END;
          END;

          BEGIN
            --Inserta los contra asientos en el detalle
            FOR R_ASIENTOS_DET IN C_ASIENTOS_DET LOOP
              BEGIN
                INSERT INTO EJECUCION.T_ASIENTOS_DET AD
                  (AD.ID_ASIENTO_DET,
                   AD.ID_ASIENTO,
                   AD.ID_CTA_CONTABLE,
                   AD.FEC_CONTABLE,
                   AD.DEBE,
                   AD.HABER,
                   AD.ID_DEVENGADODET,
                   AD.ANULADO,
                   AD.ID_RECURSO_D_H,
                   AD.ID_AUX_PROV,
                   AD.ID_AUX_OG,
                   AD.USER_ING,
                   AD.FEC_ING)
                VALUES
                  (EJECUCION.SEQ_ASIENTOS_DET.NEXTVAL,
                   V_PROX_ASIENTO,
                   R_ASIENTOS_DET.ID_CTA_CONTABLE,
                   R_ASIENTOS_DET.FEC_CONTABLE,
                   R_ASIENTOS_DET.HABER,
                   R_ASIENTOS_DET.DEBE,
                   R_ASIENTOS_DET.ID_DEVENGADODET,
                   'S',
                   (SELECT DECODE(R_ASIENTOS_DET.HABER,
                                  NULL,
                                  CTAC.ID_RECURSO_HABER,
                                  CTAC.ID_RECURSO_DEBE)
                      FROM EJECUCION.T_CUENTAS_CONTABLES CTAC
                     WHERE CTAC.ID_CTA_CONTABLE = R_ASIENTOS_DET.ID_CTA_CONTABLE),
                   R_ASIENTOS_DET.ID_AUX_PROV,
                   R_ASIENTOS_DET.ID_AUX_OG,
                   USER,
                   SYSDATE);
              EXCEPTION
                WHEN OTHERS THEN
                  BEGIN
                    V_MSGERR := SUBSTR(SQLERRM, 1, 200);
                    V_ERRLOG := FNC_INSERTAR_T_LOG_PD('EJECUCION.T_DEVENGADO_DETALLE',
                                                      V_PROX_ASIENTO,
                                                      'FALLA INSERT EJECUCION.T_ASIENTOS_DET AL GENERAR EL CONTRA ASIENTO' ||
                                                      V_MSGERR);
                  EXCEPTION
                    WHEN OTHERS THEN
                      NULL;
                  END;
              END;
            END LOOP;
            --Anula los detalles de los asientos originales
            BEGIN
              UPDATE EJECUCION.T_ASIENTOS_DET
                 SET ANULADO = 'S', FEC_ACT = SYSDATE, USER_ACT = USER
               WHERE ID_ASIENTO = V_ASIENTO_ACTUAL;
            EXCEPTION
              WHEN OTHERS THEN
                BEGIN
                  V_MSGERR := SUBSTR(SQLERRM, 1, 200);
                  V_ERRLOG := EJECUCION.PKG_ASIENTOS_DEVENGADO.FNC_INSERTAR_T_LOG_PD('EJECUCION.t_devengado',
                                                                                TO_CHAR(V_ASIENTO_ACTUAL),
                                                                                'Falla Anular asientos originales ' ||
                                                                                V_MSGERR);
                EXCEPTION
                  WHEN OTHERS THEN
                    NULL;
                END;
            END;
          EXCEPTION
            WHEN OTHERS THEN
              NULL;
          END;
        END IF;
    */
  END INSERTAR_CONTRA_ASIENTOS;

  PROCEDURE ANTES_GEN_ASIENTOS(P_FALLA_ASIENTO   OUT NUMBER,
                               P_GENERA_ASIENTO  OUT NUMBER,
                               P_ID_OBJETO_GASTO EJECUCION.T_DEVENGADO_DETALLE.ID_OBJETO_GASTO%TYPE,
                               P_ID_TIPODOC      EJECUCION.T_DEVENGADO.ID_TIPODOC%TYPE) IS
    /*
        V_ID_CTA_CONTABLE    NUMBER := 0;
        V_TIPO_SALDO         VARCHAR2(1);
        V_GENERA_ASIENTO     NUMBER(10) := 0;
        V_ID_TIPO_ASIENTO    NUMBER;
        V_LEYENDA            VARCHAR2(500);
        V_PARTIDA            PRESUPUESTO.T_OBJETOS_GASTO.ID_OBJETO_GASTO%TYPE;
        V_ID_RECURSO_D_H     NUMBER(10);
        V_FALLA_ASIENTO      NUMBER:=0;
    */
  BEGIN
    /* PARTIDA DOBLE
    No debe generarse el asiento si se carga un devengado:
      1) Asociado a otro devengado, salvo para: NOTA DE CREDITO, NOTA DE CREDITO PAICOR,
                                                NOTA DE DEBITO Y CERTIFICACION DE SALDO.
      2) Contra las partidas 20 y 22. */
    /*
    SELECT DECODE(LENGTH(EJECUCION.F_NRO_OBJETO_GASTO(P_ID_OBJETO_GASTO)),
                  7,
                  SUBSTR(EJECUCION.F_NRO_OBJETO_GASTO(P_ID_OBJETO_GASTO), 1, 1),
                  SUBSTR(EJECUCION.F_NRO_OBJETO_GASTO(P_ID_OBJETO_GASTO), 1, 2))
      INTO V_PARTIDA
      FROM DUAL;

    IF V_PARTIDA = 20 OR V_PARTIDA = 22 THEN
      V_GENERA_ASIENTO := 0;
    ELSE
      V_GENERA_ASIENTO := 1;
    END IF;

    IF V_GENERA_ASIENTO!=0 THEN
      BEGIN
         SELECT CC.TIPO_SALDO
          INTO V_TIPO_SALDO
          FROM EJECUCION.T_CODIGOS_CONTABILIZACION CC
         WHERE CC.ID_TIPODOC =
               DECODE(P_ID_TIPODOC, '167', '91', P_ID_TIPODOC)
           AND CC.TABLA_ORIGEN = 'T_DEVENGADO';
      EXCEPTION
        WHEN NO_DATA_FOUND THEN
          V_FALLA_ASIENTO:=1;
          V_TIPO_SALDO:=NULL;
      END;

      BEGIN
         SELECT MC.ID_CTA_CONTABLE
          INTO V_ID_CTA_CONTABLE
          FROM EJECUCION.T_MATRIZ_CONTABLE MC
         WHERE MC.ID_OBJETO_GASTO = P_ID_OBJETO_GASTO;
      EXCEPTION
        WHEN NO_DATA_FOUND THEN
          V_FALLA_ASIENTO:=1;
          V_ID_CTA_CONTABLE:=0;
      END;

      IF V_ID_CTA_CONTABLE!=0 AND V_TIPO_SALDO IS NOT NULL THEN
        BEGIN
           SELECT DECODE(V_TIPO_SALDO,
                        'H',
                        CTAC.ID_RECURSO_HABER,
                        CTAC.ID_RECURSO_DEBE)
            INTO V_ID_RECURSO_D_H
            FROM EJECUCION.T_CUENTAS_CONTABLES CTAC
           WHERE CTAC.ID_CTA_CONTABLE =V_ID_CTA_CONTABLE;
        EXCEPTION
          WHEN NO_DATA_FOUND THEN
            V_FALLA_ASIENTO:=1;
        END;
      ELSE
        V_FALLA_ASIENTO:=1;
      END IF;
    END IF;

    P_FALLA_ASIENTO:=V_FALLA_ASIENTO;
    P_GENERA_ASIENTO:=V_GENERA_ASIENTO;*/
    P_FALLA_ASIENTO  := 0;
    P_GENERA_ASIENTO := 0;

  END ANTES_GEN_ASIENTOS;

  --/*Scripts son para corregir datos*/
  PROCEDURE SCRIPT_INSERTAR_ASIENTO(P_T_DEVENGADO OUT TY_CURSOR,
                                    P_CANTIDAD    NUMBER) IS
    /*
    V_ID_DEVENGADO EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE := 0;
    V_ID_ASIENTO   NUMBER;
    V_MSGERR       VARCHAR2(500);
    V_ERRLOG       BOOLEAN := FALSE;
    V_DEVENGADO    VARCHAR2(32767);
    V_PASO         NUMBER := 0;
    V_CANTIDAD     NUMBER := P_CANTIDAD;*/
    /*Devengados que eran replica*/
    /*SELECT *
     FROM EJECUCION.T_DEVENGADO D
    WHERE NOT EXISTS (SELECT 1
             FROM EJECUCION.T_ASIENTOS A
            WHERE A.ID_DEVENGADO = D.ID_DEVENGADO)
      AND D.ID_VIGENCIA = 166
      AND EXISTS (SELECT 1
             FROM EJECUCION.T_DEVENGADO_LOG DL
            WHERE DL.ID_DEVENGADO = D.ID_DEVENGADO
              AND DL.ID_TIPODOC = '167')
      AND ID_ESTADO != 20
      AND ROWNUM <= DECODE(V_CANTIDAD,NULL,100,V_CANTIDAD);*/
    /*Devengado que son replica*/
    /*CURSOR C_ASIENTO IS
    SELECT *
      FROM EJECUCION.T_DEVENGADO D
     WHERE NOT EXISTS
     (SELECT 1
              FROM EJECUCION.T_ASIENTOS A
             WHERE A.ID_DEVENGADO = D.ID_DEVENGADO)
       AND D.ID_VIGENCIA = 166
       AND D.ID_TIPODOC = '167'
       AND ID_ESTADO != 20
       AND ROWNUM <= DECODE(V_CANTIDAD, NULL, 100, V_CANTIDAD);*/
    /*Tienen diferencias entre el monto del devengado_detalle y el asiento_detalle*/
    /*CURSOR C_ASIENTO IS
    SELECT A.ID_DEVENGADO
      FROM (SELECT ROUND(SUM(ABS(DD.CANTIDAD * DD.IMPORTE)), 2) IMPORTE,
                   DD.ID_DEVENGADO
              FROM EJECUCION.T_DEVENGADO_DETALLE DD
             WHERE ID_ESTADO != 20
               AND EXISTS (SELECT 1
                      FROM EJECUCION.T_DEVENGADO D
                     WHERE D.ID_ESTADO != 20
                       AND D.ID_DEVENGADO = DD.ID_DEVENGADO
                       AND D.ID_VIGENCIA = 166)
               AND 20 != DECODE(LENGTH(EJECUCION.F_NRO_OBJETO_GASTO(ID_OBJETO_GASTO)),
                                7,
                                SUBSTR(EJECUCION.F_NRO_OBJETO_GASTO(ID_OBJETO_GASTO),
                                       1,
                                       1),
                                SUBSTR(EJECUCION.F_NRO_OBJETO_GASTO(ID_OBJETO_GASTO),
                                       1,
                                       2))
               AND 22 != DECODE(LENGTH(EJECUCION.F_NRO_OBJETO_GASTO(ID_OBJETO_GASTO)),
                                7,
                                SUBSTR(EJECUCION.F_NRO_OBJETO_GASTO(ID_OBJETO_GASTO),
                                       1,
                                       1),
                                SUBSTR(EJECUCION.F_NRO_OBJETO_GASTO(ID_OBJETO_GASTO),
                                       1,
                                       2))
             GROUP BY DD.ID_DEVENGADO) A,
           (SELECT NVL(SUM(DEBE), 0) MONTO, DD.ID_DEVENGADO
              FROM EJECUCION.T_ASIENTOS_DET AD, EJECUCION.T_DEVENGADO_DETALLE DD
             WHERE AD.ID_DEVENGADODET IN
                   (SELECT DD.ID_DEVENGADODET
                      FROM EJECUCION.T_DEVENGADO_DETALLE DD
                     WHERE EXISTS (SELECT 1
                              FROM EJECUCION.T_DEVENGADO D
                             WHERE D.ID_ESTADO != 20
                               AND D.ID_DEVENGADO = DD.ID_DEVENGADO
                               AND D.ID_VIGENCIA = 166))
               AND AD.ANULADO = 'N'
               AND DD.ID_DEVENGADODET = AD.ID_DEVENGADODET
             GROUP BY DD.ID_DEVENGADO) B
     WHERE A.ID_DEVENGADO = B.ID_DEVENGADO
       AND A.IMPORTE != B.MONTO
       AND ROWNUM<=DECODE(V_CANTIDAD, NULL, 1000, V_CANTIDAD);*/
    /*Era replica, se genero el asiento cabecera y no el detalle*/
    /*CURSOR C_ASIENTO IS
    SELECT A.ID_DEVENGADO
      FROM EJECUCION.T_ASIENTOS A, EJECUCION.T_DEVENGADO D
     WHERE NOT EXISTS (SELECT 1
              FROM EJECUCION.T_ASIENTOS_DET AD
             WHERE AD.ID_ASIENTO = A.ID_ASIENTO)
       AND EXISTS
     (SELECT 1
              FROM EJECUCION.T_DEVENGADO_LOG DL
             WHERE DL.ID_DEVENGADO = A.ID_DEVENGADO
               AND DL.ID_TIPODOC = 167)
       AND A.VISIBLE = 'S'
       AND A.ID_VIGENCIA = 166
       AND A.ID_DEVENGADO IS NOT NULL
       AND D.ID_DEVENGADO = A.ID_DEVENGADO
       AND D.ID_ESTADO != 20
       AND ROWNUM <= DECODE(V_CANTIDAD, NULL, 100, V_CANTIDAD)
     GROUP BY A.ID_DEVENGADO;*/
    --       En lugar del sgte cursor habria que hacer una qry que controle asiento contra dev det
    /*No se genero el asiento detalle, no eran replica*/
    /*   CURSOR C_ASIENTO IS
    SELECT A.ID_DEVENGADO
      FROM EJECUCION.T_ASIENTOS A, EJECUCION.T_DEVENGADO D
     WHERE NOT EXISTS (SELECT 1
              FROM EJECUCION.T_ASIENTOS_DET AD
             WHERE AD.ID_ASIENTO = A.ID_ASIENTO)
       AND NOT EXISTS (SELECT 1
              FROM EJECUCION.T_DEVENGADO_LOG DL
             WHERE DL.ID_DEVENGADO = A.ID_DEVENGADO
               AND DL.ID_TIPODOC = 167)
       AND A.VISIBLE = 'S'
       AND A.ID_VIGENCIA = 166
       AND A.ID_DEVENGADO IS NOT NULL
       AND D.ID_DEVENGADO = A.ID_DEVENGADO
       AND D.ID_ESTADO != 20
       AND ROWNUM <= DECODE(V_CANTIDAD, NULL, 100, V_CANTIDAD)
     GROUP BY A.ID_DEVENGADO;*/
    /*Certificacion de saldo ahora deben generarse asientos*/
    /*    CURSOR C_ASIENTO IS
          SELECT *
            FROM EJECUCION.T_DEVENGADO
           WHERE ID_TIPODOC = 166
             AND ID_VIGENCIA = 166
             AND ID_ESTADO != 20;

        CURSOR C_DEVENGADODET IS
          SELECT *
            FROM EJECUCION.T_DEVENGADO_DETALLE DD
           WHERE ID_DEVENGADO = V_ID_DEVENGADO
             AND ID_ESTADO != 20
           ORDER BY DD.ID_DEVENGADO, DD.ID_DEVENGADODET;
    */
  BEGIN
    OPEN P_T_DEVENGADO FOR
      SELECT '' FROM DUAL;
    /*
        FOR R_ASIENTO IN C_ASIENTO LOOP
          --Se anula todo lo que exista para que se genere correctamente
          UPDATE EJECUCION.T_ASIENTOS AA
             SET AA.VISIBLE = 'N'
           WHERE AA.ID_DEVENGADO = R_ASIENTO.ID_DEVENGADO
             AND AA.VISIBLE = 'S';

          UPDATE EJECUCION.T_ASIENTOS_DET AD
             SET AD.ANULADO = 'S'
           WHERE AD.ID_ASIENTO IN
                 (SELECT A.ID_ASIENTO
                    FROM EJECUCION.T_ASIENTOS A
                   WHERE A.ID_DEVENGADO = R_ASIENTO.ID_DEVENGADO)
             AND AD.ANULADO = 'N';

          V_ID_DEVENGADO := R_ASIENTO.ID_DEVENGADO;
          V_PASO         := 0;
          FOR R_DEVENGADODET IN C_DEVENGADODET LOOP
            IF V_PASO = 0 THEN
              BEGIN
                --Se inserta el asiento cabecera
                EJECUCION.PKG_ASIENTOS_DEVENGADO.INSERTAR_ASIENTOS_CAB(R_DEVENGADODET.ID_DEVENGADO,
                                                                  V_ID_ASIENTO,
                                                                  V_MSGERR);
              EXCEPTION
                WHEN OTHERS THEN
                  RAISE_APPLICATION_ERROR(-20027,
                                          'FALLO SCRIPT INSERTAR_ASIENTOS_DET ' ||
                                          R_DEVENGADODET.ID_DEVENGADO ||
                                          ' -Error: ' || SQLERRM);
                  V_ERRLOG := EJECUCION.PKG_ASIENTOS_DEVENGADO.FNC_INSERTAR_T_LOG_PD('FALLO SCRIPT INSERTAR_ASIENTOS_CAB',
                                                                                R_DEVENGADODET.ID_DEVENGADO,
                                                                                'FALLO SCRIPT INSERTAR_ASIENTOS_CAB ' ||
                                                                                SQLERRM);
              END;
              V_PASO := V_PASO + 1;
            END IF;

            BEGIN
              EJECUCION.PKG_ASIENTOS_DEVENGADO.INSERTAR_ASIENTOS_DET(R_DEVENGADODET.ID_DEVENGADO,
                                                                R_DEVENGADODET.ID_DEVENGADODET,
                                                                R_DEVENGADODET.ID_OBJETO_GASTO,
                                                                R_DEVENGADODET.IMPORTE,
                                                                R_DEVENGADODET.CANTIDAD,
                                                                V_ID_ASIENTO,
                                                                V_MSGERR);
            EXCEPTION
              WHEN OTHERS THEN
                V_ERRLOG := EJECUCION.PKG_ASIENTOS_DEVENGADO.FNC_INSERTAR_T_LOG_PD('FALLO SCRIPT INSERTAR_ASIENTOS_DET',
                                                                              R_DEVENGADODET.ID_DEVENGADO,
                                                                              'FALLO SCRIPT INSERTAR_ASIENTOS_DET' ||
                                                                              SQLERRM);
                RAISE_APPLICATION_ERROR(-20027,
                                        'FALLO SCRIPT INSERTAR_ASIENTOS_DET ' ||
                                        R_DEVENGADODET.ID_DEVENGADO ||
                                        ' -Error: ' || SQLERRM);
            END;
            --SI LA CANTIDAD ES <200 FALLA PORQUE EL TIPO DE DATO ES MUY CHICO,
            --RECORDAR CAMBIAR EL TIPO DE DATOS ASI SACO ESTE IF
            IF V_CANTIDAD < 200 THEN
              V_DEVENGADO := V_DEVENGADO || ',' ||
                             TO_CHAR(R_DEVENGADODET.ID_DEVENGADO);
            END IF;
          END LOOP;
          COMMIT;
        END LOOP;

        IF V_CANTIDAD < 200 THEN
          OPEN P_T_DEVENGADO FOR
            SELECT V_DEVENGADO FROM DUAL;
        END IF;

        COMMIT;
      EXCEPTION
        WHEN OTHERS THEN
          RAISE_APPLICATION_ERROR(-20027,
                                  'FALLO SCRIPT ' || V_ID_DEVENGADO ||
                                  ' -Error: ' || SQLERRM);
    */
  END SCRIPT_INSERTAR_ASIENTO;

  PROCEDURE SCRIPT_BORRAR_ASIENTOS(P_CANTIDAD NUMBER DEFAULT 0) IS
    V_CANTIDAD NUMBER := P_CANTIDAD;

    /*CURSOR C_HIST_AS_DET IS
      SELECT *
        FROM EJECUCION.T_HIST_ASIENTOS_DET
       WHERE ID_DEVENGADODET IS NOT NULL
        AND TO_CHAR(FEC_CONTABLE,'YYYY')='2012';

    CURSOR C_HIST_AS IS
      SELECT *
        FROM EJECUCION.T_HIST_ASIENTOS
       WHERE ID_DEVENGADO IS NOT NULL
         AND ID_VIGENCIA = 166;*/

    /*CURSOR C_ASIENTOS_DET IS
      SELECT * FROM EJECUCION.T_ASIENTOS_DET WHERE ID_DEVENGADODET IS NOT NULL
      AND TO_CHAR(FEC_CONTABLE,'YYYY')='2012';

    CURSOR C_ASIENTOS IS
      SELECT * FROM EJECUCION.T_ASIENTOS WHERE ID_DEVENGADO IS NOT NULL
      AND ID_VIGENCIA = 166;*/
    --ALTER TRIGGER TG_T_ASIENTOS_DET_DEL DISABLE;
  BEGIN
    V_CANTIDAD := 0;
    /*
        --FOR R_HIST_AS_DET IN C_HIST_AS_DET LOOP
        DELETE FROM EJECUCION.T_HIST_ASIENTOS_DET HD
         WHERE ID_DEVENGADODET IS NOT NULL
           AND TO_CHAR(FEC_CONTABLE, 'YYYY') = '2012';
        --       WHERE HD.ID_ASIENTO_DET = R_HIST_AS_DET.ID_ASIENTO_DET;

        COMMIT;
        --END LOOP;

        -- FOR R_HIST_AS IN C_HIST_AS LOOP
        DELETE FROM EJECUCION.T_HIST_ASIENTOS HA
         WHERE ID_DEVENGADO IS NOT NULL
           AND ID_VIGENCIA = 166;
        --WHERE HA.ID_ASIENTO = R_HIST_AS.ID_ASIENTO;

        COMMIT;
        -- END LOOP;
    */
    /*FOR R_ASIENTOS_DET IN C_ASIENTOS_DET LOOP
      DELETE FROM EJECUCION.T_ASIENTOS_DET AD
       WHERE AD.ID_ASIENTO_DET = R_ASIENTOS_DET.ID_ASIENTO_DET;
      COMMIT;
    END LOOP;

    FOR R_ASIENTOS IN C_ASIENTOS LOOP
      DELETE FROM EJECUCION.T_ASIENTOS A
       WHERE A.ID_ASIENTO = R_ASIENTOS.ID_ASIENTO;
      COMMIT;
    END LOOP;*/
    --ALTER TRIGGER TG_T_ASIENTOS_DET_DEL ENABLE;
  END SCRIPT_BORRAR_ASIENTOS;
END PKG_ASIENTOS_DEVENGADO;
PACKAGE PKG_ASIENTOS_DEVENGADO IS

  -- Author  : Eugenia Peralta
  -- Created : 15/11/2011 12:31:08 p.m.
  -- Purpose : REGISTRAR LOS ASIENTOS CONTABLES DERIVADOS DEL DEVENGADO

  /*Objetivo:      Insertar automaticamente Asientos Contables de Partida Doble

    Observaciones: No debe impedirse la generacion del devengado, por lo tanto
                   los errores no deben mostrarse en pantalla sino que deben loguearse en
                   EJECUCION.T_LOG_PD para luego insertarlos manualmente.
  */
  TYPE TY_CURSOR IS REF CURSOR;

FUNCTION OBTENER_HABER_ASIENTO(p_ID_DEVENGADO IN NUMBER DEFAULT NULL)
                 RETURN NUMBER;

  PROCEDURE INSERTAR_ASIENTOS(p_ID_DEVENGADO IN NUMBER DEFAULT NULL,
                              P_ID_ASIENTO   OUT NUMBER,
                              P_MSG          IN OUT VARCHAR2);

  PROCEDURE INSERTAR_CONTAASIENTO(P_ID_ASIENTO IN NUMBER);

  PROCEDURE COPIAR_ASIENTOS_DET(P_ID_DEVENGADODET IN NUMBER,
                                P_ID_ASIENTO      IN NUMBER,
                                P_ID_ASIENTO_OLD  IN NUMBER,
                                P_MSGERR          OUT VARCHAR2);

  FUNCTION FNC_INSERTAR_T_LOG_PD(P_TABLA         IN EJECUCION.T_LOG_PD.TABLA%TYPE,
                                 P_IDENTIFICADOR IN EJECUCION.T_LOG_PD.IDENTIFICADOR%TYPE,
                                 P_COMENTARIO    IN EJECUCION.T_LOG_PD.COMENTARIO%TYPE)
    RETURN BOOLEAN;

  PROCEDURE INSERTAR_ASIENTOS_CAB(P_ID_DEVENGADO IN EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE,
                                  P_ID_ASIENTO   OUT NUMBER /*EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE*/,
                                  P_MSGERR       OUT VARCHAR2);

  PROCEDURE INSERTAR_ASIENTOS_DET(P_ID_DEVENGADO    IN EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE,
                                  P_ID_DEVENGADODET IN EJECUCION.T_DEVENGADO_DETALLE.ID_DEVENGADODET%TYPE,
                                  P_ID_OBJETO_GASTO IN PRESUPUESTO.T_OBJETOS_GASTO.ID_OBJETO_GASTO%TYPE,
                                  P_IMPORTE         IN EJECUCION.T_DEVENGADO_DETALLE.IMPORTE%TYPE,
                                  P_CANTIDAD        IN EJECUCION.T_DEVENGADO_DETALLE.CANTIDAD%TYPE,
                                  P_ID_ASIENTO      IN NUMBER /*EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE*/,
                                  P_MSGERR          OUT VARCHAR2);

  PROCEDURE INSERTAR_CONTRA_ASIENTOS(P_ID_DEVENGADO    IN EJECUCION.T_DEVENGADO.ID_DEVENGADO%TYPE DEFAULT NULL,
                                     P_ID_DEVENGADODET IN EJECUCION.T_DEVENGADO_DETALLE.ID_DEVENGADODET%TYPE DEFAULT NULL,
                                     P_ID_ASIENTO      OUT NUMBER /*EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE*/);

  PROCEDURE ANTES_GEN_ASIENTOS(P_FALLA_ASIENTO   OUT NUMBER,
                               P_GENERA_ASIENTO  OUT NUMBER,
                               P_ID_OBJETO_GASTO EJECUCION.T_DEVENGADO_DETALLE.ID_OBJETO_GASTO%TYPE,
                               P_ID_TIPODOC      EJECUCION.T_DEVENGADO.ID_TIPODOC%TYPE);

  PROCEDURE SCRIPT_INSERTAR_ASIENTO(P_T_DEVENGADO OUT TY_CURSOR,
                                    P_CANTIDAD    NUMBER);
  PROCEDURE SCRIPT_BORRAR_ASIENTOS(P_CANTIDAD NUMBER DEFAULT 0);
END PKG_ASIENTOS_DEVENGADO;
