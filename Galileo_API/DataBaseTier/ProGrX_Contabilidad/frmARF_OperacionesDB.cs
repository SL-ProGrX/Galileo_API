using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmARF_OperacionesDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly int vModulo = 20;

        public FrmARF_OperacionesDB(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmARF_OperacionesDB(PortalDB portalDb, MSecurityMainDb mSecurityMainDb)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mSecurityMainDb;
        }

        /// <summary>
        /// Divisas listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Listar(int codEmpresa)
        {
            const string sql = @"
                select COD_DIVISA as item, rtrim(DESCRIPCION) as descripcion
                from vSys_Divisas
                order by DESCRIPCION";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Operaciones listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ArfOperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            const string sql = @"
                select top 200
                    O.Operacion as operacion,
                    rtrim(isnull(O.COD_LOCAL, '')) as cod_local,
                    rtrim(isnull(U.Descripcion, '')) as unidad_desc,
                    isnull(O.COD_ACREEDOR, 0) as cod_acreedor,
                    rtrim(isnull(A.Descripcion, '')) as arrendatario_desc,
                    isnull(O.ESTADO, '') as estado,
                    case isnull(O.ESTADO, '')
                        when 'R' then 'Recibida'
                        when 'A' then 'Activa'
                        when 'C' then 'Cerrada'
                        when 'N' then 'Anulada'
                        when 'P' then 'Pendiente'
                        else 'Pendiente'
                    end as estado_desc
                from ARF_OPERACIONES O
                left join ARF_UNIDADES U on U.COD_LOCAL = O.COD_LOCAL
                left join ARF_ACREEDORES A on A.COD_ACREEDOR = O.COD_ACREEDOR
                order by O.Operacion desc";

            return DbHelper.ExecuteListQuery<ArfOperacionBusquedaDto>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Consultar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<ArfOperacionRegistroDto> Consultar(int codEmpresa, int operacion)
        {
            const string sql = @"
                select
                    O.Operacion as operacion,
                    rtrim(isnull(O.COD_LOCAL, '')) as cod_local,
                    rtrim(isnull(U.Descripcion, '')) as unidad_desc,
                    isnull(O.COD_ACREEDOR, 0) as cod_acreedor,
                    rtrim(isnull(A.Descripcion, '')) as arrendatario_desc,
                    rtrim(isnull(O.COD_DIVISA, '')) as cod_divisa,
                    rtrim(isnull(D.DESCRIPCION, '')) as divisa_desc,
                    rtrim(isnull(O.PERIODICIDAD, '')) as periodicidad,
                    case isnull(O.PERIODICIDAD, '')
                        when 'M' then 'Mensual'
                        when 'T' then 'Trimestral'
                        when 'S' then 'Semestral'
                        when 'A' then 'Anual'
                        else ''
                    end as periodicidad_desc,
                    isnull(O.CUOTA, 0) as cuota,
                    isnull(O.PLAZO, 1) as plazo,
                    isnull(O.TASA_DESCUENTO, 0) as tasa_descuento,
                    isnull(O.TASA_INTERES, 0) as tasa_interes,
                    rtrim(isnull(O.INCREMENTO_TIPO, '')) as incremento_tipo,
                    case isnull(O.INCREMENTO_TIPO, '')
                        when 'M' then 'Monto'
                        when 'P' then 'Porcentaje'
                        else ''
                    end as incremento_tipo_desc,
                    isnull(O.INCREMENTO_VALOR, 0) as incremento_valor,
                    isnull(O.DEPOSITO_GARANTIA_MONTO, 0) as deposito_garantia_monto,
                    cast(isnull(O.DEPOSITO_GARANTIA_IND, 0) as bit) as deposito_garantia_ind,
                    O.FECHA_INICIO as fecha_inicio,
                    O.FECHA_FINALIZA as fecha_finaliza,
                    isnull(O.NOTAS, '') as notas,
                    rtrim(isnull(O.ESTADO, '')) as estado,
                    case isnull(O.ESTADO, '')
                        when 'R' then 'Recibida'
                        when 'A' then 'Activa'
                        when 'C' then 'Cerrada'
                        when 'N' then 'Anulada'
                        when 'P' then 'Pendiente'
                        else 'Pendiente'
                    end as estado_desc,
                    rtrim(isnull(O.REGISTRO_USUARIO, '')) as registro_usuario,
                    O.REGISTRO_FECHA as registro_fecha,
                    rtrim(isnull(O.ACTIVA_USUARIO, '')) as activa_usuario,
                    O.ACTIVA_FECHA as activa_fecha
                from ARF_OPERACIONES O
                left join ARF_UNIDADES U on U.COD_LOCAL = O.COD_LOCAL
                left join ARF_ACREEDORES A on A.COD_ACREEDOR = O.COD_ACREEDOR
                left join vSys_Divisas D on D.COD_DIVISA = O.COD_DIVISA
                where O.Operacion = @operacion";

            var result = DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                new ArfOperacionRegistroDto(),
                new { operacion });

            result.Result ??= null;
            return result;
        }

        /// <summary>
        /// Scroll
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="direccion"></param>
        /// <returns></returns>
        public ErrorDto<ArfOperacionRegistroDto> Scroll(int codEmpresa, int operacion, int direccion)
        {
            var sql = "select top 1 Operacion from ARF_OPERACIONES ";

            if (direccion > 0)
            {
                sql += "where Operacion > @operacion order by Operacion asc";
            }
            else
            {
                sql += "where Operacion < @operacion order by Operacion desc";
            }

            var idResp = DbHelper.ExecuteSingleQuery<int?>(_portalDb, codEmpresa, sql, null, new { operacion });

            if (idResp.Result is null || idResp.Result <= 0)
            {
                return new ErrorDto<ArfOperacionRegistroDto>
                {
                    Code = -2,
                    Description = "No se encontraron más registros",
                    Result = null
                };
            }

            return Consultar(codEmpresa, idResp.Result.Value);
        }

        /// <summary>
        /// Arrendadores listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Arrendadores_Listar(int codEmpresa)
        {
            const string sql = @"
                select
                    cast(COD_ACREEDOR as varchar(20)) as item,
                    rtrim(Descripcion) as descripcion
                from ARF_ACREEDORES
                order by COD_ACREEDOR";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Unidades listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Listar(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(COD_LOCAL) as item,
                    rtrim(Descripcion) as descripcion
                from ARF_UNIDADES
                order by COD_LOCAL";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Guardar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<ArfOperacionGuardarResponseDto> Guardar(int codEmpresa, ArfOperacionGuardarRequestDto request)
        {
            var response = new ErrorDto<ArfOperacionGuardarResponseDto>();

            using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                var notas = (request.notas ?? string.Empty).Trim();
                var estadoActual = request.operacion.HasValue && request.operacion.Value > 0
                    ? cn.ExecuteScalar<string?>(
                        "select isnull(ESTADO, 'P') from ARF_OPERACIONES where Operacion = @operacion",
                        new { operacion = request.operacion.Value },
                        tx)
                    : "P";

                if (request.cod_acreedor <= 0)
                    throw new Exception("No se ha especificado un Arrendador.");

                if (string.IsNullOrWhiteSpace(request.cod_local))
                    throw new Exception("No se ha especificado una Unidad/Local.");

                if (request.cuota <= 0)
                    throw new Exception("El Monto no es válido.");

                if (request.tasa_descuento < 0 || request.tasa_descuento > 100)
                    throw new Exception("La Tasa Descuento no es válida.");

                if (request.tasa_interes < 0 || request.tasa_interes > 100)
                    throw new Exception("La Tasa de Interés no es válida.");

                if (request.plazo <= 0)
                    throw new Exception("El Plazo no es válido.");

                if (request.incremento_tipo == "P" && (request.incremento_valor < 0 || request.incremento_valor > 100))
                    throw new Exception("El Porcentaje de Incremento Anual no es válido.");

                if (request.incremento_tipo == "M" && request.incremento_valor < 0)
                    throw new Exception("El Monto del Incremento Anual no es válido.");

                if (request.deposito_garantia_monto < 0)
                    throw new Exception("El dato del depósito de garantía no es válido.");

                if (request.fecha_inicio >= request.fecha_finaliza)
                    throw new Exception("Rango de Fechas Erróneo, verificar.");

                if (!string.Equals(estadoActual, "R", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(estadoActual, "P", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Esta Operación no puede ser modificada porque no se encuentra en estado de recibido.");

                int operacion;

                if (!request.operacion.HasValue || request.operacion.Value <= 0)
                {
                    const string insertSql = @"
                        insert into ARF_OPERACIONES
                        (
                            COD_ACREEDOR, COD_LOCAL, TASA_DESCUENTO, TASA_INTERES, PERIODICIDAD,
                            CUOTA, PLAZO, FECHA_INICIO, FECHA_FINALIZA, CORTE_ULTIMO, PAGO_PROXIMO,
                            NOTAS, ESTADO, DEPOSITO_GARANTIA_MONTO, DEPOSITO_GARANTIA_IND,
                            INCREMENTO_TIPO, INCREMENTO_VALOR, VALOR_NOMINAL, DEPRECIACION_ACUM,
                            VALOR_LIBROS, VALOR_INICIAL, COD_DIVISA, REGISTRO_FECHA, REGISTRO_USUARIO
                        )
                        output inserted.Operacion
                        values
                        (
                            @cod_acreedor, @cod_local, @tasa_descuento, @tasa_interes, @periodicidad,
                            @cuota, @plazo, @fecha_inicio, @fecha_finaliza, null, null,
                            @notas, 'R', @deposito_garantia_monto, @deposito_garantia_ind,
                            @incremento_tipo, @incremento_valor, 0, 0, 0, 0,
                            @cod_divisa, getdate(), @usuario
                        )";

                    operacion = cn.ExecuteScalar<int>(insertSql, new
                    {
                        request.cod_acreedor,
                        cod_local = request.cod_local.Trim(),
                        request.tasa_descuento,
                        request.tasa_interes,
                        request.periodicidad,
                        request.cuota,
                        request.plazo,
                        request.fecha_inicio,
                        request.fecha_finaliza,
                        notas,
                        request.deposito_garantia_monto,
                        deposito_garantia_ind = request.deposito_garantia_ind ? 1 : 0,
                        request.incremento_tipo,
                        request.incremento_valor,
                        request.cod_divisa,
                        request.usuario
                    }, tx);

                    RegistrarBitacora(
                        codEmpresa,
                        request.usuario,
                        "Registra - WEB",
                        $"Registro de Operación de Arrendamiento No.: {operacion}");
                }
                else
                {
                    const string updateSql = @"
                        update ARF_OPERACIONES
                        set
                            COD_ACREEDOR = @cod_acreedor,
                            COD_LOCAL = @cod_local,
                            TASA_DESCUENTO = @tasa_descuento,
                            TASA_INTERES = @tasa_interes,
                            PERIODICIDAD = @periodicidad,
                            CUOTA = @cuota,
                            PLAZO = @plazo,
                            FECHA_INICIO = @fecha_inicio,
                            FECHA_FINALIZA = @fecha_finaliza,
                            CORTE_ULTIMO = null,
                            PAGO_PROXIMO = null,
                            NOTAS = @notas,
                            ESTADO = 'R',
                            DEPOSITO_GARANTIA_MONTO = @deposito_garantia_monto,
                            DEPOSITO_GARANTIA_IND = @deposito_garantia_ind,
                            INCREMENTO_TIPO = @incremento_tipo,
                            INCREMENTO_VALOR = @incremento_valor,
                            VALOR_NOMINAL = 0,
                            DEPRECIACION_ACUM = 0,
                            VALOR_LIBROS = 0,
                            VALOR_INICIAL = 0,
                            COD_DIVISA = @cod_divisa
                        where Operacion = @operacion";

                    cn.Execute(updateSql, new
                    {
                        operacion = request.operacion.Value,
                        request.cod_acreedor,
                        cod_local = request.cod_local.Trim(),
                        request.tasa_descuento,
                        request.tasa_interes,
                        request.periodicidad,
                        request.cuota,
                        request.plazo,
                        request.fecha_inicio,
                        request.fecha_finaliza,
                        notas,
                        request.deposito_garantia_monto,
                        deposito_garantia_ind = request.deposito_garantia_ind ? 1 : 0,
                        request.incremento_tipo,
                        request.incremento_valor,
                        request.cod_divisa
                    }, tx);

                    operacion = request.operacion.Value;

                    RegistrarBitacora(
                        codEmpresa,
                        request.usuario,
                        "Modifica - WEB",
                        $"Modifica Operación de Arrendamiento No.: {operacion}");
                }

                cn.Execute(
                    "spARF_Operacion_Plan_Add",
                    new { txtOperacion = operacion, Recalcular = 1 },
                    tx,
                    commandType: CommandType.StoredProcedure);

                tx.Commit();

                response.Result = new ArfOperacionGuardarResponseDto
                {
                    operacion = operacion,
                    mensaje = "Información guardada satisfactoriamente."
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Activar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Activar(int codEmpresa, ArfOperacionActivarRequestDto request)
        {
            var response = new ErrorDto();

            using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));
            cn.Open();

            try
            {
                cn.Execute(
                    "spARF_Operacion_Activacion",
                    new
                    {
                        Operacion = request.operacion,
                        Usuario = request.usuario
                    },
                    commandType: CommandType.StoredProcedure);

                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    "Registra - WEB",
                    $"Activación de Operación de Arrendamiento No.: {request.operacion}");

                response.Code = 0;
                response.Description = "Activación aplicada satisfactoriamente.";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Plan listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<ArfOperacionPlanDto>> Plan_Listar(int codEmpresa, int operacion)
        {
            const string sql = @"exec spARF_Operacion_Plan @operacion";
            return DbHelper.ExecuteListQuery<ArfOperacionPlanDto>(_portalDb, codEmpresa, sql, new { operacion });
        }

        /// <summary>
        /// Cierres listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<ArfOperacionCierreDto>> Cierres_Listar(int codEmpresa, int operacion)
        {
            const string sql = @"
                select
                    CORTE as corte,
                    CUOTA as cuota,
                    DEPRECIACION_GASTO as depreciacion_gasto,
                    DEPRECIACION_ACUM as depreciacion_acum,
                    VALOR_LIBROS as valor_libros,
                    PASIVO as pasivo,
                    PAGO_PROXIMO as pago_proximo,
                    CORTE_ULTIMO as corte_ultimo
                from vARF_Cierre_Operacion_Consulta
                where OPERACION = @operacion
                order by CORTE desc";

            return DbHelper.ExecuteListQuery<ArfOperacionCierreDto>(_portalDb, codEmpresa, sql, new { operacion });
        }

        /// <summary>
        /// Asientos main listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaCorte"></param>
        /// <returns></returns>
        public ErrorDto<List<ArfOperacionAsientoMainDto>> AsientosMain_Listar(
            int codEmpresa,
            int operacion,
            DateTime fechaInicio,
            DateTime fechaCorte)
        {
            const string sql = @"
                exec spARF_Asientos_Consulta
                    @pInicio,
                    @pCorte,
                    @pFiltro,
                    @pDetalle,
                    @pCuenta";

            return DbHelper.ExecuteListQuery<ArfOperacionAsientoMainDto>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    pInicio = fechaInicio.ToString("yyyy-MM-dd 00:00:00"),
                    pCorte = fechaCorte.ToString("yyyy-MM-dd 23:59:59"),
                    pFiltro = "",
                    pDetalle = $"Operación: {operacion}",
                    pCuenta = ""
                });
        }

        /// <summary>
        /// Asiento detalle listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="tipoAsiento"></param>
        /// <param name="numAsiento"></param>
        /// <returns></returns>
        public ErrorDto<List<ArfOperacionAsientoDetalleDto>> AsientoDetalle_Listar(
            int codEmpresa,
            int codContabilidad,
            string tipoAsiento,
            string numAsiento)
        {
            const string sql = @"
                select *
                from vARF_ASIENTOS_DETALLE
                where COD_CONTABILIDAD = @codContabilidad
                  and tipo_asiento = @tipoAsiento
                  and num_asiento = @numAsiento
                order by Linea_Id";

            return DbHelper.ExecuteListQuery<ArfOperacionAsientoDetalleDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { codContabilidad, tipoAsiento, numAsiento });
        }

        /// <summary>
        /// Cambios listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<ArfOperacionCambioDto>> Cambios_Listar(int codEmpresa, int operacion)
        {
            const string sql = @"
                select
                    Id_Cambio as id_cambio,
                    Operacion as operacion,
                    Tipo_Cambio as tipo_cambio,
                    Periodo_Afecta as periodo_afecta,
                    V_Anterior as v_anterior,
                    V_Actual as v_actual,
                    Registro_Fecha as registro_fecha,
                    Registro_Usuario as registro_usuario,
                    Notas as notas
                from ARF_OPERACIONES_CAMBIOS
                where Operacion = @operacion
                order by Registro_Fecha desc";

            return DbHelper.ExecuteListQuery<ArfOperacionCambioDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { operacion });
        }

        /// <summary>
        /// Cierre actual obtener
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<ArfOperacionFiniquitoPreviewDto> CierreActual_Obtener(int codEmpresa, int operacion)
        {
            const string sql = @"exec spARF_Cierre_Actual @operacion";
            var result = DbHelper.ExecuteSingleQuery(_portalDb, codEmpresa, sql, new ArfOperacionFiniquitoPreviewDto(), new { operacion });
            result.Result ??= null;
            return result;
        }

        /// <summary>
        /// Cambio aplicar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Cambio_Aplicar(int codEmpresa, ArfOperacionCambioRequestDto request)
        {
            const string sql = @"exec spARF_Operacion_Cambios
                @Operacion, @Usuario, @TasaDescuento, @TasaInteres, @Monto, @Plazo, @Notas, @Periodo";

            return EjecutarProcesoOperacion(
                codEmpresa,
                sql,
                new
                {
                    Operacion = request.operacion,
                    Usuario = (request.usuario ?? string.Empty).Trim(),
                    TasaDescuento = request.tasa_descuento,
                    TasaInteres = request.tasa_interes,
                    Monto = request.monto,
                    Plazo = request.plazo,
                    Notas = (request.notas ?? string.Empty).Trim(),
                    Periodo = request.periodo
                },
                request.usuario,
                $"Cambio de Condiciones de Operación de Arrendamiento No.: {request.operacion}",
                "Cambios aplicados satisfactoriamente.");
        }

        /// <summary>
        /// Finiquito aplicar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Finiquito_Aplicar(int codEmpresa, ArfOperacionFiniquitoRequestDto request)
        {
            const string sql = @"exec spARF_Operacion_Finiquito @Operacion, @Usuario, @Notas, @Periodo";

            return EjecutarProcesoOperacion(
                codEmpresa,
                sql,
                new
                {
                    Operacion = request.operacion,
                    Usuario = (request.usuario ?? string.Empty).Trim(),
                    Notas = (request.notas ?? string.Empty).Trim(),
                    Periodo = request.periodo
                },
                request.usuario,
                $"Finiquito de la Operación de Arrendamiento No.: {request.operacion}",
                "Finiquito aplicado satisfactoriamente.");
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalleMovimiento)
        {
            _mSecurityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                Movimiento = movimiento,
                Modulo = vModulo,
                DetalleMovimiento = detalleMovimiento
            });
        }

        private ErrorDto EjecutarProcesoOperacion(
            int codEmpresa,
            string sql,
            object parametros,
            string usuario,
            string detalleBitacora,
            string mensajeExito)
        {
            try
            {
                DbHelper.ExecuteSingleQuery<dynamic>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    null,
                    parametros);

                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    "Aplica",
                    detalleBitacora);

                return new ErrorDto
                {
                    Code = 0,
                    Description = mensajeExito
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }
    }
}
