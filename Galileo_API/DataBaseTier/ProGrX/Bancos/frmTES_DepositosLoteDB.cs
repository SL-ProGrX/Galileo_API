using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.TES;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_DepositosLoteDB
    {
        private readonly IConfiguration? _config;
        private mCntLinkDB mCntLink;
        private mTesoreria mTesoreria;

        public frmTES_DepositosLoteDB(IConfiguration config)
        {
            _config = config;
            mCntLink = new mCntLinkDB(_config);
            mTesoreria = new mTesoreria(_config);
        }

        /// <summary>
        /// Obtener lista de cuenta bancarias que tiene acceso el usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO<List<TES_Cuenta_BancariaDTO>> TES_DepositosLote_Ctas_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TES_Cuenta_BancariaDTO>>
            {
                Code = 0,
                Result = new List<TES_Cuenta_BancariaDTO>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"exec spTes_Cuenta_Bancaria_Acceso @usuario,'DP','SOL'";
                    response.Result = connection.Query<TES_Cuenta_BancariaDTO>(query,
                        new { usuario = usuario }).ToList();
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
        /// Cargar archivo de depositos lote
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="archivoData"></param>
        /// <returns></returns>
        public ErrorDTO<List<TES_Depositos_TramiteDTO>> TES_DepositosLote_ArchivoCarga(int CodEmpresa, string archivoData)
        {
            List<TES_Depositos_TramiteDTO> lista = JsonConvert.DeserializeObject <List<TES_Depositos_TramiteDTO>>(archivoData) ?? new List<TES_Depositos_TramiteDTO>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TES_Depositos_TramiteDTO>>
            {
                Code = 0,
                Result = new List<TES_Depositos_TramiteDTO>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var item in lista)
                    {
                        var query = @"select dbo.fxTes_DP_Cargado(@banco,@documento,'',@monto) as Existe";
                        var vExiste = connection.QueryFirstOrDefault<int>(query,
                            new
                            {
                                banco = item.id_banco,
                                documento = item.documento,
                                monto = item.monto
                            });

                        string vInconsistencia = "";

                        switch (vExiste)
                        {
                            case 0: // Sin Inconsistencia
                                vInconsistencia = "";
                                break;
                            case 1: // Existe / Identificado
                                vInconsistencia = "Existe  / Identificado";
                                break;
                            case 2: // Existe / No Identificado
                                vInconsistencia = "Existe  / No Identificado";
                                break;
                            case 3: // Existe Registro pero a nombre de otra persona
                                vInconsistencia = "Existe Registro pero a nombre de otra persona";
                                break;
                            case 4: // Existe Registro con Monto Diferente
                                vInconsistencia = "Existe Registro con Monto Diferente";
                                break;
                        }

                        item.inconsistencia = vInconsistencia;
                        item.existe = (vExiste > 0) ? true : false;
                    }

                    response.Result = lista;
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
        /// Procesar depositos lote del archivo cargado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cuenta"></param>
        /// <param name="usuario"></param>
        /// <param name="archivoData"></param>
        /// <returns></returns>
        public ErrorDTO TES_DepositosLote_Procesar(int CodEmpresa, string cuenta, string usuario, string archivoData)
        {
            List<TES_Depositos_TramiteDTO> lista = JsonConvert.DeserializeObject<List<TES_Depositos_TramiteDTO>>(archivoData) ?? new List<TES_Depositos_TramiteDTO>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };
            var query = "";
            int vCasos = 0;
            bool vInconsistencia = false;
            try
            {
                string vCuenta = mCntLink.fxgCntCuentaFormato(CodEmpresa, false, cuenta, 0);
                bool ctaValida = mCntLink.fxgCntCuentaValida(CodEmpresa, vCuenta);
                if (!ctaValida)
                {
                    response.Code = -1;
                    response.Description = "La cuenta especificada para registro no es v&aacute;lida...verifique!";
                    return response;
                }
                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var item in lista)
                    {
                        if (item.existe.HasValue && !item.existe.Value)
                        {
                            query = @"insert TES_DEPOSITOS_TRAMITE 
                            (id_Banco,documento,nsolicitud,fecha,monto,descripcion,registro_fecha,registro_usuario,id_requerida,identificado, cod_cuenta) 
                            values(@banco, @documento, 0, @fecha, @monto, @descripcion, dbo.MyGetdate(), @usuario, @requiereId, 0, @cuenta)";
                            vCasos = vCasos + 1;
                        }
                        else if (item.existe.HasValue && item.existe.Value)
                        {
                            query = @"insert TES_DEPOSITOS_TRAMITE_INCONSISTENCIAS
                            (id_Banco,documento,fecha,monto,descripcion,registro_fecha,registro_usuario,inconsistencia)
                            values(@banco, @documento, @fecha, @monto, @descripcion, dbo.MyGetdate(), @usuario, @inconsistencia)";
                            vInconsistencia = true;
                        }
                        connection.Execute(query, 
                            new { 
                                banco = item.id_banco,
                                documento = item.documento,
                                fecha = item.fecha,
                                monto = item.monto,
                                descripcion = item.descripcion,
                                inconsistencia = item.inconsistencia,
                                requiereId = item.requiere_identificacion,
                                usuario = usuario,
                                cuenta = vCuenta
                            });

                        if (vCasos == 0)
                        {
                            response.Description = "No se procesaron casos *--Revisados--* para el control de dep&oacute;sitos!";
                        }
                        else
                        {
                            response.Description = "Carga realizada Satisfactoriamente... Registros Procesados: "+vCasos;
                        }

                        if (vInconsistencia)
                        {
                            response.Description += "\nSe presentaron inconsistencias en la carga..Revise en el TAB de consulta de inconsistencias!";
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
        /// Obtener lista de inconsistencias
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <param name="fecha_inicio"></param>
        /// <param name="fecha_corte"></param>
        /// <returns></returns>
        public ErrorDTO<TablasListaGenericaModel> TES_DepositosLote_Inconsistencias_Obtener(int CodEmpresa, string filtros)
        {
            Filtros_Inconsistencias param = JsonConvert.DeserializeObject<Filtros_Inconsistencias>(filtros) ?? new Filtros_Inconsistencias();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TablasListaGenericaModel>
            {
                Code = 0,
                Result = new TablasListaGenericaModel()
            };
            string vFiltro = "";
            try
            {
                var fechaInicio = param.fecha_inicio.Date;
                var fechaCorte = param.fecha_corte.Date.AddDays(1).AddTicks(-1);
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = @"select COUNT(Tra.Documento) From TES_DEPOSITOS_TRAMITE_INCONSISTENCIAS Tra 
                        inner join Tes_Bancos Bn on Tra.id_banco = Bn.id_Banco
                        Where Tra.Fecha between @fechaInicio and @fechaCorte and Tra.Id_Banco = @banco";

                    var query = @"select Tra.Documento, Tra.Monto, Tra.Fecha, Tra.Descripcion, Tra.Inconsistencia, Tra.Registro_Fecha, Tra.Registro_Usuario, Bn.Descripcion as 'Banco'
                        From TES_DEPOSITOS_TRAMITE_INCONSISTENCIAS Tra inner join Tes_Bancos Bn on Tra.id_banco = Bn.id_Banco
                        Where Tra.Fecha between @fechaInicio and @fechaCorte and Tra.Id_Banco = @banco";

                    if (!string.IsNullOrEmpty(param.filtro))
                    {
                        vFiltro = $@" and (Tra.Documento like @filtro OR Tra.Descripcion like @filtro
                            OR Tra.Inconsistencia like @filtro OR Bn.Descripcion like @filtro)  ";
                    }

                    query += vFiltro + @$" ORDER BY Tra.Documento 
                                    OFFSET {param.pagina} ROWS 
                                    FETCH NEXT {param.paginacion} ROWS ONLY";

                    var parametros = new
                    {
                        banco = param.banco,
                        fechaInicio = fechaInicio,
                        fechaCorte = fechaCorte,
                        filtro = $"%{param.filtro}%"
                    };
                    response.Result.total = connection.QueryFirstOrDefault<int>(queryT, parametros);
                    response.Result.lista = connection.Query<TES_Depositos_Tramite_InconsistenciasDTO>(query, parametros).ToList();
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
        /// Obtener lista de depositos tramite
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<TablasListaGenericaModel> TES_DepositosLote_Registro_Obtener(int CodEmpresa, string filtros)
        {
            Filtros_Registro param = JsonConvert.DeserializeObject<Filtros_Registro>(filtros) ?? new Filtros_Registro();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "",
                Result = new TablasListaGenericaModel()
            };
            try
            {
                var where = "";
                var FechaInicio = param.fecha_inicio.Date;
                var FechaCorte = param.fecha_corte.Date.AddDays(1).AddTicks(-1);
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = @"select count(Tra.dp_tramite_id) from TES_DEPOSITOS_TRAMITE Tra 
                    inner join Tes_Bancos Bn on Tra.id_banco = Bn.id_Banco ";

                    var query = @"select Tra.*, Bn.Descripcion as 'BancoDesc'
                    From TES_DEPOSITOS_TRAMITE Tra inner join Tes_Bancos Bn on Tra.id_banco = Bn.id_Banco";

                    switch (param.cboFechas)
                    {
                        case "D":
                            where += " Where Tra.Fecha between @fechaInicio and @fechaCorte ";
                            break;
                        case "I":
                            where += " Where Tra.Identifica_Fecha between @fechaInicio and @fechaCorte ";
                            break;
                        case "R":
                            where += " Where Tra.Tes_Aplicado_Fecha between @fechaInicio and @fechaCorte ";
                            break;
                    }

                    if (!string.IsNullOrEmpty(param.numDoc))
                    {
                        where += " and Tra.Documento like @numDoc ";
                    }

                    switch (param.cboFiltro)
                    {
                        case 1:
                            where += " and Tra.Identificado = 1 and Tra.Tes_Aplicado = 0";
                            break;
                        case 2:
                            where += " and Tra.Identificado = 1 and Tra.Tes_Aplicado = 1";
                            break;
                        case 3:
                            where += " and Tra.Identificado = 0 and Tra.Tes_Aplicado = 1";
                            break;
                        case 4:
                            where += " and Tra.Identificado = 0 and Tra.Tes_Aplicado = 0";
                            break;
                        default:
                            break;
                    }

                    if (!string.IsNullOrEmpty(param.filtro))
                    {
                        where += $@" and (Tra.descripcion like @filtro OR Tra.dp_tramite_id like @filtro
                            OR Tra.documento like @filtro OR Tra.cliente_nombre like @filtro)  ";
                    }

                    queryT += where + " and Tra.Id_Banco = @banco ";
                    query += where + @$" and Tra.Id_Banco = @banco 
                                    ORDER BY Tra.dp_tramite_id 
                                    OFFSET {param.pagina} ROWS 
                                    FETCH NEXT {param.paginacion} ROWS ONLY";

                    var parametros = new
                    {
                        banco = param.banco,
                        numDoc = $"%{param.numDoc}%",
                        fechaInicio = FechaInicio,
                        fechaCorte = FechaCorte,
                        filtro = $"%{param.filtro}%"
                    };
                    response.Result.total = connection.QueryFirstOrDefault<int>(queryT, parametros);
                    response.Result.lista = connection.Query<TES_Depositos_Tramite_BancoDTO>(query, parametros).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener un número de cuenta bancario mediante la categoría seleccionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Categoria"></param>
        /// <returns></returns>
        public ErrorDTO<string> TES_DepositosLote_CategoriaCta_Obtener(int CodEmpresa, string Categoria)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<string>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };
            try
            {
                switch (Categoria)
                {
                    case "01": //Depositos en Cajas
                        response.Result = mTesoreria.fxTesParametro(CodEmpresa, "05");
                        break;
                    case "02": //Depositos sin Identificar
                        response.Result = mTesoreria.fxTesParametro(CodEmpresa, "06");
                        break;
                    case "03": //Depositos Otros..
                        response.Result = mTesoreria.fxTesParametro(CodEmpresa, "07");
                        break;
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
        /// Aplicar registro de depositos seleccionados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Datos"></param>
        /// <returns></returns>
        public ErrorDTO TES_DepositosLote_Registro_Aplicar(int CodEmpresa, string Usuario, string Datos)
        {
            List<TES_Depositos_Tramite_BancoDTO> lista = JsonConvert.DeserializeObject<List<TES_Depositos_Tramite_BancoDTO>>(Datos) ?? new List<TES_Depositos_Tramite_BancoDTO>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                string vRemesa = mTesoreria.fxTesParametro(CodEmpresa, "08");
                vRemesa = vRemesa + 1;
                using var connection = new SqlConnection(stringConn);
                {
                    var queryParam = "update tes_parametros set valor = @remesa where cod_parametro = '08'";
                    connection.Execute(queryParam, new { remesa = vRemesa });

                    foreach (var item in lista)
                    {
                        var query = "exec spTES_Deposito_Lote_Registra @banco, @documento, @usuario, @remesa";
                        connection.Execute(query,
                            new
                            {
                                banco = item.id_banco,
                                documento = item.documento,
                                usuario = Usuario,
                                remesa = vRemesa
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
        /// Actualizar depositos lote
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO TES_DepositosLote_Registro_Actualizar(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spTES_Deposito_Lote_Actualiza";
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
        /// Desvincular los depositos seleccionados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Datos"></param>
        /// <returns></returns>
        public ErrorDTO TES_DepositosLote_Registro_Desvincular(int CodEmpresa, string Usuario, string Datos)
        {
            List<TES_Depositos_Tramite_BancoDTO> lista = JsonConvert.DeserializeObject<List<TES_Depositos_Tramite_BancoDTO>>(Datos) ?? new List<TES_Depositos_Tramite_BancoDTO>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var item in lista)
                    {
                        var query = "exec spTES_Deposito_Desvincula @banco, @documento ,@cedula, @usuario";
                        connection.Execute(query,
                            new
                            {
                                banco = item.id_banco,
                                documento = item.documento,
                                cedula = item.cliente_id,
                                usuario = Usuario,
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
    }
}
