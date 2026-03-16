using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    
    public class FrmCntXPlantillaAsientosGeneraDB
    {

        private readonly PortalDB _portalDb;

        public FrmCntXPlantillaAsientosGeneraDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de plantillas de asientos por contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <returns>Lista de plantillas de asientos.</returns>
        public ErrorDto<List<CntXPlantillaAsientosDto>> CntXPlantillaAsientos_Lista(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                select Cod_Plantilla, Descripcion, Consecutivo, Anio_Inicio, Mes_Inicio, Tipo_Asiento, Asiento_Descripcion, Asiento_Detalle, Asiento_Documento
                from CntX_Plantilla_Asientos
                where cod_contabilidad = @codContabilidad
                order by cod_plantilla";
            return DbHelper.ExecuteListQuery<CntXPlantillaAsientosDto>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Obtiene una plantilla de asientos por contabilidad y código de plantilla.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="codPlantilla">Código de plantilla.</param>
        /// <returns>Plantilla de asientos.</returns>
        public ErrorDto<CntXPlantillaAsientosDto?> CntXPlantillaAsientos_Get(int codEmpresa, int codContabilidad, string codPlantilla)
        {
            var sql = @"
                select Cod_Plantilla, Descripcion, Consecutivo, Anio_Inicio, Mes_Inicio, Tipo_Asiento, Asiento_Descripcion, Asiento_Detalle, Asiento_Documento
                from CntX_Plantilla_Asientos
                where cod_contabilidad = @codContabilidad
                  and cod_plantilla = @codPlantilla";
            return DbHelper.ExecuteSingleQuery<CntXPlantillaAsientosDto>(_portalDb, codEmpresa, sql, default, new { codContabilidad, codPlantilla });
        }

        /// <summary>
        /// Actualiza el consecutivo de una plantilla de asientos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de actualización.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXPlantillaAsientos_UpdateConsecutivo(int codEmpresa, CntXPlantillaAsientosUpdateParams param)
        {
            var sql = @"
                update CntX_Plantilla_Asientos
                set consecutivo = @Consecutivo
                where cod_contabilidad = @CodContabilidad
                  and cod_plantilla = @CodPlantilla";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Inserta un asiento en Cntx_Asientos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del asiento.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntxAsientos_Insert(int codEmpresa, CntxAsientosInsertParams param)
        {
            var sql = @"
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
                  @CodContabilidad,
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
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Obtiene el detalle de una plantilla de asientos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="codPlantilla">Código de plantilla.</param>
        /// <returns>Lista de detalles de plantilla.</returns>
        public ErrorDto<List<CntXPlantillaDetalleDto>> CntXPlantillaDetalle_Lista(int codEmpresa, int codContabilidad, string codPlantilla)
        {
            var sql = @"
                select *
                from CntX_Plantilla_detalle
                where cod_contabilidad = @codContabilidad
                  and cod_plantilla = @codPlantilla
                order by num_linea";
            return DbHelper.ExecuteListQuery<CntXPlantillaDetalleDto>(_portalDb, codEmpresa, sql, new { codContabilidad, codPlantilla });
        }

        /// <summary>
        /// Inserta un detalle de asiento en Cntx_Asientos_detalle.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del detalle.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntxAsientosDetalle_Insert(int codEmpresa, CntxAsientosDetalleInsertParams param)
        {
            var sql = @"
                insert into Cntx_Asientos_detalle
                (
                  cod_contabilidad,
                  tipo_asiento,
                  num_asiento,
                  cod_cuenta,
                  Monto_Debito,
                  Monto_credito,
                  Documento,
                  Detalle,
                  num_linea,
                  cod_unidad,
                  cod_divisa,
                  Tipo_Cambio,
                  cod_centro_costo
                )
                values
                (
                  @CodContabilidad,
                  @TipoAsiento,
                  @NumAsiento,
                  @CodCuenta,
                  @MontoDebito,
                  @MontoCredito,
                  @Documento,
                  @Detalle,
                  @NumLinea,
                  @CodUnidad,
                  @CodDivisa,
                  @TipoCambio,
                  @CodCentroCosto
                )";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Valida si existe un periodo abierto ('P') para el año, mes y contabilidad dados.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="anio">Año del periodo.</param>
        /// <param name="mes">Mes del periodo.</param>
        /// <returns>Total de periodos abiertos encontrados.</returns>
        public ErrorDto<int> CntXPeriodos_ExisteAbierto(int codEmpresa, int codContabilidad, int anio, int mes)
        {
            var sql = @"
                select isnull(count(*),0) as existe
                from CntX_Periodos
                where anio = @anio
                  and mes = @mes
                  and cod_contabilidad = @codContabilidad
                  and estado = 'P'";
            return DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sql, default, new { anio, mes, codContabilidad });
        }
    }
}
