using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesMonitoreoDB
    {
        private readonly PortalDB _portalDB;
        private readonly MCntLinkDB mCntLink;

        public FrmTesMonitoreoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            mCntLink = new MCntLinkDB(config);
        }

        /// <summary>
        /// Obtener el monitoreo de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TesMonitoreoDto>> TES_Monitoreo_Obtener(int CodEmpresa, DateTime fechaCorte)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string fechaCorteStr = MProGrXAuxiliarDB.validaFechaGlobal(fechaCorte, "yyyy-MM-dd 23:59:59") ?? string.Empty;

                var query = $@"exec spTes_Monitoreo_Saldos_Movimientos @pFechaCorte ";
                var result = conn.Query<TesMonitoreoDto>(query,
                new
                {
                    pFechaCorte = fechaCorteStr
                },
                commandTimeout: 120 // en segundos
                ).ToList();

                return DbHelper.CreateOkResponse<List<TesMonitoreoDto>>(result);

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesMonitoreoDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtener el monitoreo de los documentos de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Corte"></param>
        /// <returns></returns>
        public ErrorDto<List<TesMonitoreoDto>> TES_Monitoreo_Documentos_Obtener(int CodEmpresa, string Corte)
        {
            DateTime fecha = DateTime.Parse(Corte, System.Globalization.CultureInfo.InvariantCulture);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string fechaCorteStr = MProGrXAuxiliarDB.validaFechaGlobal(fecha, "yyyy-MM-dd 23:59:59") ?? string.Empty;

                var query = $@"exec spTes_W_Monitoreo_Saldos_MovTesoreria @pFechaCorte ";
                var result = conn.Query<TesMonitoreoDto>(query,
                new
                {
                    pFechaCorte = fechaCorteStr
                },
                commandTimeout: 0 // en segundos
                ).ToList();

                return DbHelper.CreateOkResponse<List<TesMonitoreoDto>>(result);

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesMonitoreoDto>>(ex.Message);
            }


            //using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            //try
            //{
               


                //var Lista = TES_Monitoreo_Obtener(CodEmpresa, fechaCorte).Result;
                //var result = new List<TesMonitoreoDto>();
                //foreach (var item in Lista)
                //{
                //    var fechaInicio = item.inicio.AddDays(-1).Date;
                //    string ctaConta = mCntLink.fxgCntCuentaFormato(CodEmpresa, false, item.cuentaBanco, 0);

                //    //Emisiones de Documentos
                //    var queryE = @"select D.debehaber as Movimiento,sum(D.monto / D.Tipo_Cambio) as Total
                //        from Tes_Transacciones C inner join Tes_Trans_Asiento D on C.nsolicitud = D.nsolicitud
                //        where C.fecha_emision between @inicio 
                //        and @corte and C.estado in('I','T','A') 
                //        and D.cuenta_contable = @ctaconta 
                //        and D.Tipo_Cambio <> 0
                //        group by D.debehaber";
                //    var emisionesDoc = conn.Query<TesMonitoreoDocumentosDto>(queryE,
                //        new
                //        {
                //            inicio = fechaInicio,
                //            corte = fechaCorte,
                //            ctaconta = ctaConta
                //        }).ToList();
                //    foreach (var rs in emisionesDoc)
                //    {
                //        if (rs.movimiento == "D")
                //        {
                //            item.totalCreditos = rs.total;
                //        }
                //        else
                //        {
                //            item.totalDebitos = rs.total;
                //        }
                //    }

                //    //Anulaciones de Documentos
                //    var queryA = @"select D.debehaber as Movimiento,sum(D.monto/ D.Tipo_Cambio) as Total
                //        from Tes_Transacciones C inner join Tes_Trans_Asiento D on C.nsolicitud = D.nsolicitud
                //        where C.fecha_anula between @inicio 
                //        and @corte and C.estado in('A') 
                //        and D.cuenta_contable = @ctaconta 
                //        and D.Tipo_Cambio <> 0
                //        group by D.debehaber";
                //    var anulacionesDoc = conn.Query<TesMonitoreoDocumentosDto>(queryA,
                //        new
                //        {
                //            inicio = fechaInicio,
                //            corte = fechaCorte,
                //            ctaconta = ctaConta
                //        }).ToList();

                //    foreach (var rs in anulacionesDoc)
                //    {
                //        if (rs.movimiento == "D")
                //        {
                //            item.totalDebitos += rs.total;
                //        }
                //        else
                //        {
                //            item.totalCreditos += rs.total;
                //        }
                //    }
                //    item.saldoFinal = item.saldoInicial - item.totalDebitos + item.totalCreditos;

                //    result.Add(item);
                //}
            
                //return DbHelper.CreateOkResponse<List<TesMonitoreoDto>>(result);
            
            //}
            //catch (Exception ex)
            //{
            //    return DbHelper.CreateErrorResponse<List<TesMonitoreoDto>>(ex.Message);
            //}
        }
    }
}
