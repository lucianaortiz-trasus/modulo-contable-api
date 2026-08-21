PACKAGE BODY "PKG_T_CUENTAS_CONTABLES" AS

    PROCEDURE INSERTAR(p_ID_CTA_CONTABLE     OUT EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE,
                       p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL,
                       p_CODIGO              IN EJECUCION.T_CUENTAS_CONTABLES.CODIGO%TYPE DEFAULT NULL,
                       p_NOMBRE              IN EJECUCION.T_CUENTAS_CONTABLES.NOMBRE%TYPE DEFAULT NULL,
                       p_ACTIVA              IN EJECUCION.T_CUENTAS_CONTABLES.ACTIVA%TYPE DEFAULT NULL,
                       p_TIPO_SALDO          IN EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE DEFAULT NULL,
                       p_NIVEL               IN EJECUCION.T_CUENTAS_CONTABLES.NIVEL%TYPE DEFAULT NULL,
                       p_USER_ING            IN EJECUCION.T_CUENTAS_CONTABLES.USER_ING%TYPE DEFAULT NULL,
                       p_FEC_ING             IN EJECUCION.T_CUENTAS_CONTABLES.FEC_ING%TYPE DEFAULT NULL,
                       p_USER_ACT            IN EJECUCION.T_CUENTAS_CONTABLES.USER_ACT%TYPE DEFAULT NULL,
                       p_FEC_ACT             IN EJECUCION.T_CUENTAS_CONTABLES.FEC_ACT%TYPE DEFAULT NULL,
                       p_ID_NATURALEZA       IN EJECUCION.T_CUENTAS_CONTABLES.ID_NATURALEZA%TYPE DEFAULT NULL,
                       p_ID_RECURSO_DEBE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_DEBE%TYPE DEFAULT NULL,
                       p_ID_RECURSO_HABER    IN EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_HABER%TYPE DEFAULT NULL,
                       p_IMPUTABLE           IN EJECUCION.T_CUENTAS_CONTABLES.IMPUTABLE%TYPE DEFAULT NULL,
                       p_AUXILIAR            IN EJECUCION.T_CUENTAS_CONTABLES.AUXILIAR%TYPE DEFAULT NULL,
                       p_ID_JURISDICCION     IN EJECUCION.T_CUENTAS_CONTABLES.ID_JURISDICCION%TYPE DEFAULT NULL,
                       p_ID_VIGENCIA         IN EJECUCION.T_CUENTAS_CONTABLES.ID_VIGENCIA%TYPE DEFAULT NULL,
                       p_ASIENTO_OPS         EJECUCION.T_CUENTAS_CONTABLES.ASIENTO_OPS%TYPE DEFAULT NULL,
                       p_ID_CTA_BANCARIA     EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_BANCARIA%TYPE DEFAULT NULL,
                       p_INVENTARIABLE       EJECUCION.T_CUENTAS_CONTABLES.INVENTARIABLE%TYPE DEFAULT NULL,
                       p_ID_PROVEEDOR        EJECUCION.T_CUENTAS_CONTABLES.ID_PROVEEDOR%TYPE DEFAULT NULL,
                       p_AMORTIZACION        EJECUCION.T_CUENTAS_CONTABLES.AMORTIZACION%TYPE DEFAULT NULL,
                       p_INGRESO_PROPIO      EJECUCION.T_CUENTAS_CONTABLES.INGRESO_PROPIO%TYPE DEFAULT NULL,
                       p_MSG                 IN OUT VARCHAR2) IS
        V_NIVEL            NUMBER;
        V_COUNT            NUMBER;
        V_IMPUTABLE        VARCHAR(1);
        v_codigo_existente number;

        v_ID_CTA_CONTABLE_REL EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE;
        v_CODIGO              EJECUCION.T_CUENTAS_CONTABLES.CODIGO%TYPE;
        v_NOMBRE              EJECUCION.T_CUENTAS_CONTABLES.NOMBRE%TYPE;
        v_ACTIVA              EJECUCION.T_CUENTAS_CONTABLES.ACTIVA%TYPE;
        v_TIPO_SALDO          EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE;
        v_USER_ING            EJECUCION.T_CUENTAS_CONTABLES.USER_ING%TYPE;
        v_FEC_ING             EJECUCION.T_CUENTAS_CONTABLES.FEC_ING%TYPE;
        v_USER_ACT            EJECUCION.T_CUENTAS_CONTABLES.USER_ACT%TYPE;
        v_FEC_ACT             EJECUCION.T_CUENTAS_CONTABLES.FEC_ACT%TYPE;
        v_ID_NATURALEZA       EJECUCION.T_CUENTAS_CONTABLES.ID_NATURALEZA%TYPE;
        v_ID_RECURSO_DEBE     EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_DEBE%TYPE;
        v_ID_RECURSO_HABER    EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_HABER%TYPE;
        v_AUXILIAR            EJECUCION.T_CUENTAS_CONTABLES.AUXILIAR%TYPE;
        v_ID_JURISDICCION     EJECUCION.T_CUENTAS_CONTABLES.ID_JURISDICCION%TYPE;
        v_ID_VIGENCIA         EJECUCION.T_CUENTAS_CONTABLES.ID_VIGENCIA%TYPE;
        v_ASIENTO_OPS         EJECUCION.T_CUENTAS_CONTABLES.ASIENTO_OPS%TYPE;
        v_ID_CTA_BANCARIA     EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_BANCARIA%TYPE;
        v_INVENTARIABLE       EJECUCION.T_CUENTAS_CONTABLES.INVENTARIABLE%TYPE;
        v_ID_PROVEEDOR        EJECUCION.T_CUENTAS_CONTABLES.ID_PROVEEDOR%TYPE;
        v_AMORTIZACION        EJECUCION.T_CUENTAS_CONTABLES.AMORTIZACION%TYPE;
        v_INGRESO_PROPIO      EJECUCION.T_CUENTAS_CONTABLES.INGRESO_PROPIO%TYPE;

    BEGIN

        v_ID_CTA_CONTABLE_REL := p_ID_CTA_CONTABLE_REL;
        v_CODIGO              := p_CODIGO;
        v_NOMBRE              := p_NOMBRE;
        v_ACTIVA              := p_ACTIVA;
        v_TIPO_SALDO          := p_TIPO_SALDO;
        v_USER_ING            := p_USER_ING;
        v_FEC_ING             := p_FEC_ING;
        v_USER_ACT            := p_USER_ACT;
        v_FEC_ACT             := p_FEC_ACT;
        v_ID_NATURALEZA       := p_ID_NATURALEZA;
        v_ID_RECURSO_DEBE     := p_ID_RECURSO_DEBE;
        v_ID_RECURSO_HABER    := p_ID_RECURSO_HABER;
        v_AUXILIAR            := p_AUXILIAR;
        v_ID_JURISDICCION     := p_ID_JURISDICCION;
        v_ID_VIGENCIA         := p_ID_VIGENCIA;
        v_ASIENTO_OPS         := p_ASIENTO_OPS;
        v_ID_CTA_BANCARIA     := p_ID_CTA_BANCARIA;
        v_INVENTARIABLE       := p_INVENTARIABLE;
        v_ID_PROVEEDOR        := p_ID_PROVEEDOR;
        v_AMORTIZACION        := p_AMORTIZACION;
        v_INGRESO_PROPIO      := p_INGRESO_PROPIO;
        v_codigo_existente    := 0;

        SELECT SQ_T_CUENTAS_CONTABLES.NEXTVAL
          INTO p_ID_CTA_CONTABLE
          FROM DUAL;

        SELECT COUNT(CC.NIVEL)
          INTO V_COUNT
          FROM T_CUENTAS_CONTABLES CC
         WHERE CC.NIVEL = p_NIVEL;

        IF V_COUNT > 0 THEN

            SELECT DISTINCT CC.NIVEL
              INTO V_NIVEL
              FROM T_CUENTAS_CONTABLES CC
             WHERE CC.NIVEL = p_NIVEL;

        ELSE
            V_NIVEL := p_NIVEL;

        END IF;

        select count(*)
          into v_codigo_existente
          from EJECUCION.t_cuentas_contables cc1
         where cc1.codigo = v_CODIGO;

        v_codigo := substr(v_codigo, 0, v_nivel * 2 + v_nivel - 1);

        if v_codigo_existente = 0 then

            INSERT INTO EJECUCION.T_CUENTAS_CONTABLES
                (ID_CTA_CONTABLE,
                 ID_CTA_CONTABLE_REL,
                 CODIGO,
                 NOMBRE,
                 ACTIVA,
                 TIPO_SALDO,
                 NIVEL,
                 USER_ING,
                 FEC_ING,
                 USER_ACT,
                 FEC_ACT,
                 ID_NATURALEZA,
                 ID_RECURSO_DEBE,
                 ID_RECURSO_HABER,
                 IMPUTABLE,
                 AUXILIAR,
                 ID_JURISDICCION,
                 ID_VIGENCIA,
                 ASIENTO_OPS,
                 ID_CTA_BANCARIA,
                 INVENTARIABLE,
                 ID_PROVEEDOR,
                 AMORTIZACION,
                 INGRESO_PROPIO)
            VALUES
                (p_ID_CTA_CONTABLE,
                 v_ID_CTA_CONTABLE_REL,
                 v_CODIGO,
                 v_NOMBRE,
                 v_ACTIVA,
                 v_TIPO_SALDO,
                 p_NIVEL,
                 v_USER_ING,
                 v_FEC_ING,
                 v_USER_ACT,
                 v_FEC_ACT,
                 v_ID_NATURALEZA,
                 v_ID_RECURSO_DEBE,
                 v_ID_RECURSO_HABER,
                 p_IMPUTABLE,
                 v_AUXILIAR,
                 v_ID_JURISDICCION,
                 v_ID_VIGENCIA,
                 v_ASIENTO_OPS,
                 v_ID_CTA_BANCARIA,
                 v_INVENTARIABLE,
                 v_ID_PROVEEDOR,
                 v_AMORTIZACION,
                 v_INGRESO_PROPIO);
        else
            p_MSG := 'El codigo ya existe.';
        end if;

    EXCEPTION
        WHEN v_ERROR_00001 THEN
            p_MSG := 'El codigo ya existe.';
            PKG_UTIL.BASE_ROLLBACK;
        WHEN v_ERROR_01407 THEN
            p_MSG := 'No se puede insertar valores nulos.';
            PKG_UTIL.BASE_ROLLBACK;
        WHEN OTHERS THEN
            p_MSG := 'Error al INSERTAR. Verifique: ' || SQLERRM;
            PKG_UTIL.BASE_ROLLBACK;

    END INSERTAR;

    PROCEDURE EDITAR(p_ID_CTA_CONTABLE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                     p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL,
                     p_CODIGO              IN EJECUCION.T_CUENTAS_CONTABLES.CODIGO%TYPE DEFAULT NULL,
                     p_NOMBRE              IN EJECUCION.T_CUENTAS_CONTABLES.NOMBRE%TYPE DEFAULT NULL,
                     p_ACTIVA              IN EJECUCION.T_CUENTAS_CONTABLES.ACTIVA%TYPE DEFAULT NULL,
                     p_TIPO_SALDO          IN EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE DEFAULT NULL,
                     p_NIVEL               IN EJECUCION.T_CUENTAS_CONTABLES.NIVEL%TYPE DEFAULT NULL,
                     p_USER_ING            IN EJECUCION.T_CUENTAS_CONTABLES.USER_ING%TYPE DEFAULT NULL,
                     p_FEC_ING             IN EJECUCION.T_CUENTAS_CONTABLES.FEC_ING%TYPE DEFAULT NULL,
                     p_USER_ACT            IN EJECUCION.T_CUENTAS_CONTABLES.USER_ACT%TYPE DEFAULT NULL,
                     p_FEC_ACT             IN EJECUCION.T_CUENTAS_CONTABLES.FEC_ACT%TYPE DEFAULT NULL,
                     p_ID_NATURALEZA       IN EJECUCION.T_CUENTAS_CONTABLES.ID_NATURALEZA%TYPE DEFAULT NULL,
                     p_ID_RECURSO_DEBE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_DEBE%TYPE DEFAULT NULL,
                     p_ID_RECURSO_HABER    IN EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_HABER%TYPE DEFAULT NULL,
                     p_IMPUTABLE           IN EJECUCION.T_CUENTAS_CONTABLES.IMPUTABLE%TYPE DEFAULT NULL,
                     p_AUXILIAR            IN EJECUCION.T_CUENTAS_CONTABLES.AUXILIAR%TYPE DEFAULT NULL,
                     p_ASIENTO_OPS         EJECUCION.T_CUENTAS_CONTABLES.ASIENTO_OPS%TYPE DEFAULT NULL,
                     p_ID_CTA_BANCARIA     EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_BANCARIA%TYPE DEFAULT NULL,
                     p_INVENTARIABLE       EJECUCION.T_CUENTAS_CONTABLES.INVENTARIABLE%TYPE DEFAULT NULL,
                     p_ID_PROVEEDOR        EJECUCION.T_CUENTAS_CONTABLES.ID_PROVEEDOR%TYPE DEFAULT NULL,
                     p_AMORTIZACION        EJECUCION.T_CUENTAS_CONTABLES.AMORTIZACION%TYPE DEFAULT NULL,
                     p_INGRESO_PROPIO      EJECUCION.T_CUENTAS_CONTABLES.INGRESO_PROPIO%TYPE DEFAULT NULL,
                     p_MSG                 IN OUT varchar2) IS
        v_NUM_NIVEL       NUMBER;
        p_NIVEL1          VARCHAR2(10);
        p_NIVEL2          VARCHAR2(10);
        p_NIVEL3          VARCHAR2(10);
        p_NIVEL4          VARCHAR2(10);
        p_NIVEL5          VARCHAR2(10);
        p_NIVEL6          VARCHAR2(10);
        p_NIVEL7          VARCHAR2(10);
        v_CODIGO          VARCHAR2(50);
        v_ID_CTA_CONTABLE NUMBER;

        v_ID_CTA_CONTABLE_REL EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE;
        v_NOMBRE              EJECUCION.T_CUENTAS_CONTABLES.NOMBRE%TYPE;
        v_ACTIVA              EJECUCION.T_CUENTAS_CONTABLES.ACTIVA%TYPE;
        v_TIPO_SALDO          EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE;
        v_USER_ING            EJECUCION.T_CUENTAS_CONTABLES.USER_ING%TYPE;
        v_FEC_ING             EJECUCION.T_CUENTAS_CONTABLES.FEC_ING%TYPE;
        v_USER_ACT            EJECUCION.T_CUENTAS_CONTABLES.USER_ACT%TYPE;
        v_FEC_ACT             EJECUCION.T_CUENTAS_CONTABLES.FEC_ACT%TYPE;
        v_ID_NATURALEZA       EJECUCION.T_CUENTAS_CONTABLES.ID_NATURALEZA%TYPE;
        v_ID_RECURSO_DEBE     EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_DEBE%TYPE;
        v_ID_RECURSO_HABER    EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_HABER%TYPE;
        v_AUXILIAR            EJECUCION.T_CUENTAS_CONTABLES.AUXILIAR%TYPE;
        v_IMPUTABLE           EJECUCION.T_CUENTAS_CONTABLES.IMPUTABLE%TYPE;
        v_NIVEL               EJECUCION.T_CUENTAS_CONTABLES.NIVEL%TYPE;
        v_ASIENTO_OPS         EJECUCION.T_CUENTAS_CONTABLES.ASIENTO_OPS%TYPE;
        v_ID_CTA_BANCARIA     EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_BANCARIA%TYPE;
        v_INVENTARIABLE       EJECUCION.T_CUENTAS_CONTABLES.INVENTARIABLE%TYPE;
        v_ID_PROVEEDOR        EJECUCION.T_CUENTAS_CONTABLES.ID_PROVEEDOR%TYPE;
        v_AMORTIZACION        EJECUCION.T_CUENTAS_CONTABLES.AMORTIZACION%TYPE;
        v_INGRESO_PROPIO      EJECUCION.T_CUENTAS_CONTABLES.INGRESO_PROPIO%TYPE;

        CURSOR p_T_CUENTAS_CONTABLES IS
            SELECT CC.ID_CTA_CONTABLE,
                   CC.ID_CTA_CONTABLE_REL,
                   DECODE(LENGTH(CC.CODIGO),
                          15,
                          SUBSTR(CC.CODIGO, 1, 1),
                          16,
                          SUBSTR(CC.CODIGO, 1, 2)) NIVEL1,
                   DECODE(LENGTH(CC.CODIGO),
                          15,
                          SUBSTR(CC.CODIGO, 2, 2),
                          16,
                          SUBSTR(CC.CODIGO, 3, 2)) NIVEL2,
                   DECODE(LENGTH(CC.CODIGO),
                          15,
                          SUBSTR(CC.CODIGO, 4, 2),
                          16,
                          SUBSTR(CC.CODIGO, 5, 2)) NIVEL3,
                   DECODE(LENGTH(CC.CODIGO),
                          15,
                          SUBSTR(CC.CODIGO, 6, 2),
                          16,
                          SUBSTR(CC.CODIGO, 7, 2)) NIVEL4,
                   DECODE(LENGTH(CC.CODIGO),
                          15,
                          SUBSTR(CC.CODIGO, 8, 2),
                          16,
                          SUBSTR(CC.CODIGO, 9, 2)) NIVEL5,
                   DECODE(LENGTH(CC.CODIGO),
                          15,
                          SUBSTR(CC.CODIGO, 10, 2),
                          16,
                          SUBSTR(CC.CODIGO, 11, 2)) NIVEL6,
                   DECODE(LENGTH(CC.CODIGO),
                          15,
                          SUBSTR(CC.CODIGO, 12, 2),
                          16,
                          SUBSTR(CC.CODIGO, 13, 4)) NIVEL7
              FROM EJECUCION.T_CUENTAS_CONTABLES CC
             START WITH CC.ID_CTA_CONTABLE = p_ID_CTA_CONTABLE
            CONNECT BY PRIOR CC.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE_REL;

    BEGIN
        v_ID_CTA_CONTABLE_REL := p_ID_CTA_CONTABLE_REL;
        v_NOMBRE              := p_NOMBRE;
        v_ACTIVA              := p_ACTIVA;
        v_TIPO_SALDO          := p_TIPO_SALDO;
        v_USER_ING            := p_USER_ING;
        v_FEC_ING             := p_FEC_ING;
        v_USER_ACT            := p_USER_ACT;
        v_FEC_ACT             := p_FEC_ACT;
        v_ID_NATURALEZA       := p_ID_NATURALEZA;
        v_ID_RECURSO_DEBE     := p_ID_RECURSO_DEBE;
        v_ID_RECURSO_HABER    := p_ID_RECURSO_HABER;
        v_AUXILIAR            := p_AUXILIAR;
        v_IMPUTABLE           := p_IMPUTABLE;
        v_NIVEL               := p_NIVEL;
        v_ASIENTO_OPS         := p_ASIENTO_OPS;
        v_ID_CTA_BANCARIA     := p_ID_CTA_BANCARIA;
        v_INVENTARIABLE       := p_INVENTARIABLE;
        v_ID_PROVEEDOR        := p_ID_PROVEEDOR;
        v_AMORTIZACION        := p_AMORTIZACION;
        v_INGRESO_PROPIO      := p_INGRESO_PROPIO;
  /*
        SELECT DISTINCT CC.NIVEL
          INTO v_NUM_NIVEL
          FROM EJECUCION.T_CUENTAS_CONTABLES CC
         WHERE CC.ID_CTA_CONTABLE = p_ID_CTA_CONTABLE;

        SELECT DECODE(LENGTH(p_CODIGO),
                      15,
                      SUBSTR(p_CODIGO, 1, 1),
                      16,
                      SUBSTR(p_CODIGO, 1, 2)) NIVEL1,
               DECODE(LENGTH(p_CODIGO),
                      15,
                      SUBSTR(p_CODIGO, 2, 2),
                      16,
                      SUBSTR(p_CODIGO, 3, 2)) NIVEL2,
               DECODE(LENGTH(p_CODIGO),
                      15,
                      SUBSTR(p_CODIGO, 4, 2),
                      16,
                      SUBSTR(p_CODIGO, 5, 2)) NIVEL3,
               DECODE(LENGTH(p_CODIGO),
                      15,
                      SUBSTR(p_CODIGO, 6, 2),
                      16,
                      SUBSTR(p_CODIGO, 7, 2)) NIVEL4,
               DECODE(LENGTH(p_CODIGO),
                      15,
                      SUBSTR(p_CODIGO, 8, 2),
                      16,
                      SUBSTR(p_CODIGO, 9, 2)) NIVEL5,
               DECODE(LENGTH(p_CODIGO),
                      15,
                      SUBSTR(p_CODIGO, 10, 2),
                      16,
                      SUBSTR(p_CODIGO, 11, 2)) NIVEL6,
               DECODE(LENGTH(p_CODIGO),
                      15,
                      SUBSTR(p_CODIGO, 12, 2),
                      16,
                      SUBSTR(p_CODIGO, 13, 4)) NIVEL7
          INTO p_NIVEL1,
               p_NIVEL2,
               p_NIVEL3,
               p_NIVEL4,
               p_NIVEL5,
               p_NIVEL6,
               p_NIVEL7
          FROM DUAL;

        SELECT CC.CODIGO
          INTO v_CODIGO
          FROM EJECUCION.T_CUENTAS_CONTABLES CC
         WHERE CC.ID_CTA_CONTABLE = p_ID_CTA_CONTABLE;*/
 /*
        if p_CODIGO <> v_CODIGO then
            IF v_NUM_NIVEL = 1 THEN

                FOR R_CODIGOS in p_T_CUENTAS_CONTABLES LOOP
                    UPDATE EJECUCION.T_CUENTAS_CONTABLES
                       SET CODIGO   = p_NIVEL1 || R_CODIGOS.NIVEL2 ||
                                      R_CODIGOS.NIVEL3 || R_CODIGOS.NIVEL4 ||
                                      R_CODIGOS.NIVEL5 || R_CODIGOS.NIVEL6 ||
                                      R_CODIGOS.NIVEL7,
                           USER_ACT = user,
                           FEC_ACT  = sysdate
                     WHERE ID_CTA_CONTABLE = R_CODIGOS.ID_CTA_CONTABLE;

                    IF R_CODIGOS.ID_CTA_CONTABLE_REL IS NOT NULL THEN
                        v_ID_CTA_CONTABLE := R_CODIGOS.ID_CTA_CONTABLE;
                    END IF;

                END LOOP;

            END IF;

            IF v_NUM_NIVEL = 2 THEN

                FOR R_CODIGOS in p_T_CUENTAS_CONTABLES LOOP
                    UPDATE EJECUCION.T_CUENTAS_CONTABLES
                       SET CODIGO   = R_CODIGOS.NIVEL1 || p_NIVEL2 ||
                                      R_CODIGOS.NIVEL3 || R_CODIGOS.NIVEL4 ||
                                      R_CODIGOS.NIVEL5 || R_CODIGOS.NIVEL6 ||
                                      R_CODIGOS.NIVEL7,
                           USER_ACT = user,
                           FEC_ACT  = sysdate
                     WHERE ID_CTA_CONTABLE = R_CODIGOS.ID_CTA_CONTABLE;

                END LOOP;

            END IF;
            IF v_NUM_NIVEL = 3 THEN

                FOR R_CODIGOS in p_T_CUENTAS_CONTABLES LOOP
                    UPDATE EJECUCION.T_CUENTAS_CONTABLES
                       SET CODIGO   = R_CODIGOS.NIVEL1 || R_CODIGOS.NIVEL2 ||
                                      p_NIVEL3 || R_CODIGOS.NIVEL4 ||
                                      R_CODIGOS.NIVEL5 || R_CODIGOS.NIVEL6 ||
                                      R_CODIGOS.NIVEL7,
                           USER_ACT = user,
                           FEC_ACT  = sysdate
                     WHERE ID_CTA_CONTABLE = R_CODIGOS.ID_CTA_CONTABLE;

                END LOOP;

            END IF;

            IF v_NUM_NIVEL = 4 THEN
                FOR R_CODIGOS in p_T_CUENTAS_CONTABLES LOOP
                    UPDATE EJECUCION.T_CUENTAS_CONTABLES
                       SET CODIGO   = R_CODIGOS.NIVEL1 || R_CODIGOS.NIVEL2 ||
                                      R_CODIGOS.NIVEL3 || p_NIVEL4 ||
                                      R_CODIGOS.NIVEL5 || R_CODIGOS.NIVEL6 ||
                                      R_CODIGOS.NIVEL7,
                           USER_ACT = user,
                           FEC_ACT  = sysdate
                     WHERE ID_CTA_CONTABLE = R_CODIGOS.ID_CTA_CONTABLE;

                END LOOP;

            END IF;

            IF v_NUM_NIVEL = 5 THEN
                FOR R_CODIGOS in p_T_CUENTAS_CONTABLES LOOP
                    UPDATE EJECUCION.T_CUENTAS_CONTABLES
                       SET CODIGO   = R_CODIGOS.NIVEL1 || R_CODIGOS.NIVEL2 ||
                                      R_CODIGOS.NIVEL3 || R_CODIGOS.NIVEL4 ||
                                      p_NIVEL5 || R_CODIGOS.NIVEL6 ||
                                      R_CODIGOS.NIVEL7,
                           USER_ACT = user,
                           FEC_ACT  = sysdate
                     WHERE ID_CTA_CONTABLE = R_CODIGOS.ID_CTA_CONTABLE;

                END LOOP;

            END IF;

            IF v_NUM_NIVEL = 6 THEN
                FOR R_CODIGOS in p_T_CUENTAS_CONTABLES LOOP
                    UPDATE EJECUCION.T_CUENTAS_CONTABLES
                       SET CODIGO   = R_CODIGOS.NIVEL1 || R_CODIGOS.NIVEL2 ||
                                      R_CODIGOS.NIVEL3 || R_CODIGOS.NIVEL4 ||
                                      R_CODIGOS.NIVEL5 || p_NIVEL6 ||
                                      R_CODIGOS.NIVEL7,
                           USER_ACT = user,
                           FEC_ACT  = sysdate
                     WHERE ID_CTA_CONTABLE = R_CODIGOS.ID_CTA_CONTABLE;

                END LOOP;

            END IF;

            IF v_NUM_NIVEL = 7 THEN
                FOR R_CODIGOS in p_T_CUENTAS_CONTABLES LOOP
                    UPDATE EJECUCION.T_CUENTAS_CONTABLES
                       SET CODIGO   = R_CODIGOS.NIVEL1 || R_CODIGOS.NIVEL2 ||
                                      R_CODIGOS.NIVEL3 || R_CODIGOS.NIVEL4 ||
                                      R_CODIGOS.NIVEL5 || R_CODIGOS.NIVEL6 ||
                                      p_NIVEL7,
                           USER_ACT = user,
                           FEC_ACT  = sysdate
                     WHERE ID_CTA_CONTABLE = R_CODIGOS.ID_CTA_CONTABLE;

                END LOOP;

            END IF;

        end if;
 */
        UPDATE EJECUCION.T_CUENTAS_CONTABLES
           SET ID_CTA_CONTABLE_REL = v_ID_CTA_CONTABLE_REL,
               NOMBRE              = INITCAP(LOWER(v_NOMBRE)),
               ACTIVA              = v_ACTIVA,
               TIPO_SALDO          = v_TIPO_SALDO,
               NIVEL               = v_NIVEL,
               USER_ING            = v_USER_ING,
               FEC_ING             = v_FEC_ING,
               USER_ACT            = v_USER_ACT,
               FEC_ACT             = v_FEC_ACT,
               ID_NATURALEZA       = v_ID_NATURALEZA,
               ID_RECURSO_DEBE     = v_ID_RECURSO_DEBE,
               ID_RECURSO_HABER    = v_ID_RECURSO_HABER,
               AUXILIAR            = v_AUXILIAR,
               IMPUTABLE           = v_IMPUTABLE,
               ASIENTO_OPS         = v_ASIENTO_OPS,
               ID_CTA_BANCARIA     = v_ID_CTA_BANCARIA,
               INVENTARIABLE       = v_INVENTARIABLE,
               ID_PROVEEDOR        = v_ID_PROVEEDOR,
               AMORTIZACION        = v_AMORTIZACION,
               INGRESO_PROPIO      = v_INGRESO_PROPIO

         WHERE ID_CTA_CONTABLE = p_ID_CTA_CONTABLE;

    exception
        when v_error_00001 then
            p_MSG := 'Numero duplicado para esa vigencia.';
            pkg_util.base_rollback;
        when v_error_01407 then
            p_MSG := 'No se puede insertar valores nulos.';
            pkg_util.base_rollback;
        when others then
            p_MSG := 'Error al EDITAR. Verifique: ' || sqlerrm;
            pkg_util.base_rollback;

    END EDITAR;

    PROCEDURE CAMBIARCUENTABANCARIA(p_ID_CTA_CONTABLE IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                                    p_ID_CTA_BANCARIA EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_BANCARIA%TYPE DEFAULT NULL,
                                    p_MSG             IN OUT varchar2) IS
        v_ID_CTA_CONTABLE NUMBER;
        v_ID_CTA_BANCARIA EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_BANCARIA%TYPE;

    BEGIN
        v_ID_CTA_CONTABLE := p_ID_CTA_CONTABLE;
        v_ID_CTA_BANCARIA := p_ID_CTA_BANCARIA;

        UPDATE EJECUCION.T_CUENTAS_CONTABLES
           SET ID_CTA_BANCARIA = v_ID_CTA_BANCARIA
         WHERE ID_CTA_CONTABLE = p_ID_CTA_CONTABLE;

    exception
        when v_error_00001 then
            p_MSG := 'Numero duplicado para esa vigencia.';
            pkg_util.base_rollback;
        when v_error_01407 then
            p_MSG := 'No se puede insertar valores nulos.';
            pkg_util.base_rollback;
        when others then
            p_MSG := 'Error al EDITAR. Verifique: ' || sqlerrm;
            pkg_util.base_rollback;

    END CAMBIARCUENTABANCARIA;

    PROCEDURE ELIMINAR(p_ID_CTA_CONTABLE IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                       p_MSG             IN OUT varchar2) IS
        v_ID_CTA_CONTABLE EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE;

        CURSOR p_T_CUENTAS_CONTABLES IS
            SELECT CC.ID_CTA_CONTABLE, CC.ID_CTA_CONTABLE_REL, LEVEL
              FROM EJECUCION.T_CUENTAS_CONTABLES CC
             START WITH CC.ID_CTA_CONTABLE = v_ID_CTA_CONTABLE
            CONNECT BY PRIOR CC.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE_REL
             ORDER BY LEVEL DESC;

    BEGIN
        v_ID_CTA_CONTABLE := p_ID_CTA_CONTABLE;

        FOR r_T_CTA_CC in p_T_CUENTAS_CONTABLES LOOP

            DELETE FROM EJECUCION.T_CUENTAS_CONTABLES CC
             WHERE CC.ID_CTA_CONTABLE = r_T_CTA_CC.ID_CTA_CONTABLE;

        END LOOP;

    exception
        when v_error_2292 then
            p_MSG := 'No se puede Eliminar el registro existen relaciones para ese ejercicio asociadas a otras tablas.';
            pkg_util.base_rollback;
        when others then
            p_MSG := 'Error al ELIMINAR. Verifique: ' || sqlerrm;
            pkg_util.base_rollback;

    END ELIMINAR;

    PROCEDURE BUSCARPORPK(p_T_CUENTAS_CONTABLES OUT ty_CURSOR,
                          p_ID_CTA_CONTABLE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL) IS

        v_ID_CTA_CONTABLE EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE;

    BEGIN
        v_ID_CTA_CONTABLE := p_ID_CTA_CONTABLE;

        OPEN p_T_CUENTAS_CONTABLES FOR
            SELECT CC.ID_CTA_CONTABLE,
                   CC.ID_CTA_CONTABLE_REL,
                   CC.CODIGO,
                   CC.NOMBRE,
                   CC.ACTIVA,
                   CC.TIPO_SALDO,
                   CC.NIVEL,
                   CC.USER_ING,
                   CC.FEC_ING,
                   CC.USER_ACT,
                   CC.FEC_ACT,
                   CC.ID_NATURALEZA,
                   CC.ID_RECURSO_DEBE,
                   CC.ID_RECURSO_HABER,
                   CC.IMPUTABLE,
                   CC.AUXILIAR,
                   CC.CLASIFICADOR_RELACIONADO,
                   CC.ASIENTO_OPS,
                   CC.ID_CTA_BANCARIA,
                   CC.INVENTARIABLE,
                   CC.ID_PROVEEDOR,
                   CC.AMORTIZACION,
                   CC.INGRESO_PROPIO,
                   P.RAZON                     AS nombre_proveedor,
                   CB.N_CUENTA_BANCARIA        AS nombre_cta_bancaria

              FROM EJECUCION.T_CUENTAS_CONTABLES CC
              LEFT JOIN CAJA.T_PROVEEDORES P
                ON (CC.ID_PROVEEDOR = P.ID_PROVEEDOR)
              LEFT JOIN CAJA.T_CUENTAS_BANCARIA CB
                ON (CC.ID_CTA_BANCARIA = CB.ID_CUENTA_BANCARIA)
             WHERE (CC.ID_CTA_CONTABLE = v_ID_CTA_CONTABLE);
    END BUSCARPORPK;

    PROCEDURE BUSCARPORCUENTABANCARIA(p_T_CUENTAS_CONTABLES OUT ty_CURSOR,
                                      p_ID_CUENTA_BANCARIA  IN CAJA.T_CUENTAS_BANCARIA.ID_CUENTA_BANCARIA%TYPE DEFAULT NULL) IS

        v_ID_CUENTA_BANCARIA CAJA.T_CUENTAS_BANCARIA.ID_CUENTA_BANCARIA%TYPE;

    BEGIN
        v_ID_CUENTA_BANCARIA := p_ID_CUENTA_BANCARIA;

        OPEN p_T_CUENTAS_CONTABLES FOR
            SELECT CC.ID_CTA_CONTABLE,
                   CC.ID_CTA_CONTABLE_REL,
                   CC.CODIGO,
                   CC.NOMBRE,
                   CC.ACTIVA,
                   CC.TIPO_SALDO,
                   CC.NIVEL,
                   CC.USER_ING,
                   CC.FEC_ING,
                   CC.USER_ACT,
                   CC.FEC_ACT,
                   CC.ID_NATURALEZA,
                   CC.ID_RECURSO_DEBE,
                   CC.ID_RECURSO_HABER,
                   CC.IMPUTABLE,
                   CC.AUXILIAR,
                   CC.CLASIFICADOR_RELACIONADO,
                   CC.ASIENTO_OPS,
                   CC.ID_CTA_BANCARIA,
                   CC.INVENTARIABLE,
                   CC.ID_PROVEEDOR,
                   CC.AMORTIZACION,
                   CC.INGRESO_PROPIO,
                   P.RAZON                     AS nombre_proveedor,
                   CB.N_CUENTA_BANCARIA        AS nombre_cta_bancaria

              FROM EJECUCION.T_CUENTAS_CONTABLES CC
              LEFT JOIN CAJA.T_PROVEEDORES P
                ON (CC.ID_PROVEEDOR = P.ID_PROVEEDOR)
              LEFT JOIN CAJA.T_CUENTAS_BANCARIA CB
                ON (CC.ID_CTA_BANCARIA = CB.ID_CUENTA_BANCARIA)
             WHERE (CC.Id_Cta_Bancaria = v_ID_CUENTA_BANCARIA);
    END BUSCARPORCUENTABANCARIA;

    PROCEDURE BUSCAR(p_T_CUENTAS_CONTABLES OUT ty_CURSOR,
                     p_ID_CTA_CONTABLE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                     p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL,
                     p_CODIGO              IN EJECUCION.T_CUENTAS_CONTABLES.CODIGO%TYPE DEFAULT NULL,
                     p_NOMBRE              IN EJECUCION.T_CUENTAS_CONTABLES.NOMBRE%TYPE DEFAULT NULL,
                     p_ACTIVA              IN EJECUCION.T_CUENTAS_CONTABLES.ACTIVA%TYPE DEFAULT NULL,
                     p_TIPO_SALDO          IN EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE DEFAULT NULL,
                     p_NIVEL               IN EJECUCION.T_CUENTAS_CONTABLES.NIVEL%TYPE DEFAULT NULL,
                     p_USER_ING            IN EJECUCION.T_CUENTAS_CONTABLES.USER_ING%TYPE DEFAULT NULL,
                     p_FEC_ING             IN EJECUCION.T_CUENTAS_CONTABLES.FEC_ING%TYPE DEFAULT NULL,
                     p_USER_ACT            IN EJECUCION.T_CUENTAS_CONTABLES.USER_ACT%TYPE DEFAULT NULL,
                     p_FEC_ACT             IN EJECUCION.T_CUENTAS_CONTABLES.FEC_ACT%TYPE DEFAULT NULL,
                     p_ID_NATURALEZA       IN EJECUCION.T_CUENTAS_CONTABLES.ID_NATURALEZA%TYPE DEFAULT NULL,
                     p_ID_RECURSO_DEBE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_DEBE%TYPE DEFAULT NULL,
                     p_ID_RECURSO_HABER    IN EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_HABER%TYPE DEFAULT NULL,
                     p_IMPUTABLE           IN EJECUCION.T_CUENTAS_CONTABLES.IMPUTABLE%TYPE DEFAULT NULL,
                     p_AUXILIAR            IN EJECUCION.T_CUENTAS_CONTABLES.AUXILIAR%TYPE DEFAULT NULL,
                     p_PAGINA              IN NUMBER DEFAULT NULL,
                     p_TAMANOPAGINA        IN NUMBER DEFAULT NULL,
                     p_CANTIDADDEREGISTROS OUT NUMBER

                     ) IS
        v_ID_CTA_CONTABLE     EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE;
        v_ID_CTA_CONTABLE_REL EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE;
        v_CODIGO              EJECUCION.T_CUENTAS_CONTABLES.CODIGO%TYPE;
        v_NOMBRE              EJECUCION.T_CUENTAS_CONTABLES.NOMBRE%TYPE;
        v_ACTIVA              EJECUCION.T_CUENTAS_CONTABLES.ACTIVA%TYPE;
        v_TIPO_SALDO          EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE;
        v_USER_ING            EJECUCION.T_CUENTAS_CONTABLES.USER_ING%TYPE;
        v_FEC_ING             EJECUCION.T_CUENTAS_CONTABLES.FEC_ING%TYPE;
        v_USER_ACT            EJECUCION.T_CUENTAS_CONTABLES.USER_ACT%TYPE;
        v_FEC_ACT             EJECUCION.T_CUENTAS_CONTABLES.FEC_ACT%TYPE;
        v_ID_NATURALEZA       EJECUCION.T_CUENTAS_CONTABLES.ID_NATURALEZA%TYPE;
        v_ID_RECURSO_DEBE     EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_DEBE%TYPE;
        v_ID_RECURSO_HABER    EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_HABER%TYPE;
        v_AUXILIAR            EJECUCION.T_CUENTAS_CONTABLES.AUXILIAR%TYPE;
        v_IMPUTABLE           EJECUCION.T_CUENTAS_CONTABLES.IMPUTABLE%TYPE;
        v_NIVEL               EJECUCION.T_CUENTAS_CONTABLES.NIVEL%TYPE;
        v_PAGINA              NUMBER;
        v_TAMANOPAGINA        NUMBER;

        v_INICIO NUMBER;
        v_FIN    NUMBER;

        v_SELECT        varchar2(2000);
        v_SELECT_PAGINA varchar2(2000);
        v_SELECT_COUNT  varchar2(2000);

        v_CANTIDAD varchar2(2000);

        v_FROM_COUNT varchar2(1000);

        v_WHERE_PAGINA varchar2(1000);
        v_WHERE_COUNT  varchar2(1000);

        v_SUB_CONSULTA VARCHAR2(30000);
        v_BUSCAR       VARCHAR2(30000);
        v_AND          VARCHAR2(5);
    BEGIN
        v_ID_CTA_CONTABLE     := p_ID_CTA_CONTABLE;
        v_ID_CTA_CONTABLE_REL := p_ID_CTA_CONTABLE_REL;
        v_CODIGO              := p_CODIGO;
        v_NOMBRE              := p_NOMBRE;
        v_ACTIVA              := p_ACTIVA;
        v_TIPO_SALDO          := p_TIPO_SALDO;
        v_USER_ING            := p_USER_ING;
        v_FEC_ING             := p_FEC_ING;
        v_USER_ACT            := p_USER_ACT;
        v_FEC_ACT             := p_FEC_ACT;
        v_ID_NATURALEZA       := p_ID_NATURALEZA;
        v_ID_RECURSO_DEBE     := p_ID_RECURSO_DEBE;
        v_ID_RECURSO_HABER    := p_ID_RECURSO_HABER;
        v_AUXILIAR            := p_AUXILIAR;
        v_IMPUTABLE           := p_IMPUTABLE;
        v_NIVEL               := p_NIVEL;
        v_PAGINA              := p_PAGINA;
        v_TAMANOPAGINA        := p_TAMANOPAGINA;

        BEGIN
            v_SELECT_COUNT := 'SELECT COUNT(*) ';
            v_FROM_COUNT   := 'FROM EJECUCION.T_CUENTAS_CONTABLES ';
            v_WHERE_COUNT  := ' ';
            v_AND          := 'WHERE';

            IF v_ID_CTA_CONTABLE IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE = ' ||
                                 v_ID_CTA_CONTABLE;
                v_AND         := ' AND';
            END IF;

            IF v_ID_CTA_CONTABLE_REL IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL = ' ||
                                 v_ID_CTA_CONTABLE_REL;
                v_AND         := ' AND';
            END IF;

            IF v_CODIGO IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.CODIGO LIKE ''%' ||
                                 v_CODIGO || '%''';
                v_AND         := ' AND';
            END IF;

            IF v_NOMBRE IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.NOMBRE LIKE ''%' ||
                                 v_NOMBRE || '%''';
                v_AND         := ' AND';
            END IF;

            IF v_ACTIVA IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.ACTIVA LIKE ''%' ||
                                 v_ACTIVA || '%''';
                v_AND         := ' AND';
            END IF;

            IF v_TIPO_SALDO IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO LIKE ''%' ||
                                 v_TIPO_SALDO || '%''';
                v_AND         := ' AND';
            END IF;

            IF v_NIVEL IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.NIVEL = ' ||
                                 v_NIVEL;
                v_AND         := ' AND';
            END IF;

            IF v_USER_ING IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.USER_ING LIKE ''%' ||
                                 v_USER_ING || '%''';
                v_AND         := ' AND';
            END IF;

            IF TO_DATE(v_FEC_ING, 'DD/MM/YYYY') IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.FEC_ING = ''' ||
                                 v_FEC_ING || '''';
                v_AND         := ' AND';
            END IF;

            IF v_USER_ACT IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.USER_ACT LIKE ''%' ||
                                 v_USER_ACT || '%''';
                v_AND         := ' AND';
            END IF;

            IF TO_DATE(v_FEC_ACT, 'DD/MM/YYYY') IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.FEC_ACT = ''' ||
                                 v_FEC_ACT || '''';
                v_AND         := ' AND';
            END IF;

            IF v_ID_NATURALEZA IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.ID_NATURALEZA = ' ||
                                 v_ID_NATURALEZA;
                v_AND         := ' AND';
            END IF;

            IF v_ID_RECURSO_DEBE IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_DEBE = ' ||
                                 v_ID_RECURSO_DEBE;
                v_AND         := ' AND';
            END IF;

            IF v_ID_RECURSO_HABER IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_HABER = ' ||
                                 v_ID_RECURSO_HABER;
            END IF;

            v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                             '  (ID_CTA_CONTABLE >= 11353) ';
            v_CANTIDAD    := v_SELECT_COUNT || v_FROM_COUNT ||
                             v_WHERE_COUNT;
        END;

        EXECUTE IMMEDIATE v_CANTIDAD
            INTO p_CANTIDADDEREGISTROS;

        BEGIN
            IF v_PAGINA IS NULL THEN
                v_INICIO := 0;
                v_FIN    := p_CANTIDADDEREGISTROS;
            ELSE
                IF v_PAGINA = 1 THEN
                    v_INICIO := 0;
                    v_FIN    := v_INICIO + v_TAMANOPAGINA;
                ELSE
                    v_INICIO := (v_PAGINA - 1) * v_TAMANOPAGINA;
                    v_FIN    := v_INICIO + v_TAMANOPAGINA;
                END IF;
            END IF;
        END;

        BEGIN
            v_SELECT_PAGINA := ' SELECT * FROM (SELECT V.* , ROWNUM  NUMERODEREGISTRO FROM (';
            v_SELECT        := 'SELECT ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.CODIGO, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.NOMBRE, ' ||

                               'EJECUCION.T_CUENTAS_CONTABLES.ACTIVA, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.NIVEL, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.USER_ING, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.FEC_ING, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.USER_ACT, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.FEC_ACT, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.ID_NATURALEZA, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_DEBE, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_HABER, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.IMPUTABLE, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.AUXILIAR,
            EJECUCION.T_CUENTAS_CONTABLES.CLASIFICADOR_RELACIONADO ';

            v_SUB_CONSULTA := v_SELECT || v_FROM_COUNT || v_WHERE_COUNT ||
                              ' ORDER BY 1'; --Falta colocar columna de Ordenamiento
            --        v_WHERE_PAGINA := ' WHERE NUMERODEREGISTRO > ' || v_INICIO || '' || ' AND NUMERODEREGISTRO <= ' || v_FIN || '';
            v_WHERE_PAGINA := ' WHERE NUMERODEREGISTRO > ' || v_INICIO || '' ||
                              ' AND NUMERODEREGISTRO <= 30';

            v_BUSCAR := v_SELECT_PAGINA || v_SUB_CONSULTA || ')V )' ||
                        v_WHERE_PAGINA || ' ORDER BY 1 '; --Falta colocar columna de Ordenamiento

        END;

        --insert into pruebas values (v_BUSCAR,sysdate,'Cuenta_contable'); commit;

        OPEN p_T_CUENTAS_CONTABLES FOR v_BUSCAR;

    END BUSCAR;

    PROCEDURE PRC_GENERARCODIGOS(p_T_CUENTAS_CONTABLES OUT ty_CURSOR,
                                 p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL,
                                 p_ID_JURISDICCION IN EJECUCION.T_CUENTAS_CONTABLES.ID_JURISDICCION%TYPE DEFAULT NULL) IS
        V_CANT_DIGITOS        NUMBER;
        v_NUM_NIVEL           number;
        p_CODIGO              VARCHAR2(100);
        v_ID_CTA_CONTABLE_REL EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE;

    BEGIN


        v_ID_CTA_CONTABLE_REL := p_ID_CTA_CONTABLE_REL;

        IF v_ID_CTA_CONTABLE_REL IS NULL OR v_ID_CTA_CONTABLE_REL = 0 THEN
            -- ES UN PADRE DE PRIMER NIVEL
            v_NUM_NIVEL := 1;
        ELSE

            SELECT DISTINCT (CC.NIVEL + 1)
              INTO v_NUM_NIVEL
              FROM EJECUCION.T_CUENTAS_CONTABLES CC
             WHERE CC.ID_CTA_CONTABLE = v_ID_CTA_CONTABLE_REL;

        END IF;

        IF v_NUM_NIVEL = 1 THEN
                SELECT RPAD(TO_NUMBER(MAX(SUBSTR(CC.CODIGO, 0, 2))) + 1,
                            2,
                            '0')
                  INTO p_CODIGO
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE CC.ID_JURISDICCION = P_ID_JURISDICCION;

