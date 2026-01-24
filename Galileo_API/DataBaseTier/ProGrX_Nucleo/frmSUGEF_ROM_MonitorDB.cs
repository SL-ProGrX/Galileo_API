using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Nucleo;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSugefRomMonitorDB
    {
        private readonly PortalDB _portalDb;

        public FrmSugefRomMonitorDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el tipo de cambio SUGEF para una fecha dada.
        /// </summary>
        public ErrorDto<SugefTipoCambioResult?> SUGEF_TipoCambio_Obtener(int codEmpresa, DateTime fecha)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var query = "SELECT dbo.fxSUGEF_Tipo_Cambio(@Fecha) AS TC";
                var param = new { Fecha = fecha.ToString("yyyy-MM-dd") };
                return conn.QueryFirstOrDefault<SugefTipoCambioResult>(query, param);
            });
        }

        /// <summary>
        /// Ejecuta el procedimiento spSUGEF_ROM_Monitor_Consulta para obtener el monitoreo ROM SUGEF.
        /// </summary>
        public ErrorDto<List<SugefRomMonitorConsultaResult>> SUGEF_ROM_Monitor_Consulta(int codEmpresa, DateTime corte)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var param = new { Corte = corte };
                return conn.Query<SugefRomMonitorConsultaResult>(
                    "spSUGEF_ROM_Monitor_Consulta",
                    param,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            });
        }

        /// <summary>
        /// Ejecuta el procedimiento spSUGEF_ROM_Monitor_Detalle para obtener el detalle de un ROM.
        /// </summary>
        public ErrorDto<List<SugefRomMonitorDetalleResult>> SUGEF_ROM_Monitor_Detalle(int codEmpresa, DateTime corte, int rom)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var param = new { Corte = corte, ROM = rom };
                return conn.Query<SugefRomMonitorDetalleResult>(
                    "spSUGEF_ROM_Monitor_Detalle",
                    param,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            });
        }

        /// <summary>
        /// Ejecuta el procedimiento spSUGEF_ROM_Monitor_Forma_Pago para obtener el detalle de formas de pago.
        /// </summary>
        public ErrorDto<List<SugefRomMonitorFormaPagoResult>> SUGEF_ROM_Monitor_Forma_Pago(int codEmpresa, DateTime corte, string tipoDoc, string numDoc)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var param = new { Corte = corte, TipoDoc = tipoDoc, NumDoc = numDoc };
                return conn.Query<SugefRomMonitorFormaPagoResult>(
                    "spSUGEF_ROM_Monitor_Forma_Pago",
                    param,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            });
        }

        /// <summary>
        /// Lista las entidades de pago activas.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> SUGEF_EntidadesPago_Lista(int codEmpresa)
        {
            var query = @"SELECT COD_ENTIDAD_PAGO as item, DESCRIPCION as descripcion 
                          FROM SIF_ENTIDADES_PAGO 
                          WHERE ACTIVA = 1 
                          ORDER BY COD_ENTIDAD_PAGO";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Lista los orígenes de recursos activos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> SUGEF_OrigenRecursos_Lista(int codEmpresa)
        {
            var query = @"SELECT COD_ORIGEN_RECURSOS as item, DESCRIPCION as descripcion 
                          FROM SIF_ORIGEN_RECURSOS 
                          WHERE ACTIVA = 1 
                          ORDER BY COD_ORIGEN_RECURSOS";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Ejecuta el procedimiento spSUGEF_ROM_Monitor para procesar el monitoreo ROM SUGEF.
        /// </summary>
        public ErrorDto<bool> SUGEF_ROM_Monitor(int codEmpresa, SugefRomMonitorParams param)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var dbParam = new { param.Corte, param.BaseDol, param.Usuario };
                conn.Execute("spSUGEF_ROM_Monitor", dbParam, commandType: CommandType.StoredProcedure);
                return true;
            });
        }

        /// <summary>
        /// Ejecuta el procedimiento spSUGEF_ROM_Monitor_Forma_Pago_Actualiza para actualizar formas de pago en el monitoreo ROM SUGEF.
        /// </summary>
        public ErrorDto<bool> SUGEF_ROM_Monitor_Forma_Pago_Actualiza(int codEmpresa, SugefRomMonitorFormaPagoActualizaParams param)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var dbParam = new
                {
                    param.LineaId,
                    param.TipoDoc,
                    param.NumDoc,
                    param.PagadorId,
                    param.Origen,
                    param.Notas,
                    param.Usuario
                };
                conn.Execute("spSUGEF_ROM_Monitor_Forma_Pago_Actualiza", dbParam, commandType: CommandType.StoredProcedure);
                return true;
            });
        }
    }
}
