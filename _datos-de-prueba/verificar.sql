-- =====================================================================================
-- Datos de prueba del modulo contable — verificacion
--
-- Solo lee. Muestra que hay en la base, separando lo real de lo sintetico, y controla
-- que todos los asientos cuadren.
-- =====================================================================================

set pagesize 200 linesize 140 feedback off
col ORIGEN format a16
col LEYENDA format a44
col NOMBRE format a34

prompt === asientos: reales contra datos de prueba ===
select case when leyenda like '[QA]%' then 'prueba' else 'real' end origen,
       count(*) asientos, min(fec_contable) desde, max(fec_contable) hasta
from EJECUCION.T_ASIENTOS group by case when leyenda like '[QA]%' then 'prueba' else 'real' end;

prompt === distribucion por ejercicio y mes ===
select anio, to_char(fec_contable,'MM') mes, count(*) asientos, sum(1) total
from EJECUCION.T_ASIENTOS where leyenda like '[QA]%'
group by anio, to_char(fec_contable,'MM') order by anio, mes;

prompt === distribucion por tipo de asiento ===
select a.id_tipo_asiento, t.descripcion, count(*) asientos
from EJECUCION.T_ASIENTOS a join EJECUCION.T_TIPO_ASIENTO t on t.id_tipo_asiento=a.id_tipo_asiento
where a.leyenda like '[QA]%' group by a.id_tipo_asiento, t.descripcion order by 1;

prompt === renglones por asiento ===
select renglones, count(*) cantidad_de_asientos from (
  select d.id_asiento, count(*) renglones
    from EJECUCION.T_ASIENTOS_DET d join EJECUCION.T_ASIENTOS a on a.id_asiento=d.id_asiento
   where a.leyenda like '[QA]%' group by d.id_asiento)
group by renglones order by renglones;

prompt === CONTROL: asientos descuadrados (tiene que dar 0) ===
select count(*) descuadrados from (
  select d.id_asiento, sum(nvl(d.debe,0)) - sum(nvl(d.haber,0)) dif
    from EJECUCION.T_ASIENTOS_DET d group by d.id_asiento having sum(nvl(d.debe,0)) <> sum(nvl(d.haber,0)));

prompt === CONTROL: numeros repetidos dentro de una jurisdiccion (tiene que dar 0) ===
select count(*) numeros_repetidos from (
  select id_jurisdiccion, numero from EJECUCION.T_ASIENTOS
   group by id_jurisdiccion, numero having count(*) > 1);

prompt === CONTROL: renglones que apuntan a cuentas no imputables (tiene que dar 0) ===
select count(*) sobre_no_imputables
from EJECUCION.T_ASIENTOS_DET d
join EJECUCION.T_ASIENTOS a on a.id_asiento = d.id_asiento
join EJECUCION.T_CUENTAS_CONTABLES c on c.id_cta_contable = d.id_cta_contable
where a.leyenda like '[QA]%' and c.imputable <> 'S';

prompt === las 10 cuentas mas movidas ===
select * from (
  select c.codigo, substr(c.nombre,1,32) nombre, count(*) movimientos,
         sum(nvl(d.debe,0)) debe, sum(nvl(d.haber,0)) haber
    from EJECUCION.T_ASIENTOS_DET d
    join EJECUCION.T_ASIENTOS a on a.id_asiento=d.id_asiento
    join EJECUCION.T_CUENTAS_CONTABLES c on c.id_cta_contable=d.id_cta_contable
   where a.leyenda like '[QA]%'
   group by c.codigo, substr(c.nombre,1,32) order by count(*) desc)
where rownum <= 10;

exit
