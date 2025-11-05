using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using System.Runtime.CompilerServices;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_Auto_RegistroDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 9; // Modulo de Tesorería

        public frmTES_Auto_RegistroDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Método para consultar un registro de auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="autoReg"></param> 
        /// <returns></returns>
        public ErrorDTO<TesAuto_RegistroDTO> Tes_AutoRegistro_Consultar(int CodEmpresa, int autoReg)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TesAuto_RegistroDTO>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new TesAuto_RegistroDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from vTES_AUTO_REGISTRO where ID_Auto = {autoReg} ";
                    response.Result = connection.QueryFirstOrDefault<TesAuto_RegistroDTO>(query);
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
        /// Método para guardar un registro de auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="registro"></param>
        /// <returns></returns>
        public ErrorDTO Tes_AutoRegistro_Guardar(int CodEmpresa, TesAuto_RegistroDTO registro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Registro guardado correctamente"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int cargaDiarias = (registro.apl_carga_diaria == true) ? 1 : 0;
                    int conciliacion = (registro.apl_conciliacion == true) ? 1 : 0;
                    int ignoraRegistro = (registro.ignora_registro == true) ? 1 : 0;
                    int filtraCtasBancos = (registro.filtra_cta_bancos == true) ? 1 : 0;
                    int ind_Referencia = (registro.ind_info_persona == true) ? 1 : 0;
                    int activo = 0;
                    if (registro.id_auto == 0)
                    {
                        activo = 1;
                    }
                    else
                    {
                        activo = (registro.activo == true) ? 1 : 0;
                    }

                    string usuario = (registro.id_auto == 0)? registro.registro_usuario : registro.modifica_usuario;
                    string cuenta = (registro.cod_cuenta_mask == null)? "0": registro.cod_cuenta_mask.Replace("-", "");

                    var query = $@"exec spTes_Auto_Registro_Add 
                                            {registro.id_auto}, 
                                            '{registro.descripcion}',
                                            '{registro.palabras_clave}', 
                                            '{registro.detalle}', 
                                            '{registro.cod_concepto}', 
                                             '{cuenta}', 
                                             '{registro.cod_unidad}', 
                                             '{registro.cod_centro_costo}', 
                                             {((registro.mnt_inicio==null) ? 0: registro.mnt_inicio) },
                                             {((registro.mnt_corte == null) ? 0 : registro.mnt_corte)}, 
                                             {cargaDiarias},
                                             {conciliacion},
                                             {ind_Referencia},
                                             {((registro.tipo_beneficiario == null) ? 0 : registro.tipo_beneficiario)},
                                             '{registro.beneficiario_id}',
                                             '{registro.beneficiario_nombre}',
                                             {activo},
                                             '{usuario}',
                                             'A',
                                             '{registro.apl_tipo_mov}',
                                             '{registro.tipo_doc}',
                                             {ignoraRegistro},
                                             {filtraCtasBancos} ";

                    AutoRegGuardar excecuted = connection.Query<AutoRegGuardar>(query).FirstOrDefault();
                    response.Description = excecuted.auto_id.ToString();
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
        /// Método para eliminar un registro de auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="autoReg"></param>
        /// <returns></returns>
        public ErrorDTO Tes_AutoRegistro_Eliminar(int CodEmpresa, TesAuto_RegistroDTO registro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Registro eliminado correctamente"
            };
            try
            {
                if(registro == null)
                {
                    response.Code = -1;
                    response.Description = "Debe especificar un registro para eliminar";
                    return response;
                }

                    using var connection = new SqlConnection(stringConn);
                    {
                        int cargaDiarias = (registro.apl_carga_diaria == true) ? 1 : 0;
                        int conciliacion = (registro.apl_conciliacion == true) ? 1 : 0;
                        int ignoraRegistro = (registro.ignora_registro == true) ? 1 : 0;
                        int filtraCtasBancos = (registro.filtra_cta_bancos == true) ? 1 : 0;
                        int ind_Referencia = (registro.ind_info_persona == true) ? 1 : 0;
                        int activo = (registro.activo == true) ? 1 : 0;

                        string usuario = registro.modifica_usuario ?? registro.registro_usuario;

                        var query = $@"exec spTes_Auto_Registro_Add 
                                            {registro.id_auto}, 
                                            '{registro.descripcion}',
                                            '{registro.palabras_clave}', 
                                            '{registro.detalle}', 
                                            '{registro.cod_concepto}', 
                                             '{registro.cod_cuenta}', 
                                             '{registro.cod_unidad}', 
                                             '{registro.cod_centro_costo}', 
                                             {registro.mnt_inicio},
                                             {registro.mnt_corte}, 
                                             {cargaDiarias},
                                             {conciliacion},
                                             {ind_Referencia},
                                             {registro.tipo_beneficiario},
                                             '{registro.beneficiario_id}',
                                             '{registro.beneficiario_nombre}',
                                             {activo},
                                             '{usuario}',
                                             'E',
                                             '{registro.apl_tipo_mov}',
                                             '{registro.tipo_doc}',
                                             {ignoraRegistro},
                                             {filtraCtasBancos} ";

                        AutoRegGuardar excecuted = connection.Query<AutoRegGuardar>(query).FirstOrDefault();
                        response.Description = excecuted.auto_id.ToString();
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
        /// Método para obtener las cuentas bancarias asociadas a un auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="FiltraCtas"></param>
        /// <returns></returns>
        public ErrorDTO<List<TesAutoRegCtaBancariasData>> Tes_AutoRegistroCtaBancos_Obtener(int CodEmpresa, int? codigo, string? FiltraCtas)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TesAutoRegCtaBancariasData>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<TesAutoRegCtaBancariasData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string valCodigo = (codigo == 0) ? "NULL" : codigo.ToString();
                    var query = $@"exec spTes_Auto_Registro_Ctas {valCodigo}, '{FiltraCtas}'";
                    response.Result = connection.Query<TesAutoRegCtaBancariasData>(query).ToList();
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
        /// Método para asignar o des asignar una cuenta bancaria a un auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CtaBanco"></param>
        /// <returns></returns>
        public ErrorDTO Tes_AutoRegistroCtaBancos_Asignar(int CodEmpresa, int codigo , int cta ,bool asignado, string usuario )
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string asignadoStr = (asignado == true) ? "A" : "E";

                    var query = $@"exec spTes_Auto_Registro_Ctas_Add {codigo}, {cta} ,'{asignadoStr}', '{usuario}'";
                    connection.Execute(query);
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
        /// Método para obtener los tipos de auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Tes_AutoRegistroTipos_Obtener(int CodEmpresa, int? tipo, string? filtro)
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
                    var query = "";
                    switch (tipo)
                    {
                        case 1: //Personas
                            query = $@"Select Cedula as 'item', Nombre as 'descripcion' from Socios Where Cedula like '%{filtro}%'";
                            break;
                        case 2: //Bancos
                            query = $@"select ID_BANCO as 'item',descripcion from TES_BANCOS where ID_BANCO like '%{filtro}%'";
                            break;
                        case 3: //Proveedores
                            query = $@"Select CEDJUR as 'item', DESCRIPCION  from CXP_PROVEEDORES where CEDJUR like '%{filtro}%'";
                            break;
                        case 4: //Cuentas por Cobrar
                            query = $@"select Cod_Acreedor as 'item', DESCRIPCION  from CRD_APA_ACREEDORES where cod_acreedor like '%{filtro}%'";
                            break;
                        case 5: //Empleados
                            query = $@"Select IDENTIFICACION as 'item', NOMBRE_COMPLETO as 'descripcion' from RH_PERSONAS Where IDENTIFICACION like '%{filtro}%'";
                            break;
                        case 6: //Directos
                            query = $@"Select CODIGO as 'item', BENEFICIARIO as 'descripcion' from vTes_Beneficiarios Where CODIGO like '%{filtro}%'";
                            break;
                    }

                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();

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
        /// Método para obtener los centros de costos asociados a un auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Tes_AutoRegistroCentroCostos_Obtener(int CodEmpresa)
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
        /// Método para obtener los códigos y descripciones de conceptos, unidades o centros de costos asociados a un auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Tes_AutoRegistroCodigoDesc_Obtener(int CodEmpresa, string tipo, string codigo)
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
                    var query = "";
                    switch (tipo)
                    {
                        case "Cta":
                        case "Con":
                            query = $@"select cod_concepto as 'item', DESCRIPCION as 'descripcion' , from vTes_Conceptos Where cod_concepto = '{codigo}'";
                            break;
                        case "Ud":
                            query = $@"select cod_Unidad as 'item', DESCRIPCION as 'descripcion' from vCNTX_UNIDADES_LOCAL Where cod_Unidad = '{codigo}'";
                            break;
                        case "Cc":
                            query = $@"select cod_Centro_Costo as 'item', DESCRIPCION as 'descripcion' from vCNTX_CENTRO_COSTO_LOCAL Where cod_Centro_Costo = '{codigo}'";
                            break;
                    }

                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();

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
        /// Método para obtener los conceptos de auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<TesAutoregistroConceptos>> Tes_AutoRegistroConceptos_Obtener(int CodEmpresa, string concepto = null)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TesAutoregistroConceptos>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<TesAutoregistroConceptos>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string where = "";
                    if(concepto != null)
                    {
                        where = $" AND COD_CONCEPTO = '{concepto}'";
                    }

                    var query = $"select COD_CONCEPTO, DESCRIPCION, COD_CUENTA_MASK, DP_TRAMITE_APL, CUENTA_DESC from vTes_Conceptos WHERE AUTO_REGISTRO = 1 AND ESTADO = 'A' {where}";
                    response.Result = connection.Query<TesAutoregistroConceptos>(query).ToList();

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
        /// Método para obtener las unidades asociadas a un auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Tes_AutoRegistroCentroUnidades_Obtener(int CodEmpresa)
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
        /// Método para obtener los tipos de documentos asociados a un auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="TipoMov"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Tes_AutoRegistroTiposDoc_Obtener(int CodEmpresa, string TipoMov)
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
                    var query = $@"exec spTes_Tipos_Docs '{TipoMov}' ";
                    var datos = connection.Query<TipoMovData>(query).ToList();
                    if (datos != null)
                    {
                        foreach (var item in datos)
                        {
                            string idx = item.tipo;
                            string itmx = item.descripcion;

                            response.Result.Add(new DropDownListaGenericaModel { item = idx, descripcion = itmx });
                        }
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
        /// Método para obtener una lista de registros de auto registro de tesorería con paginación y filtros
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
        /// Método para obtener un registro de auto registro de tesorería por ID mediente scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="autoReg"></param>
        /// <returns></returns>
        public ErrorDTO<TesAuto_RegistroDTO> Tes_AutoRegistro_scroll(int CodEmpresa, int autoReg, int? scroll)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TesAuto_RegistroDTO>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new TesAuto_RegistroDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string where = "";
                    scroll = (scroll == null) ? 0 : scroll;
                    if (scroll == 1) //busca el registro anterior
                    {
                        if(autoReg == 0 || autoReg == null)
                        {
                            autoReg = 99999999;
                        }

                        where = $" WHERE ID_Auto < {autoReg} ORDER BY ID_Auto DESC ";
                    }
                    else if (scroll == 2) //busca el registro siguiente
                    {
                        if (autoReg == 0 || autoReg == null)
                        {
                            autoReg = 0;
                        }

                        where = $" WHERE ID_Auto > {autoReg} ORDER BY ID_Auto ASC ";
                    }

                        var query = $@"select top 1 * from vTES_AUTO_REGISTRO {where} ";
                    response.Result = connection.QueryFirstOrDefault<TesAuto_RegistroDTO>(query);
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

    }
}
