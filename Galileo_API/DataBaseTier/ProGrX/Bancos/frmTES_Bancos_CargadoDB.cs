using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo.Models.ProGrX.Bancos;
using Microsoft.ReportingServices.Diagnostics.Internal;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesBancosCargadoDB
    {
        private readonly PortalDB _portalDB;


        public FrmTesBancosCargadoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la cuenta de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaBancosCargados>> Tes_Bancos_Obtener(int CodEmpresa, string usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = "exec spTes_Cuenta_Bancaria_Acceso @usuario, @TipoDoc, @Acceso";

                return conn.Query<DropDownListaBancosCargados>(query, new { @Usuario = usuario, @TipoDoc = "DP", @Acceso = "SOL" }).ToList(); 
            });
        }


        /// <summary>
        /// Metodo para obtener los conceptos 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TesBancoCargadoConceptos>> Tes_BancosCargadoConceptos_Obtener(int CodEmpresa, string? concepto = null)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var conceptoTrim = concepto?.Trim();
                var hasConcepto = !string.IsNullOrWhiteSpace(conceptoTrim);

                const string sql = @"
                            SELECT
                                COD_CONCEPTO,
                                DESCRIPCION,
                                COD_CUENTA_MASK,
                                DP_TRAMITE_APL,
                                CUENTA_DESC
                            FROM vTes_Conceptos
                            WHERE AUTO_REGISTRO = 1
                              AND ESTADO = 'A'
                              AND (@concepto IS NULL OR COD_CONCEPTO = @concepto);";

                var response = conn.Query<TesBancoCargadoConceptos>(
                    sql,
                    new { concepto = hasConcepto ? conceptoTrim : null }
                ).ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
               return DbHelper.CreateErrorResponse<List<TesBancoCargadoConceptos>>(ex.Message);
            }
            
        }

        /// <summary>
        /// Metodo para obtener las unidades asociadas 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancosCargadoCentroUnidades_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"select COD_UNIDAD as 'item', DESCRIPCION from vCNTX_UNIDADES_LOCAL";
                var Result = conn.Query<DropDownListaGenericaModel>(query).ToList();
                return DbHelper.CreateOkResponse(Result);
            }
            catch (Exception ex)
            {
               return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Metodo para obtener los centros de costos 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancosCargadoCentroCostos_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"select COD_CENTRO_COSTO as 'item', DESCRIPCION from vCNTX_CENTRO_COSTO_LOCAL";
                var request = conn.Query<DropDownListaGenericaModel>(query).ToList();

                return DbHelper.CreateOkResponse(request);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            } 
        }


        /// <summary>
        /// Metodo para obtener una lista de registros de auto registro de tesorer�a con paginaci�n y filtros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesAutoRegistroLista> Tes_AutoRegistroLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var result = new ErrorDto<TesAutoRegistroLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new TesAutoRegistroLista()
                {
                    total = 0,
                    lista = new List<TesAutoRegistroDto>()
                }
            };
            try
            {
                var filtro = filtros?.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(filtro);

                
                var offset = (filtros?.pagina).GetValueOrDefault(0);
                var fetch = (filtros?.paginacion).GetValueOrDefault(0);
                var usarPaginacion = fetch > 0;

                var filtroLike = hasFiltro ? $"%{filtro}%" : null;

                const string sqlCount = @"
                    SELECT COUNT(1)
                    FROM vTES_AUTO_REGISTRO
                    WHERE
                        (@filtro IS NULL)
                     OR (CAST(id_auto AS NVARCHAR(50)) LIKE @filtroLike)
                     OR (descripcion LIKE @filtroLike)
                     OR (palabras_clave LIKE @filtroLike);";

                result.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    filtro = hasFiltro ? filtro : null,
                    filtroLike
                });

                var sqlList = @"
                    SELECT *
                    FROM vTES_AUTO_REGISTRO
                    WHERE
                        (@filtro IS NULL)
                     OR (CAST(id_auto AS NVARCHAR(50)) LIKE @filtroLike)
                     OR (descripcion LIKE @filtroLike)
                     OR (palabras_clave LIKE @filtroLike)
                    ORDER BY id_auto ";

                if (usarPaginacion)
                {
                    sqlList += @"
                        OFFSET @offset ROWS
                        FETCH NEXT @fetch ROWS ONLY;";
                }

                result.Result.lista = conn.Query<TesAutoRegistroDto>(sqlList, new
                {
                    filtro = hasFiltro ? filtro : null,
                    filtroLike,
                    offset,
                    fetch
                }).ToList();

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<TesAutoRegistroDto>();
            }
            return result;
        }


        /// <summary>
        /// Aplica el archivo de bancos cargado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_banco"></param>
        /// <param name="usuario"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public ErrorDto TES_BancosCargados_Aplicar(int CodEmpresa, string cod_banco, string usuario, List<TesCargadoExcelDto> file)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = ""
            };

            try
            {
                var sb = new StringBuilder();
                foreach (var row in file)
                {

                    var query = @"EXEC spTes_Bancos_Mov_Load @IdBanco, @Fecha, @Documento, @TipoMov, @Importe,@Descripcion";

                    var result = conn.Query<int>(query, new
                    {
                        IdBanco = cod_banco,
                        Fecha = row.fecha,
                        Documento = row.documento,
                        TipoMov = row.tipo,
                        Importe = row.importe,
                        Descripcion = row.descripcion,
                    }).FirstOrDefault();

                    if (result == -1)
                    {
                        sb.AppendLine($"Documento Repetido: [{row.documento}]");
                    }
                }

                response.Description = sb.ToString();

                if (response.Description.Length > 0)
                {
                    response.Code = -1;
                }
                else
                {
                    response.Description = "Ok";
                }
                return response;
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los registros de bancos cargados pendientes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<TeslistaRegistroBancosDto>> TES_ListaRegistroBancos_Obtener(int CodEmpresa, string filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            TesFiltrosRegistroBancoDto filtro = JsonConvert.DeserializeObject<TesFiltrosRegistroBancoDto>(filtros) ?? new TesFiltrosRegistroBancoDto();

            try
            {

                string query = "spTes_Bancos_Mov_Consulta";

                string fechaIni = MProGrXAuxiliarDB.validaFechaGlobal(filtro.fechaInicio, "yyyy-MM-dd" + " 00:00:00");
                string fechaFin = MProGrXAuxiliarDB.validaFechaGlobal(filtro.fechaCorte, "yyyy-MM-dd" + " 23:59:59");

                var parameters = new
                {
                    BancoId = filtro.cod_cuenta,
                    Documento = filtro.ndocumento,
                    Tipo = filtro.tipoMovimiento,
                    FechaTipo = filtro.base_,
                    FInicio = fechaIni,
                    FCorte = fechaFin,
                    MntInicio = filtro.montoInicio,
                    MntCorte = filtro.montoCorte,
                    Estado = filtro.estado,
                    Descripcion = filtro.descripcion
                };

                var response = conn
                    .Query<TeslistaRegistroBancosDto>(query, parameters, commandType: CommandType.StoredProcedure)
                    .ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TeslistaRegistroBancosDto>>(ex.Message);
            }
        }


        /// <summary>
        /// Aplica el registro de bancos cargados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="registroLista"></param>
        /// <returns></returns>
        public async Task<ErrorDto> TES_RegistrosBancosCargados_Aplicar(int CodEmpresa, string registroLista)
        {
            List<RegistroBancoDto> lista = JsonConvert.DeserializeObject<List<RegistroBancoDto>>(registroLista) ?? new List<RegistroBancoDto>();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                string error = string.Empty;
                foreach (var item in lista)
                {

                    var parametros = new
                    {
                        LineaId = item.Linea_Id,
                        Usuario = item.Usuario,
                        AutoId = item.Auto_Id,
                        Concepto = item.Concepto,
                        Unidad = item.Unidad,
                        Centro = item.Centro,
                        Cuenta = item.Cuenta
                    };

                    var result = await conn.QueryFirstOrDefaultAsync<dynamic>(
                                    "spTes_Bancos_Mov_Registro",
                                    parametros,
                                    commandType: CommandType.StoredProcedure);

                    if(result.Ok == 0)
                    {
                        error = " - Linea: " + result.LineaId + " error: " +result.Mensaje;
                    }

                }

                if (!string.IsNullOrEmpty(error))
                {
                    return DbHelper.ErrorResponse(error);
                }

                return DbHelper.OkResponse("Registros procesados correctamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            
        }

        public ErrorDto TES_RegistrosBancosCargados_Elimina(int CodEmpresa, string registroLista)
        {
            List<RegistroBancoDto> lista = JsonConvert.DeserializeObject<List<RegistroBancoDto>>(registroLista) ?? new List<RegistroBancoDto>();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                foreach (var item in lista)
                {
                    var querySP = "spTes_Bancos_Mov_Elimina";
                    conn.Execute(querySP, new
                    {
                        LineaId = item.Linea_Id
                    },
                    commandType: CommandType.StoredProcedure);
                }
                return DbHelper.OkResponse("Registro procesado correctamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            
        }
    }
}