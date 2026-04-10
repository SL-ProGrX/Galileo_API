using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOAplExcContratosInformesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmCOAplExcContratosInformesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data">Información de bitácora a registrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene el catálogo fijo de reportes de la pantalla de Aplicación de Excedentes a Mora por Contratos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Lista de reportes disponibles.</returns>
        public ErrorDto<List<CoAplExcContratosInformeItemDto>> CO_AplExc_Contratos_Informes_Catalogo_Obtener(int CodEmpresa)
        {
            var lista = new List<CoAplExcContratosInformeItemDto>
            {
                new() { codigo = "x00", descripcion = "Auxiliar de Aplicación de Excedentes a Mora" },
                new() { codigo = "x01", descripcion = "Auxiliar de Aplicación de Excedentes a Mora Detallado" },
                new() { codigo = "x02", descripcion = "Desglose de Aplicación de Excedentes a Mora" },
                new() { codigo = "x03", descripcion = "Contratos de Aplicación de Excedentes a Mora" }
            };

            return DbHelper.CreateOkResponse(lista);
        }

        /// <summary>
        /// Obtiene la lista de aplicaciones para buscador (F4).
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="texto">Texto de búsqueda opcional por aplicación, usuario o fecha.</param>
        /// <returns>Lista de aplicaciones disponibles para F4.</returns>
        public ErrorDto<List<CoAplExcContratosAplicacionF4Dto>> CO_AplExc_Contratos_Informes_Aplicaciones_F4_Obtener(int CodEmpresa, string? texto)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                texto = (texto ?? string.Empty).Trim();
                var like = texto.Length > 0 ? $"%{texto}%" : null;

                const string sql = @"
                    select
                        ID_APLICACION as id_aplicacion,
                        FECHA as fecha,
                        rtrim(isnull(USUARIO, '')) as usuario
                    from CBR_APLICA_CONTRATOS_EXCED_MORA
                    where @texto = ''
                       or cast(ID_APLICACION as varchar(20)) like @like
                       or rtrim(isnull(USUARIO, '')) like @like
                       or convert(varchar(10), FECHA, 103) like @like
                    order by FECHA desc, ID_APLICACION desc;";

                var lista = conn.Query<CoAplExcContratosAplicacionF4Dto>(sql, new
                {
                    texto,
                    like
                }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CoAplExcContratosAplicacionF4Dto>>(
                    ex.Message,
                    -1,
                    new List<CoAplExcContratosAplicacionF4Dto>());
            }
        }

        /// <summary>
        /// Obtiene la lista de personas para buscador (F4).
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="texto">Texto de búsqueda opcional por cédula, identificación alterna o nombre.</param>
        /// <returns>Lista de personas disponibles para F4.</returns>
        public ErrorDto<List<CoAplExcContratosPersonaF4Dto>> CO_AplExc_Contratos_Informes_Personas_F4_Obtener(int CodEmpresa, string? texto)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                texto = (texto ?? string.Empty).Trim();
                var like = texto.Length > 0 ? $"%{texto}%" : null;

                const string sql = @"
                    select
                        rtrim(isnull(CEDULA, '')) as cedula,
                        rtrim(isnull(CEDULAR, '')) as cedular,
                        rtrim(isnull(NOMBRE, '')) as nombre
                    from SOCIOS
                    where @texto = ''
                       or rtrim(isnull(CEDULA, '')) like @like
                       or rtrim(isnull(CEDULAR, '')) like @like
                       or rtrim(isnull(NOMBRE, '')) like @like
                    order by NOMBRE;";

                var lista = conn.Query<CoAplExcContratosPersonaF4Dto>(sql, new
                {
                    texto,
                    like
                }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CoAplExcContratosPersonaF4Dto>>(
                    ex.Message,
                    -1,
                    new List<CoAplExcContratosPersonaF4Dto>());
            }
        }
    }
}