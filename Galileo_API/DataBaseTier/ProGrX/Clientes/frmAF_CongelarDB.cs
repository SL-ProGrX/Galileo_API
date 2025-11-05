using AutoMapper.Internal.Mappers;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_CongelarDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmAF_CongelarDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        #region Consulta

        /// <summary>
        /// Metodo para obtener los socios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Congela_Socios_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"SELECT CEDULA as item,NOMBRE as descripcion FROM SOCIOS ORDER BY CEDULA";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene los bloqueos y congelamientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosCongelar"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> AF_BloqueosCongelamientos_Obtener(int CodEmpresa, string filtrosCongelar, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            AF_CongelarFiltros filtro = JsonConvert.DeserializeObject<AF_CongelarFiltros>(filtrosCongelar) ?? new AF_CongelarFiltros();
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<AF_CongelarDTO>()
                }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = @$"select COUNT(*)
		                                from afi_congelar C inner join Socios S on C.cedula = S.cedula
		                                inner join afi_congelar_causas X on C.cod_causa = X.cod_causa
		                                where 
		                                C.cedula LIKE @cedula
		                                AND S.nombre LIKE @nombre
		                                ";
                    if (!filtro.chkTodasFechas)
                    {
                        queryT += $@"AND C.fecha_Inicia BETWEEN  '{filtro.fecha_desde:yyyy-MM-dd} 00:00:00' AND '{filtro.fecha_hasta:yyyy-MM-dd} 23:59:00'";
                    }

                    if(filtro.estado != "X")
                    {
                        queryT += "AND C.estado = @Estado";
                    }

                    response.Result.total = connection.QueryFirstOrDefault<int>(queryT, new
                    {
                        cedula = $"%{filtro.cedula}%",
                        nombre = $"%{filtro.nombre}%",
                        Estado = filtro.estado

                    });

                    var query = @$"
                            select * FROM (
                            select C.*,S.nombre,rtrim(C.cod_causa) as 'CausaId', rtrim(X.descripcion) as 'CausaDesc'
		                                from afi_congelar C inner join Socios S on C.cedula = S.cedula
		                                inner join afi_congelar_causas X on C.cod_causa = X.cod_causa
		                                where 
		                                C.cedula LIKE @cedula
		                                AND S.nombre LIKE @nombre
		                                ";
                    ;
                    if (!filtro.chkTodasFechas)
                    {
                        query += $@"AND C.fecha_Inicia BETWEEN  '{filtro.fecha_desde:yyyy-MM-dd} 00:00:00' AND '{filtro.fecha_hasta:yyyy-MM-dd} 23:59:00'";
                    }

                    if (filtro.estado != "X")
                    {
                        query += "AND C.estado = @Estado";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_congelar";
                    }

                    if (!String.IsNullOrEmpty(filtros.filtro))
                    {
                        query += $@" AND ( C.cod_congelar like @causa OR C.cedula LIKE @cedulaF
		                                OR S.nombre LIKE @nombreF)";
                    }


                    if (filtros.pagina != null)
                    {
                        query += $@") T
                                ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                OFFSET {filtros.pagina} ROWS
                                FETCH NEXT {filtros.paginacion} ROWS ONLY";
                    }

                    response.Result.lista = connection.Query<AF_CongelarDTO>(query, new
                    {
                        cedula = $"%{filtro.cedula}%",
                        nombre = $"%{filtro.nombre}%",
                        Estado = filtro.estado,
                        causa = $"%{filtros.filtro}%",
                        cedulaF = $"%{filtros.filtro}%",
                        nombreF = $"%{filtros.filtro}%",

                    }).ToList();
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
        /// Metodo para exportar los bloqueos y congelamientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosCongelar"></param>
        /// <returns></returns>
        public ErrorDto<List<AF_CongelarDTO>> AF_BloqueosCongelamientos_Exportar(int CodEmpresa, string filtrosCongelar)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            AF_CongelarFiltros filtro = JsonConvert.DeserializeObject<AF_CongelarFiltros>(filtrosCongelar) ?? new AF_CongelarFiltros();
            var response = new ErrorDto<List<AF_CongelarDTO>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AF_CongelarDTO>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = @$"
                            select * FROM (
                            select C.*,S.nombre,rtrim(C.cod_causa) as 'CausaId', rtrim(X.descripcion) as 'CausaDesc'
		                                from afi_congelar C inner join Socios S on C.cedula = S.cedula
		                                inner join afi_congelar_causas X on C.cod_causa = X.cod_causa
		                                where 
		                                C.cedula LIKE @Cedula
		                                AND S.nombre LIKE @Nombre
		                                AND C.estado = @Estado ";
                    ;
                    if (!filtro.chkTodasFechas)
                    {
                        query += $@"AND C.fecha_Inicia BETWEEN  '{filtro.fecha_desde:yyyy-MM-dd} 00:00:00' AND '{filtro.fecha_hasta:yyyy-MM-dd} 23:59:00'";
                    }

                    query += $@") T ORDER BY cod_congelar DESC";

                    response.Result = connection.Query<AF_CongelarDTO>(query, new
                    {
                        Cedula = $"%{filtro.cedula}%",
                        Nombre = $"%{filtro.nombre}%",
                        Estado = filtro.estado

                    }).ToList();
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

        #endregion

        #region Registro

        /// <summary>
        /// Metodo para obtener tipos de causa para congelar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CongelarCausaLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select rtrim(COD_CAUSA) as 'item',  rtrim(descripcion) as  'descripcion' from AFI_CONGELAR_CAUSAS  where Activa = 1 order by COD_CAUSA";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Obtener tipos de causa para congelar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CongelarCausa_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"SELECT 
                                    COD_CAUSA AS IDX,
                                    DESCRIPCION AS ItmX,
                                    ACTIVA,
                                    REGISTRO_FECHA,
                                    REGISTRO_USUARIO
                                FROM 
                                    AFI_CONGELAR_CAUSAS
                                ORDER BY 
                                    COD_CAUSA;";
                    response.Result = connection.Query(query)
                        .Select(row => new DropDownListaGenericaModel
                        {
                            item = row.IDX,
                            descripcion = row.ItmX
                        }).ToList();
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
        /// Inserta o Actualiza Bloqueos o Congelamientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="congelar"></param>
        /// <returns></returns>
        public ErrorDto AF_BloqueosCongelamientos_Guardar(int CodEmpresa, string usuario, AF_CongelarDTO congelar)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Guardado correctamente"
            };

            try
            {

                using var connection = new SqlConnection(stringConn);
                {

                    if (congelar.cod_congelar != 0)
                    {
                        var sql = @"
                                UPDATE afi_congelar
                                SET
                                    notas = @Notas,
                                    cod_causa = @CodCausa,
                                    estado = @Estado,
                                    per_liquidacion = @PerLiquidacion,
                                    per_mostrar_ec = @PerMostrarEc,
                                    per_abono_cajas = @PerAbonoCajas,
                                    per_cierra_AcCreditos = @PerCierraAcCreditos,
                                    per_cobro_judicial = @PerCobroJudicial,
                                    per_traspaso_deudas = @PerTraspasoDeudas,
                                    per_reversiones = @PerReversiones,
                                    per_readecuaciones = @PerReadecuaciones,
                                    per_deducciones_creditos = @PerDeduccionesCreditos,
                                    per_deducciones_aportes = @PerDeduccionesAportes,
                                    per_generacion_mora = @PerGeneracionMora,
                                    per_cobro_FndSol = @PerCobroFndSol,
                                    per_cobro_cuotaCr = @PerCobroCuotaCr,
                                    fecha_inicia = @FechaInicia,
                                    fecha_finaliza = @FechaFinaliza
                                WHERE cod_congelar = @CodCongelar;
                                ";

                        connection.Execute(sql, new
                        {
                            Notas = congelar.notas?.Trim(),
                            CodCausa = congelar.cod_causa?.Trim(),
                            Estado = congelar.estado?.Trim(),
                            PerLiquidacion = congelar.per_liquidacion,
                            PerMostrarEc = congelar.per_mostrar_ec,
                            PerAbonoCajas = congelar.per_abono_cajas,
                            PerCierraAcCreditos = congelar.per_cierra_accreditos,
                            PerCobroJudicial = congelar.per_cobro_judicial,
                            PerTraspasoDeudas = congelar.per_traspaso_deudas,
                            PerReversiones = congelar.per_reversiones,
                            PerReadecuaciones = congelar.per_readecuaciones,
                            PerDeduccionesCreditos = congelar.per_deducciones_creditos,
                            PerDeduccionesAportes = congelar.per_deducciones_aportes,
                            PerGeneracionMora = congelar.per_generacion_mora,
                            PerCobroFndSol = congelar.per_cobro_fndsol,
                            PerCobroCuotaCr = congelar.per_cobro_cuotacr,
                            FechaInicia = congelar.fecha_inicia,
                            FechaFinaliza = congelar.fecha_finaliza,
                            CodCongelar = congelar.cod_congelar
                        });

                    }
                    else
                    {
                        var sql = @"
                        INSERT INTO afi_congelar (
                            cedula,
                            cod_causa,
                            notas,
                            fecha_crea,
                            usuario_crea,
                            estado,
                            fecha_Inicia,
                            fecha_Finaliza,
                            per_liquidacion,
                            per_mostrar_ec,
                            per_abono_cajas,
                            per_cierra_AcCreditos,
                            per_cobro_judicial,
                            per_traspaso_deudas,
                            per_reversiones,
                            per_readecuaciones,
                            per_deducciones_creditos,
                            per_deducciones_aportes,
                            per_generacion_mora,
                            per_cobro_FndSol,
                            per_cobro_cuotaCr
                        )
                        VALUES (
                            @Cedula,
                            @CodCausa,
                            @Notas,
                            dbo.MyGetDate(),
                            @UsuarioCrea,
                            @Estado,
                            @FechaInicia,
                            @FechaFinaliza,
                            @PerLiquidacion,
                            @PerMostrarEc,
                            @PerAbonoCajas,
                            @PerCierraAcCreditos,
                            @PerCobroJudicial,
                            @PerTraspasoDeudas,
                            @PerReversiones,
                            @PerReadecuaciones,
                            @PerDeduccionesCreditos,
                            @PerDeduccionesAportes,
                            @PerGeneracionMora,
                            @PerCobroFndSol,
                            @PerCobroCuotaCr
                        );";

                        connection.Execute(sql, new
                        {
                            Cedula = congelar.cedula,
                            CodCausa = congelar.cod_causa,
                            Notas = congelar.notas,
                            UsuarioCrea = congelar.usuario_crea,
                            Estado = congelar.estado,
                            FechaInicia = congelar.fecha_inicia,
                            FechaFinaliza = congelar.fecha_finaliza,
                            PerLiquidacion = congelar.per_liquidacion,
                            PerMostrarEc = congelar.per_mostrar_ec,
                            PerAbonoCajas = congelar.per_abono_cajas,
                            PerCierraAcCreditos = congelar.per_cierra_accreditos,
                            PerCobroJudicial = congelar.per_cobro_judicial,
                            PerTraspasoDeudas = congelar.per_traspaso_deudas,
                            PerReversiones = congelar.per_reversiones,
                            PerReadecuaciones = congelar.per_readecuaciones,
                            PerDeduccionesCreditos = congelar.per_deducciones_creditos,
                            PerDeduccionesAportes = congelar.per_deducciones_aportes,
                            PerGeneracionMora = congelar.per_generacion_mora,
                            PerCobroFndSol = congelar.per_cobro_fndsol,
                            PerCobroCuotaCr = congelar.per_cobro_cuotacr
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


        #endregion

        #region Mantenimiento

        /// <summary>
        /// Metodo para obtener tipos de causa para congelar en mantenimiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AF_CongelaCausaDTO>> AF_CongelarCausaMant_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AF_CongelaCausaDTO>>
            {
                Code = 0,
                Result = new List<AF_CongelaCausaDTO>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select COD_CAUSA,descripcion,Activa,registro_fecha,registro_usuario from AFI_CONGELAR_CAUSAS  order by COD_CAUSA";
                    response.Result = connection.Query<AF_CongelaCausaDTO>(query).ToList();
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
        /// Metodo para eliminar una causa de congelamiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_causa"></param>
        /// <returns></returns>
        public ErrorDto AF_CongelarCausaMant_Eliminar(int CodEmpresa, string cod_causa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //valido si la causa se esta usando
                    var query = @"select COUNT(*) from AFI_CONGELAR where cod_causa = @CodCausa";
                    int uso = connection.QueryFirstOrDefault<int>(query, new { CodCausa = cod_causa });
                    if (uso > 0)
                    {
                        response.Code = -1;
                        response.Description = "No se puede eliminar la causa porque está siendo utilizada en congelamientos.";
                        return response;
                    }

                    query = @"DELETE FROM AFI_CONGELAR_CAUSAS WHERE COD_CAUSA = @CodCausa";
                    connection.Execute(query, new { CodCausa = cod_causa });
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
        /// Metodo para guardar una causa de congelamiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="causa"></param>
        /// <returns></returns>
        public ErrorDto AF_CongelarCausaMant_Guardar(int CodEmpresa, string usuario ,AF_CongelaCausaDTO causa)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                if (causa.isNew)
                {
                    response = AF_CongelarCausaMant_Insertar(CodEmpresa, usuario, causa);
                }
                else
                {
                    response = AF_CongelarCausaMant_Actualiza(CodEmpresa, usuario, causa);
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
        /// Metodo para insertar una causa de congelamiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="causa"></param>
        /// <returns></returns>
        private ErrorDto AF_CongelarCausaMant_Insertar(int CodEmpresa, string usuario, AF_CongelaCausaDTO causa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    

                    var query = @"insert AFI_CONGELAR_CAUSAS(cod_causa,descripcion, activa, registro_fecha, registro_usuario)
                                   VALUES(@cod_causa,@descripcion, @activa,dbo.MyGetDate(), @registro_usuario)";
                    connection.Execute(query, new {
                        cod_causa = causa.cod_causa,
                        descripcion = causa.descripcion,
                        activa = (causa.activa == true) ? 1: 0,
                        registro_usuario = usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Causa de Congelamiento Cod: : {causa.cod_causa}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });
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
        /// Metodo para actualizar una causa de congelamiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="causa"></param>
        /// <returns></returns>
        private ErrorDto AF_CongelarCausaMant_Actualiza(int CodEmpresa, string usuario, AF_CongelaCausaDTO causa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {


                    var query = @"update AFI_CONGELAR_CAUSAS set descripcion = @descripcion , Activa = @activa  where cod_causa = @cod_causa";
                    connection.Execute(query, new
                    {
                        cod_causa = causa.cod_causa,
                        descripcion = causa.descripcion,
                        activa = (causa.activa == true) ? 1 : 0
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Causa de Congelamiento Cod: : {causa.cod_causa}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        #endregion




    }
}