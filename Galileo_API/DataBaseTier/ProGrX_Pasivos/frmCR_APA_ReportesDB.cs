using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Pasivos;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX_Pasivos
{
    public class FrmCrApaReportesDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrApaReportesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el listado de acreedores para dropdown.
        /// Permite ordenar por código o por descripción según parámetro.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="ordenarPor">Valores esperados: "cod" o "descrip".</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_APA_Reportes_Acreedores_Dropdown_Obtener(
            int codEmpresa,
            string ordenarPor)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var orden = (ordenarPor ?? string.Empty).Trim().ToLowerInvariant();
                var sortCode = orden == "descrip" ? 2 : 1;

                const string sql = @"
                    SELECT
                        RTRIM(COD_ACREEDOR) AS item,
                        RTRIM(DESCRIPCION) AS descripcion
                    FROM CRD_APA_ACREEDORES
                    ORDER BY
                        CASE WHEN @sortCode = 1 THEN COD_ACREEDOR END,
                        CASE WHEN @sortCode = 2 THEN DESCRIPCION END;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new { sortCode }).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message, -1, []);
            }
        }

        /// <summary>
        /// Obtiene las operaciones APA por acreedor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codAcreedor"></param>
        /// <returns></returns>
        public ErrorDto<List<CrApaReportesOperacion>> CR_APA_Reportes_Operaciones_Obtener(int codEmpresa, string codAcreedor)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sql = @"
                    SELECT
                        OPERACION AS Operacion,
                        RTRIM(COD_ACREEDOR) AS Acreedor,
                        MONTO AS Monto,
                        SALDO AS Saldo,
                        FECHA_FORMALIZA AS Formaliza
                    FROM CRD_APA_OPERACIONES
                    WHERE COD_ACREEDOR = @CodAcreedor;";

                var lista = conn.Query<CrApaReportesOperacion>(sql, new { CodAcreedor = codAcreedor }).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrApaReportesOperacion>>(ex.Message, -1, []);
            }
        }

        /// <summary>
        /// Consulta los datos base del acreedor seleccionado usando SP.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codAcreedor"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaMovimientosAcreedorDto?> CR_APA_Reportes_Acreedor_Obtener(int codEmpresa, string codAcreedor)
        {
            codAcreedor = (codAcreedor ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(codAcreedor))
            {
                return DbHelper.CreateErrorResponse<FrmCrApaMovimientosAcreedorDto?>(
                    "Debe indicar el acreedor.",
                    -1,
                    null);
            }

            return DbHelper.ExecuteSingleQuery<FrmCrApaMovimientosAcreedorDto?>(
                _portalDb,
                codEmpresa,
                "exec spAPA_ConsultaAcreedor @Acreedor",
                null,
                new { Acreedor = codAcreedor });
        }

        /// <summary>
        /// Consulta el resumen principal de una operación APA usando SP.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codAcreedor"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaMovimientosOperacionDto?> CR_APA_Reportes_Operacion_Obtener(
            int codEmpresa,
            string codAcreedor,
            string operacion)
        {            
            return DbHelper.ExecuteSingleQuery<FrmCrApaMovimientosOperacionDto?>(
                _portalDb,
                codEmpresa,
                "exec spAPA_ConsultaOperacion @Acreedor, @Operacion",
                null,
                new
                {
                    Acreedor = codAcreedor,
                    Operacion = operacion
                });
        }

        /// <summary>
        /// Obtiene el saldo de operaciones APA a una fecha de corte usando SP.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="fechaCorte"></param>
        /// <returns></returns>
        public ErrorDto<List<CrApaReportesSaldoCorte>> CR_APA_Reportes_SaldosCorte_Obtener(int codEmpresa, DateTime fechaCorte)
        {
            return DbHelper.ExecuteListQuery<CrApaReportesSaldoCorte>(
                _portalDb,
                codEmpresa,
                "exec spCrdApaSaldosCorte @Fecha",
                new { Fecha = fechaCorte });
        }

        /// <summary>
        /// Valida si existe corte auxiliar para la fecha indicada.
        /// Compara contra la fecha con hora 23:59:00.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="fechaCorte"></param>
        /// <returns></returns>
        public ErrorDto<int> CR_APA_Reportes_AuxiliarCorte_Existe(int codEmpresa, DateTime fechaCorte)
        {
            var fechaFinDia = fechaCorte.Date.AddHours(23).AddMinutes(59);

            const string sql = @"
                SELECT COUNT(*)
                FROM CRD_APA_ACREEDORES
                WHERE CORTE_FECHA = @FechaCorte;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { FechaCorte = fechaFinDia });
        }

        /// <summary>
        /// Ejecuta el proceso de auxiliar al corte para la fecha indicada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="fechaCorte"></param>
        /// <returns></returns>
        public ErrorDto CR_APA_Reportes_AuxiliarCorte_Aplicar(int codEmpresa, DateTime fechaCorte)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                "exec spAPA_AuxiliarCorte @Fecha",
                new { Fecha = fechaCorte });
        }
    }
}
