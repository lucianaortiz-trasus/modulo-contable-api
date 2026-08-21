PACKAGE BODY PKG_T_ASIENTOS AS
    PROCEDURE INSERTAR(p_ID_ASIENTO      OUT EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE,
                       p_NUMERO          IN EJECUCION.T_ASIENTOS.NUMERO%TYPE DEFAULT NULL,
                       p_ANIO            IN EJECUCION.T_ASIENTOS.ANIO%TYPE DEFAULT NULL,
                       p_ID_VIGENCIA     IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL,
                       p_ID_UNIDAD_ADM   IN EJECUCION.T_ANTICIPOS.ID_UNIDAD_ADM%TYPE DEFAULT NULL,
                       p_OBSERVACIONES   IN EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE DEFAULT NULL,
                       p_FEC_ALTA        IN EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE DEFAULT NULL,
                       p_USER_ALTA       IN EJECUCION.T_ASIENTOS.USER_ALTA%TYPE DEFAULT NULL,
                       p_FEC_ACT         IN EJECUCION.T_ASIENTOS.FEC_ACT%TYPE DEFAULT NULL,
                       p_USER_ACT        IN EJECUCION.T_ASIENTOS.USER_ACT%TYPE DEFAULT NULL,
                       p_ID_ASIENTO_REL  IN EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE DEFAULT NULL,
                       p_ID_RUR          IN EJECUCION.T_ASIENTOS.ID_RUR%TYPE DEFAULT NULL,
                       p_leyenda         IN EJECUCION.T_ASIENTOS.LEYENDA%TYPE DEFAULT NULL,
                       p_fec_contable    IN EJECUCION.T_ASIENTOS.FEC_CONTABLE%TYPE DEFAULT NULL,
                       p_id_tipo_asiento IN EJECUCION.T_ASIENTOS.ID_TIPO_ASIENTO%TYPE DEFAULT NULL,
                       p_MSG             IN OUT VARCHAR2) IS
        v_NUMERO       number;
        v_EJERCICIO    number;
        V_fec_contable DATE;

        v_ANIO            EJECUCION.T_ASIENTOS.ANIO%TYPE;
        v_ID_VIGENCIA     EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE;
        v_ID_UNIDAD_ADM   EJECUCION.T_ANTICIPOS.ID_UNIDAD_ADM%TYPE;
        V_ID_JURISDICCION ejecucion.t_asientos.id_jurisdiccion%type;
        v_OBSERVACIONES   EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE;
        v_FEC_ALTA        EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE;
        v_USER_ALTA       EJECUCION.T_ASIENTOS.USER_ALTA%TYPE;
        v_FEC_ACT         EJECUCION.T_ASIENTOS.FEC_ACT%TYPE;
        v_USER_ACT        EJECUCION.T_ASIENTOS.USER_ACT%TYPE;
        v_ID_ASIENTO_REL  EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE;
        v_ID_RUR          EJECUCION.T_ASIENTOS.ID_RUR%TYPE;
        v_leyenda         EJECUCION.T_ASIENTOS.LEYENDA%TYPE;
        v_id_tipo_asiento EJECUCION.T_ASIENTOS.ID_TIPO_ASIENTO%TYPE;

    BEGIN

        v_ANIO            := p_ANIO;
        v_ID_VIGENCIA     := p_ID_VIGENCIA;
        v_ID_UNIDAD_ADM   := p_ID_UNIDAD_ADM;
        v_OBSERVACIONES   := p_OBSERVACIONES;
        v_FEC_ALTA        := p_FEC_ALTA;
        v_USER_ALTA       := p_USER_ALTA;
        v_FEC_ACT         := p_FEC_ACT;
        v_USER_ACT        := p_USER_ACT;
        v_ID_ASIENTO_REL  := p_ID_ASIENTO_REL;
        v_ID_RUR          := p_ID_RUR;
        v_leyenda         := p_leyenda;
        v_id_tipo_asiento := p_id_tipo_asiento;

        SELECT TO_NUMBER(TO_CHAR(p_fec_contable, 'YYYY')),
               p_fec_contable,
               V.ID_VIGENCIA
          INTO V_ANIO, V_fec_contable, V_ID_VIGENCIA
          FROM PRESUPUESTO.T_VIGENCIAS V
         WHERE V.EJERCICIO = TO_NUMBER(TO_CHAR(p_fec_contable, 'YYYY'));

        SELECT UA.ID_JURISDICCION
          INTO V_ID_JURISDICCION
          FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA,
               PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA_ORIG
         WHERE UA.NUMERO_UNIDAD_ADM = UA_ORIG.NUMERO_UNIDAD_ADM
           AND UA_ORIG.ID_UNIDAD_ADM = v_ID_UNIDAD_ADM
           AND UA.ID_VIGENCIA = V_ID_VIGENCIA;

        SELECT EJECUCION.F_NUMERO_ASIENTO(V_ID_JURISDICCION)
          INTO V_NUMERO
          FROM DUAL;

        SELECT EJECUCION.Sq_t_Asientos.NEXTVAL INTO p_ID_ASIENTO FROM DUAL;


        INSERT INTO EJECUCION.T_ASIENTOS
            (ID_ASIENTO,
             NUMERO,
             ANIO,
             ID_VIGENCIA,
             ID_JURISDICCION,
             OBSERVACIONES,
             FEC_ALTA,
             USER_ALTA,
             FEC_ACT,
             USER_ACT,
             ID_ASIENTO_REL,
             ID_RUR,
             LEYENDA,
             FEC_CONTABLE,
             ID_TIPO_ASIENTO)
        VALUES
            (p_ID_ASIENTO,
             v_NUMERO, --EJECUCION.f_Numero_Asiento(p_ANIO),
             v_ANIO,
             v_ID_VIGENCIA,
             V_ID_JURISDICCION,
             v_OBSERVACIONES,
             v_FEC_ALTA,
             v_USER_ALTA,
             v_FEC_ACT,
             v_USER_ACT,
             v_ID_ASIENTO_REL,
             v_ID_RUR,
             v_leyenda,
             V_fec_contable, --   p_fec_contable,
             v_id_tipo_asiento);

    EXCEPTION
        WHEN v_ERROR_00001 THEN
            p_MSG := 'El codigo ya existe.' || v_NUMERO;
            PKG_UTIL.BASE_ROLLBACK;
        WHEN v_ERROR_01407 THEN
            p_MSG := 'No se puede insertar valores nulos.';
            PKG_UTIL.BASE_ROLLBACK;
        WHEN OTHERS THEN
            p_MSG := 'Error al INSERTAR. Verifique: ' || SQLERRM;
            PKG_UTIL.BASE_ROLLBACK;

    END INSERTAR;
    /*
      PROCEDURE EDITAR(p_ID_ASIENTO     IN EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE DEFAULT NULL,
                       p_NUMERO         IN EJECUCION.T_ASIENTOS.NUMERO%TYPE DEFAULT NULL,
                       p_ANIO           IN EJECUCION.T_ASIENTOS.ANIO%TYPE DEFAULT NULL,
                       p_ID_VIGENCIA    IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL,
                       p_ID_UNIDAD_ADM  IN EJECUCION.T_ASIENTOS.ID_UNIDAD_ADM%TYPE DEFAULT NULL,
                       p_OBSERVACIONES  IN EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE DEFAULT NULL,
                       p_FEC_ALTA       IN EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE DEFAULT NULL,
                       p_USER_ALTA      IN EJECUCION.T_ASIENTOS.USER_ALTA%TYPE DEFAULT NULL,
                       p_FEC_ACT        IN EJECUCION.T_ASIENTOS.FEC_ACT%TYPE DEFAULT NULL,
                       p_USER_ACT       IN EJECUCION.T_ASIENTOS.USER_ACT%TYPE DEFAULT NULL,
                       p_ID_ASIENTO_REL IN EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE DEFAULT NULL,
                       p_ID_RUR         IN EJECUCION.T_ASIENTOS.ID_RUR%TYPE DEFAULT NULL,
                       p_MSG            IN OUT varchar2) IS
        v_ID_ASIENTO     EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE;
        v_NUMERO         EJECUCION.T_ASIENTOS.NUMERO%TYPE;
        v_ANIO           EJECUCION.T_ASIENTOS.ANIO%TYPE;
        v_ID_VIGENCIA    EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE;
        v_ID_UNIDAD_ADM  EJECUCION.T_ASIENTOS.ID_UNIDAD_ADM%TYPE;
        v_OBSERVACIONES  EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE;
        v_FEC_ALTA       EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE;
        v_USER_ALTA      EJECUCION.T_ASIENTOS.USER_ALTA%TYPE;
        v_FEC_ACT        EJECUCION.T_ASIENTOS.FEC_ACT%TYPE;
        v_USER_ACT       EJECUCION.T_ASIENTOS.USER_ACT%TYPE;
        v_ID_ASIENTO_REL EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE;
        v_ID_RUR         EJECUCION.T_ASIENTOS.ID_RUR%TYPE;

      BEGIN
        v_ID_ASIENTO     := p_ID_ASIENTO;
        v_NUMERO         := p_NUMERO;
        v_ANIO           := p_ANIO;
        v_ID_VIGENCIA    := p_ID_VIGENCIA;
        v_ID_UNIDAD_ADM  := p_ID_UNIDAD_ADM;
        v_OBSERVACIONES  := p_OBSERVACIONES;
        v_FEC_ALTA       := p_FEC_ALTA;
        v_USER_ALTA      := p_USER_ALTA;
        v_FEC_ACT        := p_FEC_ACT;
        v_USER_ACT       := p_USER_ACT;
        v_ID_ASIENTO_REL := p_ID_ASIENTO_REL;
        v_ID_RUR         := p_ID_RUR;

        UPDATE EJECUCION.T_ASIENTOS
           SET NUMERO         = v_NUMERO,
               ANIO           = v_ANIO,
               ID_VIGENCIA    = v_ID_VIGENCIA,
               ID_UNIDAD_ADM  = v_ID_UNIDAD_ADM,
               OBSERVACIONES  = v_OBSERVACIONES,
               FEC_ALTA       = v_FEC_ALTA,
               USER_ALTA      = v_USER_ALTA,
               FEC_ACT        = v_FEC_ACT,
               USER_ACT       = v_USER_ACT,
               ID_ASIENTO_REL = v_ID_ASIENTO_REL,
               ID_RUR         = v_ID_RUR
         WHERE ID_ASIENTO = v_ID_ASIENTO;

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

      PROCEDURE ELIMINAR(p_ID_ASIENTO IN EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE DEFAULT NULL,
                         p_MSG        IN OUT varchar2) IS
        v_ID_ASIENTO EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE;
      BEGIN
        v_ID_ASIENTO := p_ID_ASIENTO;

        DELETE FROM EJECUCION.T_ASIENTOS WHERE ID_ASIENTO = v_ID_ASIENTO;

      exception
        when v_error_2292 then
          p_MSG := 'No se puede Eliminar el registro existen relaciones para ese ejercicio asociadas a otras tablas.';
          pkg_util.base_rollback;
        when others then
          p_MSG := 'Error al ELIMINAR. Verifique: ' || sqlerrm;
          pkg_util.base_rollback;

      END ELIMINAR;
    */
    PROCEDURE BUSCARPORPK(p_T_ASIENTOS OUT ty_CURSOR,
                          p_ID_ASIENTO IN EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE DEFAULT NULL) IS
        v_ID_ASIENTO EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE;
    BEGIN
        v_ID_ASIENTO := p_ID_ASIENTO;

        OPEN p_T_ASIENTOS FOR
            SELECT EJECUCION.T_ASIENTOS.ID_ASIENTO,
                   EJECUCION.T_ASIENTOS.NUMERO,
                   EJECUCION.T_ASIENTOS.ANIO,
                   EJECUCION.T_ASIENTOS.ID_VIGENCIA,
                   EJECUCION.T_ASIENTOS.ID_JURISDICCION,
                   (SELECT UA1.ID_UNIDAD_ADM
                      FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA1
                     WHERE UA1.ID_JURISDICCION =
                           EJECUCION.T_ASIENTOS.ID_JURISDICCION) ID_UNIDAD_ADM,
                   EJECUCION.T_ASIENTOS.OBSERVACIONES,
                   EJECUCION.T_ASIENTOS.FEC_ALTA,
                   EJECUCION.T_ASIENTOS.USER_ALTA,
                   EJECUCION.T_ASIENTOS.FEC_ACT,
                   EJECUCION.T_ASIENTOS.USER_ACT,
                   EJECUCION.T_ASIENTOS.ID_ASIENTO_REL,
                   EJECUCION.T_ASIENTOS.ID_RUR,
                   EJECUCION.T_ASIENTOS.ID_TIPO_ASIENTO,
                   EJECUCION.T_ASIENTOS.FEC_CONTABLE,
                   EJECUCION.T_ASIENTOS.LEYENDA
              FROM EJECUCION.T_ASIENTOS
             WHERE (EJECUCION.T_ASIENTOS.ID_ASIENTO = v_ID_ASIENTO);
    END BUSCARPORPK;

     PROCEDURE BUSCAR_ASIENTO_INGR_BI(p_T_ASIENTOS  OUT ty_CURSOR,
                                       p_ID_ANTICIPO IN EJECUCION.T_ASIENTOS.ID_ANTICIPO%TYPE DEFAULT NULL,
                                       p_ID_EP       IN EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO%TYPE DEFAULT NULL,
                                       p_TIPO_DOC    IN varchar2) IS
        p_LEYENDA     varchar2(100);
        v_ID_ANTICIPO EJECUCION.T_ASIENTOS.ID_ANTICIPO%TYPE;
        v_ID_EP       EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO%TYPE;
        v_TIPO_DOC    varchar2(500);

      BEGIN
        v_ID_ANTICIPO := p_ID_ANTICIPO;
        v_ID_EP       := p_ID_EP;
        v_TIPO_DOC    := p_TIPO_DOC;

        OPEN p_T_ASIENTOS FOR
        SELECT 1 FROM DUAL WHERE 1= 0;
        --Si es un ingreso de un ajuste de anticipo
       /* IF v_TIPO_DOC = 'A' THEN
          select cc.leyenda
            INTO p_LEYENDA
            from EJECUCION.t_codigos_contabilizacion cc
           where cc.tabla_origen = 'BANDEJADEINGRESO'
             and cc.tipo_mov_bing = 'ANT';

          OPEN p_T_ASIENTOS FOR
            SELECT ASI.ID_ASIENTO,
                   ASI.NUMERO,
                   ASI.ANIO,
                   ASI.ID_VIGENCIA,
                   (SELECT UA.ID_UNIDAD_ADM FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA WHERE UA.ID_JURISDICCION = ASI.ID_JURISDICCION) ID_UNIDAD_ADM,
                   ASI.OBSERVACIONES,
                   ASI.FEC_ALTA,
                   ASI.USER_ALTA,
                   ASI.FEC_ACT,
                   ASI.USER_ACT,
                   ASI.ID_ASIENTO_REL,
                   ASI.ID_ANTICIPO,
                   ASI.ID_TIPO_ASIENTO,
                   ASI.FEC_CONTABLE,
                   ASI.LEYENDA,
                   null id_elemento_pago
              FROM EJECUCION.T_ASIENTOS ASI
             WHERE ASI.ID_ANTICIPO = v_ID_ANTICIPO --DEL AJUSTE DE ANTICIPO
               and asi.leyenda = p_LEYENDA --'INGRESO DEV. ANTICIPO'
               and asi.id_asiento_rel is null --No es un contraasiento
               and asi.id_asiento not in
                   (select id_asiento_rel
                      from EJECUCION.t_asientos
                     where id_asiento_rel is not null); --No esta relacionado a ningun contraasiento
        END IF;
*/       --Si es un ingreso de una OE
      /*  IF v_TIPO_DOC = 'OE' THEN
          select cc.leyenda
            INTO p_LEYENDA
            from EJECUCION.t_codigos_contabilizacion cc
           where cc.tabla_origen = 'BANDEJADEINGRESO'
             and cc.tipo_mov_bing = 'OE';

          OPEN p_T_ASIENTOS FOR
            SELECT ASI.ID_ASIENTO,
                   ASI.NUMERO,
                   ASI.ANIO,
                   ASI.ID_VIGENCIA,
                   (SELECT UA.ID_UNIDAD_ADM FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA WHERE UA.ID_JURISDICCION = ASI.ID_JURISDICCION) ID_UNIDAD_ADM,
                   ASI.OBSERVACIONES,
                   ASI.FEC_ALTA,
                   ASI.USER_ALTA,
                   ASI.FEC_ACT,
                   ASI.USER_ACT,
                   ASI.ID_ASIENTO_REL,
                   ASI.ID_ANTICIPO,
                   ASI.ID_TIPO_ASIENTO,
                   ASI.FEC_CONTABLE,
                   ASI.LEYENDA,
                   ASD.ID_ELEMENTO_PAGO
              FROM EJECUCION.T_ASIENTOS ASI, EJECUCION.T_ASIENTOS_DET ASD
             WHERE ASD.ID_ELEMENTO_PAGO = v_ID_EP
               and ASD.ID_ASIENTO = ASI.ID_ASIENTO
               and ASD.debe is not null
               and ASD.anulado = 'N'
               and asi.leyenda = p_LEYENDA --'PAGO OE CON CHEQUE'
               and asi.id_asiento_rel is null --No es un contraasiento
               and asi.id_asiento not in
                   (select id_asiento_rel
                      from EJECUCION.t_asientos
                     where id_asiento_rel is not null); --No esta relacionado a ningun contraasiento
        END IF;
*/--        Si es un ingreso de una OP
      /*  IF v_TIPO_DOC = 'OP' THEN
          select cc.leyenda
            INTO p_LEYENDA
            from EJECUCION.t_codigos_contabilizacion cc
           where cc.tabla_origen = 'BANDEJADEINGRESO'
             and cc.tipo_mov_bing = 'OP';

          OPEN p_T_ASIENTOS FOR
            SELECT ASI.ID_ASIENTO,
                   ASI.NUMERO,
                   ASI.ANIO,
                   ASI.ID_VIGENCIA,
                   (SELECT UA.ID_UNIDAD_ADM FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA WHERE UA.ID_JURISDICCION = ASI.ID_JURISDICCION) ID_UNIDAD_ADM,
                   ASI.OBSERVACIONES,
                   ASI.FEC_ALTA,
                   ASI.USER_ALTA,
                   ASI.FEC_ACT,
                   ASI.USER_ACT,
                   ASI.ID_ASIENTO_REL,
                   ASI.ID_ANTICIPO,
                   ASI.ID_TIPO_ASIENTO,
                   ASI.FEC_CONTABLE,
                   ASI.LEYENDA,
                   ASD.ID_ELEMENTO_PAGO
              FROM EJECUCION.T_ASIENTOS ASI, EJECUCION.T_ASIENTOS_DET ASD
             WHERE ASD.ID_ELEMENTO_PAGO = v_ID_EP
               and ASD.ID_ASIENTO = ASI.ID_ASIENTO
               and ASD.debe is not null
               and ASD.anulado = 'N'
               and asi.leyenda = p_LEYENDA --'PAGO OP CON CHEQUE'
               and asi.id_asiento_rel is null --No es un contraasiento
               and asi.id_asiento not in
                   (select id_asiento_rel
                      from EJECUCION.t_asientos
                     where id_asiento_rel is not null); --No esta relacionado a ningun contraasiento
        END IF;
*/      END BUSCAR_ASIENTO_INGR_BI;

      PROCEDURE INSERTAR_CONTRAASIENTO_INGR_BI(p_ID_ASIENTO      OUT EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE,
                                               p_ID_ASIENTO_ORIG IN EJECUCION.T_ASIENTOS.ID_ASIENTO %TYPE DEFAULT NULL,
                                               p_ID_LIBRO_BANCO  IN EJECUCION.T_LIBRO_BANCO.ID_LIBRO_BANCO %TYPE DEFAULT NULL,
                                               p_TIPO_DOC        IN varchar2,
                                               p_MSG             IN OUT VARCHAR) IS

        V_id_cta_contable_orig NUMBER(10);
        V_tipo_saldo_orig      varchar2(2);
        V_leyenda              varchar2(100);
        V_id_cta_contable_dest NUMBER(10);
        V_tipo_saldo_dest      varchar2(2);

        V_id_tipo_asiento NUMBER(10);

        V_id_recurso_debe_orig  NUMBER(10);
        V_id_recurso_haber_orig NUMBER(10);
        V_id_recurso_debe_dest  NUMBER(10);
        V_id_recurso_haber_dest NUMBER(10);

        V_id_unidad_adm_orig      NUMBER(10);
        V_id_vigencia_orig        NUMBER(10);
        V_id_asiento_orig         number(10);
        V_id_anticipo_ajuste_orig number(10);
        V_fec_contable_orig       varchar2(20);
        V_ejercicio_orig          NUMBER(10);
        V_monto_orig              number(18, 2);
        V_id_elemento_pago_orig   number(10);
        v_es_administracion_gral          number(10);
        V_numero_asiento  varchar2(11);
        V_id_asiento      number(10);
        V_id_asiento_det  number(10);
        V_importe_d       number(18, 2);
        V_importe_h       number(18, 2);
        V_id_cta_contable NUMBER(10);
        V_id_recurso      NUMBER(10);

        /******************************************************************************************/
        /*Gali 12716 - Se agrego para calcular correctamente la fecha de anulacion para el asiento*/
        /******************************************************************************************/
        V_fecha_anulacion varchar2(20);
        /******************************************************************************************/
        --p_MSG varchar(200);

        v_ID_LIBRO_BANCO EJECUCION.T_LIBRO_BANCO.ID_LIBRO_BANCO%TYPE;
        v_TIPO_DOC       varchar2(500);

      BEGIN
        v_ID_LIBRO_BANCO := p_ID_LIBRO_BANCO;
        v_TIPO_DOC       := p_TIPO_DOC;
        /*Se hace el contraasiento del ingreso del ajuste positivo del anticipo*/
        /*Se hace el contraasiento de BI para el Ajuste Positivo del Anticipo Ingresado*/
        /*IF v_TIPO_DOC= 'A' THEN

                \*Se traen los datos necesarios para PD para contraasiento del ingreso del ajuste positivo del Anticipo*\

                select  cc.id_cta_contable, cc.tipo_saldo, cc.leyenda,
                        cc.id_cta_contable_dest, cc.tipo_saldo_dest,
                        cc.id_tipo_asiento,
                        (select ctc.id_recurso_debe
                        from EJECUCION.t_cuentas_contables ctc
                        where cc.id_cta_contable=ctc.id_cta_contable) id_recurso_debe_orig,
                        (select ctc.id_recurso_haber
                        from EJECUCION.t_cuentas_contables ctc
                        where cc.id_cta_contable=ctc.id_cta_contable) id_recurso_haber_orig,
                        (select ctc.id_recurso_debe
                        from EJECUCION.t_cuentas_contables ctc
                        where cc.id_cta_contable_dest=ctc.id_cta_contable) id_recurso_debe_dest,
                        (select ctc.id_recurso_haber
                        from EJECUCION.t_cuentas_contables ctc
                        where cc.id_cta_contable_dest=ctc.id_cta_contable) id_recurso_haber_dest
                INTO    V_id_cta_contable_orig, V_tipo_saldo_orig, V_leyenda,
                        V_id_cta_contable_dest, V_tipo_saldo_dest,
                        V_id_tipo_asiento,V_id_recurso_debe_orig, V_id_recurso_haber_orig,
                        V_id_recurso_debe_dest, V_id_recurso_haber_dest
                from  EJECUCION.t_codigos_contabilizacion cc
                where   cc.tabla_origen='BANDEJADEINGRESO'
                        and cc.tipo_mov_bing='ANT';

                select  asd.id_asiento, asd.debe, asd.id_elemento_pago,
                        asi.id_vigencia,  ASI.ID_JURISDICCION, --(SELECT UA.ID_UNIDAD_ADM FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA WHERE UA.ID_JURISDICCION = ASI.ID_JURISDICCION),
                        asi.id_anticipo, asi.fec_contable, asi.anio
                INTO    V_id_asiento_orig, V_monto_orig, V_id_elemento_pago_orig,
                        V_id_vigencia_orig, V_id_unidad_adm_orig,
                        V_id_anticipo_ajuste_orig, V_fec_contable_orig, V_ejercicio_orig
                from    EJECUCION.t_asientos_det asd, EJECUCION.T_ASIENTOS asi
                where   ASI.ID_ASIENTO= p_ID_ASIENTO_ORIG
                        and asd.id_asiento = asi.id_asiento
                        and asd.debe is not null
                        and asd.anulado = 'N';

                        select count(ua.id_unidad_adm) into v_es_administracion_gral
      from PRESUPUESTO.t_unidades_administrativa ua
     where ua.id_unidad_adm = V_id_unidad_adm_orig
       and ua.id_jurisdiccion in
           (select j.id_jurisdiccion from PRESUPUESTO.t_jurisdicciones j where j.id_grupo_sector = (select gs.id_grupo_sector
                                                                       from PRESUPUESTO.t_grupo_sectores gs
                                                                      where gs.id_vigencia = V_id_vigencia_orig
                                                                        and gs.numero_grupo_sector =
                                                                            '1010101'));

                 \*Se calcula la fecha de anulacion*\
                 \******************************************************************************************\
                 \*Gali 12716 - Se agrego para calcular correctamente la fecha de anulacion para el asiento*\
                 \******************************************************************************************\
                 IF TO_CHAR (to_date(V_fec_contable_orig,'DD/MM/YYYY'),'YYYY') < TO_CHAR (sysdate,'YYYY') THEN
                    V_fecha_anulacion := to_date('31/12/' || TO_CHAR (to_date(V_fec_contable_orig,'DD/MM/YYYY'),'YYYY'),'DD/MM/YYYY');--,'YYYY');
                 ELSE
                    V_fecha_anulacion := sysdate;
                 END IF;
                \******************************************************************************************\

                 \*Se insertan los datos para PD para contraasiento del ingreso del ajuste positivo del Anticipo*\
    if v_es_administracion_gral <> 0 then

                V_numero_asiento := null;--f_Numero_Asiento(V_ejercicio_orig) ; se comenta porque no se coloca al generar nro asiento

                select EJECUCION.Sq_t_Asientos.nextval
                INTO V_id_asiento
                from dual;

                INSERT INTO EJECUCION.T_ASIENTOS
                (
                id_asiento,
                numero,
                anio,
                id_vigencia,
                id_jurisdiccion,
                observaciones,
                user_alta,
                fec_alta,
                user_act,
                fec_act,
                id_asiento_rel,
                id_anticipo,
                id_tipo_asiento,
                fec_contable,
                leyenda
                )
                VALUES
                (
                V_id_asiento,
                V_numero_asiento,
                V_ejercicio_orig, --v_EJERCICIO,--esta bien???? o V_ejercicio_orig del asiento original
                V_id_vigencia_orig,--del asiento original
                V_id_unidad_adm_orig,--del asiento original
                null,  --observacion
                user,
                sysdate,
                null,
                null,
                V_id_asiento_orig, --id_asiento original al que le pertenece este contraasiento --del asiento original
                V_id_anticipo_ajuste_orig, --Del Ajuste --del asiento original
                V_id_tipo_asiento,
                \*****************************************************************************************************\
                 \*Gali 12716 - Se modifico para guardar correctamente la fecha de anulacion calculada para el asiento*\
                 \*****************************************************************************************************\
                 V_fecha_anulacion,--sysdate, --o V_fec_contable_orig = ta.fecha_deposito, --del asiento original
                 \*****************************************************************************************************\
                \**************************************************************************\
                 \*12862 - Se agrega la palabra ANULA delante de la leyenda que corresponda*\
                 \**************************************************************************\
                 'ANULA ' ||
                 \**************************************************************************\
                 V_leyenda
                );

                \**\
                \**\
                FOR i in 1 .. 2 LOOP
                 select  EJECUCION.Sq_t_Asientos_det.nextval
                       INTO V_id_asiento_det
                       from dual;

                       if i = 1  then  -- CARGO EL REGISTRO DEL ORIGEN
                         V_id_cta_contable := V_id_cta_contable_dest;
                         IF V_tipo_saldo_dest = 'H' THEN
                               V_importe_d := V_monto_orig; --//entLroBco.monto=entBan.importe=ta.importe_ajuste importe, --del asiento detalle original
                               V_importe_h := NULL;
                               V_id_recurso := V_id_recurso_debe_dest;
                          ELSE
                               V_importe_d := NULL;
                               V_importe_h := V_monto_orig; --//entLroBco.monto=entBan.importe=ta.importe_ajuste importe, --del asiento detalle original
                               V_id_recurso := V_id_recurso_haber_dest;
                          END IF;
                       else  -- CARGO EL REGISTRO DEL DESTINO
                         V_id_cta_contable :=  V_id_cta_contable_orig;
                          IF  V_tipo_saldo_orig = 'H' THEN
                               V_importe_d := V_monto_orig; --//entLroBco.monto=entBan.importe=ta.importe_ajuste importe, --del asiento detalle original
                               V_importe_h := NULL;
                               V_id_recurso := V_id_recurso_debe_orig;
                          ELSE
                               V_importe_d := NULL;
                               V_importe_h := V_monto_orig; --//entLroBco.monto=entBan.importe=ta.importe_ajuste importe, --del asiento detalle original
                               V_id_recurso := V_id_recurso_haber_orig;
                          END IF;
                       end if;

                       INSERT INTO EJECUCION.T_ASIENTOS_DET
                       (
                       id_asiento_det,
                       id_asiento,
                       id_cta_contable,
                       fec_contable,
                       debe,
                       haber,
                       id_elemento_pago,
                       user_ing,
                       fec_ing,
                       user_act,
                       fec_act,
                       anulado,
                       id_recurso_d_h
                       )
                       VALUES
                       (
                       V_id_asiento_det,
                       V_id_asiento,
                       V_id_cta_contable,
                       \*************************************************************************************************************\
                         \*Gali 12716 - Se modifico para guardar correctamente la fecha de anulacion calculada para el asiento detalle*\
                         \*************************************************************************************************************\
                         V_fecha_anulacion,--sysdate, --o V_fec_contable_orig = ta.fecha_deposito --del asiento original
                         \*************************************************************************************************************\
                       V_importe_d, -- debe
                       V_importe_h, -- haber
                       V_id_elemento_pago_orig, --del asiento detalle original
                       user,
                       sysdate,
                       null,
                       null,
                       'N',
                       V_id_recurso
                       );
                END LOOP;
                \**\
                \**\
               p_ID_ASIENTO := V_id_asiento;
               end if;
         END IF;
*/
         /*Se hace el contraasiento del ingreso de la OE*/
        /*Se hace el contraasiento de BI para la OE Ingresada*/
       /* IF v_TIPO_DOC= 'OE' THEN

               \*Se traen los datos necesarios para PD para contraasiento del ingreso de la OE*\

                select  cc.id_cta_contable, cc.tipo_saldo, cc.leyenda,
                        cc.id_cta_contable_dest, cc.tipo_saldo_dest,
                        cc.id_tipo_asiento,
                        (select ctc.id_recurso_debe
                        from EJECUCION.t_cuentas_contables ctc
                        where cc.id_cta_contable=ctc.id_cta_contable) id_recurso_debe_orig,
                        (select ctc.id_recurso_haber
                        from EJECUCION.t_cuentas_contables ctc
                        where cc.id_cta_contable=ctc.id_cta_contable) id_recurso_haber_orig,
                        (select ctc.id_recurso_debe
                        from EJECUCION.t_cuentas_contables ctc
                        where cc.id_cta_contable_dest=ctc.id_cta_contable) id_recurso_debe_dest,
                        (select ctc.id_recurso_haber
                        from EJECUCION.t_cuentas_contables ctc
                        where cc.id_cta_contable_dest=ctc.id_cta_contable) id_recurso_haber_dest
                INTO    V_id_cta_contable_orig, V_tipo_saldo_orig, V_leyenda,
                        V_id_cta_contable_dest, V_tipo_saldo_dest,
                        V_id_tipo_asiento,V_id_recurso_debe_orig, V_id_recurso_haber_orig,
                        V_id_recurso_debe_dest, V_id_recurso_haber_dest
                from  EJECUCION.t_codigos_contabilizacion cc
                where   cc.tabla_origen='BANDEJADEINGRESO'
                        and cc.tipo_mov_bing='OE';

                select  asd.id_asiento, asd.debe, asd.id_elemento_pago,
                          asi.id_vigencia, (SELECT UA.ID_UNIDAD_ADM FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA WHERE UA.ID_JURISDICCION = ASI.ID_JURISDICCION),
                          asi.id_anticipo, asi.fec_contable, asi.anio
                 INTO     V_id_asiento_orig, V_monto_orig, V_id_elemento_pago_orig,
                          V_id_vigencia_orig, V_id_unidad_adm_orig,
                          V_id_anticipo_ajuste_orig, V_fec_contable_orig, V_ejercicio_orig
                 from     EJECUCION.t_asientos_det asd, EJECUCION.T_ASIENTOS asi
                 where    ASI.ID_ASIENTO= p_ID_ASIENTO_ORIG
                          and asd.id_asiento = asi.id_asiento
                          and asd.debe is not null
                          and asd.anulado = 'N';

     select count(ua.id_unidad_adm) into v_es_administracion_gral
      from PRESUPUESTO.t_unidades_administrativa ua
     where ua.id_unidad_adm = V_id_unidad_adm_orig
       and ua.id_jurisdiccion in
           (select j.id_jurisdiccion from PRESUPUESTO.t_jurisdicciones j where j.id_grupo_sector = (select gs.id_grupo_sector
                                                                       from PRESUPUESTO.t_grupo_sectores gs
                                                                      where gs.id_vigencia = V_id_vigencia_orig
                                                                        and gs.numero_grupo_sector =
                                                                            '1010101'));

                 \*Se calcula la fecha de anulacion*\
                 \******************************************************************************************\
                 \*Gali 12716 - Se agrego para calcular correctamente la fecha de anulacion para el asiento*\
                 \******************************************************************************************\
                 IF TO_CHAR (to_date(V_fec_contable_orig,'DD/MM/YYYY'),'YYYY') < TO_CHAR (sysdate,'YYYY') THEN
                    V_fecha_anulacion := to_date('31/12/' || TO_CHAR (to_date(V_fec_contable_orig,'DD/MM/YYYY'),'YYYY'),'DD/MM/YYYY');--,'YYYY');
                 ELSE
                    V_fecha_anulacion := sysdate;
                 END IF;

                 \*Se insertan los datos para PD para contraasiento del ingreso de la OE*\
    if v_es_administracion_gral <> 0 then

                 V_numero_asiento := null;--f_Numero_Asiento(V_ejercicio_orig) ; se comenta porque no se coloca al generar nro asiento

                 select EJECUCION.Sq_t_Asientos.nextval
                 INTO V_id_asiento
                 from dual;

                 INSERT INTO EJECUCION.T_ASIENTOS
                 (
                 id_asiento,
                 numero,
                 anio,
                 id_vigencia,
                 id_unidad_adm,
                 observaciones,
                 user_alta,
                 fec_alta,
                 user_act,
                 fec_act,
                 id_asiento_rel,
                 id_anticipo,
                 id_tipo_asiento,
                 fec_contable,
                 leyenda
                 )
                 VALUES
                 (
                 V_id_asiento,
                 V_numero_asiento,
                 V_ejercicio_orig, --v_EJERCICIO,--esta bien???? o V_ejercicio_orig del asiento original
                 V_id_vigencia_orig,--del asiento original
                 V_id_unidad_adm_orig,--del asiento original
                 null,  --observacion
                 user,
                 sysdate,
                 null,
                 null,
                 V_id_asiento_orig,--id_asiento original al que le pertenece este contraasiento --del asiento original
                 null, --V_id_anticipo_ajuste_orig, --Del Ajuste --del asiento original
                V_id_tipo_asiento,
                \*****************************************************************************************************\
                 \*Gali 12716 - Se modifico para guardar correctamente la fecha de anulacion calculada para el asiento*\
                 \*****************************************************************************************************\
                 V_fecha_anulacion,--sysdate, --o V_fec_contable_orig = ep.fecha_deposito, --del asiento original
                 \*****************************************************************************************************\
                \**************************************************************************\
                 \*12862 - Se agrega la palabra ANULA delante de la leyenda que corresponda*\
                 \**************************************************************************\
                 'ANULA ' ||
                 \**************************************************************************\
                 V_leyenda
                );

                \**\
                \**\
                FOR i in 1 .. 2 LOOP
                 select  EJECUCION.Sq_t_Asientos_det.nextval
                       INTO V_id_asiento_det
                       from dual;

                       if i = 1  then  -- CARGO EL REGISTRO DEL ORIGEN
                         V_id_cta_contable := V_id_cta_contable_dest;
                         IF V_tipo_saldo_dest = 'H' THEN
                               V_importe_d := V_monto_orig; --//entLroBco.monto=ep.monto_pagar --del asiento detalle original
                               V_importe_h := NULL;
                               V_id_recurso := V_id_recurso_debe_dest;
                          ELSE
                               V_importe_d := NULL;
                               V_importe_h := V_monto_orig; --//entLroBco.monto=ep.monto_pagar --del asiento detalle original
                               V_id_recurso := V_id_recurso_haber_dest;
                          END IF;
                       else  -- CARGO EL REGISTRO DEL DESTINO
                         V_id_cta_contable :=  V_id_cta_contable_orig;
                          IF  V_tipo_saldo_orig = 'H' THEN
                               V_importe_d := V_monto_orig; --//entLroBco.monto=ep.monto_pagar --del asiento detalle original
                               V_importe_h := NULL;
                               V_id_recurso := V_id_recurso_debe_orig;
                          ELSE
                               V_importe_d := NULL;
                               V_importe_h := V_monto_orig; --//entLroBco.monto=ep.monto_pagar --del asiento detalle original
                               V_id_recurso := V_id_recurso_haber_orig;
                          END IF;
                       end if;

                       INSERT INTO EJECUCION.T_ASIENTOS_DET
                       (
                       id_asiento_det,
                       id_asiento,
                       id_cta_contable,
                       fec_contable,
                       debe,
                       haber,
                       id_elemento_pago,
                       user_ing,
                       fec_ing,
                       user_act,
                       fec_act,
                       anulado,
                       id_recurso_d_h
                       )
                       VALUES
                       (
                       V_id_asiento_det,
                       V_id_asiento,
                       V_id_cta_contable,
                       \*************************************************************************************************************\
                         \*Gali 12716 - Se modifico para guardar correctamente la fecha de anulacion calculada para el asiento detalle*\
                         \*************************************************************************************************************\
                         V_fecha_anulacion,--sysdate, --o V_fec_contable_orig = ep.fecha_deposito --del asiento original
                       \*************************************************************************************************************\
                         V_importe_d, -- debe
                       V_importe_h, -- haber
                       V_id_elemento_pago_orig, --del asiento detalle original
                       user,
                       sysdate,
                       null,
                       null,
                       'N',
                       V_id_recurso
                       );
                END LOOP;
                \**\
                \**\
               p_ID_ASIENTO := V_id_asiento;
               end if;
         END IF;
*/
        /*Se hace el contraasiento del ingreso de la OP*/
        /*Se hace el contraasiento de BI para la OP Ingresada*/
       /* IF v_TIPO_DOC= 'OP' THEN

               \*Se traen los datos necesarios para PD para contraasiento del ingreso de la OP*\

                select  cc.id_cta_contable, cc.tipo_saldo, cc.leyenda,
                        cc.id_cta_contable_dest, cc.tipo_saldo_dest,
                        cc.id_tipo_asiento,
                        (select ctc.id_recurso_debe
                        from EJECUCION.t_cuentas_contables ctc
                        where cc.id_cta_contable=ctc.id_cta_contable) id_recurso_debe_orig,
                        (select ctc.id_recurso_haber
                        from EJECUCION.t_cuentas_contables ctc
                        where cc.id_cta_contable=ctc.id_cta_contable) id_recurso_haber_orig,
                        (select ctc.id_recurso_debe
                        from EJECUCION.t_cuentas_contables ctc
                        where cc.id_cta_contable_dest=ctc.id_cta_contable) id_recurso_debe_dest,
                        (select ctc.id_recurso_haber
                        from EJECUCION.t_cuentas_contables ctc
                        where cc.id_cta_contable_dest=ctc.id_cta_contable) id_recurso_haber_dest
                INTO    V_id_cta_contable_orig, V_tipo_saldo_orig, V_leyenda,
                        V_id_cta_contable_dest, V_tipo_saldo_dest,
                        V_id_tipo_asiento,V_id_recurso_debe_orig, V_id_recurso_haber_orig,
                        V_id_recurso_debe_dest, V_id_recurso_haber_dest
                from  EJECUCION.t_codigos_contabilizacion cc
                where   cc.tabla_origen='BANDEJADEINGRESO'
                        and cc.tipo_mov_bing='OP';

                select  asd.id_asiento, asd.debe, asd.id_elemento_pago,
                        asi.id_vigencia, (SELECT UA.ID_UNIDAD_ADM FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA WHERE UA.ID_JURISDICCION = ASI.ID_JURISDICCION),
                        asi.id_anticipo, asi.fec_contable, asi.anio
                INTO    V_id_asiento_orig, V_monto_orig, V_id_elemento_pago_orig,
                        V_id_vigencia_orig, V_id_unidad_adm_orig,
                        V_id_anticipo_ajuste_orig, V_fec_contable_orig, V_ejercicio_orig
                from    EJECUCION.t_asientos_det asd, EJECUCION.T_ASIENTOS asi
                where   ASI.ID_ASIENTO= p_ID_ASIENTO_ORIG
                        and asd.id_asiento = asi.id_asiento
                        and asd.debe is not null
                        and asd.anulado = 'N';

     select count(ua.id_unidad_adm) into v_es_administracion_gral
      from PRESUPUESTO.t_unidades_administrativa ua
     where ua.id_unidad_adm = V_id_unidad_adm_orig
       and ua.id_jurisdiccion in
           (select j.id_jurisdiccion from PRESUPUESTO.t_jurisdicciones j where j.id_grupo_sector = (select gs.id_grupo_sector
                                                                       from PRESUPUESTO.t_grupo_sectores gs
                                                                      where gs.id_vigencia = V_id_vigencia_orig
                                                                        and gs.numero_grupo_sector =
                                                                            '1010101'));

                 \*Se calcula la fecha de anulacion*\
                 \******************************************************************************************\
                 \*Gali 12716 - Se agrego para calcular correctamente la fecha de anulacion para el asiento*\
                 \******************************************************************************************\
                 IF TO_CHAR (to_date(V_fec_contable_orig,'DD/MM/YYYY'),'YYYY') < TO_CHAR (sysdate,'YYYY') THEN
                    V_fecha_anulacion := to_date('31/12/' || TO_CHAR (to_date(V_fec_contable_orig,'DD/MM/YYYY'),'YYYY'),'DD/MM/YYYY');--,'YYYY');
                 ELSE
                    V_fecha_anulacion := sysdate;
                 END IF;
                 \******************************************************************************************\

                 \*Se insertan los datos para PD para contraasiento del ingreso de la OP*\
    if v_es_administracion_gral <> 0 then

                V_numero_asiento := null;--f_Numero_Asiento(V_ejercicio_orig) ; se comenta porque no se coloca al generar nro asiento

                select EJECUCION.Sq_t_Asientos.nextval
                INTO V_id_asiento
                from dual;

                INSERT INTO EJECUCION.T_ASIENTOS
                (
                id_asiento,
                numero,
                anio,
                id_vigencia,
                id_unidad_adm,
                observaciones,
                user_alta,
                fec_alta,
                user_act,
                fec_act,
                id_asiento_rel,
                id_anticipo,
                id_tipo_asiento,
                fec_contable,
                leyenda
                 \************************************************************************\
                 \*12959 - Se agrega guardar la relacion del id_libro_banco en el asiento*\
                 \************************************************************************\
                 , id_libro_banco
                 \************************************************************************\
                )
                VALUES
                (
                V_id_asiento,
                V_numero_asiento,
                V_ejercicio_orig, --v_EJERCICIO,--esta bien???? o V_ejercicio_orig del asiento original
                V_id_vigencia_orig,--del asiento original
                V_id_unidad_adm_orig,--del asiento original
                null,  --observacion
                user,
                sysdate,
                null,
                null,
                V_id_asiento_orig,--id_asiento original al que le pertenece este contraasiento --del asiento original
                null, --V_id_anticipo_ajuste_orig, --Del Ajuste --del asiento original
                V_id_tipo_asiento,
                 \*****************************************************************************************************\
                 \*Gali 12716 - Se modifico para guardar correctamente la fecha de anulacion calculada para el asiento*\
                 \*****************************************************************************************************\
                V_fecha_anulacion,--sysdate, --o V_fec_contable_orig = ep.fecha_deposito --del asiento original
                 \*****************************************************************************************************\
                \**************************************************************************\
                 \*12862 - Se agrega la palabra ANULA delante de la leyenda que corresponda*\
                 \**************************************************************************\
                 'ANULA ' ||
                 \**************************************************************************\
                 V_leyenda
                 \************************************************************************\
                 \*12959 - Se agrega guardar la relacion del id_libro_banco en el asiento*\
                 \************************************************************************\
                 , v_ID_LIBRO_BANCO
                 \************************************************************************\
                );

                \**\
                \**\
                FOR i in 1 .. 2 LOOP
                 select  EJECUCION.Sq_t_Asientos_det.nextval
                       INTO V_id_asiento_det
                       from dual;

                       if i = 1  then  -- CARGO EL REGISTRO DEL ORIGEN
                         V_id_cta_contable := V_id_cta_contable_dest;
                         IF V_tipo_saldo_dest = 'H' THEN
                               V_importe_d := V_monto_orig; --//entLroBco.monto=ep.monto_pagar --del asiento detalle original
                               V_importe_h := NULL;
                               V_id_recurso := V_id_recurso_debe_dest;
                          ELSE
                               V_importe_d := NULL;
                               V_importe_h := V_monto_orig; --//entLroBco.monto=ep.monto_pagar --del asiento detalle original
                               V_id_recurso := V_id_recurso_haber_dest;
                          END IF;
                       else  -- CARGO EL REGISTRO DEL DESTINO
                         V_id_cta_contable :=  V_id_cta_contable_orig;
                          IF  V_tipo_saldo_orig = 'H' THEN
                               V_importe_d := V_monto_orig; --//entLroBco.monto=ep.monto_pagar --del asiento detalle original
                               V_importe_h := NULL;
                               V_id_recurso := V_id_recurso_debe_orig;
                          ELSE
                               V_importe_d := NULL;
                               V_importe_h := V_monto_orig; --//entLroBco.monto=ep.monto_pagar --del asiento detalle original
                               V_id_recurso := V_id_recurso_haber_orig;
                          END IF;
                       end if;

                       INSERT INTO EJECUCION.T_ASIENTOS_DET
                       (
                       id_asiento_det,
                       id_asiento,
                       id_cta_contable,
                       fec_contable,
                       debe,
                       haber,
                       id_elemento_pago,
                       user_ing,
                       fec_ing,
                       user_act,
                       fec_act,
                       anulado,
                       id_recurso_d_h
                       )
                       VALUES
                       (
                       V_id_asiento_det,
                       V_id_asiento,
                       V_id_cta_contable,
                       \*************************************************************************************************************\
                         \*Gali 12716 - Se modifico para guardar correctamente la fecha de anulacion calculada para el asiento detalle*\
                         \*************************************************************************************************************\
                         V_fecha_anulacion,--sysdate, --o V_fec_contable_orig = ep.fecha_deposito --del asiento original
                         \*************************************************************************************************************\
                       V_importe_d, -- debe
                       V_importe_h, -- haber
                       V_id_elemento_pago_orig, --del asiento detalle original
                       user,
                       sysdate,
                       null,
                       null,
                       'N',
                       V_id_recurso
                       );
                END LOOP;
                \**\
                \**\
               p_ID_ASIENTO := V_id_asiento;
         END IF;
         end if;
*/
      /*EXCEPTION
        when others then
          insert into EJECUCION.t_log_pd
            (ID_LOG_PD, TABLA, IDENTIFICADOR, COMENTARIO, FECHA, USUARIO)
          VALUES
            (EJECUCION.sq_t_log_pd.nextval,
             'EJECUCION.T_LIBRO_BANCO',
             v_ID_LIBRO_BANCO,
             'ERROR AL INSERTAR ASIENTO O REVERSA DE ASIENTO, POR ANULACION DE INGRESO DE BI',
             sysdate,
             user);
          p_MSG := 'Error en  GENERACION_ASIENTOS ' || sqlerrm;
          dbms_output.put_line(p_MSG);*/

      END INSERTAR_CONTRAASIENTO_INGR_BI;
      /***********************************************************/

    PROCEDURE BUSCAR(p_T_ASIENTOS          OUT ty_CURSOR,
                     p_ID_ASIENTO          IN EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE DEFAULT NULL,
                     p_NUMERO              IN EJECUCION.T_ASIENTOS.NUMERO%TYPE DEFAULT NULL,
                     p_ANIO                IN EJECUCION.T_ASIENTOS.ANIO%TYPE DEFAULT NULL,
                     p_ID_VIGENCIA         IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL,
                     p_ID_UNIDAD_ADM       IN EJECUCION.T_ANTICIPOS.ID_UNIDAD_ADM%TYPE DEFAULT NULL,
                     p_OBSERVACIONES       IN EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE DEFAULT NULL,
                     p_FEC_ALTA            IN EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE DEFAULT NULL,
                     p_USER_ALTA           IN EJECUCION.T_ASIENTOS.USER_ALTA%TYPE DEFAULT NULL,
                     p_FEC_ACT             IN EJECUCION.T_ASIENTOS.FEC_ACT%TYPE DEFAULT NULL,
                     p_USER_ACT            IN EJECUCION.T_ASIENTOS.USER_ACT%TYPE DEFAULT NULL,
                     p_ID_ASIENTO_REL      IN EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE DEFAULT NULL,
                     p_ID_RUR              IN EJECUCION.T_ASIENTOS.ID_RUR%TYPE DEFAULT NULL,
                     p_PAGINA              IN NUMBER DEFAULT NULL,
                     p_TAMANOPAGINA        IN NUMBER DEFAULT NULL,
                     p_CANTIDADDEREGISTROS OUT NUMBER

                     ) IS
        v_ID_ASIENTO     EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE;
        v_NUMERO         EJECUCION.T_ASIENTOS.NUMERO%TYPE;
        v_ANIO           EJECUCION.T_ASIENTOS.ANIO%TYPE;
        v_ID_VIGENCIA    EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE;
        v_ID_UNIDAD_ADM  EJECUCION.T_ANTICIPOS.ID_UNIDAD_ADM%TYPE;
        v_OBSERVACIONES  EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE;
        v_FEC_ALTA       EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE;
        v_USER_ALTA      EJECUCION.T_ASIENTOS.USER_ALTA%TYPE;
        v_FEC_ACT        EJECUCION.T_ASIENTOS.FEC_ACT%TYPE;
        v_USER_ACT       EJECUCION.T_ASIENTOS.USER_ACT%TYPE;
        v_ID_ASIENTO_REL EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE;
        v_ID_RUR         EJECUCION.T_ASIENTOS.ID_RUR%TYPE;
        v_PAGINA         NUMBER;
        v_TAMANOPAGINA   NUMBER;

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
        v_ID_ASIENTO     := p_ID_ASIENTO;
        v_NUMERO         := p_NUMERO;
        v_ANIO           := p_ANIO;
        v_ID_VIGENCIA    := p_ID_VIGENCIA;
        v_ID_UNIDAD_ADM  := p_ID_UNIDAD_ADM;
        v_OBSERVACIONES  := p_OBSERVACIONES;
        v_FEC_ALTA       := p_FEC_ALTA;
        v_USER_ALTA      := p_USER_ALTA;
        v_FEC_ACT        := p_FEC_ACT;
        v_USER_ACT       := p_USER_ACT;
        v_ID_ASIENTO_REL := p_ID_ASIENTO_REL;
        v_ID_RUR         := p_ID_RUR;
        v_PAGINA         := p_PAGINA;
        v_TAMANOPAGINA   := p_TAMANOPAGINA;

        BEGIN
            v_SELECT_COUNT := 'SELECT COUNT(*) ';
            v_FROM_COUNT   := 'FROM EJECUCION.T_ASIENTOS JOIN EJECUCION.T_TIPO_ASIENTO ON EJECUCION.T_ASIENTOS.ID_TIPO_ASIENTO =  EJECUCION.T_TIPO_ASIENTO.ID_TIPO_ASIENTO ';
            v_WHERE_COUNT  := '';
            v_AND          := 'WHERE';

            IF v_ID_ASIENTO IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_ASIENTOS.ID_ASIENTO = ' ||
                                 v_ID_ASIENTO;
                v_AND         := ' AND';
            END IF;

            IF v_NUMERO IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_ASIENTOS.NUMERO = ' ||
                                 v_NUMERO;
                v_AND         := ' AND';
            END IF;

            IF v_ANIO IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_ASIENTOS.ANIO = ' || v_ANIO;
                v_AND         := ' AND';
            END IF;

            IF v_ID_VIGENCIA IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_ASIENTOS.ID_VIGENCIA = ' ||
                                 v_ID_VIGENCIA;
                v_AND         := ' AND';
            END IF;

            IF v_ID_UNIDAD_ADM IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_ASIENTOS.ID_JURISDICCION = (
      SELECT UA1.ID_JURISDICCION FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA1
      WHERE UA1.ID_UNIDAD_ADM = ' ||
                                 v_ID_UNIDAD_ADM || ' ) ';
                v_AND         := ' AND';
            END IF;

            IF v_OBSERVACIONES IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_ASIENTOS.OBSERVACIONES LIKE ''%' ||
                                 v_OBSERVACIONES || '%''';
                v_AND         := ' AND';
            END IF;

            IF TO_DATE(v_FEC_ALTA, 'DD/MM/YYYY') IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_ASIENTOS.FEC_ALTA = ''' ||
                                 v_FEC_ALTA || '''';
                v_AND         := ' AND';
            END IF;

            IF v_USER_ALTA IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_ASIENTOS.USER_ALTA LIKE ''%' ||
                                 v_USER_ALTA || '%''';
                v_AND         := ' AND';
            END IF;

            IF TO_DATE(v_FEC_ACT, 'DD/MM/YYYY') IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_ASIENTOS.FEC_ACT = ''' ||
                                 v_FEC_ACT || '''';
                v_AND         := ' AND';
            END IF;

            IF v_USER_ACT IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_ASIENTOS.USER_ACT LIKE ''%' ||
                                 v_USER_ACT || '%''';
                v_AND         := ' AND';
            END IF;

            IF v_ID_ASIENTO_REL IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_ASIENTOS.ID_ASIENTO_REL = ' ||
                                 v_ID_ASIENTO_REL;
                v_AND         := ' AND';
            END IF;

            IF v_ID_RUR IS NOT NULL THEN
                v_WHERE_COUNT := v_WHERE_COUNT || v_AND ||
                                 ' EJECUCION.T_ASIENTOS.ID_RUR = ' ||
                                 v_ID_RUR;
            END IF;

            v_CANTIDAD := v_SELECT_COUNT || v_FROM_COUNT || v_WHERE_COUNT;
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
                               'EJECUCION.T_ASIENTOS.ID_ASIENTO, ' ||
                               'EJECUCION.T_ASIENTOS.NUMERO, ' ||
                               'EJECUCION.T_ASIENTOS.ANIO, ' ||
                               'EJECUCION.T_ASIENTOS.ID_VIGENCIA, ' ||
                               ' EJECUCION.T_ASIENTOS.ID_JURISDICCION, ' ||
                               ' (SELECT UA1.ID_UNIDAD_ADM
                            FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA1
                            WHERE UA1.ID_JURISDICCION = EJECUCION.T_ASIENTOS.ID_JURISDICCION) ID_UNIDAD_ADM, ' ||
                               'EJECUCION.T_ASIENTOS.OBSERVACIONES, ' ||
                               'EJECUCION.T_ASIENTOS.FEC_ALTA, ' ||
                               'EJECUCION.T_ASIENTOS.USER_ALTA, ' ||
                               'EJECUCION.T_ASIENTOS.FEC_ACT, ' ||
                               'EJECUCION.T_ASIENTOS.USER_ACT, ' ||
                               'EJECUCION.T_ASIENTOS.ID_ASIENTO_REL, ' ||
                               'EJECUCION.T_ASIENTOS.ID_RUR, ' ||
                               'EJECUCION.T_ASIENTOS.id_tipo_asiento, ' ||
                               'EJECUCION.T_ASIENTOS.leyenda, ' ||
                               'DECODE(EJECUCION.T_TIPO_ASIENTO.id_tipo_asiento, 1, ''AUTOMATICO'', ''MANUAL'') descripcion, ' ||
                               'EJECUCION.T_ASIENTOS.fec_contable ';

            v_SUB_CONSULTA := v_SELECT || v_FROM_COUNT || v_WHERE_COUNT ||
                              ' ORDER BY 1 DESC'; --Falta colocar columna de Ordenamiento
            v_WHERE_PAGINA := ' WHERE NUMERODEREGISTRO > ' || v_INICIO || '' ||
                              ' AND NUMERODEREGISTRO <= ' || v_FIN || '';

            v_BUSCAR := v_SELECT_PAGINA || v_SUB_CONSULTA || ')V )' ||
                        v_WHERE_PAGINA || ' ORDER BY 1 DESC'; --Falta colocar columna de Ordenamiento

        END;

        OPEN p_T_ASIENTOS FOR v_BUSCAR;
    END BUSCAR;

    PROCEDURE CONSULTAR_UA(p_T_ASIENTOS  OUT ty_CURSOR,
                           p_ID_VIGENCIA IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL) IS
        v_ID_VIGENCIA EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE;
    BEGIN
        v_ID_VIGENCIA := p_ID_VIGENCIA;

        OPEN p_T_ASIENTOS FOR
            SELECT UA.ESTADO
              FROM PRESUPUESTO.t_unidades_administrativa ua
             WHERE ua.id_vigencia = v_ID_VIGENCIA
             GROUP BY UA.ESTADO;

    END CONSULTAR_UA;

    /*
      PROCEDURE BUSCARSIGNUM(P_ID_VIGENCIA IN PRESUPUESTO.T_VIGENCIAS.ID_VIGENCIA%TYPE DEFAULT NULL,
                             sig_nume      OUT VARCHAR2,
                             P_MSG         OUT VARCHAR2) IS
        v_ejercicio   number;
        v_ID_VIGENCIA EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE;
      BEGIN
        v_ID_VIGENCIA := P_ID_VIGENCIA;

        select v.ejercicio
          into v_ejercicio
          from PRESUPUESTO.t_vigencias v
         where v.id_vigencia = v_ID_VIGENCIA;

        select null --EJECUCION.f_Numero_Asiento(v_ejercicio)--nvl(max(a.numero )  ,0)  +1  se comenta porque no se coloca al generar nro asiento
          into sig_nume
          from dual;
        --where a.id_vigencia= P_ID_VIGENCIA ;

      EXCEPTION
        WHEN NO_DATA_FOUND THEN
          sig_nume := 1;
        WHEN OTHERS THEN
          p_msg := 'Error en PKG_T_ASIENTOS.BUSCARSIGNUM en T_ASIENTOS Verifique: ' ||
                   sqlerrm;

      END BUSCARSIGNUM;
    */
    PROCEDURE TRAER_RECURSO_DEBE(p_T_ASIENTOS OUT ty_CURSOR,
                                 p_ID_CC      IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                                 p_TIPO_SALDO IN EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE DEFAULT NULL) IS
        v_ID_CC      EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE;
        v_TIPO_SALDO EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE;
    BEGIN
        v_ID_CC      := p_ID_CC;
        v_TIPO_SALDO := p_TIPO_SALDO;

        OPEN p_T_ASIENTOS FOR
            SELECT DISTINCT cc.id_recurso_debe,
                            --r.descripcion,
                            '' descripcion,
                            cc.id_recurso_haber
              FROM t_cuentas_contables cc /*, t_recurso_d_h r*/
             WHERE cc.id_cta_contable = v_ID_CC
            /*\*AND   cc.tipo_saldo = p_TIPO_SALDO*\*/
            /* AND cc.id_recurso_debe = r.id_recurso_d_h*/
            ;

    END TRAER_RECURSO_DEBE;

    PROCEDURE TRAER_RECURSO_HABER(p_T_ASIENTOS OUT ty_CURSOR,
                                  p_ID_CC      IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                                  p_TIPO_SALDO IN EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE DEFAULT NULL) IS
        v_ID_CC      EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE;
        v_TIPO_SALDO EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE;
    BEGIN
        v_ID_CC      := p_ID_CC;
        v_TIPO_SALDO := p_TIPO_SALDO;

        OPEN p_T_ASIENTOS FOR
            SELECT DISTINCT cc.id_recurso_haber,
                            --  r.descripcion,
                            '' descripcion,
                            cc.id_recurso_debe
              FROM t_cuentas_contables cc /*, t_recurso_d_h r*/
             WHERE cc.id_cta_contable = v_ID_CC
            /*\*AND   cc.tipo_saldo = p_TIPO_SALDO*\
            AND cc.id_recurso_haber = r.id_recurso_d_h*/
            ;

    END TRAER_RECURSO_HABER;
    /*
      PROCEDURE INSERTAR_REVERSA(p_ID_VIGENCIA     IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL,
                                 p_ID_UNIDAD_ADM   IN EJECUCION.T_ASIENTOS.ID_UNIDAD_ADM%TYPE DEFAULT NULL,
                                 p_NUMERO          IN EJECUCION.T_ASIENTOS.NUMERO%TYPE DEFAULT NULL,
                                 p_FEC_CONTABLE    IN EJECUCION.T_ASIENTOS.FEC_CONTABLE%TYPE DEFAULT NULL,
                                 p_ID_TIPO_ASIENTO IN EJECUCION.T_ASIENTOS.ID_TIPO_ASIENTO%TYPE DEFAULT NULL,
                                 p_LEYENDA         IN EJECUCION.T_ASIENTOS.LEYENDA%TYPE DEFAULT NULL,
                                 p_OBSERVACIONES   IN EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE DEFAULT NULL,
                                 p_USER_ING        IN EJECUCION.T_ASIENTOS.USER_ALTA%TYPE DEFAULT NULL,
                                 p_FEC_ING         IN EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE DEFAULT NULL,
                                 p_USER_ACT        IN EJECUCION.T_ASIENTOS.USER_ACT%TYPE DEFAULT NULL,
                                 p_FEC_ACT         IN EJECUCION.T_ASIENTOS.FEC_ACT%TYPE DEFAULT NULL,
                                 p_ID_ASIENTO_REL  IN EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE,
                                 p_MSG             IN OUT VARCHAR2) IS
        p_ID_ASIENTO_REVERSA NUMBER;
        NUEVO_NRO_ASIENTO    VARCHAR2(10);
        v_EJERCICIO          NUMBER;
        v_NUMERO             NUMBER;
        v_FECHA_CONTABLE     DATE;
        v_NUEVA_FECHA        DATE;
        v_NUMERO_REVERSA     NUMBER;
        v_es_administracion_gral          number(10);
      BEGIN

        SELECT EJECUCION.Sq_t_Asientos.NEXTVAL INTO p_ID_ASIENTO_REVERSA FROM DUAL;

        SELECT V.EJERCICIO
          INTO v_EJERCICIO
          FROM PRESUPUESTO.T_VIGENCIAS V
         WHERE V.ID_VIGENCIA = p_ID_VIGENCIA;

        \*select nvl(max(a.numero )  ,0)  +1
        into NUEVO_NRO_ASIENTO
        from EJECUCION.T_ASIENTOS a
        where a.id_vigencia= P_ID_VIGENCIA ;*\

        SELECT A.NUMERO, A.FEC_CONTABLE
          INTO v_NUMERO, v_FECHA_CONTABLE
          FROM EJECUCION.T_ASIENTOS A
         WHERE A.ID_ASIENTO = p_ID_ASIENTO_REL;

        IF (v_FECHA_CONTABLE < SYSDATE) THEN
          v_NUEVA_FECHA := TO_DATE('31/12/' || v_EJERCICIO, 'DD/MM/YYYY');
        ELSE
          v_NUEVA_FECHA := SYSDATE;

        END IF;
     select count(ua.id_unidad_adm) into v_es_administracion_gral
      from PRESUPUESTO.t_unidades_administrativa ua
     where ua.id_unidad_adm = p_ID_UNIDAD_ADM
       and ua.id_jurisdiccion in
           (select j.id_jurisdiccion from PRESUPUESTO.t_jurisdicciones j where j.id_grupo_sector = (select gs.id_grupo_sector
                                                                       from PRESUPUESTO.t_grupo_sectores gs
                                                                      where gs.id_vigencia = p_ID_VIGENCIA
                                                                        and gs.numero_grupo_sector =
                                                                            '1010101'));

       if v_es_administracion_gral <> 0 then
        v_NUMERO_REVERSA := null; --EJECUCION.f_Numero_Asiento(v_EJERCICIO); se comenta porque no se coloca al generar nro asiento

        INSERT INTO EJECUCION.T_ASIENTOS
          (ID_ASIENTO,
           NUMERO,
           ANIO,
           ID_VIGENCIA,
           ID_UNIDAD_ADM,
           OBSERVACIONES,
           FEC_ALTA,
           USER_ALTA,
           FEC_ACT,
           USER_ACT,
           ID_ASIENTO_REL,
           ID_TIPO_ASIENTO,
           FEC_CONTABLE,
           LEYENDA)
        VALUES
          (p_ID_ASIENTO_REVERSA,
           v_NUMERO_REVERSA,
           v_EJERCICIO,
           p_ID_VIGENCIA,
           p_ID_UNIDAD_ADM,
           p_OBSERVACIONES,
           p_FEC_ING,
           p_USER_ING,
           p_FEC_ACT,
           p_USER_ACT,
           p_ID_ASIENTO_REL,
           p_ID_TIPO_ASIENTO,
           v_NUEVA_FECHA,
           'REVERSA ASIENTO ' || v_NUMERO

           );

        INSERT INTO EJECUCION.T_ASIENTOS_DET
          (ID_ASIENTO_DET,
           ID_ASIENTO,
           ID_CTA_CONTABLE,
           FEC_CONTABLE,
           HABER,
           DEBE,
           USER_ING,
           FEC_ING,
           USER_ACT,
           FEC_ACT,
           ID_RUR_DET,
           ANULADO,
           ID_RECURSO_D_H)
          SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                 p_ID_ASIENTO_REVERSA,
                 EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE,
                 v_NUEVA_FECHA,
                 EJECUCION.T_ASIENTOS_DET.DEBE,
                 EJECUCION.T_ASIENTOS_DET.HABER,
                 EJECUCION.T_ASIENTOS_DET.USER_ING,
                 EJECUCION.T_ASIENTOS_DET.FEC_ING,
                 EJECUCION.T_ASIENTOS_DET.USER_ACT,
                 EJECUCION.T_ASIENTOS_DET.FEC_ACT,
                 EJECUCION.T_ASIENTOS_DET.ID_RUR_DET,
                 'N',
                 EJECUCION.T_ASIENTOS_DET.ID_RECURSO_D_H
            FROM EJECUCION.T_ASIENTOS_DET
           WHERE (EJECUCION.T_ASIENTOS_DET.ID_ASIENTO = p_ID_ASIENTO_REL);
           end if;

      END INSERTAR_REVERSA;

      PROCEDURE INSERTAR_ASIENTO_CIERRE(p_ID_VIGENCIA IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL) IS
        v_ID_VIGENCIA       EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE;
        DIFDebeHaber        NUMBER;
        v_SALDODEBE         NUMBER;
        v_SALDOHABER        NUMBER;
        NUEVO_NRO_ASIENTO   VARCHAR2(50);
        v_UNIDAD_CENTRAL    NUMBER;
        p_ID_ASIENTO_CIERRE NUMBER;
        v_EJERCICIO         NUMBER;
        v_ID_RECUSRSO_HABER NUMBER;
        v_ID_RECUSRSO_DEBE  NUMBER;
      BEGIN
        v_ID_VIGENCIA := p_ID_VIGENCIA;

        SELECT EJECUCION.Sq_t_Asientos.NEXTVAL INTO p_ID_ASIENTO_CIERRE FROM DUAL;

        SELECT V.EJERCICIO
          INTO v_EJERCICIO
          FROM PRESUPUESTO.T_VIGENCIAS V
         WHERE V.ID_VIGENCIA = v_ID_VIGENCIA;

        \*SELECT nvl(max(a.numero )  ,0)  +1
        INTO NUEVO_NRO_ASIENTO
        FROM EJECUCION.T_ASIENTOS a
        WHERE a.id_vigencia= P_ID_VIGENCIA ;*\

        SELECT UA.ID_UNIDAD_ADM
          INTO v_UNIDAD_CENTRAL
          FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
         WHERE UA.ID_VIGENCIA = v_ID_VIGENCIA
           AND UA.NOMBRE_UNIDAD_ADM = 'Administracion Central';

        select SUM(ABS(SALDO))
          INTO v_SALDODEBE
          from vt_saldo_ctacontable vt
         where vt.id_naturaleza in (4, 5)
           and vt.tipo_saldo = 'D'
           and vt.ID_VIGENCIA = v_ID_VIGENCIA;

        select SUM(ABS(SALDO))
          INTO v_SALDOHABER
          from vt_saldo_ctacontable vt
         where vt.id_naturaleza in (4, 5)
           and vt.tipo_saldo = 'H'
           and vt.ID_VIGENCIA = v_ID_VIGENCIA;

        DIFDebeHaber := ABS(v_SALDODEBE) - ABS(v_SALDOHABER);

        INSERT INTO EJECUCION.T_ASIENTOS
          (ID_ASIENTO,
           NUMERO,
           ANIO,
           ID_VIGENCIA,
           ID_UNIDAD_ADM,
           OBSERVACIONES,
           FEC_ALTA,
           USER_ALTA,
           FEC_ACT,
           USER_ACT,
           ID_ASIENTO_REL,
           ID_TIPO_ASIENTO,
           FEC_CONTABLE,
           LEYENDA)
          SELECT p_ID_ASIENTO_CIERRE,
                 null, --EJECUCION.f_Numero_Asiento(v_EJERCICIO),--NUEVO_NRO_ASIENTO, se comenta porque no se coloca al generar nro asiento
                 v_EJERCICIO,
                 v_ID_VIGENCIA,
                 v_UNIDAD_CENTRAL,
                 'ASIENTO DE CIERRE',
                 SYSDATE,
                 'SUAF',
                 NULL,
                 NULL,
                 NULL,
                 6,
                 TO_DATE('31/12/' || v_EJERCICIO, 'DD/MM/YYYY'),
                 'ASIENTO DE CIERRE'
            FROM DUAL;

        DECLARE
          CURSOR P_T_ASIENTOS_DET IS
            SELECT *
              from vt_saldo_ctacontable vt
             where vt.id_naturaleza in (4, 5)
               and vt.ID_VIGENCIA = v_ID_VIGENCIA;

        BEGIN

          FOR R_ASIENTOS IN P_T_ASIENTOS_DET LOOP

            IF R_ASIENTOS.TIPO_SALDO = 'D' THEN

              SELECT CC.ID_RECURSO_HABER
                INTO v_ID_RECUSRSO_HABER
                FROM EJECUCION.T_CUENTAS_CONTABLES CC
               WHERE CC.ID_CTA_CONTABLE = R_ASIENTOS.ID_CTA_CONTABLE;

              INSERT INTO EJECUCION.T_ASIENTOS_DET
                (ID_ASIENTO_DET,
                 ID_ASIENTO,
                 ID_CTA_CONTABLE,
                 FEC_CONTABLE,
                 HABER,
                 DEBE,
                 USER_ING,
                 FEC_ING,
                 USER_ACT,
                 FEC_ACT,
                 ID_RUR_DET,
                 ID_RECURSO_D_H,
                 ANULADO)
                SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                       p_ID_ASIENTO_CIERRE,
                       R_ASIENTOS.ID_CTA_CONTABLE,
                       SYSDATE, --R_ASIENTOS.FEC_CONTABLE,
                       R_ASIENTOS.SALDO,
                       NULL,
                       'SUAF',
                       SYSDATE,
                       NULL,
                       NULL,
                       NULL,
                       v_ID_RECUSRSO_HABER, --R_ASIENTOS.ID_RECURSO_D_H,
                       'N'
                  FROM DUAL;

            ELSE

              SELECT CC.ID_RECURSO_DEBE
                INTO v_ID_RECUSRSO_DEBE
                FROM EJECUCION.T_CUENTAS_CONTABLES CC
               WHERE CC.ID_CTA_CONTABLE = R_ASIENTOS.ID_CTA_CONTABLE;

              INSERT INTO EJECUCION.T_ASIENTOS_DET
                (ID_ASIENTO_DET,
                 ID_ASIENTO,
                 ID_CTA_CONTABLE,
                 FEC_CONTABLE,
                 HABER,
                 DEBE,
                 USER_ING,
                 FEC_ING,
                 USER_ACT,
                 FEC_ACT,
                 ID_RUR_DET,
                 ID_RECURSO_D_H,
                 ANULADO)
                SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                       p_ID_ASIENTO_CIERRE,
                       R_ASIENTOS.ID_CTA_CONTABLE,
                       SYSDATE, --R_ASIENTOS.FEC_CONTABLE,
                       NULL,
                       R_ASIENTOS.SALDO,
                       'SUAF',
                       SYSDATE,
                       NULL,
                       NULL,
                       NULL,
                       v_ID_RECUSRSO_DEBE, --R_ASIENTOS.ID_RECURSO_D_H,
                       'N'
                  FROM DUAL;

            END IF;

          END LOOP;

        END;

        IF v_SALDOHABER > v_SALDODEBE THEN

          INSERT INTO EJECUCION.T_ASIENTOS_DET
            (ID_ASIENTO_DET,
             ID_ASIENTO,
             ID_CTA_CONTABLE,
             FEC_CONTABLE,
             HABER,
             DEBE,
             USER_ING,
             FEC_ING,
             USER_ACT,
             FEC_ACT,
             ID_RUR_DET,
             ID_RECURSO_D_H,
             ANULADO)
            SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                   p_ID_ASIENTO_CIERRE,
                   11270,
                   SYSDATE,
                   ABS(DIFDebeHaber),
                   NULL,
                   'SUAF',
                   SYSDATE,
                   NULL,
                   NULL,
                   NULL,
                   3,
                   'N'
              FROM DUAL;
        ELSE

          INSERT INTO EJECUCION.T_ASIENTOS_DET
            (ID_ASIENTO_DET,
             ID_ASIENTO,
             ID_CTA_CONTABLE,
             FEC_CONTABLE,
             HABER,
             DEBE,
             USER_ING,
             FEC_ING,
             USER_ACT,
             FEC_ACT,
             ID_RUR_DET,
             ID_RECURSO_D_H,
             ANULADO)
            SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                   p_ID_ASIENTO_CIERRE,
                   11270,
                   SYSDATE,
                   NULL,
                   ABS(DIFDebeHaber),
                   'SUAF',
                   SYSDATE,
                   NULL,
                   NULL,
                   NULL,
                   4,
                   'N'

              FROM DUAL;

        END IF;

        SELECT EJECUCION.Sq_t_Asientos.NEXTVAL INTO p_ID_ASIENTO_CIERRE FROM DUAL;

        \* SELECT nvl(max(a.numero )  ,0)  +1
                   INTO NUEVO_NRO_ASIENTO
                   FROM EJECUCION.T_ASIENTOS a
                   WHERE a.id_vigencia= P_ID_VIGENCIA ;
        *\

        SELECT UA.ID_UNIDAD_ADM
          INTO v_UNIDAD_CENTRAL
          FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
         WHERE UA.ID_VIGENCIA = v_ID_VIGENCIA
           AND UA.NOMBRE_UNIDAD_ADM = 'Administracion Central';

        select SUM(ABS(SALDO))
          INTO v_SALDODEBE
          from vt_saldo_ctacontable vt
         where vt.id_naturaleza in (1, 2)
           and vt.tipo_saldo = 'D'
           and vt.ID_VIGENCIA = v_ID_VIGENCIA;

        select SUM(ABS(SALDO))
          INTO v_SALDOHABER
          from vt_saldo_ctacontable vt
         where vt.id_naturaleza in (1, 2)
           and vt.tipo_saldo = 'H'
           and vt.ID_VIGENCIA = v_ID_VIGENCIA;

        DIFDebeHaber := ABS(v_SALDODEBE) - ABS(v_SALDOHABER);

        INSERT INTO EJECUCION.T_ASIENTOS
          (ID_ASIENTO,
           NUMERO,
           ANIO,
           ID_VIGENCIA,
           ID_UNIDAD_ADM,
           OBSERVACIONES,
           FEC_ALTA,
           USER_ALTA,
           FEC_ACT,
           USER_ACT,
           ID_ASIENTO_REL,
           ID_TIPO_ASIENTO,
           FEC_CONTABLE,
           LEYENDA)
          SELECT p_ID_ASIENTO_CIERRE,
                 null, --EJECUCION.f_Numero_Asiento(v_EJERCICIO),--NUEVO_NRO_ASIENTO, se comenta porque no se coloca al generar nro asiento
                 v_EJERCICIO,
                 v_ID_VIGENCIA,
                 v_UNIDAD_CENTRAL,
                 'ASIENTO DE CIERRE',
                 SYSDATE,
                 'SUAF',
                 NULL,
                 NULL,
                 NULL,
                 6,
                 TO_DATE('31/12/' || v_EJERCICIO, 'DD/MM/YYYY'),
                 'ASIENTO DE CIERRE'
            FROM DUAL;

        DECLARE
          CURSOR P_T_ASIENTOS_DET IS
            SELECT *
              from vt_saldo_ctacontable vt
             where vt.id_naturaleza in (1, 2)
               and vt.ID_VIGENCIA = v_ID_VIGENCIA;

        BEGIN

          FOR R_ASIENTOS IN P_T_ASIENTOS_DET LOOP

            IF R_ASIENTOS.TIPO_SALDO = 'D' THEN

              SELECT CC.ID_RECURSO_HABER
                INTO v_ID_RECUSRSO_HABER
                FROM EJECUCION.T_CUENTAS_CONTABLES CC
               WHERE CC.ID_CTA_CONTABLE = R_ASIENTOS.ID_CTA_CONTABLE;

              INSERT INTO EJECUCION.T_ASIENTOS_DET
                (ID_ASIENTO_DET,
                 ID_ASIENTO,
                 ID_CTA_CONTABLE,
                 FEC_CONTABLE,
                 HABER,
                 DEBE,
                 USER_ING,
                 FEC_ING,
                 USER_ACT,
                 FEC_ACT,
                 ID_RUR_DET,
                 ID_RECURSO_D_H,
                 ANULADO)
                SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                       p_ID_ASIENTO_CIERRE,
                       R_ASIENTOS.ID_CTA_CONTABLE,
                       SYSDATE, --R_ASIENTOS.FEC_CONTABLE,
                       R_ASIENTOS.SALDO,
                       NULL,
                       'SUAF',
                       SYSDATE,
                       NULL,
                       NULL,
                       NULL,
                       v_ID_RECUSRSO_HABER,
                       'N'
                  FROM DUAL;

            ELSE

              SELECT CC.ID_RECURSO_DEBE
                INTO v_ID_RECUSRSO_DEBE
                FROM EJECUCION.T_CUENTAS_CONTABLES CC
               WHERE CC.ID_CTA_CONTABLE = R_ASIENTOS.ID_CTA_CONTABLE;

              INSERT INTO EJECUCION.T_ASIENTOS_DET
                (ID_ASIENTO_DET,
                 ID_ASIENTO,
                 ID_CTA_CONTABLE,
                 FEC_CONTABLE,
                 HABER,
                 DEBE,
                 USER_ING,
                 FEC_ING,
                 USER_ACT,
                 FEC_ACT,
                 ID_RUR_DET,
                 ID_RECURSO_D_H,
                 ANULADO)
                SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                       p_ID_ASIENTO_CIERRE,
                       R_ASIENTOS.ID_CTA_CONTABLE,
                       SYSDATE, --R_ASIENTOS.FEC_CONTABLE,
                       NULL,
                       R_ASIENTOS.SALDO,
                       'SUAF',
                       SYSDATE,
                       NULL,
                       NULL,
                       NULL,
                       v_ID_RECUSRSO_DEBE, --R_ASIENTOS.ID_RECURSO_D_H,
                       'N'
                  FROM DUAL;

            END IF;

          END LOOP;

        END;

        IF v_SALDOHABER > v_SALDODEBE THEN

          INSERT INTO EJECUCION.T_ASIENTOS_DET
            (ID_ASIENTO_DET,
             ID_ASIENTO,
             ID_CTA_CONTABLE,
             FEC_CONTABLE,
             HABER,
             DEBE,
             USER_ING,
             FEC_ING,
             USER_ACT,
             FEC_ACT,
             ID_RUR_DET,
             ID_RECURSO_D_H,
             ANULADO)
            SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                   p_ID_ASIENTO_CIERRE,
                   11270,
                   SYSDATE,
                   ABS(DIFDebeHaber),
                   NULL,
                   'SUAF',
                   SYSDATE,
                   NULL,
                   NULL,
                   NULL,
                   3,
                   'N'
              FROM DUAL;
        ELSE

          INSERT INTO EJECUCION.T_ASIENTOS_DET
            (ID_ASIENTO_DET,
             ID_ASIENTO,
             ID_CTA_CONTABLE,
             FEC_CONTABLE,
             HABER,
             DEBE,
             USER_ING,
             FEC_ING,
             USER_ACT,
             FEC_ACT,
             ID_RUR_DET,
             ID_RECURSO_D_H,
             ANULADO)
            SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                   p_ID_ASIENTO_CIERRE,
                   11270,
                   SYSDATE,
                   NULL,
                   ABS(DIFDebeHaber),
                   'SUAF',
                   SYSDATE,
                   NULL,
                   NULL,
                   NULL,
                   4,
                   'N'
              FROM DUAL;

        END IF;

      END INSERTAR_ASIENTO_CIERRE;

      PROCEDURE INSERTAR_ASIENTO_APERTURA(p_ID_VIGENCIA IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL) IS
        DIFDebeHaber            NUMBER;
        v_SALDODEBE             NUMBER;
        v_SALDOHABER            NUMBER;
        NUEVO_NRO_ASIENTO       VARCHAR2(50);
        v_UNIDAD_CENTRAL        NUMBER;
        p_ID_ASIENTO_CIERRE     NUMBER;
        v_EJERCICIO             NUMBER;
        v_NRO_ASIENTO           NUMBER;
        v_EJERCICIO_ANIO_PASADO NUMBER;
        v_ID_VIGENCIA_PASADO    NUMBER;
        v_ID_RECUSRSO_HABER     NUMBER;
        v_ID_RECUSRSO_DEBE      NUMBER;
        v_ID_VIGENCIA           EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE;
      BEGIN
        v_ID_VIGENCIA := p_ID_VIGENCIA;

        SELECT EJECUCION.Sq_t_Asientos.NEXTVAL INTO p_ID_ASIENTO_CIERRE FROM DUAL;

        SELECT V.EJERCICIO
          INTO v_EJERCICIO
          FROM PRESUPUESTO.T_VIGENCIAS V
         WHERE V.ID_VIGENCIA = v_ID_VIGENCIA;

        SELECT V.EJERCICIO - 1
          INTO v_EJERCICIO_ANIO_PASADO
          FROM PRESUPUESTO.T_VIGENCIAS V
         WHERE V.ID_VIGENCIA = v_ID_VIGENCIA;

        SELECT V.ID_VIGENCIA
          INTO v_ID_VIGENCIA_PASADO
          FROM PRESUPUESTO.T_VIGENCIAS V
         WHERE V.EJERCICIO = v_EJERCICIO_ANIO_PASADO;

        \* SELECT nvl(max(a.numero )  ,0)  +1
        INTO NUEVO_NRO_ASIENTO
        FROM EJECUCION.T_ASIENTOS a
        WHERE a.id_vigencia= P_ID_VIGENCIA ;*\

        SELECT UA.ID_UNIDAD_ADM
          INTO v_UNIDAD_CENTRAL
          FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA
         WHERE UA.ID_VIGENCIA = v_ID_VIGENCIA
           AND UA.NOMBRE_UNIDAD_ADM = 'Administracion Central';

        SELECT SUM(DEBE)
          INTO v_SALDODEBE
          FROM T_ASIENTOS A, T_ASIENTOS_DET DET, T_CUENTAS_CONTABLES CC
         WHERE A.ID_ASIENTO = DET.ID_ASIENTO
           AND CC.ID_CTA_CONTABLE = DET.ID_CTA_CONTABLE
           AND CC.ID_NATURALEZA IN (1, 2)
           AND A.ID_TIPO_ASIENTO IN (6)
           AND A.ID_VIGENCIA = v_ID_VIGENCIA_PASADO;

        SELECT SUM(HABER)
          INTO v_SALDOHABER
          FROM T_ASIENTOS A, T_ASIENTOS_DET DET, T_CUENTAS_CONTABLES CC
         WHERE A.ID_ASIENTO = DET.ID_ASIENTO
           AND CC.ID_CTA_CONTABLE = DET.ID_CTA_CONTABLE
           AND CC.ID_NATURALEZA IN (1, 2)
           AND A.ID_TIPO_ASIENTO IN (6)
           AND A.ID_VIGENCIA = v_ID_VIGENCIA_PASADO;

        DIFDebeHaber := ABS(v_SALDODEBE) - ABS(v_SALDOHABER);

        INSERT INTO EJECUCION.T_ASIENTOS
          (ID_ASIENTO,
           NUMERO,
           ANIO,
           ID_VIGENCIA,
           ID_UNIDAD_ADM,
           OBSERVACIONES,
           FEC_ALTA,
           USER_ALTA,
           FEC_ACT,
           USER_ACT,
           ID_ASIENTO_REL,
           ID_TIPO_ASIENTO,
           FEC_CONTABLE,
           LEYENDA)
          SELECT p_ID_ASIENTO_CIERRE,
                 null, --EJECUCION.f_Numero_Asiento(v_EJERCICIO),--NUEVO_NRO_ASIENTO, se comenta porque no se coloca al generar nro asiento
                 v_EJERCICIO,
                 v_ID_VIGENCIA,
                 v_UNIDAD_CENTRAL,
                 'ASIENTO DE APERTURA',
                 SYSDATE,
                 'SUAF',
                 NULL,
                 NULL,
                 NULL,
                 6,
                 TO_DATE('01/01/' || v_EJERCICIO, 'DD/MM/YYYY'),
                 'ASIENTO DE APERTURA'
            FROM DUAL;

        DECLARE
          CURSOR P_T_ASIENTOS_DET IS

          \* SELECT CC.TIPO_SALDO, DET.ID_CTA_CONTABLE,SUM(DET.DEBE) SALDOD, DET.ID_RECURSO_D_H, DET.FEC_CONTABLE,SUM(DET.HABER) SALDOH
                                   FROM T_ASIENTOS A, T_ASIENTOS_DET DET, T_CUENTAS_CONTABLES CC
                                   WHERE A.ID_ASIENTO = DET.ID_ASIENTO
                                   AND   CC.ID_CTA_CONTABLE = DET.ID_CTA_CONTABLE
                                   AND   CC.ID_NATURALEZA IN (1,2)
                                   AND   A.ID_VIGENCIA = v_ID_VIGENCIA_PASADO
                                   GROUP BY CC.TIPO_SALDO, DET.ID_CTA_CONTABLE,DET.ID_RECURSO_D_H,DET.FEC_CONTABLE
                                   ;*\
            SELECT CC.TIPO_SALDO, DET.ID_CTA_CONTABLE, DET.DEBE, DET.HABER
              FROM T_ASIENTOS A, T_ASIENTOS_DET DET, T_CUENTAS_CONTABLES CC
             WHERE A.ID_ASIENTO = DET.ID_ASIENTO
               AND CC.ID_CTA_CONTABLE = DET.ID_CTA_CONTABLE
               AND CC.ID_NATURALEZA IN (1, 2)
               AND A.ID_TIPO_ASIENTO = 6
               AND A.ID_VIGENCIA = v_ID_VIGENCIA_PASADO
            --    GROUP BY CC.TIPO_SALDO, DET.ID_CTA_CONTABLE
            ;

        BEGIN

          FOR R_ASIENTOS IN P_T_ASIENTOS_DET LOOP

            IF R_ASIENTOS.DEBE IS NOT NULL THEN

              SELECT CC.ID_RECURSO_HABER
                INTO v_ID_RECUSRSO_HABER
                FROM EJECUCION.T_CUENTAS_CONTABLES CC
               WHERE CC.ID_CTA_CONTABLE = R_ASIENTOS.ID_CTA_CONTABLE;

              INSERT INTO EJECUCION.T_ASIENTOS_DET
                (ID_ASIENTO_DET,
                 ID_ASIENTO,
                 ID_CTA_CONTABLE,
                 FEC_CONTABLE,
                 DEBE,
                 HABER,
                 USER_ING,
                 FEC_ING,
                 USER_ACT,
                 FEC_ACT,
                 ID_RUR_DET,
                 ID_RECURSO_D_H,
                 ANULADO)
                SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                       p_ID_ASIENTO_CIERRE,
                       R_ASIENTOS.ID_CTA_CONTABLE,
                       SYSDATE,
                       NULL,
                       R_ASIENTOS.DEBE,
                       'SUAF',
                       SYSDATE,
                       NULL,
                       NULL,
                       NULL,
                       v_ID_RECUSRSO_HABER,
                       'N'
                  FROM DUAL;

            ELSE
              SELECT CC.ID_RECURSO_DEBE
                INTO v_ID_RECUSRSO_DEBE
                FROM EJECUCION.T_CUENTAS_CONTABLES CC
               WHERE CC.ID_CTA_CONTABLE = R_ASIENTOS.ID_CTA_CONTABLE;

              INSERT INTO EJECUCION.T_ASIENTOS_DET
                (ID_ASIENTO_DET,
                 ID_ASIENTO,
                 ID_CTA_CONTABLE,
                 FEC_CONTABLE,
                 DEBE,
                 HABER,
                 USER_ING,
                 FEC_ING,
                 USER_ACT,
                 FEC_ACT,
                 ID_RUR_DET,
                 ID_RECURSO_D_H,
                 ANULADO)
                SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                       p_ID_ASIENTO_CIERRE,
                       R_ASIENTOS.ID_CTA_CONTABLE,
                       SYSDATE,
                       R_ASIENTOS.HABER,
                       NULL,
                       'SUAF',
                       SYSDATE,
                       NULL,
                       NULL,
                       NULL,
                       v_ID_RECUSRSO_DEBE,
                       'N'
                  FROM DUAL;

            END IF;

          END LOOP;

        END;

        IF v_SALDOHABER > v_SALDODEBE THEN

          INSERT INTO EJECUCION.T_ASIENTOS_DET
            (ID_ASIENTO_DET,
             ID_ASIENTO,
             ID_CTA_CONTABLE,
             FEC_CONTABLE,
             DEBE,
             HABER,
             USER_ING,
             FEC_ING,
             USER_ACT,
             FEC_ACT,
             ID_RUR_DET,
             ID_RECURSO_D_H,
             ANULADO)
            SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                   p_ID_ASIENTO_CIERRE,
                   1038,
                   SYSDATE,
                   NULL,
                   ABS(DIFDebeHaber),
                   'SUAF',
                   SYSDATE,
                   NULL,
                   NULL,
                   NULL,
                   3,
                   'N'
              FROM DUAL;
        ELSE

          INSERT INTO EJECUCION.T_ASIENTOS_DET
            (ID_ASIENTO_DET,
             ID_ASIENTO,
             ID_CTA_CONTABLE,
             FEC_CONTABLE,
             DEBE,
             HABER,
             USER_ING,
             FEC_ING,
             USER_ACT,
             FEC_ACT,
             ID_RUR_DET,
             ID_RECURSO_D_H,
             ANULADO)
            SELECT EJECUCION.Sq_t_Asientos_DET.NEXTVAL,
                   p_ID_ASIENTO_CIERRE,
                   1038,
                   SYSDATE,
                   ABS(DIFDebeHaber),
                   NULL,
                   'SUAF',
                   SYSDATE,
                   NULL,
                   NULL,
                   NULL,
                   4,
                   'N'
              FROM DUAL;

        END IF;

      END INSERTAR_ASIENTO_APERTURA;
    */
    PROCEDURE BUSCAR_PARA_GRILLA(p_ASIENTOS              OUT TY_CURSOR,
                                 p_ID_VIGENCIA           IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE,
                                 p_ID_UNIDAD_ADM         IN EJECUCION.T_ANTICIPOS.ID_UNIDAD_ADM%TYPE,
                                 p_FECHA_DESDE           IN EJECUCION.T_ASIENTOS.FEC_CONTABLE%TYPE,
                                 p_FECHA_HASTA           IN EJECUCION.T_ASIENTOS.FEC_CONTABLE%TYPE,
                                 p_FILTRO_COL            IN VARCHAR2,
                                 p_FILTRO_VAL            IN VARCHAR2,
                                 p_ID_TIPO_EP            IN CAJA.T_ELEMENTOS_PAGO.ID_TIPO_EP%TYPE,
                                 p_PAGINA                IN NUMBER DEFAULT 1,
                                 p_TAMANO_PAGINA         IN NUMBER DEFAULT 10,
                                 p_CANTIDAD_DE_REGISTROS OUT NUMBER) IS
        v_CABECERA_CONSULTA VARCHAR2(2500);
        v_CUERPO_CONSULTA   VARCHAR2(5000);
        v_EJERCICIO         PRESUPUESTO.T_VIGENCIAS.EJERCICIO%TYPE;
        v_INICIO            NUMBER;
        v_FIN               NUMBER;

    BEGIN
        v_CABECERA_CONSULTA := 'SELECT COUNT(*)
                          FROM EJECUCION.T_ASIENTOS A ';

        IF p_FILTRO_VAL IS NOT NULL THEN
            CASE p_FILTRO_COL
                WHEN 'NRO_DEVENGADO' THEN
                    v_CUERPO_CONSULTA := ' JOIN EJECUCION.T_DEVENGADO D ON A.ID_DEVENGADO = D.ID_DEVENGADO
                          WHERE D.NRO_DEVENGADO LIKE ''%' ||
                                         p_FILTRO_VAL || '%''';

                WHEN 'NRO_EP' THEN
                    v_CUERPO_CONSULTA := ' LEFT JOIN EJECUCION.T_ASIENTOS_DET AD ON A.ID_ASIENTO = AD.ID_ASIENTO
                               LEFT JOIN CAJA.T_ELEMENTOS_PAGO EP ON AD.ID_ELEMENTO_PAGO = EP.ID_ELEMENTO_PAGO
                          WHERE EP.ID_TIPO_EP = ' ||
                                         p_ID_TIPO_EP ||
                                         ' AND
                                EP.NUMERO_EP LIKE ''%' ||
                                         p_FILTRO_VAL || '%''';

                WHEN 'NRO_DUEE' THEN
                    v_CUERPO_CONSULTA := ' LEFT JOIN EJECUCION.T_DIETDAC D ON A.ID_DIETDAC = D.ID_DIETDAC
                          WHERE D.NRO_DUEE || ''/'' || D.NRO_INTERVENCION LIKE ''' ||
                                         p_FILTRO_VAL || '''
                             OR D.NRO_DOCUMENTO LIKE ''' ||
                                         p_FILTRO_VAL || '''';

                WHEN 'NRO_LIBRO_BANCO' THEN
                    v_CUERPO_CONSULTA := ' LEFT JOIN EJECUCION.T_LIBRO_BANCO LB ON A.ID_LIBRO_BANCO = LB.ID_LIBRO_BANCO
                          WHERE LB.NRO_DOC = ' ||
                                         p_FILTRO_VAL;

                WHEN 'NRO_RUR' THEN
                    v_CUERPO_CONSULTA := ' LEFT JOIN EJECUCION.T_RUR R ON A.ID_RUR = R.ID_RUR
                          WHERE R.NRO_DOC = ' ||
                                         p_FILTRO_VAL;

                WHEN 'NRO_ANTICIPO' THEN
                    v_CUERPO_CONSULTA := ' JOIN EJECUCION.T_ANTICIPOS AT ON A.ID_ANTICIPO = AT.ID_ANTICIPO
                          WHERE AT.NTO_ANTICIPO LIKE ''' ||
                                         p_FILTRO_VAL || '''';

                WHEN 'NRO_ASIENTO' THEN
                    /*\*
                    * se deja de generar el numero de asiento automaticamente y se empiezan a identificar a traves
                    * del id_asiento. Si el asiento es posterior a este cambio se busca por numero, sino por id
                     *\*/
                    SELECT V.EJERCICIO
                      INTO v_EJERCICIO
                      FROM PRESUPUESTO.T_VIGENCIAS V
                     WHERE V.ID_VIGENCIA = p_ID_VIGENCIA;

