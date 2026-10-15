using System.Globalization;
using System.Text;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Convenios;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Convenios
{
    /// <summary>
    /// Acceso a datos de frmCR_ConveniosLiqDetalle (detalle de la liquidación de convenios).
    /// </summary>
    public class FrmCrConveniosLiqDetalleDb
    {
        private readonly PortalDB _portalDb;

        public const string TipoCargos = "Cargos";
        public const string TipoRecaudacion = "Recaudacion";
        public const string TipoComisionRecaudacion = "ComisionRecaudacion";
        public const string TipoDevoluciones = "Devoluciones";
        public const string TipoComisionDevolucion = "ComisionDevolucion";
        public const string TipoNuevosCreditos = "NuevosCreditos";
        public const string TipoComisionCreditos = "ComisionCreditos";
        public const string TipoReservas = "Reservas";

        private const string FormatoFecha = "dd/MM/yyyy";
        private const string Todas = "TODAS";

        public FrmCrConveniosLiqDetalleDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Encabezado de la orden (sbConsultaConvenio).
        /// </summary>
        public ErrorDto<CrLiqDetalleCabecera?> CrConveniosLiqDetalle_Cabecera_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            const string sqlQuery = @"
                select
                    rtrim(O.COD_CONVENIO) as cod_convenio,
                    rtrim(isnull(C.DESCRIPCION, '')) as descripcion,
                    O.COD_ORDEN as cod_orden,
                    rtrim(isnull(O.ESTADO, '')) as estado,
                    case O.ESTADO when 'A' then 'Abierta' when 'C' then 'Cerrada' when 'T' then 'Tramitada' else '' end as estado_desc
                from CRD_CONVENIOS_ORDENES O
                    inner join CRD_CONVENIOS C on O.COD_CONVENIO = C.COD_CONVENIO
                where O.COD_CONVENIO = @CodConvenio and O.COD_ORDEN = @CodOrden;";

            return DbHelper.ExecuteSingleQuery<CrLiqDetalleCabecera>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                null,
                new { CodConvenio = codConvenio.Trim(), CodOrden = codOrden });
        }

        /// <summary>
        /// Instituciones del rubro para el combo (consulta inicial de sbConsultaConvenio).
        /// </summary>
        public ErrorDto<List<CrLiqDetalleInstitucion>> CrConveniosLiqDetalle_Instituciones_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden,
            string tipo)
        {
            var sp = ObtenerSpInstituciones(tipo);
            if (sp == null)
                return DbHelper.CreateErrorResponse(MensajeTipoInvalido(tipo), -2, new List<CrLiqDetalleInstitucion>());

            return DbHelper.ExecuteListQuery<CrLiqDetalleInstitucion>(
                _portalDb,
                codEmpresa,
                $@"exec {sp} @CodConvenio, @CodOrden",
                new { CodConvenio = codConvenio.Trim(), CodOrden = codOrden });
        }

        /// <summary>
        /// Listado y totales del rubro (sbConsultas → sbCargos / sbRecaudacion / sbDevolucion / sbCreditosNuevos).
        /// </summary>
        public ErrorDto<CrLiqDetalleConsulta?> CrConveniosLiqDetalle_Consulta_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden,
            string tipo,
            char informe,
            int? codInstitucion)
        {
            var rubro = ObtenerRubro(tipo);
            if (rubro == null)
                return DbHelper.CreateErrorResponse<CrLiqDetalleConsulta?>(MensajeTipoInvalido(tipo), -2, null);

            var spec = CrLiqDetalleEspecificaciones.Obtener(rubro.Value, informe);

            return DbHelper.WithConn<CrLiqDetalleConsulta?>(_portalDb, codEmpresa, conn =>
            {
                var filas = LeerFilas(conn, spec, codConvenio, codOrden, informe, codInstitucion);
                return ArmarConsulta(rubro.Value, spec, filas);
            });
        }

        /// <summary>
        /// Archivo CSV del rubro (sbRecaudacion_Archivo / sbDevolucion_Archivo / sbCreditos_Archivo).
        /// </summary>
        public ErrorDto<CrLiqDetalleArchivo?> CrConveniosLiqDetalle_Archivo_Generar(
            int codEmpresa,
            string codConvenio,
            int codOrden,
            string tipo,
            int? codInstitucion,
            string? nombreInstitucion)
        {
            var rubro = ObtenerRubro(tipo);
            if (rubro == null)
                return DbHelper.CreateErrorResponse<CrLiqDetalleArchivo?>(MensajeTipoInvalido(tipo), -2, null);

            if (rubro == CrLiqDetalleRubro.Cargos || tipo == TipoReservas)
                return DbHelper.CreateErrorResponse<CrLiqDetalleArchivo?>(
                    "Este rubro no genera archivo.", -2, null);

            var spec = CrLiqDetalleEspecificaciones.Obtener(rubro.Value, 'D');
            var institucion = string.IsNullOrWhiteSpace(nombreInstitucion) ? Todas : nombreInstitucion.Trim();

            return DbHelper.WithConn<CrLiqDetalleArchivo?>(_portalDb, codEmpresa, conn =>
            {
                var filas = LeerFilas(conn, spec, codConvenio, codOrden, 'D', codInstitucion);
                return new CrLiqDetalleArchivo
                {
                    nombre = NombreArchivo(rubro.Value, codConvenio, codOrden, institucion),
                    contenido = ArmarCsv(rubro.Value, filas)
                };
            });
        }

        internal static CrLiqDetalleRubro? ObtenerRubro(string tipo)
            => tipo switch
            {
                TipoCargos => CrLiqDetalleRubro.Cargos,
                TipoRecaudacion or TipoComisionRecaudacion or TipoReservas => CrLiqDetalleRubro.Recaudacion,
                TipoDevoluciones or TipoComisionDevolucion => CrLiqDetalleRubro.Devolucion,
                TipoNuevosCreditos or TipoComisionCreditos => CrLiqDetalleRubro.Creditos,
                _ => null
            };

        private static string? ObtenerSpInstituciones(string tipo)
            => tipo switch
            {
                TipoRecaudacion or TipoComisionRecaudacion => "spConvenios_Orden_Recaudacion_Institucion_Rsm",
                TipoDevoluciones or TipoComisionDevolucion => "spConvenios_Orden_Devolucion_Institucion_Rsm",
                TipoCargos or TipoNuevosCreditos or TipoComisionCreditos or TipoReservas
                    => "spConvenios_Orden_Creditos_Institucion_Rsm",
                _ => null
            };

        private static string MensajeTipoInvalido(string tipo)
            => $"El tipo de detalle '{tipo}' no es válido.";

        private static List<Dictionary<string, object?>> LeerFilas(
            SqlConnection conn,
            CrLiqDetalleConsultaSpec spec,
            string codConvenio,
            int codOrden,
            char informe,
            int? codInstitucion)
        {
            var usaInstitucion = codInstitucion.HasValue && spec.SpInstitucion != null;
            var sp = usaInstitucion ? spec.SpInstitucion! : spec.Sp;

            var parametros = new DynamicParameters();
            parametros.Add("CodConvenio", codConvenio.Trim());
            parametros.Add("CodOrden", codOrden);

            var sql = $"exec {sp} @CodConvenio, @CodOrden";
            if (spec.TieneInforme)
            {
                parametros.Add("Informe", informe.ToString());
                sql += ", @Informe";
            }

            if (usaInstitucion)
            {
                parametros.Add("CodInstitucion", codInstitucion);
                sql += ", @CodInstitucion";
            }

            return conn.Query(sql, parametros)
                .Select(r => new Dictionary<string, object?>(
                    (IDictionary<string, object?>)r, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        private static CrLiqDetalleConsulta ArmarConsulta(
            CrLiqDetalleRubro rubro,
            CrLiqDetalleConsultaSpec spec,
            List<Dictionary<string, object?>> filas)
        {
            var consulta = new CrLiqDetalleConsulta
            {
                columnas = spec.Columnas
                    .Select((c, i) => new CrLiqDetalleColumna { field = Campo(i), header = c.Header, align = c.Align })
                    .ToList()
            };
            AplicarEtiquetas(rubro, consulta);

            decimal total = 0, comision = 0, subMonto = 0, subComision = 0;
            string? institucionActual = null;

            foreach (var fila in filas)
            {
                var monto = Decimal(fila, spec.CampoTotal, spec.Sp);
                var com = spec.CampoComision != null ? Decimal(fila, spec.CampoComision, spec.Sp) : 0m;

                if (spec.AgrupaInstitucion)
                {
                    var inst = Texto(fila, "Institucion", spec.Sp);
                    institucionActual ??= inst;

                    if (institucionActual != inst)
                    {
                        // Cambio de institución: total del grupo y fila en blanco (como el VB6).
                        consulta.filas.Add(FilaTotal(spec, institucionActual, subMonto, subComision));
                        consulta.filas.Add(new CrLiqDetalleFila());
                        institucionActual = inst;
                        subMonto = 0;
                        subComision = 0;
                    }

                    subMonto += monto;
                    subComision += com;
                }

                consulta.filas.Add(FilaDetalle(spec, fila));
                total += monto;
                comision += com;
            }

            if (spec.AgrupaInstitucion && institucionActual != null)
                consulta.filas.Add(FilaTotal(spec, institucionActual, subMonto, subComision));

            consulta.total_1 = total;
            consulta.total_2 = comision;
            consulta.total_3 = total - comision;
            return consulta;
        }

        private static void AplicarEtiquetas(CrLiqDetalleRubro rubro, CrLiqDetalleConsulta consulta)
        {
            switch (rubro)
            {
                case CrLiqDetalleRubro.Cargos:
                    consulta.titulo = "Cargos de Formalización Asociados al Convenio";
                    consulta.etiqueta_total_1 = "T.Recaudado";
                    break;
                case CrLiqDetalleRubro.Recaudacion:
                    consulta.titulo = "Recaudaciones";
                    consulta.etiqueta_total_1 = "Recaudado";
                    break;
                case CrLiqDetalleRubro.Devolucion:
                    consulta.titulo = "Devoluciones";
                    consulta.etiqueta_total_1 = "Recaudado";
                    break;
                default:
                    consulta.titulo = "Créditos Formalizados";
                    consulta.etiqueta_total_1 = "T.Girado";
                    break;
            }

            consulta.muestra_comision = rubro != CrLiqDetalleRubro.Cargos;
            if (consulta.muestra_comision)
            {
                consulta.etiqueta_total_2 = "Comisión";
                consulta.etiqueta_total_3 = "Neto";
            }
        }

        private static CrLiqDetalleFila FilaDetalle(CrLiqDetalleConsultaSpec spec, Dictionary<string, object?> fila)
        {
            var resultado = new CrLiqDetalleFila();
            for (var i = 0; i < spec.Columnas.Count; i++)
            {
                var col = spec.Columnas[i];
                resultado.valores[Campo(i)] = col.Campo == null
                    ? string.Empty
                    : Formatear(Valor(fila, col.Campo, spec.Sp), col.Formato);
            }

            return resultado;
        }

        private static CrLiqDetalleFila FilaTotal(
            CrLiqDetalleConsultaSpec spec,
            string institucion,
            decimal monto,
            decimal comision)
        {
            var resultado = new CrLiqDetalleFila { es_total = true, campo_azul = Campo(spec.IndiceInstitucion) };
            for (var i = 0; i < spec.Columnas.Count; i++)
                resultado.valores[Campo(i)] = string.Empty;

            resultado.valores[Campo(1)] = new string(' ', 20) + "Total:";
            resultado.valores[Campo(2)] = monto;
            resultado.valores[Campo(3)] = comision;
            resultado.valores[Campo(4)] = monto - comision;
            resultado.valores[Campo(spec.IndiceInstitucion)] = institucion;
            return resultado;
        }

        private static string Campo(int indice) => $"c{indice}";

        private static object? Valor(Dictionary<string, object?> fila, string campo, string sp)
        {
            if (!fila.TryGetValue(campo, out var valor))
                throw new InvalidOperationException($"La columna '{campo}' no existe en el resultado de {sp}.");

            return valor is DBNull ? null : valor;
        }

        private static decimal Decimal(Dictionary<string, object?> fila, string campo, string sp)
            => Convert.ToDecimal(Valor(fila, campo, sp) ?? 0m, CultureInfo.InvariantCulture);

        private static string Texto(Dictionary<string, object?> fila, string campo, string sp)
            => Convert.ToString(Valor(fila, campo, sp), CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;

        private static object? Formatear(object? valor, CrLiqDetalleFormato formato)
        {
            if (valor == null)
                return string.Empty;

            return formato switch
            {
                CrLiqDetalleFormato.Monto => Convert.ToDecimal(valor, CultureInfo.InvariantCulture),
                CrLiqDetalleFormato.Fecha => valor is DateTime fecha ? fecha.ToString(FormatoFecha) : string.Empty,
                CrLiqDetalleFormato.Texto => Convert.ToString(valor, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty,
                _ => valor
            };
        }

        private static string NombreArchivo(CrLiqDetalleRubro rubro, string codConvenio, int codOrden, string institucion)
        {
            var prefijo = rubro switch
            {
                CrLiqDetalleRubro.Recaudacion => "Recaudacion",
                CrLiqDetalleRubro.Devolucion => "Devolucion",
                _ => "CreditosNuevos"
            };

            var inst = institucion.Length > 20 ? institucion[..20] : institucion;
            var nombre = $"{prefijo}_{codConvenio.Trim()}_{codOrden:0000} [{inst}].csv";

            foreach (var invalido in Path.GetInvalidFileNameChars())
                nombre = nombre.Replace(invalido, '_');

            return nombre;
        }

        private static byte[] ArmarCsv(CrLiqDetalleRubro rubro, List<Dictionary<string, object?>> filas)
        {
            var sb = new StringBuilder();

            if (rubro == CrLiqDetalleRubro.Creditos)
            {
                sb.AppendLine("Cédula,Nombre,Monto,Comision,Neto,Fecha,Documento,Institucion,Operación,Linea,Destino");
                foreach (var f in filas)
                {
                    sb.AppendLine(string.Join(",",
                        Csv(f, "Cedula"), Csv(f, "Nombre"), Csv(f, "Monto"), Csv(f, "Comision"), Csv(f, "Neto"),
                        CsvFecha(f, "fechaforp"), Csv(f, "Documento").Trim(), Csv(f, "InstitucionDesc"),
                        Csv(f, "id_Solicitud"), Csv(f, "Codigo"), Csv(f, "Destino")));
                }
            }
            else
            {
                sb.AppendLine("Cédula,Nombre,Monto,Fecha,Comprobante,Institucion,Referencia");
                foreach (var f in filas)
                {
                    var comprobante = rubro == CrLiqDetalleRubro.Recaudacion
                        ? $"{Csv(f, "Tcon")}-{Csv(f, "Ncon")}"
                        : $"Liq.{Csv(f, "Consec")}";
                    var referencia = rubro == CrLiqDetalleRubro.Recaudacion
                        ? $"{Csv(f, "Codigo")}.{Csv(f, "id_Solicitud")}"
                        : $"{Csv(f, "Retencion_Codigo")}.{Csv(f, "cod_Plan")}.{Csv(f, "cod_Contrato")}";

                    sb.AppendLine(string.Join(",",
                        Csv(f, "Cedula"), Csv(f, "Nombre"), Csv(f, "Monto"), CsvFecha(f, "fecha"),
                        comprobante, Csv(f, "Institucion"), referencia));
                }
            }

            // UTF-8 con BOM para que Excel respete las tildes.
            return new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        }

        private static string Csv(Dictionary<string, object?> fila, string campo)
        {
            if (!fila.TryGetValue(campo, out var valor) || valor is null or DBNull)
                return string.Empty;

            return Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static string CsvFecha(Dictionary<string, object?> fila, string campo)
            => fila.TryGetValue(campo, out var valor) && valor is DateTime fecha
                ? fecha.ToString(FormatoFecha)
                : string.Empty;
    }
}
