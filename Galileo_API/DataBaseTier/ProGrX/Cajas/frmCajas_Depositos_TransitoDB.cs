using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Microsoft.Extensions.Configuration;

namespace Galileo.DataBaseTier
{
    public class FrmCajasDepositosTransitoDB
    {
        private readonly PortalDB _portalDb;

        public FrmCajasDepositosTransitoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
        }

        /// <summary>
        /// Consulta de listado de cuentas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_DepositosTransito_Cuentas_Obtener(int CodEmpresa)
        {
            var result = DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>());

            try
            {
                var connectionString = _portalDb.ObtenerDbConnStringEmpresa(CodEmpresa);
                var datos = DbHelper.ExecuteStoredProcedureList<DepositosCuentasBancarias>(
                    connectionString,
                    "spCajas_DepositosCuentasBancarias");

                if (datos.Code != 0)
                {
                    result.Code = datos.Code;
                    result.Description = datos.Description;
                    result.Result = null;
                    return result;
                }

                result.Result = (datos.Result ?? new List<DepositosCuentasBancarias>())
                    .Select(d => new DropDownListaGenericaModel
                    {
                        item = d.id_banco,
                        descripcion = d.descripcion
                    })
                    .ToList();
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
        ///  Consulta de dep�sitos en tr�nsito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasDepositosTransitoData>> Cajas_Depositos_Transito_Consultar(int CodEmpresa, FiltrosData filtros)
        {
            var result = DbHelper.CreateOkResponse(new List<CajasDepositosTransitoData>());

            try
            {
                filtros ??= new FiltrosData();

                var fechaInicio = filtros.fecha_inicio?.Date.AddHours(0).AddMinutes(0).AddSeconds(0);
                var fechaCorte = filtros.fecha_corte?.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                var connectionString = _portalDb.ObtenerDbConnStringEmpresa(CodEmpresa);

                var datos = DbHelper.ExecuteStoredProcedureList<CajasDepositosTransitoData>(
                    connectionString,
                    "spCajas_Depositos_Transito",
                    new
                    {
                        fecha_inicio = fechaInicio,
                        fecha_corte = fechaCorte,
                        filtros.banco,
                        filtros.numero,
                        filtros.MntInicio,
                        filtros.MntCorte
                    });

                if (datos.Code != 0)
                {
                    result.Code = datos.Code;
                    result.Description = datos.Description;
                    result.Result = null;
                    return result;
                }

                result.Result = datos.Result ?? new List<CajasDepositosTransitoData>();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }
    }
}