/*                    IF v_EJERCICIO > 2012 THEN
                        v_CUERPO_CONSULTA := ' WHERE A.ID_ASIENTO = ' ||
                                             p_FILTRO_VAL;
                    ELSE*/
                        v_CUERPO_CONSULTA := ' WHERE A.NUMERO = ' ||
                                             p_FILTRO_VAL;
         --           END IF;

            END CASE;
            v_CUERPO_CONSULTA := v_CUERPO_CONSULTA ||
                                 ' AND A.ID_VIGENCIA = ' || p_ID_VIGENCIA;

        ELSE
            v_CUERPO_CONSULTA := ' WHERE A.ID_VIGENCIA = ' || p_ID_VIGENCIA;

        END IF;

        IF p_ID_UNIDAD_ADM IS NOT NULL THEN
            v_CUERPO_CONSULTA := v_CUERPO_CONSULTA ||
                                 ' AND A.ID_JURISDICCION = (
      SELECT UA1.ID_JURISDICCION FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA1
      WHERE UA1.ID_UNIDAD_ADM = ' ||
                                 p_ID_UNIDAD_ADM || ' ) ';
        END IF;

        IF p_FECHA_DESDE IS NOT NULL THEN
            v_CUERPO_CONSULTA := v_CUERPO_CONSULTA ||
                                 ' AND A.FEC_CONTABLE >= ''' ||
                                 p_FECHA_DESDE || '''';
        END IF;

        IF p_FECHA_HASTA IS NOT NULL THEN
            v_CUERPO_CONSULTA := v_CUERPO_CONSULTA ||
                                 ' AND A.FEC_CONTABLE <= ''' ||
                                 p_FECHA_HASTA || '''';
        END IF;

        EXECUTE IMMEDIATE v_CABECERA_CONSULTA || v_CUERPO_CONSULTA
            INTO p_CANTIDAD_DE_REGISTROS;

        IF p_PAGINA IS NULL THEN
            v_INICIO := 0;
            v_FIN    := p_CANTIDAD_DE_REGISTROS;

        ELSE
            IF p_PAGINA = 1 THEN
                v_INICIO := 0;
                v_FIN    := v_INICIO + p_TAMANO_PAGINA;

            ELSE
                v_INICIO := (p_PAGINA - 1) * p_TAMANO_PAGINA;
                v_FIN    := v_INICIO + p_TAMANO_PAGINA;

            END IF;
        END IF;

        v_CABECERA_CONSULTA := 'SELECT *
                          FROM (SELECT V.*, ROWNUM NUMERO_DE_REGISTRO
                                FROM (SELECT A.ID_ASIENTO,
                                             A.NUMERO,
                                             A.ANIO,
                                             A.ID_VIGENCIA,
                                             A.ID_JURISDICCION,
                                             (SELECT UA1.ID_UNIDAD_ADM
                                              FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA UA1
                                              WHERE UA1.ID_JURISDICCION = A.ID_JURISDICCION) ID_UNIDAD_ADM,
                                             A.OBSERVACIONES,
                                             A.FEC_ALTA,
                                             A.USER_ALTA,
                                             A.FEC_ACT,
                                             A.USER_ACT,
                                             A.ID_ASIENTO_REL,
                                             A.ID_RUR,
                                             A.ID_DEVENGADO,
                                             A.ID_DIETDAC,
                                             A.ID_ANTICIPO,
                                             A.ID_TIPO_ASIENTO,
                                             A.FEC_CONTABLE,
                                             A.LEYENDA,
                                             A.ID_TRANSFERENCIA,
                                             A.ID_MOV_TESO,
                                             A.ID_LIBRO_BANCO,
                                             A.ID_BIEN,
                                             '''' VISIBLE,
                                             V.EJERCICIO
                                      FROM EJECUCION.T_ASIENTOS A JOIN PRESUPUESTO.T_VIGENCIAS V ON A.ID_VIGENCIA = V.ID_VIGENCIA ';
        v_CUERPO_CONSULTA   := v_CUERPO_CONSULTA ||
                               'ORDER BY ANIO DESC, NUMERO DESC
                                     )V
                               )
                          WHERE NUMERO_DE_REGISTRO >= ' ||
                               v_INICIO || ' AND NUMERO_DE_REGISTRO <= ' ||
                               v_FIN;
        -- insert into pruebas values (v_CABECERA_CONSULTA || v_CUERPO_CONSULTA,sysdate,'ASIENTO_BUSCAR');
        COMMIT;
        OPEN p_ASIENTOS FOR v_CABECERA_CONSULTA || v_CUERPO_CONSULTA;

    END BUSCAR_PARA_GRILLA;



  PROCEDURE INSERTAR_REVERSA(p_ID_VIGENCIA     IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL,
                             p_ID_UNIDAD_ADM   IN PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA.ID_UNIDAD_ADM%TYPE DEFAULT NULL,
                             p_NUMERO          IN EJECUCION.T_ASIENTOS.NUMERO%TYPE DEFAULT NULL,
                             p_FEC_CONTABLE    IN EJECUCION.T_ASIENTOS.FEC_CONTABLE%TYPE DEFAULT NULL,
                             p_ID_TIPO_ASIENTO IN EJECUCION.T_ASIENTOS.ID_TIPO_ASIENTO%TYPE DEFAULT NULL,
                             p_LEYENDA         IN EJECUCION.T_ASIENTOS.LEYENDA%TYPE DEFAULT NULL,
                             p_OBSERVACIONES   IN EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE DEFAULT NULL,
                             p_USER_ING        IN EJECUCION.T_ASIENTOS.USER_ALTA%TYPE DEFAULT NULL,
                             p_FEC_ING         IN EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE DEFAULT NULL,
                             p_USER_ACT        IN EJECUCION.T_ASIENTOS.USER_ACT%TYPE DEFAULT NULL,
                             p_FEC_ACT         IN EJECUCION.T_ASIENTOS.FEC_ACT%TYPE DEFAULT NULL,
                             p_ID_ASIENTO_REL  IN EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE,
                             p_MSG             IN OUT VARCHAR2) IS
    p_ID_ASIENTO_REVERSA     NUMBER;
    NUEVO_NRO_ASIENTO        VARCHAR2(10);
    v_EJERCICIO              NUMBER;
        v_id_vigencia              NUMBER(10);
    v_NUMERO                 NUMBER;
    v_FECHA_CONTABLE         DATE;
    v_NUEVA_FECHA            DATE;
    v_NUMERO_REVERSA         NUMBER;
    v_es_administracion_gral number(10);
    V_ID_JURISDICCION        NUMBER(10);
  BEGIN

    SELECT EJECUCION.Sq_t_Asientos.NEXTVAL
      INTO p_ID_ASIENTO_REVERSA
      FROM DUAL;

    SELECT V.EJERCICIO
      INTO v_EJERCICIO
      FROM PRESUPUESTO.T_VIGENCIAS V
     WHERE V.ID_VIGENCIA = p_ID_VIGENCIA;



    /*select nvl(max(a.numero )  ,0)  +1
    into NUEVO_NRO_ASIENTO
    from EJECUCION.T_ASIENTOS a
    where a.id_vigencia= P_ID_VIGENCIA ;*/

    SELECT A.NUMERO, A.FEC_CONTABLE
      INTO v_NUMERO, v_FECHA_CONTABLE
      FROM EJECUCION.T_ASIENTOS A
     WHERE A.ID_ASIENTO = p_ID_ASIENTO_REL;

    IF (to_number(to_char(v_FECHA_CONTABLE, 'yyyy')) <
       to_number(to_char(SYSDATE, 'yyyy'))) THEN
      v_NUEVA_FECHA := TO_DATE('31/12/' || v_EJERCICIO, 'DD/MM/YYYY');
    ELSE
      v_NUEVA_FECHA := SYSDATE;

    END IF;

   SELECT ua.ID_JURISDICCION, ua.id_vigencia
      INTO V_ID_JURISDICCION,v_id_vigencia
     FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA ua
    WHERE ua.ID_JURISDICCION =
          (SELECT ua1.ID_JURISDICCION
             FROM PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA ua1
             join PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA ua2
               on ua1.numero_unidad_adm = ua2.numero_unidad_adm
             join presupuesto.t_vigencias v1
               on v1.id_vigencia = ua1.id_vigencia
            where v1.ejercicio = to_char(v_nueva_fecha, 'yyyy')
              and ua2.id_unidad_adm = p_id_unidad_adm

           );

    /*  select count(ua.id_unidad_adm) into v_es_administracion_gral
     from PRESUPUESTO.t_unidades_administrativa ua
    where ua.id_unidad_adm = p_ID_UNIDAD_ADM
      and ua.id_jurisdiccion in
          (select j.id_jurisdiccion from PRESUPUESTO.t_jurisdicciones j where j.id_grupo_sector = (select gs.id_grupo_sector
                                                                      from PRESUPUESTO.t_grupo_sectores gs
                                                                     where gs.id_vigencia = p_ID_VIGENCIA
                                                                       and gs.numero_grupo_sector =
                                                                           '1010101'));*/

    /*    if v_es_administracion_gral <> 0 then
    v_NUMERO_REVERSA := null; --EJECUCION.f_Numero_Asiento(v_EJERCICIO); se comenta porque no se coloca al generar nro asiento*/
    select EJECUCION.f_Numero_Asiento(V_ID_JURISDICCION)
      into v_NUMERO_REVERSA
      from dual;

    INSERT INTO EJECUCION.T_ASIENTOS
      (ID_ASIENTO,
       NUMERO,
       ANIO,
       ID_VIGENCIA,
       ID_JURISDICCION,
       OBSERVACIONES,
       FEC_ALTA,
       USER_ALTA,
       FEC_ACT,
       USER_ACT,
       ID_ASIENTO_REL,
       ID_TIPO_ASIENTO,
       FEC_CONTABLE,
       LEYENDA)
    VALUES
      (p_ID_ASIENTO_REVERSA,
       v_NUMERO_REVERSA,
       v_EJERCICIO,
       v_ID_VIGENCIA,
       V_ID_JURISDICCION,
       p_OBSERVACIONES,
       sysdate,
       user,
       null,
       null,
       p_ID_ASIENTO_REL,
       p_ID_TIPO_ASIENTO,
       v_NUEVA_FECHA,
       'REVERSA ASIENTO ' || to_char(v_NUMERO)

       );

    INSERT INTO EJECUCION.T_ASIENTOS_DET
      (ID_ASIENTO_DET,
       ID_ASIENTO,
       ID_CTA_CONTABLE,
       FEC_CONTABLE,
       HABER,
       DEBE,
       USER_ING,
       FEC_ING,
       USER_ACT,
       FEC_ACT,
       ID_RUR_DET,
       ANULADO,
       ID_RECURSO_D_H)
      SELECT EJECUCION.Sq_t_Asientos_Detalles.NEXTVAL,
             p_ID_ASIENTO_REVERSA,
             EJECUCION.T_ASIENTOS_DET.ID_CTA_CONTABLE,
             v_NUEVA_FECHA,
             EJECUCION.T_ASIENTOS_DET.DEBE,
             EJECUCION.T_ASIENTOS_DET.HABER,
             EJECUCION.T_ASIENTOS_DET.USER_ING,
             EJECUCION.T_ASIENTOS_DET.FEC_ING,
             EJECUCION.T_ASIENTOS_DET.USER_ACT,
             EJECUCION.T_ASIENTOS_DET.FEC_ACT,
             EJECUCION.T_ASIENTOS_DET.ID_RUR_DET,
             'S',
             EJECUCION.T_ASIENTOS_DET.ID_RECURSO_D_H
        FROM EJECUCION.T_ASIENTOS_DET
       WHERE (EJECUCION.T_ASIENTOS_DET.ID_ASIENTO = p_ID_ASIENTO_REL);
           update EJECUCION.T_ASIENTOS_DET
       set anulado = 'S'
     where id_asiento = p_ID_ASIENTO_REL;

