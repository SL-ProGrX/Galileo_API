using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrConsultaBitacoraDB
    {
        private readonly PortalDB _portalDB;

        private const string SpRegistro = "spSIFPersonaMovimientos";
        private const string SpCreditos = "spCrdPersonaMovimientos";
        private const string SpFondos = "spSys_Consulta_Integrada_Mov_Fnd";
        private const string SpPatrimonio = "spSys_Consulta_Integrada_Mov_Pat";
        private const string SpBancos = "spSys_Consulta_Integrada_Mov_Bancos";

        public FrmCrConsultaBitacoraDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la información inicial de la pantalla de consulta de bitácora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaBitacoraEncabezadoDto> CR_ConsultaBitacora_Encabezado_Obtener(int CodEmpresa, string cedula)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        rtrim(cedula) as cedula,
                        rtrim(nombre) as nombre,
                        dbo.MyGetdate() as fecha_servidor
                    from socios
                    where cedula = @cedula;";

                var result = conn.QueryFirstOrDefault<CrConsultaBitacoraEncabezadoDto>(
                    sql,
                    new { cedula = (cedula ?? string.Empty).Trim() }
                ) ?? new CrConsultaBitacoraEncabezadoDto();

                result.fecha_inicio = result.fecha_servidor?.Date.AddDays(-7);
                result.fecha_corte = result.fecha_servidor?.Date;

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConsultaBitacoraEncabezadoDto>(
                    ex.Message,
                    -1,
                    new CrConsultaBitacoraEncabezadoDto()
                );
            }
        }

        /// <summary>
        /// Obtiene la lista de movimientos generales de la persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraRegistroDto>> CR_ConsultaBitacora_Registro_Lista_Obtener(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return ConsultarLista<CrConsultaBitacoraRegistroDto>(CodEmpresa, request, SpRegistro, false);
        }

        /// <summary>
        /// Exporta la lista de movimientos generales de la persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraRegistroDto>> CR_ConsultaBitacora_Registro_Lista_Export(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return CR_ConsultaBitacora_Registro_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista de movimientos de créditos de la persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraCreditosDto>> CR_ConsultaBitacora_Creditos_Lista_Obtener(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return ConsultarLista<CrConsultaBitacoraCreditosDto>(CodEmpresa, request, SpCreditos, false);
        }

        /// <summary>
        /// Exporta la lista de movimientos de créditos de la persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraCreditosDto>> CR_ConsultaBitacora_Creditos_Lista_Export(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return CR_ConsultaBitacora_Creditos_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista de movimientos de fondos de la persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraFondosDto>> CR_ConsultaBitacora_Fondos_Lista_Obtener(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return ConsultarLista<CrConsultaBitacoraFondosDto>(CodEmpresa, request, SpFondos, true);
        }

        /// <summary>
        /// Exporta la lista de movimientos de fondos de la persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraFondosDto>> CR_ConsultaBitacora_Fondos_Lista_Export(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return CR_ConsultaBitacora_Fondos_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista de movimientos de patrimonio de la persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraPatrimonioDto>> CR_ConsultaBitacora_Patrimonio_Lista_Obtener(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return ConsultarLista<CrConsultaBitacoraPatrimonioDto>(CodEmpresa, request, SpPatrimonio, false);
        }

        /// <summary>
        /// Exporta la lista de movimientos de patrimonio de la persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraPatrimonioDto>> CR_ConsultaBitacora_Patrimonio_Lista_Export(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return CR_ConsultaBitacora_Patrimonio_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista de movimientos de bancos de la persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraBancosDto>> CR_ConsultaBitacora_Bancos_Lista_Obtener(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return ConsultarLista<CrConsultaBitacoraBancosDto>(CodEmpresa, request, SpBancos, false);
        }

        /// <summary>
        /// Exporta la lista de movimientos de bancos de la persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraBancosDto>> CR_ConsultaBitacora_Bancos_Lista_Export(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return CR_ConsultaBitacora_Bancos_Lista_Obtener(CodEmpresa, request);
        }

        private ErrorDto<CrConsultaBitacoraLista<T>> ConsultarLista<T>(
            int CodEmpresa,
            CrConsultaBitacoraRequest request,
            string storedProcedure,
            bool incluyeMovBancario)
        {
            var listaVacia = new CrConsultaBitacoraLista<T>();

            try
            {
                var req = NormalizarRequest(request);
                if (string.IsNullOrWhiteSpace(req.cedula))
                {
                    return DbHelper.CreateOkResponse(listaVacia);
                }

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var parametros = CrearParametros(req, incluyeMovBancario);

                var lista = conn.Query<T>(
                    storedProcedure,
                    parametros,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return DbHelper.CreateOkResponse(new CrConsultaBitacoraLista<T>
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConsultaBitacoraLista<T>>(
                    ex.Message,
                    -1,
                    listaVacia
                );
            }
        }

        private static CrConsultaBitacoraRequest NormalizarRequest(CrConsultaBitacoraRequest? request)
        {
            var fechaInicio = request?.fecha_inicio?.Date ?? DateTime.Today.AddDays(-7);
            var fechaCorte = request?.fecha_corte?.Date ?? DateTime.Today;

            return new CrConsultaBitacoraRequest
            {
                cedula = (request?.cedula ?? string.Empty).Trim(),
                fecha_inicio = fechaInicio,
                fecha_corte = fechaCorte.AddDays(1).AddTicks(-1),
                mov_bancario = request?.mov_bancario ?? false
            };
        }

        private static DynamicParameters CrearParametros(CrConsultaBitacoraRequest request, bool incluyeMovBancario)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@Cedula", request.cedula);
            parametros.Add("@Inicio", request.fecha_inicio);
            parametros.Add("@Corte", request.fecha_corte);

            if (incluyeMovBancario)
            {
                parametros.Add("@MovBancario", request.mov_bancario.GetValueOrDefault() ? 1 : 0);
            }

            return parametros;
        }
    }
}