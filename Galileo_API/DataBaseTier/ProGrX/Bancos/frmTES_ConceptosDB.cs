using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using PgxAPI.Models;
using Microsoft.Data.SqlClient;
using Dapper;
using PgxAPI.Models.CxP;
using PgxAPI.BusinessLogic;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_ConceptosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 9; // Modulo de Tesorería
        private readonly mCntLinkDB _mCnt;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmTES_ConceptosDB(IConfiguration? config)
        {
            _config = config;
            _mCnt = new mCntLinkDB(_config);
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de conceptos de tesorería con paginacion y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesConceptosLista> Tes_ConceptosLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<TesConceptosLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new TesConceptosLista()
                {
                    total = 0,
                    lista = new List<TesConceptosData>()
                }
            };
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(cod_concepto) from vTes_conceptos";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_concepto LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR cod_cuenta_Mask LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    if(filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_concepto";
                    }

                   

                    query = $@"select cod_concepto,descripcion,activo,cod_cuenta_Mask, AUTO_REGISTRO, DP_TRAMITE_APL 
                                            from vTes_conceptos  
                                        {filtros.filtro} 
                                     ORDER BY {filtros.sortField} {(filtros.sortOrder == 1 ? "DESC": "ASC")}
                                        {paginaActual}
                                        {paginacionActual} ";
                    result.Result.lista = connection.Query<TesConceptosData>(query).ToList();
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
        /// Guarda un concepto de tesorería, ya sea insertando o actualizando según corresponda.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        public ErrorDto Tes_Conceptos_Guardar(int CodEmpresa, string usuario ,TesConceptosData concepto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select isnull(count(*),0) as Existe from tes_conceptos 
                                 where cod_concepto = @concepto";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { concepto = concepto.cod_concepto });

                    string vCuenta = _mCnt.fxgCntCuentaFormato(CodEmpresa, false, concepto.cod_cuenta_mask, 0);
                    bool cuentaValida = _mCnt.fxgCntCuentaValida(CodEmpresa, vCuenta);
                    if (!cuentaValida)
                    {
                        result.Code = -1;
                        result.Description = "La cuenta contable no es válida.";
                        return result;
                    }

                    if (existe > 0)
                    {
                        result = Tes_Conceptos_Actualizar(CodEmpresa, usuario, vCuenta, concepto);
                    }
                    else
                    {
                        result = Tes_Conceptos_Insertar(CodEmpresa, usuario, vCuenta ,concepto);
                    }

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Inserta un nuevo concepto de tesorería en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cuenta"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        private ErrorDto Tes_Conceptos_Insertar(int CodEmpresa, string usuario, string cuenta ,TesConceptosData concepto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query = @"
                            INSERT INTO tes_conceptos (
                                cod_concepto,descripcion,estado,cod_cuenta,
                                AUTO_REGISTRO, DP_TRAMITE_APL, REGISTRO_FECHA, REGISTRO_USUARIO
                            )
                            VALUES (
                                @cod_concepto, @descripcion, @estado, @cod_cuenta,
                                @auto_registro, @dp_tramite_apl,
                                dbo.myGetdate(), @usuario
                            )";

                    connection.Execute(query, new 
                    {
                        cod_concepto = concepto.cod_concepto,
                        descripcion = concepto.descripcion,
                        estado = concepto.activo ? 'A' : 'I',
                        cod_cuenta = cuenta,
                        auto_registro = concepto.auto_registro ? 1 : 0,
                        dp_tramite_apl = concepto.dp_tramite_apl ? 1 : 0,
                        usuario = usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Concepto Desembolso: {concepto.cod_concepto} - {concepto.descripcion}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Actualiza un concepto de tesorería existente en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cuenta"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        private ErrorDto Tes_Conceptos_Actualizar(int CodEmpresa, string usuario,string cuenta, TesConceptosData concepto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query = @"
                            UPDATE tes_conceptos
                            SET 
                                descripcion = @descripcion,
                                estado = @estado,
                                cod_cuenta = @cod_cuenta,
                                AUTO_REGISTRO = @auto_registro,
                                DP_TRAMITE_APL = @dp_tramite_apl,
                                MODIFICA_FECHA = dbo.myGetdate(),
                                MODIFICA_USUARIO = @usuario
                            WHERE cod_concepto = @cod_concepto";

                    connection.Execute(query, new
                    {
                        cod_concepto = concepto.cod_concepto,
                        descripcion = concepto.descripcion,
                        estado = (concepto.activo == true) ? 'A' : 'I',
                        cod_cuenta = cuenta,
                        auto_registro = concepto.auto_registro ? 1 : 0,
                        dp_tramite_apl = concepto.dp_tramite_apl ? 1 : 0,
                        usuario = usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Concepto Desembolso: {concepto.cod_concepto} - {concepto.descripcion}",
                        Movimiento = "MODIFICA - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Elimina un concepto de tesorería por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Tes_Conceptos_Eliminar(int CodEmpresa, string tipo ,string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query = $@"DELETE FROM tes_conceptos 
                                      WHERE cod_concepto = @cod_concepto";
                    connection.Execute(query, new { cod_concepto = tipo });
                    _Security_MainDB.Bitacora(
                        new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = "Concepto Desembolso: " + tipo,
                            Movimiento = "ELIMINAR - WEB",
                            Modulo = vModulo
                        });
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Método para buscar conceptos para exportar por excel y pdf.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<TesConceptosData>> Tes_Conceptos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<TesConceptosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<TesConceptosData>()
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_concepto LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR cod_cuenta_Mask LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    query = $@"select cod_concepto,descripcion,activo,cod_cuenta_Mask, AUTO_REGISTRO, DP_TRAMITE_APL 
                                            from vTes_conceptos  
                                        {filtros.filtro} 
                                     ORDER BY cod_concepto ";
                    result.Result = connection.Query<TesConceptosData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<TesConceptosData>();
            }
            return result;
        }

        /// <summary>
        /// Valida si un concepto de tesorería existe en la base de datos.
        /// Valor -1 para error
        /// Valor  1 para existe
        /// Valor  0 para no existe
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto Tes_Concepto_Valida(int CodEmpresa, string codigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query = $@"SELECT COUNT('X') FROM tes_conceptos 
                                      WHERE  UPPER(COD_CONCEPTO) = '{codigo.ToUpper()}' ";
                    var existe = connection.QueryFirstOrDefault<int>(query);

                    if (existe > 0)
                    {
                        result.Code = 1; // Existe
                        result.Description = "El concepto de tesorería ya existe.";
                    }
                    else
                    {
                        result.Code = 0; // No existe
                        result.Description = "El concepto de tesorería no existe.";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
    }
}
