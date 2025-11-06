using Dapper;
using Microsoft.Data.SqlClient;
using PdfSharp.Pdf.Filters;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using PgxAPI.Models.Security;
using System.Text;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_RemesasLiquidacionesDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;
        private readonly MSecurityMainDb _Security_MainDB;

        public frmAF_RemesasLiquidacionesDB(IConfiguration? config)
        {
            _config = config;
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        #region Remesas
        /// <summary>
        /// Metodo para obtener las remesas de liquidaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<AfRemesasLiquidacionesLista> AF_RemesasLiquidaciones_Remesa_Obtener(int CodEmpresa, FiltrosLazyLoadData filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<AfRemesasLiquidacionesLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new AfRemesasLiquidacionesLista()
                {
                    total = 0,
                    lista = new List<AfRemesaLiquidacionDto>()
                }
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select COUNT(*) from AFI_REMESAS_LIQ";
                    result.Result.total = connection.ExecuteScalar<int>(query);

                    if (filtro.filtro != null && filtro.filtro != "")
                    {
                        filtro.filtro = $@"WHERE ( 
                                              COD_REMESA like '%{filtro.filtro}%' 
                                              OR USUARIO like '%{filtro.filtro}%'
                                              OR NOTAS like '%{filtro.filtro}%'
                                              OR FECHA like '%{filtro.filtro}%' 
                                          )";
                    }

                    if (filtro.sortField == "" || filtro.sortField == null)
                    {
                        filtro.sortField = "FECHA";
                    }

                    if (filtro.sortOrder == 0)
                    {
                        filtro.sortOrder = 1; //Por defecto orden ascendente
                    }

                    query = $@"select * from AFI_REMESAS_LIQ {filtro.filtro}
                                         ORDER BY {filtro.sortField} {(filtro.sortOrder == -1 ? "ASC" : "DESC")}  
                                         OFFSET {filtro.pagina} ROWS
                                         FETCH NEXT {filtro.paginacion} ROWS ONLY";

                    result.Result.lista = connection.Query<AfRemesaLiquidacionDto>(query).ToList();
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
        /// Metodo para obtener una remesa de liquidacion por su codigo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto<AfRemesaLiquidacionDto> AF_RemesasLiquidaciones_Remesa_Obtener(int CodEmpresa, int remesa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<AfRemesaLiquidacionDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new AfRemesaLiquidacionDto()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select * from AFI_REMESAS_LIQ where Cod_Remesa = {remesa}";
                    result.Result = connection.Query<AfRemesaLiquidacionDto>(query).FirstOrDefault();
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
        /// Metodo para guardar una remesa de liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto AF_RemesasLiquidaciones_Remesa_Guardar(int CodEmpresa, AfRemesaLiquidacionDto remesa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {
                if (remesa.estado == "C")
                {
                    result.Code = -1;
                    result.Description = "No se puede Modifica la remesa, porque esta ya fue cerrada...";
                    return result;
                }

                using var connection = new SqlConnection(stringConn);
                {

                    //consulto si existe remesa
                    var existeQry = @$"select COUNT(*) from AFI_REMESAS_LIQ where COD_REMESA = {remesa.cod_remesa}";
                    var existe = connection.ExecuteScalar<int>(existeQry);

                    string fechaInicioStr = _AuxiliarDB.validaFechaGlobal(remesa.fecha_inicio);
                    string fechaCorteStr = _AuxiliarDB.validaFechaGlobal(remesa.fecha_corte);

                    if (existe == 0)
                    {
                        int permiso = Derecho(CodEmpresa,remesa.usuario, "frmAF_RemesasLiquidaciones", "nuevo").Result;
                        if (permiso == 0)
                        {
                            result.Code = -1;
                            result.Description = "No tiene los permisos para realizar esta opción, verifique...!!!";
                            return result;
                        }

                        //obtengo nuevo codigo
                        var query = "select isnull(max(cod_remesa),0) + 1 as Ultimo from AFI_REMESAS_LIQ";
                        remesa.cod_remesa = connection.ExecuteScalar<int>(query);
                        //inserto
                        var insertQry = $@"INSERT INTO AFI_REMESAS_LIQ (cod_remesa,usuario,fecha,estado,fecha_inicio,fecha_corte,notas)
                                            VALUES (@cod_remesa,@usuario,@fecha,'A',@fecha_inicio,@fecha_corte,@notas)";
                        var values = new
                        {
                            cod_remesa = remesa.cod_remesa,
                            fecha = remesa.fecha,
                            usuario = remesa.usuario,
                            fecha_inicio = fechaInicioStr,
                            fecha_corte = fechaCorteStr,
                            notas = remesa.notas,
                        };
                        connection.Execute(insertQry, values);

                        _Security_MainDB.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = remesa.usuario.ToUpper(),
                            DetalleMovimiento = $"Remesa de Afliaciones  a Microfilmado : {remesa.cod_remesa}",
                            Movimiento = "Registra - WEB",
                            Modulo = vModulo
                        });
                    }
                    else
                    {
                        int permiso = Derecho(CodEmpresa,remesa.usuario, "frmAF_RemesasLiquidaciones", "nuevo").Result;
                        if (permiso == 0)
                        {
                            result.Code = -1;
                            result.Description = "No tiene los permisos para realizar esta opción, verifique...!!!";
                            return result;
                        }
                        //actualizo
                        var updateQry = $@"UPDATE AFI_REMESAS_LIQ
                                            SET 
                                                USUARIO = @usuario,
                                                FECHA_INICIO = @fecha_inicio,
                                                FECHA_CORTE = @fecha_corte,
                                                NOTAS = @notas
                                            WHERE COD_REMESA = @cod_remesa";
                        var values = new
                        {
                            cod_remesa = remesa.cod_remesa,
                            usuario = remesa.usuario.ToUpper(),
                            fecha_inicio = fechaInicioStr,
                            fecha_corte = fechaCorteStr,
                            notas = remesa.notas,
                        };
                        connection.Execute(updateQry, values);

                        _Security_MainDB.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = remesa.usuario,
                            DetalleMovimiento = $"Remesa de Afliaciones  a Microfilmado : {remesa.cod_remesa}",
                            Movimiento = "Modifica - WEB",
                            Modulo = vModulo
                        });
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
        /// Metodo para eliminar una remesa de liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto AF_RemesasLiquidaciones_Remesa_Eliminar(int CodEmpresa, string usuario, int cod_remesa, string estado)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {
                int permiso = Derecho(CodEmpresa,usuario, "frmAF_RemesasLiquidaciones", "borrar").Result;
                if (permiso == 0)
                {
                    result.Code = -1;
                    result.Description = "No tiene los permisos para realizar esta opción, verifique...!!!";
                    return result;
                }

                if (estado == "C")
                {
                    result.Code = -1;
                    result.Description = "No se puede Modifica la remesa, porque esta ya fue cerrada...";
                    return result;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    if (estado == "A")
                    {
                        var deleteQry = $@"DELETE FROM AFI_REMESAS_LIQ WHERE COD_REMESA = @cod_remesa";
                        var values = new
                        {
                            cod_remesa = cod_remesa,
                        };
                        connection.Execute(deleteQry, values);

                        _Security_MainDB.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Remesa deAfiliaciones(Ingresos) : {cod_remesa}",
                            Movimiento = "Elimina - WEB",
                            Modulo = vModulo
                        });
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
        #endregion

        #region Cargas
        /// <summary>
        /// Metodo para obtener las remesa de liquidacion activas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfRemesaLiquidacionDto>> AF_RemesasLiquidaciones_Carga_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<AfRemesaLiquidacionDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRemesaLiquidacionDto>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select * from AFI_REMESAS_LIQ where estado = 'A' order by fecha desc";
                    result.Result = connection.Query<AfRemesaLiquidacionDto>(query).ToList();
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
        /// Metodo para obtener las oficinas asociadas a una remesa de liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto<AfRemesasLiquiCargaDatos> AF_RemesasLiqui_CargaOficinas_Obtener(int CodEmpresa, int remesa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<AfRemesasLiquiCargaDatos>
            {
                Code = 0,
                Description = "Ok",
                Result = new AfRemesasLiquiCargaDatos()
                {
                    cboOficinas = new List<DropDownListaGenericaModel>()
                }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select fecha_inicio,fecha_corte from AFI_REMESAS_LIQ where cod_remesa = @remesa";
                    var remesaData = connection.QueryFirstOrDefault<AfRemesasLiquiCargaDatos>(query, new { remesa = remesa });
                    if (remesaData != null)
                    {
                        result.Result.fecha_inicio = remesaData.fecha_inicio;
                        result.Result.fecha_corte = remesaData.fecha_corte;
                    }
                    else
                    {
                        return result;
                    }

                        //Busco oficinas
                        query = @$"select rtrim(cod_oficina) as 'item', rtrim(cod_oficina) + ' - ' + rtrim(descripcion) as 'descripcion'
		                        from SIF_Oficinas  where cod_oficina in(
		                        select cod_oficina 
		                        from Liquidacion  where Fecliq between  '{remesaData.fecha_inicio:yyyy-MM-dd} 00:00:00'
		                        and '{remesaData.fecha_corte:yyyy-MM-dd} 23:59:00' and cod_remesa is null)
		                        order by cod_oficina";
                    result.Result.cboOficinas = connection.Query<DropDownListaGenericaModel>(query).ToList();

                    //Agregar en la posicion 1 el item TODOS con valor T
                    result.Result.cboOficinas.Insert(0, new DropDownListaGenericaModel
                    {
                        item = "T",
                        descripcion = "T - TODAS LAS OFICINAS"
                    });
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
        /// Metodo para obtener las liquidaciones asociadas a una remesa de liquidacion y oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <param name="oficina"></param>
        /// <returns></returns>
        public ErrorDto<List<AfRemesasLiquiCargaLista>> AF_RemesasLiqui_CargaLista_Obtener(int CodEmpresa, int remesa, string oficina)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<AfRemesasLiquiCargaLista>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRemesasLiquiCargaLista>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select fecha_inicio,fecha_corte from AFI_REMESAS_LIQ where cod_remesa = @remesa";
                    var remesaData = connection.QueryFirstOrDefault<AfRemesasLiquiCargaDatos>(query, new { remesa = remesa });
                    

                    string where = "";

                    if (!string.IsNullOrEmpty(oficina) && oficina != "T")
                    {
                        where = $"  and L.cod_Oficina = '{oficina}' ";
                    }

                    //Busco oficinas
                    query = @$"select L.Consec,L.Cedula,S.nombre,L.FecLiq
		                        from  Liquidacion L inner join Socios S on L.cedula = S.cedula
		                        where L.FecLiq between '{remesaData.fecha_inicio:yyyy-MM-dd} 00:00:00'
		                        and '{remesaData.fecha_corte:yyyy-MM-dd} 23:59:00' and L.cod_remesa is null
		                        and dbo.fxSIFTagCierre(l.CEDULA, L.CONSEC,'LIQ') = 1 {where}
                                 order by L.consec";
                    result.Result = connection.Query<AfRemesasLiquiCargaLista>(query).ToList();

                    //if(result.Result == null || result.Result.Count == 0)
                    //{
                    //    //lleno una linea temproal para pruebas
                    //    result.Result = new List<Af_RemesasLiquiCargaLista>();
                    //    result.Result.Add( new Af_RemesasLiquiCargaLista
                    //    {
                    //        consec = 0,
                    //        cedula = "000000000",
                    //        nombre = "SIN REGISTROS PARA LA SELECCION REALIZADA ** BORRAR **",
                    //        fecLiq = DateTime.Now,
                    //    });

                    //    result.Result.Add(new Af_RemesasLiquiCargaLista
                    //    {
                    //        consec = -1,
                    //        cedula = "000000001",
                    //        nombre = "SIN REGISTROS PARA LA SELECCION REALIZADA ** BORRAR **",
                    //        fecLiq = DateTime.Now,
                    //    });

                    //}

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
        /// Metodo para cargar las liquidaciones a una remesa de liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto AF_RemesasLiquidaciones_Carga_Cargar(int CodEmpresa, int remesa, string usuario, List<AfRemesasLiquiCargaLista> datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //'Valida el Estado de la Remesa
                    var query = $@"select count(*) as Existe from AFI_REMESAS_LIQ
		                                where cod_remesa = @cod_remesa
		                                and estado = 'A'";
                    var values = new
                    {
                        cod_remesa = remesa,
                    };
                    
                    var existe = connection.ExecuteScalar<int>(query, values);
                    if (existe == 0)
                    {
                        result.Code = -1;
                        result.Description = "La Remesa actual; ya se encuentra cerrada...";
                        return result;
                    }

                    foreach (AfRemesasLiquiCargaLista item in datos)
                    {
                        //Actualizo la liquidacion con la remesa
                        query = $@"update Liquidacion set cod_remesa = @cod_remesa
                                     where consec = @consec";
                        var filtros = new
                        {
                            cod_remesa = remesa,
                            consec = item.consec,
                        };
                        connection.Execute(query, filtros);
                        
                    }

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Carga Remesa Liquidaciones a Microfilmado : {remesa}",
                        Movimiento = "Aplica - WEB",
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
        /// Metodo para cerrar la carga de liquidaciones a una remesa de liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto AF_RemesasLiquidaciones_Carga_Cerrar(int CodEmpresa, int remesa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //'Valida el Estado de la Remesa
                    var query = $@"select count(*) as Existe from AFI_REMESAS_LIQ
		                                where cod_remesa = @cod_remesa
		                                and estado = 'A'";
                    var values = new
                    {
                        cod_remesa = remesa,
                    };

                    var existe = connection.ExecuteScalar<int>(query, values);
                    if (existe == 0)
                    {
                        result.Code = -1;
                        result.Description = "La Remesa actual; ya se encuentra cerrada...";
                        return result;
                    }

                    //Actualiza el Estado de la Remesa como cerrada
                    query = $@"update AFI_REMESAS_LIQ set estado = 'C' 
                                     where cod_remesa = @cod_remesa";
                    var filtros = new   
                        {
                        cod_remesa = remesa,
                    };
                    connection.Execute(query, filtros);

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Cierra Remesa Liquidaciones a Microfilmado :  {remesa}",
                        Movimiento = "Aplica - WEB",
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

        #endregion

        #region Reportes

        /// <summary>
        /// Metodo para obtener las remesas de liquidaciones para reporte
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public ErrorDto<List<AfRemesaLiquidacionDto>> AF_RemesasLiquidaciones_Reporte_Obtener(int CodEmpresa, DateTime fechaInicio, DateTime fechaCorte, int top)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<AfRemesaLiquidacionDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRemesaLiquidacionDto>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    
                    var query = @$"select TOP {top} * from AFI_REMESAS_LIQ 
                                    WHERE fecha between '{fechaInicio:yyyy-MM-dd} 00:00:00' and '{fechaCorte:yyyy-MM-dd} 23:59:00'
                                    order by fecha desc";
                    result.Result = connection.Query<AfRemesaLiquidacionDto>(query).ToList();
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
        /// Metodo para aplicar el reporte de remesas de liquidaciones a microfilmado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto AF_RemesasLiquidaciones_Reporte_Aplicar(int CodEmpresa,string usuario, int remesa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {
                int permiso = Derecho(CodEmpresa,usuario, "frmAF_RemesasLiquidaciones", "cmdMicrofilm").Result;
                if (permiso == 0)
                {
                    result.Code = -1;
                    result.Description = "No tiene los permisos para realizar esta opción, verifique...!!!";
                    return result;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    //Valido la remesa
                    var existeQry = $@"select * from AFI_REMESAS_LIQ where cod_remesa = @remesa";
                    var remesaData = connection.QueryFirstOrDefault<AfRemesaLiquidacionDto>(existeQry, new { remesa = remesa });
                    if (remesaData == null)
                    {
                        result.Code = -1;
                        result.Description = "La Remesa seleccionada no existe, verifique...!!!";
                        return result;
                    }


                    var query = $@"update AFI_REMESAS_LIQ set Microfilm_Fecha = dbo.MyGetdate(), Microfilm_usuario = @usuario
                                      where cod_remesa = @remesa";

                    connection.Execute(query, new
                    {
                        usuario = usuario,
                        remesa = remesa,
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

        #endregion

        #region Consultas

        /// <summary>
        /// Metodo para obtener las remesas de liquidaciones asociadas a una liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consec"></param>
        /// <returns></returns>
        public ErrorDto<string> AF_RemesasLiquidaciones_Consultas_Obtener(int CodEmpresa, string consec)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = ""
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    connection.Open();

                    var query = @$"select A.* from AFI_REMESAS_LIQ A inner join Liquidacion X on A.cod_remesa = X.cod_remesa where X.consec  = @consec";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@consec", consec);

                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                result.Result = "** No se encontró Liquidación en las remesas registradas **";
                            }
                            else
                            {
                                var sb = new StringBuilder();

                                while (reader.Read())
                                {
                                    sb.AppendLine($"Remesa\t...: {reader["cod_remesa"]}");
                                    sb.AppendLine($"Fecha\t...: {Convert.ToDateTime(reader["fecha"]).ToString("dd/MM/yyyy")}");
                                    sb.AppendLine($"Usuario\t...: {reader["usuario"]}");
                                    sb.AppendLine(); // línea en blanco entre registros
                                }

                                result.Result = sb.ToString();
                            }
                        }
                    }
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

        #endregion

        /// <summary>
        /// Valida permisos de usuario
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private ErrorDto<int> Derecho(int CodEmpresa,string usuario, string formName, string boton)
        {
            var result = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            result.Result = _Security_MainDB.Derecho(new ParametrosAccesoDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario.ToUpper(),
                Modulo = vModulo,
                FormName = formName,
                Boton = boton
            });

            return result;
        }

        /// <summary>
        /// Metodo que valida si la empresa requiere autorizacion para ciertas operaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        private ErrorDto<bool> sbRequiereAutorizacion(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };
            try
            {
           
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(valor,'') from CRD_PARAMETROS where cod_parametro = '27'";

                    var valor = connection.Query<string>(query).FirstOrDefault();
                    if(valor == "S")
                    {
                        result.Result = true;
                    }

                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = false;
            }
            return result;
        }


    }
}