/*  end if;
*/
END INSERTAR_REVERSA;




  PROCEDURE ELIMINAR(p_ID_ASIENTO IN EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE DEFAULT NULL,
                     p_MSG        IN OUT varchar2) IS

    v_ID_ASIENTO     EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE;
    V_numero_asiento EJECUCION.T_ASIENTOS.NUMERO%TYPE;

  BEGIN

    SELECT ejecucion.f_Numero_Asiento(ID_JURISDICCION)
      INTO V_numero_asiento
      FROM T_ASIENTOS A
     WHERE A.ID_ASIENTO = P_ID_ASIENTO;

    select EJECUCION.Sq_t_Asientos.nextval INTO V_id_asiento from dual;

    INSERT INTO EJECUCION.T_ASIENTOS

      (id_asiento,
       numero,
       anio,
       id_vigencia,
       id_jurisdiccion,
       observaciones,
       user_alta,
       fec_alta,
       user_act,
       fec_act,
       id_asiento_rel,
       id_tipo_asiento,
       fec_contable,
       leyenda)
      (select V_id_asiento,
              V_numero_asiento,
              ANIO,
              id_vigencia,
              id_jurisdiccion,
              'CONTRAASIENTO' || observaciones,
              USER,
              SYSDATE,
              NULL,
              NULL,
              p_id_asiento,
              id_tipo_asiento,
              fec_contable,
              'CONTRAASIENTO' || leyenda
         from ejecucion.t_asientos
        where id_asiento = p_id_asiento);

    INSERT INTO EJECUCION.T_ASIENTOS_DET
      (id_asiento_det,
       id_asiento,
       id_cta_contable,
       fec_contable,
       debe,
       haber,
       user_ing,
       fec_ing,
       anulado)
      (SELECT EJECUCION.Sq_t_Asientos_Detalles.nextval,
              V_id_asiento,
              id_cta_contable,
              fec_contable,
              haber,
              debe,
              user,
              sysdate,
              'S'
         from ejecucion.t_asientos_det ad
        where ad.id_asiento = p_ID_ASIENTO);

    update EJECUCION.T_ASIENTOS_DET
       set anulado = 'S'
     where id_asiento = p_id_asiento;

  exception
    when v_error_2292 then
      p_MSG := 'No se puede Eliminar el registro existen relaciones para ese ejercicio asociadas a otras tablas.';
      pkg_util.base_rollback;
    when others then
      p_MSG := 'Error al ELIMINAR. Verifique: ' || sqlerrm;
      pkg_util.base_rollback;

  END ELIMINAR;


