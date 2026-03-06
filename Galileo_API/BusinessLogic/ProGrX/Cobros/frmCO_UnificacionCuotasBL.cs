using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.DataBaseTier.ProGrX.Cobros;

namespace Galileo_API.BusinessTier.ProGrX.Cobros
{
    public class FrmCOUnificacionCuotasBL
    {
        private readonly FrmCOUnificacionCuotasDB Db;

        public FrmCOUnificacionCuotasBL(IConfiguration config)
        {
            Db = new FrmCOUnificacionCuotasDB(config);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CO_UnificacionCuotas_Codigos_Obtener(int CodEmpresa, string? texto)
        {
            return Db.CO_UnificacionCuotas_Codigos_Obtener(CodEmpresa, texto);
        }
        public ErrorDto<CoUnificacionCuotasListaResult> Co_UnificacionCuotas_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            return Db.Co_UnificacionCuotas_Lista_Obtener(CodEmpresa, jfiltros);
        }

        public ErrorDto<CoUnificacionCuotasListaResult> Co_UnificacionCuotas_Lista_Export(int CodEmpresa, string jfiltros)
        {
            return Db.Co_UnificacionCuotas_Lista_Export(CodEmpresa, jfiltros);
        }

        public ErrorDto<CoUnificacionCuotasUnificarResponse> Co_UnificacionCuotas_Unificar(int CodEmpresa, CoUnificacionCuotasUnificarRequest req)
        {
            return Db.Co_UnificacionCuotas_Unificar(CodEmpresa, req);
        }
    }
}