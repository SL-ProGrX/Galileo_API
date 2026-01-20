using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Newtonsoft.Json;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesDepositosLoteDB
    {
        private readonly PortalDB _portalDB;
        private readonly MCntLinkDB mCntLink;
        private readonly MTesoreria mTesoreria;

        public FrmTesDepositosLoteDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            mCntLink = new MCntLinkDB(config);
            mTesoreria = new MTesoreria(config);
        }

        /// <summary>
        /// Obtener lista de cuenta bancarias que tiene acceso el usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<TesCuentaBancariaDto>> TES_DepositosLote_Ctas_Obtener(int CodEmpresa, string usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"exec spTes_Cuenta_Bancaria_Acceso @usuario,'DP','SOL'";

                return conn.Query<TesCuentaBancariaDto>(query, new { usuario = usuario }).ToList();
            });
        }

        /// <summary>
        /// Cargar archivo de depositos lote
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="archivoData"></param>
        /// <returns></returns>
        public ErrorDto<List<TesDepositosTramiteDto>> TES_DepositosLote_ArchivoCarga(int CodEmpresa, string archivoData)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            List<TesDepositosTramiteDto> lista = JsonConvert.DeserializeObject <List<TesDepositosTramiteDto>>(archivoData) ?? new List<TesDepositosTramiteDto>();
          
            try
            {
                foreach (var item in lista)
                {
                    var query = @"select dbo.fxTes_DP_Cargado(@banco,@documento,'',@monto) as Existe";
                    var vExiste = conn.QueryFirstOrDefault<int>(query,
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
                    item.existe = (vExiste > 0);
                }

                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesDepositosTramiteDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Procesar depositos lote del archivo cargado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cuenta"></param>
        /// <param name="usuario"></param>
        /// <param name="archivoData"></param>
        /// <returns></returns>
        public ErrorDto TES_DepositosLote_Procesar(int CodEmpresa, string cuenta, string usuario, string archivoData)
        {
            List<TesDepositosTramiteDto> lista = JsonConvert.DeserializeObject<List<TesDepositosTramiteDto>>(archivoData) ?? new List<TesDepositosTramiteDto>();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var query = "";
            string mensaje = "";
            int vCasos = 0;
            bool vInconsistencia = false;
            try
            {
                string vCuenta = mCntLink.fxgCntCuentaFormato(CodEmpresa, false, cuenta, 0);
                bool ctaValida = mCntLink.fxgCntCuentaValida(CodEmpresa, vCuenta);
                if (!ctaValida)
                {
                    return DbHelper.ErrorResponse("La cuenta especificada para registro no es v&aacute;lida...verifique!");
                }
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
                    conn.Execute(query,
                        new
                        {
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
                        mensaje = "No se procesaron casos *--Revisados--* para el control de dep&oacute;sitos!";
                    }
                    else
                    {
                        mensaje = "Carga realizada Satisfactoriamente... Registros Procesados: " + vCasos;
                    }
                    var mensajeBuilder = new StringBuilder(mensaje);
                    if (vInconsistencia)
                    {
                        mensajeBuilder.AppendLine("\nSe presentaron inconsistencias en la carga..Revise en el TAB de consulta de inconsistencias!");
                    }
                    mensaje = mensajeBuilder.ToString();
                }

                return DbHelper.OkResponse(mensaje);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtener lista de inconsistencias
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <param name="fecha_inicio"></param>
        /// <param name="fecha_corte"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_DepositosLote_Inconsistencias_Obtener(int CodEmpresa, string filtros)
        {
            FiltrosInconsistencias param = JsonConvert.DeserializeObject<FiltrosInconsistencias>(filtros) ?? new FiltrosInconsistencias();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Result = new TablasListaGenericaModel()
            };
            string vFiltro = "";
            try
            {
                var fechaInicio = param.fecha_inicio.Date;
                var fechaCorte = param.fecha_corte.Date.AddDays(1).AddTicks(-1);
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
                response.Result.total = conn.QueryFirstOrDefault<int>(queryT, parametros);
                response.Result.lista = conn.Query<TesDepositosTramiteInconsistenciasDto>(query, parametros).ToList();

                return response;
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }
        }

        /// <summary>
        /// Obtener lista de depositos tramite
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_DepositosLote_Registro_Obtener(int CodEmpresa, string filtros)
        {
            FiltrosRegistro param = JsonConvert.DeserializeObject<FiltrosRegistro>(filtros) ?? new FiltrosRegistro();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<TablasListaGenericaModel>
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
                response.Result.total = conn.QueryFirstOrDefault<int>(queryT, parametros);
                response.Result.lista = conn.Query<TesDepositosTramiteBancoDto>(query, parametros).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }
            return response;
        }

        /// <summary>
        /// Obtener un número de cuenta bancario mediante la categoría seleccionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Categoria"></param>
        /// <returns></returns>
        public ErrorDto<string> TES_DepositosLote_CategoriaCta_Obtener(int CodEmpresa, string Categoria)
        {
            try
            {
                string response = string.Empty;
                switch (Categoria)
                {
                    case "01": //Depositos en Cajas
                        response = mTesoreria.fxTesParametro(CodEmpresa, "05");
                        break;
                    case "02": //Depositos sin Identificar
                        response = mTesoreria.fxTesParametro(CodEmpresa, "06");
                        break;
                    case "03": //Depositos Otros..
                        response = mTesoreria.fxTesParametro(CodEmpresa, "07");
                        break;
                }

                return DbHelper.CreateOkResponse<string>(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<string>(ex.Message);
            }
        }

        /// <summary>
        /// Aplicar registro de depositos seleccionados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Datos"></param>
        /// <returns></returns>
        public ErrorDto TES_DepositosLote_Registro_Aplicar(int CodEmpresa, string Usuario, string Datos)
        {
            List<TesDepositosTramiteBancoDto> lista = JsonConvert.DeserializeObject<List<TesDepositosTramiteBancoDto>>(Datos) ?? new List<TesDepositosTramiteBancoDto>();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                string vRemesa = mTesoreria.fxTesParametro(CodEmpresa, "08");
                vRemesa = vRemesa + 1;
                var queryParam = "update tes_parametros set valor = @remesa where cod_parametro = '08'";
                conn.Execute(queryParam, new { remesa = vRemesa });

                foreach (var item in lista)
                {
                    var query = "exec spTES_Deposito_Lote_Registra @banco, @documento, @usuario, @remesa";
                    conn.Execute(query,
                        new
                        {
                            banco = item.id_banco,
                            documento = item.documento,
                            usuario = Usuario,
                            remesa = vRemesa
                        });
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualizar depositos lote
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto TES_DepositosLote_Registro_Actualizar(int CodEmpresa)
        {
            const string query = "exec spTES_Deposito_Lote_Actualiza";
            return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query);
        }

        /// <summary>
        /// Desvincular los depositos seleccionados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Datos"></param>
        /// <returns></returns>
        public ErrorDto TES_DepositosLote_Registro_Desvincular(int CodEmpresa, string Usuario, string Datos)
        {
            List<TesDepositosTramiteBancoDto> lista = JsonConvert.DeserializeObject<List<TesDepositosTramiteBancoDto>>(Datos) ?? new List<TesDepositosTramiteBancoDto>();
            const string query = "exec spTES_Deposito_Desvincula @banco, @documento ,@cedula, @usuario";

            try
            {
                foreach (var item in lista)
                {
                    var parametros = new
                    {
                        banco = item.id_banco,
                        documento = item.documento,
                        cedula = item.cliente_id,
                        usuario = Usuario,
                    };

                    DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, parametros);
                }

                return DbHelper.CreateOkResponse(); 
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            
        }
    }
}
