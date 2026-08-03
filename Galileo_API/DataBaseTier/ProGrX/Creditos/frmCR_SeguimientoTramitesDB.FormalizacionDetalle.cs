using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        private const string ERROR_OPERACION_REQUERIDA = "Debe indicar la operación.";
        /// <summary>
        /// Obtiene las operaciones a refundir de la sección CRD del lsw de formalización.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<
            CrSeguimientoTramitesFormalizacionRefundicionItem>>
            Cr_SeguimientoTramites_Formalizacion_Refundiciones_Obtener(
                int codEmpresa,
                int operacion)
        {
            const string sql = """
                select
                    R.id_solicitud,
                    rtrim(R.codigo) as codigo,
                    isnull(R.monto, 0) as monto,
                    rtrim(isnull(R.tipo, '')) as tipo,
                    case rtrim(isnull(R.tipo, ''))
                        when 'C' then 'Cancela'
                        when 'M' then 'Morosidad'
                        when 'P' then 'Pendientes'
                        else ''
                    end as tipo_descripcion,
                    rtrim(isnull(C.descripcion, '')) as descripcion
                from refundiciones R
                inner join catalogo C on R.codigo = C.codigo
                where R.id_solicitudr = @Operacion
                order by R.id_solicitud;
                """;

            return Cr_SeguimientoTramites_Formalizacion_Detalle_Listar<
                CrSeguimientoTramitesFormalizacionRefundicionItem>(
                    codEmpresa,
                    operacion,
                    sql,
                    item => item.monto);
        }

        /// <summary>
        /// Obtiene los desembolsos y rebajos de la sección DES del lsw de formalización.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<
            CrSeguimientoTramitesFormalizacionDesembolsoItem>>
            Cr_SeguimientoTramites_Formalizacion_Desembolsos_Obtener(
                int codEmpresa,
                int operacion)
        {
            const string sql = """
                select
                    rtrim(isnull(CONCEPTO, '')) as concepto,
                    isnull(monto, 0) as monto,
                    isnull(retener, 0) as retener,
                    case when isnull(retener, 0) = 1 then 'SI' else 'NO' end as retiene_descripcion
                from desembolsos
                where id_solicitud = @Operacion;
                """;

            return Cr_SeguimientoTramites_Formalizacion_Detalle_Listar<
                CrSeguimientoTramitesFormalizacionDesembolsoItem>(
                    codEmpresa,
                    operacion,
                    sql,
                    item => item.monto);
        }

        /// <summary>
        /// Obtiene las retenciones a refundir de la sección RET del lsw de formalización.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<
            CrSeguimientoTramitesFormalizacionRefundeRetencionItem>>
            Cr_SeguimientoTramites_Formalizacion_RefundeRetenciones_Obtener(
                int codEmpresa,
                int operacion)
        {
            const string sql = """
                select
                    R.id_solicitud,
                    rtrim(R.codigo) as codigo,
                    isnull(R.monto, 0) as monto,
                    rtrim(isnull(C.descripcion, '')) as descripcion
                from REFUNDE_RETENCION R
                inner join catalogo C on R.codigo = C.codigo
                where R.id_solicitudr = @Operacion
                order by R.id_solicitud;
                """;

            return Cr_SeguimientoTramites_Formalizacion_Detalle_Listar<
                CrSeguimientoTramitesFormalizacionRefundeRetencionItem>(
                    codEmpresa,
                    operacion,
                    sql,
                    item => item.monto);
        }

        /// <summary>
        /// Obtiene los requisitos de la operación de la sección REQ del lsw de formalización.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesFormalizacionRequisitoItem>>
            Cr_SeguimientoTramites_Formalizacion_Requisitos_Obtener(
                int codEmpresa,
                int operacion)
        {
            const string sql = """
                select
                    rtrim(O.cod_requisito) as cod_requisito,
                    rtrim(isnull(R.descripcion, '')) as descripcion,
                    case when isnull(O.estado, 0) = 2 then 0 else isnull(R.visible, 0) end as visible,
                    isnull(O.estado, 0) as estado
                from requisitos_adicionales R
                inner join operacion_requisitos O on R.cod_requisito = O.cod_requisito
                where O.id_solicitud = @Operacion
                order by O.estado, R.cod_requisito;
                """;

            return DbHelper.ExecuteListQuery<CrSeguimientoTramitesFormalizacionRequisitoItem>(
                _portalDb,
                codEmpresa,
                sql,
                new { Operacion = operacion });
        }

        /// <summary>
        /// Obtiene los cargos adicionales de la sección CAR del lsw de formalización.
        /// El total suma únicamente las bases Crédito y Avalúo, tal como el VB6.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<
            CrSeguimientoTramitesFormalizacionCargoItem>>
            Cr_SeguimientoTramites_Formalizacion_Cargos_Obtener(
                int codEmpresa,
                int operacion)
        {
            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn => conn.Query<CrSeguimientoTramitesFormalizacionCargoItem>(
                    "exec spCrdOperacionFormalizaCargosLista @Operacion;",
                    new { Operacion = operacion }).ToList());

            if (response.Code != 0 || response.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    response.Description ?? "No fue posible obtener los cargos adicionales.",
                    response.Code.GetValueOrDefault(-1),
                    new CrSeguimientoTramitesFormalizacionDetalleLista<
                        CrSeguimientoTramitesFormalizacionCargoItem>());
            }

            List<CrSeguimientoTramitesFormalizacionCargoItem> lista = response.Result;
            foreach (CrSeguimientoTramitesFormalizacionCargoItem item in lista)
            {
                Cr_SeguimientoTramites_Formalizacion_Cargo_Describir(item);
            }

            return DbHelper.CreateOkResponse(
                new CrSeguimientoTramitesFormalizacionDetalleLista<
                    CrSeguimientoTramitesFormalizacionCargoItem>
                {
                    lista = lista,
                    total = lista
                        .Where(item => item.@base is "C" or "A")
                        .Sum(item => item.monto)
                });
        }

        /// <summary>
        /// Obtiene los fiadores y co-deudores de la sección FIA del lsw de formalización.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesFormalizacionFiadorItem>>
            Cr_SeguimientoTramites_Formalizacion_Fiadores_Obtener(
                int codEmpresa,
                int operacion)
        {
            const string sql = """
                select
                    rtrim(S.cedula) as cedula,
                    rtrim(isnull(S.nombre, '')) as nombre
                from Fiadores F
                inner join Socios S on F.cedulaf = S.cedula
                where F.estado = 'A' and F.id_solicitud = @Operacion;
                """;

            return DbHelper.ExecuteListQuery<CrSeguimientoTramitesFormalizacionFiadorItem>(
                _portalDb,
                codEmpresa,
                sql,
                new { Operacion = operacion });
        }

        /// <summary>
        /// Obtiene el impacto en liquidez de la sección ILI del lsw de formalización.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionImpactoLiquidezData>
            Cr_SeguimientoTramites_Formalizacion_ImpactoLiquidez_Obtener(
                int codEmpresa,
                int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    ERROR_OPERACION_REQUERIDA,
                    -2,
                    new CrSeguimientoTramitesFormalizacionImpactoLiquidezData());
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn => conn.QueryFirstOrDefault<
                    CrSeguimientoTramitesFormalizacionImpactoLiquidezData>(
                        "exec spCrd_SGT_Impacto_Liquidez @Operacion;",
                        new { Operacion = operacion })
                    ?? new CrSeguimientoTramitesFormalizacionImpactoLiquidezData());
        }

        /// <summary>
        /// Obtiene el deudor, fiadores y co-deudores con su estado de firma
        /// para la sección FIR del lsw de formalización.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesFormalizacionFirmaItem>>
            Cr_SeguimientoTramites_Formalizacion_Firmas_Obtener(
                int codEmpresa,
                int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    ERROR_OPERACION_REQUERIDA,
                    -2,
                    new List<CrSeguimientoTramitesFormalizacionFirmaItem>());
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn => Cr_SeguimientoTramites_Formalizacion_Firmas_Cargar(conn, operacion));
        }

        /// <summary>
        /// Registra o retira la firma del deudor o de un fiador,
        /// equivalente a lsw_ItemCheck del formulario VB6.
        /// </summary>
        public ErrorDto Cr_SeguimientoTramites_Formalizacion_Firma_Actualizar(
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionFirmaActualizarRequest request)
        {
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.ErrorResponse(ERROR_OPERACION_REQUERIDA, -2);
            }

            bool esDeudor = string.Equals(
                request.calidad?.Trim(),
                "D",
                StringComparison.OrdinalIgnoreCase);

            if (esDeudor)
            {
                const string sqlDeudor = """
                    update reg_creditos
                    set firma_deudor = @Firma,
                        fechaforf = dbo.MyGetdate()
                    where id_solicitud = @Operacion;
                    """;

                return DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlDeudor,
                    new { Firma = request.firma ? 1 : 0, Operacion = request.operacion });
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.ErrorResponse("Debe indicar la identificación del fiador.", -2);
            }

            const string sqlFiador = """
                update fiadores
                set firma = @Firma
                where ID_SOLICITUD = @Operacion and CedulaF = @Cedula;
                """;

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlFiador,
                new
                {
                    Firma = request.firma ? "S" : "N",
                    Operacion = request.operacion,
                    Cedula = request.cedula.Trim()
                });
        }

        private static List<CrSeguimientoTramitesFormalizacionFirmaItem>
            Cr_SeguimientoTramites_Formalizacion_Firmas_Cargar(
                IDbConnection conn,
                int operacion)
        {
            const string sql = """
                select
                    rtrim(R.cedula) as cedula,
                    rtrim(isnull(S.nombre, '')) as nombre,
                    isnull(R.firma_deudor, 0) as firma_deudor
                from reg_creditos R
                inner join Socios S on R.cedula = S.cedula
                where R.id_solicitud = @Operacion;

                select
                    rtrim(F.cedulaf) as cedulaf,
                    rtrim(isnull(S.nombre, '')) as nombre,
                    rtrim(isnull(F.Calidad, '')) as calidad,
                    rtrim(isnull(F.Firma, 'N')) as firma
                from Fiadores F
                inner join Socios S on F.cedulaf = S.cedula
                where F.estado = 'A' and F.id_solicitud = @Operacion;
                """;

            using SqlMapper.GridReader grid = conn.QueryMultiple(
                sql,
                new { Operacion = operacion });

            var firmas = new List<CrSeguimientoTramitesFormalizacionFirmaItem>();

            CrSeguimientoTramitesFormalizacionDeudorFirmaRaw? deudor =
                grid.ReadFirstOrDefault<CrSeguimientoTramitesFormalizacionDeudorFirmaRaw>();

            if (deudor is not null)
            {
                firmas.Add(Cr_SeguimientoTramites_Formalizacion_Firma_Crear(
                    deudor.cedula,
                    "D",
                    "Deudor",
                    deudor.nombre,
                    deudor.firma_deudor == 1));
            }

            foreach (CrSeguimientoTramitesFormalizacionFiadorFirmaRaw fiador in
                grid.Read<CrSeguimientoTramitesFormalizacionFiadorFirmaRaw>())
            {
                bool esFiador = string.Equals(
                    fiador.calidad,
                    "F",
                    StringComparison.OrdinalIgnoreCase);

                firmas.Add(Cr_SeguimientoTramites_Formalizacion_Firma_Crear(
                    fiador.cedulaf,
                    fiador.calidad,
                    esFiador ? "Fiadores" : "Co-Dedudor",
                    fiador.nombre,
                    string.Equals(fiador.firma, "S", StringComparison.OrdinalIgnoreCase)));
            }

            return firmas;
        }

        private static CrSeguimientoTramitesFormalizacionFirmaItem
            Cr_SeguimientoTramites_Formalizacion_Firma_Crear(
                string cedula,
                string calidad,
                string tipoDescripcion,
                string nombre,
                bool firma)
        {
            return new CrSeguimientoTramitesFormalizacionFirmaItem
            {
                cedula = cedula.Trim(),
                calidad = calidad.Trim(),
                tipo_descripcion = tipoDescripcion,
                nombre = nombre.Trim(),
                firma = firma,
                firma_descripcion = firma ? "SI" : "NO"
            };
        }

        private static void Cr_SeguimientoTramites_Formalizacion_Cargo_Describir(
            CrSeguimientoTramitesFormalizacionCargoItem item)
        {
            item.@base = item.@base.Trim();
            item.tipo = item.tipo.Trim();
            item.base_descripcion = item.@base switch
            {
                "C" => "Crédito",
                "A" => "Avalúo",
                "P" => "Prima",
                _ => string.Empty
            };
            item.tipo_descripcion = string.Equals(
                item.tipo,
                "P",
                StringComparison.OrdinalIgnoreCase)
                    ? "Porcentaje"
                    : "Monto";
        }

        private ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<T>>
            Cr_SeguimientoTramites_Formalizacion_Detalle_Listar<T>(
                int codEmpresa,
                int operacion,
                string sql,
                Func<T, decimal> monto)
        {
            var vacia = new CrSeguimientoTramitesFormalizacionDetalleLista<T>();
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse("Debe indicar la operación.", -2, vacia);
            }

            var response = DbHelper.ExecuteListQuery<T>(
                _portalDb,
                codEmpresa,
                sql,
                new { Operacion = operacion });

            if (response.Code != 0 || response.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    response.Description ?? "No fue posible obtener el detalle.",
                    response.Code.GetValueOrDefault(-1),
                    vacia);
            }

            return DbHelper.CreateOkResponse(
                new CrSeguimientoTramitesFormalizacionDetalleLista<T>
                {
                    lista = response.Result,
                    total = response.Result.Sum(monto)
                });
        }
    }
}
