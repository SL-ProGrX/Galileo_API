using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.DataBaseTier
{
    public class FrmSifComunicadosDB
    {
        private readonly IConfiguration _config;

        public FrmSifComunicadosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Inserta un comunicado en la base de datos del cliente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="comunicado"></param>
        /// <returns></returns>
        public ErrorDto Comunicados_Insertar(int CodCliente, SifComunicadoDto comunicado)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                const string query = @"INSERT INTO sif_comunicados (
    fecha,
    inicio,
    corte,
    usuario,
    nota,
    ffuente,
    fcolor,
    fcursiva,
    fnegrita
) VALUES (
    @fecha,
    @inicio,
    @corte,
    @usuario,
    @nota,
    @ffuente,
    @fcolor,
    @fcursiva,
    @fnegrita
);";

                var p = new
                {
                    fecha = DateTime.Now,
                    inicio = comunicado.inicio,
                    corte = comunicado.corte,
                    usuario = comunicado.usuario,
                    nota = comunicado.nota,
                    ffuente = comunicado.ffuente,
                    fcolor = comunicado.fcolor,
                    fcursiva = comunicado.fcursiva,
                    fnegrita = comunicado.fnegrita
                };

                connection.Execute(query, p);
            }
            catch (Exception ex)
            {
                info.Code = -1;
                if (ex.Message.Contains("Cannot insert duplicate key"))
                {
                    info.Description = "El código de comunicado ya existe";
                }
                else
                {
                    info.Description = ex.Message;
                }
            }
            return info;
        }


        /// <summary>
        /// Consulta el consecutivo del comunicado anterior o siguiente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<int> ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<int> response = new ErrorDto<int>();
            response.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string qDescFirst = @"select Top 1 COD_COMUNICADO from sif_comunicados
order by COD_COMUNICADO desc;";

                const string qDescPrev = @"select Top 1 COD_COMUNICADO from sif_comunicados
where COD_COMUNICADO < @consecutivo
order by COD_COMUNICADO desc;";

                const string qAscNext = @"select Top 1 COD_COMUNICADO from sif_comunicados
where COD_COMUNICADO > @consecutivo
order by COD_COMUNICADO asc;";

                string t = (tipo ?? string.Empty).Trim().ToLowerInvariant();

                if (t == "desc")
                {
                    if (consecutivo == 0)
                        response.Result = connection.ExecuteScalar<int>(qDescFirst);
                    else
                        response.Result = connection.ExecuteScalar<int>(qDescPrev, new { consecutivo });
                }
                else
                {
                    response.Result = connection.ExecuteScalar<int>(qAscNext, new { consecutivo });
                }

                if (response.Result == 0)
                {
                    response.Code = -2;
                    response.Description = "No se encontraron más resultados.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }


        /// <summary>
        /// Obtiene un comunicado por su código
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Comunicado"></param>
        /// <returns></returns>
        public ErrorDto<SifComunicadoDto> Comunicado_Obtener(int CodEmpresa, int Cod_Comunicado)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<SifComunicadoDto> response = new ErrorDto<SifComunicadoDto>();
            response.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                const string query = "SELECT * FROM sif_comunicados WHERE COD_COMUNICADO = @Cod_Comunicado";

                response.Result = connection.Query<SifComunicadoDto>(query, new { Cod_Comunicado }).FirstOrDefault();
                if (response.Result == null)
                {
                    response.Code = -2;
                    response.Description = "No se encontraron resultados.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        /// <summary>
        /// Obtiene lista de comunicados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<SifComunicadoDto>> ComunicadosLista_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<List<SifComunicadoDto>> response = new ErrorDto<List<SifComunicadoDto>>();
            response.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                const string query = "SELECT * FROM sif_comunicados";

                response.Result = connection.Query<SifComunicadoDto>(query).ToList();
                if (response.Result == null || !response.Result.Any())
                {
                    response.Code = -2;
                    response.Description = "No se encontraron resultados.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        /// <summary>
        /// Actualiza un comunicado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Comunicado_Actualizar(int CodEmpresa, SifComunicadoDto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"UPDATE sif_comunicados
SET INICIO = @inicio,
    CORTE = @corte,
    NOTA = @nota,
    FFUENTE = @ffuente,
    FCOLOR = @fcolor,
    FCURSIVA = @fcursiva,
    FNEGRITA = @fnegrita
WHERE COD_COMUNICADO = @cod_comunicado;";

                var p = new
                {
                    inicio = request.inicio,
                    corte = request.corte,
                    nota = request.nota,
                    ffuente = request.ffuente,
                    fcolor = request.fcolor,
                    fcursiva = request.fcursiva,
                    fnegrita = request.fnegrita,
                    cod_comunicado = request.cod_comunicado
                };

                info.Code = connection.Execute(query, p);
                info.Description = "Ok";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

    }
}