/*            SELECT PKG_T_CUENTAS_CONTABLES.FUNC_PROXIMO_NIVEL1(v_ID_CTA_CONTABLE_REL)
              INTO p_CODIGO
              FROM dual;*/
        ELSE
            IF v_NUM_NIVEL = 2 THEN
                SELECT MAX(SUBSTR(CC.CODIGO, 0, 2)) ||
                       RPAD(TO_NUMBER(MAX(SUBSTR(CC.CODIGO, 4, 2))) + 1,
                            2,
                            '0')
                  INTO p_CODIGO
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(V_NUM_NIVEL,
                               1,
                               ID_CTA_CONTABLE_REL,
                               V_ID_CTA_CONTABLE_REL) OR
                       ID_CTA_CONTABLE =
                       DECODE(V_NUM_NIVEL,
                               1,
                               ID_CTA_CONTABLE,
                               V_ID_CTA_CONTABLE_REL));
                /*            SELECT
                max(DECODE(LENGTH(CC.CODIGO),13,SUBSTR(CC.CODIGO,1,1),14,SUBSTR(CC.CODIGO,1,2)))||
                PKG_T_CUENTAS_CONTABLES.FUNC_PROXIMO_NIVEL2(v_ID_CTA_CONTABLE_REL)||
                max(DECODE(LENGTH(CC.CODIGO),13,SUBSTR(CC.CODIGO,4,2),14,SUBSTR(CC.CODIGO,5,2)) )||
                max(DECODE(LENGTH(CC.CODIGO),13,SUBSTR(CC.CODIGO,6,2),14,SUBSTR(CC.CODIGO,7,2))  )
                INTO p_CODIGO
                FROM  EJECUCION.T_CUENTAS_CONTABLES CC
                WHERE (CC.ID_CTA_CONTABLE_REL = DECODE(v_NUM_NIVEL, 1,ID_CTA_CONTABLE_REL,v_ID_CTA_CONTABLE_REL )
                OR ID_CTA_CONTABLE = DECODE (v_NUM_NIVEL, 1,ID_CTA_CONTABLE,  v_ID_CTA_CONTABLE_REL));*/
            ELSE
                IF v_NUM_NIVEL = 3 THEN
                    SELECT MAX(SUBSTR(CC.CODIGO, 0, 2)) ||
                           MAX(SUBSTR(CC.CODIGO, 4, 2)) ||
                           RPAD(TO_NUMBER(MAX(SUBSTR(CC.CODIGO, 7, 2))) + 1,
                                2,
                                '0')
                      INTO p_CODIGO
                      FROM EJECUCION.T_CUENTAS_CONTABLES CC
                     WHERE (CC.ID_CTA_CONTABLE_REL =
                           DECODE(V_NUM_NIVEL,
                                   1,
                                   ID_CTA_CONTABLE_REL,
                                   V_ID_CTA_CONTABLE_REL) OR
                           ID_CTA_CONTABLE =
                           DECODE(V_NUM_NIVEL,
                                   1,
                                   ID_CTA_CONTABLE,
                                   V_ID_CTA_CONTABLE_REL));
                    /*                    SELECT max(DECODE(LENGTH(CC.CODIGO),
                                     13,
                                     SUBSTR(CC.CODIGO, 1, 1),
                                     14,
                                     SUBSTR(CC.CODIGO, 1, 2))) ||
                          max(DECODE(LENGTH(CC.CODIGO),
                                     13,
                                     SUBSTR(CC.CODIGO, 2, 2),
                                     14,
                                     SUBSTR(CC.CODIGO, 3, 2))) ||
                          PKG_T_CUENTAS_CONTABLES.FUNC_PROXIMO_NIVEL3(v_ID_CTA_CONTABLE_REL) ||
                          max(DECODE(LENGTH(CC.CODIGO),
                                     13,
                                     SUBSTR(CC.CODIGO, 6, 2),
                                     14,
                                     SUBSTR(CC.CODIGO, 7, 2)))
                     INTO p_CODIGO
                     FROM EJECUCION.T_CUENTAS_CONTABLES CC
                    WHERE (CC.ID_CTA_CONTABLE_REL =
                          DECODE(v_NUM_NIVEL,
                                  1,
                                  CC.ID_CTA_CONTABLE_REL,
                                  v_ID_CTA_CONTABLE_REL) OR
                          ID_CTA_CONTABLE =
                          DECODE(v_NUM_NIVEL,
                                  1,
                                  ID_CTA_CONTABLE,
                                  v_ID_CTA_CONTABLE_REL));*/
                ELSE
                    IF v_NUM_NIVEL = 4 THEN
                        SELECT MAX(SUBSTR(CC.CODIGO, 0, 2)) ||
                               MAX(SUBSTR(CC.CODIGO, 4, 2)) ||
                               MAX(SUBSTR(CC.CODIGO, 7, 2)) ||
                               RPAD(TO_NUMBER(MAX(SUBSTR(CC.CODIGO, 10, 2))) + 1,
                                    2,
                                    '0')
                          INTO p_CODIGO
                          FROM EJECUCION.T_CUENTAS_CONTABLES CC
                         WHERE (CC.ID_CTA_CONTABLE_REL =
                               DECODE(V_NUM_NIVEL,
                                       1,
                                       ID_CTA_CONTABLE_REL,
                                       V_ID_CTA_CONTABLE_REL) OR
                               ID_CTA_CONTABLE =
                               DECODE(V_NUM_NIVEL,
                                       1,
                                       ID_CTA_CONTABLE,
                                       V_ID_CTA_CONTABLE_REL));
                        /*
                        SELECT max(DECODE(LENGTH(CC.CODIGO),
                                          13,
                                          SUBSTR(CC.CODIGO, 1, 1),
                                          14,
                                          SUBSTR(CC.CODIGO, 1, 2))) ||
                               max(DECODE(LENGTH(CC.CODIGO),
                                          13,
                                          SUBSTR(CC.CODIGO, 2, 2),
                                          14,
                                          SUBSTR(CC.CODIGO, 3, 2))) ||
                               max(DECODE(LENGTH(CC.CODIGO),
                                          13,
                                          SUBSTR(CC.CODIGO, 4, 2),
                                          14,
                                          SUBSTR(CC.CODIGO, 5, 2))) ||
                               PKG_T_CUENTAS_CONTABLES.FUNC_PROXIMO_NIVEL4(v_ID_CTA_CONTABLE_REL)
                          INTO p_CODIGO
                          FROM EJECUCION.T_CUENTAS_CONTABLES CC
                         WHERE (CC.ID_CTA_CONTABLE_REL =
                               DECODE(v_NUM_NIVEL,
                                       1,
                                       CC.ID_CTA_CONTABLE_REL,
                                       v_ID_CTA_CONTABLE_REL) OR
                               ID_CTA_CONTABLE =
                               DECODE(v_NUM_NIVEL,
                                       1,
                                       ID_CTA_CONTABLE,
                                       v_ID_CTA_CONTABLE_REL));*/
                    ELSE
                        IF v_NUM_NIVEL = 5 THEN
                            SELECT MAX(SUBSTR(CC.CODIGO, 0, 2)) ||
                                   MAX(SUBSTR(CC.CODIGO, 4, 2)) ||
                                   MAX(SUBSTR(CC.CODIGO, 7, 2)) ||
                                   MAX(SUBSTR(CC.CODIGO, 10, 2)) ||
                                   RPAD(TO_NUMBER(MAX(SUBSTR(CC.CODIGO,
                                                             13,
                                                             2))) + 1,
                                        2,
                                        '0')
                              INTO p_CODIGO
                              FROM EJECUCION.T_CUENTAS_CONTABLES CC
                             WHERE (CC.ID_CTA_CONTABLE_REL =
                                   DECODE(V_NUM_NIVEL,
                                           1,
                                           ID_CTA_CONTABLE_REL,
                                           V_ID_CTA_CONTABLE_REL) OR
                                   ID_CTA_CONTABLE =
                                   DECODE(V_NUM_NIVEL,
                                           1,
                                           ID_CTA_CONTABLE,
                                           V_ID_CTA_CONTABLE_REL));
                            /*
                                                        SELECT max(DECODE(LENGTH(CC.CODIGO),
                                                                          13,
                                                                          SUBSTR(CC.CODIGO, 1, 1),
                                                                          14,
                                                                          SUBSTR(CC.CODIGO, 1, 2))) ||
                                                               max(DECODE(LENGTH(CC.CODIGO),
                                                                          13,
                                                                          SUBSTR(CC.CODIGO, 2, 2),
                                                                          14,
                                                                          SUBSTR(CC.CODIGO, 3, 2))) ||
                                                               max(DECODE(LENGTH(CC.CODIGO),
                                                                          13,
                                                                          SUBSTR(CC.CODIGO, 4, 2),
                                                                          14,
                                                                          SUBSTR(CC.CODIGO, 5, 2))) ||
                                                               max(DECODE(LENGTH(CC.CODIGO),
                                                                          13,
                                                                          SUBSTR(CC.CODIGO, 6, 2),
                                                                          14,
                                                                          SUBSTR(CC.CODIGO, 7, 2))) ||
                                                               PKG_T_CUENTAS_CONTABLES.FUNC_PROXIMO_NIVEL5(v_ID_CTA_CONTABLE_REL)
                                                          INTO p_CODIGO
                                                          FROM EJECUCION.T_CUENTAS_CONTABLES CC
                                                         WHERE (CC.ID_CTA_CONTABLE_REL =
                                                               DECODE(v_NUM_NIVEL,
                                                                       1,
                                                                       CC.ID_CTA_CONTABLE_REL,
                                                                       v_ID_CTA_CONTABLE_REL) OR
                                                               ID_CTA_CONTABLE =
                                                               DECODE(v_NUM_NIVEL,
                                                                       1,
                                                                       ID_CTA_CONTABLE,
                                                                       v_ID_CTA_CONTABLE_REL));
                            */
                        ELSE
                            IF v_NUM_NIVEL = 6 THEN
                                SELECT max(DECODE(LENGTH(CC.CODIGO),
                                                  13,
                                                  SUBSTR(CC.CODIGO, 1, 1),
                                                  14,
                                                  SUBSTR(CC.CODIGO, 1, 2))) ||
                                       max(DECODE(LENGTH(CC.CODIGO),
                                                  13,
                                                  SUBSTR(CC.CODIGO, 2, 2),
                                                  14,
                                                  SUBSTR(CC.CODIGO, 3, 2))) ||
                                       max(DECODE(LENGTH(CC.CODIGO),
                                                  13,
                                                  SUBSTR(CC.CODIGO, 4, 2),
                                                  14,
                                                  SUBSTR(CC.CODIGO, 5, 2))) ||
                                       max(DECODE(LENGTH(CC.CODIGO),
                                                  13,
                                                  SUBSTR(CC.CODIGO, 6, 2),
                                                  14,
                                                  SUBSTR(CC.CODIGO, 7, 2))) ||
                                       max(DECODE(LENGTH(CC.CODIGO),
                                                  13,
                                                  SUBSTR(CC.CODIGO, 8, 2),
                                                  14,
                                                  SUBSTR(CC.CODIGO, 9, 2))) ||
                                       PKG_T_CUENTAS_CONTABLES.FUNC_PROXIMO_NIVEL6(v_ID_CTA_CONTABLE_REL)
                                  INTO p_CODIGO
                                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                                 WHERE (CC.ID_CTA_CONTABLE_REL =
                                       DECODE(v_NUM_NIVEL,
                                               1,
                                               CC.ID_CTA_CONTABLE_REL,
                                               v_ID_CTA_CONTABLE_REL) OR
                                       ID_CTA_CONTABLE =
                                       DECODE(v_NUM_NIVEL,
                                               1,
                                               ID_CTA_CONTABLE,
                                               v_ID_CTA_CONTABLE_REL));
                            ELSE
                                IF v_NUM_NIVEL = 7 THEN
                                    SELECT max(DECODE(LENGTH(CC.CODIGO),
                                                      13,
                                                      SUBSTR(CC.CODIGO, 1, 1),
                                                      14,
                                                      SUBSTR(CC.CODIGO, 1, 2))) ||
                                           max(DECODE(LENGTH(CC.CODIGO),
                                                      13,
                                                      SUBSTR(CC.CODIGO, 2, 2),
                                                      14,
                                                      SUBSTR(CC.CODIGO, 3, 2))) ||
                                           max(DECODE(LENGTH(CC.CODIGO),
                                                      13,
                                                      SUBSTR(CC.CODIGO, 4, 2),
                                                      14,
                                                      SUBSTR(CC.CODIGO, 5, 2))) ||
                                           max(DECODE(LENGTH(CC.CODIGO),
                                                      13,
                                                      SUBSTR(CC.CODIGO, 6, 2),
                                                      14,
                                                      SUBSTR(CC.CODIGO, 7, 2))) ||
                                           max(DECODE(LENGTH(CC.CODIGO),
                                                      13,
                                                      SUBSTR(CC.CODIGO, 8, 2),
                                                      14,
                                                      SUBSTR(CC.CODIGO, 9, 2))) ||
                                           PKG_T_CUENTAS_CONTABLES.FUNC_PROXIMO_NIVEL7(v_ID_CTA_CONTABLE_REL)
                                      INTO p_CODIGO
                                      FROM EJECUCION.T_CUENTAS_CONTABLES CC
                                     WHERE (CC.ID_CTA_CONTABLE_REL =
                                           DECODE(v_NUM_NIVEL,
                                                   1,
                                                   CC.ID_CTA_CONTABLE_REL,
                                                   v_ID_CTA_CONTABLE_REL) OR
                                           ID_CTA_CONTABLE =
                                           DECODE(v_NUM_NIVEL,
                                                   1,
                                                   ID_CTA_CONTABLE,
                                                   v_ID_CTA_CONTABLE_REL));
                                END IF;
                            END IF;
                        END IF;
                    END IF;
                END IF;
            END IF;
        END IF;

        P_CODIGO := RPAD(P_CODIGO, 16, '0');

        OPEN p_T_CUENTAS_CONTABLES FOR
            SELECT DECODE(LENGTH(p_CODIGO),
                          15,
                          SUBSTR(p_CODIGO, 1, 1),
                          16,
                          SUBSTR(p_CODIGO, 1, 2)) NIVEL1,
                   DECODE(LENGTH(p_CODIGO),
                          15,
                          SUBSTR(p_CODIGO, 2, 2),
                          16,
                          SUBSTR(p_CODIGO, 3, 2)) NIVEL2,
                   DECODE(LENGTH(p_CODIGO),
                          15,
                          SUBSTR(p_CODIGO, 4, 2),
                          16,
                          SUBSTR(p_CODIGO, 5, 2)) NIVEL3,
                   DECODE(LENGTH(p_CODIGO),
                          15,
                          SUBSTR(p_CODIGO, 6, 2),
                          16,
                          SUBSTR(p_CODIGO, 7, 2)) NIVEL4,
                   DECODE(LENGTH(p_CODIGO),
                          15,
                          SUBSTR(p_CODIGO, 8, 2),
                          16,
                          SUBSTR(p_CODIGO, 9, 2)) NIVEL5,
                   DECODE(LENGTH(p_CODIGO),
                          15,
                          SUBSTR(p_CODIGO, 10, 2),
                          16,
                          SUBSTR(p_CODIGO, 11, 2)) NIVEL6,
                   DECODE(LENGTH(p_CODIGO),
                          15,
                          SUBSTR(p_CODIGO, 12, 2),
                          16,
                          SUBSTR(p_CODIGO, 13, 2)) NIVEL7,
                   v_NUM_NIVEL nivel

              FROM DUAL;

        /*exception


        when others then
               p_MSG :='Error en PKG_T_OBJETOS_GSTO.PRC_GENERARCODIGOS '|| sqlerrm;
               pkg_util.base_rollback;*/

    END PRC_GENERARCODIGOS;

    function FUNC_PROXIMO_NIVEL1(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2 is

        P_PADRE     VARCHAR2(100);
        v_NUM_NIVEL number;
        p_MAXCOD    VARCHAR2(100);
        V_INDICE    NUMBER;

    BEGIN

        v_NUM_NIVEL := 1;

        SELECT nvl(max(distinct to_number(DECODE(LENGTH(CC.CODIGO),
                                        15,
                                        SUBSTR(CC.CODIGO, 1, 1),
                                        16,
                                        SUBSTR(CC.CODIGO, 1, 2)))),
                   1)
          INTO p_MAXCOD
          FROM EJECUCION.T_CUENTAS_CONTABLES CC;

        IF p_MAXCOD = 99 THEN
            -- TENGO QUE BUSCAR EL PRIMER PADRE DISPONIBLE DE LA SERIE
            FOR i IN 1 .. 99 LOOP
                V_INDICE := i;
                SELECT i
                  INTO V_INDICE
                  FROM DUAL
                 WHERE i IN (
                             -- LISTA DE NUMERO USADOS PARA PADRES
                             SELECT DISTINCT TO_NUMBER(DECODE(LENGTH(CC.CODIGO),
                                                               13,
                                                               SUBSTR(CC.CODIGO,
                                                                      1,
                                                                      1),
                                                               14,
                                                               SUBSTR(CC.CODIGO,
                                                                      1,
                                                                      2)))
                               FROM EJECUCION.T_CUENTAS_CONTABLES CC);

            END LOOP;

        ELSE
            IF p_MAXCOD >= 9 THEN
                SELECT rpad(to_char(SUBSTR(MAX(CC.CODIGO), 1, 2) + 1),
                            16,
                            0)
                  INTO P_PADRE
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC;
            ELSE

                SELECT rpad('0' || lpad(to_char(SUBSTR(MAX(CC.CODIGO), 1, 2) + 1),
                                        1,
                                        0),
                            16,
                            0)
                  INTO P_PADRE
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC;
            END IF;

            RETURN(P_PADRE);

        END IF;

    exception
        WHEN no_data_found THEN
            if LENGTH(v_INDICE) = 2 then
                P_PADRE := rpad(v_INDICE, 8, 0);
                RETURN(P_PADRE);
            else
                P_PADRE := rpad(v_INDICE, 7, 0);
                RETURN(P_PADRE);
            end if;

    END FUNC_PROXIMO_NIVEL1;

    function FUNC_PROXIMO_NIVEL2(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2 is

        P_HIJO VARCHAR2(100);
        --V_CANT_DIGITOS NUMBER;
        v_NUM_NIVEL           number;
        p_MAXCOD              VARCHAR2(100);
        V_INDICE              NUMBER;
        v_ID_CTA_CONTABLE_REL EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE;

    BEGIN
        v_ID_CTA_CONTABLE_REL := p_ID_CTA_CONTABLE_REL;
        v_NUM_NIVEL           := 2;

        SELECT nvl(max(distinct to_number(SUBSTR(CC.CODIGO,
                                        (LENGTH(CC.CODIGO) - 1),
                                        2))),
                   2) -- nvl(max( distinct to_number( DECODE(LENGTH(CC.CODIGO),15,SUBSTR(CC.CODIGO,2,2),16,SUBSTR(CC.CODIGO,3,2)))),2)--MAX(DECODE(LENGTH(CC.CODIGO),13,SUBSTR(CC.CODIGO,2,2),14,SUBSTR(CC.CODIGO,3,2)) )
          INTO p_MAXCOD
          FROM EJECUCION.T_CUENTAS_CONTABLES CC
         WHERE (CC.ID_CTA_CONTABLE_REL =
               DECODE(v_NUM_NIVEL,
                       1,
                       CC.ID_CTA_CONTABLE_REL,
                       v_ID_CTA_CONTABLE_REL) OR
               CC.ID_CTA_CONTABLE =
               DECODE(v_NUM_NIVEL,
                       1,
                       CC.ID_CTA_CONTABLE,
                       v_ID_CTA_CONTABLE_REL));

        IF p_MAXCOD = 99 THEN
            -- TENGO QUE BUSCAR EL PRIMER PADRE DISPONIBLE DE LA SERIE
            FOR i IN 1 .. 99 LOOP
                V_INDICE := i;
                SELECT i
                  INTO V_INDICE
                  FROM DUAL
                 WHERE i IN (
                             -- LISTA DE NUMERO USADOS PARA HIJOS
                             SELECT DISTINCT DECODE(LENGTH(CC.CODIGO),
                                                     13,
                                                     SUBSTR(CC.CODIGO, 2, 2),
                                                     14,
                                                     SUBSTR(CC.CODIGO, 3, 2))
                               FROM EJECUCION.T_CUENTAS_CONTABLES CC
                              WHERE (CC.ID_CTA_CONTABLE_REL =
                                    DECODE(v_NUM_NIVEL,
                                            1,
                                            CC.ID_CTA_CONTABLE_REL,
                                            v_ID_CTA_CONTABLE_REL) OR
                                    CC.ID_CTA_CONTABLE =
                                    DECODE(v_NUM_NIVEL,
                                            1,
                                            CC.ID_CTA_CONTABLE,
                                            v_ID_CTA_CONTABLE_REL)));

            END LOOP;

        ELSE
            IF p_MAXCOD >= 9 THEN
                SELECT to_char(SUBSTR(MAX(CC.CODIGO), 1, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 3, 2) + 1) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 5, 16))
                  INTO P_HIJO
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE_REL,
                               v_ID_CTA_CONTABLE_REL) OR
                       CC.ID_CTA_CONTABLE =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE,
                               v_ID_CTA_CONTABLE_REL));

            ELSE

                SELECT to_char(SUBSTR(MAX(REPLACE(CC.CODIGO, '.', '')),
                                      1,
                                      2)) || '0' ||
                       to_char(SUBSTR(MAX(REPLACE(CC.CODIGO, '.', '')),
                                      3,
                                      2) + 1) ||
                       to_char(SUBSTR(MAX(REPLACE(CC.CODIGO, '.', '')),
                                      5,
                                      16))
                  INTO P_HIJO
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE_REL,
                               v_ID_CTA_CONTABLE_REL) OR
                       CC.ID_CTA_CONTABLE =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE,
                               v_ID_CTA_CONTABLE_REL));
            END IF;

            RETURN(P_HIJO || '00');

        END IF;

    exception
        WHEN no_data_found THEN
            --               p_MSG:='No existen mas codigos disponibles'||SQLERRM;
            P_HIJO := Lpad(v_INDICE, 2, 0);
            RETURN(P_HIJO);

    END FUNC_PROXIMO_NIVEL2;

    function FUNC_PROXIMO_NIVEL3(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2 is

        P_NIETO VARCHAR2(100);
        --V_CANT_DIGITOS NUMBER;
        v_NUM_NIVEL           number;
        p_MAXCOD              VARCHAR2(100);
        V_INDICE              NUMBER;
        v_ID_CTA_CONTABLE_REL EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE;

    BEGIN
        v_ID_CTA_CONTABLE_REL := p_ID_CTA_CONTABLE_REL;
        v_NUM_NIVEL           := 3;

        SELECT MAX(DECODE(LENGTH(CC.CODIGO),
                          15,
                          SUBSTR(CC.CODIGO, 4, 2),
                          16,
                          SUBSTR(CC.CODIGO, 5, 2)))
          INTO p_MAXCOD
          FROM EJECUCION.T_CUENTAS_CONTABLES CC
         WHERE (CC.ID_CTA_CONTABLE_REL =
               DECODE(v_NUM_NIVEL,
                       1,
                       CC.ID_CTA_CONTABLE_REL,
                       v_ID_CTA_CONTABLE_REL) OR
               CC.ID_CTA_CONTABLE =
               DECODE(v_NUM_NIVEL,
                       1,
                       CC.ID_CTA_CONTABLE,
                       v_ID_CTA_CONTABLE_REL));

        IF p_MAXCOD = 99 THEN
            -- TENGO QUE BUSCAR EL PRIMER PADRE DISPONIBLE DE LA SERIE
            FOR i IN 1 .. 99 LOOP
                V_INDICE := i;
                SELECT i
                  INTO V_INDICE
                  FROM DUAL
                 WHERE i IN (
                             -- LISTA DE NUMERO USADOS PARA HIJOS
                             SELECT DISTINCT DECODE(LENGTH(CC.CODIGO),
                                                     13,
                                                     SUBSTR(CC.CODIGO, 4, 2),
                                                     14,
                                                     SUBSTR(CC.CODIGO, 5, 2))
                               FROM EJECUCION.T_CUENTAS_CONTABLES CC
                              WHERE (CC.ID_CTA_CONTABLE_REL =
                                    DECODE(v_NUM_NIVEL,
                                            1,
                                            CC.ID_CTA_CONTABLE_REL,
                                            v_ID_CTA_CONTABLE_REL) OR
                                    CC.ID_CTA_CONTABLE =
                                    DECODE(v_NUM_NIVEL,
                                            1,
                                            CC.ID_CTA_CONTABLE,
                                            v_ID_CTA_CONTABLE_REL)));

            END LOOP;

        ELSE
            IF p_MAXCOD >= 9 THEN
                SELECT to_char(SUBSTR(MAX(CC.CODIGO), 1, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 3, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 5, 2) + 1) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 7, 16))
                  INTO P_NIETO
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE_REL,
                               v_ID_CTA_CONTABLE_REL) OR
                       CC.ID_CTA_CONTABLE =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE,
                               v_ID_CTA_CONTABLE_REL));

            ELSE

                SELECT to_char(SUBSTR(MAX(REPLACE(CC.CODIGO, '.', '')),
                                      1,
                                      2)) ||
                       to_char(SUBSTR(MAX(REPLACE(CC.CODIGO, '.', '')),
                                      3,
                                      2)) || '0' ||
                       to_char(SUBSTR(MAX(REPLACE(CC.CODIGO, '.', '')),
                                      5,
                                      2) + 1) ||
                       to_char(SUBSTR(MAX(REPLACE(CC.CODIGO, '.', '')),
                                      7,
                                      16))
                  INTO P_NIETO
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE_REL,
                               v_ID_CTA_CONTABLE_REL) OR
                       CC.ID_CTA_CONTABLE =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE,
                               v_ID_CTA_CONTABLE_REL));

            END IF;
            RETURN(P_NIETO || '00');

        END IF;

    exception
        WHEN no_data_found THEN
            --               p_MSG:='No existen mas codigos disponibles'||SQLERRM;
            P_NIETO := Lpad(v_INDICE, 2, 0);
            RETURN(P_NIETO);

    END FUNC_PROXIMO_NIVEL3;

    function FUNC_PROXIMO_NIVEL4(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2 is

        P_BISNIETO VARCHAR2(100);
        --V_CANT_DIGITOS NUMBER;
        v_NUM_NIVEL           number;
        p_MAXCOD              VARCHAR2(100);
        V_INDICE              NUMBER;
        v_ID_CTA_CONTABLE_REL EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE;

    BEGIN
        v_ID_CTA_CONTABLE_REL := p_ID_CTA_CONTABLE_REL;
        v_NUM_NIVEL           := 3;

        SELECT MAX(DECODE(LENGTH(CC.CODIGO),
                          13,
                          SUBSTR(CC.CODIGO, 6, 2),
                          14,
                          SUBSTR(CC.CODIGO, 7, 2)))
          INTO p_MAXCOD
          FROM EJECUCION.T_CUENTAS_CONTABLES CC
         WHERE (CC.ID_CTA_CONTABLE_REL =
               DECODE(v_NUM_NIVEL,
                       1,
                       CC.ID_CTA_CONTABLE_REL,
                       v_ID_CTA_CONTABLE_REL) OR
               CC.ID_CTA_CONTABLE =
               DECODE(v_NUM_NIVEL,
                       1,
                       CC.ID_CTA_CONTABLE,
                       v_ID_CTA_CONTABLE_REL));

        IF p_MAXCOD = 99 THEN
            -- TENGO QUE BUSCAR EL PRIMER PADRE DISPONIBLE DE LA SERIE
            FOR i IN 1 .. 99 LOOP
                V_INDICE := i;
                SELECT i
                  INTO V_INDICE
                  FROM DUAL
                 WHERE i IN (
                             -- LISTA DE NUMERO USADOS PARA HIJOS
                             SELECT DISTINCT DECODE(LENGTH(CC.CODIGO),
                                                     13,
                                                     SUBSTR(CC.CODIGO, 6, 2),
                                                     14,
                                                     SUBSTR(CC.CODIGO, 7, 2))
                               FROM EJECUCION.T_CUENTAS_CONTABLES CC
                              WHERE (CC.ID_CTA_CONTABLE_REL =
                                    DECODE(v_NUM_NIVEL,
                                            1,
                                            CC.ID_CTA_CONTABLE_REL,
                                            v_ID_CTA_CONTABLE_REL) or
                                    CC.ID_CTA_CONTABLE =
                                    DECODE(v_NUM_NIVEL,
                                            1,
                                            CC.ID_CTA_CONTABLE,
                                            v_ID_CTA_CONTABLE_REL)));

            END LOOP;

        ELSE
            IF p_MAXCOD >= 9 THEN
                SELECT to_char(SUBSTR(MAX(CC.CODIGO), 1, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 3, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 5, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 7, 2) + 1) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 9, 16))
                  INTO P_BISNIETO
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE_REL,
                               v_ID_CTA_CONTABLE_REL) or
                       CC.ID_CTA_CONTABLE =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE,
                               v_ID_CTA_CONTABLE_REL));

            ELSE
                SELECT to_char(SUBSTR(MAX(CC.CODIGO), 1, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 3, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 5, 2)) || '0' ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 7, 2) + 1) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 9, 16))
                  INTO P_BISNIETO
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE_REL,
                               v_ID_CTA_CONTABLE_REL) or
                       CC.ID_CTA_CONTABLE =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE,
                               v_ID_CTA_CONTABLE_REL));
            END IF;

            RETURN(P_BISNIETO || '00');

        END IF;

    exception
        WHEN no_data_found THEN
            --               p_MSG:='No existen mas codigos disponibles'||SQLERRM;
            P_BISNIETO := Lpad(v_INDICE, 2, 0);
            RETURN(P_BISNIETO);

    END FUNC_PROXIMO_NIVEL4;

    function FUNC_PROXIMO_NIVEL5(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2 is

        P_NIVEL5 VARCHAR2(100);
        --V_CANT_DIGITOS NUMBER;
        v_NUM_NIVEL           number;
        p_MAXCOD              VARCHAR2(100);
        V_INDICE              NUMBER;
        v_ID_CTA_CONTABLE_REL EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE;

    BEGIN
        v_ID_CTA_CONTABLE_REL := p_ID_CTA_CONTABLE_REL;
        v_NUM_NIVEL           := 3;

        SELECT MAX(DECODE(LENGTH(CC.CODIGO),
                          13,
                          SUBSTR(CC.CODIGO, 8, 2),
                          14,
                          SUBSTR(CC.CODIGO, 9, 2)))
          INTO p_MAXCOD
          FROM EJECUCION.T_CUENTAS_CONTABLES CC
         WHERE (CC.ID_CTA_CONTABLE_REL =
               DECODE(v_NUM_NIVEL,
                       1,
                       CC.ID_CTA_CONTABLE_REL,
                       v_ID_CTA_CONTABLE_REL) OR
               CC.ID_CTA_CONTABLE =
               DECODE(v_NUM_NIVEL,
                       1,
                       CC.ID_CTA_CONTABLE,
                       v_ID_CTA_CONTABLE_REL));

        IF p_MAXCOD = 99 THEN
            -- TENGO QUE BUSCAR EL PRIMER PADRE DISPONIBLE DE LA SERIE
            FOR i IN 1 .. 99 LOOP
                V_INDICE := i;
                SELECT i
                  INTO V_INDICE
                  FROM DUAL
                 WHERE i IN (
                             -- LISTA DE NUMERO USADOS PARA HIJOS
                             SELECT DISTINCT DECODE(LENGTH(CC.CODIGO),
                                                     13,
                                                     SUBSTR(CC.CODIGO, 8, 2),
                                                     14,
                                                     SUBSTR(CC.CODIGO, 9, 2))
                               FROM EJECUCION.T_CUENTAS_CONTABLES CC
                              WHERE (CC.ID_CTA_CONTABLE_REL =
                                    DECODE(v_NUM_NIVEL,
                                            1,
                                            CC.ID_CTA_CONTABLE_REL,
                                            v_ID_CTA_CONTABLE_REL) or
                                    CC.ID_CTA_CONTABLE =
                                    DECODE(v_NUM_NIVEL,
                                            1,
                                            CC.ID_CTA_CONTABLE,
                                            v_ID_CTA_CONTABLE_REL)));

            END LOOP;

        ELSE
            IF p_MAXCOD >= 9 THEN
                SELECT to_char(SUBSTR(MAX(CC.CODIGO), 1, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 3, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 5, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 7, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 9, 2) + 1) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 11, 16))
                  INTO P_NIVEL5
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE_REL,
                               v_ID_CTA_CONTABLE_REL) or
                       CC.ID_CTA_CONTABLE =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE,
                               v_ID_CTA_CONTABLE_REL));

            ELSE

                SELECT to_char(SUBSTR(MAX(CC.CODIGO), 1, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 3, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 5, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 7, 2)) || '0' ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 9, 2) + 1) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 11, 16)) --LPAD(TO_CHAR(max(DECODE(LENGTH(CC.CODIGO),13,SUBSTR(CC.CODIGO,8,2),14,SUBSTR(CC.CODIGO,9,2))+1 )),2,0)
                  INTO P_NIVEL5
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE_REL,
                               v_ID_CTA_CONTABLE_REL) or
                       CC.ID_CTA_CONTABLE =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE,
                               v_ID_CTA_CONTABLE_REL));

            END IF;

            RETURN(P_NIVEL5 || '00');

        END IF;

    exception
        WHEN no_data_found THEN
            --               p_MSG:='No existen mas codigos disponibles'||SQLERRM;
            P_NIVEL5 := Lpad(v_INDICE, 2, 0);
            RETURN(P_NIVEL5);

    END FUNC_PROXIMO_NIVEL5;

    function FUNC_PROXIMO_NIVEL6(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2 is

        P_NIVEL6 VARCHAR2(100);
        --V_CANT_DIGITOS NUMBER;
        v_NUM_NIVEL           number;
        p_MAXCOD              VARCHAR2(100);
        V_INDICE              NUMBER;
        v_ID_CTA_CONTABLE_REL EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE;

    BEGIN
        v_ID_CTA_CONTABLE_REL := p_ID_CTA_CONTABLE_REL;
        v_NUM_NIVEL           := 3;

        SELECT MAX(DECODE(LENGTH(CC.CODIGO),
                          13,
                          SUBSTR(CC.CODIGO, 9, 2),
                          14,
                          SUBSTR(CC.CODIGO, 10, 2)))
          INTO p_MAXCOD
          FROM EJECUCION.T_CUENTAS_CONTABLES CC
         WHERE (CC.ID_CTA_CONTABLE_REL =
               DECODE(v_NUM_NIVEL,
                       1,
                       CC.ID_CTA_CONTABLE_REL,
                       v_ID_CTA_CONTABLE_REL) OR
               CC.ID_CTA_CONTABLE =
               DECODE(v_NUM_NIVEL,
                       1,
                       CC.ID_CTA_CONTABLE,
                       v_ID_CTA_CONTABLE_REL));

        IF p_MAXCOD = 99 THEN
            -- TENGO QUE BUSCAR EL PRIMER PADRE DISPONIBLE DE LA SERIE
            FOR i IN 1 .. 99 LOOP
                V_INDICE := i;
                SELECT i
                  INTO V_INDICE
                  FROM DUAL
                 WHERE i IN (
                             -- LISTA DE NUMERO USADOS PARA HIJOS
                             SELECT DISTINCT DECODE(LENGTH(CC.CODIGO),
                                                     13,
                                                     SUBSTR(CC.CODIGO, 9, 2),
                                                     14,
                                                     SUBSTR(CC.CODIGO, 10, 2))
                               FROM EJECUCION.T_CUENTAS_CONTABLES CC
                              WHERE (CC.ID_CTA_CONTABLE_REL =
                                    DECODE(v_NUM_NIVEL,
                                            1,
                                            CC.ID_CTA_CONTABLE_REL,
                                            v_ID_CTA_CONTABLE_REL) or
                                    CC.ID_CTA_CONTABLE =
                                    DECODE(v_NUM_NIVEL,
                                            1,
                                            CC.ID_CTA_CONTABLE,
                                            v_ID_CTA_CONTABLE_REL)));

            END LOOP;

        ELSE

            IF p_MAXCOD >= 9 THEN
                SELECT to_char(SUBSTR(MAX(CC.CODIGO), 1, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 3, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 5, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 7, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 9, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 11, 2) + 1) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 13, 16))
                  INTO P_NIVEL6
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE_REL,
                               v_ID_CTA_CONTABLE_REL) or
                       CC.ID_CTA_CONTABLE =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE,
                               v_ID_CTA_CONTABLE_REL));

            ELSE

                SELECT to_char(SUBSTR(MAX(CC.CODIGO), 1, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 3, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 5, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 7, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 9, 2)) || '0' ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 11, 2) + 1) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 13, 16))
                  INTO P_NIVEL6
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE_REL,
                               v_ID_CTA_CONTABLE_REL) or
                       CC.ID_CTA_CONTABLE =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE,
                               v_ID_CTA_CONTABLE_REL));

            END IF;
            RETURN(P_NIVEL6);

        END IF;

    exception
        WHEN no_data_found THEN
            --               p_MSG:='No existen mas codigos disponibles'||SQLERRM;
            P_NIVEL6 := Lpad(v_INDICE, 2, 0);
            RETURN(P_NIVEL6);

    END FUNC_PROXIMO_NIVEL6;

    function FUNC_PROXIMO_NIVEL7(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2 is

        P_NIVEL7 VARCHAR2(100);
        --V_CANT_DIGITOS NUMBER;
        v_NUM_NIVEL           number;
        p_MAXCOD              VARCHAR2(100);
        V_INDICE              NUMBER;
        v_ID_CTA_CONTABLE_REL EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE;

    BEGIN
        v_ID_CTA_CONTABLE_REL := p_ID_CTA_CONTABLE_REL;
        v_NUM_NIVEL           := 7;

        SELECT MAX(DECODE(LENGTH(CC.CODIGO),
                          13,
                          SUBSTR(CC.CODIGO, 10, 2),
                          14,
                          SUBSTR(CC.CODIGO, 11, 4)))
          INTO p_MAXCOD
          FROM EJECUCION.T_CUENTAS_CONTABLES CC
         WHERE (CC.ID_CTA_CONTABLE_REL =
               DECODE(v_NUM_NIVEL,
                       1,
                       CC.ID_CTA_CONTABLE_REL,
                       v_ID_CTA_CONTABLE_REL) OR
               CC.ID_CTA_CONTABLE =
               DECODE(v_NUM_NIVEL,
                       1,
                       CC.ID_CTA_CONTABLE,
                       v_ID_CTA_CONTABLE_REL));

        IF p_MAXCOD = 99 THEN
            -- TENGO QUE BUSCAR EL PRIMER PADRE DISPONIBLE DE LA SERIE
            FOR i IN 1 .. 99 LOOP
                V_INDICE := i;
                SELECT i
                  INTO V_INDICE
                  FROM DUAL
                 WHERE i IN (
                             -- LISTA DE NUMERO USADOS PARA HIJOS
                             SELECT DISTINCT DECODE(LENGTH(CC.CODIGO),
                                                     13,
                                                     SUBSTR(CC.CODIGO, 9, 2),
                                                     14,
                                                     SUBSTR(CC.CODIGO, 10, 2))
                               FROM EJECUCION.T_CUENTAS_CONTABLES CC
                              WHERE (CC.ID_CTA_CONTABLE_REL =
                                    DECODE(v_NUM_NIVEL,
                                            1,
                                            CC.ID_CTA_CONTABLE_REL,
                                            v_ID_CTA_CONTABLE_REL) or
                                    CC.ID_CTA_CONTABLE =
                                    DECODE(v_NUM_NIVEL,
                                            1,
                                            CC.ID_CTA_CONTABLE,
                                            v_ID_CTA_CONTABLE_REL)));

            END LOOP;

        ELSE
            IF p_MAXCOD >= 9 THEN
                SELECT to_char(SUBSTR(MAX(CC.CODIGO), 1, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 3, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 5, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 7, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 9, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 11, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO) + 1, 13, 4))
                  INTO P_NIVEL7
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE_REL,
                               v_ID_CTA_CONTABLE_REL) or
                       CC.ID_CTA_CONTABLE =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE,
                               v_ID_CTA_CONTABLE_REL));

            ELSE

                SELECT to_char(SUBSTR(MAX(CC.CODIGO), 1, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 3, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 5, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 7, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 9, 2)) ||
                       to_char(SUBSTR(MAX(CC.CODIGO), 11, 2)) || '0' ||
                       to_char(SUBSTR(MAX(CC.CODIGO) + 1, 13, 4))
                  INTO P_NIVEL7
                  FROM EJECUCION.T_CUENTAS_CONTABLES CC
                 WHERE (CC.ID_CTA_CONTABLE_REL =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE_REL,
                               v_ID_CTA_CONTABLE_REL) or
                       CC.ID_CTA_CONTABLE =
                       DECODE(v_NUM_NIVEL,
                               1,
                               CC.ID_CTA_CONTABLE,
                               v_ID_CTA_CONTABLE_REL));

            END IF;
            RETURN(P_NIVEL7);

        END IF;

    exception
        WHEN no_data_found THEN
            --               p_MSG:='No existen mas codigos disponibles'||SQLERRM;
            P_NIVEL7 := Lpad(v_INDICE, 2, 0);
            RETURN(P_NIVEL7);

    END FUNC_PROXIMO_NIVEL7;

    PROCEDURE TRAER_COD_NIVEL_COM(p_T_CUENTAS_CONTABLES OUT ty_CURSOR,
                                  p_ID_CTA_CONTABLE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL) IS
        v_ID_CTA_CONTABLE EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE;
    BEGIN
        v_ID_CTA_CONTABLE := p_ID_CTA_CONTABLE;
        OPEN p_T_CUENTAS_CONTABLES FOR
            SELECT SUBSTR(CC.CODIGO, 1, 2) NIVEL1,
                   SUBSTR(CC.CODIGO, 4, 2) NIVEL2,
                   SUBSTR(CC.CODIGO, 7, 2) NIVEL3,
                   SUBSTR(CC.CODIGO, 10, 2) NIVEL4,
                   SUBSTR(CC.CODIGO, 13, 2) NIVEL5,
                   SUBSTR(CC.CODIGO, 16, 2) NIVEL6,
                   SUBSTR(CC.CODIGO, 19, 4) NIVEL7,
                   NVL(CC.NIVEL, 0) nivel

              FROM EJECUCION.T_CUENTAS_CONTABLES CC
             WHERE CC.ID_CTA_CONTABLE = v_ID_CTA_CONTABLE;

    END TRAER_COD_NIVEL_COM;

    PROCEDURE REPORTE_EXCEL(p_T_CUENTAS_CONTABLES OUT ty_CURSOR) IS
    BEGIN
        OPEN p_T_CUENTAS_CONTABLES FOR
            SELECT SUBSTR(CC.CODIGO, 1, 2) TIT,
                   SUBSTR(CC.CODIGO, 3, 2) CAP,
                   SUBSTR(CC.CODIGO, 5, 2) RUB,
                   SUBSTR(CC.CODIGO, 7, 2) CTA,
                   SUBSTR(CC.CODIGO, 9, 2) SUBCTA,
                   SUBSTR(CC.CODIGO, 11, 2) DET_1,
                   SUBSTR(CC.CODIGO, 13, 4) DET_2,
                   CC.CODIGO CODIGO_CTA,
                   CC.NOMBRE NOMBRE_CTA,
                   NCC.DESCRIPCION TIPO_CTA,
                   CC.IMPUTABLE IMPUTABLE,
                   CC.TIPO_SALDO TIPO_SALDO,
                   (SELECT RDH.DESCRIPCION
                      FROM EJECUCION.T_RECURSO_D_H RDH
                     WHERE RDH.ID_RECURSO_D_H = CC.ID_RECURSO_DEBE) DEBE,
                   (SELECT RDH.DESCRIPCION
                      FROM EJECUCION.T_RECURSO_D_H RDH
                     WHERE RDH.ID_RECURSO_D_H = CC.ID_RECURSO_HABER) HABER

              FROM T_CUENTAS_CONTABLES CC, T_NATURALEZA_CC NCC
             WHERE CC.ID_NATURALEZA = NCC.ID_NATURALEZA_CC;

    END REPORTE_EXCEL;

