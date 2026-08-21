PACKAGE BODY "PKG_T_ASIENTOS_DET" AS
  PROCEDURE INSERTAR_DET(p_ID_ASIENTO_DET     OUT EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE,
                         p_ID_ASIENTO         IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO%TYPE DEFAULT NULL,
                         p_ID_CTA_CONTABLE    IN EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                         p_FEC_CONTABLE       IN EJECUCION.T_ASIENTOS_DET.FEC_CONTABLE%TYPE DEFAULT NULL,
                         p_DEBE               IN EJECUCION.T_ASIENTOS_DET.DEBE%TYPE DEFAULT NULL,
                         p_HABER              IN EJECUCION.T_ASIENTOS_DET.HABER%TYPE DEFAULT NULL,
                         p_ID_RUR_DET         IN EJECUCION.T_ASIENTOS_DET.ID_RUR_DET%TYPE DEFAULT NULL,
                         p_ID_DEVENGADODET    IN EJECUCION.T_ASIENTOS_DET.ID_DEVENGADODET%TYPE DEFAULT NULL,
                         p_ID_DIETDAC_DET     IN EJECUCION.T_ASIENTOS_DET.ID_DIETDAC_DET%TYPE DEFAULT NULL,
                         p_ID_MOV_TESO_DET    IN EJECUCION.T_ASIENTOS_DET.ID_MOV_TESO_DET%TYPE DEFAULT NULL,
                         p_ID_ELEMENTO_PAGO   IN EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO%TYPE DEFAULT NULL,
                         p_USER_ING           IN EJECUCION.T_ASIENTOS_DET.USER_ING%TYPE DEFAULT NULL,
                         p_FEC_ING            IN EJECUCION.T_ASIENTOS_DET.FEC_ING%TYPE DEFAULT NULL,
                         p_USER_ACT           IN EJECUCION.T_ASIENTOS_DET.USER_ACT%TYPE DEFAULT NULL,
                         p_FEC_ACT            IN EJECUCION.T_ASIENTOS_DET.FEC_ACT%TYPE DEFAULT NULL,
                         p_ANULADO            IN EJECUCION.T_ASIENTOS_DET.ANULADO%TYPE DEFAULT NULL,
                         p_ID_RECURSO         IN EJECUCION.T_ASIENTOS_DET.ID_RECURSO_D_H%TYPE DEFAULT NULL,
                         p_ID_AUXILIAR        IN EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                         p_id_cuenta_bancaria in EJECUCION.T_ASIENTOS_DET.id_cuenta_bancaria%TYPE DEFAULT NULL,
                         p_MSG                IN OUT VARCHAR2) IS
    V_FEC_CONTABLE       varchar2(20);
    p_ID_LIBRO_BANCO     number(10);
    v_ID_ASIENTO         EJECUCION.T_ASIENTOS_DET.ID_ASIENTO%TYPE;
    v_ID_CTA_CONTABLE    EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE%TYPE;
    v_DEBE               EJECUCION.T_ASIENTOS_DET.DEBE%TYPE;
    v_HABER              EJECUCION.T_ASIENTOS_DET.HABER%TYPE;
    v_ID_RUR_DET         EJECUCION.T_ASIENTOS_DET.ID_RUR_DET%TYPE;
    v_ID_DEVENGADODET    EJECUCION.T_ASIENTOS_DET.ID_DEVENGADODET%TYPE;
    v_ID_DIETDAC_DET     EJECUCION.T_ASIENTOS_DET.ID_DIETDAC_DET%TYPE;
    v_ID_MOV_TESO_DET    EJECUCION.T_ASIENTOS_DET.ID_MOV_TESO_DET%TYPE;
    v_ID_ELEMENTO_PAGO   EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO%TYPE;
    v_USER_ING           EJECUCION.T_ASIENTOS_DET.USER_ING%TYPE;
    v_FEC_ING            EJECUCION.T_ASIENTOS_DET.FEC_ING%TYPE;
    v_USER_ACT           EJECUCION.T_ASIENTOS_DET.USER_ACT%TYPE;
    v_FEC_ACT            EJECUCION.T_ASIENTOS_DET.FEC_ACT%TYPE;
    v_ANULADO            EJECUCION.T_ASIENTOS_DET.ANULADO%TYPE;
    v_ID_RECURSO         EJECUCION.T_ASIENTOS_DET.ID_RECURSO_D_H%TYPE;
    V_id_cuenta_bancaria EJECUCION.T_ASIENTOS_DET.id_cuenta_bancaria%TYPE;
    /*\*   v_ID_AUXILIAR  EJECUCION.T_ASIENTOS_DET.ID_AUXILIAR%TYPE;*\*/

  BEGIN

    v_ID_ASIENTO         := p_ID_ASIENTO;
    v_ID_CTA_CONTABLE    := p_ID_CTA_CONTABLE;
    v_DEBE               := p_DEBE;
    v_HABER              := p_HABER;
    v_ID_RUR_DET         := p_ID_RUR_DET;
    v_ID_DEVENGADODET    := p_ID_DEVENGADODET;
    v_ID_DIETDAC_DET     := p_ID_DIETDAC_DET;
    v_ID_MOV_TESO_DET    := p_ID_MOV_TESO_DET;
    v_ID_ELEMENTO_PAGO   := p_ID_ELEMENTO_PAGO;
    v_USER_ING           := p_USER_ING;
    v_FEC_ING            := p_FEC_ING;
    v_USER_ACT           := p_USER_ACT;
    v_FEC_ACT            := p_FEC_ACT;
    v_FEC_ACT            := p_FEC_ACT;
    v_ID_RECURSO         := p_ID_RECURSO;
    v_ANULADO            := p_ANULADO;
    V_id_cuenta_bancaria := p_id_cuenta_bancaria;
    --  v_ID_AUXILIAR  :=p_ID_AUXILIAR;

    SELECT SQ_T_ASIENTOS_DETALLES.NEXTVAL INTO p_ID_ASIENTO_DET FROM DUAL;

    SELECT A.FEC_CONTABLE
      INTO V_FEC_CONTABLE
      FROM EJECUCION.T_ASIENTOS A
     WHERE A.ID_ASIENTO = v_ID_ASIENTO;

    INSERT INTO EJECUCION.T_ASIENTOS_DET
      (ID_ASIENTO_DET,
       ID_ASIENTO,
       ID_CTA_CONTABLE,
       FEC_CONTABLE,
       DEBE,
       HABER,
       ID_RUR_DET,
       ID_DEVENGADODET,
       ID_DIETDAC_DET,
       ID_MOV_TESO_DET,
       ID_ELEMENTO_PAGO,
       USER_ING,
       FEC_ING,
       USER_ACT,
       FEC_ACT,
       ANULADO,
       ID_RECURSO_D_H,
       id_cuenta_bancaria /*\*,
                             ID_AUXILIAR*\*/)
    VALUES
      (p_ID_ASIENTO_DET,
       v_ID_ASIENTO,
       v_ID_CTA_CONTABLE,
       V_FEC_CONTABLE,
       --        p_FEC_CONTABLE,
       v_DEBE,
       v_HABER,
       v_ID_RUR_DET,
       v_ID_DEVENGADODET,
       v_ID_DIETDAC_DET,
       v_ID_MOV_TESO_DET,
       v_ID_ELEMENTO_PAGO,
       v_USER_ING,
       v_FEC_ING,
       v_USER_ACT,
       v_FEC_ACT,
       v_ANULADO,
       v_ID_RECURSO,
       V_id_cuenta_bancaria /*\*,
                             v_ID_AUXILIAR*\*/);

    SELECT EJECUCION.SEQ_T_LIBRO_BANCO.NEXTVAL
      INTO p_ID_LIBRO_BANCO
      FROM DUAL;

    if V_id_cuenta_bancaria is not null then

      insert into t_libro_banco lb
        (id_libro_banco,
         id_cuenta_origen,
         id_cuenta_destino,
         fecha,
         id_tipo_ep,
         numero_ep,
         monto,
         user_ing,
         fec_ing,
         tipo_movimiento,
         id_tipo_doc,
         anulado,
         concepto,
         id_cuenta_contable)
      values
        (p_ID_LIBRO_BANCO,
         decode(nvl(v_DEBE, 0), 0, null, v_id_cuenta_bancaria),
         decode(nvl(v_DEBE, 0), 0, v_id_cuenta_bancaria, null),
         V_FEC_CONTABLE,
         15,
         '0',
         decode(nvl(v_DEBE, 0), 0, v_HABER, v_DEBE),
         v_USER_ING,
         v_FEC_ING,
         decode(nvl(v_DEBE, 0), 0, 'H', 'D'),
         15,
         'N',
         'GENERADO A PARTIR DE ASIENTO',
         v_ID_CTA_CONTABLE);

      update EJECUCION.T_ASIENTOS_DET AD
         SET AD.ID_LIBRO_BANCO = p_ID_LIBRO_BANCO
       WHERE AD.ID_ASIENTO_DET = p_ID_ASIENTO_DET;

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

  END INSERTAR_DET;
  /*
  PROCEDURE EDITAR
  (
      p_ID_ASIENTO_DET IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE DEFAULT NULL,
      p_ID_ASIENTO IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO%TYPE DEFAULT NULL,
      p_ID_CTA_CONTABLE IN EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
      p_FEC_CONTABLE IN EJECUCION.T_ASIENTOS_DET.FEC_CONTABLE%TYPE DEFAULT NULL,
      p_DEBE IN EJECUCION.T_ASIENTOS_DET.DEBE%TYPE DEFAULT NULL,
      p_HABER IN EJECUCION.T_ASIENTOS_DET.HABER%TYPE DEFAULT NULL,
      p_ID_RUR_DET IN EJECUCION.T_ASIENTOS_DET.ID_RUR_DET%TYPE DEFAULT NULL,
      p_ID_DEVENGADODET IN EJECUCION.T_ASIENTOS_DET.ID_DEVENGADODET%TYPE DEFAULT NULL,
      p_ID_DIETDAC_DET IN EJECUCION.T_ASIENTOS_DET.ID_DIETDAC_DET%TYPE DEFAULT NULL,
      p_ID_MOV_TESO_DET IN EJECUCION.T_ASIENTOS_DET.ID_MOV_TESO_DET%TYPE DEFAULT NULL,
      p_ID_ELEMENTO_PAGO IN EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO%TYPE DEFAULT NULL,
      p_USER_ING IN EJECUCION.T_ASIENTOS_DET.USER_ING%TYPE DEFAULT NULL,
      p_FEC_ING IN EJECUCION.T_ASIENTOS_DET.FEC_ING%TYPE DEFAULT NULL,
      p_USER_ACT IN EJECUCION.T_ASIENTOS_DET.USER_ACT%TYPE DEFAULT NULL,
      p_FEC_ACT IN EJECUCION.T_ASIENTOS_DET.FEC_ACT%TYPE DEFAULT NULL,
      p_ANULADO IN EJECUCION.T_ASIENTOS_DET.ANULADO%TYPE DEFAULT NULL
  ,p_MSG IN OUT varchar2
  )
  IS
      v_ID_ASIENTO_DET EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE;
      v_ID_ASIENTO  EJECUCION.T_ASIENTOS_DET.ID_ASIENTO%TYPE;
      v_ID_CTA_CONTABLE  EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE%TYPE;
      v_DEBE  EJECUCION.T_ASIENTOS_DET.DEBE%TYPE;
      v_HABER  EJECUCION.T_ASIENTOS_DET.HABER%TYPE;
      v_ID_RUR_DET  EJECUCION.T_ASIENTOS_DET.ID_RUR_DET%TYPE;
      v_ID_DEVENGADODET  EJECUCION.T_ASIENTOS_DET.ID_DEVENGADODET%TYPE;
      v_ID_DIETDAC_DET  EJECUCION.T_ASIENTOS_DET.ID_DIETDAC_DET%TYPE;
      v_ID_MOV_TESO_DET  EJECUCION.T_ASIENTOS_DET.ID_MOV_TESO_DET%TYPE ;
      v_ID_ELEMENTO_PAGO  EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO%TYPE;
      v_USER_ING  EJECUCION.T_ASIENTOS_DET.USER_ING%TYPE;
      v_FEC_ING  EJECUCION.T_ASIENTOS_DET.FEC_ING%TYPE;
      v_USER_ACT  EJECUCION.T_ASIENTOS_DET.USER_ACT%TYPE;
      v_FEC_ACT  EJECUCION.T_ASIENTOS_DET.FEC_ACT%TYPE;
      v_ANULADO  EJECUCION.T_ASIENTOS_DET.ANULADO%TYPE;
      v_FEC_CONTABLE EJECUCION.T_ASIENTOS_DET.FEC_CONTABLE%TYPE;

  BEGIN
      v_ID_ASIENTO_DET := p_ID_ASIENTO_DET;
      v_ID_ASIENTO := p_ID_ASIENTO;
      v_ID_CTA_CONTABLE  :=p_ID_CTA_CONTABLE;
      v_DEBE :=p_DEBE;
      v_HABER  :=p_HABER;
      v_ID_RUR_DET  :=p_ID_RUR_DET;
      v_ID_DEVENGADODET  :=p_ID_DEVENGADODET;
      v_ID_DIETDAC_DET  :=p_ID_DIETDAC_DET;
      v_ID_MOV_TESO_DET :=p_ID_MOV_TESO_DET;
      v_ID_ELEMENTO_PAGO :=p_ID_ELEMENTO_PAGO;
      v_USER_ING  :=p_USER_ING;
      v_FEC_ING  :=p_FEC_ING;
      v_USER_ACT  :=p_USER_ACT;
      v_FEC_ACT  :=p_FEC_ACT;
      v_FEC_ACT  :=p_FEC_ACT;
      v_ANULADO := p_ANULADO;
      v_FEC_CONTABLE := p_FEC_CONTABLE;


          UPDATE EJECUCION.T_ASIENTOS_DET SET
          ID_ASIENTO = v_ID_ASIENTO,
          ID_CTA_CONTABLE = v_ID_CTA_CONTABLE,
          FEC_CONTABLE = v_FEC_CONTABLE,
          DEBE = v_DEBE,
          HABER = v_HABER,
          ID_RUR_DET = v_ID_RUR_DET,
          ID_DEVENGADODET = v_ID_DEVENGADODET,
          ID_DIETDAC_DET = v_ID_DIETDAC_DET,
          ID_MOV_TESO_DET = v_ID_MOV_TESO_DET,
          ID_ELEMENTO_PAGO = v_ID_ELEMENTO_PAGO,
          USER_ING = v_USER_ING,
          FEC_ING = v_FEC_ING,
          USER_ACT = v_USER_ACT,
          FEC_ACT = v_FEC_ACT,
          ANULADO = v_ANULADO
      WHERE
          ID_ASIENTO_DET = v_ID_ASIENTO_DET
      ;

       exception
         when v_error_00001 then
            p_MSG :='Numero duplicado para esa vigencia.';
            pkg_util.base_rollback;
         when v_error_01407 then
            p_MSG :='No se puede insertar valores nulos.';
            pkg_util.base_rollback;
         when others then
            p_MSG :='Error al EDITAR. Verifique: '|| sqlerrm;
            pkg_util.base_rollback;

  END EDITAR;

  PROCEDURE ELIMINAR
  (
      p_ID_ASIENTO_DET IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE DEFAULT NULL
  ,p_MSG IN OUT varchar2
  )
  IS
  v_ID_ASIENTO_DET EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE;

  BEGIN
  v_ID_ASIENTO_DET := p_ID_ASIENTO_DET;

      DELETE FROM EJECUCION.T_ASIENTOS_DET
      WHERE
          ID_ASIENTO_DET = v_ID_ASIENTO_DET
      ;

       exception
         when v_error_2292 then
            p_MSG :='No se puede Eliminar el registro existen relaciones para ese ejercicio asociadas a otras tablas.';
            pkg_util.base_rollback;
         when others then
            p_MSG :='Error al ELIMINAR. Verifique: '|| sqlerrm;
            pkg_util.base_rollback;

  END ELIMINAR;

  PROCEDURE BUSCARPORPK
  (
      p_T_ASIENTOS_DET OUT ty_CURSOR,
      p_ID_ASIENTO_DET IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE DEFAULT NULL
  )
  IS
  v_ID_ASIENTO_DET EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE;

  BEGIN
  v_ID_ASIENTO_DET := p_ID_ASIENTO_DET;

      OPEN p_T_ASIENTOS_DET FOR
      SELECT
          EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET,
          EJECUCION.T_ASIENTOS_DET.ID_ASIENTO,
          EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE,
          EJECUCION.T_ASIENTOS_DET.FEC_CONTABLE,
          EJECUCION.T_ASIENTOS_DET.DEBE,
          EJECUCION.T_ASIENTOS_DET.HABER,
          EJECUCION.T_ASIENTOS_DET.ID_RUR_DET,
          EJECUCION.T_ASIENTOS_DET.ID_DEVENGADODET,
          EJECUCION.T_ASIENTOS_DET.ID_DIETDAC_DET,
          EJECUCION.T_ASIENTOS_DET.ID_MOV_TESO_DET,
          EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO,
          EJECUCION.T_ASIENTOS_DET.USER_ING,
          EJECUCION.T_ASIENTOS_DET.FEC_ING,
          EJECUCION.T_ASIENTOS_DET.USER_ACT,
          EJECUCION.T_ASIENTOS_DET.FEC_ACT,
          EJECUCION.T_ASIENTOS_DET.ANULADO
      FROM EJECUCION.T_ASIENTOS_DET
      WHERE
          (EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET = v_ID_ASIENTO_DET)
      ;
  END BUSCARPORPK;
  */
  PROCEDURE BUSCAR(p_T_ASIENTOS_DET      OUT ty_CURSOR,
                   p_ID_ASIENTO_DET      IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE DEFAULT NULL,
                   p_ID_ASIENTO          IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO%TYPE DEFAULT NULL,
                   p_ID_CTA_CONTABLE     IN EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                   p_FEC_CONTABLE        IN EJECUCION.T_ASIENTOS_DET.FEC_CONTABLE%TYPE DEFAULT NULL,
                   p_DEBE                IN EJECUCION.T_ASIENTOS_DET.DEBE%TYPE DEFAULT NULL,
                   p_HABER               IN EJECUCION.T_ASIENTOS_DET.HABER%TYPE DEFAULT NULL,
                   p_ID_RUR_DET          IN EJECUCION.T_ASIENTOS_DET.ID_RUR_DET%TYPE DEFAULT NULL,
                   p_ID_DEVENGADODET     IN EJECUCION.T_ASIENTOS_DET.ID_DEVENGADODET%TYPE DEFAULT NULL,
                   p_ID_DIETDAC_DET      IN EJECUCION.T_ASIENTOS_DET.ID_DIETDAC_DET%TYPE DEFAULT NULL,
                   p_ID_MOV_TESO_DET     IN EJECUCION.T_ASIENTOS_DET.ID_MOV_TESO_DET%TYPE DEFAULT NULL,
                   p_ID_ELEMENTO_PAGO    IN EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO%TYPE DEFAULT NULL,
                   p_USER_ING            IN EJECUCION.T_ASIENTOS_DET.USER_ING%TYPE DEFAULT NULL,
                   p_FEC_ING             IN EJECUCION.T_ASIENTOS_DET.FEC_ING%TYPE DEFAULT NULL,
                   p_USER_ACT            IN EJECUCION.T_ASIENTOS_DET.USER_ACT%TYPE DEFAULT NULL,
                   p_FEC_ACT             IN EJECUCION.T_ASIENTOS_DET.FEC_ACT%TYPE DEFAULT NULL,
                   p_ANULADO             IN EJECUCION.T_ASIENTOS_DET.ANULADO%TYPE DEFAULT NULL,
                   p_PAGINA              IN NUMBER DEFAULT NULL,
                   p_TAMANOPAGINA        IN NUMBER DEFAULT NULL,
                   p_CANTIDADDEREGISTROS OUT NUMBER

                   ) IS
    v_ID_ASIENTO_DET   EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE;
    v_ID_ASIENTO       EJECUCION.T_ASIENTOS_DET.ID_ASIENTO%TYPE;
    v_ID_CTA_CONTABLE  EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE%TYPE;
    v_DEBE             EJECUCION.T_ASIENTOS_DET.DEBE%TYPE;
    v_HABER            EJECUCION.T_ASIENTOS_DET.HABER%TYPE;
    v_ID_RUR_DET       EJECUCION.T_ASIENTOS_DET.ID_RUR_DET%TYPE;
    v_ID_DEVENGADODET  EJECUCION.T_ASIENTOS_DET.ID_DEVENGADODET%TYPE;
    v_ID_DIETDAC_DET   EJECUCION.T_ASIENTOS_DET.ID_DIETDAC_DET%TYPE;
    v_ID_MOV_TESO_DET  EJECUCION.T_ASIENTOS_DET.ID_MOV_TESO_DET%TYPE;
    v_ID_ELEMENTO_PAGO EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO%TYPE;
    v_USER_ING         EJECUCION.T_ASIENTOS_DET.USER_ING%TYPE;
    v_FEC_ING          EJECUCION.T_ASIENTOS_DET.FEC_ING%TYPE;
    v_USER_ACT         EJECUCION.T_ASIENTOS_DET.USER_ACT%TYPE;
    v_FEC_ACT          EJECUCION.T_ASIENTOS_DET.FEC_ACT%TYPE;
    v_ANULADO          EJECUCION.T_ASIENTOS_DET.ANULADO%TYPE;
    v_PAGINA           NUMBER;
    v_TAMANOPAGINA     NUMBER;
    v_FEC_CONTABLE     EJECUCION.T_ASIENTOS_DET.FEC_CONTABLE%TYPE;

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
    v_AND          VARCHAR2(500);
  BEGIN
    v_ID_ASIENTO_DET   := p_ID_ASIENTO_DET;
    v_ID_ASIENTO       := p_ID_ASIENTO;
    v_ID_CTA_CONTABLE  := p_ID_CTA_CONTABLE;
    v_DEBE             := p_DEBE;
    v_HABER            := p_HABER;
    v_ID_RUR_DET       := p_ID_RUR_DET;
    v_ID_DEVENGADODET  := p_ID_DEVENGADODET;
    v_ID_DIETDAC_DET   := p_ID_DIETDAC_DET;
    v_ID_MOV_TESO_DET  := p_ID_MOV_TESO_DET;
    v_ID_ELEMENTO_PAGO := p_ID_ELEMENTO_PAGO;
    v_USER_ING         := p_USER_ING;
    v_FEC_ING          := p_FEC_ING;
    v_USER_ACT         := p_USER_ACT;
    v_FEC_ACT          := p_FEC_ACT;
    v_FEC_ACT          := p_FEC_ACT;
    v_ANULADO          := p_ANULADO;
    v_PAGINA           := p_PAGINA;
    v_TAMANOPAGINA     := p_TAMANOPAGINA;
    v_FEC_CONTABLE     := p_FEC_CONTABLE;

    BEGIN
      v_SELECT_COUNT := 'SELECT COUNT(*) ';
      v_FROM_COUNT   := 'FROM EJECUCION.T_ASIENTOS_DET, EJECUCION.T_RECURSO_D_H, EJECUCION.t_cuentas_contables cc, EJECUCION.t_asientos a ';
      v_WHERE_COUNT  := ' ';
      v_AND          := 'WHERE EJECUCION.T_ASIENTOS_DET.ID_RECURSO_D_H = EJECUCION.T_RECURSO_D_H.ID_RECURSO_D_H (+) AND EJECUCION.T_ASIENTOS_DET.id_cta_contable =
                                        cc.id_cta_contable
                                    and a.id_asiento = EJECUCION.T_ASIENTOS_DET.id_asiento
                                    /*and cc.clasificador_relacionado is not null*/ AND ';

      IF v_ID_ASIENTO_DET IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET = ' ||
                         v_ID_ASIENTO_DET;
        v_AND         := ' AND';
      END IF;

      IF v_ID_ASIENTO IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.ID_ASIENTO = ' ||
                         v_ID_ASIENTO;
        v_AND         := ' AND';
      END IF;

      IF v_ID_CTA_CONTABLE IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE = ' ||
                         v_ID_CTA_CONTABLE;
        v_AND         := ' AND';
      END IF;

      IF TO_DATE(v_FEC_CONTABLE, 'DD/MM/YYYY') IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.FEC_CONTABLE = ''' ||
                         v_FEC_CONTABLE || '''';
        v_AND         := ' AND';
      END IF;

      IF v_DEBE IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.DEBE = ' || v_DEBE;
        v_AND         := ' AND';
      END IF;

      IF v_HABER IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.HABER = ' || v_HABER;
        v_AND         := ' AND';
      END IF;

      IF v_ID_RUR_DET IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.ID_RUR_DET = ' ||
                         v_ID_RUR_DET;
        v_AND         := ' AND';
      END IF;

      IF v_ID_DEVENGADODET IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.ID_DEVENGADODET = ' ||
                         v_ID_DEVENGADODET;
        v_AND         := ' AND';
      END IF;

      IF v_ID_DIETDAC_DET IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.ID_DIETDAC_DET = ' ||
                         v_ID_DIETDAC_DET;
        v_AND         := ' AND';
      END IF;

      IF v_ID_MOV_TESO_DET IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.ID_MOV_TESO_DET = ' ||
                         v_ID_MOV_TESO_DET;
        v_AND         := ' AND';
      END IF;

      IF v_ID_ELEMENTO_PAGO IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO = ' ||
                         v_ID_ELEMENTO_PAGO;
        v_AND         := ' AND';
      END IF;

      IF v_USER_ING IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.USER_ING LIKE ''%' ||
                         v_USER_ING || '%''';
        v_AND         := ' AND';
      END IF;

      IF TO_DATE(v_FEC_ING, 'DD/MM/YYYY') IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.FEC_ING = ''' ||
                         v_FEC_ING || '''';
        v_AND         := ' AND';
      END IF;

      IF v_USER_ACT IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.USER_ACT LIKE ''%' ||
                         v_USER_ACT || '%''';
        v_AND         := ' AND';
      END IF;

      IF TO_DATE(v_FEC_ACT, 'DD/MM/YYYY') IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.FEC_ACT = ''' ||
                         v_FEC_ACT || '''';
        v_AND         := ' AND';
      END IF;

      IF v_ANULADO IS NOT NULL THEN
        v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                         ' EJECUCION.T_ASIENTOS_DET.ANULADO LIKE ''%' ||
                         v_ANULADO || '%''';
      END IF;

      v_CANTIDAD := v_SELECT_COUNT || v_FROM_COUNT || v_WHERE_COUNT;
      /*insert into pruebas values (v_CANTIDAD,sysdate,'mop');
      COMMIT;*/
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
                         'EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET, ' ||
                         'EJECUCION.T_ASIENTOS_DET.ID_ASIENTO, ' ||
                         'EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE, ' ||
                         'EJECUCION.T_ASIENTOS_DET.FEC_CONTABLE, ' ||
                         'EJECUCION.T_ASIENTOS_DET.DEBE, ' ||
                         'EJECUCION.T_ASIENTOS_DET.HABER, ' ||
                         'EJECUCION.T_ASIENTOS_DET.ID_RUR_DET, ' ||
                         'EJECUCION.T_ASIENTOS_DET.ID_DEVENGADODET, ' ||
                         'EJECUCION.T_ASIENTOS_DET.ID_DIETDAC_DET, ' ||
                         'EJECUCION.T_ASIENTOS_DET.ID_MOV_TESO_DET, ' ||
                         'EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO, ' ||
                         'EJECUCION.T_ASIENTOS_DET.USER_ING, ' ||
                         'EJECUCION.T_ASIENTOS_DET.FEC_ING, ' ||
                         'EJECUCION.T_ASIENTOS_DET.USER_ACT, ' ||
                         'EJECUCION.T_ASIENTOS_DET.FEC_ACT, ' ||
                         'EJECUCION.T_ASIENTOS_DET.ID_RECURSO_D_H, ' ||
                         'EJECUCION.T_RECURSO_D_H.DESCRIPCION, ' ||
                         'EJECUCION.T_ASIENTOS_DET.ANULADO, ' ||
                         'EJECUCION.T_ASIENTOS_DET.ID_CUENTA_BANCARIA, ' ||
                         ' (SELECT CB.n_cuenta_bancaria FROM CAJA.T_CUENTAS_BANCARIA CB WHERE CB.ID_CUENTA_BANCARIA
                             = EJECUCION.T_ASIENTOS_DET.ID_CUENTA_BANCARIA) n_cuenta_bancaria, ' ||
                         'EJECUCION.T_ASIENTOS_DET.ID_LIBRO_BANCO,
                          A.LEYENDA
            /*decode(cc.clasificador_relacionado, ''PROVEEDORES'',
         (select p.razon
           from CAJA.t_proveedores p
          where p.id_proveedor = EJECUCION.T_ASIENTOS_DET.id_aux_prov),
          decode(cc.clasificador_relacionado ,''CUENTAS BANCARIAS '',
           (select c.nro_cuenta ||''-''|| c.n_cuenta_bancaria
                   from CAJA.t_cuentas_bancaria c
                  where c.id_cuenta_bancaria = EJECUCION.T_ASIENTOS_DET.ID_AUX_CTA_BAN)
                  ,decode(cc.clasificador_relacionado,''GASTOS'',
                   (select o.nombre_objeto
                           from PRESUPUESTO.t_objetos_gasto o
                          where o.id_objeto_gasto = EJECUCION.T_ASIENTOS_DET.ID_AUX_OG),
                           (select r.nombre_recurso
                                   from PRESUPUESTO.t_recursos r
                                  where r.id_recurso = EJECUCION.T_ASIENTOS_DET.ID_AUX_REC))))*/leyenda ';

      v_SUB_CONSULTA := v_SELECT || v_FROM_COUNT || v_WHERE_COUNT ||
                        ' ORDER BY 1'; --Falta colocar columna de Ordenamiento
      v_WHERE_PAGINA := ' WHERE NUMERODEREGISTRO > ' || v_INICIO || '' ||
                        ' AND NUMERODEREGISTRO <= ' || v_FIN || '';

      v_BUSCAR := v_SELECT_PAGINA || v_SUB_CONSULTA || ')V )' ||
                  v_WHERE_PAGINA || ' ORDER BY 1'; --Falta colocar columna de Ordenamiento
      insert into pruebas values (V_BUSCAR, sysdate, 'BUSCAR_ASIENTO_DET');
      COMMIT;
    END;

    OPEN p_T_ASIENTOS_DET FOR v_BUSCAR;
  END BUSCAR;

