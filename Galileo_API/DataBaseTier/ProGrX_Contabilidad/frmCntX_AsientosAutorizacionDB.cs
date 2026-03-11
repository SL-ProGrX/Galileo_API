using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXAsientosAutorizacionDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXAsientosAutorizacionDb(IConfiguration config)
            : this(new PortalDB(config)) { }

        public FrmCntXAsientosAutorizacionDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Obtiene la lista de tipos de asientos 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXAsientos_Tipos_Obtener(int codEmpresa, int codConta)
        {
            string query = @"select Tipo_Asiento as item, descripcion from CntX_Tipos_Asientos 
                where cod_contabilidad = @codConta";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb, codEmpresa, query, new { codConta });
        }

        /// <summary>
        /// Obtiene la lista de asientos pendientes de autorizacion 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="tipoAsiento"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXAsientoAutorizacionData>> CntXAsientos_ListaPendientes_Obtener(int codEmpresa, int codConta, string tipoAsiento, int anio, int mes)
        {
            string query = @"select A.Num_asiento,A.Tipo_Asiento,A.Descripcion,A.Fecha_Asiento
                ,sum(isnull(D.monto_Debito,0)) as Debitos,sum(isnull(D.monto_credito,0)) as creditos
                from Cntx_Asientos A left join Cntx_Asientos_Detalle D on A.cod_contabilidad = D.cod_contabilidad
                and A.tipo_asiento = D.tipo_asiento and A.num_asiento = D.num_asiento
                where A.tipo_asiento = @tipoAsiento and A.cod_contabilidad = @codConta
                and A.modulo <> 20 and A.fecha_autoriza is null 
                and A.anio =  @anio
                and A.mes = @mes
                group by A.Num_asiento,A.Tipo_Asiento,A.Descripcion,A.Fecha_Asiento";
            return DbHelper.ExecuteListQuery<CntXAsientoAutorizacionData>(
                _portalDb, codEmpresa, query, new { codConta, tipoAsiento, anio, mes });
        }

        /// <summary>
        /// Autoriza los asientos seleccionados
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="usuario"></param>
        /// <param name="lista"></param>
        /// <returns></returns>
        public ErrorDto CntXAsientos_Autorizar(int codEmpresa, int codConta, string usuario, List<CntXAsientoAutorizacionData> lista)
        {
            var result = new ErrorDto();
            try
            {
                string query = @"
                    update Cntx_Asientos
                       set user_autoriza = @usuario,
                           fecha_autoriza = GETDATE()
                     where cod_contabilidad = @codConta
                       and tipo_asiento = @tipoAsiento
                       and num_asiento = @numAsiento";

                foreach (var item in lista)
                {
                    DbHelper.ExecuteNonQuery(
                        _portalDb,
                        codEmpresa,
                        query,
                        new
                        {
                            usuario,
                            codConta,
                            tipoAsiento = item.tipo_asiento,
                            numAsiento = item.num_asiento
                        });
                }

                result.Code = 0;
                result.Description = "Asientos foraneos autorizados satisfactoriamente.";
                return result;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                return result;
            }
        }
    }
}
