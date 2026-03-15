using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXPlantillaRateGenDb
    {
        private readonly PortalDB _portalDb;
        private readonly MCntXCalculosDb _mCalculos;

        public FrmCntXPlantillaRateGenDb(IConfiguration config)
            : this(new PortalDB(config), new MCntXCalculosDb(config)) { }

        public FrmCntXPlantillaRateGenDb(PortalDB portalDb, MCntXCalculosDb mCalculos)
        {
            _portalDb = portalDb;
            _mCalculos = mCalculos;
        }

        /// <summary>
        /// Obtiene la lista de plantillas 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXPlantillaRate_Lista_Obtener(int codEmpresa, int codConta)
        {
            var sql = @"SELECT cod_plantilla AS item, descripcion
                FROM CntX_Plantilla_Rate WHERE cod_contabilidad = @codConta";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { codConta });
        }

        /// <summary>
        /// Obtiene el detalle de una plantilla seleccionada
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codPlantilla"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXPlantillaRateDetalleData>> CntXPlantillaRate_Detalle_Obtener(int codEmpresa, int codConta, int codPlantilla)
        {
            var sql = @"select D.*,c.descripcion,U.descripcion as UniDes 
                from CntX_Plantilla_Rate_Detalle D inner join CntX_Cuentas C on D.cod_contabilidad = C.cod_contabilidad 
                and D.cod_cuenta = C.cod_cuenta 
                inner join CntX_Unidades U on D.cod_contabilidad = U.cod_contabilidad and D.cod_unidad = U.cod_unidad 
                Where D.cod_contabilidad = @codConta
                and D.cod_plantilla = @codPlantilla
                order by D.num_linea";
            return DbHelper.ExecuteListQuery<CntXPlantillaRateDetalleData>(_portalDb, codEmpresa, sql, new { codConta, codPlantilla });
        }

        /// <summary>
        /// Genera asiento de plantilla
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXPlantillaRate_Generar(int codEmpresa, CntXPlantillaRateGenerarRequest request)
        {
            try
            {
                var periodoExiste = _mCalculos.FxCntX_PeriodoVerifica(codEmpresa, request.cod_contabilidad, request.periodo_anio, request.periodo_mes);

                if (!periodoExiste)
                    return ErrorResultPlantilla("El Periodo Actual se Encuentra Cerrado o no se ha creado, verifique...");

                var plantilla = CntXPlantillaRate_Obtener(codEmpresa, request);
                if (plantilla.Code == -1 || plantilla.Result == null)
                    return ErrorResultPlantilla(plantilla.Description ?? "No se encontró la plantilla.");

                int consecutivo = plantilla.Result.consecutivo + 1;
                string tipoAsiento = (plantilla.Result.tipo_asiento ?? string.Empty).Trim();
                string numAsiento = $"PTR{request.cod_plantilla:000}-{consecutivo:000000}";

                var updateConsecutivo = CntXPlantillaRate_ActualizarConsecutivo(codEmpresa, request, consecutivo);
                if (updateConsecutivo.Code == -1)
                    return ErrorResultPlantilla(updateConsecutivo.Description);

                var maestro = CntXAsiento_CrearMaestro(codEmpresa, request, tipoAsiento, numAsiento);
                if (maestro.Code == -1)
                    return ErrorResultPlantilla(maestro.Description);

                var detallePlantilla = CntXPlantillaRate_Detalle_Obtener(codEmpresa, request.cod_contabilidad, request.cod_plantilla);
                if (detallePlantilla.Code == -1)
                    return ErrorResultPlantilla(detallePlantilla.Description);

                var lineas = detallePlantilla.Result ?? new List<CntXPlantillaRateDetalleData>();

                foreach (var item in lineas.OrderBy(x => x.num_linea))
                {
                    decimal montoDebito = request.monto * (item.debitos / 100m);
                    decimal montoCredito = request.monto * (item.creditos / 100m);

                    var detalle = CntXAsiento_CrearDetalle(codEmpresa, request, tipoAsiento, numAsiento, item, montoDebito, montoCredito);
                    if (detalle.Code == -1)
                        return ErrorResultPlantilla(detalle.Description);
                }

                return new ErrorDto
                {
                    Code = 0,
                    Description = $"Asiento aplicado: {numAsiento}"
                };
            }
            catch (Exception ex)
            {
                return ErrorResultPlantilla(ex.Message);
            }
        }

        /// <summary>
        /// Obtener informacion de una planilla en especifico
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CntXPlantillaRateData?> CntXPlantillaRate_Obtener(int codEmpresa, CntXPlantillaRateGenerarRequest request)
        {
            string query = @"
                select 
                    cod_contabilidad,
                    cod_plantilla,
                    consecutivo,
                    tipo_asiento
                from CntX_Plantilla_Rate
                where cod_contabilidad = @CodConta
                  and cod_plantilla = @CodPlantilla";

            return DbHelper.ExecuteSingleQuery<CntXPlantillaRateData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new
                {
                    CodConta = request.cod_contabilidad,
                    CodPlantilla = request.cod_plantilla
                });
        }

        /// <summary>
        /// Actualiza el consecutivo de la plantilla
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDto CntXPlantillaRate_ActualizarConsecutivo(int codEmpresa, CntXPlantillaRateGenerarRequest request, int consecutivo)
        {
            string query = @"
            update CntX_Plantilla_Rate
               set consecutivo = @Consecutivo
             where cod_contabilidad = @CodConta
               and cod_plantilla = @CodPlantilla";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Consecutivo = consecutivo,
                    CodConta = request.cod_contabilidad,
                    CodPlantilla = request.cod_plantilla
                });
        }

        /// <summary>
        /// Crea el maestro del asiento con la informacion de la plantilla 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="tipoAsiento"></param>
        /// <param name="numAsiento"></param>
        /// <returns></returns>
        public ErrorDto CntXAsiento_CrearMaestro(int codEmpresa, CntXPlantillaRateGenerarRequest request, string tipoAsiento, string numAsiento)
        {
            DateTime fechaAsiento = new DateTime(request.periodo_anio, request.periodo_mes, 1, 0, 0, 0, DateTimeKind.Local);
            string notas = $"GENERADO CON PLANTILLA RATE COD : {request.cod_plantilla:000}";

            string query = @"
            insert into Cntx_Asientos
            (
                cod_contabilidad,
                tipo_asiento,
                num_asiento,
                descripcion,
                fecha_asiento,
                balanceado,
                anio,
                mes,
                user_crea,
                modulo,
                notas
            )
            values
            (
                @CodConta,
                @TipoAsiento,
                @NumAsiento,
                @Descripcion,
                @FechaAsiento,
                'S',
                @Anio,
                @Mes,
                @Usuario,
                20,
                @Notas
            )";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    CodConta = request.cod_contabilidad,
                    TipoAsiento = tipoAsiento,
                    NumAsiento = numAsiento,
                    Descripcion = request.descripcion?.Trim() ?? string.Empty,
                    FechaAsiento = fechaAsiento,
                    Anio = request.periodo_anio,
                    Mes = request.periodo_mes,
                    Usuario = request.usuario,
                    Notas = notas
                });
        }

        /// <summary>
        /// Crea el detalle del asiento 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="tipoAsiento"></param>
        /// <param name="numAsiento"></param>
        /// <param name="item"></param>
        /// <param name="montoDebito"></param>
        /// <param name="montoCredito"></param>
        /// <returns></returns>
        public ErrorDto CntXAsiento_CrearDetalle(int codEmpresa, CntXPlantillaRateGenerarRequest request, 
            string tipoAsiento, string numAsiento, CntXPlantillaRateDetalleData item, decimal montoDebito, decimal montoCredito)
        {
            string query = @"
            insert into Cntx_Asientos_detalle
            (
                cod_contabilidad,
                tipo_asiento,
                num_asiento,
                cod_cuenta,
                monto_debito,
                monto_credito,
                documento,
                detalle,
                num_linea,
                cod_unidad,
                cod_centro_costo,
                cod_divisa,
                tipo_cambio
            )
            values
            (
                @CodConta,
                @TipoAsiento,
                @NumAsiento,
                @CodCuenta,
                @MontoDebito,
                @MontoCredito,
                @Documento,
                @Detalle,
                @NumLinea,
                @CodUnidad,
                @CodCentroCosto,
                @CodDivisa,
                1
            )";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    CodConta = request.cod_contabilidad,
                    TipoAsiento = tipoAsiento,
                    NumAsiento = numAsiento,
                    CodCuenta = item.cod_cuenta?.Trim() ?? string.Empty,
                    MontoDebito = montoDebito,
                    MontoCredito = montoCredito,
                    Documento = request.documento?.Trim() ?? string.Empty,
                    Detalle = item.detalle?.Trim() ?? string.Empty,
                    NumLinea = item.num_linea,
                    CodUnidad = item.cod_unidad?.Trim() ?? string.Empty,
                    CodCentroCosto = item.cod_centro_costo?.Trim() ?? string.Empty,
                    CodDivisa = item.cod_divisa?.Trim() ?? string.Empty
                });
        }

        /// <summary>
        /// Auxiliar para mensaje de error
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        private ErrorDto ErrorResultPlantilla(string? description)
        {
            return new ErrorDto
            {
                Code = -1,
                Description = description
            };
        }

    }
}
