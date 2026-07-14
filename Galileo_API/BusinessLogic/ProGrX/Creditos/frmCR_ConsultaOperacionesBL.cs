using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrConsultaOperacionesBl
    {
        private readonly FrmCrConsultaOperacionesDb _db;

        public FrmCrConsultaOperacionesBl(IConfiguration config)
        {
            _db = new FrmCrConsultaOperacionesDb(config);
        }

        public ErrorDto<List<CrConsultaOperacionesBusquedaOperacionDto>> CrConsultaOperaciones_BusquedaOperaciones_Obtener(
            int codEmpresa)
            => _db.CrConsultaOperaciones_BusquedaOperaciones_Obtener(codEmpresa);

        public ErrorDto<List<CrConsultaOperacionesBusquedaSocioDto>> CrConsultaOperaciones_BusquedaSocios_Obtener(
            int codEmpresa)
            => _db.CrConsultaOperaciones_BusquedaSocios_Obtener(codEmpresa);

        public ErrorDto<List<CrConsultaOperacionesListaDto>> CrConsultaOperaciones_Cedula_Obtener(
            int codEmpresa,
            string cedula)
            => _db.CrConsultaOperaciones_Cedula_Obtener(codEmpresa, cedula);

        public ErrorDto<CrConsultaOperacionesDetalleDto> CrConsultaOperaciones_Detalle_Obtener(
            int codEmpresa,
            int operacion)
            => _db.CrConsultaOperaciones_Detalle_Obtener(codEmpresa, operacion);
    }
}