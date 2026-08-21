PACKAGE BODY "PKG_REPORTES_CONTABILIDAD" AS

  PROCEDURE PRC_LIBRO_DIARIO

  (P_T_CURSOR      OUT TY_CURSOR,
   P_EJERCICIO     IN presupuesto.T_VIGENCIAS.EJERCICIO%TYPE DEFAULT NULL,
   P_FECHA_CORTE   IN ejecucion.T_ASIENTOS_DET.FEC_CONTABLE%TYPE DEFAULT NULL,
   P_FECHA_INICIO  IN ejecucion.T_ASIENTOS_DET.FEC_CONTABLE%TYPE DEFAULT NULL,
   P_ID_UNIDAD_ADM IN presupuesto.T_UNIDADES_ADMINISTRATIVA.ID_UNIDAD_ADM%TYPE DEFAULT NULL) IS

    V_SENTENCIA      varchar2(100);
    V_SENTENCIA2     varchar2(100);
    V_NRO_UNIDAD_ADM presupuesto.T_UNIDADES_ADMINISTRATIVA.NUMERO_UNIDAD_ADM%TYPE;
    V_UNIDAD_ADM     VARCHAR2(100);

  BEGIN

    V_SENTENCIA := '  ALTER SESSION SET NLS_DATE_FORMAT=' || '''' ||
                   ' DD/MM/YYYY' || '''';
    EXECUTE IMMEDIATE V_SENTENCIA;

    V_SENTENCIA2 := 'ALTER SESSION SET NLS_LANGUAGE=SPANISH';
    EXECUTE IMMEDIATE V_SENTENCIA2;

    SELECT UA.NUMERO_UNIDAD_ADM,
           UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM
      INTO V_NRO_UNIDAD_ADM, V_UNIDAD_ADM
      FROM presupuesto.T_UNIDADES_ADMINISTRATIVA UA
     WHERE UA.ID_UNIDAD_ADM = P_ID_UNIDAD_ADM;

    OPEN p_T_CURSOR FOR

    --DEVENGADO
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             A.FEC_CONTABLE FEC_CONTABLE,
             ad.fec_ing FEC_REGISTRO,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             NVL(AD.DEBE, 0) MONTO_DEBE,
             NVL(ad.haber, 0) MONTO_HABER,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             'DEVENGADO' TIPO_DOC,
             DEV.NRO_DEVENGADO NRO_DOC,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             (decode(a.id_tipo_asiento,
                     1,
                     '--',
                     (decode(cc.imputable, 'N', 'NO', 'SI')))) IMP_PRESUP,
             nvl2(ad.id_cat_prog, NULL, '--') C_PROG,
             nvl2(ad.id_objeto_gasto, NULL, '--') O_GASTO,
             (decode(dev.id_proveedor,
                     null,
                     '--',
                     (select p.razon
                        from caja.t_proveedores p
                       where p.id_proveedor = dev.id_proveedor))) proveedor,
             (select max(op.nrodoc)
                from caja.t_ordenes_pago   op,
                     caja.t_beneficiarios  b,
                     caja.t_comprobantes   co,
                     ejecucion.t_devengado dev1
               where op.id_orden_pago = b.id_orden_pago
                 and b.id_beneficiario = co.id_beneficiario
                 and co.id_devengado = dev1.id_devengado
                 and dev1.id_devengado = dev.id_devengado
                 AND OP.ANULADO = 'N') op1,
             null op2,
             --PARAMETROS

             P_EJERCICIO P_EJERCICIO,

             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,

             --PARAMETROS OPCIONALES

             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,

             V_UNIDAD_ADM P_ID_UNIDAD_ADM

        FROM ejecucion.T_ASIENTOS          A,
             ejecucion.T_ASIENTOS_DET      AD,
             ejecucion.T_CUENTAS_CONTABLES CC,
             --ejecucion.T_RECURSO_D_H        RDH,
             ejecucion.T_TIPO_ASIENTO              TA,
             ejecucion.T_DEVENGADO                 DEV,
             ejecucion.T_DEVENGADO_DETALLE         DEVDET,
             presupuesto.T_UNIDADES_ADMINISTRATIVA UA

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
            --and ad.id_recurso_d_h = rdh.id_recurso_d_h
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
            --AND DEVDET.ID_DEVENGADO=DEV.ID_DEVENGADO 02/03/2012
         AND A.ID_DEVENGADO = DEV.ID_DEVENGADO
         AND AD.ID_DEVENGADODET = DEVDET.ID_DEVENGADODET
         AND A.Id_Jurisdiccion = UA.Id_Jurisdiccion
            --AND A.VISIBLE <> 'N'

         AND A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')
         AND /*to_date*/
             (A.FEC_CONTABLE) <= P_FECHA_CORTE
         AND ((A.FEC_CONTABLE >= P_FECHA_INICIO) OR
             (P_FECHA_INICIO IS NULL AND
             A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')))
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
      --AND DEVDET.ID_ESTADO <> 20  \*Modificado por Lau Martinez debido al cambio en la tabla t_devengado_detalle (nueva columna id_estado)*\

      UNION

      --- MOV_TESO

      select A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             A.FEC_CONTABLE FEC_CONTABLE,
             ad.fec_ing FEC_REGISTRO,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             NVL(AD.DEBE, 0) MONTO_DEBE,
             NVL(ad.haber, 0) MONTO_HABER,
             --RDH.DESCRIPCION EFECTO_EJEC,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             'MOV TESO' TIPO_DOC,
             MOVTESO.NRO_MOV_TESO NRO_DOC,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             (decode(a.id_tipo_asiento,
                     1,
                     '--',
                     (decode(cc.imputable, 'N', 'NO', 'SI')))) IMP_PRESUP,
             --                NULL C_PROG,
             nvl2(ad.id_cat_prog, NULL, '--') C_PROG,
             --             NULL O_GASTO,
             nvl2(ad.id_objeto_gasto, NULL, '--') O_GASTO,
             nvl2(MOVTESO_DET.ID_PROVEEDOR, null, '--') proveedor,
             null op1,
             null op2,
             P_EJERCICIO P_EJERCICIO,

             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,

             --PARAMETROS OPCIONALES

             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,

             V_UNIDAD_ADM P_ID_UNIDAD_ADM

        FROM ejecucion.T_ASIENTOS          A,
             ejecucion.T_ASIENTOS_DET      AD,
             ejecucion.T_CUENTAS_CONTABLES CC,
             caja.T_CUENTAS_BANCARIA       CB,
             caja.T_CUENTAS_BANCARIA       CTA,
             --ejecucion.T_RECURSO_D_H        RDH,
             ejecucion.T_TIPO_ASIENTO              TA,
             caja.T_MOVIMIENTOS_TESO_DET           MOVTESO_DET,
             caja.t_Movimientos_Teso               MOVTESO,
             presupuesto.T_UNIDADES_ADMINISTRATIVA UA

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
            --and ad.id_recurso_d_h = rdh.id_recurso_d_h
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND CB.ID_CUENTA_BANCARIA = MOVTESO.ID_CUENTA_BANCARIA
         AND CTA.ID_CUENTA_BANCARIA = MOVTESO_DET.ID_CUENTA_BANCARIA
         AND MOVTESO_DET.ID_MOV_TESO = MOVTESO.ID_MOV_TESO
         AND AD.ID_MOV_TESO_DET = MOVTESO_DET.ID_MOV_TESO_DET
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
            --AND A.VISIBLE <> 'N'
         AND AD.ID_MOV_TESO_DET IS NOT NULL
         AND A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')
         AND /*to_date*/
             (A.FEC_CONTABLE) <= P_FECHA_CORTE
         AND ((A.FEC_CONTABLE >= P_FECHA_INICIO) OR
             (P_FECHA_INICIO IS NULL AND
             A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')))
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM

      UNION
      --ELEMENTOS PAGO

      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             A.FEC_CONTABLE FEC_CONTABLE,
             ad.fec_ing FEC_REGISTRO,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             NVL(AD.DEBE, 0) MONTO_DEBE,
             NVL(ad.haber, 0) MONTO_HABER,
             --RDH.DESCRIPCION EFECTO_EJEC,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             'ELEMENTO PAGO' TIPO_DOC,
             EP.NUMERO_EP NRO_DOC,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             (decode(a.id_tipo_asiento,
                     1,
                     '--',
                     (decode(cc.imputable, 'N', 'NO', 'SI')))) IMP_PRESUP,
             --NULL C_PROG,
             nvl2(ad.id_cat_prog, NULL, '--') C_PROG,
             --NULL O_GASTO,
             nvl2(ad.id_objeto_gasto, NULL, '--') O_GASTO,
             decode(ep.id_proveedor,
                    null,
                    '--',
                    (select p.razon
                       from caja.t_proveedores p
                      where p.id_proveedor = ep.id_proveedor)) proveedor,

             decode(ep.id_elemento_pago,
                    null,
                    '--',
                    ejecucion.f_busca_nro_op_x_ep(ep.id_elemento_pago, 1)) op1,
             decode(ep.id_elemento_pago,
                    null,
                    '--',
                    ejecucion.f_busca_nro_op_x_ep(ep.id_elemento_pago, 2)) op2,

             P_EJERCICIO P_EJERCICIO,

             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,

             --PARAMETROS OPCIONALES

             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,

             V_UNIDAD_ADM P_ID_UNIDAD_ADM

        FROM ejecucion.T_ASIENTOS          A,
             ejecucion.T_ASIENTOS_DET      AD,
             ejecucion.T_CUENTAS_CONTABLES CC,
             caja.T_CUENTAS_BANCARIA       CB,
             caja.T_CUENTAS_BANCARIA       CTA,
             --ejecucion.T_RECURSO_D_H        RDH,
             ejecucion.T_TIPO_ASIENTO              TA,
             caja.T_ELEMENTOS_PAGO                 EP,
             presupuesto.T_UNIDADES_ADMINISTRATIVA UA

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
            --and ad.id_recurso_d_h = rdh.id_recurso_d_h
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND AD.ID_ELEMENTO_PAGO = EP.ID_ELEMENTO_PAGO
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
            --AND A.VISIBLE <> 'N'
         AND EP.ID_CUENTA_EP = CB.ID_CUENTA_BANCARIA(+)
         AND CTA.ID_CUENTA_BANCARIA(+) = EP.ID_CUENTA_TRANSF
            --and ep.id_elemento_pago = 38341
         AND A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')
         AND (A.FEC_CONTABLE) <= P_FECHA_CORTE
         AND ((A.FEC_CONTABLE >= P_FECHA_INICIO) OR
             (P_FECHA_INICIO IS NULL AND
             A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')))
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
      --    AND EP.ID_ELEMENTO_PAGO IS NOT NULL

      UNION
      --ANTICIPOS

      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             A.FEC_CONTABLE FEC_CONTABLE,
             ad.fec_ing FEC_REGISTRO,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             NVL(AD.DEBE, 0) MONTO_DEBE,
             NVL(ad.haber, 0) MONTO_HABER,
             --RDH.DESCRIPCION EFECTO_EJEC,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             'ANTICIPO' TIPO_DOC,
             AN.NTO_ANTICIPO NRO_DOC,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             (decode(a.id_tipo_asiento,
                     1,
                     '--',
                     (decode(cc.imputable, 'N', 'NO', 'SI')))) IMP_PRESUP,
             -- NULL C_PROG,*\
             nvl2(ad.id_cat_prog, NULL, '--') C_PROG,
             --NULL O_GASTO,*\
             nvl2(ad.id_objeto_gasto, NULL, '--') O_GASTO,
             decode(an.id_proveedor,
                    null,
                    '--',
                    (select p.razon
                       from caja.t_proveedores p
                      where p.id_proveedor = an.id_proveedor)) proveedor,

             decode(an.id_anticipo,
                    null,
                    '--',
                    (select min(op.nrodoc)
                       from caja.t_ordenes_pago op
                      where op.id_anticipo = an.id_anticipo
                        and op.anulado = 'N')) op1,
             null op2,

             P_EJERCICIO P_EJERCICIO,

             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,

             --PARAMETROS OPCIONALES

             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,

             V_UNIDAD_ADM P_ID_UNIDAD_ADM

        FROM ejecucion.T_ASIENTOS          A,
             ejecucion.T_ASIENTOS_DET      AD,
             ejecucion.T_CUENTAS_CONTABLES CC,
             --ejecucion.T_RECURSO_D_H        RDH,
             ejecucion.T_TIPO_ASIENTO              TA,
             ejecucion.T_ANTICIPOS                 AN,
             presupuesto.T_UNIDADES_ADMINISTRATIVA UA

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
            --and ad.id_recurso_d_h = rdh.id_recurso_d_h
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         and a.id_anticipo = an.id_anticipo
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND AD.ID_ELEMENTO_PAGO IS NULL --se duplicaba el registro 23/01/2012
            --AND A.VISIBLE <> 'N'
         AND A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')
         AND (A.FEC_CONTABLE) <= P_FECHA_CORTE
         AND ((A.FEC_CONTABLE >= P_FECHA_INICIO) OR
             (P_FECHA_INICIO IS NULL AND
             A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')))
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM

      UNION
      -- COMPROBANTE DEDUCCION
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             A.FEC_CONTABLE FEC_CONTABLE,
             ad.fec_ing FEC_REGISTRO,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             NVL(AD.DEBE, 0) MONTO_DEBE,
             NVL(ad.haber, 0) MONTO_HABER,
             --RDH.DESCRIPCION EFECTO_EJEC,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             'DEDUCCION' TIPO_DOC,
             DED2.DOCUMENTO_NRO NRO_DOC,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             (decode(a.id_tipo_asiento,
                     1,
                     '--',
                     (decode(cc.imputable, 'N', 'NO', 'SI')))) IMP_PRESUP,
             --\*  NULL C_PROG,*\
             nvl2(ad.id_cat_prog, NULL, '--') C_PROG,
             -- \*NULL O_GASTO,*\
             nvl2(ad.id_objeto_gasto, NULL, '--') O_GASTO,
             null proveedor,
             null op1,
             null op2,

             --PARAMETROS OBLIGATORIOS

             P_EJERCICIO P_EJERCICIO,

             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,

             --PARAMETROS OPCIONALES

             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,

             V_UNIDAD_ADM P_ID_UNIDAD_ADM

        FROM ejecucion.T_ASIENTOS          A,
             ejecucion.T_ASIENTOS_DET      AD,
             ejecucion.T_CUENTAS_CONTABLES CC,
             --ejecucion.T_RECURSO_D_H        RDH,
             ejecucion.T_TIPO_ASIENTO              TA,
             presupuesto.T_UNIDADES_ADMINISTRATIVA UA,
             caja.T_DEDUCCIONES                    DED2,
             caja.T_COMPROBANTES_DEDUCCION         CD2

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
            --AND ad.id_recurso_d_h = rdh.id_recurso_d_h
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND AD.ID_COMPROBANTE_DEDUCCION = CD2.ID_COMP_DEDUCCION
         AND CD2.ID_DEDUCCION = DED2.ID_DEDUCCION
         AND AD.ID_COMPROBANTE_DEDUCCION IS NOT NULL
            --AND A.VISIBLE <> 'N'

         AND A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')
         AND A.FEC_CONTABLE <= P_FECHA_CORTE --P_FECHA_CORTE(HASTA)
         AND ((A.FEC_CONTABLE >= P_FECHA_INICIO) OR
             (P_FECHA_INICIO IS NULL AND
             A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')))
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
      UNION

      --TRANSFERENCIAS INTERNAS
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             A.FEC_CONTABLE FEC_CONTABLE,
             ad.fec_ing FEC_REGISTRO,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             NVL(AD.DEBE, 0) MONTO_DEBE,
             NVL(ad.haber, 0) MONTO_HABER,
             --RDH.DESCRIPCION EFECTO_EJEC,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             'TRANSFERENCIA' TIPO_DOC,
             T.NRO_TRANSF NRO_DOC,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             (decode(a.id_tipo_asiento,
                     1,
                     '--',
                     (decode(cc.imputable, 'N', 'NO', 'SI')))) IMP_PRESUP,
             --   \*  NULL C_PROG,*\
             nvl2(ad.id_cat_prog, NULL, '--') C_PROG,
             -- \* NULL O_GASTO,*\
             nvl2(ad.id_objeto_gasto, NULL, '--') O_GASTO,
             null proveedor,
             null op1,
             null op2,

             --PARAMETROS OBLIGATORIOS

             P_EJERCICIO P_EJERCICIO,

             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,

             --PARAMETROS OPCIONALES

             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,

             V_UNIDAD_ADM P_ID_UNIDAD_ADM

        FROM ejecucion.T_ASIENTOS          A,
             ejecucion.T_ASIENTOS_DET      AD,
             ejecucion.T_CUENTAS_CONTABLES CC,
             caja.T_CUENTAS_BANCARIA       CBO,
             caja.T_CUENTAS_BANCARIA       CBD,
             --ejecucion.T_RECURSO_D_H        RDH,
             ejecucion.T_TIPO_ASIENTO              TA,
             presupuesto.T_UNIDADES_ADMINISTRATIVA UA,
             ejecucion.T_TRANSFERENCIAS            T

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
            --AND AD.ID_RECURSO_D_H = RDH.ID_RECURSO_D_H
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND A.ID_TRANSFERENCIA = T.ID_TRANSFERENCIA
         AND CBO.ID_CUENTA_BANCARIA = T.ID_CUENTA_BANCARIA_ORIG
         AND CBD.ID_CUENTA_BANCARIA = T.ID_CUENTA_BANCARIA_DEST
         AND T.ID_TRANSFERENCIA IS NOT NULL
            --AND A.VISIBLE <> 'N'
         AND A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')
         AND (A.FEC_CONTABLE) <= P_FECHA_CORTE
         AND ((A.FEC_CONTABLE >= P_FECHA_INICIO) OR
             (P_FECHA_INICIO IS NULL AND
             A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')))
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM

      UNION

      --RETENCION LIQUIDACION      (24/01/2012)

      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             A.FEC_CONTABLE FEC_CONTABLE,
             ad.fec_ing FEC_REGISTRO,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             NVL(AD.DEBE, 0) MONTO_DEBE,
             NVL(ad.haber, 0) MONTO_HABER,
             --RDH.DESCRIPCION EFECTO_EJEC,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             'RETENCION LIQUIDACION' TIPO_DOC,
             TO_CHAR(RL.NRO_COMP_RETENCION) NRO_DOC,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             (decode(a.id_tipo_asiento,
                     1,
                     '--',
                     (decode(cc.imputable, 'N', 'NO', 'SI')))) IMP_PRESUP,
             --  \*  NULL C_PROG,*\
             nvl2(ad.id_cat_prog, NULL, '--') C_PROG,
             --  \*NULL O_GASTO,*\
             nvl2(ad.id_objeto_gasto, NULL, '--') O_GASTO,

             -----------jose peralta   caso 2061 nesecitan ver proveedor y op----------------
             --\* null proveedor,
             --    null op,*\
             (decode(rl.id_proveedor,
                     null,
                     '--',
                     (select p.razon
                        from caja.t_proveedores p
                       where p.id_proveedor = rl.id_proveedor))) proveedor,

             op.nrodoc op1,
             null      op2,
             -----------jose peralta  caso 2061 nesecitan ver proveedor y op----------------

             --PARAMETROS OBLIGATORIOS

             P_EJERCICIO P_EJERCICIO,

             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,

             --PARAMETROS OPCIONALES

             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,

             V_UNIDAD_ADM P_ID_UNIDAD_ADM

        FROM ejecucion.T_ASIENTOS          A,
             ejecucion.T_ASIENTOS_DET      AD,
             ejecucion.T_CUENTAS_CONTABLES CC,
             --ejecucion.T_RECURSO_D_H              RDH,
             ejecucion.T_TIPO_ASIENTO              TA,
             presupuesto.T_UNIDADES_ADMINISTRATIVA UA,
             caja.t_retenciones_liquidacion        rl,
             -----------jose peralta  caso 2061 nesecitan ver proveedor y op----------------
             --\*CAJA.t_retenciones_liquidacion ret,*\
             caja.t_comprobantes  com,
             caja.t_beneficiarios ben,
             caja.t_ordenes_pago  op
      -----------jose peralta   caso 2061 nesecitan ver proveedor y op----------------

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
            --and ad.id_recurso_d_h = rdh.id_recurso_d_h
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         and ad.id_retencion_liquidacion = rl.id_retencion_liquidacion
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION

            -----------jose peralta  caso 2061 nesecitan ver proveedor y op----------------
            --  \*and ret.id_retencion_liquidacion = ad.id_retencion_liquidacion*\
         and rl.id_comprobante = com.id_comprobante -- modifico porque estaba suplicada la tabla ret
         and com.id_beneficiario = ben.id_beneficiario
         and ben.id_orden_pago = op.id_orden_pago
            -----------jose peralta   caso 2061 nesecitan ver proveedor y op----------------

            --AND A.VISIBLE <> 'N'
         AND A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')
         AND (A.FEC_CONTABLE) <= P_FECHA_CORTE --P_FECHA_CORTE(HASTA)
            --P_EJERCICIO O P_FECHA_INICIO
         AND ((A.FEC_CONTABLE >= P_FECHA_INICIO) OR
             (P_FECHA_INICIO IS NULL AND
             A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')))
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM

      UNION

      --MANUALES

      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             A.FEC_CONTABLE FEC_CONTABLE,
             ad.fec_ing FEC_REGISTRO,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             NVL(AD.DEBE, 0) MONTO_DEBE,
             NVL(ad.haber, 0) MONTO_HABER,
             --RDH.DESCRIPCION EFECTO_EJEC,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             '' TIPO_DOC,
             NULL NRO_DOC,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             (decode(a.id_tipo_asiento,
                     1,
                     '--',
                     (decode(ad.id_cat_prog, '', 'NO', 'SI')))) IMP_PRESUP,
             nvl2(ad.id_cat_prog, (F_PROGRAMA_FMT(AD.id_cat_prog)), '--') c_prog,

             decode(ad.id_objeto_gasto,
                    null,
                    '--',
                    (SELECT OG.NUMERO_OBJETO
                       FROM presupuesto.T_OBJETOS_GASTO OG
                      WHERE OG.ID_OBJETO_GASTO = AD.ID_OBJETO_GASTO)) o_gasto,

             null proveedor,
             null op1,
             null op2,
             --PARAMETROS OBLIGATORIOS

             P_EJERCICIO P_EJERCICIO,

             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,

             --PARAMETROS OPCIONALES

             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,

             V_UNIDAD_ADM P_ID_UNIDAD_ADM

        FROM ejecucion.T_ASIENTOS          A,
             ejecucion.T_ASIENTOS_DET      AD,
             ejecucion.T_CUENTAS_CONTABLES CC,
             --ejecucion.T_RECURSO_D_H        RDH,
             ejecucion.T_TIPO_ASIENTO              TA,
             presupuesto.T_UNIDADES_ADMINISTRATIVA UA

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
            --and ad.id_recurso_d_h = rdh.id_recurso_d_h
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
            --AND AD.ID_RUR_DET IS NULL
         AND AD.ID_DEVENGADODET IS NULL
         AND AD.ID_DIETDAC_DET IS NULL
         AND AD.ID_MOV_TESO_DET IS NULL
         AND AD.ID_ELEMENTO_PAGO IS NULL
         AND AD.ID_COMPROBANTE_DEDUCCION IS NULL
         AND AD.ID_RETENCION_LIQUIDACION IS NULL
         AND A.ID_ANTICIPO IS NULL
         AND A.ID_TRANSFERENCIA IS NULL
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
            --AND A.VISIBLE <> 'N'
         AND A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')
         AND (A.FEC_CONTABLE) <= P_FECHA_CORTE --P_FECHA_CORTE(HASTA)
            --P_EJERCICIO O P_FECHA_INICIO
         AND ((A.FEC_CONTABLE >= P_FECHA_INICIO) OR
             (P_FECHA_INICIO IS NULL AND
             A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')))
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
         AND A.ID_TIPO_ASIENTO = 2 --MANUAL

      UNION

      --AUTOMATICOS QUE NO SE PUEDEN RELACIONAR A NINGUN DETALLE (DEVENGADO, RETENCION LIQUIDACION, ELEMENTO DE PAGO, ETC)
      --AGREGADO POR WALTER KREICK EL 21/02/2013. CASO 1232. VISTO CON FERNANDO CUEVAS.

      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             A.FEC_CONTABLE FEC_CONTABLE,
             ad.fec_ing FEC_REGISTRO,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             NVL(AD.DEBE, 0) MONTO_DEBE,
             NVL(ad.haber, 0) MONTO_HABER,
             --RDH.DESCRIPCION EFECTO_EJEC,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             DECODE(A.ID_DEVENGADO, NULL, '', 'DEVENGADO') TIPO_DOC,
             DECODE(A.ID_DEVENGADO,
                    NULL,
                    NULL,
                    (SELECT max(D.NRO_DEVENGADO)
                       FROM ejecucion.T_DEVENGADO D
                      WHERE D.ID_DEVENGADO = A.ID_DEVENGADO)) NRO_DOC,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             (decode(a.id_tipo_asiento,
                     1,
                     '--',
                     (decode(cc.imputable, 'N', 'NO', 'SI')))) IMP_PRESUP,
             nvl2(ad.id_cat_prog, (F_PROGRAMA_FMT(AD.id_cat_prog)), '--') c_prog,
             decode(ad.id_objeto_gasto,
                    null,
                    '--',
                    (SELECT OG.NUMERO_OBJETO
                       FROM presupuesto.T_OBJETOS_GASTO OG
                      WHERE OG.ID_OBJETO_GASTO = AD.ID_OBJETO_GASTO)) o_gasto,
             DECODE(A.ID_DEVENGADO,
                    NULL,
                    '--',
                    (SELECT PRO.RAZON
                       FROM caja.T_PROVEEDORES PRO, ejecucion.T_DEVENGADO DE1
                      WHERE PRO.ID_PROVEEDOR = DE1.ID_PROVEEDOR
                        AND DE1.ID_DEVENGADO = A.ID_DEVENGADO)) PROVEEDOR,
             DECODE(A.ID_DEVENGADO,
                    NULL,
                    '--',
                    (select max(op.nrodoc)
                       from caja.t_ordenes_pago   op,
                            caja.t_beneficiarios  b,
                            caja.t_comprobantes   co,
                            ejecucion.t_devengado dev1
                      where op.id_orden_pago = b.id_orden_pago
                        and b.id_beneficiario = co.id_beneficiario
                        and co.id_devengado = dev1.id_devengado

                        and dev1.id_devengado = A.ID_DEVENGADO
                        AND OP.ANULADO = 'N')) op1,
             null op2,

             --PARAMETROS OBLIGATORIOS

             P_EJERCICIO P_EJERCICIO,

             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,

             --PARAMETROS OPCIONALES

             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,

             V_UNIDAD_ADM P_ID_UNIDAD_ADM

        FROM ejecucion.T_ASIENTOS          A,
             ejecucion.T_ASIENTOS_DET      AD,
             ejecucion.T_CUENTAS_CONTABLES CC,
             --ejecucion.T_RECURSO_D_H        RDH,
             ejecucion.T_TIPO_ASIENTO              TA,
             presupuesto.T_UNIDADES_ADMINISTRATIVA UA

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
            --and ad.id_recurso_d_h = rdh.id_recurso_d_h
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
            --AND AD.ID_RUR_DET IS NULL
         AND AD.ID_DEVENGADODET IS NULL
         AND AD.ID_DIETDAC_DET IS NULL
         AND AD.ID_MOV_TESO_DET IS NULL
         AND AD.ID_ELEMENTO_PAGO IS NULL
         AND AD.ID_COMPROBANTE_DEDUCCION IS NULL
         AND AD.ID_RETENCION_LIQUIDACION IS NULL
         AND A.ID_ANTICIPO IS NULL
         AND A.ID_TRANSFERENCIA IS NULL
         AND AD.ID_INGRESOS_DET IS NULL -- AGREGADO CASO 2727 EHZ 10/03/2014
         AND AD.ID_COBRO IS NULL -- AGREGADO CASO 2727 EHZ 10/03/2014
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
            --AND A.VISIBLE <> 'N'
         AND A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')
         AND (A.FEC_CONTABLE) <= P_FECHA_CORTE --P_FECHA_CORTE(HASTA)
            --P_EJERCICIO O P_FECHA_INICIO
         AND ((A.FEC_CONTABLE >= P_FECHA_INICIO) OR
             (P_FECHA_INICIO IS NULL AND
             A.FEC_CONTABLE >=
             TO_DATE('01/ENE/' || P_EJERCICIO, 'DD/MM/RRRR')))
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
         AND A.ID_TIPO_ASIENTO = 1 --AUTOMATICOS

       order by NRO_ASIENTO asc, MONTO_DEBE desc;

  END PRC_LIBRO_DIARIO;

  PROCEDURE PRC_MAYORES(p_T_CURSOR        OUT TY_CURSOR,
                        P_EJERCICIO       IN PRESUPUESTO.T_VIGENCIAS.EJERCICIO %TYPE DEFAULT NULL,
                        P_FECHA_CORTE     IN DATE DEFAULT NULL,
                        P_ID_CTA_CONTABLE IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE %TYPE DEFAULT NULL,
                        P_FECHA_INICIO    IN DATE DEFAULT NULL,
                        P_UNIDAD_ADM      IN PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA.ID_UNIDAD_ADM %TYPE DEFAULT NULL,
                        P_JURISDICCION    IN PRESUPUESTO.T_JURISDICCIONES.ID_JURISDICCION %TYPE DEFAULT NULL) IS

    V_SENTENCIA         varchar2(100);
    V_SENTENCIA2        varchar2(100);
    V_NRO_CUENTA        EJECUCION.T_CUENTAS_CONTABLES.CODIGO%TYPE;
    V_NRO_UNIDAD_ADM    PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA.NUMERO_UNIDAD_ADM%TYPE;
    V_FECHA_INICIO      DATE;
    V_FECHA_INICIO_ANIO DATE;
    V_FECHA_CORTE       DATE;
    V_FECHA_INICIO_ACUM DATE;
    V_FECHA_CORTE_ACUM  DATE;
    V_SALDO_INICIAL     EJECUCION.T_ASIENTOS_DET.DEBE%TYPE;
    v_ejercicio         PRESUPUESTO.T_VIGENCIAS.EJERCICIO%TYPE;
  BEGIN
    V_SENTENCIA := '  ALTER SESSION SET NLS_DATE_FORMAT=' || '''' ||
                   ' DD/MM/RRRR' || '''';
    EXECUTE IMMEDIATE V_SENTENCIA;
    V_SENTENCIA2 := 'ALTER SESSION SET NLS_LANGUAGE=SPANISH';
    EXECUTE IMMEDIATE V_SENTENCIA2;

    SELECT CC.CODIGO, UA.NUMERO_UNIDAD_ADM, V.EJERCICIO
      INTO V_NRO_CUENTA, V_NRO_UNIDAD_ADM, V_EJERCICIO
      FROM EJECUCION.T_CUENTAS_CONTABLES         CC,
           PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA,
           PRESUPUESTO.T_VIGENCIAS               V
     WHERE CC.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE
       AND UA.ID_VIGENCIA = V.ID_VIGENCIA
       AND CC.ID_JURISDICCION = UA.ID_JURISDICCION;

    V_FECHA_CORTE       := TO_DATE(p_FECHA_CORTE, 'DD/MM/RRRR');
    V_FECHA_INICIO_ANIO := TO_DATE('01/01/' || P_EJERCICIO, 'DD/MM/RRRR');

    IF (P_FECHA_INICIO IS NULL) THEN
      V_FECHA_INICIO     := TO_DATE('01/01/' || P_EJERCICIO, 'DD/MM/RRRR');
      V_FECHA_CORTE_ACUM := TO_DATE('01/01/' || P_EJERCICIO, 'DD/MM/RRRR') - 1;
    ELSE
      V_FECHA_INICIO     := TO_DATE(p_FECHA_INICIO, 'DD/MM/RRRR');
      V_FECHA_CORTE_ACUM := TO_DATE(p_FECHA_INICIO, 'DD/MM/RRRR') - 1;
    END IF;
    V_FECHA_INICIO_ACUM := TO_DATE('01/01/' ||
                                   TO_CHAR(TO_NUMBER(P_EJERCICIO) - 1),
                                   'DD/MM/RRRR');

    SELECT NVL(SUM(SALDO), 0)
      INTO V_SALDO_INICIAL
      FROM (
            --DEVENGADO
            SELECT decode(AD.DEBE, null, 0, ad.debe) -
                    decode(ad.haber, null, 0, ad.haber) SALDO
              FROM EJECUCION.T_ASIENTOS          A,
                    EJECUCION.T_ASIENTOS_DET      AD,
                    EJECUCION.T_CUENTAS_CONTABLES CC,
                    EJECUCION.T_TIPO_ASIENTO      TA,
                    EJECUCION.T_DEVENGADO         DEV,
                    --EJECUCION.T_DEVENGADO_DETALLE  DEVDET,
                    CAJA.T_PROVEEDORES                    PROV,
                    PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               AND A.ID_DEVENGADO = DEV.ID_DEVENGADO
                  --AND AD.ID_DEVENGADODET = DEVDET.ID_DEVENGADODET
               AND DEV.ID_PROVEEDOR = PROV.ID_PROVEEDOR
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND A.ID_TIPO_ASIENTO = 1
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND (((CC.CODIGO NOT LIKE '04%' AND CC.CODIGO NOT LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ACUM) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)) AND
                   ((CC.CODIGO LIKE '04%' OR CC.CODIGO LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ANIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)))

            UNION ALL

            --- MOV_TESO
            SELECT decode(AD.DEBE, null, 0, ad.debe) -
                    decode(ad.haber, null, 0, ad.haber) SALDO
              FROM EJECUCION.T_ASIENTOS                  A,
                    EJECUCION.T_ASIENTOS_DET              AD,
                    EJECUCION.T_CUENTAS_CONTABLES         CC,
                    EJECUCION.T_TIPO_ASIENTO              TA,
                    CAJA.T_MOVIMIENTOS_TESO_DET           MOVTESO_DET,
                    CAJA.T_MOVIMIENTOS_TESO               MOVTESO,
                    CAJA.T_PROVEEDORES                    PROV,
                    PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               AND MOVTESO_DET.ID_MOV_TESO = MOVTESO.ID_MOV_TESO
               AND AD.ID_MOV_TESO_DET = MOVTESO_DET.ID_MOV_TESO_DET
               AND MOVTESO_DET.ID_PROVEEDOR = PROV.ID_PROVEEDOR
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND A.ID_TIPO_ASIENTO = 1
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND (((CC.CODIGO NOT LIKE '04%' AND CC.CODIGO NOT LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ACUM) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)) AND
                   ((CC.CODIGO LIKE '04%' OR CC.CODIGO LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ANIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)))

            UNION ALL

            --ELEMENTOS PAGO
            SELECT decode(AD.DEBE, null, 0, ad.debe) -
                    decode(ad.haber, null, 0, ad.haber) SALDO
              FROM EJECUCION.T_ASIENTOS                  A,
                    EJECUCION.T_ASIENTOS_DET              AD,
                    EJECUCION.T_CUENTAS_CONTABLES         CC,
                    EJECUCION.T_TIPO_ASIENTO              TA,
                    CAJA.T_ELEMENTOS_PAGO                 EP,
                    CAJA.t_proveedores                    prov,
                    PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               AND AD.ID_ELEMENTO_PAGO = EP.ID_ELEMENTO_PAGO
               and ep.id_proveedor = prov.id_proveedor(+)
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND A.ID_TIPO_ASIENTO = 1
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND (((CC.CODIGO NOT LIKE '04%' AND CC.CODIGO NOT LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ACUM) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)) AND
                   ((CC.CODIGO LIKE '04%' OR CC.CODIGO LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ANIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)))

            UNION ALL

            --ANTICIPOS
            SELECT decode(AD.DEBE, null, 0, ad.debe) -
                    decode(ad.haber, null, 0, ad.haber) SALDO
              FROM EJECUCION.T_ASIENTOS                  A,
                    EJECUCION.T_ASIENTOS_DET              AD,
                    EJECUCION.T_CUENTAS_CONTABLES         CC,
                    EJECUCION.T_TIPO_ASIENTO              TA,
                    EJECUCION.T_ANTICIPOS                 AN,
                    CAJA.T_PROVEEDORES                    PROV,
                    PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               and a.id_anticipo = an.id_anticipo
               and an.id_proveedor = prov.id_proveedor
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND AD.ID_ELEMENTO_PAGO IS NULL
               AND A.ID_TIPO_ASIENTO = 1
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND (((CC.CODIGO NOT LIKE '04%' AND CC.CODIGO NOT LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ACUM) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)) AND
                   ((CC.CODIGO LIKE '04%' OR CC.CODIGO LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ANIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)))

            UNION ALL

            --COMPROBANTE DEDUCCION (24/01/2012)
            SELECT decode(AD.DEBE, null, 0, ad.debe) -
                    decode(ad.haber, null, 0, ad.haber) SALDO
              FROM EJECUCION.T_ASIENTOS                  A,
                    EJECUCION.T_ASIENTOS_DET              AD,
                    EJECUCION.T_CUENTAS_CONTABLES         CC,
                    EJECUCION.T_TIPO_ASIENTO              TA,
                    PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA,
                    CAJA.T_COMPROBANTES_DEDUCCION         COMDED,
                    CAJA.T_COMPROBANTES                   COM,
                    CAJA.T_BENEFICIARIOS                  BEN,
                    CAJA.T_PROVEEDORES                    PROV
             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               AND AD.ID_COMPROBANTE_DEDUCCION = COMDED.ID_COMP_DEDUCCION
               AND COMDED.ID_COMPROBANTE = COM.ID_COMPROBANTE
               AND COM.ID_BENEFICIARIO = BEN.ID_BENEFICIARIO
               AND BEN.ID_PROVEEDOR = PROV.ID_PROVEEDOR
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND A.ID_TIPO_ASIENTO = 1
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND (((CC.CODIGO NOT LIKE '04%' AND CC.CODIGO NOT LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ACUM) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)) AND
                   ((CC.CODIGO LIKE '04%' OR CC.CODIGO LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ANIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)))

            UNION ALL

            --RETENCION LIQUIDACION (24/01/2012)
            SELECT decode(AD.DEBE, null, 0, ad.debe) -
                    decode(ad.haber, null, 0, ad.haber) SALDO
              FROM EJECUCION.T_ASIENTOS                  A,
                    EJECUCION.T_ASIENTOS_DET              AD,
                    EJECUCION.T_CUENTAS_CONTABLES         CC,
                    EJECUCION.T_TIPO_ASIENTO              TA,
                    PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA,
                    CAJA.T_RETENCIONES_LIQUIDACION        RL,
                    CAJA.T_PROVEEDORES                    PROV
             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               AND AD.ID_RETENCION_LIQUIDACION = RL.ID_RETENCION_LIQUIDACION
               AND RL.ID_PROVEEDOR = PROV.ID_PROVEEDOR(+)
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND A.ID_TIPO_ASIENTO = 1
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND (((CC.CODIGO NOT LIKE '04%' AND CC.CODIGO NOT LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ACUM) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)) AND
                   ((CC.CODIGO LIKE '04%' OR CC.CODIGO LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ANIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)))

            UNION ALL

            --TRANSFERENCIAS INTERNAS   29/02/2012
            SELECT decode(AD.DEBE, null, 0, ad.debe) -
                    decode(ad.haber, null, 0, ad.haber) SALDO
              FROM EJECUCION.T_ASIENTOS                  A,
                    EJECUCION.T_ASIENTOS_DET              AD,
                    EJECUCION.T_CUENTAS_CONTABLES         CC,
                    EJECUCION.T_TIPO_ASIENTO              TA,
                    PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA,
                    EJECUCION.T_TRANSFERENCIAS            T
             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND A.ID_TRANSFERENCIA = T.ID_TRANSFERENCIA
               AND T.ID_TRANSFERENCIA IS NOT NULL
               AND A.ID_TIPO_ASIENTO = 1
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND (((CC.CODIGO NOT LIKE '04%' AND CC.CODIGO NOT LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ACUM) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)) AND
                   ((CC.CODIGO LIKE '04%' OR CC.CODIGO LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ANIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)))

            UNION ALL

            --LIBRO BANCO
            SELECT decode(AD.DEBE, null, 0, ad.debe) -
                    decode(ad.haber, null, 0, ad.haber) SALDO

              FROM EJECUCION.T_ASIENTOS                  A,
                    EJECUCION.T_ASIENTOS_DET              AD,
                    EJECUCION.T_CUENTAS_CONTABLES         CC,
                    EJECUCION.T_TIPO_ASIENTO              TA,
                    PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA,
                    EJECUCION.T_LIBRO_BANCO               LB,
                    CAJA.T_PROVEEDORES                    PROV,
                    EJECUCION.T_TIPODOC                   TD

             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               AND LB.ID_LIBRO_BANCO = A.ID_LIBRO_BANCO
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND TD.ID_TIPODOC = LB.ID_TIPO_DOC
               AND CC.ID_PROVEEDOR = PROV.ID_PROVEEDOR(+)

               AND AD.ID_DEVENGADODET IS NULL
               AND AD.ID_DIETDAC_DET IS NULL
               AND AD.ID_MOV_TESO_DET IS NULL
               AND AD.ID_ELEMENTO_PAGO IS NULL
               AND AD.ID_COMPROBANTE_DEDUCCION IS NULL
               AND AD.ID_RETENCION_LIQUIDACION IS NULL
               AND A.ID_ANTICIPO IS NULL
               AND A.ID_TRANSFERENCIA IS NULL
               AND LB.ORIGEN_CARGA = 'ABM'
               AND A.ID_TIPO_ASIENTO = 1
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND (((CC.CODIGO NOT LIKE '04%' AND CC.CODIGO NOT LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ACUM) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)) AND
                   ((CC.CODIGO LIKE '04%' OR CC.CODIGO LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ANIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)))

            UNION ALL

            --APERTURA Y CIERRE
            SELECT decode(AD.DEBE, null, 0, ad.debe) -
                    decode(ad.haber, null, 0, ad.haber) SALDO
              FROM EJECUCION.T_ASIENTOS                  A,
                    EJECUCION.T_ASIENTOS_DET              AD,
                    EJECUCION.T_CUENTAS_CONTABLES         CC,
                    EJECUCION.T_TIPO_ASIENTO              TA,
                    PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA

             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND A.ID_TIPO_ASIENTO = 1
               AND (A.OBSERVACIONES LIKE '%ASIENTO APERTURA DE EJERCICIO%' OR
                   A.OBSERVACIONES LIKE '%ASIENTO CIERRE DE EJERCICIO%')
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND (((CC.CODIGO NOT LIKE '04%' AND CC.CODIGO NOT LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ACUM) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)) AND
                   ((CC.CODIGO LIKE '04%' OR CC.CODIGO LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ANIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)))

            UNION ALL

            --MANUALES (04/01/2012)
            SELECT decode(AD.DEBE, null, 0, ad.debe) -
                    decode(ad.haber, null, 0, ad.haber) SALDO
              FROM EJECUCION.T_ASIENTOS                  A,
                    EJECUCION.T_ASIENTOS_DET              AD,
                    EJECUCION.T_CUENTAS_CONTABLES         CC,
                    EJECUCION.T_TIPO_ASIENTO              TA,
                    PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND A.ID_TIPO_ASIENTO = 2
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND (((CC.CODIGO NOT LIKE '04%' AND CC.CODIGO NOT LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ACUM) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)) AND
                   ((CC.CODIGO LIKE '04%' OR CC.CODIGO LIKE '05%') AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                   V_FECHA_INICIO_ANIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                   V_FECHA_CORTE_ACUM)))

                   );

    OPEN p_T_CURSOR FOR
    --DEVENGADO
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
             decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
             V_SALDO_INICIAL SALDO_INICIAL,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             cc.tipo_saldo,
             'DEVENGADO' TIPO_DOC,
             DEV.NRO_DEVENGADO NRO_DOC,
             dev.nro_comprobante COMPROBANTE,
             PROV.RAZON PROVEEDOR,
             ('DEVENGADO ' || DEV.NRO_DEVENGADO || '-' ||
             dev.nro_comprobante || '. ' || '-' || A.LEYENDA || '-' ||
             A.OBSERVACIONES) NOTA,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             --PARAMETROS OBLIGATORIOS
             P_EJERCICIO P_EJERCICIO,
             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
             (SELECT CU.NOMBRE
                FROM EJECUCION.T_CUENTAS_CONTABLES CU
               WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
             --PARAMETROS OPCIONALES
             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,

             NVL2(PROV.RAZON,
                  PROV.RAZON,
                  (select max(prov.razon)
                     from EJECUCION.t_devengado dev, CAJA.T_PROVEEDORES PROV
                    where dev.id_devengado = a.id_devengado
                      and prov.id_proveedor = dev.id_proveedor)) AS BENEFICIARIO_DE_PAGO, --caso 5070

             (select max(op.nrodoc)
                from CAJA.t_ordenes_pago   op,
                     CAJA.t_beneficiarios  b,
                     CAJA.t_comprobantes   co,
                     EJECUCION.t_devengado dev1
               where op.id_orden_pago = b.id_orden_pago
                 and b.id_beneficiario = co.id_beneficiario
                 and co.id_devengado = dev1.id_devengado
                 and dev1.id_devengado = dev.id_devengado
                 AND OP.ANULADO = 'N') op1,

             (select max(op1.nrodoc)
                from CAJA.t_ordenes_pago op1
               where op1.id_anticipo = dev.id_anticipo
                 AND OP1.ANULADO = 'N') op2

        FROM EJECUCION.T_ASIENTOS          A,
             EJECUCION.T_ASIENTOS_DET      AD,
             EJECUCION.T_CUENTAS_CONTABLES CC,
             EJECUCION.T_TIPO_ASIENTO      TA,
             EJECUCION.T_DEVENGADO         DEV,
             --      EJECUCION.T_DEVENGADO_DETALLE  DEVDET,
             CAJA.T_PROVEEDORES                    PROV,
             PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND A.ID_DEVENGADO = DEV.ID_DEVENGADO
            --     AND AD.ID_DEVENGADODET = DEVDET.ID_DEVENGADODET
         AND DEV.ID_PROVEEDOR = PROV.ID_PROVEEDOR
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND A.ID_TIPO_ASIENTO = 1
            --PARAMETROS OBLIGATORIOS
         AND CC.CODIGO = V_NRO_CUENTA
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
            --PARAMETROS OPCIONALES
         AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
             (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))

      UNION

      --- MOV_TESO
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
             decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
             V_SALDO_INICIAL SALDO_INICIAL,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             cc.tipo_saldo,
             'MOV TESO' TIPO_DOC,
             MOVTESO.NRO_MOV_TESO NRO_DOC,
             NULL COMPROBANTE,
             PROV.RAZON PROVEEDOR,
             ('MOV TESO ' || MOVTESO.NRO_MOV_TESO || ' ' || '-' ||
             A.LEYENDA || '-' || A.OBSERVACIONES) NOTA,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             --PARAMETROS OBLIGATORIOS
             P_EJERCICIO P_EJERCICIO,
             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
             (SELECT CU.NOMBRE
                FROM EJECUCION.T_CUENTAS_CONTABLES CU
               WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
             --PARAMETROS OPCIONALES
             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,
             -- NVL2(PROV.RAZON, PROV.RAZON, '') AS BENEFICIARIO_DE_PAGO,--caso 5070
             NVL2(PROV.RAZON,
                  PROV.RAZON,
                  (select prov.razon
                     from EJECUCION.t_devengado dev, CAJA.T_PROVEEDORES PROV
                    where dev.id_devengado = a.id_devengado
                      and prov.id_proveedor = dev.id_proveedor)) AS BENEFICIARIO_DE_PAGO, --caso 5070
             null op1,
             null op2

        FROM EJECUCION.T_ASIENTOS                  A,
             EJECUCION.T_ASIENTOS_DET              AD,
             EJECUCION.T_CUENTAS_CONTABLES         CC,
             EJECUCION.T_TIPO_ASIENTO              TA,
             CAJA.T_MOVIMIENTOS_TESO_DET           MOVTESO_DET,
             CAJA.T_MOVIMIENTOS_TESO               MOVTESO,
             CAJA.T_PROVEEDORES                    PROV,
             PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND MOVTESO_DET.ID_MOV_TESO = MOVTESO.ID_MOV_TESO
         AND AD.ID_MOV_TESO_DET = MOVTESO_DET.ID_MOV_TESO_DET
         AND MOVTESO_DET.ID_PROVEEDOR = PROV.ID_PROVEEDOR
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND A.ID_TIPO_ASIENTO = 1
            --PARAMETROS OBLIGATORIOS
         AND CC.CODIGO = V_NRO_CUENTA
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
            --PARAMETROS OPCIONALES
         AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
             (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))

      UNION

      --ELEMENTOS PAGO
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
             decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
             V_SALDO_INICIAL SALDO_INICIAL,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             cc.tipo_saldo,
             'ELEMENTO PAGO' TIPO_DOC,
             EP.NUMERO_EP NRO_DOC,
             NULL COMPROBANTE,
             PROV.RAZON PROVEEDOR,
             ('ELEMENTO PAGO ' || EP.NUMERO_EP || ' ' || '-' || A.LEYENDA || '-' ||
             A.OBSERVACIONES) NOTA,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             --PARAMETROS OBLIGATORIOS
             P_EJERCICIO P_EJERCICIO,
             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
             (SELECT CU.NOMBRE
                FROM EJECUCION.T_CUENTAS_CONTABLES CU
               WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
             --PARAMETROS OPCIONALES
             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,
             NVL2(PROV.RAZON,
                  PROV.RAZON,
                  (select prov.razon
                     from EJECUCION.t_devengado dev, CAJA.T_PROVEEDORES PROV
                    where dev.id_devengado = a.id_devengado
                      and prov.id_proveedor = dev.id_proveedor)) AS BENEFICIARIO_DE_PAGO, --caso 5070
             decode(ep.id_elemento_pago,
                    null,
                    '--',
                    EJECUCION.f_busca_nro_op_x_ep(ep.id_elemento_pago, 1)) op1,
             decode(ep.id_elemento_pago,
                    null,
                    '--',
                    EJECUCION.f_busca_nro_op_x_ep(ep.id_elemento_pago, 2)) op2

        FROM EJECUCION.T_ASIENTOS                  A,
             EJECUCION.T_ASIENTOS_DET              AD,
             EJECUCION.T_CUENTAS_CONTABLES         CC,
             EJECUCION.T_TIPO_ASIENTO              TA,
             CAJA.T_ELEMENTOS_PAGO                 EP,
             CAJA.t_proveedores                    prov,
             PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND AD.ID_ELEMENTO_PAGO = EP.ID_ELEMENTO_PAGO
         and ep.id_proveedor = prov.id_proveedor(+)
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND A.ID_TIPO_ASIENTO = 1
            --PARAMETROS OBLIGATORIOS
         AND CC.CODIGO = V_NRO_CUENTA
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
            --PARAMETROS OPCIONALES
         AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
             (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))

      UNION

      --ANTICIPOS
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
             decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
             V_SALDO_INICIAL SALDO_INICIAL,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             cc.tipo_saldo,
             'ANTICIPO' TIPO_DOC,
             AN.NTO_ANTICIPO NRO_DOC,
             NULL COMPROBANTE,
             PROV.RAZON PROVEEDOR,
             ('ANTICIPO ' || AN.NTO_ANTICIPO || ' ' || '-' || A.LEYENDA || '-' ||
             A.OBSERVACIONES) NOTA,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             --PARAMETROS OBLIGATORIOS
             P_EJERCICIO P_EJERCICIO,
             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
             (SELECT CU.NOMBRE
                FROM EJECUCION.T_CUENTAS_CONTABLES CU
               WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
             --PARAMETROS OPCIONALES
             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,
             -- NVL2(PROV.RAZON, PROV.RAZON, '') AS BENEFICIARIO_DE_PAGO,--caso 5070
             NVL2(PROV.RAZON,
                  PROV.RAZON,
                  (select prov.razon
                     from EJECUCION.t_devengado dev, CAJA.T_PROVEEDORES PROV
                    where dev.id_devengado = a.id_devengado
                      and prov.id_proveedor = dev.id_proveedor)) AS BENEFICIARIO_DE_PAGO, --caso 5070
             decode(an.id_anticipo,
                    null,
                    '--',
                    (select min(op.nrodoc)
                       from CAJA.t_ordenes_pago op
                      where op.id_anticipo = an.id_anticipo
                        and op.anulado = 'N')) op1,
             null op2
        FROM EJECUCION.T_ASIENTOS                  A,
             EJECUCION.T_ASIENTOS_DET              AD,
             EJECUCION.T_CUENTAS_CONTABLES         CC,
             EJECUCION.T_TIPO_ASIENTO              TA,
             EJECUCION.T_ANTICIPOS                 AN,
             CAJA.T_PROVEEDORES                    PROV,
             PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         and a.id_anticipo = an.id_anticipo
         and an.id_proveedor = prov.id_proveedor
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND AD.ID_ELEMENTO_PAGO IS NULL --se duplicaba el registro 23/01/2012
         AND A.ID_TIPO_ASIENTO = 1
            --PARAMETROS OBLIGATORIOS
         AND CC.CODIGO = V_NRO_CUENTA
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
            --PARAMETROS OPCIONALES
         AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
             (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))

      UNION

      --COMPROBANTE DEDUCCION (24/01/2012)
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
             decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
             V_SALDO_INICIAL SALDO_INICIAL,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             cc.tipo_saldo,
             'COMPROBANTE DEDUCCION' TIPO_DOC,
             TO_CHAR(COM.NRO_COMPROBANTE) NRO_DOC,
             NULL COMPROBANTE,
             PROV.RAZON PROVEEDOR,
             ('COMPROBANTE DEDUCCION ' || TO_CHAR(COM.NRO_COMPROBANTE) || ' ' || '-' ||
             A.LEYENDA || '-' || A.OBSERVACIONES) NOTA,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             --PARAMETROS OBLIGATORIOS
             P_EJERCICIO P_EJERCICIO,
             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
             (SELECT CU.NOMBRE
                FROM EJECUCION.T_CUENTAS_CONTABLES CU
               WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
             --PARAMETROS OPCIONALES
             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,
             -- NVL2(PROV.RAZON, PROV.RAZON, '') AS BENEFICIARIO_DE_PAGO,--caso 5070
             NVL2(PROV.RAZON,
                  PROV.RAZON,
                  (select prov.razon
                     from EJECUCION.t_devengado dev, CAJA.T_PROVEEDORES PROV
                    where dev.id_devengado = a.id_devengado
                      and prov.id_proveedor = dev.id_proveedor)) AS BENEFICIARIO_DE_PAGO, --caso 5070
             null op1,
             null op2

        FROM EJECUCION.T_ASIENTOS                  A,
             EJECUCION.T_ASIENTOS_DET              AD,
             EJECUCION.T_CUENTAS_CONTABLES         CC,
             EJECUCION.T_TIPO_ASIENTO              TA,
             PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA,
             CAJA.T_COMPROBANTES_DEDUCCION         COMDED,
             CAJA.T_COMPROBANTES                   COM,
             CAJA.T_BENEFICIARIOS                  BEN,
             CAJA.T_PROVEEDORES                    PROV
       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND AD.ID_COMPROBANTE_DEDUCCION = COMDED.ID_COMP_DEDUCCION
         AND COMDED.ID_COMPROBANTE = COM.ID_COMPROBANTE
         AND COM.ID_BENEFICIARIO = BEN.ID_BENEFICIARIO
         AND BEN.ID_PROVEEDOR = PROV.ID_PROVEEDOR
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND A.ID_TIPO_ASIENTO = 1
            --PARAMETROS OBLIGATORIOS
         AND CC.CODIGO = V_NRO_CUENTA
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
            --PARAMETROS OPCIONALES
         AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
             (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))

      UNION

      --RETENCION LIQUIDACION (24/01/2012)
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
             decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
             V_SALDO_INICIAL SALDO_INICIAL,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             cc.tipo_saldo,
             'RETENCION LIQUIDACION' TIPO_DOC,
             TO_CHAR(RL.NRO_COMP_RETENCION) NRO_DOC,
             NULL COMPROBANTE,
             PROV.RAZON PROVEEDOR,
             ('RETENCION LIQUIDACION ' || TO_CHAR(RL.NRO_COMP_RETENCION) || ' ' || '-' ||
             A.OBSERVACIONES || '-' || A.LEYENDA) NOTA,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             --PARAMETROS OBLIGATORIOS
             P_EJERCICIO P_EJERCICIO,
             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
             (SELECT CU.NOMBRE
                FROM EJECUCION.T_CUENTAS_CONTABLES CU
               WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
             --PARAMETROS OPCIONALES
             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,
             -- NVL2(PROV.RAZON, PROV.RAZON, '') AS BENEFICIARIO_DE_PAGO,--caso 5070
             NVL2(PROV.RAZON,
                  PROV.RAZON,
                  (select prov.razon
                     from EJECUCION.t_devengado dev, CAJA.T_PROVEEDORES PROV
                    where dev.id_devengado = a.id_devengado
                      and prov.id_proveedor = dev.id_proveedor)) AS BENEFICIARIO_DE_PAGO, --caso 5070
             op.nrodoc op1,
             null op2

        FROM EJECUCION.T_ASIENTOS                  A,
             EJECUCION.T_ASIENTOS_DET              AD,
             EJECUCION.T_CUENTAS_CONTABLES         CC,
             EJECUCION.T_TIPO_ASIENTO              TA,
             PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA,
             CAJA.T_RETENCIONES_LIQUIDACION        RL,
             CAJA.T_PROVEEDORES                    PROV,
             CAJA.t_comprobantes                   com,
             CAJA.t_beneficiarios                  ben,
             CAJA.t_ordenes_pago                   op

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND AD.ID_RETENCION_LIQUIDACION = RL.ID_RETENCION_LIQUIDACION
         AND RL.ID_PROVEEDOR = PROV.ID_PROVEEDOR
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         and rl.id_comprobante = com.id_comprobante -- modifico porque estaba duplicada la tabla ret
         and com.id_beneficiario = ben.id_beneficiario
         and ben.id_orden_pago = op.id_orden_pago
         AND A.ID_TIPO_ASIENTO = 1
            --PARAMETROS OBLIGATORIOS
         AND CC.CODIGO = V_NRO_CUENTA
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
            --PARAMETROS OPCIONALES
         AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
             (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))

      UNION

      --TRANSFERENCIAS INTERNAS   29/02/2012
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
             decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
             V_SALDO_INICIAL SALDO_INICIAL,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             cc.tipo_saldo,
             'TRANSFERENCIA' TIPO_DOC,
             T.NRO_TRANSF NRO_DOC,
             NULL COMPROBANTE,
             NULL PROVEEDOR,
             ('TRANSFERENCIA ' || T.NRO_TRANSF || '-' || A.OBSERVACIONES || '-' ||
             A.LEYENDA) NOTA,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             --PARAMETROS OBLIGATORIOS
             P_EJERCICIO P_EJERCICIO,
             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
             (SELECT CU.NOMBRE
                FROM EJECUCION.T_CUENTAS_CONTABLES CU
               WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
             --PARAMETROS OPCIONALES
             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,
             -- NVL2(PROV.RAZON, PROV.RAZON, '') AS BENEFICIARIO_DE_PAGO,--caso 5070
             NVL2(PROV.RAZON,
                  PROV.RAZON,
                  (select prov.razon
                     from EJECUCION.t_devengado dev, CAJA.T_PROVEEDORES PROV
                    where dev.id_devengado = a.id_devengado
                      and prov.id_proveedor = dev.id_proveedor)) AS BENEFICIARIO_DE_PAGO, --caso 5070
             null op1,
             null op2

        FROM EJECUCION.T_ASIENTOS                  A,
             EJECUCION.T_ASIENTOS_DET              AD,
             EJECUCION.T_CUENTAS_CONTABLES         CC,
             EJECUCION.T_TIPO_ASIENTO              TA,
             PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA,
             EJECUCION.T_TRANSFERENCIAS            T,
             CAJA.T_PROVEEDORES                    PROV
       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND A.ID_TRANSFERENCIA = T.ID_TRANSFERENCIA
         AND T.ID_TRANSFERENCIA IS NOT NULL
         AND A.ID_TIPO_ASIENTO = 1
            --PARAMETROS OBLIGATORIOS
         AND CC.CODIGO = V_NRO_CUENTA
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
            --PARAMETROS OPCIONALES
         AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
             (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))
         AND CC.ID_PROVEEDOR = PROV.ID_PROVEEDOR(+)

      UNION
      --LIBRO BANCO
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
             decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
             V_SALDO_INICIAL SALDO_INICIAL,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             cc.tipo_saldo,
             'LIBRO BANCO' TIPO_DOC,
             TO_CHAR(LB.NRO_DOC) NRO_DOC,
             TD.DESCRIPCION COMPROBANTE,
             PROV.RAZON PROVEEDOR,
             (A.OBSERVACIONES || '-' || A.LEYENDA) NOTA,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             --PARAMETROS OBLIGATORIOS
             P_EJERCICIO P_EJERCICIO,
             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
             (SELECT CU.NOMBRE
                FROM EJECUCION.T_CUENTAS_CONTABLES CU
               WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
             --PARAMETROS OPCIONALES
             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,
             -- NVL2(PROV.RAZON, PROV.RAZON, '') AS BENEFICIARIO_DE_PAGO,--caso 5070
             NVL2(PROV.RAZON,
                  PROV.RAZON,
                  (select prov.razon
                     from EJECUCION.t_devengado dev, CAJA.T_PROVEEDORES PROV
                    where dev.id_devengado = a.id_devengado
                      and prov.id_proveedor = LB.id_proveedor)) AS BENEFICIARIO_DE_PAGO, --caso 5070
             NULL op1,
             null op2

        FROM EJECUCION.T_ASIENTOS                  A,
             EJECUCION.T_ASIENTOS_DET              AD,
             EJECUCION.T_CUENTAS_CONTABLES         CC,
             EJECUCION.T_TIPO_ASIENTO              TA,
             PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA,
             EJECUCION.T_LIBRO_BANCO               LB,
             CAJA.T_PROVEEDORES                    PROV,
             EJECUCION.T_TIPODOC                   TD

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND LB.ID_LIBRO_BANCO = A.ID_LIBRO_BANCO
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND TD.ID_TIPODOC = LB.ID_TIPO_DOC
         AND LB.ID_PROVEEDOR = PROV.ID_PROVEEDOR(+)
         AND A.ID_TIPO_ASIENTO = 1
         AND LB.ORIGEN_CARGA = 'ABM'
            --PARAMETROS OBLIGATORIOS
         AND CC.CODIGO = V_NRO_CUENTA
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
            --PARAMETROS OPCIONALES
         AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
             (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))

      /*      UNION

            --APORTES (04/01/2012)
            SELECT A.NUMERO NRO_ASIENTO,
                   A.ID_ASIENTO,
                   AD.ID_ASIENTO_DET,
                   TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
                   CC.CODIGO NRO_CUENTA,
                   CC.NOMBRE NOM_CUENTA,
                   decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
                   decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
                   V_SALDO_INICIAL SALDO_INICIAL,
                   A.OBSERVACIONES,
                   A.LEYENDA LEYENDA_ASIENTO,
                   TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
                   cc.tipo_saldo,
                   'APORTE' TIPO_DOC,
                   TO_CHAR(APOR.PERIODO) NRO_DOC,
                   TD.DESCRIPCION COMPROBANTE,
                   '' PROVEEDOR,
                   (A.OBSERVACIONES || '-' || A.LEYENDA) NOTA,
                   (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
                   --PARAMETROS OBLIGATORIOS
                   P_EJERCICIO P_EJERCICIO,
                   (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
                   (SELECT CU.NOMBRE
                      FROM EJECUCION.T_CUENTAS_CONTABLES CU
                     WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
                   --PARAMETROS OPCIONALES
                   (SELECT DECODE(P_FECHA_INICIO,
                                  NULL,
                                  '--',
                                  TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                      FROM DUAL) P_FECHA_INICIO,
                   '' AS BENEFICIARIO_DE_PAGO,
                   null op1,
                   null op2

              FROM EJECUCION.T_ASIENTOS           A,
                   EJECUCION.T_ASIENTOS_DET       AD,
                   EJECUCION.T_CUENTAS_CONTABLES  CC,
                   EJECUCION.T_TIPO_ASIENTO       TA,
                   PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA,
                   EJECUCION.T_APORTES            APOR,
                   EJECUCION.T_TIPODOC            TD,
                   EJECUCION.T_APORTES_DETALLE    APORD

             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND A.ID_APORTE = APOR.ID_APORTE
               AND AD.ID_APORTE_DET = APORD.ID_APORTE_DET
               AND APORD.ID_APORTE = APOR.ID_APORTE
               AND TD.ID_TIPODOC(+) = APOR.ID_TIPODOC
               AND A.ID_TIPO_ASIENTO = 1
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))
      */
      /*      UNION

            --INGRESOS
            SELECT A.NUMERO NRO_ASIENTO,
                   A.ID_ASIENTO,
                   AD.ID_ASIENTO_DET,
                   TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
                   CC.CODIGO NRO_CUENTA,
                   CC.NOMBRE NOM_CUENTA,
                   decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
                   decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
                   V_SALDO_INICIAL SALDO_INICIAL,
                   A.OBSERVACIONES,
                   A.LEYENDA LEYENDA_ASIENTO,
                   TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
                   cc.tipo_saldo,
                   'INGRESOS' TIPO_DOC,
                   I.NRO_INGRESO NRO_DOC,
                   '' COMPROBANTE,
                   CLI.RAZON PROVEEDOR,
                   (A.LEYENDA) NOTA,
                   (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
                   --PARAMETROS OBLIGATORIOS
                   P_EJERCICIO P_EJERCICIO,
                   (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
                   (SELECT CU.NOMBRE
                      FROM EJECUCION.T_CUENTAS_CONTABLES CU
                     WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
                   --PARAMETROS OPCIONALES
                   (SELECT DECODE(P_FECHA_INICIO,
                                  NULL,
                                  '--',
                                  TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                      FROM DUAL) P_FECHA_INICIO,

                   CLI.RAZON AS BENEFICIARIO_DE_PAGO,

                   NULL op1,
                   null op2
              FROM EJECUCION.T_ASIENTOS           A,
                   EJECUCION.T_ASIENTOS_DET       AD,
                   EJECUCION.T_CUENTAS_CONTABLES  CC,
                   EJECUCION.T_TIPO_ASIENTO       TA,
                   EJECUCION.T_INGRESOS           I,
                   EJECUCION.T_CLIENTES           CLI,
                   PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               AND A.ID_INGRESOS = I.ID_INGRESOS
               AND I.ID_CLIENTE = CLI.ID_CLIENTE
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND A.ID_TIPO_ASIENTO = 1
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))

            UNION

            --COBROS
            SELECT A.NUMERO NRO_ASIENTO,
                   A.ID_ASIENTO,
                   AD.ID_ASIENTO_DET,
                   TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
                   CC.CODIGO NRO_CUENTA,
                   CC.NOMBRE NOM_CUENTA,
                   decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
                   decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
                   V_SALDO_INICIAL SALDO_INICIAL,
                   A.OBSERVACIONES,
                   A.LEYENDA LEYENDA_ASIENTO,
                   TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
                   cc.tipo_saldo,
                   'COBROS' TIPO_DOC,
                   C.NRO_RECIBO NRO_DOC,
                   '' COMPROBANTE,
                   '' PROVEEDOR,
                   A.LEYENDA NOTA, -- GALI 5651
                   (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
                   --PARAMETROS OBLIGATORIOS
                   P_EJERCICIO P_EJERCICIO,
                   (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
                   (SELECT CU.NOMBRE
                      FROM EJECUCION.T_CUENTAS_CONTABLES CU
                     WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
                   --PARAMETROS OPCIONALES
                   (SELECT DECODE(P_FECHA_INICIO,
                                  NULL,
                                  '--',
                                  TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                      FROM DUAL) P_FECHA_INICIO,

                   '' AS BENEFICIARIO_DE_PAGO,

                   NULL op1,
                   null op2
              FROM EJECUCION.T_ASIENTOS           A,
                   EJECUCION.T_ASIENTOS_DET       AD,
                   EJECUCION.T_CUENTAS_CONTABLES  CC,
                   EJECUCION.T_TIPO_ASIENTO       TA,
                   EJECUCION.t_Cobro              C,
                   PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
             WHERE AD.ID_ASIENTO = A.ID_ASIENTO
               AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
               AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
               AND AD.ID_COBRO = C.ID_COBRO
               AND A.ID_JURISDICCION = UA.ID_JURISDICCION
               AND A.ID_TIPO_ASIENTO = 1
                  --PARAMETROS OBLIGATORIOS
               AND CC.CODIGO = V_NRO_CUENTA
               AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
                  --PARAMETROS OPCIONALES
               AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
                   (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))
      */
      UNION

      --SELLOS
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
             decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
             V_SALDO_INICIAL SALDO_INICIAL,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             cc.tipo_saldo,
             'SELLOS' TIPO_DOC,
             '' NRO_DOC,
             '' COMPROBANTE,
             '' PROVEEDOR,
             A.LEYENDA NOTA,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             --PARAMETROS OBLIGATORIOS
             P_EJERCICIO P_EJERCICIO,
             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
             (SELECT CU.NOMBRE
                FROM EJECUCION.T_CUENTAS_CONTABLES CU
               WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
             --PARAMETROS OPCIONALES
             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,

             '' AS BENEFICIARIO_DE_PAGO,

             NULL op1,
             null op2
        FROM EJECUCION.T_ASIENTOS                  A,
             EJECUCION.T_ASIENTOS_DET              AD,
             EJECUCION.T_CUENTAS_CONTABLES         CC,
             EJECUCION.T_TIPO_ASIENTO              TA,
             CAJA.T_SELLOS                         S,
             PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND A.ID_SELLO = S.ID_SELLO
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND A.ID_TIPO_ASIENTO = 1
            --PARAMETROS OBLIGATORIOS
         AND CC.CODIGO = V_NRO_CUENTA
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
            --PARAMETROS OPCIONALES
         AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
             (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))

      UNION

      --- rur
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
             decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
             V_SALDO_INICIAL SALDO_INICIAL,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             cc.tipo_saldo,
             'RECURSO ' TIPO_DOC,
             TO_CHAR(R.NRO_DOC) NRO_DOC,
             NULL COMPROBANTE,
             NULL PROVEEDOR,
             ('RECURSO ' || R.NRO_DOC || '-' || A.LEYENDA || '-' ||
             A.OBSERVACIONES) NOTA,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             --PARAMETROS OBLIGATORIOS
             P_EJERCICIO P_EJERCICIO,
             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
             (SELECT CU.NOMBRE
                FROM EJECUCION.T_CUENTAS_CONTABLES CU
               WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
             --PARAMETROS OPCIONALES
             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,
             -- NVL2(PROV.RAZON, PROV.RAZON, '') AS BENEFICIARIO_DE_PAGO,--caso 5070
             '' AS BENEFICIARIO_DE_PAGO, --caso 5070
             null op1,
             null op2

        FROM EJECUCION.T_ASIENTOS                  A,
             EJECUCION.T_ASIENTOS_DET              AD,
             EJECUCION.T_CUENTAS_CONTABLES         CC,
             EJECUCION.T_TIPO_ASIENTO              TA,
         --    EJECUCION.T_RUR_DETALLE               RD,
             EJECUCION.T_RUR                       R,
             PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND A.ID_RUR = R.ID_RUR
       --  AND AD.ID_RUR_DET = RD.ID_RUR_DET
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND A.ID_TIPO_ASIENTO = 1
            --PARAMETROS OBLIGATORIOS
         AND CC.CODIGO = V_NRO_CUENTA
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
            --PARAMETROS OPCIONALES
         AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
             (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))

      UNION

      --INICIALES Y CIERRE
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
             decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
             V_SALDO_INICIAL SALDO_INICIAL,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             cc.tipo_saldo,
             'APERTURA Y CIERRE' TIPO_DOC,
             NULL NRO_DOC,
             NULL COMPROBANTE,
             NULL PROVEEDOR,
             (A.OBSERVACIONES || '-' || A.LEYENDA) NOTA,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             --PARAMETROS OBLIGATORIOS
             P_EJERCICIO P_EJERCICIO,
             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
             (SELECT CU.NOMBRE
                FROM EJECUCION.T_CUENTAS_CONTABLES CU
               WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
             --PARAMETROS OPCIONALES
             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,
             -- NVL2(PROV.RAZON, PROV.RAZON, '') AS BENEFICIARIO_DE_PAGO,--caso 5070
             '' AS BENEFICIARIO_DE_PAGO, --caso 5070
             null op1,
             null op2

        FROM EJECUCION.T_ASIENTOS                  A,
             EJECUCION.T_ASIENTOS_DET              AD,
             EJECUCION.T_CUENTAS_CONTABLES         CC,
             EJECUCION.T_TIPO_ASIENTO              TA,
             PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND A.ID_TIPO_ASIENTO = 1
         AND (A.OBSERVACIONES LIKE '%ASIENTO APERTURA DE EJERCICIO%' OR
             A.OBSERVACIONES LIKE '%ASIENTO CIERRE DE EJERCICIO%')
            --PARAMETROS OBLIGATORIOS
         AND CC.CODIGO = V_NRO_CUENTA
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
            --PARAMETROS OPCIONALES
         AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
             (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))

      UNION

      --MANUALES (04/01/2012)
      SELECT A.NUMERO NRO_ASIENTO,
             A.ID_ASIENTO,
             AD.ID_ASIENTO_DET,
             TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') FEC_CONTABLE,
             CC.CODIGO NRO_CUENTA,
             CC.NOMBRE NOM_CUENTA,
             decode(AD.DEBE, null, 0, ad.debe) MONTO_DEBE,
             decode(ad.haber, null, 0, ad.haber) MONTO_HABER,
             V_SALDO_INICIAL SALDO_INICIAL,
             A.OBSERVACIONES,
             A.LEYENDA LEYENDA_ASIENTO,
             TA.Codigo || ' - ' || TA.DESCRIPCION TIPO_ASIENTO,
             cc.tipo_saldo,
             'MANUAL' TIPO_DOC,
             NULL NRO_DOC,
             NULL COMPROBANTE,
             NULL PROVEEDOR,
             (A.OBSERVACIONES || '-' || A.LEYENDA) NOTA,
             (UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM) UNIDAD_ADM,
             --PARAMETROS OBLIGATORIOS
             P_EJERCICIO P_EJERCICIO,
             (SELECT TO_DATE(P_FECHA_CORTE, 'DD/MM/RRRR') FROM DUAL) P_FECHA_CORTE,
             (SELECT CU.NOMBRE
                FROM EJECUCION.T_CUENTAS_CONTABLES CU
               WHERE CU.ID_CTA_CONTABLE = P_ID_CTA_CONTABLE) P_CTA_CONTABLE,
             --PARAMETROS OPCIONALES
             (SELECT DECODE(P_FECHA_INICIO,
                            NULL,
                            '--',
                            TO_DATE(P_FECHA_INICIO, 'DD/MM/RRRR'))
                FROM DUAL) P_FECHA_INICIO,
             -- NVL2(PROV.RAZON, PROV.RAZON, '') AS BENEFICIARIO_DE_PAGO,--caso 5070
             NVL2(PROV.RAZON,
                  PROV.RAZON,
                  (select prov.razon
                     from EJECUCION.t_devengado dev, CAJA.T_PROVEEDORES PROV
                    where dev.id_devengado = a.id_devengado
                      and prov.id_proveedor = dev.id_proveedor)) AS BENEFICIARIO_DE_PAGO, --caso 5070
             null op1,
             null op2

        FROM EJECUCION.T_ASIENTOS                  A,
             EJECUCION.T_ASIENTOS_DET              AD,
             EJECUCION.T_CUENTAS_CONTABLES         CC,
             EJECUCION.T_TIPO_ASIENTO              TA,
             PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA,
             CAJA.T_PROVEEDORES                    PROV

       WHERE AD.ID_ASIENTO = A.ID_ASIENTO
         AND AD.ID_CTA_CONTABLE = CC.ID_CTA_CONTABLE
         AND A.ID_TIPO_ASIENTO = TA.ID_TIPO_ASIENTO
         AND A.ID_JURISDICCION = UA.ID_JURISDICCION
         AND A.ID_TIPO_ASIENTO = 2
            --PARAMETROS OBLIGATORIOS
         AND CC.CODIGO = V_NRO_CUENTA
         AND UA.NUMERO_UNIDAD_ADM = V_NRO_UNIDAD_ADM
            --PARAMETROS OPCIONALES
         AND ((TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >= V_FECHA_INICIO) AND
             (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <= V_FECHA_CORTE))
         AND CC.ID_PROVEEDOR = PROV.ID_PROVEEDOR(+)

       ORDER BY FEC_CONTABLE, NRO_ASIENTO;

  END PRC_MAYORES;

  PROCEDURE PRC_SUMAS_Y_SALDOS

  (p_T_CURSOR     OUT TY_CURSOR,
   p_id_vigencia  IN PRESUPUESTO.T_VIGENCIAS.ID_VIGENCIA%TYPE DEFAULT NULL,
   p_FECHA_CORTE  IN DATE DEFAULT NULL,
   p_nivel        IN NUMBER DEFAULT NULL,
   p_FECHA_INICIO IN DATE DEFAULT NULL,
   p_unidad_adm   IN PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA.ID_UNIDAD_ADM%TYPE DEFAULT NULL) IS

    V_SENTENCIA  varchar2(100);
    V_SENTENCIA2 varchar2(100);

    V_NRO_UNIDAD_ADM    PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA.NUMERO_UNIDAD_ADM%TYPE;
    V_UNIDAD_ADM        VARCHAR2(100);
    V_EJERCICIO         PRESUPUESTO.T_VIGENCIAS.EJERCICIO%TYPE;
    V_FECHA_INICIO      DATE;
    V_FECHA_INICIO_ANIO DATE;
    V_FECHA_CORTE       DATE;
    V_FECHA_INICIO_ACUM DATE;
    V_FECHA_CORTE_ACUM  DATE;
    V_NIVEL             NUMBER;

  BEGIN

    V_SENTENCIA := '  ALTER SESSION SET NLS_DATE_FORMAT=' || '''' ||
                   ' DD/MM/YYYY' || '''';
    EXECUTE IMMEDIATE V_SENTENCIA;

    V_SENTENCIA2 := 'ALTER SESSION SET NLS_LANGUAGE=SPANISH';
    EXECUTE IMMEDIATE V_SENTENCIA2;

    SELECT UA.NUMERO_UNIDAD_ADM,
           UA.NUMERO_UNIDAD_ADM || ' - ' || UA.NOMBRE_UNIDAD_ADM
      INTO V_NRO_UNIDAD_ADM, V_UNIDAD_ADM
      FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
     WHERE UA.ID_UNIDAD_ADM = p_UNIDAD_ADM;

    SELECT V.EJERCICIO
      INTO V_EJERCICIO
      FROM PRESUPUESTO.T_VIGENCIAS V
     WHERE V.ID_VIGENCIA = p_ID_VIGENCIA;

    V_FECHA_CORTE       := TO_DATE(p_FECHA_CORTE, 'DD/MM/RRRR');
    V_FECHA_INICIO_ANIO := TO_DATE('01/01/' || V_EJERCICIO, 'DD/MM/RRRR');

    IF (P_FECHA_INICIO IS NULL) THEN
      V_FECHA_INICIO     := TO_DATE('01/01/' || V_EJERCICIO, 'DD/MM/RRRR');
      V_FECHA_CORTE_ACUM := TO_DATE('01/01/' || V_EJERCICIO, 'DD/MM/RRRR') - 1;
    ELSE
      V_FECHA_INICIO     := TO_DATE(p_FECHA_INICIO, 'DD/MM/RRRR');
      V_FECHA_CORTE_ACUM := TO_DATE(p_FECHA_INICIO, 'DD/MM/RRRR') - 1;
    END IF;
    V_FECHA_INICIO_ACUM := TO_DATE('01/01/' ||
                                   TO_CHAR(TO_NUMBER(V_EJERCICIO) - 1),
                                   'DD/MM/RRRR');

    V_NIVEL := p_NIVEL;

    OPEN p_T_CURSOR FOR

      select TABLA.Nro_Cuenta,
             TABLA.CODIGO,
             TABLA.NOMBRE,
             TABLA.IMPUTABLE,
             SUM(TABLA.DEBE_ACUMULADO) DEBE_ACUMULADO,
             SUM(TABLA.HABER_ACUMULADO) HABER_ACUMULADO,
             SUM(TABLA.DEBE_PERIODO) DEBE_PERIODO,
             SUM(TABLA.HABER_PERIODO) HABER_PERIODO,
             TABLA.P_UNIDAD_ADM,
             TABLA.P_EJERCICIO,
             TABLA.P_FECHA_INICIO,
             TABLA.P_FECHA_CORTE,
             TABLA.P_NIVEL

        from ( -- TODAS LAS CUENTAS
              select CASE LENGTH(CC.CODIGO)
                        WHEN 2 THEN
                         CC.CODIGO
                        WHEN 5 THEN
                         '  ' || CC.CODIGO
                        WHEN 8 THEN
                         '    ' || CC.CODIGO
                        WHEN 11 THEN
                         '      ' || CC.CODIGO
                        WHEN 14 THEN
                         '        ' || CC.CODIGO
                        WHEN 17 THEN
                         '          ' || CC.CODIGO
                      END Nro_Cuenta,
                      cc.codigo,
                      CC.NOMBRE,
                      DECODE(cc.imputable, 'S', 'S', 'N') IMPUTABLE,
                      0 DEBE_PERIODO,
                      0 HABER_PERIODO,
                      0 DEBE_ACUMULADO,
                      0 HABER_ACUMULADO,
                      V_UNIDAD_ADM AS P_UNIDAD_ADM,
                      V_EJERCICIO AS P_EJERCICIO,
                      DECODE(V_FECHA_INICIO, NULL, '--/--/--', V_FECHA_INICIO) P_FECHA_INICIO,
                      DECODE(V_FECHA_CORTE, NULL, '--/--/--', V_FECHA_CORTE) P_FECHA_CORTE,
                      DECODE(V_NIVEL, NULL, '--', V_NIVEL) P_NIVEL

                from EJECUCION.t_cuentas_contables         cc,
                      PRESUPUESTO.t_unidades_administrativa ua
               where cc.Id_Jurisdiccion = ua.Id_Jurisdiccion
                    --   and ua.numero_unidad_adm = V_NRO_UNIDAD_ADM
                    -- AND ua.id_vigencia = p_ID_VIGENCIA
                 and cc.CODIGO <> '01.01.01.02.36'
                 AND CC.ID_CTA_CONTABLE <> 11323

               group by cc.codigo, CC.NOMBRE, cc.imputable

              UNION

              --PERIODO - CUENTAS IMPUTABLES
              select CASE LENGTH(CC.CODIGO)
                        WHEN 2 THEN
                         CC.CODIGO
                        WHEN 5 THEN
                         '  ' || CC.CODIGO
                        WHEN 8 THEN
                         '    ' || CC.CODIGO
                        WHEN 11 THEN
                         '      ' || CC.CODIGO
                        WHEN 14 THEN
                         '        ' || CC.CODIGO
                        WHEN 17 THEN
                         '          ' || CC.CODIGO
                      END Nro_Cuenta,
                      cc.codigo,
                      CC.NOMBRE,
                      DECODE(cc.imputable, 'S', 'S', 'N') IMPUTABLE,
                      nvl(sum(adet.debe), 0) DEBE_PERIODO,
                      nvl(sum(adet.haber), 0) HABER_PERIODO,
                      0 DEBE_ACUMULADO,
                      0 HABER_ACUMULADO,
                      V_UNIDAD_ADM AS P_UNIDAD_ADM,
                      V_EJERCICIO AS P_EJERCICIO,
                      DECODE(V_FECHA_INICIO, NULL, '--/--/--', V_FECHA_INICIO) P_FECHA_INICIO,
                      DECODE(V_FECHA_CORTE, NULL, '--/--/--', V_FECHA_CORTE) P_FECHA_CORTE,
                      DECODE(V_NIVEL, NULL, '--', V_NIVEL) P_NIVEL

                from EJECUCION.t_cuentas_contables         cc,
                      PRESUPUESTO.t_unidades_administrativa ua,
                      EJECUCION.t_asientos                  a,
                      EJECUCION.t_asientos_det              adet
               where cc.Id_Jurisdiccion = ua.Id_Jurisdiccion
                    --      and ua.numero_unidad_adm = V_NRO_UNIDAD_ADM
                    --AND ua.id_vigencia = p_ID_VIGENCIA --comentado Gali 4113
                 and cc.id_cta_contable = adet.id_cta_contable
                 and adet.id_asiento = a.id_asiento
                    --and (adet.anulado = 'N' or adet.anulado is null)
                 AND (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') BETWEEN
                     TO_DATE(V_FECHA_INICIO, 'DD/MM/RRRR') AND
                     TO_DATE(V_FECHA_CORTE, 'DD/MM/RRRR'))

                 and cc.CODIGO <> '01.01.01.02.36'
                 AND ADET.ID_CTA_CONTABLE <> 11323

                 and cc.imputable = 'S'
               group by cc.codigo, CC.NOMBRE, cc.imputable

              UNION

              --PERIODO - CUENTAS NO IMPUTABLES
              select CASE LENGTH(CC.CODIGO)
                        WHEN 2 THEN
                         CC.CODIGO
                        WHEN 5 THEN
                         '  ' || CC.CODIGO
                        WHEN 8 THEN
                         '    ' || CC.CODIGO
                        WHEN 11 THEN
                         '      ' || CC.CODIGO
                        WHEN 14 THEN
                         '        ' || CC.CODIGO
                        WHEN 17 THEN
                         '          ' || CC.CODIGO
                      END Nro_Cuenta,
                      cc.codigo,
                      CC.NOMBRE,
                      DECODE(cc.imputable, 'S', 'S', 'N') IMPUTABLE,
                      nvl(sum(adet.debe), 0) DEBE_PERIODO,
                      nvl(sum(adet.haber), 0) HABER_PERIODO,
                      0 DEBE_ACUMULADO,
                      0 HABER_ACUMULADO,
                      V_UNIDAD_ADM AS P_UNIDAD_ADM,
                      V_EJERCICIO AS P_EJERCICIO,
                      DECODE(V_FECHA_INICIO, NULL, '--/--/--', V_FECHA_INICIO) P_FECHA_INICIO,
                      DECODE(V_FECHA_CORTE, NULL, '--/--/--', V_FECHA_CORTE) P_FECHA_CORTE,
                      DECODE(V_NIVEL, NULL, '--', V_NIVEL) P_NIVEL

                from EJECUCION.t_cuentas_contables         cc,
                      PRESUPUESTO.t_unidades_administrativa ua,
                      EJECUCION.t_asientos                  a,
                      EJECUCION.t_asientos_det              adet,
                      EJECUCION.t_cuentas_contables         cta
               where cc.Id_Jurisdiccion = ua.Id_Jurisdiccion
                    --and cta.Id_Jurisdiccion = ua.Id_Jurisdiccion
                    --    and ua.numero_unidad_adm = V_NRO_UNIDAD_ADM
                    --AND ua.id_vigencia = p_ID_VIGENCIA--comentado Gali 4113
                 and cta.id_cta_contable = adet.id_cta_contable
                 and adet.id_asiento = a.id_asiento
                    --and (adet.anulado = 'N' or adet.anulado is null)
                 AND (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') BETWEEN
                     TO_DATE(V_FECHA_INICIO, 'DD/MM/RRRR') AND
                     TO_DATE(V_FECHA_CORTE, 'DD/MM/RRRR'))

                 and cta.CODIGO <> '01.01.01.02.36'
                 and cc.CODIGO <> '01.01.01.02.36'
                 AND ADET.ID_CTA_CONTABLE <> 11323
                 and cc.imputable = 'N'
                 and cta.codigo like cc.codigo || '%'
               group by cc.codigo, CC.NOMBRE, cc.imputable

              UNION

              --ACUMULADO - CUENTAS IMPUTABLES
              select CASE LENGTH(CC.CODIGO)
                        WHEN 2 THEN
                         CC.CODIGO
                        WHEN 5 THEN
                         '  ' || CC.CODIGO
                        WHEN 8 THEN
                         '    ' || CC.CODIGO
                        WHEN 11 THEN
                         '      ' || CC.CODIGO
                        WHEN 14 THEN
                         '        ' || CC.CODIGO
                        WHEN 17 THEN
                         '          ' || CC.CODIGO
                      END Nro_Cuenta,
                      cc.codigo,
                      CC.NOMBRE,
                      DECODE(cc.imputable, 'S', 'S', 'N') IMPUTABLE,
                      0 DEBE_PERIODO,
                      0 HABER_PERIODO,
                      nvl(sum(adet.debe), 0) DEBE_ACUMULADO,
                      nvl(sum(adet.haber), 0) HABER_ACUMULADO,
                      V_UNIDAD_ADM AS P_UNIDAD_ADM,
                      V_EJERCICIO AS P_EJERCICIO,
                      DECODE(V_FECHA_INICIO, NULL, '--/--/--', V_FECHA_INICIO) P_FECHA_INICIO,
                      DECODE(V_FECHA_CORTE, NULL, '--/--/--', V_FECHA_CORTE) P_FECHA_CORTE,
                      DECODE(V_NIVEL, NULL, '--', V_NIVEL) P_NIVEL

                from EJECUCION.t_cuentas_contables         cc,
                      PRESUPUESTO.t_unidades_administrativa ua,
                      EJECUCION.t_asientos                  a,
                      EJECUCION.t_asientos_det              adet
               where cc.Id_Jurisdiccion = ua.Id_Jurisdiccion
                    --     and ua.numero_unidad_adm = V_NRO_UNIDAD_ADM
                    --AND ua.id_vigencia = p_ID_VIGENCIA--comentado Gali 4113
                 and cc.id_cta_contable = adet.id_cta_contable
                 and adet.id_asiento = a.id_asiento
                    --and (adet.anulado = 'N' or adet.anulado is null)
                    /*                 AND (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') BETWEEN
                    TO_DATE(V_FECHA_INICIO_ACUM, 'DD/MM/RRRR') AND
                    TO_DATE(V_FECHA_CORTE_ACUM, 'DD/MM/RRRR'))*/

                 AND (((CC.CODIGO NOT LIKE '04%' AND
                     CC.CODIGO NOT LIKE '05%') AND
                     (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                     V_FECHA_INICIO_ACUM) AND
                     (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                     V_FECHA_CORTE_ACUM)))

                 and cc.CODIGO <> '01.01.01.02.36'
                 AND ADET.ID_CTA_CONTABLE <> 11323

                 and cc.imputable = 'S'
               group by cc.codigo, CC.NOMBRE, cc.imputable

              UNION

              --ACUMULADO - CUENTAS NO IMPUTABLES
              select CASE LENGTH(CC.CODIGO)
                        WHEN 2 THEN
                         CC.CODIGO
                        WHEN 5 THEN
                         '  ' || CC.CODIGO
                        WHEN 8 THEN
                         '    ' || CC.CODIGO
                        WHEN 11 THEN
                         '      ' || CC.CODIGO
                        WHEN 14 THEN
                         '        ' || CC.CODIGO
                        WHEN 17 THEN
                         '          ' || CC.CODIGO
                      END Nro_Cuenta,
                      cc.codigo,
                      CC.NOMBRE,
                      DECODE(cc.imputable, 'S', 'S', 'N') IMPUTABLE,
                      0 DEBE_PERIODO,
                      0 HABER_PERIODO,
                      nvl(sum(adet.debe), 0) DEBE_ACUMULADO,
                      nvl(sum(adet.haber), 0) HABER_ACUMULADO,
                      V_UNIDAD_ADM AS P_UNIDAD_ADM,
                      V_EJERCICIO AS P_EJERCICIO,
                      DECODE(V_FECHA_INICIO, NULL, '--/--/--', V_FECHA_INICIO) P_FECHA_INICIO,
                      DECODE(V_FECHA_CORTE, NULL, '--/--/--', V_FECHA_CORTE) P_FECHA_CORTE,
                      DECODE(V_NIVEL, NULL, '--', V_NIVEL) P_NIVEL

                from EJECUCION.t_cuentas_contables         cc,
                      PRESUPUESTO.t_unidades_administrativa ua,
                      EJECUCION.t_asientos                  a,
                      EJECUCION.t_asientos_det              adet,
                      EJECUCION.t_cuentas_contables         cta
               where cc.Id_Jurisdiccion = ua.Id_Jurisdiccion
                    --  and cta.Id_Jurisdiccion = ua.Id_Jurisdiccion
                    --    and ua.numero_unidad_adm = V_NRO_UNIDAD_ADM
                    --AND ua.id_vigencia = p_ID_VIGENCIA--comentado Gali 4113
                 and cta.id_cta_contable = adet.id_cta_contable
                 and adet.id_asiento = a.id_asiento
                    --and (adet.anulado = 'N' or adet.anulado is null)
                    /*                 AND (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') BETWEEN
                    TO_DATE(V_FECHA_INICIO_ACUM, 'DD/MM/RRRR') AND
                    TO_DATE(V_FECHA_CORTE_ACUM, 'DD/MM/RRRR'))*/

                 AND (((CC.CODIGO NOT LIKE '04%' AND
                     CC.CODIGO NOT LIKE '05%') AND
                     (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') >=
                     V_FECHA_INICIO_ACUM) AND
                     (TO_DATE(A.FEC_CONTABLE, 'DD/MM/RRRR') <=
                     V_FECHA_CORTE_ACUM)))

                 and cc.imputable = 'N'
                 and cta.CODIGO <> '01.01.01.02.36'
                 and cc.CODIGO <> '01.01.01.02.36'
                 AND ADET.ID_CTA_CONTABLE <> 11323
                 and cta.codigo like cc.codigo || '%'
               group by cc.codigo, CC.NOMBRE, cc.imputable) tabla
      -- where CODIGO <> '01.01.01.02.36'
       group by Nro_Cuenta,
                CODIGO,
                Nombre,
                IMPUTABLE,
                P_UNIDAD_ADM,
                P_EJERCICIO,
                P_FECHA_INICIO,
                P_FECHA_CORTE,
                P_NIVEL

       order by tabla.codigo;
  END PRC_SUMAS_Y_SALDOS;

END PKG_REPORTES_CONTABILIDAD;

PACKAGE "PKG_REPORTES_CONTABILIDAD" AS

  TYPE TY_CURSOR IS REF CURSOR;

  PROCEDURE PRC_LIBRO_DIARIO

  (P_T_CURSOR      OUT TY_CURSOR,
   P_EJERCICIO     IN presupuesto.T_VIGENCIAS.EJERCICIO%TYPE DEFAULT NULL,
   P_FECHA_CORTE   IN ejecucion.T_ASIENTOS_DET.FEC_CONTABLE%TYPE DEFAULT NULL,
   P_FECHA_INICIO  IN ejecucion.T_ASIENTOS_DET.FEC_CONTABLE%TYPE DEFAULT NULL,
   P_ID_UNIDAD_ADM IN presupuesto.T_UNIDADES_ADMINISTRATIVA.ID_UNIDAD_ADM%TYPE DEFAULT NULL);

  PROCEDURE PRC_MAYORES(p_T_CURSOR        OUT TY_CURSOR,
                        P_EJERCICIO       IN PRESUPUESTO.T_VIGENCIAS.EJERCICIO %TYPE DEFAULT NULL,
                        P_FECHA_CORTE     IN DATE DEFAULT NULL,
                        P_ID_CTA_CONTABLE IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE %TYPE DEFAULT NULL,
                        P_FECHA_INICIO    IN DATE DEFAULT NULL,
                        P_UNIDAD_ADM      IN PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA.ID_UNIDAD_ADM %TYPE DEFAULT NULL,
                        P_JURISDICCION    IN PRESUPUESTO.T_JURISDICCIONES.ID_JURISDICCION %TYPE DEFAULT NULL);

  PROCEDURE PRC_SUMAS_Y_SALDOS

  (p_T_CURSOR     OUT TY_CURSOR,
   p_id_vigencia  IN PRESUPUESTO.T_VIGENCIAS.ID_VIGENCIA%TYPE DEFAULT NULL,
   p_FECHA_CORTE  IN DATE DEFAULT NULL,
   p_nivel        IN NUMBER DEFAULT NULL,
   p_FECHA_INICIO IN DATE DEFAULT NULL,
   p_unidad_adm   IN PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA.ID_UNIDAD_ADM%TYPE DEFAULT NULL);

END PKG_REPORTES_CONTABILIDAD;

