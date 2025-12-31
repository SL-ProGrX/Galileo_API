using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndPagoComisionDb
    {
        private readonly PortalDB _portalDB;

        public FrmFndPagoComisionDb(IConfiguration? config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PagoComision_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            const string sqlBancos = @"select B.id_banco as item,B.descripcion as descripcion
                    from tes_banco_asg T inner join Tes_Bancos B on T.id_banco = B.id_banco 
	                where T.nombre = @Usuario and B.Estado = 'A'";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                CodEmpresa,
                sqlBancos,
                new { Usuario });
        }

        /// <summary>
        /// Obtener lista de pago comision
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<FndPagoComisionVendedorData>> FND_PagoComision_Obtener(int CodEmpresa, FndPagoComisionFiltros Filtros)
        {
            var response = new ErrorDto<List<FndPagoComisionVendedorData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndPagoComisionVendedorData>()
            };

            try
            {
                var fechaInicio = (Filtros.fecha_inicio ?? DateTime.Today).Date;
                var fechaCorte = (Filtros.fecha_corte ?? DateTime.Today).Date.AddDays(1).AddSeconds(-1);
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                select 
                    V.cod_vendedor, V.cedula,V.nombre,V.cuenta_ahorros,V.Tipo_Pago 
                    ,V.cod_banco,V.minimo,V.porc_comision,sum(C.monto) as Monto, count(*) as Casos 
                from 
                    fnd_contratos C inner join fnd_vendedores V on C.cod_vendedor = V.cod_vendedor 
                where 
                    C.ind_comision = 0 and C.estado <> 'L'
                    and C.fecha_inicio between @FechaInicio and @FechaCorte
                    and V.aplica_comision = 1
                group by 
                    V.cod_vendedor, V.cedula,V.nombre,V.cuenta_ahorros,V.Tipo_Pago,V.cod_banco
                    ,V.minimo,V.porc_comision";

                var parametros = new
                {
                    FechaInicio = fechaInicio,
                    FechaCorte = fechaCorte
                };

                var lista = connection
                    .Query<FndPagoComisionVendedorData>(sql, parametros)
                    .ToList();

                foreach (var item in lista)
                {
                    item.monto_comision = item.monto > 0
                        ? Math.Round(
                            item.monto * (item.porc_comision / 100m),
                            2
                        )
                        : 0;
                }

                lista = lista.Where(x => x.monto > 0).ToList();

                response.Result = lista;
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
        /// Generar pago comision
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtros"></param>
        /// <param name="Vendedores"></param>
        /// <returns></returns>
        public ErrorDto FND_PagoComision_Generar(int CodEmpresa, FndPagoComisionFiltros Filtros, List<FndPagoComisionVendedorData> Vendedores)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                //vCuenta = cta_comisiones from fnd_parametros
                const string sqlCuentaComisiones = "SELECT cta_comisiones FROM fnd_parametros";
                var vCuenta = connection.QueryFirstOrDefault<string>(sqlCuentaComisiones);

                if (string.IsNullOrWhiteSpace(vCuenta))
                {
                    response.Code = -2;
                    response.Description = "No se encontró la cuenta de comisiones.";
                    return response;
                }

                //// var vFecha = DateTime.Now;
                //// var fechaDesde = Filtros.fecha_inicio.Date;
                //// var fechaHasta = Filtros.fecha_corte.Date.AddDays(1).AddSeconds(-1);

                ////int totalProcesados = 0;

                ////if (Vendedores != null)
                ////{
                ////Inicia Transacción
                ////foreach (var v in Vendedores)
                ////{
                ////    if (v.monto_comision <= 0)
                ////        continue;

                ////    // Armar el modelo para fxMaestroTesoreria
                ////    var maestroModel = new MaestroTesoreriaRequest
                ////    {
                ////        banco = v.cod_banco,    
                ////        tipoDocumento = v.tipo_pago,  
                ////        monto = v.monto_comision,
                ////        codigo = v.cod_vendedor,
                ////        beneficiario = v.cedula,
                ////        op = 0,
                ////        detalle1 = "FONDOS EXTRAORD.",
                ////        referencia = 0,
                ////        detalle2 = "PAGO COMISION",
                ////        cuenta = v.cuenta_ahorros, 
                ////        fecha = vFecha
                ////    };
                ////    var resMaestro = fxMaestroTesoreria(CodEmpresa, maestroModel);

                ////    if (resMaestro.Code != 0 || resMaestro.Result <= 0)
                ////    {
                ////        response.Code = -2;
                ////        response.Description = "Error al generar la transacción de tesorería: " + resMaestro.Description;
                ////        return response;
                ////    }
                ////    long lngSolicitud = resMaestro.Result;

                ////    // fxCuentaBanco
                ////    const string sqlCuentaBanco = @"SELECT ctaConta FROM Tes_Bancos WHERE id_banco = @Banco";
                ////    var cuentaBanco = connection.QueryFirstOrDefault<string>(
                ////        sqlCuentaBanco,
                ////        new { Banco = v.cod_banco }
                ////    )?.Trim();

                ////    if (string.IsNullOrWhiteSpace(cuentaBanco))
                ////    {
                ////        response.Code = -2;
                ////        response.Description = $"No se encontró la cuenta contable para el banco {v.cod_banco}.";
                ////        return response;
                ////    }

                ////    // sbCreaDetalle(lngSolicitud, fxCuentaBanco(...), montoComision, "H", 1)
                ////    var resDetH = sbCreaDetalle(
                ////        CodEmpresa,
                ////        (int)lngSolicitud, 
                ////        cuentaBanco,
                ////        v.monto_comision,
                ////        "H",
                ////        1
                ////    );

                ////    if (resDetH.Code != 0)
                ////    {
                ////        response.Code = -2;
                ////        response.Description = "Error al crear detalle (H): " + resDetH.Description;
                ////        return response;
                ////    }

                ////    // sbCreaDetalle(lngSolicitud, vCuenta, montoComision, "D", 2)
                ////    var resDetD = sbCreaDetalle(
                ////        CodEmpresa,
                ////        (int)lngSolicitud,
                ////        vCuenta,
                ////        v.monto_comision,
                ////        "D",
                ////        2
                ////    );

                ////    if (resDetD.Code != 0)
                ////    {
                ////        response.Code = -2;
                ////        response.Description = "Error al crear detalle (D): " + resDetD.Description;
                ////        return response;
                ////    }

                ////    //Actualiza Contratos indicando que ya se proceso la comisión para este vendedor
                ////    const string sqlUpdateContratos = @"
                ////        UPDATE ht_contratos
                ////        SET 
                ////            ind_comision = 1,
                ////            comision_fecha = @FechaComision,
                ////            comision_Tesoreria = @NSolicitud,
                ////            comision_monto = monto * (@PorcComision / 100.0)
                ////        WHERE 
                ////            cod_vendedor = @CodVendedor
                ////            AND ind_comision = 0
                ////            AND fecha_contrato BETWEEN @FechaInicio AND @FechaFin
                ////            AND estado <> 'L';
                ////    ";
                ////    connection.Execute(
                ////        sqlUpdateContratos,
                ////        new
                ////        {
                ////            FechaComision = vFecha,
                ////            NSolicitud = lngSolicitud,
                ////            PorcComision = v.porc_comision,
                ////            CodVendedor = v.cod_vendedor,
                ////            FechaInicio = fechaDesde,
                ////            FechaFin = fechaHasta
                ////        }
                ////    );

                ////        totalProcesados++;
                ////    }
                ////Cierra Transacción
                ////}

                ////if (totalProcesados > 0)
                ////{
                ////    response.Description = "Comisiones generadas a Tesorería...";
                ////}
                ////else
                ////{
                ////    response.Description = "No se generaron comisiones (ningún registro seleccionado o con monto > 0).";
                ////}
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtener maestro tesoreria
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<long> fxMaestroTesoreria(int CodEmpresa, MaestroTesoreriaRequest model)
        {
            var response = new ErrorDto<long>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlInsert = @"
                    INSERT INTO Tes_Transacciones
                    (
                        id_banco, tipo, codigo, beneficiario, monto, fecha_solicitud, estado, estadoi,
                        modulo, submodulo, cta_ahorros, detalle1, detalle2, referencia, op, genera, actualiza
                    )
                    VALUES
                    (
                        @Banco, @TipoDocumento, @Codigo, @Beneficiario, @Monto, @Fecha, 'P', 'P',
                        'TE', 'N', @Cuenta, @Detalle1, @Detalle2, @Referencia, @Op, 'S', 'S'
                    );

                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS NSolicitud;
                ";

                long nsolicitud = connection.ExecuteScalar<long>(
                    sqlInsert,
                    new
                    {
                        Banco = model.banco,
                        TipoDocumento = model.tipoDocumento,
                        Codigo = model.codigo.Trim(),
                        Beneficiario = model.beneficiario,
                        Monto = model.monto,
                        Fecha = model.fecha,
                        Cuenta = model.cuenta,
                        Detalle1 = model.detalle1,
                        Detalle2 = model.detalle2,
                        Referencia = model.referencia,
                        Op = model.op
                    }
                );

                const string sqlValidar = @"SELECT TOP 1 NSolicitud FROM Tes_Transacciones 
                    WHERE NSolicitud = @Solicitud AND RTRIM(LTRIM(Codigo)) = @Codigo
                ";
                long validado = connection.ExecuteScalar<long?>(
                    sqlValidar,
                    new { Solicitud = nsolicitud, Codigo = model.codigo.Trim() }
                ) ?? 0;

                if (validado == 0)
                {
                    const string sqlFallback = @"SELECT MAX(NSolicitud) FROM Tes_Transacciones
                        WHERE Codigo = @Codigo AND OP = @Op";
                    validado = connection.ExecuteScalar<long?>(
                        sqlFallback,
                        new { Codigo = model.codigo.Trim(), Op = model.op }
                    ) ?? 0;
                }

                response.Result = validado;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }

            return response;
        }

        /// <summary>
        /// Crear detalle asiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="NSolicitud"></param>
        /// <param name="CuentaContable"></param>
        /// <param name="Monto"></param>
        /// <param name="DebeHaber"></param>
        /// <param name="Linea"></param>
        /// <returns></returns>
        public ErrorDto<bool> sbCreaDetalle(int CodEmpresa, int NSolicitud, string CuentaContable, decimal Monto, string DebeHaber, int Linea)
        {
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"insert Tes_Trans_Asiento(nsolicitud,cuenta_contable,monto,debehaber,linea)
                    VALUES ( @NSolicitud,  @CuentaContable, @Monto, @DebeHaber, @Linea);";

                connection.Execute(sql, new
                {
                    NSolicitud,
                    CuentaContable = CuentaContable.Trim(),
                    Monto,
                    DebeHaber,
                    Linea
                });

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }
    }
}
