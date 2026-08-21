/**
 * Compara el balance de sumas y saldos del PL/SQL original contra el del modulo nuevo.
 *
 *   node comparar.js plsql_sumas_y_saldos.csv modulo_sumas_y_saldos.json
 *
 * No busca que coincidan: busca que las diferencias sean **solo** las esperadas. Cada desviacion
 * corresponde a un defecto documentado en _analisis/REGLAS.md, y este script las clasifica para
 * que quede a la vista cual es cual.
 */

const fs = require('fs');

const [, , rutaCsv, rutaJson] = process.argv;

// ---- Lectura ---------------------------------------------------------------

const filasPlsql = new Map();

/**
 * Parseo de CSV respetando las comillas.
 *
 * Hace falta de verdad: hay nombres de cuenta con comas adentro ("Electr., Gas y Agua..."), y
 * partir por coma a secas corre todas las columnas una posicion. Eso hacia parecer que el PL/SQL
 * invertia el debe y el haber cuando en realidad estaba bien.
 */
const parsearCsv = (linea) => {
  const campos = [];
  let actual = '';
  let entreComillas = false;

  for (let i = 0; i < linea.length; i++) {
    const ch = linea[i];

    if (ch === '"') {
      if (entreComillas && linea[i + 1] === '"') {
        actual += '"';
        i++;
      } else {
        entreComillas = !entreComillas;
      }
    } else if (ch === ',' && !entreComillas) {
      campos.push(actual);
      actual = '';
    } else {
      actual += ch;
    }
  }

  campos.push(actual);
  return campos;
};

for (const linea of fs.readFileSync(rutaCsv, 'utf8').split(/\r?\n/)) {
  if (!linea.trim()) continue;

  const c = parsearCsv(linea);
  // nro_cuenta, codigo, nombre, imputable, debe_acum, haber_acum, debe_per, haber_per, ...
  filasPlsql.set(c[1].trim(), {
    codigo: c[1].trim(),
    nombre: c[2],
    imputable: c[3],
    debeAcumulado: Number(c[4]),
    haberAcumulado: Number(c[5]),
    debePeriodo: Number(c[6]),
    haberPeriodo: Number(c[7]),
  });
}

const modulo = JSON.parse(fs.readFileSync(rutaJson, 'utf8')).data;
const filasModulo = new Map(modulo.filas.map((f) => [f.codigo, f]));

// ---- Contexto para clasificar ---------------------------------------------

// Las cuentas de una jurisdiccion con mas de una unidad administrativa salen al doble en el
// original (D13). Se detecta por proporcion, no por lista fija.
const esDoble = (viejo, nuevo) =>
  nuevo !== 0 && Math.abs(viejo / nuevo - 2) < 0.0001;

const esNivelUno = (codigo) => !codigo.includes('.');

/**
 * Importes que "se van" de su rama por la cuenta que se declara padre de si misma.
 *
 * El original la cuelga igual de su rama porque agrupa por prefijo de codigo; el modulo la
 * promueve a raiz y deja la anomalia a la vista. La diferencia se propaga a todos sus ancestros,
 * asi que en vez de listar codigos a mano se calcula el importe y se lo busca.
 */
const cuentasQueSeApuntanASiMismas = ['05.01.03.04.01'];

const importesHuerfanos = new Set();

for (const codigo of cuentasQueSeApuntanASiMismas) {
  const fila = modulo.filas.find((f) => f.codigo === codigo);
  if (!fila) continue;

  for (const campo of ['debePeriodo', 'haberPeriodo', 'debeAcumulado', 'haberAcumulado']) {
    if (fila[campo]) importesHuerfanos.add(Math.round(fila[campo] * 100));
  }
}

