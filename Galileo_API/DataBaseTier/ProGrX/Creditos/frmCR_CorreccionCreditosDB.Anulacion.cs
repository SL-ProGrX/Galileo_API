using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using System.Data;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCorreccionCreditosDb
    {
        /// <summary>
        /// Valida y anula la formalizacion de una operacion, conservando las reglas del VB6.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="request">Operacion, usuario y nota de auditoria.</param>
        /// <returns>Resultado de la anulacion.</returns>
        public ErrorDto<CrCorreccionCreditosResultado> CR_CorreccionCreditos_Formalizacion_Anular(
            int codEmpresa,
            CrCorreccionCreditosAnularRequest request)
        {
            if (request is null || request.operacion <= 0)
                return CR_CorreccionCreditos_Anulacion_Error("Debe indicar una operación válida.");

            request.usuario = (request.usuario ?? string.Empty).Trim();
            request.notas = CR_CorreccionCreditos_Texto_Limitar((request.notas ?? string.Empty).Trim(), 500);
            if (string.IsNullOrWhiteSpace(request.usuario) || string.IsNullOrWhiteSpace(request.notas))
                return CR_CorreccionCreditos_Anulacion_Error("Debe indicar usuario y nota de anulación.");

            var globales = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, request.usuario);
            if (globales.Code != 0 || globales.Result is null)
                return CR_CorreccionCreditos_Anulacion_Error(globales.Description ?? "No fue posible obtener Globales.");

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Open();

                var operacion = CR_CorreccionCreditos_Anulacion_DatosObtener(conn, request.operacion);
                if (operacion is null)
                    return CR_CorreccionCreditos_Anulacion_Error("La operación no se encuentra activa.");

                var mensajes = CR_CorreccionCreditos_Anulacion_Validar(
                    conn,
                    request,
                    operacion,
                    globales.Result.SysPlanPagos);
                if (mensajes.Count > 0)
                    return CR_CorreccionCreditos_Anulacion_Error(string.Join(Environment.NewLine, mensajes));

                using var tx = conn.BeginTransaction();
                conn.Execute(
                    "delete refundiciones where id_solicitud = id_solicitudr and id_solicitud = @Operacion",
                    new { Operacion = request.operacion },
                    tx);

                var spResultado = conn.QueryFirstOrDefault<CrCorreccionCreditosAnulacionSp>(
                    "exec spCRDFormalizaAnulacion @Operacion, @Usuario, @Modo",
                    new { Operacion = request.operacion, Usuario = request.usuario, Modo = 0 },
                    tx);
                tx.Commit();

                MCredito.SbBitacoraCredito(_portalDb, codEmpresa, new MCredito.CrBitacoraCreditoRequest
                {
                    usuario = request.usuario,
                    movimiento = "13",
                    detalle = $"Monto : {operacion.montoapr}",
                    tipo = operacion.retencion ? "R" : "C",
                    operacion = request.operacion,
                    codigo = operacion.codigo,
                    notas = $"SGT Anula Formalización: {request.notas}"
                });

                var resultado = new CrCorreccionCreditosResultado
                {
                    mensaje = string.IsNullOrWhiteSpace(spResultado?.Mensaje)
                        ? "Anulación de formalización realizada satisfactoriamente."
                        : spResultado.Mensaje,
                    tipo_documento = "AFR",
                    numero_documento = request.operacion
                };
                if (globales.Result.SysDocVersion == 2)
                    CR_CorreccionCreditos_Reporte_Adjuntar(codEmpresa, request.usuario, resultado);
                return DbHelper.CreateOkResponse(resultado);
            }
            catch (DbException ex)
            {
                return CR_CorreccionCreditos_Anulacion_Error(ex.Message);
            }
        }

        /// <summary>Obtiene los datos mínimos necesarios para validar una anulación.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="operacion">Identificador de la operación.</param>
        /// <returns>Datos de anulación o null.</returns>
        private static CrCorreccionCreditosDatosAnulacion? CR_CorreccionCreditos_Anulacion_DatosObtener(
            IDbConnection conn,
            int operacion)
        {
            const string sql = @"
                select top 1 R.fechaforp, rtrim(isnull(R.codigo,'')) as codigo,
                       isnull(R.montoapr,0) as montoapr,
                       convert(bit,case when C.retencion='S' then 1 else 0 end) as retencion
                from reg_creditos R
                inner join catalogo C on R.codigo=C.codigo
                where R.id_solicitud=@Operacion and R.estado='A';";
            return conn.QueryFirstOrDefault<CrCorreccionCreditosDatosAnulacion>(sql, new { Operacion = operacion });
        }

        /// <summary>Ejecuta las validaciones generales previas a la anulación.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="request">Datos de la solicitud.</param>
        /// <param name="operacion">Datos actuales de la operación.</param>
        /// <param name="sysPlanPagos">Indicador del esquema de planes.</param>
        /// <returns>Mensajes que impiden la anulación.</returns>
        private static List<string> CR_CorreccionCreditos_Anulacion_Validar(
            IDbConnection conn,
            CrCorreccionCreditosAnularRequest request,
            CrCorreccionCreditosDatosAnulacion operacion,
            int sysPlanPagos)
        {
            var mensajes = new List<string>();
            var nivel = conn.ExecuteScalar<int>(@"
                select count(1)
                from NIVEL_GRUPOS N
                inner join NIVEL_MIEMBROS A on N.NV_COD_GRUPO=A.NV_COD_GRUPO
                inner join NIVEL_DERECHOS B on N.NV_COD_GRUPO=B.NV_COD_GRUPO
                where A.nombre=@Usuario and B.codigo=@Codigo and N.nv_tipo='N'
                  and @Monto between N.nv_desde and N.nv_hasta;",
                new { Usuario = request.usuario, Codigo = operacion.codigo, Monto = operacion.montoapr });
            if (nivel == 0)
                mensajes.Add($"No existe nivel de anulación para el código {operacion.codigo}.");

            var tesoreria = conn.ExecuteScalar<int>(@"
                select count(1) from TES_TRANSACCIONES
                where op=@Operacion and estado <> 'A';",
                new { Operacion = request.operacion });
            if (tesoreria > 0)
                mensajes.Add("Existen solicitudes o documentos emitidos en Tesorería; proceda a anularlos.");

            if (sysPlanPagos == 0)
                CR_CorreccionCreditos_AnulacionSinPlan_Validar(conn, request.operacion, operacion.fechaforp, mensajes);

            if (operacion.retencion)
                mensajes.Add("Una línea de retención no se puede anular.");
            return mensajes;
        }

        /// <summary>Valida movimientos posteriores cuando no existe plan de pagos.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="operacion">Identificador de la operación.</param>
        /// <param name="fechaFormalizacion">Fecha de formalización.</param>
        /// <param name="mensajes">Colección de mensajes de validación.</param>
        private static void CR_CorreccionCreditos_AnulacionSinPlan_Validar(
            IDbConnection conn,
            int operacion,
            DateTime fechaFormalizacion,
            ICollection<string> mensajes)
        {
            var movimientos = conn.ExecuteScalar<int>(@"
                select count(1) from CREDITOS_DT
                where id_solicitud=@Operacion and ncon <> convert(varchar(30),@Operacion);",
                new { Operacion = operacion });
            var mora = conn.ExecuteScalar<int>(
                "select count(1) from MOROSIDAD where id_solicitud=@Operacion",
                new { Operacion = operacion });
            if (movimientos > 0 || mora > 0)
                mensajes.Add("Existen movimientos posteriores a la formalización.");

            var refundiciones = conn.Query<CrCorreccionCreditosRefundicion>(@"
                select id_solicitud, consec from CREDITOS_DT
                where tcon in ('3','FRM') and ncon=convert(varchar(30),@Operacion);",
                new { Operacion = operacion });
            foreach (var refundicion in refundiciones)
            {
                var posteriores = conn.ExecuteScalar<int>(@"
                    select count(1) from CREDITOS_DT
                    where id_solicitud=@Operacion and consec>@Consecutivo;",
                    new { Operacion = refundicion.id_solicitud, Consecutivo = refundicion.consec });
                if (posteriores > 0)
                    mensajes.Add($"Existen movimientos posteriores en la operación refundida {refundicion.id_solicitud}.");
            }

            var posterioresFecha = conn.ExecuteScalar<int>(@"
                select count(1) from CREDITOS_DT
                where fechas>@FechaFormalizacion
                  and id_solicitud in (
                    select id_solicitud from CREDITOS_DT
                    where tcon in ('3','FRM') and ncon=convert(varchar(30),@Operacion)
                      and id_solicitud<>@Operacion);",
                new { FechaFormalizacion = fechaFormalizacion, Operacion = operacion });
            if (posterioresFecha > 0)
                mensajes.Add("Existen movimientos de refundiciones posteriores a la formalización.");

            var moraRefundiciones = conn.ExecuteScalar<int>(@"
                select count(1) from MOROSIDAD
                where estado='C' and fecult>@FechaFormalizacion
                  and id_solicitud in (
                    select id_solicitud from MOROSIDAD
                    where estado='C' and tcon in ('3','FRM')
                      and ncon=convert(varchar(30),@Operacion)
                      and id_solicitud=@Operacion);",
                new { FechaFormalizacion = fechaFormalizacion, Operacion = operacion });
            if (moraRefundiciones > 0)
                mensajes.Add("Existen movimientos de mora de refundiciones posteriores a la formalización.");
        }

        /// <summary>Crea una respuesta funcional de error para anulación.</summary>
        /// <param name="mensaje">Descripción del error.</param>
        /// <returns>Respuesta homologada.</returns>
        private static ErrorDto<CrCorreccionCreditosResultado> CR_CorreccionCreditos_Anulacion_Error(string mensaje)
            => DbHelper.CreateErrorResponse<CrCorreccionCreditosResultado>(
                mensaje,
                -2,
                new CrCorreccionCreditosResultado());

        private sealed class CrCorreccionCreditosDatosAnulacion
        {
            public DateTime fechaforp { get; set; } = default;
            public string codigo { get; set; } = string.Empty;
            public decimal montoapr { get; set; } = default;
            public bool retencion { get; set; } = default;
        }

        private sealed class CrCorreccionCreditosRefundicion
        {
            public int id_solicitud { get; set; } = default;
            public int consec { get; set; } = default;
        }

        private sealed class CrCorreccionCreditosAnulacionSp
        {
            public string Mensaje { get; set; } = string.Empty;
        }
    }
}
