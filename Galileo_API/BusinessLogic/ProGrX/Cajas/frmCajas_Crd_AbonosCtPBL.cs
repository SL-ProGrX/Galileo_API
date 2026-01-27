using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cobros;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasCrdAbonosCtPBl
    {
        private readonly FrmCajasCrdAbonosCtPDb _db;

        public FrmCajasCrdAbonosCtPBl(IConfiguration config) => _db = new FrmCajasCrdAbonosCtPDb(config);

        public ErrorDto<CajasCrdAbonosCtPData> CajasCrdAbonosCtP_ConsultaOperacion_Obtener(int CodEmpresa, string CodCaja, int OperacionId)
        {
            return _db.CajasCrdAbonosCtP_ConsultaOperacion_Obtener(CodEmpresa, CodCaja, OperacionId);
        }

        public ErrorDto<List<CajasCrdAbonosCtPData>> CajasCrdAbonosCtP_Operaciones_Obtener(int CodEmpresa)
        {
            return _db.CajasCrdAbonosCtP_Operaciones_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CajasCrdAbonosCtP_TipoDoc_Obtener(int CodEmpresa, string Caja)
        {
            return _db.CajasCrdAbonosCtP_TipoDoc_Obtener(CodEmpresa, Caja);
        }

        public ErrorDto<List<CajasCrdOperacionTransacData>> CajasCrdAbonosCtP_OperacionTransac_Obtener(int CodEmpresa, int IdSolicitud)
        {
            return _db.CajasCrdAbonosCtP_OperacionTransac_Obtener(CodEmpresa, IdSolicitud);
        }

        public ErrorDto<long> CajasCrdAbonosCtP_DiasActivoFecha_Obtener(int CodEmpresa, string Request)
        {
            CajasCrdAbonoTipoRequest? request = JsonConvert.DeserializeObject<CajasCrdAbonoTipoRequest>(Request);
            if (request == null)
            {
                return new ErrorDto<long>
                {
                    Code = -1,
                    Description = "Parametros no coinciden con el objeto CajasCrdAbonoTipoRequest.",
                    Result = 0
                };
            }
            return _db.CajasCrdAbonosCtP_DiasActivoFecha_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CajasCrdAbonosInfoCancelacionData> CajasCrdAbonosCtP_InfoCancelacion_Obtener(int CodEmpresa, string Request)
        {
            CajasCrdAbonoTipoRequest? request = JsonConvert.DeserializeObject<CajasCrdAbonoTipoRequest>(Request);
            if (request == null)
            {
                return new ErrorDto<CajasCrdAbonosInfoCancelacionData>
                {
                    Code = -1,
                    Description = "Parametros no coinciden con el objeto CajasCrdAbonoTipoRequest.",
                    Result = null
                };
            }
            return _db.CajasCrdAbonosCtP_InfoCancelacion_Obtener(CodEmpresa, request);
        }

        public ErrorDto CajasCrdAbonosCtP_Abono_Registrar(int CodEmpresa, CajasCrdAbonosCtPRegistrarAbonoRequest Request)
        {
            return _db.CajasCrdAbonosCtP_Abono_Registrar(CodEmpresa, Request);
        }
    }
}