const clasificar = (codigo, viejo, nuevo) => {
  if (esNivelUno(codigo) && viejo === 0 && nuevo !== 0) {
    return 'D-nivel1: el original deja las cuentas de nivel 1 en cero';
  }

  if (esDoble(viejo, nuevo)) {
    return 'D13: el original duplica por el cruce con unidades administrativas';
  }

  // La diferencia es exactamente lo que arrastra la cuenta autorreferenciada, sea en ella misma
  // o en cualquiera de sus ancestros.
  if (importesHuerfanos.has(Math.round(Math.abs(viejo - nuevo) * 100))) {
    return 'D28: la cuenta que se apunta a si misma queda fuera de su rama';
  }

  return 'SIN CLASIFICAR';
};

// ---- Comparacion -----------------------------------------------------------

const campos = ['debePeriodo', 'haberPeriodo', 'debeAcumulado', 'haberAcumulado'];
const iguales = [];
const diferencias = [];
const soloEnUno = [];

for (const [codigo, viejo] of filasPlsql) {
  const nuevo = filasModulo.get(codigo);

  if (!nuevo) {
    soloEnUno.push({ codigo, donde: 'solo en el PL/SQL' });
    continue;
  }

  const difs = campos
    .filter((campo) => Math.abs(viejo[campo] - nuevo[campo]) > 0.005)
    .map((campo) => ({
      campo,
      viejo: viejo[campo],
      nuevo: nuevo[campo],
      motivo: clasificar(codigo, viejo[campo], nuevo[campo]),
    }));

  if (difs.length === 0) iguales.push(codigo);
  else diferencias.push({ codigo, nombre: viejo.nombre, difs });
}

for (const codigo of filasModulo.keys()) {
  if (!filasPlsql.has(codigo)) soloEnUno.push({ codigo, donde: 'solo en el modulo' });
}

// ---- Informe ---------------------------------------------------------------

const porMotivo = new Map();

for (const d of diferencias) {
  for (const x of d.difs) {
    porMotivo.set(x.motivo, (porMotivo.get(x.motivo) ?? 0) + 1);
  }
}

console.log('='.repeat(78));
console.log('PARIDAD: PL/SQL original contra modulo nuevo — Sumas y Saldos');
console.log('='.repeat(78));
console.log(`filas en el PL/SQL : ${filasPlsql.size}`);
console.log(`filas en el modulo : ${filasModulo.size}`);
console.log(`coinciden exacto   : ${iguales.length}`);
console.log(`con diferencias    : ${diferencias.length}`);
console.log(`solo en uno de los dos: ${soloEnUno.length}`);
console.log('');
console.log('Diferencias por motivo:');

for (const [motivo, cantidad] of [...porMotivo].sort((a, b) => b[1] - a[1])) {
  console.log(`  ${String(cantidad).padStart(4)}  ${motivo}`);
}

const sinClasificar = diferencias.filter((d) => d.difs.some((x) => x.motivo === 'SIN CLASIFICAR'));

if (sinClasificar.length > 0) {
  console.log('');
  console.log(`ATENCION: ${sinClasificar.length} fila(s) con diferencias que no responden a`);
  console.log('ningun defecto conocido. Hay que mirarlas una por una:');

  for (const d of sinClasificar.slice(0, 12)) {
    for (const x of d.difs.filter((y) => y.motivo === 'SIN CLASIFICAR')) {
      console.log(`  ${d.codigo.padEnd(16)} ${x.campo.padEnd(15)} viejo ${String(x.viejo).padStart(15)}  nuevo ${String(x.nuevo).padStart(15)}`);
    }
  }
}

console.log('');
console.log('Ejemplos de cada desviacion conocida:');

for (const motivo of porMotivo.keys()) {
  if (motivo === 'SIN CLASIFICAR') continue;

  const ejemplo = diferencias.find((d) => d.difs.some((x) => x.motivo === motivo));
  const campo = ejemplo.difs.find((x) => x.motivo === motivo);

  console.log(`  ${motivo}`);
  console.log(`    ${ejemplo.codigo} ${ejemplo.nombre.slice(0, 30)}`);
  console.log(`    ${campo.campo}: original ${campo.viejo} -> modulo ${campo.nuevo}`);
}

process.exit(sinClasificar.length > 0 ? 1 : 0);
