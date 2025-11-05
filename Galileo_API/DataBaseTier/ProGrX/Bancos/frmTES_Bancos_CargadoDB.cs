using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_Bancos_CargadoDB
    {
        private readonly IConfiguration? _config;
        private mSecurityMainDb DBBitacora;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmTES_Bancos_CargadoDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(_config);
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Obtiene la cuenta de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaBancosCargados>> Tes_Bancos_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaBancosCargados>>
            {
                Code = 0,
                Result = new List<DropDownListaBancosCargados>()
            };
            try
            {
                var querySP = "";
                using var connection = new SqlConnection(stringConn);
                {
                    querySP = "exec spTes_Cuenta_Bancaria_Acceso @usuario, @TipoDoc, @Acceso";
                    connection.Execute(querySP, new { @Usuario = usuario, @TipoDoc = "DP", @Acceso = "SOL" });
                    response.Result = connection.Query<DropDownListaBancosCargados>(querySP, new { Usuario = usuario, TipoDoc = "DP", Acceso = "SOL" }).ToList();
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
        /// M�todo para obtener los conceptos 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<TesBancoCargadoConceptos>> Tes_BancosCargadoConceptos_Obtener(int CodEmpresa, string concepto = null)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TesBancoCargadoConceptos>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<TesBancoCargadoConceptos>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string where = "";
                    if (concepto != null)
                    {
                        where = $" AND COD_CONCEPTO = '{concepto}'";
                    }

                    var query = $"select COD_CONCEPTO, DESCRIPCION, COD_CUENTA_MASK, DP_TRAMITE_APL, CUENTA_DESC from vTes_Conceptos WHERE AUTO_REGISTRO = 1 AND ESTADO = 'A' {where}";
                    response.Result = connection.Query<TesBancoCargadoConceptos>(query).ToList();

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
        /// M�todo para obtener las unidades asociadas 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Tes_BancosCargadoCentroUnidades_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select COD_UNIDAD as 'item', DESCRIPCION from vCNTX_UNIDADES_LOCAL";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// M�todo para obtener los centros de costos 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Tes_BancosCargadoCentroCostos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select COD_CENTRO_COSTO as 'item', DESCRIPCION from vCNTX_CENTRO_COSTO_LOCAL";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }


        /// <summary>
        /// M�todo para obtener una lista de registros de auto registro de tesorer�a con paginaci�n y filtros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<TesAuto_RegistroLista> Tes_AutoRegistroLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<TesAuto_RegistroLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new TesAuto_RegistroLista()
                {
                    total = 0,
                    lista = new List<TesAuto_RegistroDTO>()
                }
            };
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = $"select Count(*) from vTES_AUTO_REGISTRO ";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE id_auto LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR palabras_clave LIKE '%" + filtros.filtro + "%' ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select * from vTES_AUTO_REGISTRO 
                                         {filtros.filtro} 
                                       ORDER BY id_auto
                                        {paginaActual}
                                        {paginacionActual} ";
                    result.Result.lista = connection.Query<TesAuto_RegistroDTO>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
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
        public ErrorDTO TES_BancosCargados_Aplicar(int CodEmpresa, string cod_banco, string usuario, List<TesCargadoExcelDTO> file)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "OK"
            };

            try
            {

                ;

                using var connection = new SqlConnection(stringConn);
                {

                    foreach (var row in file)
                    {

                        var query = @"EXEC spTes_Bancos_Mov_Load @IdBanco, @Fecha, @Documento, @TipoMov, @Importe,@Descripcion";

                        connection.Execute(query, new
                        {
                            IdBanco = cod_banco,
                            Fecha = row.fecha,
                            Documento = row.documento,
                            TipoMov = row.tipo,
                            Importe = row.importe,
                            Descripcion = row.descripcion,
                        });
                    }

                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene los registros de bancos cargados pendientes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<TES_listaRegistroBancosDTO>> TES_ListaRegistroBancos_Obtener(int CodEmpresa, string filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            TES_FiltrosRegistroBancoDTO filtro = JsonConvert.DeserializeObject<TES_FiltrosRegistroBancoDTO>(filtros) ?? new TES_FiltrosRegistroBancoDTO();

            var response = new ErrorDTO<List<TES_listaRegistroBancosDTO>>
            {
                Code = 0,
                Result = new List<TES_listaRegistroBancosDTO>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                string query = "spTes_Bancos_Mov_Consulta";

                var parameters = new
                {
                    BancoId = filtro.cod_cuenta,
                    Documento = filtro.ndocumento,
                    Tipo = filtro.tipoMovimiento,
                    FechaTipo = filtro.base_,
                    FInicio = filtro.fechaInicio,
                    FCorte = filtro.fechaCorte,
                    MntInicio = filtro.montoInicio,
                    MntCorte = filtro.montoCorte,
                    Estado = filtro.estado,
                    Descripcion = filtro.descripcion
                };

                response.Result = connection
                    .Query<TES_listaRegistroBancosDTO>(query, parameters, commandType: CommandType.StoredProcedure)
                    .ToList();


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
        /// Aplica el registro de bancos cargados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="registroLista"></param>
        /// <returns></returns>
        public ErrorDTO TES_RegistrosBancosCargados_Aplicar(int CodEmpresa, string registroLista)
        {
            List<RegistroBancoDTO> lista = JsonConvert.DeserializeObject<List<RegistroBancoDTO>>(registroLista) ?? new List<RegistroBancoDTO>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0
            };

            try
            {
                var querySP = "";
                using var connection = new SqlConnection(stringConn);
                {

                    foreach (var item in lista)
                    {
                        querySP = "exec spTes_Bancos_Mov_Registro @LineaId, @Usuario, @AutoId, @Concepto, @Unidad, @Centro, @Cuenta";
                        connection.Execute(querySP, new
                        {
                            LineaId = item.Linea_Id,
                            Usuario = item.Usuario,
                            AutoId = item.Auto_Id,
                            Concepto = item.Concepto,
                            Unidad = item.Unidad,
                            Centro = item.Centro,
                            Cuenta = item.Cuenta
                        });
                    }

                    response.Description = "Registro procesado correctamente!";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }
    }
}