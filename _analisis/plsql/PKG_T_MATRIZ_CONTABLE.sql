PACKAGE BODY "PKG_T_MATRIZ_CONTABLE" AS
    PROCEDURE INSERTAR(p_ID_MATRIZ_CBLE  OUT EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE%TYPE,
                       p_ID_CTA_CONTABLE IN EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                       p_ID_OBJETO_GASTO IN EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO%TYPE DEFAULT NULL,
                       p_ID_RECURSO      IN EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO%TYPE DEFAULT NULL,
                       p_ID_VIGENCIA     IN EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA%TYPE DEFAULT NULL,
                       p_USER_ING        IN EJECUCION.T_MATRIZ_CONTABLE.USER_ING%TYPE DEFAULT NULL,
                       p_FEC_ING         IN EJECUCION.T_MATRIZ_CONTABLE.FEC_ING%TYPE DEFAULT NULL,
                       p_USER_ACT        IN EJECUCION.T_MATRIZ_CONTABLE.USER_ACT%TYPE DEFAULT NULL,
                       p_FEC_ACT         IN EJECUCION.T_MATRIZ_CONTABLE.FEC_ACT%TYPE DEFAULT NULL,
                       p_MSG             IN OUT VARCHAR2) IS

        v_VERIFICAR_GTO NUMBER(10);
        v_VERIFICAR_REC NUMBER(10);

    BEGIN

        v_VERIFICAR_GTO := 0;
        v_VERIFICAR_REC := 0;
        p_MSG           := NULL;

        --Gali 20216 controla que no exista ya cargada la partida gasto/recurso en la MatrizCtable

        IF p_ID_OBJETO_GASTO IS NOT NULL THEN
            SELECT COUNT(*)
              INTO v_VERIFICAR_GTO
              from EJECUCION.t_matriz_contable mc
             where mc.id_objeto_gasto = p_ID_OBJETO_GASTO
               and mc.id_cta_contable = p_ID_CTA_CONTABLE;
        END IF;

        IF p_ID_RECURSO IS NOT NULL THEN
            SELECT COUNT(*)
              INTO v_VERIFICAR_REC
              from EJECUCION.t_matriz_contable mc
             where mc.id_recurso = p_ID_RECURSO;
        END IF;

        IF v_VERIFICAR_GTO > 0 THEN
            p_MSG := 'Existe Partida de Gasto YA REGISTRADA en la MATRIZ CONTABLE.';
        ELSE
            IF v_VERIFICAR_REC > 0 THEN
                p_MSG := 'Existe Partida de Recurso YA REGISTRADA en la MATRIZ CONTABLE.';
            ELSE

                SELECT EJECUCION.SEQ_MATRIZ_CBLE.NEXTVAL
                  INTO p_ID_MATRIZ_CBLE
                  FROM DUAL;

                INSERT INTO EJECUCION.T_MATRIZ_CONTABLE
                    (ID_MATRIZ_CBLE,
                     ID_CTA_CONTABLE,
                     ID_OBJETO_GASTO,
                     ID_RECURSO,
                     ID_VIGENCIA,
                     USER_ING,
                     FEC_ING,
                     USER_ACT,
                     FEC_ACT)
                VALUES
                    (p_ID_MATRIZ_CBLE,
                     p_ID_CTA_CONTABLE,
                     p_ID_OBJETO_GASTO,
                     p_ID_RECURSO,
                     p_ID_VIGENCIA,
                     p_USER_ING,
                     p_FEC_ING,
                     p_USER_ACT,
                     p_FEC_ACT);
            END IF;
        END IF;

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

    PROCEDURE EDITAR(p_ID_MATRIZ_CBLE  IN EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE%TYPE DEFAULT NULL,
                     p_ID_CTA_CONTABLE IN EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                     p_ID_OBJETO_GASTO IN EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO%TYPE DEFAULT NULL,
                     p_ID_RECURSO      IN EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO%TYPE DEFAULT NULL,
                     p_ID_VIGENCIA     IN EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA%TYPE DEFAULT NULL,
                     p_USER_ING        IN EJECUCION.T_MATRIZ_CONTABLE.USER_ING%TYPE DEFAULT NULL,
                     p_FEC_ING         IN EJECUCION.T_MATRIZ_CONTABLE.FEC_ING%TYPE DEFAULT NULL,
                     p_USER_ACT        IN EJECUCION.T_MATRIZ_CONTABLE.USER_ACT%TYPE DEFAULT NULL,
                     p_FEC_ACT         IN EJECUCION.T_MATRIZ_CONTABLE.FEC_ACT%TYPE DEFAULT NULL,
                     p_MSG             IN OUT varchar2) IS
    BEGIN
        UPDATE EJECUCION.T_MATRIZ_CONTABLE
           SET ID_CTA_CONTABLE = p_ID_CTA_CONTABLE,
               ID_OBJETO_GASTO = p_ID_OBJETO_GASTO,
               ID_RECURSO      = p_ID_RECURSO,
               ID_VIGENCIA     = p_ID_VIGENCIA,
               USER_ING        = p_USER_ING,
               FEC_ING         = p_FEC_ING,
               USER_ACT        = p_USER_ACT,
               FEC_ACT         = sysdate
         WHERE ID_MATRIZ_CBLE = p_ID_MATRIZ_CBLE;

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

    PROCEDURE ELIMINAR(p_ID_MATRIZ_CBLE IN EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE%TYPE DEFAULT NULL,
                       p_MSG            IN OUT varchar2) IS
    BEGIN
        DELETE FROM EJECUCION.T_MATRIZ_CONTABLE
         WHERE ID_MATRIZ_CBLE = p_ID_MATRIZ_CBLE;

    exception
        when v_error_2292 then
            p_MSG := 'No se puede Eliminar el registro existen relaciones para ese ejercicio asociadas a otras tablas.';
            pkg_util.base_rollback;
        when others then
            p_MSG := 'Error al ELIMINAR. Verifique: ' || sqlerrm;
            pkg_util.base_rollback;

    END ELIMINAR;

    PROCEDURE BUSCARPORPK(p_T_MATRIZ_CONTABLE OUT ty_CURSOR,
                          p_ID_MATRIZ_CBLE    IN EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE%TYPE DEFAULT NULL) IS
    BEGIN
        OPEN p_T_MATRIZ_CONTABLE FOR
            SELECT EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE,
                   EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE,
                   EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO,
                   EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO,
                   EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA,
                   EJECUCION.T_MATRIZ_CONTABLE.USER_ING,
                   EJECUCION.T_MATRIZ_CONTABLE.FEC_ING,
                   EJECUCION.T_MATRIZ_CONTABLE.USER_ACT,
                   EJECUCION.T_MATRIZ_CONTABLE.FEC_ACT
              FROM EJECUCION.T_MATRIZ_CONTABLE
             WHERE (EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE =
                   p_ID_MATRIZ_CBLE);
    END BUSCARPORPK;

    PROCEDURE BUSCARPOROBJETOGASTO(p_T_MATRIZ_CONTABLE OUT ty_CURSOR,
                                   p_ID_OBJETO_GASTO   IN EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO%TYPE DEFAULT NULL) IS
    BEGIN
        OPEN p_T_MATRIZ_CONTABLE FOR
            SELECT *
              FROM (SELECT EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE,
                           EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE,
                           EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO,
                           EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO,
                           EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA,
                           EJECUCION.T_MATRIZ_CONTABLE.USER_ING,
                           EJECUCION.T_MATRIZ_CONTABLE.FEC_ING,
                           EJECUCION.T_MATRIZ_CONTABLE.USER_ACT,
                           EJECUCION.T_MATRIZ_CONTABLE.FEC_ACT
                      FROM EJECUCION.T_MATRIZ_CONTABLE
                     WHERE (EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO =
                           p_ID_OBJETO_GASTO)
                     ORDER BY 1 DESC)
             WHERE ROWNUM = 1;
    END BUSCARPOROBJETOGASTO;

    PROCEDURE BUSCAR(p_T_MATRIZ_CONTABLE   OUT ty_CURSOR,
                     p_ID_MATRIZ_CBLE      IN EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE%TYPE DEFAULT NULL,
                     p_ID_CTA_CONTABLE     IN EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                     p_ID_OBJETO_GASTO     IN EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO%TYPE DEFAULT NULL,
                     p_ID_RECURSO          IN EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO%TYPE DEFAULT NULL,
                     p_ID_VIGENCIA         IN EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA%TYPE DEFAULT NULL,
                     p_USER_ING            IN EJECUCION.T_MATRIZ_CONTABLE.USER_ING%TYPE DEFAULT NULL,
                     p_FEC_ING             IN EJECUCION.T_MATRIZ_CONTABLE.FEC_ING%TYPE DEFAULT NULL,
                     p_USER_ACT            IN EJECUCION.T_MATRIZ_CONTABLE.USER_ACT%TYPE DEFAULT NULL,
                     p_FEC_ACT             IN EJECUCION.T_MATRIZ_CONTABLE.FEC_ACT%TYPE DEFAULT NULL,
                     p_PAGINA              IN NUMBER DEFAULT NULL,
                     p_TAMANOPAGINA        IN NUMBER DEFAULT NULL,
                     p_CANTIDADDEREGISTROS OUT NUMBER

                     ) IS
        /*Declaracion de variales*/
        var_ID_MATRIZ_CBLE  EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE%TYPE DEFAULT NULL;
        var_ID_CTA_CONTABLE EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE%TYPE DEFAULT NULL;
        var_ID_OBJETO_GASTO EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO%TYPE DEFAULT NULL;
        var_ID_RECURSO      EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO%TYPE DEFAULT NULL;
        var_ID_VIGENCIA     EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA%TYPE DEFAULT NULL;
        var_USER_ING        EJECUCION.T_MATRIZ_CONTABLE.USER_ING%TYPE DEFAULT NULL;
        var_FEC_ING         EJECUCION.T_MATRIZ_CONTABLE.FEC_ING%TYPE DEFAULT NULL;
        var_USER_ACT        EJECUCION.T_MATRIZ_CONTABLE.USER_ACT%TYPE DEFAULT NULL;
        var_FEC_ACT         EJECUCION.T_MATRIZ_CONTABLE.FEC_ACT%TYPE DEFAULT NULL;
        var_PAGINA          NUMBER DEFAULT NULL;
        var_TAMANOPAGINA    NUMBER DEFAULT NULL;

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

        /*Asignacion de parametros a variables*/

        var_ID_MATRIZ_CBLE  := p_ID_MATRIZ_CBLE;
        var_ID_CTA_CONTABLE := p_ID_CTA_CONTABLE;
        var_ID_OBJETO_GASTO := p_ID_OBJETO_GASTO;
        var_ID_RECURSO      := p_ID_RECURSO;
        var_ID_VIGENCIA     := p_ID_VIGENCIA;
        var_USER_ING        := p_USER_ING;
        var_FEC_ING         := p_FEC_ING;
        var_USER_ACT        := p_USER_ACT;
        var_FEC_ACT         := p_FEC_ACT;
        var_PAGINA          := p_PAGINA;
        var_TAMANOPAGINA    := p_TAMANOPAGINA;

        BEGIN
            v_SELECT_COUNT := 'SELECT COUNT(*) ';
            v_FROM_COUNT   := 'FROM EJECUCION.T_MATRIZ_CONTABLE ';
            v_WHERE_COUNT  := '';
            v_AND          := 'WHERE';

            IF var_ID_MATRIZ_CBLE IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE = ' ||
                                 var_ID_MATRIZ_CBLE;
                v_AND         := ' AND';
            END IF;

            IF var_ID_CTA_CONTABLE IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE = ' ||
                                 var_ID_CTA_CONTABLE;
                v_AND         := ' AND';
            END IF;

            IF var_ID_OBJETO_GASTO IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO = ' ||
                                 var_ID_OBJETO_GASTO;
                v_AND         := ' AND';
            END IF;

            IF var_ID_RECURSO IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO = ' ||
                                 var_ID_RECURSO;
                v_AND         := ' AND';
            END IF;

            IF var_ID_VIGENCIA IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA = ' ||
                                 var_ID_VIGENCIA;
                v_AND         := ' AND';
            END IF;

            IF var_USER_ING IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.USER_ING LIKE ''%' ||
                                 var_USER_ING || '%''';
                v_AND         := ' AND';
            END IF;

            IF TO_DATE(var_FEC_ING, 'DD/MM/YYYY') IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.FEC_ING = ''' ||
                                 var_FEC_ING || '''';
                v_AND         := ' AND';
            END IF;

            IF var_USER_ACT IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.USER_ACT LIKE ''%' ||
                                 var_USER_ACT || '%''';
                v_AND         := ' AND';
            END IF;

            IF TO_DATE(var_FEC_ACT, 'DD/MM/YYYY') IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.FEC_ACT = ''' ||
                                 var_FEC_ACT || '''';
            END IF;

            v_CANTIDAD := v_SELECT_COUNT || v_FROM_COUNT || v_WHERE_COUNT;
        END;

        EXECUTE IMMEDIATE v_CANTIDAD
            INTO p_CANTIDADDEREGISTROS;

        BEGIN
            IF var_PAGINA IS NULL THEN
                v_INICIO := 0;
                v_FIN    := p_CANTIDADDEREGISTROS;
            ELSE
                IF var_PAGINA = 1 THEN
                    v_INICIO := 0;
                    v_FIN    := v_INICIO + var_TAMANOPAGINA;
                ELSE
                    v_INICIO := (var_PAGINA - 1) * var_TAMANOPAGINA;
                    v_FIN    := v_INICIO + var_TAMANOPAGINA;
                END IF;
            END IF;
        END;

        BEGIN
            v_SELECT_PAGINA := ' SELECT * FROM (SELECT V.* , ROWNUM  NUMERODEREGISTRO FROM (';
            v_SELECT        := 'SELECT ' ||
                               'EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE, ' ||
                               'EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE, ' ||
                               'EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO, ' ||
                               'EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO, ' ||
                               'EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA, ' ||
                               'EJECUCION.T_MATRIZ_CONTABLE.USER_ING, ' ||
                               'EJECUCION.T_MATRIZ_CONTABLE.FEC_ING, ' ||
                               'EJECUCION.T_MATRIZ_CONTABLE.USER_ACT, ' ||
                               'EJECUCION.T_MATRIZ_CONTABLE.FEC_ACT ';

            v_SUB_CONSULTA := v_SELECT || v_FROM_COUNT || v_WHERE_COUNT ||
                              ' ORDER BY 1'; --Falta colocar columna de Ordenamiento
            v_WHERE_PAGINA := ' WHERE NUMERODEREGISTRO > ' || v_INICIO || '' ||
                              ' AND NUMERODEREGISTRO <= ' || v_FIN || '';

            v_BUSCAR := v_SELECT_PAGINA || v_SUB_CONSULTA || ')V )' ||
                        v_WHERE_PAGINA || ' ORDER BY 1'; --Falta colocar columna de Ordenamiento

        END;

        OPEN p_T_MATRIZ_CONTABLE FOR v_BUSCAR;
            insert into pruebas
           values
               (v_BUSCAR, sysdate, 'FER');
        commit;
    END BUSCAR;

    PROCEDURE REPORTE_MATRIZ_CONTABLE(p_T_MATRIZ_CONTABLE OUT ty_CURSOR,
                                      p_ID_VIGENCIA       IN EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA%TYPE DEFAULT NULL) IS
    BEGIN

        OPEN p_T_MATRIZ_CONTABLE FOR
            SELECT CC.ID_CTA_CONTABLE, OG.ID_OBJETO_GASTO, RE.ID_RECURSO --CC.CODIGO ||'-'|| CC.NOMBRE CUENTACONTABLE, OG.NUMERO_OBJETO || '-' || OG.NOMBRE_OBJETO OBJETOGASTO, RE.NUMERO_RECURSO || '-' || RE.NOMBRE_RECURSO RECURSO
              FROM T_MATRIZ_CONTABLE           MC,
                   T_CUENTAS_CONTABLES         CC,
                   PRESUPUESTO.T_OBJETOS_GASTO OG,
                   PRESUPUESTO.T_RECURSOS      RE
             WHERE MC.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE(+)
               AND MC.ID_OBJETO_GASTO = OG.ID_OBJETO_GASTO(+)
               AND MC.ID_RECURSO = RE.ID_RECURSO(+)
               AND MC.ID_VIGENCIA = p_ID_VIGENCIA;

    END REPORTE_MATRIZ_CONTABLE;

    PROCEDURE BUSCAR_DATOS_MATRIZ(p_T_MATRIZ_CONTABLE   OUT ty_CURSOR,
                                  p_ID_CTA_CONTABLE     IN EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                                  p_ID_VIGENCIA         IN EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA%TYPE DEFAULT NULL,
                                  p_ID_OBJETO_GASTO     IN EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO%TYPE DEFAULT NULL,
                                  p_ID_RECURSO          IN EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO%TYPE DEFAULT NULL,
                                  p_id_jurisdiccion     in ejecucion.t_cuentas_contables.id_jurisdiccion%type default null,
                                  p_PAGINA              IN NUMBER DEFAULT NULL,
                                  p_TAMANOPAGINA        IN NUMBER DEFAULT NULL,
                                  p_CANTIDADDEREGISTROS OUT NUMBER

                                  ) IS
        /*Declaracion de variables*/
        var_ID_CTA_CONTABLE EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE%TYPE DEFAULT NULL;
        var_ID_VIGENCIA     EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA%TYPE DEFAULT NULL;
        var_ID_OBJETO_GASTO EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO%TYPE DEFAULT NULL;
        var_ID_RECURSO      EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO%TYPE DEFAULT NULL;
        VAR_ID_JURISDICCION EJECUCION.T_CUENTAS_CONTABLES.ID_JURISDICCION%TYPE DEFAULT NULL;
        var_PAGINA          NUMBER DEFAULT NULL;
        var_TAMANOPAGINA    NUMBER DEFAULT NULL;

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
        /*Asignacion de parametros a variables*/
        var_ID_CTA_CONTABLE := p_ID_CTA_CONTABLE;
        var_ID_VIGENCIA     := p_ID_VIGENCIA;
        var_ID_OBJETO_GASTO := p_ID_OBJETO_GASTO;
        var_ID_RECURSO      := p_ID_RECURSO;
        var_PAGINA          := p_PAGINA;
        var_TAMANOPAGINA    := p_TAMANOPAGINA;
        var_ID_JURISDICCION := p_ID_JURISDICCION;

        BEGIN
            v_SELECT_COUNT := 'SELECT COUNT(*) ';
            v_FROM_COUNT   := 'FROM EJECUCION.T_MATRIZ_CONTABLE, EJECUCION.T_CUENTAS_CONTABLES, PRESUPUESTO.T_OBJETOS_GASTO,PRESUPUESTO.T_RECURSOS  ';
            v_WHERE_COUNT  := ' ';
            v_AND          := 'WHERE';

            IF var_ID_JURISDICCION IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_CUENTAS_CONTABLES.ID_JURISDICCION IN (
                                  SELECT J2.ID_JURISDICCION
                                         FROM PRESUPUESTO.T_JURISDICCIONES J1 JOIN
                                              PRESUPUESTO.T_JURISDICCIONES J2
                                              ON J2.NUMERO_JURISDICCION = J1.NUMERO_JURISDICCION
                                              WHERE J1.ID_JURISDICCION = ' ||  var_ID_JURISDICCION || ' )' ;
                v_AND         := ' AND';
            END IF;

            IF var_ID_CTA_CONTABLE IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE = ' ||
                                 var_ID_CTA_CONTABLE;
                v_AND         := ' AND';
            END IF;

            IF var_ID_RECURSO IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO = ' ||
                                 var_ID_RECURSO;
                v_AND         := ' AND';
            END IF;

            IF var_ID_OBJETO_GASTO IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO = ' ||
                                 var_ID_OBJETO_GASTO;
                v_AND         := ' AND';
            END IF;

            IF var_ID_VIGENCIA IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE = EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE (+)
                  AND EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO = PRESUPUESTO.T_OBJETOS_GASTO.ID_OBJETO_GASTO (+)
                  AND EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO = PRESUPUESTO.T_RECURSOS.ID_RECURSO (+)
                  AND EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA = ' ||
                                 var_ID_VIGENCIA;
                v_AND         := ' AND';
            END IF;

            v_CANTIDAD := v_SELECT_COUNT || v_FROM_COUNT || v_WHERE_COUNT;
        END;

        EXECUTE IMMEDIATE v_CANTIDAD
            INTO p_CANTIDADDEREGISTROS;

        BEGIN
            IF var_PAGINA IS NULL THEN
                v_INICIO := 0;
                v_FIN    := p_CANTIDADDEREGISTROS;
            ELSE
                IF var_PAGINA = 1 THEN
                    v_INICIO := 0;
                    v_FIN    := v_INICIO + var_TAMANOPAGINA;
                ELSE
                    v_INICIO := (var_PAGINA - 1) * var_TAMANOPAGINA;
                    v_FIN    := v_INICIO + var_TAMANOPAGINA;
                END IF;
            END IF;
        END;

        BEGIN
            v_SELECT_PAGINA := ' SELECT * FROM (SELECT V.* , ROWNUM  NUMERODEREGISTRO FROM (';
            v_SELECT        := 'SELECT ' ||
                               'EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.CODIGO, ' ||
                               'EJECUCION.T_CUENTAS_CONTABLES.NOMBRE, ' ||
                               'PRESUPUESTO.T_OBJETOS_GASTO.NUMERO_OBJETO, ' ||
                               'PRESUPUESTO.T_OBJETOS_GASTO.NOMBRE_OBJETO, ' ||
                               'PRESUPUESTO.T_RECURSOS.NUMERO_RECURSO, ' ||
                               'PRESUPUESTO.T_RECURSOS.NOMBRE_RECURSO ';

            v_SUB_CONSULTA := v_SELECT || v_FROM_COUNT || v_WHERE_COUNT ||
                              ' ORDER BY 1'; --Falta colocar columna de Ordenamiento
            v_WHERE_PAGINA := ' WHERE NUMERODEREGISTRO > ' || v_INICIO || '' ||
                              ' AND NUMERODEREGISTRO <= ' || v_FIN || '';

            v_BUSCAR := v_SELECT_PAGINA || v_SUB_CONSULTA || ')V )' ||
                        v_WHERE_PAGINA || ' ORDER BY 1'; --Falta colocar columna de Ordenamiento

        END;

        OPEN p_T_MATRIZ_CONTABLE FOR v_BUSCAR;

        insert into pruebas
        values
            (v_BUSCAR, sysdate, 'BUSCAR_DATOS_MATRIZ');
        commit;

    END BUSCAR_DATOS_MATRIZ;

    PROCEDURE EXISTE_PARTIDA
    --Gali 20216 controla que no exista ya cargada la partida gasto en la MatrizCtable
    (p_ID_PARTIDA NUMBER, p_TIPO VARCHAR2, p_CANTIDAD IN OUT NUMBER) IS
        v_VERIFICAR NUMBER(10);
    BEGIN

        v_VERIFICAR := 0;

        IF p_TIPO = 'G' THEN
            SELECT COUNT(*)
              INTO v_VERIFICAR
              from EJECUCION.t_matriz_contable mc
             where mc.id_objeto_gasto = p_ID_PARTIDA;
        ELSE
            IF p_TIPO = 'R' THEN
                SELECT COUNT(*)
                  INTO v_VERIFICAR
                  from EJECUCION.t_matriz_contable mc
                 where mc.id_recurso = p_ID_PARTIDA;
            END IF;
        END IF;

        p_CANTIDAD := v_VERIFICAR;

    END EXISTE_PARTIDA;

    PROCEDURE IMPORTA_RELACION(p_EJERCICIO NUMBER, p_MSG IN OUT varchar2) IS

        v_VIGENCIA_ANT     number;
        v_VIGENCIA_NEW     number;
        v_EXISTEN_DATOS    NUMBER;
        v_EXISTEN_RECURSOS NUMBER;
        v_EXISTEN_OG       NUMBER;
        v_CANTIDAD         NUMBER;

    BEGIN

        p_MSG := '';

        select v.id_vigencia
          into v_VIGENCIA_NEW
          from PRESUPUESTO.t_vigencias v
         where v.ejercicio = p_EJERCICIO;

        select v.id_vigencia
          into v_VIGENCIA_ANT
          from PRESUPUESTO.t_vigencias v
         where v.ejercicio = (p_EJERCICIO - 1);

        select count(*)
          INTO v_EXISTEN_DATOS
          from EJECUCION.t_matriz_contable mc
         where mc.id_vigencia = v_VIGENCIA_new;

        select count(*) + 1
          INTO v_EXISTEN_RECURSOS
          from PRESUPUESTO.t_recursos rec
         where rec.id_vigencia = v_VIGENCIA_new;

        select count(*)
          INTO v_EXISTEN_OG
          from PRESUPUESTO.t_objetos_gasto og
         where og.id_vigencia = v_VIGENCIA_NEW;

        if v_EXISTEN_RECURSOS > 0 then
            if v_EXISTEN_OG > 0 then
                if v_EXISTEN_DATOS = 0 then

                    insert into EJECUCION.t_matriz_contable mc

                        select EJECUCION.seq_matriz_cble.nextval AS ID_MATRIZ_CBLE,
                               r.id_cta_contable,
                               (select og1.id_objeto_gasto
                                  from PRESUPUESTO.t_objetos_gasto og1
                                 where og1.numero_objeto in
                                       (select og.numero_objeto
                                          from PRESUPUESTO.t_objetos_gasto og
                                         where og.id_objeto_gasto =
                                               r.id_objeto_gasto
                                           and og.id_vigencia =
                                               v_VIGENCIA_ANT)
                                   and og1.id_vigencia = v_VIGENCIA_NEW) as ID_OBJETO_GASTO,
                               (select rec.id_recurso
                                  from PRESUPUESTO.t_recursos rec
                                 where rec.numero_recurso in
                                       (select rec2.numero_recurso
                                          from PRESUPUESTO.t_recursos rec2
                                         where rec2.id_recurso = r.id_recurso
                                           and rec2.id_vigencia =
                                               v_VIGENCIA_ANT)
                                   and rec.id_vigencia = v_VIGENCIA_NEW) as ID_RECURSO,
                               v_VIGENCIA_NEW as ID_VIGENCIA,
                               r.User_Ing as USER_ING,
                               SYSDATE as FEC_ING,
                               r.User_Act as USER_ACT,
                               sysdate as FEC_ACT
                          from EJECUCION.t_matriz_contable r
                         where r.id_vigencia = v_VIGENCIA_ANT
                         and r.id_recurso is null;

                    select count(*)
                      INTO v_CANTIDAD
                      from EJECUCION.t_matriz_contable mc
                     where mc.id_vigencia = v_VIGENCIA_NEW;

                    p_msg := 'Se insertaron: ' || to_char(v_cantidad) ||
                             ' registros en la Matriz Contable ';

                else
                    p_MSG := 'Ya EXISTEN relaciones para la vigencia seleccionada. Verifique';
                end if;
            else
                p_MSG := 'NO EXISTEN OBJETOS DE GASTOS para la vigencia seleccionada. Verifique';
            end if;
        else
            p_MSG := 'NO EXISTEN RECURSOS para la vigencia seleccionada. Verifique';
        end if;

    EXCEPTION
        WHEN OTHERS THEN
            p_MSG := 'Error en EJECUCION.PKG_T_MATRIZ_CONTABLE.IMPORTA_RELACION. Verifique: ' ||
                     sqlerrm;

    END IMPORTA_RELACION;