END PKG_T_CUENTAS_CONTABLES;

PACKAGE "PKG_T_CUENTAS_CONTABLES" AS
    /*
     ORA-02292 integrity constraint (string.string) violated - child record found
     Cause: An attempt was made to delete a row that is referenced by a foreign key.
    */
    v_ERROR_2292 exception;
    PRAGMA Exception_Init(v_ERROR_2292, -2292);

    /*
    ORA-00001 unique constraint (string.string) violated
    Cause: An UPDATE or INSERT statement attempted to insert a duplicate key.
    */
    v_error_00001 exception;
    PRAGMA Exception_Init(v_error_00001, -00001);

    /*
    ORA-01407 cannot update (string) to NULL
    Cause: An attempt was made to update a table column USER.TABLE.COLUMN with a NULL val
    */
    v_error_01407 exception;
    PRAGMA Exception_Init(v_error_01407, -01407);

    TYPE ty_CURSOR IS REF CURSOR;

    PROCEDURE INSERTAR(p_ID_CTA_CONTABLE     OUT EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE,
                       p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL,
                       p_CODIGO              IN EJECUCION.T_CUENTAS_CONTABLES.CODIGO%TYPE DEFAULT NULL,
                       p_NOMBRE              IN EJECUCION.T_CUENTAS_CONTABLES.NOMBRE%TYPE DEFAULT NULL,
                       p_ACTIVA              IN EJECUCION.T_CUENTAS_CONTABLES.ACTIVA%TYPE DEFAULT NULL,
                       p_TIPO_SALDO          IN EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE DEFAULT NULL,
                       p_NIVEL               IN EJECUCION.T_CUENTAS_CONTABLES.NIVEL%TYPE DEFAULT NULL,
                       p_USER_ING            IN EJECUCION.T_CUENTAS_CONTABLES.USER_ING%TYPE DEFAULT NULL,
                       p_FEC_ING             IN EJECUCION.T_CUENTAS_CONTABLES.FEC_ING%TYPE DEFAULT NULL,
                       p_USER_ACT            IN EJECUCION.T_CUENTAS_CONTABLES.USER_ACT%TYPE DEFAULT NULL,
                       p_FEC_ACT             IN EJECUCION.T_CUENTAS_CONTABLES.FEC_ACT%TYPE DEFAULT NULL,
                       p_ID_NATURALEZA       IN EJECUCION.T_CUENTAS_CONTABLES.ID_NATURALEZA%TYPE DEFAULT NULL,
                       p_ID_RECURSO_DEBE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_DEBE%TYPE DEFAULT NULL,
                       p_ID_RECURSO_HABER    IN EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_HABER%TYPE DEFAULT NULL,
                       p_IMPUTABLE           IN EJECUCION.T_CUENTAS_CONTABLES.IMPUTABLE%TYPE DEFAULT NULL,
                       p_AUXILIAR            IN EJECUCION.T_CUENTAS_CONTABLES.AUXILIAR%TYPE DEFAULT NULL,
                       p_ID_JURISDICCION     IN EJECUCION.T_CUENTAS_CONTABLES.ID_JURISDICCION%TYPE DEFAULT NULL,
                       p_ID_VIGENCIA         IN EJECUCION.T_CUENTAS_CONTABLES.ID_VIGENCIA%TYPE DEFAULT NULL,
                       p_ASIENTO_OPS         EJECUCION.T_CUENTAS_CONTABLES.ASIENTO_OPS%TYPE DEFAULT NULL,
                       p_ID_CTA_BANCARIA     EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_BANCARIA%TYPE DEFAULT NULL,
                       p_INVENTARIABLE       EJECUCION.T_CUENTAS_CONTABLES.INVENTARIABLE%TYPE DEFAULT NULL,
                       p_ID_PROVEEDOR        EJECUCION.T_CUENTAS_CONTABLES.ID_PROVEEDOR%TYPE DEFAULT NULL,
                       p_AMORTIZACION        EJECUCION.T_CUENTAS_CONTABLES.AMORTIZACION%TYPE DEFAULT NULL,
                       p_INGRESO_PROPIO      EJECUCION.T_CUENTAS_CONTABLES.INGRESO_PROPIO%TYPE DEFAULT NULL,
                       p_MSG                 IN OUT VARCHAR2);

    PROCEDURE EDITAR(p_ID_CTA_CONTABLE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                     p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL,
                     p_CODIGO              IN EJECUCION.T_CUENTAS_CONTABLES.CODIGO%TYPE DEFAULT NULL,
                     p_NOMBRE              IN EJECUCION.T_CUENTAS_CONTABLES.NOMBRE%TYPE DEFAULT NULL,
                     p_ACTIVA              IN EJECUCION.T_CUENTAS_CONTABLES.ACTIVA%TYPE DEFAULT NULL,
                     p_TIPO_SALDO          IN EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE DEFAULT NULL,
                     p_NIVEL               IN EJECUCION.T_CUENTAS_CONTABLES.NIVEL%TYPE DEFAULT NULL,
                     p_USER_ING            IN EJECUCION.T_CUENTAS_CONTABLES.USER_ING%TYPE DEFAULT NULL,
                     p_FEC_ING             IN EJECUCION.T_CUENTAS_CONTABLES.FEC_ING%TYPE DEFAULT NULL,
                     p_USER_ACT            IN EJECUCION.T_CUENTAS_CONTABLES.USER_ACT%TYPE DEFAULT NULL,
                     p_FEC_ACT             IN EJECUCION.T_CUENTAS_CONTABLES.FEC_ACT%TYPE DEFAULT NULL,
                     p_ID_NATURALEZA       IN EJECUCION.T_CUENTAS_CONTABLES.ID_NATURALEZA%TYPE DEFAULT NULL,
                     p_ID_RECURSO_DEBE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_DEBE%TYPE DEFAULT NULL,
                     p_ID_RECURSO_HABER    IN EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_HABER%TYPE DEFAULT NULL,
                     p_IMPUTABLE           IN EJECUCION.T_CUENTAS_CONTABLES.IMPUTABLE%TYPE DEFAULT NULL,
                     p_AUXILIAR            IN EJECUCION.T_CUENTAS_CONTABLES.AUXILIAR%TYPE DEFAULT NULL,
                     p_ASIENTO_OPS         EJECUCION.T_CUENTAS_CONTABLES.ASIENTO_OPS%TYPE DEFAULT NULL,
                     p_ID_CTA_BANCARIA     EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_BANCARIA%TYPE DEFAULT NULL,
                     p_INVENTARIABLE       EJECUCION.T_CUENTAS_CONTABLES.INVENTARIABLE%TYPE DEFAULT NULL,
                     p_ID_PROVEEDOR        EJECUCION.T_CUENTAS_CONTABLES.ID_PROVEEDOR%TYPE DEFAULT NULL,
                     p_AMORTIZACION        EJECUCION.T_CUENTAS_CONTABLES.AMORTIZACION%TYPE DEFAULT NULL,
                     p_INGRESO_PROPIO      EJECUCION.T_CUENTAS_CONTABLES.INGRESO_PROPIO%TYPE DEFAULT NULL,
                     p_MSG                 IN OUT varchar2);

    PROCEDURE CAMBIARCUENTABANCARIA(p_ID_CTA_CONTABLE IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                                    p_ID_CTA_BANCARIA EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_BANCARIA%TYPE DEFAULT NULL,
                                    p_MSG             IN OUT varchar2);

    PROCEDURE ELIMINAR(p_ID_CTA_CONTABLE IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                       p_MSG             IN OUT varchar2);
    PROCEDURE BUSCARPORPK(p_T_CUENTAS_CONTABLES OUT ty_CURSOR,
                          p_ID_CTA_CONTABLE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL);

    PROCEDURE BUSCARPORCUENTABANCARIA(p_T_CUENTAS_CONTABLES OUT ty_CURSOR,
                                      p_ID_CUENTA_BANCARIA  IN CAJA.T_CUENTAS_BANCARIA.ID_CUENTA_BANCARIA%TYPE DEFAULT NULL);

    PROCEDURE BUSCAR(p_T_CUENTAS_CONTABLES OUT ty_CURSOR,
                     p_ID_CTA_CONTABLE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                     p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL,
                     p_CODIGO              IN EJECUCION.T_CUENTAS_CONTABLES.CODIGO%TYPE DEFAULT NULL,
                     p_NOMBRE              IN EJECUCION.T_CUENTAS_CONTABLES.NOMBRE%TYPE DEFAULT NULL,
                     p_ACTIVA              IN EJECUCION.T_CUENTAS_CONTABLES.ACTIVA%TYPE DEFAULT NULL,
                     p_TIPO_SALDO          IN EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE DEFAULT NULL,
                     p_NIVEL               IN EJECUCION.T_CUENTAS_CONTABLES.NIVEL%TYPE DEFAULT NULL,
                     p_USER_ING            IN EJECUCION.T_CUENTAS_CONTABLES.USER_ING%TYPE DEFAULT NULL,
                     p_FEC_ING             IN EJECUCION.T_CUENTAS_CONTABLES.FEC_ING%TYPE DEFAULT NULL,
                     p_USER_ACT            IN EJECUCION.T_CUENTAS_CONTABLES.USER_ACT%TYPE DEFAULT NULL,
                     p_FEC_ACT             IN EJECUCION.T_CUENTAS_CONTABLES.FEC_ACT%TYPE DEFAULT NULL,
                     p_ID_NATURALEZA       IN EJECUCION.T_CUENTAS_CONTABLES.ID_NATURALEZA%TYPE DEFAULT NULL,
                     p_ID_RECURSO_DEBE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_DEBE%TYPE DEFAULT NULL,
                     p_ID_RECURSO_HABER    IN EJECUCION.T_CUENTAS_CONTABLES.ID_RECURSO_HABER%TYPE DEFAULT NULL,
                     p_IMPUTABLE           IN EJECUCION.T_CUENTAS_CONTABLES.IMPUTABLE%TYPE DEFAULT NULL,
                     p_AUXILIAR            IN EJECUCION.T_CUENTAS_CONTABLES.AUXILIAR%TYPE DEFAULT NULL,
                     p_PAGINA              IN NUMBER DEFAULT NULL,
                     p_TAMANOPAGINA        IN NUMBER DEFAULT NULL,
                     p_CANTIDADDEREGISTROS OUT NUMBER);

        PROCEDURE PRC_GENERARCODIGOS(p_T_CUENTAS_CONTABLES OUT ty_CURSOR,
                                 p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL,
                                 p_ID_JURISDICCION IN EJECUCION.T_CUENTAS_CONTABLES.ID_JURISDICCION%TYPE DEFAULT NULL);

    function FUNC_PROXIMO_NIVEL1(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2;
    function FUNC_PROXIMO_NIVEL2(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2;
    function FUNC_PROXIMO_NIVEL3(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2;

    function FUNC_PROXIMO_NIVEL4(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2;
    function FUNC_PROXIMO_NIVEL5(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2;
    function FUNC_PROXIMO_NIVEL6(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2;
    function FUNC_PROXIMO_NIVEL7(p_ID_CTA_CONTABLE_REL IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE_REL%TYPE DEFAULT NULL

                                 ) return varchar2;
    PROCEDURE TRAER_COD_NIVEL_COM(p_T_CUENTAS_CONTABLES OUT ty_CURSOR,
                                  p_ID_CTA_CONTABLE     IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL);
    PROCEDURE REPORTE_EXCEL(p_T_CUENTAS_CONTABLES OUT ty_CURSOR);
END PKG_T_CUENTAS_CONTABLES;