END PKG_T_ASIENTOS;
PACKAGE PKG_T_ASIENTOS AS
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

    v_error_control_fecha exception;

    TYPE ty_CURSOR IS REF CURSOR;

    PROCEDURE INSERTAR(p_ID_ASIENTO      OUT EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE,
                       p_NUMERO          IN EJECUCION.T_ASIENTOS.NUMERO%TYPE DEFAULT NULL,
                       p_ANIO            IN EJECUCION.T_ASIENTOS.ANIO%TYPE DEFAULT NULL,
                       p_ID_VIGENCIA     IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL,
                       p_ID_UNIDAD_ADM   IN EJECUCION.T_ANTICIPOS.ID_UNIDAD_ADM%TYPE DEFAULT NULL,
                       p_OBSERVACIONES   IN EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE DEFAULT NULL,
                       p_FEC_ALTA        IN EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE DEFAULT NULL,
                       p_USER_ALTA       IN EJECUCION.T_ASIENTOS.USER_ALTA%TYPE DEFAULT NULL,
                       p_FEC_ACT         IN EJECUCION.T_ASIENTOS.FEC_ACT%TYPE DEFAULT NULL,
                       p_USER_ACT        IN EJECUCION.T_ASIENTOS.USER_ACT%TYPE DEFAULT NULL,
                       p_ID_ASIENTO_REL  IN EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE DEFAULT NULL,
                       p_ID_RUR          IN EJECUCION.T_ASIENTOS.ID_RUR%TYPE DEFAULT NULL,
                       p_leyenda         IN EJECUCION.T_ASIENTOS.LEYENDA%TYPE DEFAULT NULL,
                       p_fec_contable    IN EJECUCION.T_ASIENTOS.FEC_CONTABLE%TYPE DEFAULT NULL,
                       p_id_tipo_asiento IN EJECUCION.T_ASIENTOS.ID_TIPO_ASIENTO%TYPE DEFAULT NULL,
                       p_MSG             IN OUT VARCHAR2);
    /*
      PROCEDURE EDITAR(p_ID_ASIENTO     IN EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE DEFAULT NULL,
                       p_NUMERO         IN EJECUCION.T_ASIENTOS.NUMERO%TYPE DEFAULT NULL,
                       p_ANIO           IN EJECUCION.T_ASIENTOS.ANIO%TYPE DEFAULT NULL,
                       p_ID_VIGENCIA    IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL,
                       p_ID_UNIDAD_ADM  IN EJECUCION.T_ASIENTOS.ID_UNIDAD_ADM%TYPE DEFAULT NULL,
                       p_OBSERVACIONES  IN EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE DEFAULT NULL,
                       p_FEC_ALTA       IN EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE DEFAULT NULL,
                       p_USER_ALTA      IN EJECUCION.T_ASIENTOS.USER_ALTA%TYPE DEFAULT NULL,
                       p_FEC_ACT        IN EJECUCION.T_ASIENTOS.FEC_ACT%TYPE DEFAULT NULL,
                       p_USER_ACT       IN EJECUCION.T_ASIENTOS.USER_ACT%TYPE DEFAULT NULL,
                       p_ID_ASIENTO_REL IN EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE DEFAULT NULL,
                       p_ID_RUR         IN EJECUCION.T_ASIENTOS.ID_RUR%TYPE DEFAULT NULL,
                       p_MSG            IN OUT varchar2);
      PROCEDURE ELIMINAR(p_ID_ASIENTO IN EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE DEFAULT NULL,
                         p_MSG        IN OUT varchar2);
    */
    PROCEDURE BUSCARPORPK(p_T_ASIENTOS OUT ty_CURSOR,
                          p_ID_ASIENTO IN EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE DEFAULT NULL);


      PROCEDURE BUSCAR_ASIENTO_INGR_BI(p_T_ASIENTOS  OUT ty_CURSOR,
                                       p_ID_ANTICIPO IN EJECUCION.T_ASIENTOS.ID_ANTICIPO%TYPE DEFAULT NULL,
                                       p_ID_EP       IN EJECUCION.T_ASIENTOS_DET.ID_ELEMENTO_PAGO%TYPE DEFAULT NULL,
                                       p_TIPO_DOC    IN varchar2);
      /***********************************************************/
      /***********************************************************/
      /*12521 - Se agrego para poder anular los ingresos de la BI*/
      /***********************************************************/
      PROCEDURE INSERTAR_CONTRAASIENTO_INGR_BI(p_ID_ASIENTO      OUT EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE,
                                               p_ID_ASIENTO_ORIG IN EJECUCION.T_ASIENTOS.ID_ASIENTO %TYPE DEFAULT NULL,
                                               p_ID_LIBRO_BANCO  IN EJECUCION.T_LIBRO_BANCO.ID_LIBRO_BANCO %TYPE DEFAULT NULL,
                                               p_TIPO_DOC        IN varchar2,
                                               p_MSG             IN OUT VARCHAR);
      /***********************************************************/

    PROCEDURE BUSCAR(p_T_ASIENTOS          OUT ty_CURSOR,
                     p_ID_ASIENTO          IN EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE DEFAULT NULL,
                     p_NUMERO              IN EJECUCION.T_ASIENTOS.NUMERO%TYPE DEFAULT NULL,
                     p_ANIO                IN EJECUCION.T_ASIENTOS.ANIO%TYPE DEFAULT NULL,
                     p_ID_VIGENCIA         IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL,
                     p_ID_UNIDAD_ADM       IN EJECUCION.T_ANTICIPOS.ID_UNIDAD_ADM%TYPE DEFAULT NULL,
                     p_OBSERVACIONES       IN EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE DEFAULT NULL,
                     p_FEC_ALTA            IN EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE DEFAULT NULL,
                     p_USER_ALTA           IN EJECUCION.T_ASIENTOS.USER_ALTA%TYPE DEFAULT NULL,
                     p_FEC_ACT             IN EJECUCION.T_ASIENTOS.FEC_ACT%TYPE DEFAULT NULL,
                     p_USER_ACT            IN EJECUCION.T_ASIENTOS.USER_ACT%TYPE DEFAULT NULL,
                     p_ID_ASIENTO_REL      IN EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE DEFAULT NULL,
                     p_ID_RUR              IN EJECUCION.T_ASIENTOS.ID_RUR%TYPE DEFAULT NULL,
                     p_PAGINA              IN NUMBER DEFAULT NULL,
                     p_TAMANOPAGINA        IN NUMBER DEFAULT NULL,
                     p_CANTIDADDEREGISTROS OUT NUMBER);

    PROCEDURE CONSULTAR_UA(p_T_ASIENTOS  OUT ty_CURSOR,
                           p_ID_VIGENCIA IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL);
    /*
    PROCEDURE BUSCARSIGNUM(P_ID_VIGENCIA IN PRESUPUESTO.T_VIGENCIAS.ID_VIGENCIA%TYPE DEFAULT NULL,
                             sig_nume      OUT VARCHAR2,
                             P_MSG         OUT VARCHAR2);
    */
    PROCEDURE TRAER_RECURSO_HABER(p_T_ASIENTOS OUT ty_CURSOR,
                                  p_ID_CC      IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                                  p_TIPO_SALDO IN EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE DEFAULT NULL);

    PROCEDURE TRAER_RECURSO_DEBE(p_T_ASIENTOS OUT ty_CURSOR,
                                 p_ID_CC      IN EJECUCION.T_CUENTAS_CONTABLES.ID_CTA_CONTABLE%TYPE DEFAULT NULL,
                                 p_TIPO_SALDO IN EJECUCION.T_CUENTAS_CONTABLES.TIPO_SALDO%TYPE DEFAULT NULL);
    /*
      PROCEDURE INSERTAR_REVERSA(p_ID_VIGENCIA     IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL,
                                 p_ID_UNIDAD_ADM   IN EJECUCION.T_ASIENTOS.ID_UNIDAD_ADM%TYPE DEFAULT NULL,
                                 p_NUMERO          IN EJECUCION.T_ASIENTOS.NUMERO%TYPE DEFAULT NULL,
                                 p_FEC_CONTABLE    IN EJECUCION.T_ASIENTOS.FEC_CONTABLE%TYPE DEFAULT NULL,
                                 p_ID_TIPO_ASIENTO IN EJECUCION.T_ASIENTOS.ID_TIPO_ASIENTO%TYPE DEFAULT NULL,
                                 p_LEYENDA         IN EJECUCION.T_ASIENTOS.LEYENDA%TYPE DEFAULT NULL,
                                 p_OBSERVACIONES   IN EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE DEFAULT NULL,
                                 p_USER_ING        IN EJECUCION.T_ASIENTOS.USER_ALTA%TYPE DEFAULT NULL,
                                 p_FEC_ING         IN EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE DEFAULT NULL,
                                 p_USER_ACT        IN EJECUCION.T_ASIENTOS.USER_ACT%TYPE DEFAULT NULL,
                                 p_FEC_ACT         IN EJECUCION.T_ASIENTOS.FEC_ACT%TYPE DEFAULT NULL,
                                 p_ID_ASIENTO_REL  IN EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE,
                                 p_MSG             IN OUT VARCHAR2);

      PROCEDURE INSERTAR_ASIENTO_CIERRE(p_ID_VIGENCIA IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL);

      PROCEDURE INSERTAR_ASIENTO_APERTURA(p_ID_VIGENCIA IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL);
    */
    PROCEDURE BUSCAR_PARA_GRILLA(p_ASIENTOS              OUT TY_CURSOR,
                                 p_ID_VIGENCIA           IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE,
                                 p_ID_UNIDAD_ADM         IN EJECUCION.T_ANTICIPOS.ID_UNIDAD_ADM%TYPE,
                                 p_FECHA_DESDE           IN EJECUCION.T_ASIENTOS.FEC_CONTABLE%TYPE,
                                 p_FECHA_HASTA           IN EJECUCION.T_ASIENTOS.FEC_CONTABLE%TYPE,
                                 p_FILTRO_COL            IN VARCHAR2,
                                 p_FILTRO_VAL            IN VARCHAR2,
                                 p_ID_TIPO_EP            IN CAJA.T_ELEMENTOS_PAGO.ID_TIPO_EP%TYPE,
                                 p_PAGINA                IN NUMBER DEFAULT 1,
                                 p_TAMANO_PAGINA         IN NUMBER DEFAULT 10,
                                 p_CANTIDAD_DE_REGISTROS OUT NUMBER);


  PROCEDURE ELIMINAR(p_ID_ASIENTO IN EJECUCION.T_ASIENTOS.ID_ASIENTO%TYPE DEFAULT NULL,
                     p_MSG        IN OUT varchar2) ;

  PROCEDURE INSERTAR_REVERSA(p_ID_VIGENCIA     IN EJECUCION.T_ASIENTOS.ID_VIGENCIA%TYPE DEFAULT NULL,
                             p_ID_UNIDAD_ADM   IN PRESUPUESTO.T_UNIDADES_ADMINISTRATIVA.ID_UNIDAD_ADM%TYPE DEFAULT NULL,
                             p_NUMERO          IN EJECUCION.T_ASIENTOS.NUMERO%TYPE DEFAULT NULL,
                             p_FEC_CONTABLE    IN EJECUCION.T_ASIENTOS.FEC_CONTABLE%TYPE DEFAULT NULL,
                             p_ID_TIPO_ASIENTO IN EJECUCION.T_ASIENTOS.ID_TIPO_ASIENTO%TYPE DEFAULT NULL,
                             p_LEYENDA         IN EJECUCION.T_ASIENTOS.LEYENDA%TYPE DEFAULT NULL,
                             p_OBSERVACIONES   IN EJECUCION.T_ASIENTOS.OBSERVACIONES%TYPE DEFAULT NULL,
                             p_USER_ING        IN EJECUCION.T_ASIENTOS.USER_ALTA%TYPE DEFAULT NULL,
                             p_FEC_ING         IN EJECUCION.T_ASIENTOS.FEC_ALTA%TYPE DEFAULT NULL,
                             p_USER_ACT        IN EJECUCION.T_ASIENTOS.USER_ACT%TYPE DEFAULT NULL,
                             p_FEC_ACT         IN EJECUCION.T_ASIENTOS.FEC_ACT%TYPE DEFAULT NULL,
                             p_ID_ASIENTO_REL  IN EJECUCION.T_ASIENTOS.ID_ASIENTO_REL%TYPE,
                             p_MSG             IN OUT VARCHAR2);

END PKG_T_ASIENTOS;