END PKG_T_MATRIZ_CONTABLE;
PACKAGE "PKG_T_MATRIZ_CONTABLE" AS
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
    PROCEDURE INSERTAR(p_ID_MATRIZ_CBLE  OUT EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE%TYPE,
                       p_ID_CTA_CONTABLE IN EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                       p_ID_OBJETO_GASTO IN EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO%TYPE DEFAULT NULL,
                       p_ID_RECURSO      IN EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO%TYPE DEFAULT NULL,
                       p_ID_VIGENCIA     IN EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA%TYPE DEFAULT NULL,
                       p_USER_ING        IN EJECUCION.T_MATRIZ_CONTABLE.USER_ING%TYPE DEFAULT NULL,
                       p_FEC_ING         IN EJECUCION.T_MATRIZ_CONTABLE.FEC_ING%TYPE DEFAULT NULL,
                       p_USER_ACT        IN EJECUCION.T_MATRIZ_CONTABLE.USER_ACT%TYPE DEFAULT NULL,
                       p_FEC_ACT         IN EJECUCION.T_MATRIZ_CONTABLE.FEC_ACT%TYPE DEFAULT NULL,
                       p_MSG             IN OUT VARCHAR2);
    PROCEDURE EDITAR(p_ID_MATRIZ_CBLE  IN EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE%TYPE DEFAULT NULL,
                     p_ID_CTA_CONTABLE IN EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                     p_ID_OBJETO_GASTO IN EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO%TYPE DEFAULT NULL,
                     p_ID_RECURSO      IN EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO%TYPE DEFAULT NULL,
                     p_ID_VIGENCIA     IN EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA%TYPE DEFAULT NULL,
                     p_USER_ING        IN EJECUCION.T_MATRIZ_CONTABLE.USER_ING%TYPE DEFAULT NULL,
                     p_FEC_ING         IN EJECUCION.T_MATRIZ_CONTABLE.FEC_ING%TYPE DEFAULT NULL,
                     p_USER_ACT        IN EJECUCION.T_MATRIZ_CONTABLE.USER_ACT%TYPE DEFAULT NULL,
                     p_FEC_ACT         IN EJECUCION.T_MATRIZ_CONTABLE.FEC_ACT%TYPE DEFAULT NULL,
                     p_MSG             IN OUT varchar2);
    PROCEDURE ELIMINAR(p_ID_MATRIZ_CBLE IN EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE%TYPE DEFAULT NULL,
                       p_MSG            IN OUT varchar2);
    PROCEDURE BUSCARPORPK(p_T_MATRIZ_CONTABLE OUT ty_CURSOR,
                          p_ID_MATRIZ_CBLE    IN EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE%TYPE DEFAULT NULL);

    PROCEDURE BUSCARPOROBJETOGASTO(p_T_MATRIZ_CONTABLE OUT ty_CURSOR,
                                   p_ID_OBJETO_GASTO   IN EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO%TYPE DEFAULT NULL);

    PROCEDURE BUSCAR(p_T_MATRIZ_CONTABLE   OUT ty_CURSOR,
                     p_ID_MATRIZ_CBLE      IN EJECUCION.T_MATRIZ_CONTABLE.ID_MATRIZ_CBLE%TYPE DEFAULT NULL,
                     p_ID_CTA_CONTABLE     IN EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                     p_ID_OBJETO_GASTO     IN EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO%TYPE DEFAULT NULL,
                     p_ID_RECURSO          IN EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO%TYPE DEFAULT NULL,
                     p_ID_VIGENCIA         IN EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA%TYPE DEFAULT NULL,
                     p_USER_ING            IN EJECUCION.T_MATRIZ_CONTABLE.USER_ING%TYPE DEFAULT NULL,
                     p_FEC_ING             IN EJECUCION.T_MATRIZ_CONTABLE.FEC_ING%TYPE DEFAULT NULL,
                     p_USER_ACT            IN EJECUCION.T_MATRIZ_CONTABLE.USER_ACT%TYPE DEFAULT NULL,
                     p_FEC_ACT             IN EJECUCION.T_MATRIZ_CONTABLE.FEC_ACT%TYPE DEFAULT NULL,
                     p_PAGINA              IN NUMBER DEFAULT NULL,
                     p_TAMANOPAGINA        IN NUMBER DEFAULT NULL,
                     p_CANTIDADDEREGISTROS OUT NUMBER);
    PROCEDURE REPORTE_MATRIZ_CONTABLE(p_T_MATRIZ_CONTABLE OUT ty_CURSOR,
                                      p_ID_VIGENCIA       IN EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA%TYPE DEFAULT NULL);

    PROCEDURE BUSCAR_DATOS_MATRIZ(p_T_MATRIZ_CONTABLE   OUT ty_CURSOR,
                                  p_ID_CTA_CONTABLE     IN EJECUCION.T_MATRIZ_CONTABLE.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                                  p_ID_VIGENCIA         IN EJECUCION.T_MATRIZ_CONTABLE.ID_VIGENCIA%TYPE DEFAULT NULL,
                                  p_ID_OBJETO_GASTO     IN EJECUCION.T_MATRIZ_CONTABLE.ID_OBJETO_GASTO%TYPE DEFAULT NULL,
                                  p_ID_RECURSO          IN EJECUCION.T_MATRIZ_CONTABLE.ID_RECURSO%TYPE DEFAULT NULL,
                                  p_id_jurisdiccion     in ejecucion.t_cuentas_contables.id_jurisdiccion%type default null,
                                  p_PAGINA              IN NUMBER DEFAULT NULL,
                                  p_TAMANOPAGINA        IN NUMBER DEFAULT NULL,
                                  p_CANTIDADDEREGISTROS OUT NUMBER);

    PROCEDURE EXISTE_PARTIDA
    --Gali 20216 controla que no exista ya cargada la partida gasto en la MatrizCtable
    (p_ID_PARTIDA NUMBER, p_TIPO VARCHAR2, p_CANTIDAD IN OUT NUMBER);

    PROCEDURE IMPORTA_RELACION(p_EJERCICIO NUMBER, p_MSG IN OUT varchar2);

END PKG_T_MATRIZ_CONTABLE;
