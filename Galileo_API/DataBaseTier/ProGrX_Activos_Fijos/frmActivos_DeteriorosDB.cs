using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Activos_Fijos;


namespace PgxAPI.DataBaseTier.ProGrX_Activos_Fijos
{
    public class frmActivos_DeteriorosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 36;
        private readonly mSecurityMainDb _Security_MainDB;
        private readonly mActivosFijos _mActivos;

        public frmActivos_DeteriorosDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
            _mActivos = new mActivosFijos(_config);
        }

        /// <summary>
        /// Método para obtiene las justificaciones de los activos en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Deterioros_Justificaciones_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select rtrim(cod_justificacion) as 'item',rtrim(descripcion) as 'descripcion' FROM Activos_justificaciones where tipo ='D'";
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
        /// Método para obtener los activos en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosData>> Activos_Deterioros_Activos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<ActivosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select num_placa, Placa_Alterna, Nombre from Activos_Principal ";
                    result.Result = connection.Query<ActivosData>(query).ToList();
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
        /// Método para consultar los detalles de un activo en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Id_AddRet"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosDeterioroData> Activos_Deterioros_Consultar(int CodEmpresa, int Id_AddRet, string placa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<ActivosDeterioroData>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosDeterioroData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select X.*,rtrim(J.cod_justificacion) as 'Motivo_Id', rtrim(J.descripcion) as 'Motivo_Desc'
                                 ,A.nombre,P.cod_proveedor,P.descripcion as Proveedor
                               from Activos_retiro_adicion X inner join Activos_Principal A on X.num_placa = A.num_placa
                                   inner join Activos_justificaciones J on X.cod_justificacion = J.cod_justificacion
                                   left join Activos_proveedores P on X.compra_proveedor = P.cod_proveedor                                  
                                where X.Id_AddRet = @Id_AddRet and X.num_placa = @placa ";
                    result.Result = connection.Query<ActivosDeterioroData>(query, new
                    {
                        Id_AddRet,
                        placa
                    }).FirstOrDefault();
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
        ///  Método para validar un activo en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_Deterioros_Validar(int CodEmpresa, string placa, DateTime fecha)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "",
                Result = string.Empty
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select fecha_adquisicion from Activos_Principal where num_placa = @placa and  estado <> 'R' ";
                    result.Result = connection.Query<string>(query, new { placa }).FirstOrDefault();

                    if (result.Result == null)
                    {
                        result.Code = -2;
                        result.Description = "El Activo no existe, o ya fue retirado ...";
                    }
                    else
                    {

                        DateTime fecha_adquisicion = DateTime.Parse(result.Result);


                        if ((fecha - fecha_adquisicion).Days < 1)
                        {
                            result.Code = -2;
                            result.Description = "La fecha del Movimiento no es válida, ya que es menor a la del activo ...";

                        }

                    }
                    var result2 = new ActivosPeriodosData();

                    var query2 = $@"select estado, dbo.fxActivos_PeriodoActual() as 'PeriodoActual' from Activos_periodos 
                                    where anio = @anno and mes =@mes ";
                    result2 = connection.Query<ActivosPeriodosData>(query2, new { anno = fecha.Year, mes = fecha.Month }).FirstOrDefault();

                    if (result2 != null)
                    {
                        if (result2?.estado.Trim() != "P")
                        {
                            result.Code = -2;
                            result.Description = result.Description + " - El Periodo del Movimiento ya fue cerrado ... ";
                        }

                        if ((fecha.Year != result2?.periodoactual.Year) || (fecha.Month != result2?.periodoactual.Month))
                        {
                            result.Code = -2;
                            result.Description = result.Description + " - La fecha de aplicación del movimiento no corresponde al periodo abierto!";
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

        /// <summary>
        /// Método para consultar los detalles de un activo en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosDeterioroDetallaData> Activos_DeteriorosDetalle_Consultar(int CodEmpresa, string placa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<ActivosDeterioroDetallaData>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosDeterioroDetallaData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spActivos_InfoDepreciacion @placa ";
                    result.Result = connection.Query<ActivosDeterioroDetallaData>(query, new { placa }).FirstOrDefault();

                    if (result.Result == null)
                    {
                        result.Result = new ActivosDeterioroDetallaData();
                        result.Result.depreciacionPeriodo = "????";
                        result.Result.depreciacion_acum = 0;
                        result.Result.valor_historico = 0;
                        result.Result.valor_desecho = 0;
                        result.Result.valor_libros = 0;
                    }
                    else
                    {
                        result.Result.depreciacionPeriodo = result.Result.depreciacion_periodo.ToString("dd/MM/yyyy");
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

        /// <summary>
        /// Método para guardar un activo en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Activos_Deterioros_Guardar(int CodEmpresa, string usuario, ActivosDeterioroData data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok",

            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {


                    var query = $@"exec spActivos_AdicionRetiro @Placa,'D',@Justificacion,@Descripcion,@Fecha,@Monto,1,@Usuario,'', '', '', ''";
                    int Linea = connection.Query<int>(query, new
                    {
                        Placa = data.num_placa,
                        Justificacion = data.motivo_id,
                        Descripcion = data.descripcion,
                        Fecha = data.fecha,
                        Monto = Math.Abs(data.monto),
                        Usuario = usuario,
                    }).FirstOrDefault();

                    result.Code = Linea;

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Deterioro (Placa: {data.num_placa})  Deterioro Id:{data.id_addret}_ {data.motivo_desc} ",
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
        /// Método para consultar el histórico de un activo en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosHistoricoData>> Activos_Deterioros_Historico_Consultar(int CodEmpresa, string placa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<ActivosHistoricoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosHistoricoData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select X.id_AddRet,x.fecha,x.MONTO,x.DESCRIPCION, rtrim(J.cod_justificacion) + '..' + J.descripcion as Justifica                                  
                                 ,A.nombre,P.cod_proveedor,P.descripcion as Proveedor, 'Revaluación' as TipoMov
                              from Activos_retiro_adicion X inner join Activos_Principal A on X.num_placa = A.num_placa
                                inner join Activos_justificaciones J on X.cod_justificacion = J.cod_justificacion
                                left join Activos_proveedores P on X.compra_proveedor = P.cod_proveedor
                                 where X.num_placa = @placa and X.Tipo ='D'";
                    result.Result = connection.Query<ActivosHistoricoData>(query, new
                    { placa }).ToList();
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
        /// Método para consultar el nombre de un activo en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_Deterioros_ActivosNombre_Consultar(int CodEmpresa, string placa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select nombre from Activos_Principal where num_placa =@placa";
                    result.Result = connection.Query<string>(query, new { placa }).FirstOrDefault();

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
        /// Método para eliminar un activo en estado de deterioro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="placa"></param>
        /// <param name="Id_AddRet"></param>
        /// <returns></returns>
        public ErrorDto Activos_Deterioros_Eliminar(int CodEmpresa, string usuario, string placa, int Id_AddRet)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete Activos_retiro_adicion where num_placa = @placa and Id_AddRet = @Id_AddRet";
                    connection.Execute(query, new { placa, Id_AddRet });


                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Deterioro, Placa: {placa}) Id: {Id_AddRet} ",
                        Movimiento = "Elimina - WEB",
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
        ///  Método para consultar el periodo de un activo 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {


            var result = new ErrorDto<DateTime>
            {
                Code = 0,
                Description = "Ok",
                Result = DateTime.Now,
            };
            try
            {
                result.Result = _mActivos.fxCntX_PeriodoActual(CodEmpresa, contabilidad);

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
