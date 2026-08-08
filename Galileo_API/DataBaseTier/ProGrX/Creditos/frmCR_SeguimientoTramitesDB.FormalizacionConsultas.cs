using System.Data;
using System.Globalization;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        /// <summary>
        /// Obtiene la frecuencia de pago y la primer deducción sugerida para la deductora seleccionada,
        /// conservando el comportamiento de cboDeductora_Click del formulario VB6.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionDeductoraContextoData>
            Cr_SeguimientoTramites_Formalizacion_Deductora_Contexto_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionDeductoraContextoRequest request)
        {
            var result = new CrSeguimientoTramitesFormalizacionDeductoraContextoData();
            if (request is null || request.deductora_id <= 0)
            {
                return DbHelper.CreateErrorResponse("Debe indicar la deductora.", -2, result);
            }

            const string sql = """
                select isnull(Frecuencia, 'M') as itmx
                from instituciones
                where cod_institucion = @DeductoraId;
                """;

            var frecuenciaResp = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sql,
                "M",
                new { DeductoraId = request.deductora_id });

            if (frecuenciaResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    frecuenciaResp.Description ?? "No fue posible obtener la frecuencia de la deductora.",
                    frecuenciaResp.Code.GetValueOrDefault(-1),
                    result);
            }

            result.frecuencia_id = (frecuenciaResp.Result ?? "M").Trim().ToUpperInvariant();
            result.frecuencias = Cr_SeguimientoTramites_Formalizacion_Frecuencias_Crear(result.frecuencia_id);
            result.primer_deduccion = _seguimientoDb.fxPrimerDeduccion(
                codEmpresa,
                request.codigo,
                request.deductora_id);

            Cr_SeguimientoTramites_Formalizacion_PrimerDeduccion_Descomponer(result);
            return DbHelper.CreateOkResponse(result);
        }

        /// <summary>
        /// Obtiene el disponible del recurso a la fecha de desembolso indicada.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionRecursoDisponibleData>
            Cr_SeguimientoTramites_Formalizacion_Recurso_Disponible_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionRecursoDisponibleRequest request)
        {
            var result = new CrSeguimientoTramitesFormalizacionRecursoDisponibleData();
            if (request is null || string.IsNullOrWhiteSpace(request.recurso))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el recurso.", -2, result);
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn => Cr_SeguimientoTramites_Formalizacion_Disponible_Obtener(
                    conn,
                    request.recurso,
                    request.fecha_desembolso));
        }

        /// <summary>
        /// Resuelve los pasos previos de cmdAplicarFormalizacion_Click que en VB6 abrían una ventana hija:
        /// requisitos pendientes y tipo de comprobante del banco para desembolsos por cheque.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionPrevalidacionData>
            Cr_SeguimientoTramites_Formalizacion_Prevalidacion_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionPrevalidacionRequest request)
        {
            var result = new CrSeguimientoTramitesFormalizacionPrevalidacionData();
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse("Debe indicar la operación.", -2, result);
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn => Cr_SeguimientoTramites_Formalizacion_Prevalidacion_Cargar(conn, request));
        }

        private static CrSeguimientoTramitesFormalizacionPrevalidacionData
            Cr_SeguimientoTramites_Formalizacion_Prevalidacion_Cargar(
                IDbConnection conn,
                CrSeguimientoTramitesFormalizacionPrevalidacionRequest request)
        {
            const string sql = """
                select isnull(count(1), 0)
                from operacion_requisitos
                where id_solicitud = @Operacion and Estado = 0;

                select isnull(max(Comprobante), '') as itmx
                from tes_banco_docs
                where tipo = 'CK' and id_banco = @BancoId;
                """;

            using SqlMapper.GridReader grid = conn.QueryMultiple(
                sql,
                new { Operacion = request.operacion, BancoId = request.banco_id });

            int pendientes = grid.ReadSingle<int>();
            string? comprobante = grid.ReadFirstOrDefault<string>();
            bool esCheque = string.Equals(
                request.emite_tipo?.Trim(),
                "CK",
                StringComparison.OrdinalIgnoreCase);

            // fxgTESTipoDocExtraeDato devuelve "ER" cuando el banco no registra el tipo de documento.
            string comprobanteNormalizado = string.IsNullOrWhiteSpace(comprobante)
                ? "ER"
                : comprobante.Trim();

            return new CrSeguimientoTramitesFormalizacionPrevalidacionData
            {
                requisitos_pendientes = pendientes,
                requiere_requisitos = pendientes > 0,
                comprobante_ck = comprobanteNormalizado,
                requiere_documento_ck = esCheque
                    && string.Equals(comprobanteNormalizado, "02", StringComparison.Ordinal),
                banco_sin_documento_ck = esCheque
                    && string.Equals(comprobanteNormalizado, "ER", StringComparison.Ordinal)
            };
        }

        private static CrSeguimientoTramitesFormalizacionRecursoDisponibleData
            Cr_SeguimientoTramites_Formalizacion_Disponible_Obtener(
                IDbConnection conn,
                string recurso,
                DateTime fechaDesembolso)
        {
            return conn.QueryFirstOrDefault<CrSeguimientoTramitesFormalizacionRecursoDisponibleData>(
                "spCRDDisponibleRecurso",
                new
                {
                    RECURSO = recurso.Trim(),
                    FECHA = fechaDesembolso.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)
                },
                commandType: CommandType.StoredProcedure)
                    ?? new CrSeguimientoTramitesFormalizacionRecursoDisponibleData();
        }

        private static List<CrSeguimientoTramitesOpcionItem>
            Cr_SeguimientoTramites_Formalizacion_Frecuencias_Crear(string frecuenciaId)
        {
            return frecuenciaId switch
            {
                "Q" =>
                [
                    new() { item = "1", descripcion = "1er Quincena" },
                    new() { item = "2", descripcion = "2da Quincena" }
                ],
                _ =>
                [
                    new() { item = "0", descripcion = "Mensual" }
                ]
            };
        }

        private static void Cr_SeguimientoTramites_Formalizacion_PrimerDeduccion_Descomponer(
            CrSeguimientoTramitesFormalizacionDeductoraContextoData data)
        {
            long periodo = (long)decimal.Truncate(data.primer_deduccion);
            data.primer_deduccion_anio = (int)(periodo / 100);
            data.primer_deduccion_mes = (int)(periodo % 100);
            data.primer_deduccion_quincena = (int)decimal.Round(
                (data.primer_deduccion - periodo) * 10m,
                0,
                MidpointRounding.AwayFromZero);
        }

        internal static decimal Cr_SeguimientoTramites_Formalizacion_PrimerDeduccion_Componer(
            int anio,
            int mes,
            int quincena)
        {
            string periodo = string.Create(
                CultureInfo.InvariantCulture,
                $"{anio:0000}{mes:00}.{quincena}");

            return decimal.TryParse(
                periodo,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal valor)
                    ? valor
                    : 0m;
        }
    }
}