END PKG_T_ASIENTOS_DET;

PACKAGE "PKG_T_ASIENTOS_DET" AS
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

  PROCEDURE INSERTAR_DET(p_ID_ASIENTO_DET     OUT EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE,
                         p_ID_ASIENTO         IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO%TYPE DEFAULT NULL,
                         p_ID_CTA_CONTABLE    IN EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                         p_FEC_CONTABLE       IN EJECUCION.T_ASIENTOS_DET.FEC_CONTABLE%TYPE DEFAULT NULL,
                         p_DEBE               IN EJECUCION.T_ASIENTOS_DET.DEBE%TYPE DEFAULT NULL,
                         p_HABER              IN EJECUCION.T_ASIENTOS_DET.HABER%TYPE DEFAULT NULL,
                         p_ID_RUR_DET         IN EJECUCION.T_ASIENTOS_DET.ID_RUR_DET%TYPE DEFAULT NULL,
                         p_ID_DEVENGADODET    IN EJECUCION.T_ASIENTOS_DET.ID_DEVENGADODET%TYPE DEFAULT NULL,
                         p_ID_DIETDAC_DET     IN EJECUCION.T_ASIENTOS_DET.ID_DIETDAC_DET%TYPE DEFAULT NULL,
                         p_ID_MOV_TESO_DET    IN EJECUCION.T_ASIENTOS_DET.ID_MOV_TESO_DET%TYPE DEFAULT NULL,
                         p_ID_ELEMENTO_PAGO   IN EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO%TYPE DEFAULT NULL,
                         p_USER_ING           IN EJECUCION.T_ASIENTOS_DET.USER_ING%TYPE DEFAULT NULL,
                         p_FEC_ING            IN EJECUCION.T_ASIENTOS_DET.FEC_ING%TYPE DEFAULT NULL,
                         p_USER_ACT           IN EJECUCION.T_ASIENTOS_DET.USER_ACT%TYPE DEFAULT NULL,
                         p_FEC_ACT            IN EJECUCION.T_ASIENTOS_DET.FEC_ACT%TYPE DEFAULT NULL,
                         p_ANULADO            IN EJECUCION.T_ASIENTOS_DET.ANULADO%TYPE DEFAULT NULL,
                         p_ID_RECURSO         IN EJECUCION.T_ASIENTOS_DET.ID_RECURSO_D_H%TYPE DEFAULT NULL,
                         p_ID_AUXILIAR        IN EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                         p_id_cuenta_bancaria in EJECUCION.T_ASIENTOS_DET.id_cuenta_bancaria%TYPE DEFAULT NULL,
                         p_MSG                IN OUT VARCHAR2);
  /*
  PROCEDURE EDITAR
  (
      p_ID_ASIENTO_DET IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE DEFAULT NULL,
      p_ID_ASIENTO IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO%TYPE DEFAULT NULL,
      p_ID_CTA_CONTABLE IN EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
      p_FEC_CONTABLE IN EJECUCION.T_ASIENTOS_DET.FEC_CONTABLE%TYPE DEFAULT NULL,
      p_DEBE IN EJECUCION.T_ASIENTOS_DET.DEBE%TYPE DEFAULT NULL,
      p_HABER IN EJECUCION.T_ASIENTOS_DET.HABER%TYPE DEFAULT NULL,
      p_ID_RUR_DET IN EJECUCION.T_ASIENTOS_DET.ID_RUR_DET%TYPE DEFAULT NULL,
      p_ID_DEVENGADODET IN EJECUCION.T_ASIENTOS_DET.ID_DEVENGADODET%TYPE DEFAULT NULL,
      p_ID_DIETDAC_DET IN EJECUCION.T_ASIENTOS_DET.ID_DIETDAC_DET%TYPE DEFAULT NULL,
      p_ID_MOV_TESO_DET IN EJECUCION.T_ASIENTOS_DET.ID_MOV_TESO_DET%TYPE DEFAULT NULL,
      p_ID_ELEMENTO_PAGO IN EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO%TYPE DEFAULT NULL,
      p_USER_ING IN EJECUCION.T_ASIENTOS_DET.USER_ING%TYPE DEFAULT NULL,
      p_FEC_ING IN EJECUCION.T_ASIENTOS_DET.FEC_ING%TYPE DEFAULT NULL,
      p_USER_ACT IN EJECUCION.T_ASIENTOS_DET.USER_ACT%TYPE DEFAULT NULL,
      p_FEC_ACT IN EJECUCION.T_ASIENTOS_DET.FEC_ACT%TYPE DEFAULT NULL,
      p_ANULADO IN EJECUCION.T_ASIENTOS_DET.ANULADO%TYPE DEFAULT NULL
  ,p_MSG IN OUT varchar2
  );
  PROCEDURE ELIMINAR
  (
      p_ID_ASIENTO_DET IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE DEFAULT NULL
  ,p_MSG IN OUT varchar2
  );
  PROCEDURE BUSCARPORPK
  (
      p_T_ASIENTOS_DET OUT ty_CURSOR,
      p_ID_ASIENTO_DET IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE DEFAULT NULL
  );*/

  PROCEDURE BUSCAR(p_T_ASIENTOS_DET      OUT ty_CURSOR,
                   p_ID_ASIENTO_DET      IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO_DET%TYPE DEFAULT NULL,
                   p_ID_ASIENTO          IN EJECUCION.T_ASIENTOS_DET.ID_ASIENTO%TYPE DEFAULT NULL,
                   p_ID_CTA_CONTABLE     IN EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                   p_FEC_CONTABLE        IN EJECUCION.T_ASIENTOS_DET.FEC_CONTABLE%TYPE DEFAULT NULL,
                   p_DEBE                IN EJECUCION.T_ASIENTOS_DET.DEBE%TYPE DEFAULT NULL,
                   p_HABER               IN EJECUCION.T_ASIENTOS_DET.HABER%TYPE DEFAULT NULL,
                   p_ID_RUR_DET          IN EJECUCION.T_ASIENTOS_DET.ID_RUR_DET%TYPE DEFAULT NULL,
                   p_ID_DEVENGADODET     IN EJECUCION.T_ASIENTOS_DET.ID_DEVENGADODET%TYPE DEFAULT NULL,
                   p_ID_DIETDAC_DET      IN EJECUCION.T_ASIENTOS_DET.ID_DIETDAC_DET%TYPE DEFAULT NULL,
                   p_ID_MOV_TESO_DET     IN EJECUCION.T_ASIENTOS_DET.ID_MOV_TESO_DET%TYPE DEFAULT NULL,
                   p_ID_ELEMENTO_PAGO    IN EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO%TYPE DEFAULT NULL,
                   p_USER_ING            IN EJECUCION.T_ASIENTOS_DET.USER_ING%TYPE DEFAULT NULL,
                   p_FEC_ING             IN EJECUCION.T_ASIENTOS_DET.FEC_ING%TYPE DEFAULT NULL,
                   p_USER_ACT            IN EJECUCION.T_ASIENTOS_DET.USER_ACT%TYPE DEFAULT NULL,
                   p_FEC_ACT             IN EJECUCION.T_ASIENTOS_DET.FEC_ACT%TYPE DEFAULT NULL,
                   p_ANULADO             IN EJECUCION.T_ASIENTOS_DET.ANULADO%TYPE DEFAULT NULL,
                   p_PAGINA              IN NUMBER DEFAULT NULL,
                   p_TAMANOPAGINA        IN NUMBER DEFAULT NULL,
                   p_CANTIDADDEREGISTROS OUT NUMBER);
END PKG_T_ASIENTOS_DET;

