using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using System.Text;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndRemesasDb
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDB;
        private readonly int vModulo = 18;

        public FrmFndRemesasDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDB = new MSecurityMainDb(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDB.Bitacora(data);
        }

        /// <summary>
        /// Obtener remesa por codigo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto<FndRemesasData> FND_Remesa_Obtener(int CodEmpresa, int Remesa)
        {
            var response = new ErrorDto<FndRemesasData>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = "SELECT * FROM fnd_remesas WHERE remesa = @Remesa";
                var data = connection.QueryFirstOrDefault<FndRemesasData>(query, new { Remesa });

                if (data == null)
                {
                    response.Code = -2;
                    response.Description = "No se encontró la remesa indicada.";
                    return response; 
                }

                const string queryTotal = @"select isnull(sum(aportes_liq + rendi_liq),0) as Total from fnd_liquidacion 
                    where consec in (select consec from fnd_remesa_asg where remesa = @Remesa)";
                data.total = connection.ExecuteScalar<decimal>(queryTotal, new { Remesa });

                response.Result = data;
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
        /// Obtener lista de remesas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="TabIndex"></param>
        /// <param name="Lineas"></param>
        /// <returns></returns>
        public ErrorDto<List<FndRemesasData>> FND_Remesas_Lista_Obtener(int CodEmpresa, int TabIndex, int Lineas = 50)
        {
            string query = TabIndex switch
            {
                // TAB 0 - Remesas 
                0 => "SELECT TOP (@Top) * FROM fnd_remesas ORDER BY fecha DESC",

                // TAB 1 - Carga 
                1 => "SELECT * FROM fnd_remesas WHERE estado IN ('A','P') ORDER BY fecha DESC",

                // TAB 2 - Reportes 
                2 => "SELECT TOP (@Top) * FROM fnd_remesas ORDER BY fecha DESC",

                // Default → retorna vacío
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(query))
            {
                var response = new ErrorDto<List<FndRemesasData>>();
                response.Code = -2;
                response.Description = "Opción inválida.";
                response.Result = null;
                return response;
            }

            return DbHelper.ExecuteListQuery<FndRemesasData>(
                _portalDB,
                CodEmpresa,
                query,
                new { Top = Lineas });
        }

        /// <summary>
        /// Guardar remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="RemesaData"></param>
        /// <returns></returns>
        public ErrorDto FND_Remesas_Guardar(int CodEmpresa, FndRemesasData RemesaData)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var fechaInicio = (RemesaData.fecha_inicio ?? DateTime.Today).Date;
                var fechaCorte = (RemesaData.fecha_corte ?? DateTime.Today).Date.AddDays(1).AddSeconds(-1);
                if (RemesaData.remesa == 0)
                {
                    const string sqlUltimo = "select isnull(max(remesa),0) + 1 as Ultimo from fnd_remesas";
                    int nuevoCodigo = connection.ExecuteScalar<int>(sqlUltimo);

                    RemesaData.remesa = nuevoCodigo;

                    const string sqlInsert = @"insert fnd_remesas(remesa, usuario, fecha, estado, fecha_inicio, fecha_corte, notas) 
                        values(@Remesa, @Usuario, GETDATE(), 'A', @FechaInicio, @FechaCorte, @Notas)";
                    var parametrosInsert = new
                    {
                        Remesa = RemesaData.remesa,
                        Usuario = RemesaData.usuario,
                        FechaInicio = fechaInicio,
                        FechaCorte = fechaCorte,
                        Notas = RemesaData.notas ?? string.Empty
                    };
                    connection.Execute(sqlInsert, parametrosInsert);

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = RemesaData.usuario.ToUpper(),
                        DetalleMovimiento = "Remesa de Planes de Ahorro : " + RemesaData.remesa,
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });
                }
                else
                {
                    const string sqlEstado = "SELECT estado FROM fnd_remesas WHERE remesa = @Remesa";
                    var estadoActual = connection.ExecuteScalar<string>(sqlEstado, new { Remesa = RemesaData.remesa });

                    if (string.Equals(estadoActual, "C", StringComparison.OrdinalIgnoreCase))
                    {
                        response.Code = -2;
                        response.Description = "No se puede modificar la remesa, porque ya fue cerrada...";
                        return response;
                    }

                    const string sqlUpdate = @"update fnd_remesas set usuario = @Usuario, 
                        fecha_inicio = @FechaInicio, fecha_corte  = @FechaCorte, notas = @Notas 
                        where remesa = @Remesa";
                    var parametrosUpdate = new
                    {
                        Remesa = RemesaData.remesa,
                        Usuario = RemesaData.usuario,
                        FechaInicio = fechaInicio,
                        FechaCorte = fechaCorte,
                        Notas = RemesaData.notas ?? string.Empty
                    };
                    connection.Execute(sqlUpdate, parametrosUpdate);

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = RemesaData.usuario.ToUpper(),
                        DetalleMovimiento = "Remesa de Planes de Ahorro : " + RemesaData.remesa,
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
                }
                response.Code = RemesaData.remesa;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Eliminar remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto FND_Remesas_Eliminar(int CodEmpresa, int Remesa, string Usuario)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"delete fnd_remesas where remesa = @Remesa";
                connection.Execute(query, new { Remesa });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = Usuario.ToUpper(),
                    DetalleMovimiento = "Remesa de Planes de Ahorro  : " + Remesa,
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtener bancos asociados a la remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Remesas_Bancos_Obtener(int CodEmpresa, int Remesa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlFechas = "SELECT fecha_inicio, fecha_corte FROM fnd_remesas WHERE remesa = @Remesa";

                var fechas = connection.QueryFirstOrDefault<(DateTime fecha_inicio, DateTime fecha_corte)>(
                    sqlFechas, new { Remesa });

                if (fechas == default)
                {
                    response.Code = -2;
                    response.Description = "No se encontró la remesa indicada.";
                    return response;
                }

                var fechaInicio = fechas.fecha_inicio.Date; 
                var fechaCorteFin = fechas.fecha_corte.Date.AddDays(1).AddTicks(-1);

                const string sqlBancos = @"select B.id_banco as item,B.descripcion 
                    FROM Fnd_Liquidacion L inner join Tes_Bancos B on L.cod_Banco = B.id_banco
                    WHERE L.fecha BETWEEN @FechaInicio AND @FechaCorteFin
                    AND L.consec NOT IN (SELECT consec FROM fnd_remesa_asg)
                    GROUP BY B.id_banco,B.descripcion";

                var bancos = connection.Query<DropDownListaGenericaModel>(
                    sqlBancos,
                    new
                    {
                        FechaInicio = fechaInicio,
                        FechaCorteFin = fechaCorteFin
                    }
                ).ToList();

                response.Result = bancos;
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
        /// Obtener carga de la remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <param name="Banco"></param>
        /// <returns></returns>
        public ErrorDto<List<FndRemesasCargaData>> FND_Remesa_Carga_Obtener(int CodEmpresa, int Remesa, int Banco)
        {
            var response = new ErrorDto<List<FndRemesasCargaData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndRemesasCargaData>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlFechas = "SELECT fecha_inicio, fecha_corte FROM fnd_remesas WHERE remesa = @Remesa";
                var fechas = connection.QueryFirstOrDefault<(DateTime fecha_inicio, DateTime fecha_corte)>(
                    sqlFechas, new { Remesa });

                if (fechas == default)
                {
                    response.Code = -2;
                    response.Description = "No se encontró la remesa indicada.";
                    return response;
                }

                var fechaInicio = fechas.fecha_inicio.Date;
                var fechaCorteFin = fechas.fecha_corte.Date.AddDays(1).AddTicks(-1); 

                const string sql = @"SELECT 
                    L.consec,S.cedula,S.nombre,L.cod_plan,L.cod_operadora,L.cod_contrato,(L.aportes_liq+L.rendi_liq) as Monto,
                    L.Fecha,L.usuario,L.cta_ahorros,L.tipo
                FROM fnd_liquidacion L
                INNER JOIN fnd_contratos C
                    ON L.cod_operadora = C.cod_operadora
                   AND L.cod_plan      = C.cod_plan
                   AND L.cod_contrato  = C.cod_contrato
                INNER JOIN socios S
                    ON C.cedula = S.cedula
                WHERE
                    L.fecha BETWEEN @FechaInicio AND @FechaCorteFin
                    AND L.traspaso_tesoreria IS NOT NULL
                    AND L.consec NOT IN (SELECT consec FROM fnd_remesa_asg)
                    AND (@IdBanco IS NULL OR L.cod_banco = @IdBanco)
                ORDER BY L.consec";

                response.Result = connection.Query<FndRemesasCargaData>(sql,
                    new
                    {
                        FechaInicio = fechaInicio,
                        FechaCorteFin = fechaCorteFin,
                        IdBanco = (Banco == 0 ? (object?)DBNull.Value : Banco)
                    }
                ).ToList();
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
        /// Procesar carga de la remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <param name="Usuario"></param>
        /// <param name="ConsecSeleccionados"></param>
        /// <returns></returns>
        public ErrorDto FND_Remesas_Carga_Procesar(int CodEmpresa, int Remesa, string Usuario, List<int> ConsecSeleccionados)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlValidaRemesa = @"SELECT COUNT(*) FROM fnd_remesas 
                    WHERE remesa = @Remesa AND estado IN ('A', 'P')";
                var existe = connection.QueryFirstOrDefault<int>(sqlValidaRemesa, new { Remesa });

                if (existe == 0)
                {
                    response.Code = -2;
                    response.Description = "La remesa actual ya se encuentra cerrada...";
                    return response;
                }

                const string sqlInsert = "INSERT INTO fnd_remesa_asg(remesa, consec) VALUES (@Remesa, @Consec);";
                if (ConsecSeleccionados != null)
                {
                    foreach (var consec in ConsecSeleccionados)
                    {
                        connection.Execute(
                            sqlInsert, new { Remesa, Consec = consec }
                        );
                    }
                }

                const string sqlUpdateRemesa = "UPDATE fnd_remesas SET estado = 'P' WHERE remesa = @Remesa";
                connection.Execute(sqlUpdateRemesa, new { Remesa });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = Usuario.ToUpper(),
                    DetalleMovimiento = "Remesa de Fondos : " + Remesa,
                    Movimiento = "Genera - WEB",
                    Modulo = vModulo
                });

                response.Description = "Proceso realizado satisfactoriamente...";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Cerrar carga de la remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto FND_Remesas_Carga_Cerrar(int CodEmpresa, int Remesa, string Usuario)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlValidaRemesa = @"SELECT COUNT(*) FROM fnd_remesas 
                    WHERE remesa = @Remesa AND estado IN ('A', 'P');";
                var existe = connection.QueryFirstOrDefault<int>(sqlValidaRemesa, new { Remesa });

                if (existe == 0)
                {
                    response.Code = -2;
                    response.Description = "La remesa actual ya se encuentra cerrada...";
                    return response;
                }

                const string sqlUpdateRemesa = @"UPDATE fnd_remesas 
                    SET estado = 'C' WHERE remesa = @Remesa;";
                connection.Execute(sqlUpdateRemesa, new { Remesa });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = Usuario.ToUpper(),
                    DetalleMovimiento = "Remesa de Fondos Remesa : " + Remesa,
                    Movimiento = "Genera - WEB",
                    Modulo = vModulo
                });

                response.Description = "Remesa cerrada satisfactoriamente...";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtener consulta retiro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Consec"></param>
        /// <returns></returns>
        public ErrorDto<string> FND_Remesas_ConsultaRetiro_Obtener(int CodEmpresa, int Consec)
        {
            var response = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = "select A.* from fnd_remesas A inner join fnd_remesa_asg X on A.remesa = X.remesa where consec = @Consec;";
                var data = connection.QueryFirstOrDefault<FndRemesasData>(sql, new { Consec });

                if (data == null)
                {
                    response.Result = "** No se encontró retiro/liq. en las remesas registradas **";
                } 
                else
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Remesa\t ...: {data.remesa}");
                    sb.AppendLine($"Fecha\t ...: {data.fecha:dd/MM/yyyy hh:mm:ss tt}");
                    sb.Append($"Usuario\t ...: {data.usuario}");
                    response.Result = sb.ToString();
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